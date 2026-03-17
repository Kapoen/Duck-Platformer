namespace Utils
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Handles saving the game.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private string _savePath;
        private SaveData _currentSave = new SaveData();

        /// <summary>
        /// Gets the save manager instance.
        /// </summary>
        public static SaveManager Instance { get; private set; }

        /// <summary>
        /// Load the save data.
        /// </summary>
        /// <returns>The loaded save data.</returns>
        public SaveData Load()
        {
            if (!File.Exists(this._savePath))
            {
                return new SaveData();
            }

            string json = File.ReadAllText(this._savePath);
            SaveData save = JsonUtility.FromJson<SaveData>(json);

            this._currentSave = save;
            return save;
        }

        /// <summary>
        /// Add a level to the completed levels list and save it.
        /// </summary>
        /// <param name="levelName">The name of the level, which has been completed.</param>
        public void CompleteLevel(string levelName)
        {
            if (!this._currentSave.CompletedLevels.Contains(levelName))
            {
                this._currentSave.CompletedLevels.Add(levelName);
            }

            this.Save(this._currentSave);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            this._savePath = Application.persistentDataPath + "/save.json";
        }

        /// <summary>
        /// Save the data to a file.
        /// </summary>
        /// <param name="data">The save data.</param>
        private void Save(SaveData data)
        {
            this._currentSave = data;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(this._savePath, json);
        }
    }
}