using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject()) // 拿
        {
            if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 如果手上拿的是可切的才让放
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }
    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var item in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == item.input)
            {
                return item.output;
            }
        }
        return null;
    }
    private bool HasRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var item in cuttingRecipeSOArray)
        {
            if (kitchenObjectSO == item.input)
            {
                return true;
            }
        }
        return false;
    }
}
