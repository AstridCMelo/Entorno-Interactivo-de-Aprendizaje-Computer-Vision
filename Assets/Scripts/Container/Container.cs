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
                numOperation = cubeGrid.Count * SOButton.value;
                Debug.Log(numOperation);
                contenedor.VerifyNumber();
            }
            else
            {
                //Se devuelve el objeto al spawn area
                cube.transform.position = new Vector3(transformDefault.position.x, transformDefault.position.y, transformDefault.position.z);
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

}
