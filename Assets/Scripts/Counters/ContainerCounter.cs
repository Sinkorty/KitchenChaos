using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    public event EventHandler OnPlayerGrabbedObject;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (!HasKitchenObject())
            {
                // put it on the counter
                KitchenObject kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);
            }
            else
            {
                // 桌上有盘子，玩家拿食材
                if (GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        player.GetKitchenObject().DestroySelf();
                    }
                }
                // 桌上有食材，玩家拿盘子
                else if (player.GetKitchenObject().TryGetPlate(out plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
            }
        }
        else
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                //// spawn a corresponding ktichenObject
                //Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
                //// give it to player immediately
                //kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);
                KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

                OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
