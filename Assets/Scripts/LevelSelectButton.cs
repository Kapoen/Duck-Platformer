using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private string levelName;
    [SerializeField] private List<string> requiredLevels;

    [Header("Color")]
    [SerializeField] private Color lockedColor = Color.gray4;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.darkGoldenRod;

    private void Start()
    {
        SaveData data = SaveManager.Instance.Load();
        Image buttonImage = gameObject.GetComponent<Image>();

        if (data.completedLevels.Contains(levelName))
        {
            buttonImage.color = completedColor;
        }
        else if (requiredLevels.All(level => data.completedLevels.Contains(level)))
        {
            buttonImage.color = normalColor;
        }
        else
        {
            buttonImage.color = lockedColor;
        }
    }
}
