using UnityEngine;

public class Testing : MonoBehaviour
{
    Grid grid;
    private void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        float width = meshFilter.sharedMesh.bounds.size.x;
        float height = meshFilter.sharedMesh.bounds.size.z;

        float cellSize = width / 3;

        grid = new Grid(Mathf.FloorToInt(width/cellSize), Mathf.FloorToInt(height/cellSize), cellSize, transform);

        Debug.Log("Mesh width: " + meshFilter.sharedMesh.bounds.size.x);
        Debug.Log("Width calculado: " + width);
        Debug.Log("Cell size: " + cellSize);

        //Grid grid = new Grid(4, 2, 3f, transform);
    }

    public Vector3 setFullGrid()
    {
        return grid.SetFull();
    }

    public void setEmptyGrid()
    {
        grid.SetEmpty();
    }

}
