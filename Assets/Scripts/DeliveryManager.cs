using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;


    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;

    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;

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
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                Debug.Log(waitingRecipeSO);
                waitingRecipeSOList.Add(waitingRecipeSO);

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    // 提交订单
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
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
                //Debug.Log("Player delivered the correct recipe!");
                waitingRecipeSOList.RemoveAt(i);

                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
        // No match found
        Debug.Log("Player delivered the wrong recipe...");
    }
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
}
