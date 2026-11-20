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

    [Header("Reinicio del minijuego")]
public ObjectGrabController controllerCube;
public bool operacionesHabilitadas = false;

private bool esperandoSiguienteOperacion = false;

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
    if (!operacionesHabilitadas)
        return;

    if (Keyboard.current != null &&
        Keyboard.current.spaceKey.wasPressedThisFrame)
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
    num1 = Random(1, 49);
    num2 = Random(1, 49);

    containers[0].containerRight = num1;
    containers[1].containerRight = num2;

    operador = " + ";

    UpdateInterface();

    resultado = num1 + num2;

    Debug.Log("Nueva operación de nivel 2");
}

    public void VerifyOperationSuma2()
{
    if (!operacionesHabilitadas || esperandoSiguienteOperacion)
        return;

    if (containers[0].right && containers[1].right)
    {
        esperandoSiguienteOperacion = true;

        WinMessage();
        Debug.Log("Suma resuelta");

        level1 = true;

        StartCoroutine(ReiniciarConEspera(10f));
    }
}

    IEnumerator ReiniciarConEspera(float segundos)
{
    Debug.Log("Esperando 10 segundos...");

    yield return new WaitForSecondsRealtime(segundos);

    Debug.Log("Continuando con la siguiente operación");

    Time.timeScale = 1f;

    ReiniciarMinijuego();

    esperandoSiguienteOperacion = false;
}



public void ReiniciarMinijuego()
{
    // 1. Liberar el objeto agarrado
    if (controllerCube != null)
        controllerCube.ReiniciarAgarre();

    // 2. Eliminar todas las conexiones físicas
    FixedJoint[] conexiones = FindObjectsOfType<FixedJoint>();

    foreach (FixedJoint conexion in conexiones)
        Destroy(conexion);

    // 3. Limpiar los contenedores
    foreach (ContainerCompuesto compuesto in containers)
    {
        if (compuesto != null)
            compuesto.ReiniciarContenedorCompuesto();
    }

    // 4. Reiniciar los cubos originales y eliminar
    //    los cubos generados durante el juego
    InteractableObject[] cubos =
        FindObjectsOfType<InteractableObject>();

    foreach (InteractableObject cubo in cubos)
    {
        if (cubo.generadoDuranteJuego)
        {
            Destroy(cubo.gameObject);
        }
        else
        {
            cubo.ReiniciarObjeto();
        }
    }

    // 5. Generar una nueva operación
    ChangeLevel();

    Debug.Log("Minijuego reiniciado sin recargar la escena.");
}

}
