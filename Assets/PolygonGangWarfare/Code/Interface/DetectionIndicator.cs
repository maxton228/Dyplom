using UnityEngine;
using UnityEngine.UI;
public class DetectionIndicator : MonoBehaviour
{
    public Image arrowImage;
    private EnemyAwareness _targetEnemy;
    private Transform _playerTransform;

    public void Init(EnemyAwareness enemy, Transform player)
    {
        _targetEnemy = enemy;
        _playerTransform = player;
    }

    void Update()
    {
        if (_targetEnemy == null || _targetEnemy.currentAwareness <= 0)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = _targetEnemy.transform.position - _playerTransform.position;
        direction.y = 0;

        float angle = Vector3.SignedAngle(_playerTransform.forward, direction, Vector3.up);

        transform.localRotation = Quaternion.Euler(0, 0, -angle);

        arrowImage.color = Color.Lerp(Color.white, Color.red, _targetEnemy.currentAwareness);

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = _targetEnemy.currentAwareness;
    }
}