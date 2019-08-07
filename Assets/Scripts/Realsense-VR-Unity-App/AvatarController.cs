using System;
using System.IO;
using System.Diagnostics;
/*using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Structure;*/
using UnityEngine;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

/* PROBAJ ZA SVAKU SLIKU NAPRAVIT NOVI FILE SA EMGUCV*/
public class AvatarController : MonoBehaviour
{
    private const int nBodyparts = 19;
    private TcpSocket clientSocket;
    #region gameObjects
    public Transform head;
    public Transform ShoulderSpine;
    public Transform LeftShoulder;
    public Transform LeftElbow;
    public Transform LeftHand;
    public Transform RightShoulder;
    public Transform RightElbow;
    public Transform RightHand;
    public Transform MidSpine;
    public Transform BaseSpine;
    public Transform LeftHip;
    public Transform LeftKnee;
    public Transform LeftFoot;
    public Transform RightHip;
    public Transform RightKnee;
    public Transform RightFoot;
    public Transform LeftWrist;
    public Transform RightWrist;
    public Transform Neck;
    public GameObject gameFrame;
    public Texture2D _texture;
    public Texture2D newTexture;
    //COLOR_BGRA2RGBA
    private const TextureFormat Format = TextureFormat.BGRA32;

    float RotationDamping = 30.0f;

    public Transform[] joints;

    #endregion
    public float divisorX = 350f, divisorY = 400f, divisorZ = 300f;
    public float offsetX = 0f, offsetY = 2.3f, offsetZ = 8f;
    Vector3[,] gameobjectVectors = new Vector3[nBodyparts, 3];
    private Quaternion[] currentRotation;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Quaternion[] boneRotation;

    private Vector3 characterVector;
    private Vector3 characterRotation;
    private Transform playerCharacter;

    private int cnt = 0;
    private byte[] recievedImage, imageBackup;
    private string[] imageName = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    private string currentName = "image0.png";
    private string imagesPath;
    private Mat mat;
    MemoryStream materialStream;
    private Image<Gray, byte> depthImage;
    // Use this for init ialization
    void Awake()
    {
        //clientSocket = new TcpSocket("127.0.0.1", 54000);
        clientSocket = new TcpSocket("192.168.1.6", 54000);
        clientSocket.MessageReceived += ClientSocket_MessageReceived;
        imagesPath = Application.dataPath + "/Slike/";
        //   materialStream = new MemoryStream();
    }

    private void Start()
    {
        _texture = new Texture2D(640, 480, Format, false);
        //   gameFrame.GetComponent<Renderer>().material.SetTexture("_MainTex", _texture);
        gameFrame.GetComponent<Renderer>().material.mainTexture = _texture;
        // newTexture = new Texture2D(640, 480, Format, false);
        // _texture.Apply();

        //KOD ZA POKRETANJE JOINT TRACKINGA
        /*   try
           {
               Process myProcess = new Process();
               myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
               string path = "C:\\Users\\Brian\\Desktop\\testFile.bat";
               myProcess.StartInfo.Arguments = "/c" + path;
               myProcess.EnableRaisingEvents = true;
               myProcess.Start();
               myProcess.WaitForExit();
               int ExitCode = myProcess.ExitCode;
               //print(ExitCode);
           }
           catch (Exception e)
           {
               print(e);
           }*/
    }

    private bool call = false, end = false, newTexReady = false, texReady = false;
    private bool doOnce = false, doOnce2 = false;
    private int tempCnt = 0, messCnt = 0, nameCnt = 0;
    private List<byte> messageBuffer = new List<byte>();
    byte[] EndOfMessage = System.Text.Encoding.ASCII.GetBytes("image_end");

    private void ClientSocket_MessageReceived(byte[] message/*, long counter*/)
    {
        //Debug.Log("Velicina: " + System.Text.ASCIIEncoding.Unicode.GetByteCount(message));
        //cnt++;
        //Debug.Log(cnt);
        //call = true;
        // Encode texture into PNG
        // if (!doOnce)
        // {
        Debug.Log("1");
        if (!IsEndOfMessage(message, EndOfMessage))
        {
            messageBuffer.AddRange(message);
            Debug.Log("Nije cijela poruka");
            /* StreamWriter streamWriter2 = new StreamWriter("Assets/networkERRR" + cnt + ".txt");
             streamWriter2.BaseStream.Write(message, 0, message.Length);
             streamWriter2.Close();*/
            return;
            //continue reading from socket and adding to ReceivedBytes
        }
        else
        {
            Debug.Log("Kraj");
            message = RemoveEndOfMessage(message, EndOfMessage);
            messageBuffer.AddRange(message);
            message = messageBuffer.ToArray();
            Debug.Log("Velicina: " + messageBuffer.Capacity);
            messageBuffer.Clear();
        }
        Debug.Log("2");
        if (!newTexReady)
        {
            /*
            imageBackup = new byte[2150400];//[1024 * 1024];
            StreamWriter streamWriter = new StreamWriter("Assets/Slike/image1.png");
            if (imageBackup.Length < message.Length)
            {
                UnityEngine.Debug.Log("Manji je");
            }
            else
            {
                message.CopyTo(imageBackup, 0);    
                streamWriter.BaseStream.Write(imageBackup, 0, message.Length);
            }*/

            Debug.Log("3");
            //recievedImage = message;
           
            
            Debug.Log("4");
            try
            {
                StreamWriter streamWriter = new StreamWriter("Assets/Slike/image1.png");
                recievedImage = new byte[message.Length];//new byte[2150400]
                message.CopyTo(recievedImage, 0);
                streamWriter.BaseStream.Write(recievedImage, 0, message.Length);
                streamWriter.Close();
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e);
            }
            Debug.Log("5");

            //UnityEngine.Debug.Log("message length: " + message.Length);

            newTexReady = true;
            //     imageBackup = message;
        }
        Debug.Log("6");
        /*if(!doOnce)
        {
            StreamWriter streamWriter2 = new StreamWriter("Assets/networkX" + cnt + ".txt");
            streamWriter2.BaseStream.Write(message, 0, message.Length);
            streamWriter2.Close();*/

