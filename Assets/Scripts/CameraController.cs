using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;  // Obiekt, który kamera będzie śledzić (np. wąż)
    public float height = 10f; // Wysokość kamery nad planszą
    public float distance = 10f; // Dystans od środka planszy
    public float followSpeed = 5f; // Szybkość podążania kamery za celem
    public float lookSpeed = 5f; // Szybkość podążania kamery patrząc na cel

    public GridManager3D gridManager; // Referencja do GridManager3D

    void Start()
    {
        if (gridManager != null)
        {
            // Ustaw kamerę nad środkiem siatki i patrz na środek siatki
            MoveToCenterOfGrid();
        }
        else
        {
            Debug.LogError("GridManager3D nie znaleziony! Upewnij się, że istnieje w scenie.");
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            FollowTarget();
        }
    }

    // Metoda do ustawienia kamery nad środkiem siatki
    public void MoveToCenterOfGrid()
    {
        // Pobieramy rozmiar siatki z GridManager3D
        int gridWidth = gridManager.gridWidth;
        int gridHeight = gridManager.gridHeight;

        // Ustawiamy pozycję kamery nad środkiem siatki
        Vector3 centerPosition = new Vector3(gridWidth / 2f, height, gridHeight / 2f - distance);
        transform.position = centerPosition;

        // Ustawiamy punkt, na który kamera będzie patrzeć (środek siatki)
        Vector3 lookAtPosition = new Vector3(gridWidth / 2f, 0, gridHeight / 2f);
        transform.LookAt(lookAtPosition);
    }

    // Metoda do podążania kamery za celem z efektem płynności
    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + new Vector3(0, height, -distance);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Ustawienie, aby kamera płynnie patrzyła na cel
        Vector3 desiredLookAtPosition = target.position;
        Vector3 smoothLookAtPosition = Vector3.Lerp(transform.forward, (desiredLookAtPosition - transform.position).normalized, lookSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(smoothLookAtPosition);
    }
}
