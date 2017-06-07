/*
#include "opencv2/objdetect.hpp"
#include "opencv2/highgui.hpp"
#include "opencv2/imgproc.hpp"

#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>

using namespace std;
using namespace cv;

struct Circle
{
    Circle(int x, int y, int radius) : X(x), Y(y), Radius(radius) {}
    int X, Y, Radius;
};

CascadeClassifier _faceCascade;
String _windowName = "UnityOpenCV (Test)";
VideoCapture _capture;
int _scale = 1;

extern "C" int __declspec(dllexport) __stdcall  Init(int& outCameraWidth, int& outCameraHeight)
{
    // Load LBP face cascade.
    if (!_faceCascade.load("Cascades/lbpcascade_frontalface.xml"))
        return -1;

    // Open the stream.
    _capture.open(0);
    if (!_capture.isOpened())
        return -2;

    outCameraWidth  = _capture.get(CAP_PROP_FRAME_WIDTH);
    outCameraHeight = _capture.get(CAP_PROP_FRAME_HEIGHT);

    return 0;
}

extern "C" void __declspec(dllexport) __stdcall  Close()
{
    _capture.release();
}

extern "C" void __declspec(dllexport) __stdcall SetScale(int scale)
{
    _scale = scale;
}

extern "C" void __declspec(dllexport) __stdcall Detect(Circle* outFaces, int maxOutFacesCount, int& outDetectedFacesCount)
{
    Mat frame;
    _capture >> frame;
    if (frame.empty())
        return;

    std::vector<Rect> faces;
    // Convert the frame to grayscale for cascade detection.
    Mat grayscaleFrame;
    cvtColor(frame, grayscaleFrame, COLOR_BGR2GRAY);
    Mat resizedGray;
    // Scale down for better performance.
    resize(grayscaleFrame, resizedGray, Size(frame.cols / _scale, frame.rows / _scale));
    equalizeHist(resizedGray, resizedGray);

    // Detect faces.
    _faceCascade.detectMultiScale(resizedGray, faces);

    // Draw faces.
    for (size_t i = 0; i < faces.size(); i++)
    {
        Point center(_scale * (faces[i].x + faces[i].width / 2), _scale * (faces[i].y + faces[i].height / 2));
        ellipse(frame, center, Size(_scale * faces[i].width / 2, _scale * faces[i].height / 2), 0, 0, 360, Scalar(0, 0, 255), 4, 8, 0);

        // Send to application.
        outFaces[i] = Circle(faces[i].x, faces[i].y, faces[i].width / 2);
        outDetectedFacesCount++;

        if (outDetectedFacesCount == maxOutFacesCount)
            break;
    }

    // Display debug output.
    // probably don't want to have this on the mobile version.
    imshow(_windowName, frame);
}
*/