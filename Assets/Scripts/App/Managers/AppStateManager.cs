using Prototype.TopDown2DNetworked.Common;
using System;

namespace Prototype.TopDown2DNetworked
{
    public sealed class AppStateManager : IRoot, IAppStateManager
    {
        public event Action<Enumerators.AppState> AppStateChangedEvent;

        public Enumerators.AppState AppState { get; set; } = Enumerators.AppState.Undefined;

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void ChangeAppState(Enumerators.AppState stateTo)
        {
            if (AppState == stateTo)
                return;

            AppState = stateTo;

            AppStateChangedEvent?.Invoke(AppState);
        }
    }
}