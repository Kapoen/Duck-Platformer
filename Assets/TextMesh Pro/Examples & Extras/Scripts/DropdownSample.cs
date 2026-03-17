using TMPro;
using UnityEngine;

public class DropdownSample: MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text = null;

	[SerializeField]
	private TMP_Dropdown dropdownWithoutPlaceholder = null;

	[SerializeField]
	private TMP_Dropdown dropdownWithPlaceholder = null;

	public void OnButtonClick()
	{
		this.text.text = this.dropdownWithPlaceholder.value > -1 ? "Selected values:\n" + this.dropdownWithoutPlaceholder.value + " - " + this.dropdownWithPlaceholder.value : "Error: Please make a selection";
	}
}
