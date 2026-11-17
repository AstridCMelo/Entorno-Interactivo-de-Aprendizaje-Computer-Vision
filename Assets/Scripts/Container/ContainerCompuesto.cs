using System;
using UnityEngine;

public class ContainerCompuesto : MonoBehaviour
{
    public int containerRight;

    [SerializeField] Container cont1;
    [SerializeField] Container cont2;

    public void VerifyNumber()
    {
        int currentResult = cont1.numOperation + cont2.numOperation;

        if(currentResult == containerRight)
        {
            Debug.Log("El sumando es correcto");
        }

    }
}
