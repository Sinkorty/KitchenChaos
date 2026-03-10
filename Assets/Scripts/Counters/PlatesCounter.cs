using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;


    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 2f;
    private int platesSpawnedAmount;
    private int platesSpawnedAmountMax = 3;

    private void Update()
    {
        if (!IsOwner) return;

        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;
            if (GameManager.Instance.IsGamePlaying() && platesSpawnedAmount < platesSpawnedAmountMax)
            {
                SpawnPlateLogicServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateLogicServerRpc()
    {
        SpawnPlateLogicClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateLogicClientRpc()
    {
        platesSpawnedAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            if (platesSpawnedAmount > 0)
            {
                // 由于KitchenObject.SpawnKitchenObject已经实现了NetworkObject相关代码，所以直接调用没关系的
                // give player a plate
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                InteractLogicServerRpc();
            }
        }
    }
    [ServerRpc(RequireOwnership =false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
