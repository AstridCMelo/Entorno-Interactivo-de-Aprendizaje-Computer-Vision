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
    private void Awake()
    {
        colGrid = 3;
        rowGrid = 3;
        offset = 0.5f;
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
        base.OnTriggerEnter(other);
        numOperation = cubeGrid.Count * SOButton.value;
        Debug.Log(numOperation);
        contenedor.VerifyNumber();
    }


    public void RelocatetoResult(Transform transform)
    {
        foreach (InteractableObject cube in cubeGrid)
        {
            cube.transform.position = transform.position;   
        }
    }
}
