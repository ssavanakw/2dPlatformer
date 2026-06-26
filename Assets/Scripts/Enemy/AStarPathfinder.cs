using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    [SerializeField] private PathfindingGrid grid;

    public PathfindingGrid Grid => grid;

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (!targetNode.walkable) return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            // pick the node in openSet with the lowest fCost (cheapest total estimate)
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                   (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbour in grid.GetNeighbours(current))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = current.gCost + GetDistance(current, neighbour);

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = current;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null; // no path exists between start and target
    }

    private List<Vector2> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = endNode;

        while (current != startNode)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        // 10 = cost of a straight step, 14 ~= cost of a diagonal step (sqrt(2)*10, integer approx)
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}