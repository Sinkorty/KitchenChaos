using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IHasProgress
{
    // NOTE: 简单的状态机实现
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    // 供StoveCounterVisual的事件
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    // 供ProgressBar的事件
    public event EventHandler<IHasProgress.OnProgressChangedEvnetArgs> OnProgressChanged;

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO currentFryingRecipeSO;
    private BurningRecipeSO currentBurningRecipeSO;
    //private IHasProgress.OnProgressChangedEvnetArgs sharedOnProgressChangedEventArgs;


    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTiemr_OnValueChanged;
        burningTimer.OnValueChanged += BurningTiemr_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }
    private void FryingTiemr_OnValueChanged(float _, float newValue)
    {
        float fryingTimerMax = currentFryingRecipeSO == null ? 1f : currentFryingRecipeSO.fryingProgressMax;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEvnetArgs()
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax,
        });
    }
    private void BurningTiemr_OnValueChanged(float _, float newValue)
    {
        float burningTimerMax = currentBurningRecipeSO == null ? 1f : currentBurningRecipeSO.burningProgressMax;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEvnetArgs()
        {
            progressNormalized = burningTimer.Value / burningTimerMax,
        });
    }
    private void State_OnValueChanged(State _, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs()
        {
            state = newState,
        });
        // 归零，ProgressBarUI将隐藏
        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEvnetArgs()
            {
                progressNormalized = 0f,
            });
        }
    }
    private void Start()
    {
        state.Value = State.Idle;
        //sharedOnProgressChangedEventArgs = new IHasProgress.OnProgressChangedEvnetArgs();
    }
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;
                    if (fryingTimer.Value >= currentFryingRecipeSO.fryingProgressMax)
                    {
                        fryingTimer.Value = 0f;
                        //GetKitchenObject().DestroySelf();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(currentFryingRecipeSO.output, this);

                        currentBurningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());

                        state.Value = State.Fried;
                        //OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state.Value });
                        SetBurningRecipeSOClientRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                        );
                    }

                    //sharedOnProgressChangedEventArgs.progressNormalized = fryingTimer.Value / currentFryingRecipeSO.fryingProgressMax;


                    break;
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;
                    if (burningTimer.Value >= currentBurningRecipeSO.burningProgressMax)
                    {
                        burningTimer.Value = 0f;
                        //GetKitchenObject().DestroySelf();
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(currentBurningRecipeSO.output, this);

                        state.Value = State.Burned;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state.Value });
                    }

                    //sharedOnProgressChangedEventArgs.progressNormalized = burningTimer / currentBurningRecipeSO.burningProgressMax;
                    //OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);

                    break;
                case State.Burned:
                    break;
            }
            //Debug.Log(state);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject()) // 放
        {
            if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 如果手上拿的是可切的才让放
            {
                KitchenObject kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);

                InteractLogicPlaceObjectOnCounterServerRpc(
                    KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                );
            }
        }
        else if (!player.HasKitchenObject() && HasKitchenObject()) // 取
        {
            GetKitchenObject().SetKitchenObjectParent(player);

            //state.Value = State.Idle;
            SetStateIdleServerRpc();
            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state.Value });
            //sharedOnProgressChangedEventArgs.progressNormalized = 0f;
            //OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);
        }
        // 手上有盘子，桌上有菜
        else if (player.HasKitchenObject() && HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                {
                    //GetKitchenObject().DestroySelf();
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());

                    //state.Value = State.Idle;
                    SetStateIdleServerRpc();

                    //OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state.Value });
                    //sharedOnProgressChangedEventArgs.progressNormalized = 0f;
                    //OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0f; // 由于Client Side无法修改NetworkVariable，所以放在此处修改
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        currentFryingRecipeSO = GetFryingRecipeWithInput(kitchenObjectSO);
    }
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        currentBurningRecipeSO = GetBurningRecipeWithInput(kitchenObjectSO);
    }
    private KitchenObjectSO GetOutputFromInput(KitchenObjectSO kitchenObjectSO)
    {
        FryingRecipeSO cuttingRecipeSO = GetFryingRecipeWithInput(kitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        return null;
    }
    private bool HasRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        FryingRecipeSO cuttingRecipeSO = GetFryingRecipeWithInput(kitchenObjectSO);
        return cuttingRecipeSO != null;
    }
    private FryingRecipeSO GetFryingRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var item in fryingRecipeSOArray)
        {
            if (kitchenObjectSO == item.input)
            {
                return item;
            }
        }
        return null;
    }
    private BurningRecipeSO GetBurningRecipeWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var item in burningRecipeSOArray)
        {
            if (kitchenObjectSO == item.input)
            {
                return item;
            }
        }
        return null;
    }
    public bool IsFried() => state.Value == State.Fried;

    public bool IsFrying() => state.Value == State.Frying;
}
