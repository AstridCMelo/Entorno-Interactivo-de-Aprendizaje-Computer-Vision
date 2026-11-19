using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    public static bool level1 = false;
    public static bool level2 = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeLevel();
    }

    private void ChangeLevel()
    {
        if (level1 == false)
        {
            Suma2();
        }
        else
        {
            Suma3();
        }
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(ReiniciarConEspera(1f));
        }
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

    void Suma2()
    {
        num1 = Random(1,49);
        num2 = Random(1,49);

        containers[0].containerRight = num1;
        containers[1].containerRight = num2;

        operador = " + ";

        UpdateInterface();

        resultado = num1 + num2;
    }

    void Suma3()
    {

    }

    public void VerifyOperationSuma2()
    {
        if(containers[0].right == true && containers[1].right == true)
        {
            WinMessage();
            Debug.Log("Suma resuelta");
            level1 = true;
            StartCoroutine(ReiniciarConEspera(10f));
        }
    }

    IEnumerator ReiniciarConEspera(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
