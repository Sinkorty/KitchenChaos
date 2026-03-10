using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private FollowTransform followTransform;

    //private ClearCounter clearCounter;
    private IKitchenObjectParent kitchenObjectParent;


    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    // QUESTION: 为什么这里要多写一次ServerRpc，而不是直接调用ClientRpc
    // ANSWER: Client无法直接调用[ClientRpc]注解的方法
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        // 告诉原来的浮木自己不是你们的孩子了
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("IKitchenObjectParent already has a kitchenObject");
        }
        this.kitchenObjectParent = kitchenObjectParent;

        // 换浮木
        kitchenObjectParent.SetKitchenObject(this);
        //transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        //transform.localPosition = Vector3.zero;
        followTransform.SetTargetTransfrom(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    public void DestroySelf()
    {
        // Only destroy on server side
        Destroy(gameObject);
    }
    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }
    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }


    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }
    public static void DestroyKitchenObject(KitchenObject kitchenObjectSO)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObjectSO);
    }
}
