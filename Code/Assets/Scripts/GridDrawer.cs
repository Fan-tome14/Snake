using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    public Collider2D gridArea; 
    public Color gridColor = Color.white;

    public int gridWidth = 17;
    public int gridHeight = 15;

    private void OnDrawGizmos()
    {
        if (gridArea == null)
            return;

        Gizmos.color = gridColor;

        Bounds bounds = gridArea.bounds;

        float cellWidth = bounds.size.x / gridWidth;
        float cellHeight = bounds.size.y / gridHeight;

        Vector2 origin = bounds.min;

        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = origin.x + x * cellWidth;
            Gizmos.DrawLine(
                new Vector3(xPos, origin.y, 0),
                new Vector3(xPos, bounds.max.y, 0)
            );
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            float yPos = origin.y + y * cellHeight;
            Gizmos.DrawLine(
                new Vector3(origin.x, yPos, 0),
                new Vector3(bounds.max.x, yPos, 0)
            );
        }
    }
}
