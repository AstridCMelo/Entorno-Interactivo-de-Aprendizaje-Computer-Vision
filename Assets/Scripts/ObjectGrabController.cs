
using UnityEngine;

public class ObjectGrabController : MonoBehaviour
{
    [Header("Referencias")]
    public UDPReceive udpReceive;
    public Camera camara;

    [Header("Configuración")]
    public float tiempoParaAgarrar = 3f;
    public float distanciaRaycast = 100f;

    [Header("Conexión física")]
    public float tiempoParaConectar = 5f;
    public float radioDeteccionConexion = 10f;

    [Header("Estado")]
    public bool objetoAgarrado = false;

    private InteractableObject objetoActual;
    private InteractableObject objetoEnMano;
    private InteractableObject objetoDebajo;

    private float tiempoSobreObjeto = 0f;
    private float tiempoSobreConexion = 0f;

    private Vector3 offsetObjeto;
    private Vector3 posicionObjetivoObjeto;

    private Rigidbody rbObjeto;
    private bool estadoKinematicAnterior;

    private Transform padreOriginal;
    private Outline outlineActual;

    [Header("Colores del Outline")]
    public Color colorNormal = Color.white;
    public Color colorConectando = Color.green;

    private Color colorOriginalOutline;




void AlinearObjetos(
    InteractableObject superior,
    InteractableObject inferior)
{
    Collider colliderSuperior =
        superior.GetComponentInChildren<Collider>();

    Collider colliderInferior =
        inferior.GetComponentInChildren<Collider>();

    if (colliderSuperior == null || colliderInferior == null)
    {
        Debug.LogWarning("Falta un Collider para alinear los objetos.");
        return;
    }

    Bounds boundsSuperior = colliderSuperior.bounds;
    Bounds boundsInferior = colliderInferior.bounds;

    // Centrar el objeto superior en X y Z.
    float diferenciaX =
        boundsInferior.center.x - boundsSuperior.center.x;

    float diferenciaZ =
        boundsInferior.center.z - boundsSuperior.center.z;

    // Colocar la base del superior exactamente sobre
    // la parte superior del inferior.
    float diferenciaY =
        boundsInferior.max.y - boundsSuperior.min.y;

    Vector3 desplazamiento = new Vector3(
        diferenciaX,
        diferenciaY,
        diferenciaZ
    );

    superior.transform.position += desplazamiento;

    // Actualizar la física de Unity.
    Physics.SyncTransforms();

    // Actualizar el objetivo del objeto agarrado para que
    // el cursor no lo devuelva a su posición anterior.
    offsetObjeto =
        superior.transform.position - transform.position;

    posicionObjetivoObjeto = superior.transform.position;

    Debug.Log("Objetos alineados correctamente.");
}



    void Update()
    {
        if (udpReceive == null || camara == null)
            return;

        int gesto = udpReceive.digit;

        // Gesto 5: soltar objeto
        if (gesto == 5 && objetoAgarrado)
        {
            SoltarObjeto();
            return;
        }

        // Si ya tenemos un objeto agarrado
        if (objetoAgarrado)
        {
            posicionObjetivoObjeto = transform.position + offsetObjeto;

            // Solo conectar mientras se mantiene el gesto 0
            if (gesto == 0)
                ComprobarConexion();
            else
                ReiniciarConexion();

            return;
        }

        // Gesto 0: buscar objeto
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

    void FixedUpdate()
    {
        if (!objetoAgarrado || objetoEnMano == null)
            return;

        // Mover el objeto mediante el sistema físico de Unity
        if (rbObjeto != null)
        {
            rbObjeto.MovePosition(posicionObjetivoObjeto);
        }
        else
        {
            objetoEnMano.transform.position = posicionObjetivoObjeto;
        }
    }

    // --------------------------------------------------
    // DETECCIÓN DE OBJETOS
    // --------------------------------------------------

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

            if (interactuable == null)
                continue;

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

    // --------------------------------------------------
    // OUTLINE
    // --------------------------------------------------

    void ActualizarOutline(InteractableObject nuevoObjeto)
    {
        if (outlineActual != null)
        {
            outlineActual.enabled = false;
            outlineActual = null;
        }

        if (nuevoObjeto == null)
            return;

        outlineActual = nuevoObjeto.GetComponentInChildren<Outline>();

        if (outlineActual != null)
        {
            outlineActual.enabled = true;
        }


        outlineActual = nuevoObjeto.GetComponent<Outline>();

        if (outlineActual != null)
        {
            colorOriginalOutline = outlineActual.OutlineColor;
            outlineActual.OutlineColor = colorNormal;
            outlineActual.enabled = true;
        }
    }

    // --------------------------------------------------
    // AGARRAR OBJETO
    // --------------------------------------------------

    void AgarrarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        // Al agarrar un objeto conectado, se eliminan
        // sus conexiones físicas para permitir separarlo.
        DesconectarObjeto(objeto);

        objetoEnMano = objeto;
        objetoActual = objeto;

        ActualizarOutline(objeto);

        objetoAgarrado = true;

        offsetObjeto =
            objeto.transform.position - transform.position;

        posicionObjetivoObjeto = objeto.transform.position;

        padreOriginal = objeto.transform.parent;

        rbObjeto = objeto.GetComponent<Rigidbody>();

        if (rbObjeto != null)
        {
            estadoKinematicAnterior = rbObjeto.isKinematic;

            rbObjeto.linearVelocity = Vector3.zero;
            rbObjeto.angularVelocity = Vector3.zero;

            // Mientras está agarrado, el cursor controla
            // su posición.
            rbObjeto.isKinematic = true;
        }

        tiempoSobreObjeto = 0f;

        Debug.Log("Objeto agarrado: " + objeto.name);
    }

