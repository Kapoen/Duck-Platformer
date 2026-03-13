using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utils {
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance;

        private string _savePath;
        private SaveData _currentSave = new SaveData();
    
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = Application.persistentDataPath + "/save.json";
        }

        /// <summary>
        /// Save the data to a file.
        /// </summary>
        /// <param name="data">The save data.</param>
        private void Save(SaveData data)
        {
            _currentSave = data;
        
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
        }

        /// <summary>
        /// Load the save data.
        /// </summary>
        /// <returns>The loaded save data.</returns>
        public SaveData Load()
        {
            if (!File.Exists(_savePath))
            {
                return new SaveData();
            }

            string json = File.ReadAllText(_savePath);
            SaveData save = JsonUtility.FromJson<SaveData>(json);

            _currentSave = save;
            return save;
        }

        /// <summary>
        /// Add a level to the completed levels list and save it.
        /// </summary>
        /// <param name="levelName">The name of the level, which has been completed.</param>
        public void CompleteLevel(string levelName)
        {
            if (!_currentSave.completedLevels.Contains(levelName))
            {
                _currentSave.completedLevels.Add(levelName);
            }
        
            Save(_currentSave);
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public List<string> completedLevels = new List<string>();
    }
}