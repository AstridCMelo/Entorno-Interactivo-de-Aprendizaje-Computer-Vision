
using UnityEngine;
using System.Globalization;

public class DragTracking : MonoBehaviour
{
    public UDPReceive udpReceive;

    [Header("Sprites del cursor")]
    public SpriteRenderer spriteRenderer;

    public Sprite spriteGesto0;
    public Sprite spriteGesto5;

    private int ultimoGesto = -1;

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
    

    void Update()
    {

        if (spriteRenderer != null)
        {
            ActualizarSprite();
        }

        
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
    int gesto = udpReceive.digit;

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
