using UnityEngine;
using ShaderCrew;
using ShaderCrew.SeeThroughShader;
public class SnakeSpawner3D : MonoBehaviour
{
    public GameObject snakePrefab;
    public GridManager3D gridManager3D;

    private GameObject currentSnake;
    public CameraController cameraController;
    public PlayerToCameraRaycastTriggerManager playerToCameraRaycastTriggerManager;
    public PlayersPositionManager playerPositionManager;
    void Start()
    {
        gridManager3D.OnGridGenerated += SpawnSnake; // Register to the event
    }

    // Method to spawn the snake in the center of the grid
    public void SpawnSnake()
    {
        if (currentSnake == null) // Ensure the snake doesn't already exist
        {
            Vector3 spawnPosition = new Vector3(gridManager3D.gridWidth / 2, 1, gridManager3D.gridHeight / 2);
            currentSnake = Instantiate(snakePrefab, spawnPosition, Quaternion.identity);
            cameraController.target = currentSnake.transform;
            playerToCameraRaycastTriggerManager.playerList.Add(currentSnake);
            //playerPositionManager.playableCharacters.Add(currentSnake.gameObject);
        
        }
    }

    // Optional method to reset and respawn the snake
    public void RespawnSnake()
    {
        if (currentSnake != null)
        {
            Destroy(currentSnake);
        }
        SpawnSnake();
    }
}
