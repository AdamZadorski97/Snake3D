using System.Collections.Generic;
using UnityEngine;

public class SnakeController3D : MonoBehaviour
{
    public GameObject segmentPrefab;
    public float moveSpeed = 2.0f;   // Prędkość poruszania się węża (jednostki na sekundę)
    public float gridUnit = 1.0f;    // Odległość pomiędzy kafelkami siatki
    public float rotationSpeed = 1.0f;
    public List<Transform> snakeSegments { get; private set; }
    private Vector3 direction = Vector3.zero;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool hasStarted = false; // Flaga określająca, czy węż już się porusza

    private Vector2Int currentPosition; // Pozycja węża w siatce
    private GridManager3D gridManager;

    private Vector3 inputDirection;
    private Vector3 lastInputDirection;
    private Queue<Vector3> segmentPositions; // Kolejka przechowująca pozycje, po których porusza się węż
    private int score = 0;
    public CameraController cameraController;
    public void Start()
    {
        InitializeSnake(); // Używamy nowej metody
    }

    // Publiczna metoda inicjalizująca węża, dostępna w testach
    public void InitializeSnake()
    {
        snakeSegments = new List<Transform>();
        snakeSegments.Add(this.transform);

        gridManager = FindObjectOfType<GridManager3D>();
        currentPosition = new Vector2Int(gridManager.gridWidth / 2, gridManager.gridHeight / 2);
        targetPosition = transform.position;
        gridManager.SetSnakePosition(currentPosition.x, currentPosition.y);

        inputDirection = Vector3.zero; // Domyślny brak kierunku
        lastInputDirection = inputDirection; // Zapisujemy brak kierunku jako ostatni

        segmentPositions = new Queue<Vector3>();
        segmentPositions.Enqueue(transform.position);
        UIManager.Instance.UpdateScore(0);
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            return; // Do nothing if the game is over
        }
        HandleInput();

        if (!hasStarted) // Sprawdzamy, czy gra się rozpoczęła
        {
            if (inputDirection != Vector3.zero) // Rozpoczynamy ruch tylko po naciśnięciu klawisza kierunku
            {
                hasStarted = true;
            }
            else
            {
                return; // Nie poruszaj węża, dopóki gracz nie wciśnie klawisza
            }
        }

        // Jeśli węż się nie porusza, rozpoczęcie ruchu
        if (!isMoving)
        {
            MoveToNextPosition();
        }
        else
        {
            // Płynne poruszanie się w kierunku docelowym
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Jeśli osiągnięto docelową pozycję
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                isMoving = false; // Ruch został zakończony
                currentPosition = new Vector2Int(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.z)); // Aktualizujemy pozycję w siatce

                // Dodaj pozycję głowy do kolejki
                segmentPositions.Enqueue(transform.position);

                // Utrzymuj kolejkę w rozmiarze odpowiadającym długości węża
                if (segmentPositions.Count > snakeSegments.Count)
                {
                    segmentPositions.Dequeue();
                }

                // Sprawdzamy, czy węż zebrał item po dotarciu na pozycję
                CheckForItem();
            }

            // Aktualizujemy pozycje segmentów
            MoveSegments();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && lastInputDirection != Vector3.back)
            inputDirection = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.S) && lastInputDirection != Vector3.forward)
            inputDirection = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.A) && lastInputDirection != Vector3.right)
            inputDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D) && lastInputDirection != Vector3.left)
            inputDirection = Vector3.right;
    }

    void MoveToNextPosition()
    {
        // Set the new target position based on input direction
        lastInputDirection = inputDirection;
        targetPosition = transform.position + inputDirection * gridUnit;

        // Rotate the head towards the new direction
        transform.rotation = Quaternion.LookRotation(inputDirection);

        // Check if the snake is not hitting the edge of the grid
        Vector2Int newGridPosition = new Vector2Int(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.z));

        // If the new position is outside the grid bounds, trigger game over
        if (newGridPosition.x < 0 || newGridPosition.x >= gridManager.gridWidth ||
            newGridPosition.y < 0 || newGridPosition.y >= gridManager.gridHeight)
        {
            GameManager.Instance.GameOver(); // Snake hits the wall, game over
            return;
        }

        // Check if the snake is colliding with itself
        foreach (Transform segment in snakeSegments)
        {
            if (segment != this.transform && Vector3.Distance(segment.position, targetPosition) < 0.1f)
            {
                GameManager.Instance.GameOver(); // Snake collides with itself, game over
                return;
            }
        }

        // Check if the snake hits an obstacle
        if (gridManager.IsObstacleOnPosition(newGridPosition.x, newGridPosition.y))
        {
            GameManager.Instance.GameOver(); // Snake hits an obstacle, game over
            return;
        }

        // Start moving the snake if no collision was detected
        isMoving = true;
    }

    public void CheckForItem()
    {
        if (gridManager.IsItemOnPosition(currentPosition.x, currentPosition.y))
        {
            Grow();
            gridManager.ClearPosition(currentPosition.x, currentPosition.y); // Remove the item from the grid
            gridManager.SpawnSingleItemOnce(); // Spawn one new item
        }
    }

    public void Grow()
    {
        GameObject newSegment = Instantiate(segmentPrefab);
        Vector3 lastSegmentPosition = snakeSegments[snakeSegments.Count - 1].position;

        // Set the new segment's position to match the last segment's position
        newSegment.transform.position = lastSegmentPosition;

        // Set the rotation of the new segment to match the direction of the current movement
        newSegment.transform.rotation = Quaternion.LookRotation(lastInputDirection);

        // Add the new segment to the list
        snakeSegments.Add(newSegment.transform);

        // Enqueue the new segment's position to ensure it follows properly
        segmentPositions.Enqueue(lastSegmentPosition);
        score += 10; // Increment score by 10 points for each growth
        UIManager.Instance.UpdateScore(score);
    }

    void MoveSegments()
    {
        Vector3[] positionsArray = segmentPositions.ToArray();
        for (int i = 1; i < snakeSegments.Count; i++)
        {
            Vector3 targetSegmentPosition = positionsArray[i];
            Vector3 directionToTarget = targetSegmentPosition - snakeSegments[i].position;

            if (directionToTarget != Vector3.zero)
            {
                Quaternion segmentRotation = Quaternion.LookRotation(directionToTarget);
                snakeSegments[i].rotation = Quaternion.Slerp(snakeSegments[i].rotation, segmentRotation, rotationSpeed * Time.deltaTime);
            }

            snakeSegments[i].position = Vector3.MoveTowards(snakeSegments[i].position, targetSegmentPosition, moveSpeed * Time.deltaTime);
        }
    }

    public Vector2Int GetCurrentPosition()
    {
        return currentPosition;
    }

    public void SetCurrentPosition(Vector2Int newPosition)
    {
        currentPosition = newPosition;
        transform.position = new Vector3(newPosition.x, 1, newPosition.y);
        gridManager.SetSnakePosition(newPosition.x, newPosition.y);
    }

    public int GetSnakeLength()
    {
        return snakeSegments.Count;
    }
}
