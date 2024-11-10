using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameTests
{
    private GridManager3D gridManager;
    private SnakeController3D snakeController;
    private CameraController cameraController;
    private Camera camera;
    private List<GameObject> createdObjects; // Lista stworzonych obiektów

    // Inicjalizacja listy stworzonych obiektów
    [SetUp]
    public void Initialize()
    {
        createdObjects = new List<GameObject>();
        gridManager = GameObject.FindObjectOfType<GridManager3D>();

    }

    // Test sprawdzający, czy na scenie znajdują się odpowiednie skrypty
    [Test]
    public void RequiredScriptsArePresentOnScene()
    {
        // Sprawdzamy, czy na scenie znajduje się GridManager3D
        var gridManagerInScene = GameObject.FindObjectOfType<GridManager3D>();
        NUnit.Framework.Assert.IsNotNull(gridManagerInScene, "GridManager3D powinien być obecny na scenie.");

        // Sprawdzamy, czy na scenie znajduje się GridController3D
        var gridControllerInScene = GameObject.FindObjectOfType<GridManager3D>();
        NUnit.Framework.Assert.IsNotNull(gridControllerInScene, "GridController3D powinien być obecny na scenie.");

        // Sprawdzamy, czy na scenie znajduje się SnakeSpawner3D
        var snakeSpawnerInScene = GameObject.FindObjectOfType<SnakeSpawner3D>();
        NUnit.Framework.Assert.IsNotNull(snakeSpawnerInScene, "SnakeSpawner3D powinien być obecny na scenie.");

        // Sprawdzamy, czy na scenie znajduje się kamera
        var camera = GameObject.FindObjectOfType<Camera>();
        NUnit.Framework.Assert.IsNotNull(camera, "Kamera powinna być obecna na scenie.");
    }
    [Test]
    public void GridManagerHasAllVariablesAssigned()
    {

        gridManager = GameObject.FindObjectOfType<GridManager3D>();
        // Assert that tilePrefab is assigned
        NUnit.Framework.Assert.IsNotNull(gridManager.tilePrefab, "Prefab kafelka (tilePrefab) powinien być przypisany w GridManager3D.");

        // Assert that itemPrefab is assigned
        NUnit.Framework.Assert.IsNotNull(gridManager.itemPrefab, "Prefab itemu (itemPrefab) powinien być przypisany w GridManager3D.");

        // Assert that material1 is assigned
        NUnit.Framework.Assert.IsNotNull(gridManager.material1, "Materiał (material1) powinien być przypisany w GridManager3D.");

        // Assert that material2 is assigned
        NUnit.Framework.Assert.IsNotNull(gridManager.material2, "Materiał (material2) powinien być przypisany w GridManager3D.");
    }
    [Test]
    public void GridIsAtLeast4x4()
    {
        SetupWithObjects();

        NUnit.Framework.Assert.IsTrue(gridManager.gridWidth >= 4, $"Szerokość siatki powinna być większa lub równa 4, ale jest {gridManager.gridWidth}.");
        NUnit.Framework.Assert.IsTrue(gridManager.gridHeight >= 4, $"Wysokość siatki powinna być większa lub równa 4, ale jest {gridManager.gridHeight}.");
    }
    // Test sprawdzający, czy scena jest czysta przed wygenerowaniem siatki i innych obiektów
    [Test]
    public void NoCollidingObjectsOnGridAtStart()
    {
        // Zakładamy wielkość siatki, np. 10x10
        int gridWidth = 10;
        int gridHeight = 10;

        // Iterujemy po całej powierzchni siatki, sprawdzając, czy są tam jakiekolwiek obiekty
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 positionToCheck = new Vector3(x, 0, z);

                // Sprawdzamy, czy na tej pozycji znajdują się jakiekolwiek obiekty
                Collider[] colliders = Physics.OverlapSphere(positionToCheck, 0.1f);

                if (colliders.Length > 0) // Jeśli wykryto jakiekolwiek obiekty
                {
                    // Wypisujemy nazwy wszystkich obiektów znalezionych na tej pozycji
                    Debug.Log($"Na pozycji {positionToCheck} znajdują się następujące obiekty:");
                    foreach (var collider in colliders)
                    {
                        Debug.Log($"- {collider.gameObject.name}");
                    }
                }

                // Test asercji: upewnij się, że nie ma żadnych obiektów w tej pozycji
                NUnit.Framework.Assert.IsTrue(colliders.Length == 0, $"Na pozycji {positionToCheck} nie powinno być żadnych obiektów.");
            }
        }
    }

    // Ręczne generowanie siatki i ustawienie kamery
    public void SetupWithObjects()
    {
        // Tworzymy obiekt GridManager3D i dodajemy go do listy stworzonych obiektów
        GameObject gridManagerObj = new GameObject("GridManager");
        gridManager = gridManagerObj.AddComponent<GridManager3D>();

        // Ustawiamy prefabrykaty
        gridManager.tilePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridManager.tilePrefab.name = "GridTilePrefab";
        createdObjects.Add(gridManager.tilePrefab); // Dodanie kafelka do listy

        gridManager.itemPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gridManager.itemPrefab.name = "ItemPrefab";
        createdObjects.Add(gridManager.itemPrefab); // Dodanie itemu do listy

        // Ręcznie generujemy siatkę
        gridManager.CreateGrid();
        createdObjects.Add(gridManagerObj); // Dodanie obiektu do listy


        // Przemieszczamy kamerę na środek siatki
        cameraController = GameObject.FindObjectOfType<CameraController>();
        cameraController.MoveToCenterOfGrid();
    }

    // Test sprawdzający, czy kamera widzi całą siatkę po wygenerowaniu obiektów
    [Test]
    public void CameraCanSeeEntireGrid()
    {
        // Inicjalizacja sceny przed tym testem
        SetupWithObjects();
        camera = GameObject.FindObjectOfType<Camera>();
        // Sprawdzamy każdy kafelek, czy jest w polu widzenia kamery
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int z = 0; z < gridManager.gridHeight; z++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z); // Pozycja kafelka
                Vector3 viewportPoint = camera.WorldToViewportPoint(tilePosition);
                Vector3 lookAtPosition = new Vector3(gridManager.gridWidth / 2f, 0, gridManager.gridHeight / 2f);
                camera.transform.LookAt(lookAtPosition);
                // Sprawdzamy, czy kafelek znajduje się w zakresie widoku kamery
                bool isInView = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                                viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                                viewportPoint.z >= 0; // Z-odległość musi być dodatnia

                NUnit.Framework.Assert.IsTrue(isInView, $"Kafelek na pozycji {tilePosition} nie jest widoczny dla kamery.");
            }
        }
    }

    // Czyszczenie sceny po każdym teście
    [TearDown]
    public void TearDown()
    {
        // Usuwamy tylko obiekty, które zostały stworzone w testach
        foreach (var obj in createdObjects)
        {
            if (obj != null)
            {
                GameObject.DestroyImmediate(obj);
            }
        }

        // Czyszczenie listy po usunięciu obiektów
        createdObjects.Clear();
    }
}
