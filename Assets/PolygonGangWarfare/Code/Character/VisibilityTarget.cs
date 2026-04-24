using UnityEngine;
using System.Collections.Generic;
public class VisibilityTarget : MonoBehaviour
{
    [Header("Налаштування сітки")]
    [Range(2, 10)] public int heightSegments = 5;
    [Range(2, 5)] public int widthSegments = 3;

    private CapsuleCollider _col;
    private List<Vector2> _normalizedPositions = new List<Vector2>();

    private List<Vector3> _cachedWorldPoints = new List<Vector3>();

    void Awake()
    {
        _col = GetComponent<CapsuleCollider>();
        CalculateNormalizedGrid();
    }

    void CalculateNormalizedGrid()
    {
        _normalizedPositions.Clear();

        for (int y = 0; y < heightSegments; y++)
        {
            float tY = (float)y / (heightSegments - 1);
            float yRatio = Mathf.Lerp(-0.5f, 0.5f, tY);

            for (int x = 0; x < widthSegments; x++)
            {
                float tX = (float)x / (widthSegments - 1);
                float xRatio = Mathf.Lerp(-0.5f, 0.5f, tX);

                _normalizedPositions.Add(new Vector2(xRatio, yRatio));
            }
        }
    }

    public List<Vector3> GetActivePoints()
    {
        _cachedWorldPoints.Clear();

        float currentHeight = _col.height;
        float currentRadius = _col.radius;
        Vector3 centerOffset = _col.center;

        float heightScale = currentHeight * 0.9f;
        float widthScale = currentRadius * 2f * 0.8f;

        foreach (var normPos in _normalizedPositions)
        {
            Vector3 localPos = new Vector3(
                normPos.x * widthScale,   
                normPos.y * heightScale,
                0                         
            );

            localPos += centerOffset;

            Vector3 worldPos = transform.TransformPoint(localPos);

            _cachedWorldPoints.Add(worldPos);
        }

        return _cachedWorldPoints;
    }

    void OnDrawGizmos()
    {
        if (_col == null) _col = GetComponent<CapsuleCollider>();
        if (_col == null) return;

        if (!Application.isPlaying)
        {
            CalculateNormalizedGrid();
        }

        Gizmos.color = Color.yellow;
        var points = GetActivePoints();
        foreach (var p in points)
        {
            Gizmos.DrawSphere(p, 0.05f);
        }
    }
}
