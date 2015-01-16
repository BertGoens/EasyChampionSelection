using System;

namespace EasyChampionSelection.ECS.AppRuntimeResources.LolClient {
    /// <summary>
    /// An EventArgs class for <c>StaticPinvokeLolClient</c> to determine the actions.
    /// </summary>
    public class PinvokeLolClientEventArgs : EventArgs {
        /// <summary>
        /// The current state of the lolclient process.
        /// </summary>
        public readonly LolClientState CurrentState;

        /// <summary>
        /// The state before the current of the lolclient process.
        /// </summary>
        public readonly LolClientState LastState;

        public PinvokeLolClientEventArgs(LolClientState CurrentState, LolClientState LastState) {
            this.CurrentState = CurrentState;
            this.LastState = LastState;
        }
    }
}
