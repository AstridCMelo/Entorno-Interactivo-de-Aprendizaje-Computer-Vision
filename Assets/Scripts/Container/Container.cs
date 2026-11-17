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
        offset = 3f;
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
        controllerCube.SoltarObjeto();
        base.OnTriggerEnter(other);
        numOperation = cubeGrid.Count * SOButton.value;
        Debug.Log(numOperation);
        contenedor.VerifyNumber();
    }
}