        /* using (var stream = new FileStream("Assets/networkZ.txt", FileMode.Append))
         {
             //stream.Write(recievedImage, 0, recievedImage.Length);
             stream.Write(recievedImage, 0, message.Length);
         }*/
        //doOnce = true;
        // }
        //UnityEngine.Debug.Log(recievedImage.Length + " " + message.Length);
        /*   currentName = "image" + imageName[nameCnt] + ".png";
           if (File.Exists(imagesPath + "currentName"))
           {
               File.Delete(imagesPath + "currentName");
           }
           File.WriteAllBytes("Assets/Slike/" + currentName, recievedImage);
           //StreamWriter streamWriter = new StreamWriter("Assets/Slike/" + currentName);
           */
        /* StreamWriter streamWriter = new StreamWriter("Assets/Slike/image1.png");
         streamWriter.BaseStream.Write(recievedImage, 0, recievedImage.Length);
         streamWriter.Close();*/

        /*if (nameCnt == 9)
        {
            nameCnt = 0;
        }
        else
        {
            nameCnt++;
        }

        tempCnt++;*/
    }

    private bool IsEndOfMessage(byte[] MessageToCheck, byte[] EndOfMessage)
    {
        /*for (int i = 0; i < EndOfMessage.Length; i++)
        {
            if (MessageToCheck[MessageToCheck.Length - (EndOfMessage.Length + i)] != EndOfMessage[i])
                return false;
        }*/
        for (int i = 0; i < EndOfMessage.Length; i++)
        {
            if (MessageToCheck[MessageToCheck.Length - EndOfMessage.Length + i] != EndOfMessage[i])
            {
                return false;
            }
        }
        return true;
    }

    private byte[] RemoveEndOfMessage(byte[] MessageToClear, byte[] EndOfMessage)
    {
        byte[] Return = new byte[MessageToClear.Length - EndOfMessage.Length];
        Array.Copy(MessageToClear, Return, Return.Length);

        return Return;
    }

    // Update is called once per frame

    void Update()
    {

        if (newTexReady)
        {
            // StreamReader streamReader = new StreamReader("Assets/Slike/" + currentName);
            //StreamReader streamReader = new StreamReader("Assets/Slike/image1.png");
            //imageBackup = recievedImage;
            var bytes = default(byte[]);
            //    using (StreamReader streamReader = new StreamReader("Assets/Slike/" + currentName))
            using (StreamReader streamReader = new StreamReader("Assets/Slike/image1.png"))
            {
                var memstream = new MemoryStream();
                var buffer = new byte[1024 * 1024 * 2];
                var bytesRead = default(int);
                while ((bytesRead = streamReader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
            }
            _texture.LoadImage(bytes);
            //  _texture.LoadRawTextureData(imageBackup);
            //  _texture.LoadRawTextureData(b);
            //_texture.LoadImage(System.IO.File.ReadAllBytes("Assets/Slike/" + currentName));
            _texture.Apply();

            Array.Clear(recievedImage, 0, 0);
            //Array.Clear(imageBackup, 0, 0);

            newTexReady = false;
        }
        messCnt++;

    }

    private void ReformatMessage(string message)
    {
        string[] perBodyparts = message.Split(' ');
        UnityEngine.Debug.Log(perBodyparts.Length);
        /* float x0, y0, z0;
         float x1, y1, z1;
         float x2, y2, z2;
         string[] perBodyparts = message.Split(' ');*/
        string[][] wholeMessage = new string[perBodyparts.Length - 1][];

        // Astra.MaskedColorFrame frame = new Astra.MaskedColorFrame();
        for (int i = 0; i < perBodyparts.Length - 1; i++)   // Splitting message into parts, and changing vector coordinates
        {
            wholeMessage[i] = perBodyparts[i].Split(';');
        }
    }

    private void OnApplicationQuit()
    {
        /*string path = Application.dataPath + "/Slike/12.png";
        File.Delete(path);*/
    }

    public Image byteArrayToImage(byte[] byteArrayIn)
    {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        Image returnImage = Image.FromStream(ms);
        return returnImage;
    }

    public byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        MemoryStream ms = new MemoryStream();
        imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }
}
