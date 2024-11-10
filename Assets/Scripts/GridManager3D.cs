using DG.Tweening;
using ShaderCrew.SeeThroughShader;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObstacleList
{
    public List<GameObject> obstacles1x1 = new List<GameObject>();
    public List<GameObject> obstacles2x2 = new List<GameObject>();
    public List<GameObject> obstacles3x3 = new List<GameObject>();
    public List<GameObject> obstacles4x4 = new List<GameObject>();
}

[System.Serializable]
public class GrassList
{
    public List<GameObject> grass1x1 = new List<GameObject>();
}

public class GridManager3D : MonoBehaviour
{
    public int gridWidth = 20; // Width of the grid
    public int gridHeight = 20; // Height of the grid
    public GameObject tilePrefab; // Prefab for each tile
    public GameObject itemPrefab; // Prefab for the item
    public Material material1; // Material for tile type 1
    public Material material2; // Material for tile type 2
    public GameObject redCubePrefab; // Prefab for red cube to visualize obstacles

    // Configurable island parameters
    public int islandSize = 5; // Approximate size of the island (diameter)
    public float perlinScale = 5f; // Scale of Perlin noise used for heightmap
    public int minDistanceFromSnake = 5; // Minimum distance between the snake and the island
    public int minDistanceFromEdge = 3; // Minimum distance from the edge of the grid for obstacle blocks
    public int minItemDistanceFromObstacle = 2; // Minimum distance from an obstacle for item spawn
    public int numberOfItemsToSpawn = 3; // Number of items to spawn

    // Obstacle Lists
    public ObstacleList obstacleList = new ObstacleList();
    public GrassList grassList = new GrassList();
    // Grid array representing the map: 0 = empty, 1 = snake, 2 = item, 3 = obstacle
    public int[,] gridArray;
    private List<GameObject> currentItems = new List<GameObject>(); // References to the current items
    private List<GameObject> currentGrassTiles = new List<GameObject>(); // References to the current grass tiles
    public bool autoGenerateGrid = true;

    public delegate void GridGenerated();
    public event GridGenerated OnGridGenerated;
    public BotSpawner botSpawner;
    public GlobalShaderReplacement GlobalShaderReplacement;
    void Start()
    {
        if (autoGenerateGrid)
        {
            CreateGrid();
            SpawnHeightMapIslands(15); // Example: Spawning 5 heightmap-based islands
            SpawnItems(new HashSet<Vector2Int>()); // Empty set for the initial items spawn
            SpawnGrassTiles(120); // Example: Spawning 10 random grass tiles
            Invoke("UpdateShaders", 2);
        }
    }

    public void UpdateShaders()
    {
        GlobalShaderReplacement.UpdateAll();
    }

    private void OnAllTilesGenerated()
    {
        SpawnHeightMapIslands(5); // Example: Spawning 5 heightmap-based islands
        SpawnItems(new HashSet<Vector2Int>()); // Empty set for the initial items spawn
        botSpawner.SpawnBot();
        // Invoke the event to notify that the grid generation is complete
        OnGridGenerated?.Invoke();
    }

    // Create the grid at the start
    public void CreateGrid()
    {
        gridArray = new int[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 position = new Vector3(x, -1, z);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

                // Alternate materials to create a checkerboard effect
                MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
                renderer.material = ((x + z) % 2 == 0) ? material1 : material2;

                gridArray[x, z] = 0; // Initialize all cells as empty

                // Apply DoTween animation to scale and move tiles
                float randomDelay = Random.Range(0f, 1f); // Random delay for each tile
                tile.transform.localScale = Vector3.zero; // Start with scale 0
                tile.transform.DOMoveY(0f, 0.5f).SetDelay(randomDelay).SetEase(Ease.InSine); // Move Y axis
                tile.transform.DOScale(Vector3.one, 0.5f).SetDelay(randomDelay).SetEase(Ease.InSine); // Scale up
            }
        }

