using UnityEngine;
using UnityEngine.UI;
public class StealthIndicator : MonoBehaviour
{
    public Image arrowImage;
    private EnemyAwareness _target;
    private Transform _playerTransform;

    public void Setup(EnemyAwareness enemy, Transform player)
    {
        _target = enemy;
        _playerTransform = player;
    }

    void Update()
    {
        if (_target == null || _target.currentAwareness <= 0.05f || _target.IsAlerted)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = _target.transform.position - _playerTransform.position;
        dir.y = 0;

        float angle = Vector3.SignedAngle(_playerTransform.forward, dir, Vector3.up);

        transform.localRotation = Quaternion.Euler(0, 0, -angle);

        arrowImage.color = Color.Lerp(Color.white, Color.red, _target.currentAwareness);

        GetComponent<CanvasGroup>().alpha = _target.currentAwareness;
    }
}
