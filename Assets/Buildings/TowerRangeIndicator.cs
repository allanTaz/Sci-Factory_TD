using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TowerRangeIndicator : MonoBehaviour
{
    public TowerData towerData;
    public float towerRange;
    private LineRenderer lineRenderer;
    private const int segments = 64;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = new Color(1, 1, 1, 0.5f);
        lineRenderer.endColor = new Color(1, 1, 1, 0.5f);
    }

    public void ShowRange(bool show)
    {
        lineRenderer.enabled = show;
        if (show)
        {
            DrawRangeCircle();
        }
    }

    private void DrawRangeCircle()
    {
        if (towerData != null)
        {
            towerRange = towerData.range;
        }
        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = towerRange * Mathf.Cos(theta);
            float z = towerRange * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, 0, z);
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    public void UpdateRange()
    {
        if (lineRenderer.enabled)
        {
            DrawRangeCircle();
        }
    }
}