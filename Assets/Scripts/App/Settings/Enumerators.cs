namespace Prototype.TopDown2DNetworked.Common
{
    public class Enumerators
    {
        public enum AppState
        {
            Undefined,

            JoinRoom,
            Game
        }

        public enum NetworkStatus
		{
            Undefined,

            JoinFailed,
            Joined
		}

        public enum NetworkEvent
		{
            PlayerInfo,
            PlayerHit,
            PlayerRevive,
            PlayerTransform
        }
    }
}