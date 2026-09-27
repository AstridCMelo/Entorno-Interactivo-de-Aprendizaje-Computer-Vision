
using UnityEngine;

public class DigitSpawner : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject cubePrefab;

    private int lastDigit = -1;

    void Update()
    {
        int currentDigit = udpReceive.digit;

        // Si no ha cambiado el dígito, no hacemos nada
        if (currentDigit == lastDigit)
            return;

        lastDigit = currentDigit;

        // Borrar cubos anteriores
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("SpawnedCube");

        foreach (GameObject cube in cubes)
        {
            Destroy(cube);
        }

        // Generar según el dígito
        if (currentDigit == 1)
        {
            SpawnCubes(1);
        }
        else if (currentDigit == 2)
        {
            SpawnCubes(2);
        }
    }

    void SpawnCubes(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 position = new Vector3(
                i * 2.0f,
                0,
                0
            );

            GameObject cube = Instantiate(
                cubePrefab,
                position,
                Quaternion.identity
            );

            cube.tag = "SpawnedCube";
        }
    }
}

