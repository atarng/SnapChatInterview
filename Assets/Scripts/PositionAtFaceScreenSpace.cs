using UnityEngine;

public class PositionAtFaceScreenSpace : MonoBehaviour {
    private float _camDistance;

    void Start() {
        _camDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
    }

    void Update() {
        if (OpenCVFaceDetection.NormalizedFacePositions == null ||
            OpenCVFaceDetection.NormalizedFacePositions.Count == 0)
            return;

        Vector3 pos = new Vector3(OpenCVFaceDetection.NormalizedFacePositions[0].x,
                                  OpenCVFaceDetection.NormalizedFacePositions[0].y,
                                  _camDistance);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
        transform.localScale = Vector3.one * OpenCVFaceDetection.NormalizedFacePositions[0].z;
    }
}