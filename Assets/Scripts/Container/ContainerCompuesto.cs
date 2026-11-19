using System;
using UnityEngine;

public class ContainerCompuesto : MonoBehaviour
{
    public int containerRight;
    public bool right = false;
    public Container cont1;
    public Container cont2;

    public Material rightMat;
    public void VerifyNumber()
    {
        int currentResult = cont1.numOperation + cont2.numOperation;

        if(currentResult == containerRight)
        {
            Debug.Log("El sumando es correcto");
            right = true;
            OperationsManager.Instance.VerifyOperationSuma2();

            changeMaterialRight(cont1);
            changeMaterialRight(cont2);
        }
        else
        {
            right = false;

            changeMaterialFail(cont1);
            changeMaterialFail(cont2);
        }
    }

    public void changeMaterialRight(Container objeto)
    {
        Renderer renderer = objeto.GetComponentInParent<Renderer>();
        renderer.material = rightMat;
    }

    public void changeMaterialFail(Container objeto)
    {
        Renderer renderer = objeto.GetComponentInParent<Renderer>();
        renderer.material = objeto.SOButton.NormalmaterialBoton;
    }

    //public void Relocate(Transform transform1, Transform transform2)
    //{
    //    cont1.RelocatetoResult(transform1);
    //    cont2.RelocatetoResult(transform2);
    //}
}
