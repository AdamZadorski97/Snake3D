using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    public float moveSpeed = 2.0f; // Movement speed

    private Vector2Int currentPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private GridManager3D gridManager;
    private List<Vector2Int> pathToSnake = new List<Vector2Int>(); // Path from bot to snake
    private Vector2Int lastKnownSnakePosition;
    public void Initialize(Vector2Int startPosition, GridManager3D manager)
    {
        currentPosition = startPosition;
        gridManager = manager;
        transform.position = new Vector3(startPosition.x, 1, startPosition.y); // Set initial position
        lastKnownSnakePosition = gridManager.GetSnakePosition(); // Store the initial snake position
        CalculatePathToSnake(); // Initial path calculation to the snake
    }

    void Update()
    {
        // If bot is not moving and has a path, set the next position
        if (!isMoving && pathToSnake.Count > 0)
        {
            SetNextPosition();
        }

        // Move towards the target position
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    private void SetNextPosition()
    {
        // Only set the next position if there are remaining path steps
        if (pathToSnake.Count > 0)
        {
            currentPosition = pathToSnake[0]; // Update current grid position
            targetPosition = new Vector3(currentPosition.x, 1, currentPosition.y);
            pathToSnake.RemoveAt(0); // Remove the reached position from the path
            isMoving = true;
            Debug.Log("Bot moving to next position: " + currentPosition);
        }
    }


    private void MoveTowardsTarget()
    {
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if bot has reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false; // Movement is done
            Debug.Log("Bot reached target grid position: " + currentPosition);

            // Check for snake collision after each grid move
            CheckForSnakeCollision();

            // Update the snake position
            Vector2Int currentSnakePosition = gridManager.GetSnakePosition();

            // Recalculate the path if the snake has moved to a new position
            if (currentSnakePosition != lastKnownSnakePosition)
            {
                lastKnownSnakePosition = currentSnakePosition;
                CalculatePathToSnake();
            }

            // Set the next position based on the new or existing path
            SetNextPosition();
        }
    }

    private void CalculatePathToSnake()
    {
        Vector2Int snakePosition = gridManager.GetSnakePosition();
        pathToSnake = FindPath(currentPosition, snakePosition);

        if (pathToSnake.Count == 0)
        {
            Debug.Log("No path found to the snake!");
        }
        else
        {
            Debug.Log("New path calculated with " + pathToSnake.Count + " steps.");
        }
    }

    // A* Pathfinding algorithm
    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> openList = new List<Vector2Int> { start };
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gCost = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> fCost = new Dictionary<Vector2Int, int>();

        gCost[start] = 0;
        fCost[start] = GetHeuristic(start, target);

        while (openList.Count > 0)
        {
            Vector2Int current = GetLowestFCostNode(openList, fCost);

            if (current == target)
            {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || gridManager.IsObstacleOnPosition(neighbor.x, neighbor.y))
                {
                    continue; // Skip closed or obstacle nodes
                }

                int tentativeGCost = gCost[current] + 1;

                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
                else if (tentativeGCost >= gCost[neighbor])
                {
                    continue; // Not a better path
                }

                cameFrom[neighbor] = current;
                gCost[neighbor] = tentativeGCost;
                fCost[neighbor] = gCost[neighbor] + GetHeuristic(neighbor, target);
            }
        }

        return new List<Vector2Int>(); // Return empty path if no path found
    }

    private List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(node.x + 1, node.y),
            new Vector2Int(node.x - 1, node.y),
            new Vector2Int(node.x, node.y + 1),
            new Vector2Int(node.x, node.y - 1)
        };

        neighbors.RemoveAll(n => n.x < 0 || n.x >= gridManager.gridWidth || n.y < 0 || n.y >= gridManager.gridHeight);
        return neighbors;
    }

    private Vector2Int GetLowestFCostNode(List<Vector2Int> nodes, Dictionary<Vector2Int, int> fCost)
    {
        Vector2Int lowestNode = nodes[0];
        int lowestCost = fCost[lowestNode];

        foreach (var node in nodes)
        {
            if (fCost.TryGetValue(node, out int cost) && cost < lowestCost)
            {
                lowestNode = node;
                lowestCost = cost;
            }
        }

        return lowestNode;
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private void CheckForSnakeCollision()
    {
        Vector2Int snakePosition = gridManager.GetSnakePosition();
        if (snakePosition == currentPosition)
        {
            Debug.Log("Bot reached the snake! Game over.");
            GameManager.Instance.GameOver(); // Trigger game over if the bot reaches the snake head
        }
    }
}