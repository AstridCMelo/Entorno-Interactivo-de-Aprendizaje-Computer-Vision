
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;

    public int port = 5052;

    public bool startReceiving = true;

    // Aquí se guardará el dígito recibido
    public int digit = -1;

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

                // Intentar convertir el mensaje a número
                if (int.TryParse(message, out int receivedDigit))
                {
                    digit = receivedDigit;

                    Debug.Log(
                        "Dígito recibido: " + digit
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Mensaje recibido no válido: " + message
                    );
                }
            }
            catch (Exception err)
            {
                if (running)
                {
                    Debug.LogWarning(
                        "UDP: " + err.Message
                    );
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

