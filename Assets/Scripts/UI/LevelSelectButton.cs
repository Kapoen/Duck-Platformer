namespace UI
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;
    using Utils;

    /// <summary>
    /// Handles the level select button.
    /// </summary>
    public class LevelSelectButton : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField]
        private string levelName;
        [SerializeField]
        private List<string> requiredLevels;

        [Header("Color")]
        [SerializeField]
        private Color lockedColor = Color.gray4;
        [SerializeField]
        private Color normalColor = Color.white;
        [SerializeField]
        private Color completedColor = Color.darkGoldenRod;

        private void Start()
        {
            SaveData data = SaveManager.Instance.Load();
            Image buttonImage = this.gameObject.GetComponent<Image>();

            if (data.CompletedLevels.Contains(this.levelName))
            {
                buttonImage.color = this.completedColor;
            }
            else if (this.requiredLevels.Any(level => data.CompletedLevels.Contains(level)))
            {
                buttonImage.color = this.normalColor;
            }
            else
            {
                buttonImage.color = this.lockedColor;
                this.gameObject.GetComponent<Button>().interactable = false;
            }
        }
    }
}
