using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO cutKitchenObjectSO;

    public override void Interact(Player player)
    {
        // »ù±¾ÄÃÈ¡
        if (player.HasKitchenObject() && !HasKitchenObject())
        {
            player.GetKitchenObject().SetKitchenObjectParent(this);
        }
        else if (!player.HasKitchenObject() && HasKitchenObject())
        {
            GetKitchenObject().SetKitchenObjectParent(player);
        }
    }
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject())
        {
            // need to cut
            GetKitchenObject().DestroySelf();

            KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
        }
    }
}
