using UnityEngine;
using UnityEngine.InputSystem;

public class Lockpicking : MonoBehaviour
{

    [Header("3D Об'єкти")]
    public Transform lockpickPivot;
    public Transform cylinderPivot;

    [Header("Джерела звуку")]
    public AudioSource mainSource;
    public AudioSource movementSource;
    public AudioSource stuckSource;
    public AudioSource turnSource;

    [Header("Аудіо кліпи")]
    public AudioClip startSound;
    public AudioClip pickTickSound;
    public AudioClip stuckSound;
    public AudioClip turnSound;
    public AudioClip unlockSound;

    [Header("Параметри")]
    public float minPickAngle = -60f;
    public float maxPickAngle = 120f;
    public float sweetSpotRange = 8f;
    public float maxCylinderRotation = 90f;
    public float pickSpeed = 5f;
    public float cylinderSpeed = 120f;

    [Header("Налаштування довгого звуку відмички")]
    public float audioCooldown = 0.15f;
    private float currentAudioTimer = 0f;

    private float unlockAngle;
    private float currentPickAngle = 0f;
    private float currentCylinderAngle = 0f;
    private bool isTurning = false;
    private bool isUnlocked = false;

    public void ResetMinigame()
    {
        CancelInvoke();

        isUnlocked = false;
        isTurning = false;
        currentPickAngle = 0f;
        currentCylinderAngle = 0f;

        lockpickPivot.localEulerAngles = Vector3.zero;
        cylinderPivot.localEulerAngles = Vector3.zero;

        if (stuckSource != null) stuckSource.Stop();
        if (movementSource != null) movementSource.Stop();
        if (turnSource != null) turnSource.Stop();

        unlockAngle = Random.Range(minPickAngle, maxPickAngle);

        if (mainSource != null && mainSource.isActiveAndEnabled)
        {
            mainSource.PlayOneShot(startSound);
        }
        else
        {
            Debug.Log("Звук пропущено");
        }
    }

    void Start()
    {
        stuckSource.clip = stuckSound;
        stuckSource.loop = true;

        if (turnSound != null)
        {
            turnSource.clip = turnSound;
            turnSource.loop = true;
        }

        if (pickTickSound != null)
        {
            movementSource.clip = pickTickSound;
            movementSource.loop = true;
            movementSource.volume = 1f;
        }
    }

    void Update()
    {
        if (isUnlocked) return;

        HandleLockpick();
        HandleCylinder();
    }

    private void HandleLockpick()
    {
        if (isTurning)
        {
            if (movementSource.isPlaying) movementSource.Pause();
            return;
        }

        float moveInput = Mouse.current != null ? Mouse.current.delta.x.ReadValue() : 0f;
        currentPickAngle -= moveInput * pickSpeed * Time.deltaTime;
        currentPickAngle = Mathf.Clamp(currentPickAngle, minPickAngle, maxPickAngle);
        lockpickPivot.localEulerAngles = new Vector3(0, 0, currentPickAngle);

        bool isMouseMoving = Mathf.Abs(moveInput) > 0.5f;
        bool isNotBlocked = (currentPickAngle > minPickAngle + 1f) && (currentPickAngle < maxPickAngle - 1f);

        if (isMouseMoving && isNotBlocked)
        {
            currentAudioTimer = audioCooldown;
            if (!movementSource.isPlaying) movementSource.Play();
        }
        else
        {
            if (currentAudioTimer > 0) currentAudioTimer -= Time.deltaTime;
            else if (movementSource.isPlaying) movementSource.Pause();
        }
    }

    private void HandleCylinder()
    {
        bool isTrying = (Keyboard.current != null && Keyboard.current.wKey.isPressed) ||
                        (Mouse.current != null && Mouse.current.leftButton.isPressed);

        float previousCylinderAngle = currentCylinderAngle;

        if (isTrying)
        {
            isTurning = true;
            float distance = Mathf.Abs(currentPickAngle - unlockAngle);
            float allowed = (distance <= sweetSpotRange) ? maxCylinderRotation : Mathf.Clamp(maxCylinderRotation - (distance * 1.5f), 0f, maxCylinderRotation - 15f);

            if (currentCylinderAngle >= allowed && distance > sweetSpotRange)
            {
                ShakeLockpick();
                if (!stuckSource.isPlaying) stuckSource.Play();
            }
            else
            {
                if (stuckSource.isPlaying) stuckSource.Stop();
            }

            currentCylinderAngle = Mathf.MoveTowards(currentCylinderAngle, allowed, cylinderSpeed * Time.deltaTime);

            if (currentCylinderAngle >= maxCylinderRotation - 1f)
            {
                cylinderPivot.localEulerAngles = new Vector3(0, 0, -currentCylinderAngle);
                UnlockDoor();
                return;
            }
        }
        else
        {
            isTurning = false;
            currentCylinderAngle = Mathf.MoveTowards(currentCylinderAngle, 0f, cylinderSpeed * 2 * Time.deltaTime);
            if (stuckSource.isPlaying) stuckSource.Stop();
        }

        bool isCylinderMoving = Mathf.Abs(currentCylinderAngle - previousCylinderAngle) > 0.01f;

        if (isCylinderMoving)
        {
            if (!turnSource.isPlaying) turnSource.Play();
        }
        else
        {
            if (turnSource.isPlaying) turnSource.Stop();
        }

        cylinderPivot.localEulerAngles = new Vector3(0, 0, -currentCylinderAngle);
    }

    private void UnlockDoor()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        stuckSource.Stop();
        movementSource.Stop();
        turnSource.Stop();

        if (unlockSound != null) mainSource.PlayOneShot(unlockSound);

        Invoke("DisableMinigame", 0.5f);
    }

    private void DisableMinigame()
    {
        if (MinigameController.Instance != null)
        {
            MinigameController.Instance.StopLockpicking(true);
        }

        enabled = false;
    }

    private void ShakeLockpick()
    {
        float shake = Mathf.Sin(Time.time * 50f) * 1.2f;
        lockpickPivot.localEulerAngles = new Vector3(0, 0, currentPickAngle + shake);
    }
}