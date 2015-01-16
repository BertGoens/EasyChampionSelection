
namespace EasyChampionSelection.ECS.AppRuntimeResources.LolClient {
    /// <summary>
    /// An enum for all states our lolClient can be in
    /// </summary>
    public enum LolClientState {
        /// <summary>
        /// No client has been started
        /// </summary>
        NoClient = 1,

        /// <summary>
        /// Client active but not in any tracked state
        /// </summary>
        Client_Undefined = 2,

        /// <summary>
        /// Client is in champion select
        /// </summary>
        InChampSelect = 3,

        /// <summary>
        /// Client is 'afk', player is in game
        /// </summary>
        InGame = 4
    }
}
