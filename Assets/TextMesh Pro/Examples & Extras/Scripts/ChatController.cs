using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatController : MonoBehaviour {


    public TMP_InputField ChatInputField;

    public TMP_Text ChatDisplayOutput;

    public Scrollbar ChatScrollbar;

    void OnEnable()
    {
        this.ChatInputField.onSubmit.AddListener(this.AddToChatOutput);
    }

    void OnDisable()
    {
        this.ChatInputField.onSubmit.RemoveListener(this.AddToChatOutput);
    }


    void AddToChatOutput(string newText)
    {
        // Clear Input Field
        this.ChatInputField.text = string.Empty;

        var timeNow = System.DateTime.Now;

        string formattedInput = "[<#FFFF80>" + timeNow.Hour.ToString("d2") + ":" + timeNow.Minute.ToString("d2") + ":" + timeNow.Second.ToString("d2") + "</color>] " + newText;

        if (this.ChatDisplayOutput != null)
        {
            // No special formatting for first entry
            // Add line feed before each subsequent entries
            if (this.ChatDisplayOutput.text == string.Empty)
                this.ChatDisplayOutput.text = formattedInput;
            else
                this.ChatDisplayOutput.text += "\n" + formattedInput;
        }

        // Keep Chat input field active
        this.ChatInputField.ActivateInputField();

        // Set the scrollbar to the bottom when next text is submitted.
        this.ChatScrollbar.value = 0;
    }

}
