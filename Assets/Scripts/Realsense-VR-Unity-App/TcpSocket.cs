using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class TcpSocket
{
    private string ipAddress;
    private int port;
    private static Socket socket, clientSocket;
    private byte[] buffer = new byte[1024 * 1024];//[640 * 480 * 7]; //new byte[700*1024];//512
    private long counter = 0;
    public Texture2D tex;

    private int backlog = 50;

    public delegate void OnMessageReceived(byte[] message/*, long counter*/);
    public event OnMessageReceived MessageReceived;

    public TcpSocket(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        #region added for server emulation
        socket.Bind(new IPEndPoint(IPAddress.Any, port));
        Debug.Log("Bound socket to port " + port);
        socket.Listen(backlog);
        Debug.Log("Started listening...");
        Accept();
        #endregion
    }

    #region MethodsForServerEmulation
    private void AcceptedCallback(IAsyncResult result)
    {
        Console.WriteLine("Accept callback called... ");
        clientSocket = socket.EndAccept(result);
        if (socket.Connected) { Console.WriteLine("A client has connected... "); }
        clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, clientSocket);

        Accept();
    }
    public void Accept()
    {
        socket.BeginAccept(AcceptedCallback, null);
        Debug.Log("Beginning accept... ");
    }
    #endregion

    public IAsyncResult Connect()
    {
        return socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectCallback, null);
    }

    private void ConnectCallback(IAsyncResult result)
    {
        if (socket.Connected)
        {
            Debug.Log("Connected to server!");
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, null);
        }
    }

    private void ReceivedCallback(IAsyncResult result)
    {
        clientSocket = result.AsyncState as Socket;
        //Debug.Log("Entered Receive callback...");
        int bufferLength = socket.EndReceive(result);
        if (bufferLength > 0)
        {
            //counter++;
            //string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            /*   Mat img2 = CvInvoke.Imread("horse.jpg", ImreadModes.Unchanged);
               Debug.Log("Evo je: " + img2.Height);
               CvInvoke.Imwrite("rocco2.jpg", img2);*/
            //   Mat img2 = new Mat(640, 480, DepthType.Cv8U, 4);
            // CvInvoke.Imdecode(buffer, ImreadModes.Unchanged, img2);

            //Texture2D tex = new Texture2D(640, 480, TextureFormat.ASTC_RGBA_8x8, false);
            // tex.LoadImage(buffer);
            //   CvInvoke.Imwrite("rocco2.png", img2);
            //ImageViewer viewer = new ImageViewer(); //create an image viewer
            // viewer.Image = img2;
            //   viewer.ShowDialog(); //show the image viewer
            //byte[] message = buffer;
            byte[] message = new byte[bufferLength];
            // buffer.CopyTo(message, 0, bufferLength);
            Array.Copy(buffer, 0, message, 0, bufferLength);
            //  string message = buffer.ToString();
            // string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            //Debug.Log("Poruka" + message + "velicina poruke" + bufferLength);
            //string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

            if (MessageReceived != null)
            {
                MessageReceived(message/*, counter*/);
            }

            // Handle packet
            Array.Clear(buffer, 0, buffer.Length);
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, clientSocket);
        }
        else { Debug.Log("Nothing received from socket"); }

    }

    public static void CloseSocket()
    {
        if (socket != null)
        {
            socket.Close();
        }
        if (clientSocket != null)
        {
            socket.Close();
        }
    }
}
