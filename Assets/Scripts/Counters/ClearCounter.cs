using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        // ¿œ¬ﬂº≠
        //if (kitchenObject == null)
        //{
        //    Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab, counterTopPoint);
        //    kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(this);
        //}
        //else
        //{
        //    // Give the kitchenObject to the player
        //    kitchenObject.SetKitchenObjectParent(player);
        //}
        //KitchenObject kitchenObject = GetKitchenObject();

        //if (kitchenObject == null && player.HasKitchenObject())
        //{
        //    // ∑≈÷√
        //    player.GetKitchenObject().SetKitchenObjectParent(this);
        //}
        //else if (kitchenObject != null && !player.HasKitchenObject())
        //{
        //    //  ∞»°
        //    kitchenObject.SetKitchenObjectParent(player);
        //}

        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {

            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                // player carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // player is holding a plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                    
                }
                else
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}
