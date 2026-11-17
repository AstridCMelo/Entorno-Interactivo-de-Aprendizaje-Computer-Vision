using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class spawnArea : MonoBehaviour
{
    protected readonly List<InteractableObject> cubeGrid = new List<InteractableObject>();

    protected int colGrid = 5;
    protected int rowGrid = 2;
    protected float offset = 1f;

    public void SpawnCube(InteractableObject cubePrefab, SpawnerContainer SOButton)
    {
        if (cubeGrid.Count < colGrid * rowGrid)
        {
            var cube2 = Instantiate(cubePrefab);
            cube2.SOButton = SOButton;
            cube2.ChangeMat();

            defaultlocation(cube2);

            //locate(cube2);
            ////Debug.Log(cube2);
            ////cubeGrid.Add(cube2);
        }
    }

    public void locate(InteractableObject cube)
    {
        if (cubeGrid.Count < colGrid * rowGrid)
        {
            int i = cubeGrid.Count;
            int col = i % colGrid;
            int row = i / colGrid;

            cube.transform.position = new Vector3(transform.position.x + (row * offset), transform.position.y, transform.position.z + (col * offset));
            cubeGrid.Add(cube);
        }
    }

    public void defaultlocation(InteractableObject cube)
    {
        cube.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
       if(other.CompareTag("InteractCube"))
       {
            var cube = other.GetComponent<InteractableObject>();
            locate(cube);
            Debug.Log("Cantidad en lista: " + cubeGrid.Count);
       }
       
    }
    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("Objeto del trigger: " + other.name);
        int grabCube = cubeGrid.IndexOf(other.GetComponent<InteractableObject>());

        if (grabCube >= 0)
        {
            cubeGrid.RemoveAt(grabCube);
            for (int i = grabCube; i < cubeGrid.Count; i++)
            {
                int col = i % colGrid;
                int row = i / colGrid;
                cubeGrid[i].transform.position = transform.position + new Vector3(row * offset, 0, col * offset);
            }
        }
    }

}
