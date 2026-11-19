
using UnityEngine;
using System.Globalization;

public class DragTracking : MonoBehaviour
{
    //public UDPReceive udpReceive;
    UDPReceive udpReceive => UDPReceive.Instance;


    [Header("Sprites del cursor")]
    public SpriteRenderer spriteRenderer;

    public Sprite spriteGesto0;
    public Sprite spriteGesto5;

    private int ultimoGesto = -1;


    [Header("Mensajes")]
public TextAnimationController controladorTexto;

private bool manoDetectada = false;
private bool gesto0Detectado = false;
private bool gesto5Detectado = false;
private bool gestosCompletados = false;

private bool derechaDetectada = false;
private bool izquierdaDetectada = false;

private bool arribaDetectada = false;
private bool abajoDetectada = false;

private bool adelanteDetectado = false;
private bool atrasDetectado = false;

private bool ejercicioHorizontal = false;
private bool ejercicioVertical = false;
private bool ejercicioProfundidad = false;

private bool ejerciciosCompletados;

private bool bienvenidaMostrada = false;

private Vector3 posicionReferencia;
private bool referenciaInicializada = false;

[Header("Detección de movimientos")]
private bool referenciaMovimientoInicializada = false;

private Vector2 referenciaMovimientoXY;
private float referenciaMovimientoZ;

public float umbralMovimientoX = 0.08f;
public float umbralMovimientoY = 0.08f;
public float umbralMovimientoZ = 0.08f;

    [Header("Posicion en profundidad")]
    public float zLejano = 30f;
    public float zCercano = -30f;

    [Header("Suavizado")]
    public float suavizado = 4f;

    [Header("Control de sensibilidad")]
    [Range(0f, 0.1f)]
    public float zonaMuerta = 0.01f;


    [Header("Sensibilidad del cursor")]
    [Range(0.1f, 1f)]
    public float sensibilidadX = 0.5f;

    [Range(0.1f, 1f)]
    public float sensibilidadY = 0.5f;

    [Header("Limites de pantalla")]
    public Camera camara;
    
    [Range(0f, 0.1f)]
    public float margenPantalla = 0.03f;

    private Vector3 posicionObjetivo;

    // Ultima coordenada aceptada
    private Vector2 ultimaEntrada;
    private bool entradaInicializada = false;

    [Header("Validación del centro")]
public float centroX = 0.5f;
public float centroY = 0.5f;

// Qué tan cerca debe estar la mano del centro
public float toleranciaCentro = 0.10f;

private bool manoCentrada = false;
    
private bool mensajeCentroMostrado = false;


