using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class QualityManager : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown; // Use TMP_Dropdown instead of Dropdown

    void Start()
    {
        // Populate TMP_Dropdown with quality levels
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }
}
