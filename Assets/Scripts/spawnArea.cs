using System.Collections.Generic;
using UnityEngine;

public class spawnArea : MonoBehaviour
{
    List<InteractableObject> cubeGrid = new List<InteractableObject>();

    int colGrid = 5;
    int rowGrid = 2;
    float offset = 1f;

    bool[] full;

    public void SpawnCube(InteractableObject cubePrefab, SpawnerContainer SOButton)
    {
        if (cubeGrid.Count < colGrid * rowGrid)
        {
            int i = cubeGrid.Count;
            int col = i % colGrid;
            int row = i / colGrid;

            var cube2 = Instantiate(cubePrefab, new Vector3(transform.position.x + (row * offset), transform.position.y, transform.position.z + (col * offset)), Quaternion.identity);
            cube2.value = SOButton.value;

            cubeGrid.Add(cube2);
        }
    }

    public void locate(InteractableObject cubeOriginal)
    {
        if (cubeGrid.Count < colGrid * rowGrid)
        {
            int i = cubeGrid.Count;
            int col = i % colGrid;
            int row = i / colGrid;

            cubeOriginal.transform.position = new Vector3(transform.position.x + (row * offset), transform.position.y, transform.position.z + (col * offset));
            cubeGrid.Add(cubeOriginal);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        int grabCube = cubeGrid.IndexOf(other.GetComponentInChildren<InteractableObject>());
        cubeGrid.Remove(other.GetComponentInChildren<InteractableObject>());

        for (int i = grabCube; i < cubeGrid.Count; i++)
        {
            int col = i % colGrid;
            int row = i / rowGrid;
            cubeGrid[i].transform.position = transform.position + new Vector3(row * offset, 0, col * offset);
        }
    }

}
