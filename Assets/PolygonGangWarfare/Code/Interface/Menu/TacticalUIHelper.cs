using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class TacticalUIHelper : MonoBehaviour
{
    [Header("Ефекти HUD")]
    [Tooltip("Текстовий об'єкт для годинника")]
    public TextMeshProUGUI clockText;


    void Update()
    {
        if (clockText != null)
        {
            clockText.text = DateTime.Now.ToString("HH:mm:ss") + " LOC";
        }

    }
}