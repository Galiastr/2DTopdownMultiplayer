using Prototype.TopDown2DNetworked.Common;
using System;

namespace Prototype.TopDown2DNetworked
{
    public interface IAppStateManager
    {
        event Action<Enumerators.AppState> AppStateChangedEvent;

        Common.Enumerators.AppState AppState { get; set; }
        void ChangeAppState(Common.Enumerators.AppState stateTo);
    }
}