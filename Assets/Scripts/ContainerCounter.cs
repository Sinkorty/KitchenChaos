using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;


    public override void Interact(Player player)
    {
        // spawn a corresponding ktichenObject
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        // give it to player immediately
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);

    }
}