    // --------------------------------------------------
    // CONEXIÓN FÍSICA
    // --------------------------------------------------

    void ComprobarConexion()
    {
        if (objetoEnMano == null || rbObjeto == null)
            return;

        Collider[] impactos = Physics.OverlapSphere(
            objetoEnMano.transform.position,
            radioDeteccionConexion
        );

        InteractableObject candidato = null;
        float distanciaCercana = float.MaxValue;

        foreach (Collider impacto in impactos)
        {
            InteractableObject posibleObjeto =
                impacto.GetComponentInParent<InteractableObject>();

            if (posibleObjeto == null || posibleObjeto == objetoEnMano)
                continue;

            Rigidbody rbCandidato =
                posibleObjeto.GetComponent<Rigidbody>();

            // Ambos objetos necesitan Rigidbody.
            if (rbCandidato == null)
                continue;

            // Evitar conectar objetos que ya pertenecen
            // al mismo conjunto físico.
            if (rbCandidato == rbObjeto)
                continue;

            // Buscar un objeto que esté debajo del objeto agarrado.
            if (posibleObjeto.transform.position.y >=
                objetoEnMano.transform.position.y)
                continue;

            float distancia = Vector3.Distance(
                objetoEnMano.transform.position,
                impacto.ClosestPoint(objetoEnMano.transform.position)
            );

            if (distancia < distanciaCercana)
            {
                candidato = posibleObjeto;
                distanciaCercana = distancia;
            }
        }

        if (candidato == null)
        {
            ReiniciarConexion();
            return;
        }

        if (objetoDebajo != candidato)
        {
            objetoDebajo = candidato;
            tiempoSobreConexion = 0f;

            if (outlineActual != null)
                outlineActual.OutlineColor = colorConectando;

            Debug.Log("Preparando conexión con: " + candidato.name);
        }

        tiempoSobreConexion += Time.deltaTime;

        if (tiempoSobreConexion >= tiempoParaConectar)
        {
            ConectarObjetos(objetoEnMano, objetoDebajo);
        }
    }

    void ConectarObjetos(
        InteractableObject superior,
        InteractableObject inferior)
    {
        if (superior == null || inferior == null)
            return;

        Rigidbody rbSuperior = superior.GetComponent<Rigidbody>();
        Rigidbody rbInferior = inferior.GetComponent<Rigidbody>();

        if (rbSuperior == null || rbInferior == null)
        {
            Debug.LogWarning("Los dos objetos necesitan Rigidbody.");
            ReiniciarConexion();
            return;
        }

        AlinearObjetos(superior, inferior);

        // Evitar crear una conexión duplicada.
        FixedJoint[] conexiones =
            superior.GetComponents<FixedJoint>();

        foreach (FixedJoint conexion in conexiones)
        {
            if (conexion.connectedBody == rbInferior)
            {
                Debug.Log("Los objetos ya están conectados.");
                ReiniciarConexion();
                return;
            }
        }

        // Crear una conexión física entre ambos objetos.
        FixedJoint joint = superior.gameObject.AddComponent<FixedJoint>();

        joint.connectedBody = rbInferior;
        joint.enableCollision = true;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log(
            "Conexión física creada: " +
            superior.name + " -> " + inferior.name
        );

        ReiniciarConexion();
    }

    void ReiniciarConexion()
    {
        objetoDebajo = null;
        tiempoSobreConexion = 0f;

        if (outlineActual != null)
            outlineActual.OutlineColor = colorNormal;
    }
    // --------------------------------------------------
    // DESCONECTAR OBJETOS
    // --------------------------------------------------

    void DesconectarObjeto(InteractableObject objeto)
    {
        if (objeto == null)
            return;

        Rigidbody rb = objeto.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        // Buscar todos los FixedJoint de la escena.
        FixedJoint[] conexiones =
            FindObjectsOfType<FixedJoint>();

        foreach (FixedJoint conexion in conexiones)
        {
            Rigidbody rbPropio =
                conexion.GetComponent<Rigidbody>();

            // Eliminar conexiones donde el objeto agarrado
            // sea el cuerpo principal o el cuerpo conectado.
            if (rbPropio == rb || conexion.connectedBody == rb)
            {
                Debug.Log(
                    "Conexión eliminada de: " + objeto.name
                );

                Destroy(conexion);
            }
        }
    }

    // --------------------------------------------------
    // SOLTAR OBJETO
    // --------------------------------------------------

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

        ReiniciarConexion();
        ActualizarOutline(null);
    }

    // --------------------------------------------------
    // REFERENCIA PARA OTROS SCRIPTS
    // --------------------------------------------------

    public SpawnerContainer getObjetoMano()
    {
        if (objetoAgarrado && objetoEnMano != null)
        {
            return objetoEnMano.SOButton;
        }

        return null;
    }
}
