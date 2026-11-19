
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TextAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_Text texto;

    [Header("Animación")]
    public float duracionAnimacion = 0.3f;
    public float tiempoVisible = 6f;

    private Vector3 escalaOriginal;

    private Queue<string> colaMensajes = new Queue<string>();

    private HashSet<string> mensajesMostrados = new HashSet<string>();

    private bool procesandoCola = false;

    public bool EstaProcesandoCola
{
    get { return procesandoCola; }
}

    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    public void MostrarMensajeUnaVez(string id, string mensaje)
    {
        if (mensajesMostrados.Contains(id))
            return;

        mensajesMostrados.Add(id);

        MostrarMensaje(mensaje);
    }

    public void MostrarMensaje(string mensaje)
    {
        if (texto == null)
        {
            Debug.LogError("No se ha asignado el texto del diálogo.");
            return;
        }

        // Agregar mensaje a la cola
        colaMensajes.Enqueue(mensaje);

        // Iniciar procesamiento si no hay otro mensaje
        if (!procesandoCola)
        {
            procesandoCola = true;
            gameObject.SetActive(true);
            StartCoroutine(ProcesarCola());
        }
    }

    IEnumerator ProcesarCola()
    {

        while (colaMensajes.Count > 0)
        {
            string mensaje = colaMensajes.Dequeue();

            texto.text = mensaje;

            transform.localScale = Vector3.zero;

            // Aparición
            yield return StartCoroutine(
                AnimarEscala(Vector3.zero, escalaOriginal)
            );

            // Tiempo visible
            yield return new WaitForSeconds(tiempoVisible);

            // Desaparición
            yield return StartCoroutine(
                AnimarEscala(escalaOriginal, Vector3.zero)
            );
        }

        transform.localScale = escalaOriginal;

        procesandoCola = false;

        gameObject.SetActive(false);
    }

    IEnumerator AnimarEscala(Vector3 inicio, Vector3 fin)
    {
        if (duracionAnimacion <= 0f)
        {
            transform.localScale = fin;
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.deltaTime;

            float t = tiempo / duracionAnimacion;

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(
                inicio,
                fin,
                t
            );

            yield return null;
        }

        transform.localScale = fin;
    }
}
