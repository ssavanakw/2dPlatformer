using System.Collections.Generic;
using UnityEngine;

// A single cell in the pathfinding grid
public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;

    public int gCost, hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}

public class PathfindingGrid : MonoBehaviour
{
    [Header("Grid Area")]
    [Tooltip("Width/height of the area this grid covers, centered on this object")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(20f, 10f);
    [SerializeField] private float nodeRadius = 0.25f;

    [Header("Obstacles")]
    [Tooltip("Walls/platforms/hazards the enemy can't walk through")]
    [SerializeField] private LayerMask obstacleLayer;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private Vector2 GridOrigin => (Vector2)transform.position - gridWorldSize / 2f;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 origin = GridOrigin;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = origin
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up * (y * nodeDiameter + nodeRadius);

                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleLayer);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        Vector2 origin = GridOrigin;
        float percentX = Mathf.Clamp01((worldPos.x - origin.x) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPos.y - origin.y) / gridWorldSize.y);

        int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentX), 0, gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentY), 0, gridSizeY - 1);

        return grid[x, y];
    }

    public bool IsWalkable(Vector2 worldPos)
    {
        return NodeFromWorldPoint(worldPos).walkable;
    }

    public List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1f));

        if (grid == null) return;

        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable ? new Color(0f, 1f, 0f, 0.25f) : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
        }
    }
}