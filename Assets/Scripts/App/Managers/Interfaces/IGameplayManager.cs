using Prototype.TopDown2DNetworked.Common;
using System.Collections.Generic;

namespace Prototype.TopDown2DNetworked
{
    public interface IGameplayManager 
    {
        List<Player> Players { get; }

        Player CreateNewPlayer(NetworkManager.PlayerInfo player);
        void DeletePlayer(int playerId);
    }
}