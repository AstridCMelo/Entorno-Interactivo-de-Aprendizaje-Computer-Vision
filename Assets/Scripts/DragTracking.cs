using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class DragTracking : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public UDPReceive udpReceive;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        string data = udpReceive.dataLandmarks;

        if (string.IsNullOrEmpty(data))
        {

        }
        else
        {
            print(data);
            data = data.Remove(0, 1);
            data = data.Remove(data.Length - 1, 1);
            print(data);

            string[] coordinates = data.Split(',');

            //Se normaliza a coordenadas de panatalla

            float x0 = Mathf.Lerp(-25, 30, float.Parse(coordinates[0]));
            float y0 = Mathf.Lerp(-15, 20,(1 - float.Parse(coordinates[1])));
            float y9 = Mathf.Lerp(-15, 20, (1 - float.Parse(coordinates[4])));

            //Se promedia el y de los landmarks 9 y 0 para hallar el centro de la palma,
            //y de este ubicar el cursor

            float center = (y9 + y0) / 2;

            transform.localPosition = new Vector3(x0, center, transform.localPosition.z);
            Debug.Log(x0);
            Debug.Log(center);

        }
    }
}
