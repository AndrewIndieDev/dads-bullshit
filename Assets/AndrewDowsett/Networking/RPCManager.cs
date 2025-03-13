using System;
using Unity.Netcode;

namespace AndrewDowsett.Networking
{
    public class RPCManager : NetworkBehaviour
    {
        public static RPCManager Instance;
        private void Awake()
        {
            Instance = this;
        }

        #region ServerRPCs
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void PingServerRPC()
        {
            PingClientRPC();
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        public void TrySetSeatColorServerRPC(ulong clientId, int index)
        {
            Character character = CharacterManager.Instance.Characters[index];
            if (!character || character.IsAssigned)
                return;

            PersistentClient client = PersistentClient.AllClients[clientId];
            CharacterManager.Instance.SetCharacter(index, client);
            client.SetPlayerSeatIndex(index);
        }
        #endregion

        #region ClientRPCs
        [Rpc(SendTo.ClientsAndHost)]
        void PingClientRPC()
        {
            if (IsOwner)
                PingFromClientServerRPC();
        }
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = true)]
        void PingFromClientServerRPC()
        {
            DateTime time = DateTime.UtcNow;
            ServerNetworkData.LocalPlayerData.nv_PreviousPing.Value = (int)(time - ServerNetworkData.Instance.LastPingTime).TotalMilliseconds;
        }
        #endregion
    }
}