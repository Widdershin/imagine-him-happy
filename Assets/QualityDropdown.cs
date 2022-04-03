using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityDropdown : MonoBehaviour
{
    public TMPro.TMP_Dropdown dropdown;
    // Start is called before the first frame update
    void Start()
    {
        dropdown.value = QualitySettings.GetQualityLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnQualityChange()
    {
        QualitySettings.SetQualityLevel(dropdown.value, true);
    }
}
