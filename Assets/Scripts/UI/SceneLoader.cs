namespace UI
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Handles loading a new scene.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// Load a scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
