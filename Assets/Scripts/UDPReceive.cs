
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class UDPReceive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;

    public int port = 5052;
    public bool startReceiving = true;

    // Dígito clasificado
    public int digit = -1;

    // Landmarks 0 y 9
    public string dataLandmarks = "[]";

    // Profundidad normalizada (0 a 1)
    public float dataDepth = 0f;

    private bool running = true;

    void Start()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("UDP iniciado en puerto " + port);
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);

        IPEndPoint anyIP =
            new IPEndPoint(IPAddress.Any, 0);

        while (running && startReceiving)
        {
            try
            {
                byte[] dataByte =
                    client.Receive(ref anyIP);

                string message =
                    Encoding.UTF8.GetString(dataByte);

                string[] messages = message.Split('|');

                if (messages.Length >= 3)
                {
                    // 1. Recibir clasificación
                    if (int.TryParse(messages[0], out int receivedDigit))
                    {
                        digit = receivedDigit;
                    }

                    // 2. Recibir landmarks
                    dataLandmarks = messages[1];

                    // 3. Recibir profundidad
                    if (float.TryParse(
                        messages[2],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float receivedDepth))
                    {
                        dataDepth = Mathf.Clamp01(receivedDepth);
                    }
                }
                else
                {
                    Debug.LogWarning(
                        "Mensaje UDP incompleto: " + message
                    );
                }
            }
            catch (Exception err)
            {
                if (running)
                {
                    Debug.LogWarning("UDP: " + err.Message);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        running = false;
        startReceiving = false;

        if (client != null)
        {
            client.Close();
        }
    }
}
