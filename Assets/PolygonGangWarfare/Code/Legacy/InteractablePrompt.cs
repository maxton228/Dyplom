using UnityEngine;
using TMPro;

public class InteractablePrompt : MonoBehaviour
{
    private GameObject _uiCanvas;
    private TextMeshProUGUI _promptText;
    private float _lastLookTime = -1f;
    private Camera _mainCamera;

    void Awake()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            _uiCanvas = canvas.gameObject;
            _promptText = _uiCanvas.GetComponentInChildren<TextMeshProUGUI>(true);
            _uiCanvas.SetActive(false);
        }

        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (_uiCanvas == null || !_uiCanvas.activeSelf) return;

        if (_mainCamera != null)
        {
            _uiCanvas.transform.LookAt(_uiCanvas.transform.position + _mainCamera.transform.rotation * Vector3.forward,
                             _mainCamera.transform.rotation * Vector3.up);
        }

        if (Time.time - _lastLookTime > 0.1f)
        {
            _uiCanvas.SetActive(false);
        }
    }

    public void Show(string text = "[F]", Color? color = null)
    {
        if (_uiCanvas == null) return;

        _lastLookTime = Time.time;

        if (!_uiCanvas.activeSelf) _uiCanvas.SetActive(true);

        if (_promptText != null)
        {
            _promptText.text = text;
            _promptText.color = color ?? Color.white;
        }
    }
}