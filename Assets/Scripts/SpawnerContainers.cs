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

    [SerializeField] private Renderer matButton;
    [SerializeField] private Material hoverMaterial;

    [SerializeField] private TextButton buttonText;

    private bool interaction = false;

    public ObjectGrabController controllerCube;

    private void Awake()
    {
        if (SOButton != null)
        {
            matButton.material = SOButton.NormalmaterialBoton;
            buttonText.ExtractName(SOButton.value);
        }
    }

    private void Update()
    {
       if(controllerCube.getObjetoMano() != null && (interaction == false))
       {
            SOButton = controllerCube.getObjetoMano();
            matButton.material = SOButton.NormalmaterialBoton;
            buttonText.ExtractName(SOButton.value);
       }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if(other.CompareTag("SpawnedCube"))
        {
            var cubeOriginal = other.GetComponent<InteractableObject>();
            if (SOButton.value == cubeOriginal.value)
            {
                StartCoroutine(Timer(cubeOriginal));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        var cubeOriginal = other.GetComponent<InteractableObject>();

        StopCoroutine(Timer(cubeOriginal));
        interaction = false;
    }

    IEnumerator Timer(InteractableObject cubeOriginal)
    {
        interaction = true;
        matButton.material = hoverMaterial;
        yield return new WaitForSeconds(timeInteraction);

        interaction = false;
        if (SOButton.value == cubeOriginal.value)
        {
            Debug.Log("Trigguer");
            controllerCube.SoltarObjeto();
            SpawnArea.defaultlocation(cubeOriginal);
            SpawnArea.SpawnCube(cubePrefab, SOButton);
        }

        matButton.material = SOButton.NormalmaterialBoton;
    }

}
