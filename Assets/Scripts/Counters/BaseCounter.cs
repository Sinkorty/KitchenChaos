using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyObjectPlacecdHere;
    public static void ResetStaticData()
    {
        OnAnyObjectPlacecdHere = null;
    }

    [SerializeField] private Transform counterTopPoint;

    //protected KitchenObject kitchenObject;
    private KitchenObject kitchenObject;// NOTE: 这里是刻意使用private的而不用protected 即便是子类，也应该通过IKitchenObjectParent的实现方法来访问kitchenObject


    public virtual void Interact(Player player)
    {
        throw new NotImplementedException("BaseCounter.Interact is not implemented");
    }
    public virtual void InteractAlternate(Player player)
    {
        //throw new NotImplementedException("BaseCounter.InteractAlternate is not implemented");
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnAnyObjectPlacecdHere?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }
    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }
    public bool HasKitchenObject() => kitchenObject != null;

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public NetworkObject GetNetworkObject()
    {
        //Debug.LogError("Not impl");
        return NetworkObject;
    }
}
