using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnerContainers : MonoBehaviour
{
    [SerializeField] private SpawnerContainer SOButton;
    [SerializeField] private InteractableObject cubePrefab;
    [SerializeField] private float timeInteraction = 1f;
    [SerializeField] private spawnArea SpawnArea;

    [SerializeField] private Renderer matButton;
    [SerializeField] private Material hoverMaterial;

    [SerializeField] private TextButton buttonText;


    private bool interaction = false;

    public ObjectGrabController controllerCube;

     public TextAnimationController controladorTexto;


      void MostrarMensaje(string id, string mensaje)
    {
        if (controladorTexto != null)
        {
            controladorTexto.MostrarMensajeUnaVez(id, mensaje);
        }
    }

    bool DialogoTerminado()
    {
        return controladorTexto == null ||
               !controladorTexto.EstaProcesandoCola;
    }

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
        if (controllerCube.getObjetoMano() != null && !interaction)
        {
            SOButton = controllerCube.getObjetoMano();

            matButton.material = SOButton.NormalmaterialBoton;
            buttonText.ExtractName(SOButton.value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("SpawnedCube"))
        {
            var cubeOriginal = other.GetComponent<InteractableObject>();

            if (cubeOriginal != null && SOButton != null &&
                SOButton.value == cubeOriginal.value && !interaction)
            {
                interaction = true;
                StartCoroutine(Timer(cubeOriginal));

                MostrarMensaje(
                    "respuesta_botonSpawner",
                    "¡Mira cuantas cajas!"
                );

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("SpawnedCube"))
        {
            StopAllCoroutines();

            interaction = false;

            if (SOButton != null)
                matButton.material = SOButton.NormalmaterialBoton;

                MostrarMensaje(
                    "containers",
                    "Pon las cajas en los contenedores"
                );
        }
    }

    

    IEnumerator Timer(InteractableObject cubeOriginal)
    {
        interaction = true;
        matButton.material = hoverMaterial;

        while (interaction && cubeOriginal != null)
        {
            // Espera el tiempo configurado
            yield return new WaitForSeconds(timeInteraction);

            // Verifica que la interacción siga activa
            if (interaction && cubeOriginal != null &&
                SOButton != null &&
                SOButton.value == cubeOriginal.value)
            {
                Debug.Log("Spawn repetido");

                SpawnArea.SpawnCube(cubePrefab, SOButton);
            }
            else
            {
                break;
            }
        }

        interaction = false;

        if (SOButton != null)
            matButton.material = SOButton.NormalmaterialBoton;
    }
}