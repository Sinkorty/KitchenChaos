using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [System.Serializable]
    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObject;
        public GameObject gameObject;
    }
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSO_GameObjectList;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach (var item in kitchenObjectSO_GameObjectList)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (var item in kitchenObjectSO_GameObjectList)
        {
            if(item.kitchenObject == e.kitchenObjectSO)
            {
                item.gameObject.SetActive(true);
            }
        }
    }
}
