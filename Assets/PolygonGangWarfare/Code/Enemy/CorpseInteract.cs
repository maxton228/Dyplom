using UnityEngine;

public class CorpseInteract : MonoBehaviour, IInteractable
{
    private InteractablePrompt _prompt;
    private bool _isBeingDragged = false;
    private Health _health;

    void Awake()
    {
        _prompt = GetComponent<InteractablePrompt>();
        _health = transform.root.GetComponent<Health>();
    }

    public void ShowPrompt()
    {
        if (_isBeingDragged || (_health != null && !_health.isDead)) return;

        if (_prompt != null)
        {
            _prompt.Show("Тягнути тіло [F]", new Color(0.1f, 0.4f, 0.8f));
        }
    }

    public void StartDragging()
    {
        _isBeingDragged = true;
    }

    public void StopDragging()
    {
        _isBeingDragged = false;
    }
}