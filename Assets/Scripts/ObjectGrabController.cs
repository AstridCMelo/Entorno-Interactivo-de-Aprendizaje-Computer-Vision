
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

    void Update()
    {
        if (udpReceive == null || camara == null)
            return;

        int gesto = udpReceive.digit;

        // GESTO 5: SOLTAR
        if (gesto == 5 && objetoAgarrado)
        {
            SoltarObjeto();
            return;
        }

        // Si ya tenemos un objeto, lo seguimos moviendo
        if (objetoAgarrado)
        {
            MoverObjeto();
            return;
        }

        // GESTO 0: INTENTAR AGARRAR
        if (gesto == 0)
        {
            DetectarObjeto();
        }
        else
        {
            objetoActual = null;
            tiempoSobreObjeto = 0f;
        }
    }

    void DetectarObjeto()
    {
        // Obtener la posición del cursor en pantalla
        Vector3 posicionPantalla =
            camara.WorldToScreenPoint(transform.position);

        // Crear un rayo desde la cámara
        Ray rayo = camara.ScreenPointToRay(posicionPantalla);

        RaycastHit[] impactos =
            Physics.RaycastAll(rayo, distanciaRaycast);

        // Buscar un objeto interactuable
        InteractableObject objetoDetectado = null;

        float distanciaCercana = float.MaxValue;

        foreach (RaycastHit impacto in impactos)
        {
            InteractableObject interactuable =
                impacto.collider.GetComponentInParent<InteractableObject>();

            if (interactuable != null &&
                impacto.distance < distanciaCercana)
            {
                objetoDetectado = interactuable;
                distanciaCercana = impacto.distance;
            }
        }

        // Si no encontramos ningún objeto
        if (objetoDetectado == null)
        {
            objetoActual = null;
            tiempoSobreObjeto = 0f;
            return;
        }

        // Si cambiamos de objeto, reiniciar contador
        if (objetoActual != objetoDetectado)
        {
            objetoActual = objetoDetectado;
            tiempoSobreObjeto = 0f;
        }

        // Contabilizar tiempo sobre el objeto
        tiempoSobreObjeto += Time.deltaTime;

        // Comprobar si ya podemos agarrarlo
        if (tiempoSobreObjeto >= tiempoParaAgarrar)
        {
            AgarrarObjeto(objetoActual);
        }
    }

    void AgarrarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        objetoEnMano = objeto;
        objetoAgarrado = true;

        // Guardar posición relativa al cursor
        offsetObjeto =
            objeto.transform.position - transform.position;

        // Guardar padre original
        padreOriginal = objeto.transform.parent;

        // Si tiene Rigidbody, desactivar su física
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

        // Restaurar padre original
        objetoEnMano.transform.SetParent(padreOriginal, true);

        // Restaurar física
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
    }
}
