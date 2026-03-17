namespace Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The save data.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [SerializeField]
        private List<string> completedLevels = new List<string>();

        /// <summary>
        /// Gets the completed levels.
        /// </summary>
        public List<string> CompletedLevels
        {
            get { return this.completedLevels; }
        }
    }
}
