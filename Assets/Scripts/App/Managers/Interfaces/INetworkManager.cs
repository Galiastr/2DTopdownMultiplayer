using Prototype.TopDown2DNetworked.Common;
using System;
using System.Collections.Generic;

namespace Prototype.TopDown2DNetworked
{
    public interface INetworkManager
    {
        event Action<Enumerators.NetworkStatus, string> OnJoinRoomStatusChangedEvent;
        event Action OnRoomLeaveEvent;

        List<NetworkManager.PlayerInfo> PlayersInRoom { get; }

        void JoinRoom();
        void LeaveRoom();
        void SendNetworkEvent(Enumerators.NetworkEvent networkEvent, object[] data, bool all = true);
        void RefreshPlayersList();
    }
}
