using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    public event EventHandler<IHasProgress.OnProgressChangedEvnetArgs> OnProgressChanged;
    public event EventHandler OnCut;


    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject()) // 放
        {
            if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 如果手上拿的是可切的才让放
            {
                InteractLogicPlaceObjectOnCounterServerRpc();

                KitchenObject kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);
            }
        }
        else if (!player.HasKitchenObject() && HasKitchenObject()) // 取
        {
            GetKitchenObject().SetKitchenObjectParent(player);
        }
        else if(player.HasKitchenObject() && HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) // 手上有盘子，桌上有菜
            {
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                {
                    //GetKitchenObject().DestroySelf();
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
                }
            }
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;

        // 设置progress的visual事件调用（解耦）
        //CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(kitchenObject.GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEvnetArgs
        {
            progressNormalized = 0f,
        });
    }
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject())
        {
            CutObjectServerRpc();
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
        TestCuttingProgressDoneServerRpc();
    }
    [ClientRpc]
    private void CutObjectClientRpc()
    {
        KitchenObjectSO outputKitchenObjectSO = GetOutputFromInput(GetKitchenObject().GetKitchenObjectSO());

        if (outputKitchenObjectSO != null) // CuttingCounter上放的是可切的才切
        {
            cuttingProgress++;
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());

            // NOTE: 视觉相关的代码就放在后面啦
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEvnetArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
            });

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);
            Debug.Log(OnAnyCut.GetInvocationList().Length);
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            //GetKitchenObject().DestroySelf();
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
            KitchenObject.SpawnKitchenObject(cuttingRecipeSO.output, this);
        }
    }
    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(kitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        return null;
    }
    private bool HasRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(kitchenObjectSO);
        return cuttingRecipeSO != null;
    }
    private CuttingRecipeSO GetCuttingRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var item in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == item.input)
            {
                return item;
            }
        }
        return null;
    }
}
