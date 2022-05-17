using System;
using System.Collections.Generic;

namespace WellEngineered.CruiseControl.Remote.Monitor
{
    /// <summary>
    /// A general data storage bag.
    /// </summary>
    public class DataBag
    {
        #region Private fields
        private Dictionary<Type, object> dataStore = new Dictionary<Type, object>();
        #endregion

        #region Public methods
        #region Get()
        /// <summary>
        /// Gets a data item from the bag.
        /// </summary>
        /// <typeparam name="TData">The type of data item to get.</typeparam>
        /// <returns>The data item if found, null otherwise.</returns>
        public TData Get<TData>()
        {
            var dataType = typeof(TData);
            if (this.dataStore.ContainsKey(dataType))
            {
                return (TData)this.dataStore[dataType];
            }
            else
            {
                return default(TData);
            }
        }
        #endregion

        #region Set()
        /// <summary>
        /// Sets a data item in the bag.
        /// </summary>
        /// <typeparam name="TData">The type of data item to set.</typeparam>
        /// <param name="value">The value to set.</param>
        public void Set<TData>(TData value)
        {
            var dataType = typeof(TData);
            this.dataStore[dataType] = value;
        }
        #endregion

        #region Delete()
        /// <summary>
        /// Deletes a data item from the bag.
        /// </summary>
        /// <typeparam name="TData">The type of data item to delete.</typeparam>
        public void Delete<TData>()
        {
            var dataType = typeof(TData);
            if (this.dataStore.ContainsKey(dataType))
            {
                this.dataStore.Remove(dataType);
            }
        }
        #endregion
        #endregion
    }
}
