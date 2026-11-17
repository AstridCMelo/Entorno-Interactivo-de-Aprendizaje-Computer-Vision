
using UnityEngine;

public class ObjectGrabController : MonoBehaviour
{
    [Header("Referencias")]
    public UDPReceive udpReceive;
    public Camera camara;

    [Header("Configuracion")]
    public float tiempoParaAgarrar = 3f;
    public float distanciaRaycast = 100f;

    [Header("Estado")]
    public bool objetoAgarrado = false;

    private InteractableObject objetoActual;
    private InteractableObject objetoEnMano;

    private float tiempoSobreObjeto = 0f;

    private Vector3 offsetObjeto;

    private Rigidbody rbObjeto;
    private bool estadoKinematicAnterior;

    private Transform padreOriginal;

    // Referencia al contorno actual
    private Outline outlineActual;

   void Update()
{
    if (udpReceive == null || camara == null) return;

    int gesto = udpReceive.digit;

    if (gesto == 5 && objetoAgarrado)
    {
        SoltarObjeto();
        return;
    }

    if (objetoAgarrado)
    {
        MoverObjeto();
        return;
    }

    if (gesto == 0)
    {
        DetectarObjeto();
    }
    else
    {
        objetoActual = null;
        tiempoSobreObjeto = 0f;
        ActualizarOutline(null);
    }
}

    [Header("Detección")]
public float radioDeteccion = 0.15f;

void DetectarObjeto()
{
    Collider[] impactos = Physics.OverlapSphere(
        transform.position,
        radioDeteccion
    );

    InteractableObject objetoDetectado = null;
    float distanciaCercana = float.MaxValue;

    foreach (Collider impacto in impactos)
    {
        InteractableObject interactuable =
            impacto.GetComponentInParent<InteractableObject>();

        if (interactuable != null)
        {
            float distancia = Vector3.Distance(
                transform.position,
                impacto.ClosestPoint(transform.position)
            );

            if (distancia < distanciaCercana)
            {
                objetoDetectado = interactuable;
                distanciaCercana = distancia;
            }
        }
    }

    if (objetoDetectado == null)
    {
        objetoActual = null;
        tiempoSobreObjeto = 0f;
        ActualizarOutline(null);
        return;
    }

    if (objetoActual != objetoDetectado)
    {
        objetoActual = objetoDetectado;
        tiempoSobreObjeto = 0f;
        ActualizarOutline(objetoActual);
    }

    tiempoSobreObjeto += Time.deltaTime;

    if (tiempoSobreObjeto >= tiempoParaAgarrar)
    {
        AgarrarObjeto(objetoActual);
    }
}

    // ACTIVAR Y DESACTIVAR OUTLINE
    void ActualizarOutline(InteractableObject nuevoObjeto)
    {
        // Apagar contorno anterior
        if (outlineActual != null)
        {
            outlineActual.enabled = false;
            outlineActual = null;
        }

        if (nuevoObjeto == null)
            return;

        // Buscar Outline en el objeto
        outlineActual = nuevoObjeto.GetComponent<Outline>();

        if (outlineActual != null)
        {
            outlineActual.enabled = true;
        }
    }

    void AgarrarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        objetoEnMano = objeto;
        ActualizarOutline(objeto);
   
        objetoAgarrado = true;

        offsetObjeto =
            objeto.transform.position - transform.position;

        padreOriginal = objeto.transform.parent;

        rbObjeto = objeto.GetComponent<Rigidbody>();

        if (rbObjeto != null)
        {
            estadoKinematicAnterior = rbObjeto.isKinematic;

            rbObjeto.linearVelocity = Vector3.zero;
            rbObjeto.angularVelocity = Vector3.zero;

            rbObjeto.isKinematic = true;
        }

        Debug.Log("Objeto agarrado: " + objeto.name);

        tiempoSobreObjeto = 0f;
    }

    void MoverObjeto()
    {
        if (objetoEnMano == null)
            return;

        objetoEnMano.transform.position =
            transform.position + offsetObjeto;
    }

    public void SoltarObjeto()
    {
        if (objetoEnMano == null)
            return;

        objetoEnMano.transform.SetParent(padreOriginal, true);

        if (rbObjeto != null)
        {
            rbObjeto.isKinematic = estadoKinematicAnterior;
        }

        Debug.Log("Objeto soltado: " + objetoEnMano.name);

        objetoEnMano = null;
        objetoActual = null;

        rbObjeto = null;
        padreOriginal = null;

        objetoAgarrado = false;
        tiempoSobreObjeto = 0f;

        ActualizarOutline(null);
    }
}
