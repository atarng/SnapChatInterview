#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"

#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>

using namespace std;
using namespace cv;

int g_cameraWidth, g_cameraHeight;
CascadeClassifier _faceCascade;
String _windowName = "UnityOpenCV (Test)";

struct Circle
{
    Circle(int x, int y, int radius) : X(x), Y(y), Radius(radius) {}
    int X, Y, Radius;
};

extern "C" void __declspec(dllexport) __stdcall  Init(int cameraWidth, int cameraHeight)
{
    g_cameraWidth = cameraWidth;  //_capture.get(CAP_PROP_FRAME_WIDTH);
    g_cameraHeight = cameraHeight; //_capture.get(CAP_PROP_FRAME_HEIGHT);
}

extern "C" __declspec(dllexport) int __stdcall  LoadCascade(const char* str)
{
    // Load LBP face cascade.
    if (!_faceCascade.load(str))//  "Cascades/lbpcascade_frontalface.xml"))
        return -1;

    return 0;
}

//__stdcall 
extern "C" __declspec(dllexport) char* ocv_get_image(char* pixels_as_char)
{
    Mat src_frame(g_cameraHeight, g_cameraWidth, CV_8UC4, pixels_as_char);
    Mat cv__frame(g_cameraHeight, g_cameraWidth, CV_8UC3, Scalar(0, 0, 0));

    cvtColor(src_frame, cv__frame, CV_RGBA2BGR, 4);
    threshold(cv__frame, cv__frame, 128, 128, THRESH_BINARY);
    cvtColor(cv__frame, cv__frame, CV_BGR2RGBA, 4);

    char* result;
    result = new char[cv__frame.cols*cv__frame.rows * 4];
    memcpy(result, cv__frame.data, cv__frame.cols*cv__frame.rows * 4);

    return result;
}


extern "C"  __declspec(dllexport) char* __stdcall DetectAndDrawFace(
    char* pixels_as_char, Circle* outFaces, int maxOutFacesCount, int& outDetectedFacesCount
){
    Mat src_frame(g_cameraHeight, g_cameraWidth, CV_8UC4, pixels_as_char);
    Mat cv__frame(g_cameraHeight, g_cameraWidth, CV_8UC3, Scalar(0, 0, 0));

    // src -> cv
    cvtColor(src_frame, cv__frame, CV_RGBA2BGR, 4);


    // invert Height
    flip(cv__frame, cv__frame, 0);

    std::vector<Rect> faces;
    // Convert the frame to grayscale for cascade detection.
    // cv --> grayscale
    Mat grayscaleFrame;
    cvtColor(cv__frame, grayscaleFrame, COLOR_BGR2GRAY);

    // Scale down for better performance.
    //Mat resizedGray;
    //resize(grayscaleFrame, resizedGray, Size(frame.cols / _scale, frame.rows / _scale));
    //equalizeHist(resizedGray, resizedGray);

    // Detect faces.
    _faceCascade.detectMultiScale(grayscaleFrame, faces);

    // Draw faces.
    for (size_t i = 0; i < faces.size(); i++)
    {
        //_scale *
        Point center((faces[i].x + faces[i].width / 2), (faces[i].y + faces[i].height / 2));
        ellipse(cv__frame, center, //cv__frame, center,
            Size(faces[i].width / 2, faces[i].height / 2),
            0, 0, 360,
            Scalar(0, 0, 255),
            4, 8, 0);

        // Send to application.
        outFaces[i] = Circle(faces[i].x, faces[i].y, faces[i].width / 2);
        outDetectedFacesCount++;
        if (outDetectedFacesCount == maxOutFacesCount)
            break;
    }

    //imshow(_windowName, cv__frame);

    // reverseFlip
    flip(cv__frame, cv__frame, 0);
    // convert back to RGBA
    cvtColor(cv__frame, cv__frame, CV_BGR2RGBA, 4);

    char* result;
    result = new char[cv__frame.cols*cv__frame.rows * 4];
    memcpy(result, cv__frame.data, cv__frame.cols*cv__frame.rows * 4);

    return result;
}