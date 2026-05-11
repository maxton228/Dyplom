using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractablePrompt))]
[RequireComponent(typeof(AudioSource))]
public class HackablePanelInteract : MonoBehaviour, IInteractable
{
    private InteractablePrompt _prompt;
    private AudioSource _audioSource;
    private bool _isHacked = false;

    [Header("Візуальні ефекти")]
    [SerializeField] private ParticleSystem _electricitySparks;

    [Header("Звукові ефекти (Кліпи)")]
    [SerializeField] private AudioClip _primaryClip;

    [SerializeField] private AudioClip _secondaryClip;

    [Header("Події")]
    public UnityEvent OnPanelHacked;

    void Awake()
    {
        _prompt = GetComponent<InteractablePrompt>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void ShowPrompt()
    {
        if (_isHacked) return;

        if (_prompt != null)
        {
            _prompt.Show("Зламати панель [F]", new Color(1f, 0.8f, 0.2f));
        }
    }

    public void Interact()
    {
        if (_isHacked) return;
        CompleteHack();
    }

    public void CompleteHack()
    {
        _isHacked = true;

        if (_electricitySparks != null)
        {
            _electricitySparks.Play();
        }

        if (_audioSource != null)
        {
            if (_primaryClip != null) _audioSource.PlayOneShot(_primaryClip);
            if (_secondaryClip != null) _audioSource.PlayOneShot(_secondaryClip);
        }

        OnPanelHacked?.Invoke();
    }
}