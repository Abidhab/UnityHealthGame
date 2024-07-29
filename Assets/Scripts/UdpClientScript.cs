using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class UdpClientScript : MonoBehaviour
{
    public static UdpClientScript instance;

    public string[] buttonIPs; // IPs for the buttons
    public int[] buttonPorts; // Ports for the buttons
    public int listenPort = 54321;

    private UdpClient udpClient;
    private Thread receiveThread;
    private int lastSentButtonIndex = -1; // Variable to keep track of the last sent button index

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        udpClient = new UdpClient();
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        udpClient.Close();
    }

    public void SendMessageToDevice(int buttonIndex)
    {
        try
        {
            if (buttonIndex >= 0 && buttonIndex < buttonIPs.Length && buttonIndex < buttonPorts.Length)
            {
                string message = buttonIndex.ToString();
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, buttonIPs[buttonIndex], buttonPorts[buttonIndex]);
                Debug.Log($"Message sent to {buttonIPs[buttonIndex]}:{buttonPorts[buttonIndex]} - {message}");

                lastSentButtonIndex = buttonIndex; // Update the last sent button index
            }
            else
            {
                Debug.LogError("Invalid button index.");
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    private void ReceiveData()
    {
        UdpClient udpServer = new UdpClient(listenPort);

        while (true)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpServer.Receive(ref endPoint);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("Message received: " + message);

                // Enqueue message handling on the main thread
                MainThreadDispatcher.Instance.Enqueue(() => HandleReceivedMessage(message, endPoint));
            }
            catch (SocketException e)
            {
                Debug.LogError("SocketException in ReceiveData: " + e.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in ReceiveData: " + ex.ToString());
            }
        }
    }

    private void HandleReceivedMessage(string message, IPEndPoint endPoint)
    {
        try
        {
            Debug.Log("Handling message: " + message);
            int receivedButtonIndex;
            if (int.TryParse(message, out receivedButtonIndex))
            {
                if (receivedButtonIndex == lastSentButtonIndex)
                {
                    Debug.Log("Button indices match. Updating score.");
                    UiManager.instance.HandleButtonPress(receivedButtonIndex, true);
                }
                else
                {
                    Debug.LogWarning($"Received button index {receivedButtonIndex} does not match the last sent index {lastSentButtonIndex}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in HandleReceivedMessage: " + ex.Message);
        }
    }

}
