using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class HandTracking : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;

    private bool running = true;

    void Start()
    {
        udpClient = new UdpClient(5052);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("UDP iniciado en puerto 5052");
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);

                string message = Encoding.UTF8.GetString(data);

                Debug.Log("Recibido: " + message);
            }
            catch
            {
                if (running)
                {
                    Debug.LogWarning("Error recibiendo datos UDP");
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        running = false;

        if (udpClient != null)
        {
            udpClient.Close();
        }

        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
    }
}