
using UnityEngine;
using System.Globalization;

public class DragTracking : MonoBehaviour
{
    public UDPReceive udpReceive;

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

        // Coordenadas normalizadas
        float x0 = -25f + (x * 55f);

        float vertical0 = -15f + ((1f - y0) * 35f);
        float vertical9 = -15f + ((1f - y9) * 35f);

        float center = (vertical0 + vertical9) / 2f;

        Vector3 nuevaPosicion = new Vector3(
            x0,
            center,
            transform.localPosition.z
        );

        transform.localPosition = nuevaPosicion;

        Debug.Log("X original: " + x);
        Debug.Log("Y0 original: " + y0);
        Debug.Log("Y9 original: " + y9);
        Debug.Log("Nueva posición: " + nuevaPosicion);
    }
}
