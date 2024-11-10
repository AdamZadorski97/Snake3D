using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public GameObject botPrefab;
    public GridManager3D gridManager;
    private GameObject currentBot;

  

    public void SpawnBot()
    {
        if (currentBot != null)
        {
            Destroy(currentBot);
        }

        Vector2Int botPosition = GetRandomBotPosition();
        Vector3 spawnPosition = new Vector3(botPosition.x, 1, botPosition.y);
        currentBot = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

        BotController botController = currentBot.GetComponent<BotController>();
        botController.Initialize(botPosition, gridManager);
    }

    private Vector2Int GetRandomBotPosition()
    {
        Vector2Int position;
        Vector2Int snakePosition = gridManager.GetSnakePosition();

        do
        {
            int x = Random.Range(0, gridManager.gridWidth);
            int z = Random.Range(0, gridManager.gridHeight);
            position = new Vector2Int(x, z);
        }
        while (Vector2Int.Distance(position, snakePosition) < gridManager.minDistanceFromSnake || gridManager.IsObstacleOnPosition(position.x, position.y));

        return position;
    }
}
