using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Container : spawnArea
{
    public QuatityManager uiManager;
    public SpawnerContainer SOButton;
    public int numOperation = 0;
    public ContainerCompuesto contenedor;
    public ObjectGrabController controllerCube;

    public Transform transformDefault;

    
    [Header("Mensajes")]
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
        colGrid = 2;
        rowGrid = 5;
        offset = 1f;
        uiManager.ExtractName(SOButton.value);
    }

    //public int PositionValue()
    //{
    //    int position = SOButton.value;
    //    int num = cubeGrid.Count;

    //    return (num / position) % 10;
    //}

    protected override void OnTriggerEnter(Collider other)
    {
        //controllerCube.SoltarObjeto();
        if (other.CompareTag("SpawnedCube"))
        {
            var cube = other.GetComponent<InteractableObject>();

            if (cube.SOButton.value == SOButton.value)
            {
                locate(cube);
                Debug.Log("Cantidad en lista: " + cubeGrid.Count);
                MostrarMensaje(
                "contenedor_correcto",
                "Exacto, ese va ahi"
                );
                numOperation = cubeGrid.Count * SOButton.value;
                Debug.Log(numOperation);
                contenedor.VerifyNumber();
            }
            else
            {
                //Se devuelve el objeto al spawn area
                cube.transform.position = new Vector3(transformDefault.position.x, transformDefault.position.y, transformDefault.position.z);
                MostrarMensaje(
                "contenedor_incorrecto",
                "Nop, ese no va ahi"
                );
            }
        }
   
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        numOperation = cubeGrid.Count * SOButton.value;
        Debug.Log(numOperation);
        contenedor.VerifyNumber();

    }


        public void ReiniciarContenedor()
    {
        cubeGrid.Clear();
        numOperation = 0;

        if (contenedor != null)
            contenedor.right = false;

        if (SOButton != null && SOButton.NormalmaterialBoton != null)
        {
            Renderer r = GetComponentInParent<Renderer>();

            if (r != null)
                r.material = SOButton.NormalmaterialBoton;
        }
    }

}
