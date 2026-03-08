using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFail;


    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;

    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipeAmount;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of DeliveryManager");
        }
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (GameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];
                //Debug.Log(waitingRecipeSO);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);

                // // 注释掉这部分的原因 是因为 作为Host的 游戏端 也会运行SpawnNewWatingRecipeClientRpc(int)
                //waitingRecipeSOList.Add(waitingRecipeSO);
                //OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        waitingRecipeSOList.Add(recipeListSO.recipeSOList[waitingRecipeSOIndex]);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    // 提交订单
    public bool DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            bool isMatch = true;
            foreach (var item in waitingRecipeSO.kitchenObjectSOList)
            {
                // 循环遍历配方的所有原料
                if (!plateKitchenObject.GetKitchenObjectSOList().Contains(item))
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                DeliverCorrectRecipeServerRpc(i);
                return true;
            }
        }
        // No match found
        DeliverIncorrecctRecipeServerRpc();
        return false;
    }
    [ServerRpc]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        //Debug.Log("Player delivered the correct recipe!");
        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
        successfulRecipeAmount++;

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc]
    private void DeliverIncorrecctRecipeServerRpc()
    {
        DeliverIncorrecctRecipeClientRpc();
    }
    [ClientRpc]
    private void DeliverIncorrecctRecipeClientRpc()
    {
        //Debug.Log("Player delivered the wrong recipe...");
        OnRecipeFail?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
    public int GetSuccessfulRecipeAmount() => successfulRecipeAmount;
}
