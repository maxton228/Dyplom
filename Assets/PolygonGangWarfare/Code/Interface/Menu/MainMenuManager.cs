using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    [Header("Панелі")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Налаштування сцени")]
    public string gameSceneName = "Game";

    void Start()
    {
        ShowMain();
    }

    public void StartGame()
    {
        mainPanel.SetActive(false);
        loadingPanel.SetActive(true);
        Invoke("LoadScene", 6.5f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowMain()
    {
        settingsPanel.SetActive(false);
        loadingPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}