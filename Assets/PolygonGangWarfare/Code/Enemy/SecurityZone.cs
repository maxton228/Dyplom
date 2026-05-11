using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SecurityZone : MonoBehaviour
{
    [Header("Налаштування зони")]
    public bool isActive = true;

    [Tooltip("Радіус оповіщення")]
    public float alertRadius = 0f;

    [Header("Налаштування звуку")]
    public AudioClip firstAlertSound;
    public AudioClip secondAlertSound;

    private BoxCollider _triggerCollider;
    private AudioSource _audioSource;
    private bool _isAlarmPlaying = false;

    void Awake()
    {
        _triggerCollider = GetComponent<BoxCollider>();
        _triggerCollider.isTrigger = true;

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            AlertEnemies(other.transform.position);

            if (!_isAlarmPlaying)
            {
                StartCoroutine(PlayAlarmSequence());
            }
        }
    }

    private void AlertEnemies(Vector3 playerPosition)
    {
        var allEnemies = EnemyAwareness.AllEnemies;

        foreach (var enemyAwareness in allEnemies)
        {
            if (enemyAwareness == null) continue;

            TacticalEnemy enemy = enemyAwareness.GetComponent<TacticalEnemy>();
            if (enemy != null)
            {
                if (alertRadius <= 0f || Vector3.Distance(transform.position, enemy.transform.position) <= alertRadius)
                {
                    enemy.AlertFromAlly(playerPosition);
                }
            }
        }
    }

    private IEnumerator PlayAlarmSequence()
    {
        _isAlarmPlaying = true;

        if (firstAlertSound != null)
        {
            _audioSource.clip = firstAlertSound;
            _audioSource.Play();

            yield return new WaitForSeconds(firstAlertSound.length);
        }

        if (isActive && secondAlertSound != null)
        {
            _audioSource.clip = secondAlertSound;
            _audioSource.Play();
        }

    }

    public void DisableZone()
    {
        isActive = false;

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        _isAlarmPlaying = false;
    }
}