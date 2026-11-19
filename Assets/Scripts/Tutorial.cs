using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [Header("Controlador de mensajes")]
    public TextAnimationController controladorTexto;

    void Start()
    {
        Debug.Log("INICIO TUTORIAL: mostrando bienvenida");

        if (controladorTexto != null)
        {
            controladorTexto.MostrarMensajeUnaVez(
                "bienvenida",
                "¡Hola! Bienvenido a Fibonny, saluda a la camara"
            );
        }
        else
        {
            Debug.LogError("No se ha asignado el controlador de texto");
        }
    }
}