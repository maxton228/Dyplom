using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;


public class TacticalButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Текст цієї кнопки (TextMeshPro)")]
    public TextMeshProUGUI buttonText;

    public Color normalColor = new Color(0f, 0.7f, 0f);
    public Color hoverColor = new Color(0f, 1f, 0f);  

    private string originalText;

    void Start()
    {
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            originalText = buttonText.text;
            buttonText.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.text = "> " + originalText + "_";
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.text = originalText;
            buttonText.color = normalColor;
        }
    }
}