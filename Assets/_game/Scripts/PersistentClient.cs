using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentClient : NetworkBehaviour
{
    public static Dictionary<ulong, PersistentClient> persistentClients = new Dictionary<ulong, PersistentClient>();
    public static PersistentClient localClient;
    
    public NetworkVariable<bool> ready = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> score = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> lastPing = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public string steamName;
    public NetworkVariable<ulong> steamID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    //public PlayerController currentPlayerController;
    
    public bool allPlayersReady = false;

    public Action<ulong, bool> onReadyChanged;
    
    public List<string> weaponOrder = new List<string>();
    public int currentWeaponIndex = 0;
    public NetworkVariable<FixedString64Bytes> currentWeaponID = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private DateTime lastPingTime;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
            steamID.Value = SteamClient.SteamId;
        OnSteamIDChanged(0, steamID.Value);
        if (NetworkManager.IsHost)
        {
            StartCoroutine(UpdatePing());
            ShuffleWeaponOrder();
        }
    }
    
    public void ShuffleWeaponOrder()
    {
        //weaponOrder = CustomNetworkManager.Instance.roomData.weapons.Split(",").ToList();
        //if (weaponShuffleMode == EWeaponShuffleMode.Random) // TODO: Add weapon shuffle modes
            //weaponOrder.Shuffle();
        currentWeaponIndex = 0;
        if (weaponOrder.Count > 0)
            currentWeaponID.Value = weaponOrder[currentWeaponIndex];
    }

    IEnumerator UpdatePing()
    {
        while (true)
        {
            PingServerRPC();
            yield return new WaitForSeconds(5.0f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PingServerRPC()
    {
        lastPingTime = DateTime.UtcNow;
        PingClientRPC();
    }

    [ClientRpc]
    void PingClientRPC()
    {
        if (IsOwner)
            PingFromClientServerRPC();
    }

    [ServerRpc(RequireOwnership = true)]
    void PingFromClientServerRPC(ServerRpcParams serverRpcParams = default)
    {
        DateTime time = DateTime.UtcNow;
        persistentClients[serverRpcParams.Receive.SenderClientId].lastPing.Value = (int)(time - lastPingTime).TotalMilliseconds;
    }

    public override void OnNetworkSpawn()
    {
        steamID.OnValueChanged += OnSteamIDChanged;
        ready.OnValueChanged += OnReadyChanged;
        if (persistentClients.ContainsKey(OwnerClientId))
            persistentClients.Remove(OwnerClientId);
        persistentClients.Add(OwnerClientId, this);
        if (IsOwner)
            localClient = this;
    }

    public override void OnNetworkDespawn()
    {
        steamID.OnValueChanged -= OnSteamIDChanged;
        ready.OnValueChanged -= OnReadyChanged;
        if (persistentClients.ContainsKey(OwnerClientId))
            persistentClients.Remove(OwnerClientId);
    }

    void OnSteamIDChanged(ulong previous, ulong current)
    {
        steamName = new Friend(current).Name;
        gameObject.name = "Persistent Client: " + steamName;
    }

    void OnReadyChanged(bool previous, bool current)
    {
        onReadyChanged?.Invoke(OwnerClientId, current);
        if (IsHost && current && GameManager.Instance != null)
        {
            GameManager.Instance.SpawnPlayer(this);
        }
    }

    public void GoToScene(string sceneName)
    {
        if (!IsHost) return;
        GoToSceneClientRPC(sceneName);
    }

    [ClientRpc]
    void GoToSceneClientRPC(string sceneName)
    {
        //LoadingScreen.sceneToLoad = sceneName;
        //SceneManager.LoadScene("Loading");
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetReadyStatusServerRPC(bool status, ServerRpcParams serverRpcParams = default)
    {
        ulong clientID = serverRpcParams.Receive.SenderClientId;
        if (persistentClients.TryGetValue(clientID, out var client))
            client.ready.Value = status;
    }

    public void NewGameStarted()
    {
        if (IsHost)
        {
            allPlayersReady = false;
            foreach (var player in persistentClients)
            {
                player.Value.ready.OnValueChanged += OnPlayerReadyStatusChanged;
            }

            OnPlayerReadyStatusChanged(false, false);
        }
    }
    
    void OnPlayerReadyStatusChanged(bool oldValue, bool newValue)
    {
        if (IsHost)
        {
            foreach (var player in persistentClients)
            {
                if (player.Value.ready.Value == false)
                {
                    return;
                }
            }

            allPlayersReady = true;
            
            foreach (var player in persistentClients)
            {
                if (GameManager.Instance != null) 
                    GameManager.Instance.SpawnPlayer(player.Value);
            }
        }
    }

    //[ServerRpc(RequireOwnership = true)]
    //public void SpawnLaunchableServerRPC(ELaunchableType type, Vector3 position, Vector3 direction, string weaponID, ServerRpcParams parameters = default)
    //{
        //GameObject launchable = Instantiate(GameManager.Instance.launchables[(int)type], position, Quaternion.identity);
        //LaunchableBase throwable = launchable.GetComponent<LaunchableBase>();
        //throwable.Initialize(parameters.Receive.SenderClientId, direction, weaponID);
        //NetworkObject networkObject = launchable.GetComponent<NetworkObject>();
        //networkObject.SpawnWithOwnership(parameters.Receive.SenderClientId);
    //}

    //[ServerRpc(RequireOwnership = true)]
    //public void SpawnSpawnableServerRPC(ESpawnableType type, Vector3 position, Vector3 rotation, byte[] extraData, ServerRpcParams parameters = default)
    //{
    //    SpawnSpawnableClientRPC(type, position, rotation, extraData, parameters.Receive.SenderClientId);
    //}

    //[ClientRpc]
    //public void SpawnSpawnableClientRPC(ESpawnableType type, Vector3 position, Vector3 rotation, byte[] extraData, ulong clientID)
    //{
    //    if (NetworkManager.LocalClientId == clientID) return;
    //    switch (type)
    //    {
    //        default:
    //            //Instantiate(GameManager.Instance.spawnables[(int)type], position, Quaternion.Euler(rotation));
    //            break;
    //        case ESpawnableType.BounceLaser:
    //            //GameObject newBounceLaser = Instantiate(GameManager.Instance.spawnables[(int)type], position, Quaternion.Euler(rotation));
    //            //float[] positions = extraData.ToFloatArray();
    //            //if (positions.Length == 9)
    //            //    newBounceLaser.GetComponent<BounceLaser>().Initialize(new Vector3(positions[0], positions[1], positions[2]), new Vector3(positions[3], positions[4], positions[5]), new Vector3(positions[6], positions[7], positions[8]));
    //            break;
    //    }
    //}
    
    public IEnumerator SpawnDelay(ulong playerID)
    {
        yield return new WaitForSeconds(3.0f);
        GameManager.Instance.SpawnPlayer(persistentClients[playerID]);
    }

    [ServerRpc(RequireOwnership = true)]
    public void SendGameDataToPlayerServerRPC(ulong playerID, RoomData roomData)
    {
        SendGameDataToPlayerClientRPC(/*MenuController.Instance == null ? */SceneManager.GetActiveScene().name, roomData, new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { playerID } } });
    }

    [ClientRpc]
    public void SendGameDataToPlayerClientRPC(string map, RoomData roomData, ClientRpcParams clientRpcParams = default)
    {
        CustomNetworkManager.Instance.roomData = roomData;
        if (!string.IsNullOrEmpty(map))
        {
            //LoadingScreen.sceneToLoad = map;
            //SceneManager.LoadScene("Loading");
        }
    }
    
    [ClientRpc]
    public void ShowWinningSequenceClientRPC(ulong[] winnerIDs, ulong lastDeathID)
    {
        //GameManager.Instance?.ShowWinningSequence(winnerIDs, lastDeathID);
    }
    
    [ClientRpc]
    public void StartSuspenseMusicClientRPC()
    {
        //GameManager.Instance?.scoreboard?.StartSuspenseMusic();
    }
}
