
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using System;
using System.Diagnostics;
using System.IO;
using System.Collections;

public class PythonLauncher : MonoBehaviour
{
    [Header("Interfaz de carga")]
    public Slider sliderCarga;
    public TMP_Text textPorcentaje;
    public TMP_Text textEstado;

    [Header("Configuracion")]
    public string escenaJuego = "GameScene";

    private Process procesoPython;

    private readonly object bloqueo = new object();

    private int progresoRecibido = 0;
    private string mensajeRecibido = "Preparando sistema...";
    private bool pythonListo = false;
    private bool errorPython = false;
    private bool escenaCargando = false;

    [Header("Pantallas")]
    public GameObject canvasLogo;
    public GameObject canvasCarga;

    [Header("Tiempo de presentación")]
    public float duracionLogo = 3f;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    [Header("Animación del slider")]
    public float velocidadSlider = 2f;

    private float progresoVisual = 0f;
    private float ultimoProgresoRecibido  = -1;
    
    IEnumerator Start()
    {
        canvasLogo.SetActive(true);
        canvasCarga.SetActive(false);

        CanvasGroup grupoLogo = canvasLogo.GetComponent<CanvasGroup>();

        // Fade In
        yield return StartCoroutine(FadeLogo(grupoLogo, 0f, 1f, 1f));

        // Mantener el logo visible
        yield return new WaitForSeconds(duracionLogo);

        // Fade Out
        yield return StartCoroutine(FadeLogo(grupoLogo, 1f, 0f, 1f));

        canvasLogo.SetActive(false);
        canvasCarga.SetActive(true);

        ActualizarInterfaz(0, "Preparando sistema...");

        IniciarPython();
    }

    IEnumerator FadeLogo(CanvasGroup grupo, float inicio, float final, float duracion)
    {
        float tiempo = 0f;

        grupo.alpha = inicio;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            grupo.alpha = Mathf.Lerp(inicio, final, tiempo / duracion);

            yield return null;
        }

        grupo.alpha = final;
    }


    void Update()
    {
        int progreso;
        string mensaje;

        lock (bloqueo)
        {
            progreso = progresoRecibido;
            mensaje = mensajeRecibido;
        }

        // Detectar si Python envió un nuevo intervalo
        if (progreso != ultimoProgresoRecibido)
        {
            ultimoProgresoRecibido = progreso;
            progresoVisual = progreso;
        }
        else
        {
            // Avanzar lentamente hasta el siguiente porcentaje
            progresoVisual = Mathf.MoveTowards(
                progresoVisual,
                Mathf.Min(ultimoProgresoRecibido + 1f, 99f),
                velocidadSlider * Time.deltaTime
            );
        }

        ActualizarInterfaz(progreso, mensaje);

        if (pythonListo && !escenaCargando)
        {
            escenaCargando = true;
            StartCoroutine(CargarEscenaJuego());
        }
    }

    void ActualizarInterfaz(int progreso, string mensaje)
    {
        // Si Python envía un porcentaje nuevo, saltar inmediatamente
        if (progreso != ultimoProgresoRecibido)
        {
            ultimoProgresoRecibido = progreso;
            progresoVisual = progreso;
        }

        if (sliderCarga != null)
            sliderCarga.value = progresoVisual;

        if (textPorcentaje != null)
            textPorcentaje.text = Mathf.FloorToInt(progresoVisual) + "%";

        if (textEstado != null)
            textEstado.text = mensaje;
    }

    void IniciarPython()
    {
        string carpetaPython = Path.Combine(
            Application.streamingAssetsPath,
            "HandTracking"
        );

        string rutaExe = Path.Combine(
            carpetaPython,
            "HandTracking.exe"
        );

        if (!File.Exists(rutaExe))
        {
            MostrarError("No se encontro HandTracking.exe");
            return;
        }

        try
        {
            ProcessStartInfo info = new ProcessStartInfo();

            info.FileName = rutaExe;
            info.WorkingDirectory = carpetaPython;

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            // Recibir mensajes de Python
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            procesoPython = new Process();

            procesoPython.StartInfo = info;

            procesoPython.OutputDataReceived +=
                RecibirMensajePython;

            procesoPython.ErrorDataReceived +=
                RecibirErrorPython;

            procesoPython.Start();

            procesoPython.BeginOutputReadLine();
            procesoPython.BeginErrorReadLine();

            UnityEngine.Debug.Log(
                "HandTracking iniciado correctamente."
            );
        }
        catch (Exception e)
        {
            MostrarError("Error iniciando Python: " + e.Message);
        }
    }

    void RecibirMensajePython(
        object sender,
        DataReceivedEventArgs e
    )
    {
        if (string.IsNullOrEmpty(e.Data))
            return;

        UnityEngine.Debug.Log("PYTHON: " + e.Data);

        string[] partes = e.Data.Split(
            new char[] { '|' },
            3
        );

        if (partes.Length >= 3 && partes[0] == "PROGRESS")
        {
            int porcentaje;

            if (int.TryParse(partes[1], out porcentaje))
            {
                lock (bloqueo)
                {
                    progresoRecibido = Mathf.Clamp(
                        porcentaje, 0, 100
                    );

                    mensajeRecibido = partes[2];

                    if (porcentaje >= 100)
                        pythonListo = true;
                }
            }
        }

        if (partes.Length >= 2 && partes[0] == "ERROR")
        {
            MostrarError(partes[1]);
        }
    }

    void RecibirErrorPython(
        object sender,
        DataReceivedEventArgs e
    )
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.LogError(
                "PYTHON ERROR: " + e.Data
            );
        }
    }

    void MostrarError(string mensaje)
    {
        lock (bloqueo)
        {
            errorPython = true;
            mensajeRecibido = mensaje;
        }

        UnityEngine.Debug.LogError(mensaje);
    }

    IEnumerator CargarEscenaJuego()
    {
        if (errorPython)
            yield break;

        UnityEngine.Debug.Log(
            "Python listo. Cargando escena del juego..."
        );

        AsyncOperation operacion =
            SceneManager.LoadSceneAsync(escenaJuego);

        while (!operacion.isDone)
        {
            yield return null;
        }
    }

    void OnApplicationQuit()
    {
        CerrarPython();
    }

    void OnDestroy()
    {
        CerrarPython();
    }

    void CerrarPython()
    {
        if (procesoPython == null)
            return;

        try
        {
            if (!procesoPython.HasExited)
            {
                procesoPython.Kill();
                procesoPython.WaitForExit(2000);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning(e.Message);
        }

        procesoPython.Dispose();
        procesoPython = null;
    }
}
