using System;

namespace EasyChampionSelection.ECS.RiotGameData.GroupManager {

    /// <summary>
    /// A EventArgs class for GroupManager to determine the actions.
    /// </summary>
    public class GroupManagerEventArgs: EventArgs {
        /// <summary>
        /// The performed operation
        /// </summary>
        public readonly GroupManagerEventOperation eventOperation;
        /// <summary>
        /// The <c>ChampionList</c> undergoing the operation
        /// </summary>
        public readonly ChampionList operationItem;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="evntOp">The performed operation</param>
        /// <param name="hasChanged">The <c>ChampionList</c> undergoing the operation</param>
        public GroupManagerEventArgs(GroupManagerEventOperation evntOp, ChampionList hasChanged) {
            this.eventOperation = evntOp;
            this.operationItem = hasChanged;
        }
    }
}
