using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    public GridManager3D gridManager;

    // Main A* pathfinding method
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, HashSet<Vector2Int> snakeBodyPositions, Queue<Vector2Int> futureTailPositions, Vector2Int lastDirection)
    {
        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(startNode);

        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>
        {
            { startNode.position, startNode }
        };

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // If we reached the target node, return the path
            if (currentNode.position == targetNode.position)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                // Avoid blocking tail movement by checking if tail will vacate the position
                if (snakeBodyPositions.Contains(neighbor.position) && !futureTailPositions.Contains(neighbor.position))
                {
                    continue; // Avoid snake body but allow moving to future tail positions
                }

                // Allow backtracking and prevent 180-degree turns only if not cornered
                Vector2Int movementDirection = neighbor.position - currentNode.position;
                if (movementDirection == -lastDirection && !IsCornered(currentNode.position, snakeBodyPositions))
                {
                    continue; // Prevent 180-degree turns unless necessary
                }

                // Check if the neighbor is in the closed list or is an obstacle
                if (closedList.Contains(neighbor) || gridManager.IsSnakeOnPosition(neighbor.position.x, neighbor.position.y))
                {
                    continue;
                }

                float newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                // Penalize turns but allow backtracking if necessary
                if (movementDirection != lastDirection)
                {
                    newCostToNeighbor += 0.5f; // Slight penalty for turning
                }

                // Check if this is a new or better path
                if (!allNodes.ContainsKey(neighbor.position) || newCostToNeighbor < allNodes[neighbor.position].gCost)
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!allNodes.ContainsKey(neighbor.position))
                    {
                        allNodes.Add(neighbor.position, neighbor);
                    }

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null; // No valid path found
    }

    // Check if the snake is cornered (no valid moves)
    private bool IsCornered(Vector2Int currentPosition, HashSet<Vector2Int> snakeBodyPositions)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = currentPosition + direction;
            if (IsPositionValid(neighbor) && !snakeBodyPositions.Contains(neighbor))
            {
                return false; // Not cornered if there’s at least one valid direction
            }
        }
        return true; // Cornered if no valid moves exist
    }

    // Get neighbors (up, down, left, right) of the current node
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = node.position + direction;
            if (IsPositionValid(neighborPos))
            {
                neighbors.Add(new Node(neighborPos));
            }
        }

        return neighbors;
    }

    // Check if a position is valid (within bounds and not blocked)
    private bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.gridWidth && pos.y >= 0 && pos.y < gridManager.gridHeight;
    }

    // Calculate Manhattan distance (grid-based movement)
    private float GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.position.x - b.position.x);
        int distY = Mathf.Abs(a.position.y - b.position.y);
        return distX + distY; // Manhattan distance for grid-based movement
    }

    // Retrace the path from the end node to the start node
    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse(); // Reverse the path to get it from start to end
        return path;
    }
}