using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class ScreenModeSwitcher : MonoBehaviour
{
    [Header("UI Елементи")]
    public TextMeshProUGUI modeText;
    public Image buttonBackground;

    [Header("Стилі")]
    public Color activeColor = new Color(0.29f, 0.87f, 0.5f, 1f);
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = new Color(0.08f, 0.4f, 0.2f, 1f);
    public Color inactiveBgColor = new Color(0f, 0f, 0f, 0f);

    private bool isFullscreen = true;

    void Start()
    {
        isFullscreen = Screen.fullScreen;
        UpdateUI();
    }

    public void ToggleMode()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (isFullscreen)
        {
            modeText.text = "FULLSCREEN";
            modeText.color = activeTextColor;
            buttonBackground.color = activeColor;
        }
        else
        {
            modeText.text = "WINDOWED";
            modeText.color = inactiveTextColor;
            buttonBackground.color = inactiveBgColor;
        }
    }
}
