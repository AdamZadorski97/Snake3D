using UnityEngine;

public class Node
{
    public Vector2Int position;
    public float gCost; // Distance from the start node
    public float hCost; // Heuristic distance to the target node
    public Node parent; // Parent node, used to retrace the path

    public Node(Vector2Int pos)
    {
        position = pos;
        gCost = float.MaxValue; // Start with a high value for gCost
        hCost = 0;
        parent = null;
    }

    // Calculate fCost (gCost + hCost)
    public float fCost
    {
        get { return gCost + hCost; }
    }
}