using System;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using UnityEngine;

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
    private byte[] recievedImage;
    private string[] imageName = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
    private string currentName = "image0.png";
    private string imagesPath;
    private bool doOnce = false;
    // Use this for initialization
    void Awake()
    {
        clientSocket = new TcpSocket("127.0.0.1", 54000);
        clientSocket.MessageReceived += ClientSocket_MessageReceived;
        imagesPath = Application.dataPath + "/Slike/";
        /*
        // Putting the character localPosition in a vector so we can manipulate its localPosition later on
        playerCharacter = GameObject.Find("PlayerCharacter").transform;
        characterVector = playerCharacter.position;
        characterRotation = playerCharacter.rotation.eulerAngles;

        // Initializing vectors that will be changed through coordinates we'll be receiving
        #region gameobjectVectors initialization
        gameobjectVectors[0,0] = Vector3.zero;    //0 - head
        gameobjectVectors[1, 0] = Vector3.zero;     //1 - shoulderSpine
        gameobjectVectors[2, 0] = Vector3.zero;     //2 - leftShoulder
        gameobjectVectors[3, 0] = Vector3.zero;     //3 - leftElbow
        gameobjectVectors[4, 0] = Vector3.zero;     //4 - leftHand
        gameobjectVectors[5, 0] = Vector3.zero;     //5 - rightShoulder
        gameobjectVectors[6, 0] = Vector3.zero;     //6 - rightElbow
        gameobjectVectors[7, 0] = Vector3.zero;     //7 - rightHand
        gameobjectVectors[8, 0] = Vector3.zero;     //8 - midSpine
        gameobjectVectors[9,0] = Vector3.zero;     //9 - baseSpine
        gameobjectVectors[10, 0] = Vector3.zero;     //10 - leftHip
        gameobjectVectors[11, 0] = Vector3.zero;     //11 - leftKnee
        gameobjectVectors[12, 0] = Vector3.zero;     //12 - leftFoot
        gameobjectVectors[13, 0] = Vector3.zero;     //13 - rightHip
        gameobjectVectors[14, 0] = Vector3.zero;     //14 - rightKnee
        gameobjectVectors[15, 0] = Vector3.zero;     //15 - rightFoot
        gameobjectVectors[16, 0] = Vector3.zero;     //16 - leftWrist
        gameobjectVectors[17, 0] = Vector3.zero;     //17 - rightWrist
        gameobjectVectors[18, 0] = Vector3.zero;     //18 - neck

        Transform[] bodyParts = new Transform[nBodyparts];
        currentRotation = new Quaternion[nBodyparts];
        initialPositions = new Vector3[nBodyparts];
        initialRotations = new Quaternion[nBodyparts];
        boneRotation = new Quaternion[nBodyparts];

        for (int i = 0; i < joints.Length-1; ++i)
        {
            initialPositions[i] = joints[i].position;
            initialRotations[i] = joints[i].rotation;
            currentRotation[i] = joints[i].rotation;
        }

        #endregion

        // For using VR as Z buffer offset

        //startingPosX = characterVector.x;
        //startingPosZ = characterVector.z;
        */
    }

    private void Start()
    {
        _texture = new Texture2D(256, 256, Format, false);
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
    private int tempCnt = 0, messCnt = 0, nameCnt = 0;
    private void ClientSocket_MessageReceived(byte[] message, long counter)
    {
        //Debug.Log("Velicina: " + System.Text.ASCIIEncoding.Unicode.GetByteCount(message));
        cnt++;
        //Debug.Log(cnt);
        call = true;
        // Encode texture into PNG
        // if (!doOnce)
        // {
        if(!newTexReady)
        {
            recievedImage = message;
        }

        UnityEngine.Debug.Log(recievedImage.Length + " " + message.Length);
        currentName = "image" + imageName[nameCnt] + ".png";
        if (File.Exists(imagesPath + "currentName"))
        {
            File.Delete(imagesPath + "currentName");
        }
       // File.WriteAllBytes("Assets/Slike/" + currentName, recievedImage);

        StreamWriter streamWriter = new StreamWriter("Assets/Slike/" + currentName);
        streamWriter.BaseStream.Write(recievedImage, 0, recievedImage.Length);
        streamWriter.Close();

        if (nameCnt == 9)
        {  
            nameCnt = 0;
        }
        else
        {
            nameCnt++;
        }

        newTexReady = true;
        tempCnt++;

    }

    // Update is called once per frame

    void Update()
    {

        if (newTexReady)
        {
            StreamReader streamReader = new StreamReader("Assets/Slike/" + currentName);    
            var bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                var buffer = new byte[1024 * 1024 * 2];
                var bytesRead = default(int);
                while ((bytesRead = streamReader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
            }
            streamReader.Close();
            _texture.LoadImage(bytes);
            //_texture.LoadImage(System.IO.File.ReadAllBytes("Assets/Slike/" + currentName));
            _texture.Apply();
         
            Array.Clear(recievedImage, 0, 0);

            newTexReady = false;
        }
        messCnt++;
        //    byte[] bytes = _texture.EncodeToPNG();
        //   File.WriteAllBytes(@"D:\Unity\Project\Realsense test 2\Assets\Slike\image" + cnt + ".png", bytes);

        // For testing purposes, also write to a file in the project folder
        // File.WriteAllBytes("D:/Unity/Project/Realsense test 2/Assets", recievedImage);

        /* head.localPosition = gameobjectVectors[0,0];
         ShoulderSpine.localPosition = gameobjectVectors[1, 0];
         LeftShoulder.localPosition = gameobjectVectors[2, 0];
         LeftElbow.localPosition = gameobjectVectors[3, 0];
         LeftHand.localPosition = gameobjectVectors[4, 0];
         RightShoulder.localPosition = gameobjectVectors[5, 0];
         RightElbow.localPosition = gameobjectVectors[6, 0];
         RightHand.localPosition = gameobjectVectors[7, 0];
         MidSpine.localPosition = gameobjectVectors[8, 0];
         BaseSpine.localPosition = gameobjectVectors[9, 0];
         LeftHip.localPosition = gameobjectVectors[10, 0];
         LeftKnee.localPosition = gameobjectVectors[11, 0];
         LeftFoot.localPosition = gameobjectVectors[12, 0];
         RightHip.localPosition = gameobjectVectors[13, 0];
         RightKnee.localPosition = gameobjectVectors[14, 0];
         RightFoot.localPosition = gameobjectVectors[15, 0];
         LeftWrist.localPosition = gameobjectVectors[16, 0];
         RightWrist.localPosition = gameobjectVectors[17, 0];
         Neck.localPosition = gameobjectVectors[18, 0];*/

        // Move the character where the cubes are at
        /*characterVector.x = BaseSpine.position.x;
        characterVector.z = BaseSpine.position.z;*/

        /*  Quaternion[] jointRotation = new Quaternion[nBodyparts];

          for (int i = 0; i < gameobjectVectors.GetLength(0) - 1; i++)
          {
              joints[i].localPosition = gameobjectVectors[i, 0];
              //Debug.Log(gameobjectVectors[i, 1] + " " +gameobjectVectors[i,2]);
              // kod za rotaciju
              Vector3 jointUp = new Vector3(
                          gameobjectVectors[i, 1].x,
                          gameobjectVectors[i, 1].y,
                          gameobjectVectors[i, 1].z);

              Vector3 jointForward = new Vector3(
                  gameobjectVectors[i, 2].x,
                  gameobjectVectors[i, 2].y,
                  gameobjectVectors[i, 2].z);

              jointRotation[i] = Quaternion.LookRotation(jointForward, jointUp);
              Debug.Log("i: " + i + "x: " + jointRotation[i].x + "y: " + jointRotation[i].y + "z: " + jointRotation[i].z);

              boneRotation[i] = jointRotation[i];
              joints[i].transform.rotation = currentRotation[i] = Quaternion.Slerp(currentRotation[i], boneRotation[i] * initialRotations[i], Time.deltaTime * RotationDamping);
          }
          characterVector.x = joints[9].position.x;
          characterVector.z = joints[9].position.z;

          // skeletonJoint.transform.rotation =Quaternion.LookRotation(jointForward, jointUp);
          // playerCharacter.transform.rotation=  Quaternion.LookRotation(jointForward, jointUp);
          //kraj

          //  characterRotation.y = BaseSpine.rotation.eulerAngles.y;
          //    characterRotation.z = BaseSpine.rotation.eulerAngles.z;
          //Debug.Log(characterVector.x +"   Z:" +characterVector.z);
          Debug.Log("SPine: " + BaseSpine.rotation.eulerAngles.x + "y: " + BaseSpine.rotation.eulerAngles.y + "z: " + BaseSpine.rotation.eulerAngles.z);
          Debug.Log("x: " + characterRotation.x + " y: " + characterRotation.y + "z: " + characterRotation.z);
          //Debug.Log(BaseSpine.rotation);
          playerCharacter.position = characterVector;
          */
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

           /*  x0 = float.Parse(wholeMessage[i][1]);
             x0 = (x0/divisorX) + offsetX;
             y0 = float.Parse(wholeMessage[i][2]);
             y0 = (y0/divisorY) + offsetY;
             z0 = float.Parse(wholeMessage[i][3]);
             z0 = -(z0/divisorZ) + offsetZ;

             x1 = float.Parse(wholeMessage[i][4]);          
             y1 = float.Parse(wholeMessage[i][5]);
             z1 = float.Parse(wholeMessage[i][6]);

             x2 = float.Parse(wholeMessage[i][7]);
             y2 = float.Parse(wholeMessage[i][8]);
             z2 = float.Parse(wholeMessage[i][9]);

             //Debug.Log(wholeMessage[i][0] + ";" + x + ";" + y + ";" + z + ";");
            // Debug.Log(perBodyparts.Length);

             gameobjectVectors[i, 0].x = x0;
             gameobjectVectors[i, 0].y = y0;
             gameobjectVectors[i, 0].z = z0;

             gameobjectVectors[i, 1].x = x1;
             gameobjectVectors[i, 1].y = y1;
             gameobjectVectors[i, 1].z = z1;

             gameobjectVectors[i, 2].x = x2;
             gameobjectVectors[i, 2].y = y2;
             gameobjectVectors[i, 2].z = z2;*/

         }
    }

    private void OnApplicationQuit()
    {
        /*string path = Application.dataPath + "/Slike/12.png";
        File.Delete(path);*/
    }
}
