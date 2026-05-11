using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class SliderValueDisplay : MonoBehaviour
{
    public TextMeshProUGUI valueText; 
    public bool isSensitivity = false; 

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        if (slider != null)
        {
            UpdateValue(slider.value);
        }
    }

    public void UpdateValue(float value)
    {
        if (valueText == null) return;

        if (isSensitivity)
        {
            float sensValue = value * 10f;
            valueText.text = "[" + sensValue.ToString("F1") + "]";
        }
        else
        {
            int percent = Mathf.RoundToInt(value * 100f);
            valueText.text = "[" + percent + "%]";
        }
    }
}