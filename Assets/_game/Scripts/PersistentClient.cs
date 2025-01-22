using System;
using System.Collections;
using System.Collections.Generic;
using AndrewDowsett.SceneLoading;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentClient : NetworkBehaviour
{
    public static Dictionary<ulong, PersistentClient> AllClients = new Dictionary<ulong, PersistentClient>();
    public static PersistentClient LocalClient;
    
    public NetworkVariable<bool> nv_Ready = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> nv_LastPing = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<ulong> nv_SteamID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> nv_CharacterIndex = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Action<ulong, bool> onReadyChanged;
    public Action<ulong, int> onCharacterChanged;

    [SerializeField] private string steamName;
    [SerializeField] private bool allPlayersReady = false;
    
    private DateTime lastPingTime;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
            nv_SteamID.Value = SteamClient.SteamId;
        OnSteamIDChanged(0, nv_SteamID.Value);
        if (NetworkManager.IsHost)
        {
            StartCoroutine(UpdatePing());
        }
    }

    private IEnumerator UpdatePing()
    {
        while (true)
        {
            PingServerRPC();
            yield return new WaitForSeconds(2.0f);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void PingServerRPC()
    {
        lastPingTime = DateTime.UtcNow;
        PingClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PingClientRPC()
    {
        if (IsOwner)
            PingFromClientServerRPC();
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void PingFromClientServerRPC(RpcParams rpcParams = default)
    {
        DateTime time = DateTime.UtcNow;
        AllClients[rpcParams.Receive.SenderClientId].nv_LastPing.Value = (int)(time - lastPingTime).TotalMilliseconds;
    }

    public override void OnNetworkSpawn()
    {
        nv_SteamID.OnValueChanged += OnSteamIDChanged;
        nv_Ready.OnValueChanged += OnReadyChanged;
        nv_CharacterIndex.OnValueChanged += OnCharacterChanged;
        
        if (AllClients.ContainsKey(OwnerClientId))
            AllClients.Remove(OwnerClientId);
        AllClients.Add(OwnerClientId, this);
        
        if (IsOwner)
            LocalClient = this;
    }

    public override void OnNetworkDespawn()
    {
        nv_SteamID.OnValueChanged -= OnSteamIDChanged;
        nv_Ready.OnValueChanged -= OnReadyChanged;
        nv_CharacterIndex.OnValueChanged -= OnCharacterChanged;
        
        if (AllClients.ContainsKey(OwnerClientId))
            AllClients.Remove(OwnerClientId);
    }

    private void OnSteamIDChanged(ulong previous, ulong current)
    {
        steamName = new Friend(current).Name;
        gameObject.name = "Persistent Client: " + steamName;
    }

    private void OnReadyChanged(bool previous, bool current)
    {
        onReadyChanged?.Invoke(OwnerClientId, current);
        if (IsHost && current && GameManager.Instance != null)
        {
            GameManager.Instance.SpawnPlayer(this);
        }
    }

    private void OnCharacterChanged(int previousValue, int newValue)
    {
        onCharacterChanged?.Invoke(OwnerClientId, newValue);
    }

    public void GoToScene(string sceneName)
    {
        if (!IsHost) return;
        GoToSceneClientRPC(sceneName);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GoToSceneClientRPC(string sceneName)
    {
        SceneLoader.Instance.LoadScene(sceneName, true, LoadSceneMode.Single);
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void SetReadyStatusServerRPC(bool status, RpcParams rpcParams = default)
    {
        ulong clientID = rpcParams.Receive.SenderClientId;
        if (AllClients.TryGetValue(clientID, out var client))
            client.nv_Ready.Value = status;
    }

    public void NewGameStarted()
    {
        if (IsHost)
        {
            allPlayersReady = false;
            foreach (var player in AllClients)
            {
                player.Value.nv_Ready.OnValueChanged += OnPlayerReadyStatusChanged;
            }

            OnPlayerReadyStatusChanged(false, false);
        }
    }

    private void OnPlayerReadyStatusChanged(bool oldValue, bool newValue)
    {
        if (IsHost)
        {
            foreach (var player in AllClients)
            {
                if (player.Value.nv_Ready.Value == false)
                {
                    return;
                }
            }

            allPlayersReady = true;
            
            foreach (var player in AllClients)
            {
                if (GameManager.Instance != null) 
                    GameManager.Instance.SpawnPlayer(player.Value);
            }
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void SendGameDataToPlayerServerRPC(ulong playerID, RoomData roomData)
    {
        SendGameDataToPlayerClientRPC(SceneManager.GetActiveScene().name, roomData, new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { playerID } } });
    }

    [ClientRpc]
    public void SendGameDataToPlayerClientRPC(string map, RoomData roomData, ClientRpcParams clientRpcParams = default)
    {
        CustomNetworkManager.Instance.roomData = roomData;
        if (!string.IsNullOrEmpty(map))
        {
            SceneLoader.Instance.LoadScene(map, true, LoadSceneMode.Single);
        }
    }
}
