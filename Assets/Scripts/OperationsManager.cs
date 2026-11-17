using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperationsManager : MonoBehaviour
{
    public static OperationsManager Instance { get; private set; }

    public TextMeshProUGUI operacionTexto;
    public ContainerCompuesto[] containers;

    public int num1;
    public int num2;
    public int resultado;
    public string operador;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Suma();
    }

    void UpdateInterface()
    {
        operacionTexto.text = num1.ToString() + operador + num2.ToString();
    }

    void WinMessage()
    {
        operacionTexto.text = num1.ToString() + operador + num2.ToString() + " = " + resultado;
    }

    int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }

    void Suma()
    {
        num1 = Random(1,49);
        num2 = Random(1,49);

        containers[0].containerRight = num1;
        containers[1].containerRight = num2;

        operador = " + ";

        UpdateInterface();

        resultado = num1 + num2;

        containers[2].containerRight = resultado;
    }

    void Resta()
    {
        operador = " - ";
    }

    public void VerifyOperation()
    {
        if(containers[0].right == true && containers[1].right == true)
        {
            WinMessage();
            Debug.Log("Suma resuelta");
            //containers[0].Relocate(containers[2].cont1.transform, containers[2].cont2.transform);
            //containers[1].Relocate(containers[2].cont1.transform, containers[2].cont2.transform);
        }
    }

}