    void DetectarPrimeraMano(float x, float y)
    {
        if (!mensajeCentroMostrado)
        {
            mensajeCentroMostrado = true;

            controladorTexto.MostrarMensajeUnaVez(
                "centrar_mano",
                "¡Coloca tu mano en el centro de la pantalla!"
            );
        }

        bool estaEnCentro =
            Mathf.Abs(x - centroX) <= toleranciaCentro &&
            Mathf.Abs(y - centroY) <= toleranciaCentro;

        if (!estaEnCentro)
            return;

        if (manoCentrada)
            return;

        manoCentrada = true;
        manoDetectada = true;

        controladorTexto.MostrarMensajeUnaVez(
            "mano_centrada",
            "¡Muy bien! Ahora abre y cierra tu mano."
        );
    }



void DetectarMovimientos(float x, float y, float z)
{
    if (!gestosCompletados || ejerciciosCompletados)
        return;

    Vector2 posicionActual = new Vector2(x, y);

    // Establecer la primera posición de referencia
    if (!referenciaMovimientoInicializada)
    {
        referenciaMovimientoXY = posicionActual;
        referenciaMovimientoZ = z;
        referenciaMovimientoInicializada = true;
        return;
    }

    float diferenciaX = x - referenciaMovimientoXY.x;
    float diferenciaY = y - referenciaMovimientoXY.y;
    float diferenciaZ = z - referenciaMovimientoZ;

    // HORIZONTAL
    if (!ejercicioHorizontal)
    {
        if (diferenciaX >= umbralMovimientoX)
            derechaDetectada = true;

        if (diferenciaX <= -umbralMovimientoX)
            izquierdaDetectada = true;

        if (derechaDetectada && izquierdaDetectada)
        {
            ejercicioHorizontal = true;

            controladorTexto.MostrarMensajeUnaVez(
                "movimiento_horizontal",
                "¡Eso! Ahora mueve tu mano de arriba a abajo."
            );
        }
    }

    // VERTICAL
    if (!ejercicioVertical)
    {
        if (diferenciaY >= umbralMovimientoY)
            arribaDetectada = true;

        if (diferenciaY <= -umbralMovimientoY)
            abajoDetectada = true;

        if (arribaDetectada && abajoDetectada)
        {
            ejercicioVertical = true;

            controladorTexto.MostrarMensajeUnaVez(
                "movimiento_vertical",
                "¡Excelente! Ahora mueve tu mano hacia delante y atrás."
            );
        }
    }

    // PROFUNDIDAD
    if (!ejercicioProfundidad)
    {
        if (diferenciaZ >= umbralMovimientoZ)
            adelanteDetectado = true;

        if (diferenciaZ <= -umbralMovimientoZ)
            atrasDetectado = true;

        if (adelanteDetectado && atrasDetectado)
        {
            ejercicioProfundidad = true;

            controladorTexto.MostrarMensajeUnaVez(
                "movimiento_profundidad",
                "¡Excelente!"
            );
        }
    }

    // COMPLETAR LOS TRES EJERCICIOS
    if (ejercicioHorizontal &&
        ejercicioVertical &&
        ejercicioProfundidad)
    {
        ejerciciosCompletados = true;

        controladorTexto.MostrarMensajeUnaVez(
            "ejercicios_completados",
            "¡Bien hecho! Ahora, intenta agarrar un objeto."
        );
    }
}


    void Update()
    {        
        if (udpReceive == null)
            return;

        string data = udpReceive.dataLandmarks;

        // Si no hay mano, conservar la ultima posicion
        if (string.IsNullOrEmpty(data))
            return;

        data = data.Trim('[', ']');

        string[] coordinates = data.Split(',');

        if (coordinates.Length < 5)
            return;

        float x, y0, y9;

        if (!float.TryParse(coordinates[0],
            NumberStyles.Float,
            CultureInfo.InvariantCulture, out x))
            return;

        if (!float.TryParse(coordinates[1],
            NumberStyles.Float,
            CultureInfo.InvariantCulture, out y0))
            return;

        if (!float.TryParse(coordinates[4],
            NumberStyles.Float,
            CultureInfo.InvariantCulture, out y9))
            return;

         
        // =====================================
        // 1. NORMALIZAR MOVIMIENTO HORIZONTAL
        // =====================================

        x = 1f - x;

        // Centro vertical de landmarks 0 y 9
        float y = (y0 + y9) / 2f;

        // Validar que la mano esté en el centro
        DetectarPrimeraMano(x, y);

        // Reconocer gestos solamente cuando la mano esté centrada
        if (manoCentrada && spriteRenderer != null)
        {
            ActualizarSprite();
        }

        // =====================================
        // 2. ZONA MUERTA
        // =====================================

        Vector2 entradaActual = new Vector2(x, y);

        if (!entradaInicializada)
        {
            ultimaEntrada = entradaActual;
            entradaInicializada = true;
        }
        else
        {
            float diferencia = Vector2.Distance(
                entradaActual,
                ultimaEntrada
            );

            if (diferencia >= zonaMuerta)
            {
                ultimaEntrada = entradaActual;
            }
        }

        x = ultimaEntrada.x;
        y = ultimaEntrada.y;

        // =====================================
        // 3. CONVERSION A COORDENADAS UNITY
        // =====================================

        // Centro del rango actual
        float centroX = 2.5f;
        float centroY = 2.5f;

        // Movimiento relativo al centro
        float x0 = centroX + ((x - 0.5f) * 55f * sensibilidadX);

        float vertical0 = centroY +
            ((0.5f - y) * 35f * sensibilidadY);

        // =====================================
        // 4. PROFUNDIDAD RELATIVA
        // =====================================

        float depth = Mathf.Clamp01(udpReceive.dataDepth);

        // Punto central de referencia
        float profundidadMedia = 0.5f;

        // Centro del recorrido en Unity
        float zCentro = (zLejano + zCercano) / 2f;


        // Desplazamiento respecto al centro
        float desplazamientoZ = (depth - profundidadMedia) * 10f;

        // Posición final
        float z = Mathf.Clamp(
            zCentro + desplazamientoZ,
            zCercano,
            zLejano
        );

       // Los ejercicios solo comienzan cuando:
        // 1. La mano está centrada.
        // 2. Los dos gestos fueron reconocidos.
        if (manoCentrada && gestosCompletados)
        {
            DetectarMovimientos(x, y, depth);
        }

        // =====================================
        // 5. POSICION OBJETIVO
        // =====================================

        posicionObjetivo = new Vector3(
            x0,
            vertical0,
            z
        );

        // =====================================
        // 6. MOVIMIENTO SUAVIZADO
        // =====================================

        Vector3 posicionSuavizada = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            suavizado * Time.deltaTime
        );

