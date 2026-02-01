using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    public event EventHandler<OnProgressChangedEvnetArgs> OnProgressChanged;
    public class OnProgressChangedEvnetArgs : EventArgs
    {
        public float progressNormalized;
    }
    public event EventHandler OnCut;


    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject()) // 放
        {
            if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 如果手上拿的是可切的才让放
            {
                cuttingProgress = 0;
                player.GetKitchenObject().SetKitchenObjectParent(this);

                // 设置progress的visual事件调用（解耦）
                CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                OnProgressChanged?.Invoke(this, new OnProgressChangedEvnetArgs
                {
                    progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
                });
            }
        }
        else if (!player.HasKitchenObject() && HasKitchenObject()) // 取
        {
            GetKitchenObject().SetKitchenObjectParent(player);
        }
    }
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject())
        {
            KitchenObjectSO outputKitchenObjectSO = GetOutputFromInput(GetKitchenObject().GetKitchenObjectSO());

            if (outputKitchenObjectSO != null) // CuttingCounter上放的是可切的才切
            {
                cuttingProgress++;
                CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());

                if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
                {
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
                }

                // NOTE: 视觉相关的代码就放在后面啦
                OnProgressChanged?.Invoke(this, new OnProgressChangedEvnetArgs
                {
                    progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
                });

                OnCut?.Invoke(this, EventArgs.Empty);
            }
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
