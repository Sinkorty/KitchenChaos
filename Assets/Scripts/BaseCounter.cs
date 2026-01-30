using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform counterTopPoint;

    //protected KitchenObject kitchenObject;
    private KitchenObject kitchenObject;// NOTE: 这里是刻意使用private的而不用protected 即便是子类，也应该通过IKitchenObjectParent的实现方法来访问kitchenObject


    public virtual void Interact(Player player)
    {
        throw new NotImplementedException("BaseCounter.Interact is not implemented");
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
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
}
