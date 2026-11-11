using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Grid
{
    private int width;
    private int height;
    private int[,] gridArray;
    private float cellSize;
    private Transform referenceObject;
    private float offsetx;
    private float offsety;
    private float offsetx5;
    private float offsety5;
    private int emptyIndexX = 0;
    private int emptyIndexY = 0;
    public Grid(int width, int height, float cellSize, Transform referenceObject) { 
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.referenceObject = referenceObject;


        gridArray = new int[width,height];

        offsetx = - (width*cellSize) *.5f;
        offsety = - (height*cellSize) * .5f;
       
        var offset = new Vector3(offsetx,0,offsety);
        Debug.Log(offset);

        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                gridArray[i, j] = 0;
                Debug.Log(GetWorldPosition(i, j));
                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i, j + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i+1, j), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    public Vector3 SetFull()
    {
        Vector3 vectorRes = new Vector3(0, 0, 0);

        if (emptyIndexX >= 0 && emptyIndexY >= 0 && emptyIndexX < width && emptyIndexY < height)
        {
            offsetx5 -= cellSize * .5f;
            offsety5 -= cellSize * .5f;

            vectorRes = GetWorldPosition(emptyIndexX, emptyIndexY, offsetx, offsety5);
            gridArray[emptyIndexX, emptyIndexY] = 1;
            emptyIndexY++;

            if (emptyIndexY >= width && emptyIndexX < height)
            {
                emptyIndexY = 0;
                emptyIndexX++;
            }

            Debug.Log(emptyIndexX + " , " + emptyIndexY);
        }

        return vectorRes;

    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return referenceObject.TransformPoint(new Vector3(((x * cellSize) + offsetx), 0, (y * cellSize) + offsety));
    }

    public Vector3 GetWorldPosition(int x, int y, float offsetx, float offsety)
    {
        return referenceObject.TransformPoint(new Vector3(((x * cellSize) + offsetx), 0, (y * cellSize) + offsety));
    }


    public void SetEmpty()
    {
        if (emptyIndexX >= 0 && emptyIndexY >= 0 && emptyIndexX < width && emptyIndexY < height)
        {
            gridArray[emptyIndexX, emptyIndexY] = 0;
        }
    }

}
