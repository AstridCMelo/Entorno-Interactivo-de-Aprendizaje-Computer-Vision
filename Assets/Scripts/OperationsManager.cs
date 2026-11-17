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

    private void Start()
    {
        Suma();
    }

    void UpdateInterface()
    {
        operacionTexto.text = num1.ToString() + operador + num2.ToString();
    }

    int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }

    void Suma()
    {
        num1 = Random(1,99);
        num2 = Random(1,99);

        containers[0].containerRight = num1;
        containers[1].containerRight = num2;

        operador = " + ";

        UpdateInterface();

        resultado = num1 + num2;

    }

    void Resta()
    {
        operador = " - ";
    }

    void VerifyOperation()
    {

    }


}
