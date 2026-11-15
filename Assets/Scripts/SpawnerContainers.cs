using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnerContainers : MonoBehaviour
{
    [SerializeField] private SpawnerContainer SOButton;
    [SerializeField] private InteractableObject cubePrefab;
    [SerializeField] private float timeInteraction = 4f;
    [SerializeField] private spawnArea SpawnArea;

    public ObjectGrabController controllerCube;

    private void Start()
    {
        Debug.Log(SOButton.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Timer(other));
    }

    private void OnTriggerExit(Collider other)
    {
        StopCoroutine(Timer(other));
    }

    IEnumerator Timer(Collider other)
    {
        yield return new WaitForSeconds(timeInteraction);

        var cubeOriginal = other.GetComponentInChildren<InteractableObject>();

        if (SOButton.value == cubeOriginal.value)
        {
            Debug.Log("Trigguer");
            controllerCube.SoltarObjeto();
            SpawnArea.locate(cubeOriginal);
            SpawnArea.SpawnCube(cubePrefab, SOButton);
        }
    }

}