        // Call this after grid is generated
        Invoke("OnAllTilesGenerated", 1f); // Optional delay to ensure all animations complete
    }

    // Spawn heightmap-based islands at random positions with different obstacle sizes
    public void SpawnHeightMapIslands(int islandCount)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Find all available positions with respect to the minimum distance from edges
        for (int x = minDistanceFromEdge; x < gridWidth - minDistanceFromEdge; x++)
        {
            for (int z = minDistanceFromEdge; z < gridHeight - minDistanceFromEdge; z++)
            {
                if (gridArray[x, z] == 0)
                {
                    availablePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        // Spawn specified number of islands
        for (int i = 0; i < islandCount; i++)
        {
            if (availablePositions.Count == 0) break;

            // Randomly select a base position for the island
            Vector2Int islandBase = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(islandBase);

            // Ensure the island is not too close to the snake
            Vector2Int snakePosition = GetSnakePosition();
            if (Vector2Int.Distance(islandBase, snakePosition) < minDistanceFromSnake)
            {
                continue; // Skip if too close to the snake
            }

            // Choose a random obstacle from the lists
            GameObject obstaclePrefab = ChooseRandomObstacle();
            if (obstaclePrefab != null)
            {
                PlaceObstacle(islandBase, obstaclePrefab);
            }
        }
    }

    // Choose a random obstacle prefab from the available lists
    private GameObject ChooseRandomObstacle()
    {
        List<GameObject> allObstacles = new List<GameObject>();
        allObstacles.AddRange(obstacleList.obstacles1x1);
        allObstacles.AddRange(obstacleList.obstacles2x2);
        allObstacles.AddRange(obstacleList.obstacles3x3);
        allObstacles.AddRange(obstacleList.obstacles4x4);

        if (allObstacles.Count > 0)
        {
            return allObstacles[Random.Range(0, allObstacles.Count)];
        }
        else
        {
            return null;
        }
    }

    // Place an obstacle at the specified position and update the grid accordingly
    private void PlaceObstacle(Vector2Int basePosition, GameObject obstaclePrefab)
    {
        if (obstaclePrefab == null) return;

        int size = GetObstacleSize(obstaclePrefab);

        // Calculate center position for larger obstacles (like 2x2 or 3x3)
        Vector2 centerPosition = new Vector2(basePosition.x + (size - 1) / 2f, basePosition.y + (size - 1) / 2f);

        // Ensure the entire area is within bounds and not occupied
        bool canPlaceObstacle = true;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int gridX = basePosition.x + x;
                int gridZ = basePosition.y + z;
                if (gridX >= gridWidth || gridZ >= gridHeight || gridArray[gridX, gridZ] != 0)
                {
                    canPlaceObstacle = false;
                    break;
                }
            }
            if (!canPlaceObstacle) break;
        }

        if (canPlaceObstacle)
        {
            // Place the obstacle once at the center position
            Vector3 position = new Vector3(centerPosition.x, 0, centerPosition.y);
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);
            obstacle.transform.localScale = Vector3.zero;
            float randomDelay = Random.Range(0f, 1f);
            obstacle.transform.DOScale(Vector3.one, 0.5f).SetDelay(randomDelay).SetEase(Ease.InSine);

            // Mark the entire grid area as occupied by an obstacle
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    int gridX = basePosition.x + x;
                    int gridZ = basePosition.y + z;
                    gridArray[gridX, gridZ] = 3;

                    // Optional: visualize obstacle area with red cubes
                    if (redCubePrefab != null)
                    {
                        Vector3 redCubePosition = new Vector3(gridX, 1, gridZ);
                        Instantiate(redCubePrefab, redCubePosition, Quaternion.identity);
                    }
                }
            }
        }
    }

    // Get the size of the obstacle based on its prefab (assume square obstacles)
    private int GetObstacleSize(GameObject obstaclePrefab)
    {
        if (obstacleList.obstacles1x1.Contains(obstaclePrefab)) return 1;
        if (obstacleList.obstacles2x2.Contains(obstaclePrefab)) return 2;
        if (obstacleList.obstacles3x3.Contains(obstaclePrefab)) return 3;
        if (obstacleList.obstacles4x4.Contains(obstaclePrefab)) return 4;
        return 1; // Default size is 1x1 if not recognized
    }

    // Return the current position of the snake (this needs to be implemented based on your game logic)
    public Vector2Int GetSnakePosition()
    {
        SnakeController3D snakeController3D = FindObjectOfType<SnakeController3D>();
        if (snakeController3D != null)
        {
            Vector3 snakePos = snakeController3D.transform.position;
            Vector2Int position = new Vector2Int((int)snakePos.x, (int)snakePos.z);
            return position; // Example snake position
        }
        else
        {
            return new Vector2Int(gridWidth / 2, gridHeight / 2); // Example snake position
        }
    }

    // Check if the snake is on a given position
    public bool IsSnakeOnPosition(int x, int z)
    {
        return gridArray[x, z] == 1;
    }

    // Set a grid position as occupied by the snake
    public void SetSnakePosition(int x, int z)
    {
        gridArray[x, z] = 1;
    }

    // Clear a grid position when the snake moves away
    public void ClearPosition(int x, int z)
    {
        gridArray[x, z] = 0;
    }

    // Check if an item is on a given grid position
    public bool IsItemOnPosition(int x, int z)
    {
        return gridArray[x, z] == 2;
    }

    // Place multiple items on the grid ensuring they are not on snake segments and respecting obstacle distance
    public void SpawnItems(HashSet<Vector2Int> snakeBodyPositions)
    {
        // Destroy any existing items before spawning new ones
        foreach (GameObject item in currentItems)
        {
            Destroy(item);
        }
        currentItems.Clear();

        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Find all available positions that are not occupied by the snake or obstacles, and respect the min distance
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int position = new Vector2Int(x, z);

                // Check if the position is free, not part of the snake's body, and not too close to obstacles
                if (gridArray[x, z] == 0 && IsValidDistanceFromObstacles(position))
                {
                    availablePositions.Add(position);
                }
            }
        }

        // Spawn the specified number of items
        for (int i = 0; i < numberOfItemsToSpawn; i++)
        {
            if (availablePositions.Count == 0)
            {
                Debug.LogError("No valid empty space found to spawn the item.");
                return;
            }

            // Choose a random position from the list of available positions
            Vector2Int chosenPosition = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(chosenPosition);
            SetItemPosition(chosenPosition.x, chosenPosition.y);
        }
    }

    // Function to check if a position is a valid distance from obstacles
    private bool IsValidDistanceFromObstacles(Vector2Int position)
    {
        // Check the grid within minItemDistanceFromObstacle around the given position
        for (int x = -minItemDistanceFromObstacle; x <= minItemDistanceFromObstacle; x++)
        {
            for (int z = -minItemDistanceFromObstacle; z <= minItemDistanceFromObstacle; z++)
            {
                int checkX = position.x + x;
                int checkZ = position.y + z;

                if (checkX >= 0 && checkX < gridWidth && checkZ >= 0 && checkZ < gridHeight)
                {
                    if (gridArray[checkX, checkZ] == 3) // 3 represents an obstacle
                    {
                        return false; // If any nearby grid is an obstacle, the position is invalid
                    }
                }
            }
        }

        return true; // No nearby obstacles
    }

    // Place an item on the grid
    public void SetItemPosition(int x, int z)
    {
        gridArray[x, z] = 2;
        Vector3 position = new Vector3(x, 1, z); // Set the item slightly above the grid
        GameObject newItem = Instantiate(itemPrefab, position, Quaternion.identity);
        currentItems.Add(newItem);
    }
    public void SpawnSingleItemOnce()
    {
        // Lista dostępnych pozycji
        List<Vector2Int> freePositions = new List<Vector2Int>();

        // Przeszukaj planszę i znajdź wolne pozycje
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridArray[x, y] == 0) // 0 oznacza pustą pozycję
                {
                    freePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // Sprawdź, czy są dostępne wolne pozycje
        if (freePositions.Count > 0)
        {
            // Wybierz losową pozycję z wolnych
            Vector2Int randomPosition = freePositions[Random.Range(0, freePositions.Count)];
            SetItemPosition(randomPosition.x, randomPosition.y);
            Debug.Log("Item spawned at random free position: " + randomPosition);
        }
        else
        {
            Debug.Log("No free space available to spawn the item.");
        }
    }
    public bool IsObstacleOnPosition(int x, int y)
    {
        return gridArray[x, y] == 3; // Zakładamy, że 3 oznacza przeszkodę
    }

    public void SpawnGrassTiles(int grassCount)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Find all available positions on the grid
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (gridArray[x, z] == 0) // Ensure position is empty
                {
                    availablePositions.Add(new Vector2Int(x, z));
                }
            }
        }

        // Spawn specified number of grass tiles
        for (int i = 0; i < grassCount; i++)
        {
            if (availablePositions.Count == 0) break; // No more available positions

            Vector2Int grassPosition = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(grassPosition);

            // Choose a random grass prefab from the list
            if (grassList.grass1x1.Count > 0)
            {
                GameObject grassPrefab = grassList.grass1x1[Random.Range(0, grassList.grass1x1.Count)];
                Vector3 position = new Vector3(grassPosition.x, 1, grassPosition.y);
                GameObject grassTile = Instantiate(grassPrefab, position, Quaternion.identity);
                currentGrassTiles.Add(grassTile); // Store reference to the spawned grass tile

                // Update grid to mark this position as occupied by grass
                gridArray[grassPosition.x, grassPosition.y] = 4;
            }
        }
    }
}