        // =====================================
        // 7. LIMITES DE PANTALLA
        // =====================================

        transform.localPosition = LimitarPantalla(
            posicionSuavizada
        );
    }

    Vector3 LimitarPantalla(Vector3 posicionLocal)
    {
        if (camara == null)
            return posicionLocal;

        // Convertir de coordenadas locales a mundo
        Vector3 posicionMundo = transform.parent != null
            ? transform.parent.TransformPoint(posicionLocal)
            : posicionLocal;

        // Convertir a coordenadas de pantalla
        Vector3 viewport = camara.WorldToViewportPoint(
            posicionMundo
        );

        // No modificar si esta detras de la camara
        if (viewport.z <= 0f)
            return posicionLocal;

        // Limitar los bordes
        viewport.x = Mathf.Clamp(
            viewport.x,
            margenPantalla,
            1f - margenPantalla
        );

        viewport.y = Mathf.Clamp(
            viewport.y,
            margenPantalla,
            1f - margenPantalla
        );

        // Regresar a coordenadas del mundo
        Vector3 posicionLimitada =
            camara.ViewportToWorldPoint(viewport);

        // Regresar a coordenadas locales
        if (transform.parent != null)
        {
            return transform.parent.InverseTransformPoint(
                posicionLimitada
            );
        }

        return posicionLimitada;
    }


    void ActualizarSprite()
{

    if (!manoCentrada)
    return;
    int gesto = udpReceive.digit;



    if (gesto == 0)
    gesto0Detectado = true;

    if (gesto == 5)
        gesto5Detectado = true;

    if (!gestosCompletados &&
        gesto0Detectado &&
        gesto5Detectado)
    {
        gestosCompletados = true;
        referenciaMovimientoInicializada = false;

        derechaDetectada = false;
        izquierdaDetectada = false;

        arribaDetectada = false;
        abajoDetectada = false;

        adelanteDetectado = false;
        atrasDetectado = false;

        controladorTexto.MostrarMensajeUnaVez(
            "gestos_reconocidos",
            "¡Muy bien! Ahora mueve tu mano de izquierda a derecha."
        );
    }



    Debug.Log("Gesto recibido: " + gesto);

    if (gesto == ultimoGesto)
        return;

    ultimoGesto = gesto;

    if (gesto == 0)
    {
        spriteRenderer.sprite = spriteGesto0;
        Debug.Log("Sprite cambiado a gesto 0");
    }
    else if (gesto == 5)
    {
        spriteRenderer.sprite = spriteGesto5;
        Debug.Log("Sprite cambiado a gesto 5");
    }
}
}
