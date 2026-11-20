
using UnityEngine;
using System.Collections;

public class FloatingUI : MonoBehaviour
{
    [Header("Movimiento")]
    public float distancia = 18f;
    public float duracionMovimiento = 2.5f;

    [Header("Zoom")]
    public float escalaMaxima = 1.12f;
    public float duracionZoom = 0.8f;

    [Header("Pausa entre movimientos")]
    public float pausaMinima = 0.2f;
    public float pausaMaxima = 0.8f;

    private RectTransform rectTransform;
    private Vector2 posicionInicial;
    private Vector3 escalaInicial;

    private Coroutine animacion;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("FloatingUI necesita un RectTransform.");
            return;
        }

        posicionInicial = rectTransform.anchoredPosition;
        escalaInicial = rectTransform.localScale;

        animacion = StartCoroutine(AnimacionFlotante());
    }

    void OnDisable()
    {
        if (animacion != null)
        {
            StopCoroutine(animacion);
            animacion = null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = posicionInicial;
            rectTransform.localScale = escalaInicial;
        }
    }

    IEnumerator AnimacionFlotante()
    {
        while (true)
        {
            Vector2 desplazamiento = new Vector2(
                Random.Range(-distancia, distancia),
                Random.Range(-distancia, distancia)
            );

            Vector2 origen = rectTransform.anchoredPosition;
            Vector2 destino = origen + desplazamiento;

            float duracion = Random.Range(
                duracionMovimiento * 0.7f,
                duracionMovimiento * 1.4f
            );

            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;

                float t = Mathf.SmoothStep(
                    0f, 1f, tiempo / duracion
                );

                rectTransform.anchoredPosition =
                    Vector2.Lerp(origen, destino, t);

                yield return null;
            }

            if (Random.value > 0.5f)
            {
                yield return StartCoroutine(HacerZoom());
            }

            yield return new WaitForSeconds(
                Random.Range(pausaMinima, pausaMaxima)
            );
        }
    }

    IEnumerator HacerZoom()
    {
        Vector3 destino = escalaInicial * escalaMaxima;

        yield return Escalar(
            rectTransform.localScale,
            destino,
            duracionZoom
        );

        yield return Escalar(
            destino,
            escalaInicial,
            duracionZoom
        );
    }

    IEnumerator Escalar(Vector3 origen, Vector3 destino, float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f, tiempo / duracion
            );

            rectTransform.localScale =
                Vector3.Lerp(origen, destino, t);

            yield return null;
        }

        rectTransform.localScale = destino;
    }
}
