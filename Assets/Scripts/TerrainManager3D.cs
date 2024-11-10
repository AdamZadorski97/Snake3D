using DG.Tweening;
using UnityEngine;

public class TerrainManager3D : MonoBehaviour
{
    public GameObject terrainPrefab; // Prefab kostki do budowy terenu
    public GridManager3D gridManager; // Referencja do GridManager3D
    public int heightIncrement = 1; // Przyrost wysokości z każdą pętlą

    // Funkcja do generowania terenu wokół siatki
    private void Start()
    {
        GenerateTerrainAroundGrid();
    }
    public void GenerateTerrainAroundGrid()
    {
        if (gridManager == null || terrainPrefab == null)
        {
            Debug.LogError("Brak referencji do GridManager3D lub prefabrykat terenu!");
            return;
        }

        int gridWidth = gridManager.gridWidth;
        int gridHeight = gridManager.gridHeight;

        // Ustawianie początkowego poziomu terenu na 0
        int terrainHeight = 1;

        // Pętla tworząca teren wokół siatki
        for (int ring = 1; ring <= Mathf.Max(gridWidth, gridHeight); ring++)
        {
            // Górna krawędź
            for (int x = -ring; x < gridWidth + ring; x++)
            {
                CreateTerrainBlock(new Vector3(x, terrainHeight, -ring));
                CreateTerrainBlock(new Vector3(x, terrainHeight, gridHeight + ring - 1));
            }

            // Prawa i lewa krawędź
            for (int z = -ring; z < gridHeight + ring; z++)
            {
                CreateTerrainBlock(new Vector3(-ring, terrainHeight, z));
                CreateTerrainBlock(new Vector3(gridWidth + ring - 1, terrainHeight, z));
            }

            // Zwiększ wysokość dla kolejnej pętli
            terrainHeight += heightIncrement;
        }
    }

    // Funkcja pomocnicza do tworzenia kostki terenu
  

    private void CreateTerrainBlock(Vector3 position)
    {
        GameObject terrainBlock = Instantiate(terrainPrefab, position + Vector3.up * 20f, Quaternion.identity); // Start above the grid to simulate falling
        terrainBlock.transform.localScale = Vector3.zero;
        // Generate a random delay to make the blocks fall at different times
        float randomDelay = Random.Range(0f, 2f); // Adjust the range as needed for desired effect

        // Add DoTween animation to make the block fall down with a random delay
        terrainBlock.transform.DOMoveY(position.y, 1f).SetDelay(randomDelay).SetEase(Ease.OutSine);
        terrainBlock.transform.DOScale(Vector3.one, 1f).SetDelay(randomDelay).SetEase(Ease.InSine);
    }
}
