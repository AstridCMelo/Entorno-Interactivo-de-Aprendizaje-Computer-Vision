using System;
using UnityEngine;

public class ContainerCompuesto : MonoBehaviour
{
    public int containerRight;
    public bool right = false;
    public Container cont1;
    public Container cont2;


    public void VerifyNumber()
    {
        int currentResult = cont1.numOperation + cont2.numOperation;

        if(currentResult == containerRight)
        {
            Debug.Log("El sumando es correcto");
            right = true;
            OperationsManager.Instance.VerifyOperationSuma2();
        }

    }

    //public void Relocate(Transform transform1, Transform transform2)
    //{
    //    cont1.RelocatetoResult(transform1);
    //    cont2.RelocatetoResult(transform2);
    //}
}
