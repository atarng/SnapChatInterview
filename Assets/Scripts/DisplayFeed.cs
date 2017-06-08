using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using UnityEngine;

public class DisplayFeed : MonoBehaviour {
#if UNITY_EDITOR
    const string LIB_TO_LOAD = "OpenCVTest";
#elif UNITY_ANDROID
    const string LIB_TO_LOAD = "native-lib";
#endif

    // Define the structure to be sequential and with the correct byte size (3 ints = 4 bytes * 3 = 12 bytes)
    [StructLayout(LayoutKind.Sequential, Size = 12)]
    public struct CvCircle {
        public int X, Y, Radius;
    }

    //[DllImport("OpenCVTest")]
    //public static extern int ReleaseMemory(IntPtr ptr2);

    //DLL FUNCTION CONNECTIONS
    [DllImport(LIB_TO_LOAD)]
    private static extern void Init(int x, int y);

    [DllImport(LIB_TO_LOAD)]
    private static extern int LoadCascade(string streaming_ass_path);

    [DllImport(LIB_TO_LOAD)]
    private static extern IntPtr ocv_get_image(IntPtr feed);

    [DllImport(LIB_TO_LOAD)]
    private unsafe static extern IntPtr DetectAndDrawFace(IntPtr feed, 
        CvCircle* outFaces, int maxOutFacesCount, ref int outDetectedFacesCount);


    //INPUT PARAMETERS
    public GameObject src_renderGo;
    public GameObject tgt_renderGo;

    Renderer rend;
    byte[] imgdata;
    Texture2D tex;

    private CvCircle[] _faces;
    private WebCamTexture webcamscreen;
    private WebCamDevice[] devices;
#if UNITY_EDITOR
    int resx = 320;
    int resy = 240;
#elif UNITY_ANDROID
    int resx = 320;
    int resy = 240;
#endif

    int camUsed = 1;

    Color32[] pixels;
    GCHandle pixelsHandle;
    bool init = false;

    Quaternion baseRotation = Quaternion.identity;

    IEnumerator GetStreamingAsset(string path) {
        Debug.Log("CascadesPath: " + path);
        string to_write = "";
        if (path.Contains("://")) {
            WWW l_www = new WWW(path);
            yield return l_www;
            if (!string.IsNullOrEmpty(l_www.error)) {
                Debug.LogError("Can't read");
            }
            else {
                Debug.Log("streaming asset: " + l_www.text);
                to_write = l_www.text;
            }
        }
        else {
            to_write = System.IO.File.ReadAllText(path);
        }

        FileInfo f = new FileInfo(Application.persistentDataPath + "\\" + "Cascades/lbpcascade_frontalface.xml");
        f.Directory.Create();
        StreamWriter w;
        if (!f.Exists) {
            w = f.CreateText();
        }
        else {
            f.Delete();
            w = f.CreateText();
        }
        w.WriteLine(to_write);
        w.Close();

/////////////////////////////
        init = true;
        int result = LoadCascade(f.FullName); //path_to_load);
        if (result < 0) {
            if (result == -1) {
                Debug.LogWarningFormat("[{0}] Failed to find cascades definition.", GetType());
            }
            init = false;
        }
///////////////////////////
    }
 
    void Start() {
        devices = WebCamTexture.devices;
        if (devices.Length > 0) {
            webcamscreen = new WebCamTexture(resx, resy);

            if (devices.Length >= 2) {
                webcamscreen.deviceName = devices[camUsed].name;
            }
            webcamscreen.requestedWidth  = resx;
            webcamscreen.requestedHeight = resy;

            imgdata = new byte[resx * resy * 4];

            tex = new Texture2D(resx, resy, TextureFormat.RGBA32, false);
            rend = src_renderGo.GetComponent<Renderer>();
            webcamscreen.Play();

            Init(resx, resy);
///////////////////////////////////////////////////////
            _faces = new CvCircle[_maxFaceDetectCount];
            OpenCVFaceDetection.NormalizedFacePositions = new List<Vector3>();

            string path = "/Cascades/lbpcascade_frontalface.xml";

#if UNITY_EDITOR
            //Application.dataPath + "/StreamingAssets/"
            path =  Application.streamingAssetsPath + path;
#elif UNITY_ANDROID
            path = "jar:file://" + Application.dataPath + "!/assets" + path;
#endif
            StartCoroutine(GetStreamingAsset(path));

            ///////////////////////////////////////////////////

            baseRotation = src_renderGo.transform.rotation;
        }
        else {
            Debug.LogError("[DisplayFeed] No WebCamera Detected!");
        }
    }

    public void ToggleCamera() {
        if (devices.Length >= 2) {
            camUsed = (camUsed + 1) % devices.Length;

            webcamscreen.Stop();
            webcamscreen.deviceName = devices[camUsed].name;
            Debug.Log("Toggle Camera: " + webcamscreen.deviceName);
            webcamscreen.Play();
        }
    }

    private int _maxFaceDetectCount = 5;
    //public static List<Vector3> NormalizedFacePositions { get; private set; }
    void Update() {

        if (devices.Length == 0) return;
        ///////////////////////////////////////////////////

        //Debug.Log("[DisplayFeed] webcam rotation: " + webcamscreen.videoRotationAngle);

        rend = src_renderGo.GetComponent<Renderer>();
        rend.material.mainTexture = webcamscreen;

        pixels = webcamscreen.GetPixels32();
        pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        //ptr2 = pixelsHandle.AddrOfPinnedObject();
        //resx, resy,
        IntPtr addr_of_pinned_pixels = pixelsHandle.AddrOfPinnedObject();
        IntPtr result = ocv_get_image(addr_of_pinned_pixels);

        int detectedFaceCount = 0;
        if (init) {
            unsafe
            {
                fixed (CvCircle* outFaces = _faces) {
                    result = DetectAndDrawFace(addr_of_pinned_pixels, outFaces, _maxFaceDetectCount, ref detectedFaceCount);
                }
            }

            OpenCVFaceDetection.NormalizedFacePositions.Clear();
            for (int i = 0; i < detectedFaceCount; i++) {
                OpenCVFaceDetection.NormalizedFacePositions.Add(
                    // X, Y, R
                    new Vector3( ((float)_faces[i].X) / resx, 1f - (((float)_faces[i].Y) / resy), _faces[i].Radius ));
            }
        }

        //Debug.Log(string.Format("[DisplayFeed] imgData: {0} pixels.Length: {1}", imgdata.Length, pixels.Length));
        //Debug.Log(string.Format("[DisplayFeed] wxh: ({0},{1})", webcamscreen.width, webcamscreen.height));

        Marshal.Copy(result, imgdata, 0, pixels.Length * 4);
        tex.LoadRawTextureData(imgdata);
        tex.Apply();
        rend = tgt_renderGo.GetComponent<Renderer>();
        rend.material.mainTexture = tex;

        //target_renderObject.transform.rotation  = baseRotation * Quaternion.AngleAxis(webcamscreen.videoRotationAngle, Vector3.up);
        tgt_renderGo.transform.rotation = baseRotation * Quaternion.AngleAxis(webcamscreen.videoRotationAngle, Vector3.up);

        // Release memory
        // TODO: Perform Memory Cleanup steps here.
        pixelsHandle.Free();
        result = IntPtr.Zero;
    }
}