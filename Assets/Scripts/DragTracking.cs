
using UnityEngine;
using System.Globalization;

public class DragTracking : MonoBehaviour
{
    public UDPReceive udpReceive;

    [Header("Posición en profundidad")]
    public float zLejano = 0f;
    public float zCercano = -5f;

    [Header("Suavizado")]
    public float suavizado = 8f;

    private Vector3 posicionObjetivo;

    void Update()
    {
        if (udpReceive == null)
            return;

        string data = udpReceive.dataLandmarks;

        if (string.IsNullOrEmpty(data))
            return;

        data = data.Trim('[', ']');

        string[] coordinates = data.Split(',');

        if (coordinates.Length < 5)
            return;

        float x, y0, y9;

        if (!float.TryParse(coordinates[0], NumberStyles.Float,
            CultureInfo.InvariantCulture, out x))
            return;

        if (!float.TryParse(coordinates[1], NumberStyles.Float,
            CultureInfo.InvariantCulture, out y0))
            return;

        if (!float.TryParse(coordinates[4], NumberStyles.Float,
            CultureInfo.InvariantCulture, out y9))
            return;

        // Posición horizontal
        float x0 = Mathf.Round(-25f + (x * 55f));

        // Posición vertical
        float vertical0 = Mathf.Round(-15f + ((1f - y0) * 35f));
        float vertical9 = Mathf.Round(-15f + ((1f - y9) * 35f));

        float center = Mathf.Round((vertical0 + vertical9) / 2f);

        // Profundidad
        float depth = Mathf.Clamp01(udpReceive.dataDepth);
        float z = Mathf.Round(Mathf.Lerp(zLejano, zCercano, depth));

        // Posición objetivo
        posicionObjetivo = new Vector3(
            x0,
            center,
            z
        );

        // Movimiento suavizado
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            posicionObjetivo,
            suavizado * Time.deltaTime
        );
    }
}
