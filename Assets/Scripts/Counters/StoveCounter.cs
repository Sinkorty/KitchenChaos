using System;
using System.Collections;
using System.Collections.Generic;
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

    private State state;
    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO currentFryingRecipeSO;
    private BurningRecipeSO currentBurningRecipeSO;
    private IHasProgress.OnProgressChangedEvnetArgs sharedOnProgressChangedEventArgs;

    private void Start()
    {
        state = State.Idle;
        sharedOnProgressChangedEventArgs = new IHasProgress.OnProgressChangedEvnetArgs();
    }
    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;
                    if (fryingTimer >= currentFryingRecipeSO.fryingProgressMax)
                    {
                        fryingTimer = 0f;
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(currentFryingRecipeSO.output, this);

                        currentBurningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());

                        state = State.Fried;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                    }

                    sharedOnProgressChangedEventArgs.progressNormalized = fryingTimer / currentFryingRecipeSO.fryingProgressMax;
                    OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);

                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;
                    if (burningTimer >= currentBurningRecipeSO.burningProgressMax)
                    {
                        burningTimer = 0f;
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(currentBurningRecipeSO.output, this);

                        state = State.Burned;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                    }

                    sharedOnProgressChangedEventArgs.progressNormalized = burningTimer / currentBurningRecipeSO.burningProgressMax;
                    OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);

                    break;
                case State.Burned:
                    break;
            }
            Debug.Log(state);
        }
    }
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject()) // 放
        {
            if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) // 如果手上拿的是可切的才让放
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);

                currentFryingRecipeSO = GetFryingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());

                state = State.Frying;
                fryingTimer = 0f;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

            }
        }
        else if (!player.HasKitchenObject() && HasKitchenObject()) // 取
        {
            GetKitchenObject().SetKitchenObjectParent(player);
            state = State.Idle;
            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });


            sharedOnProgressChangedEventArgs.progressNormalized = 0f;
            OnProgressChanged?.Invoke(this, sharedOnProgressChangedEventArgs);
        }
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
}
