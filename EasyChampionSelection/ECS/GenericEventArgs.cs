using System;

namespace EasyChampionSelection.ECS {
    public class GenericEventArgs<TEventDataType> : EventArgs {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public TEventDataType Data { get; set; }
    }
}
