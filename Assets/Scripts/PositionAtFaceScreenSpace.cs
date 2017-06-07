using UnityEngine;

public class PositionAtFaceScreenSpace : MonoBehaviour {
    public Camera myCamera;
    private float _camDistance;

    void Start() {
        if (myCamera == null) {
            myCamera = Camera.main;
        }
        _camDistance = Vector3.Distance(myCamera.transform.position, transform.position);
    }
    private static bool s_update_called = false;
    bool applyingForce = false;
    private void static_Update() {
        applyingForce = false;
        if (!s_update_called) {
            if (Input.GetMouseButton(0)) {
                Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rh;
                if (Physics.Raycast(l_ray, out rh)) {
                    Debug.Log("RayCast Hit: " + rh.transform.name);
                    //rh.rigidbody.AddForceAtPosition(-5*(rh.point - rh.transform.position), rh.point);
                    rh.rigidbody.AddExplosionForce(5, rh.point, 1);
                    applyingForce = true;
                }
            }

            if (!applyingForce) {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, .05f);
                }
            }

            s_update_called = true;
        }
    }

    void Update() {
        if (OpenCVFaceDetection.NormalizedFacePositions == null ||
            OpenCVFaceDetection.NormalizedFacePositions.Count == 0)
            return;
        else {
            //Debug.Log("Faces Detected: " + OpenCVFaceDetection.NormalizedFacePositions.Count);
        }

        //Debug.Log(string.Format("x: {0}, y: {1}",
        //    OpenCVFaceDetection.NormalizedFacePositions[0].x,
        //    OpenCVFaceDetection.NormalizedFacePositions[0].y));

        static_Update();

        Vector3 pos = new Vector3(OpenCVFaceDetection.NormalizedFacePositions[0].x + .2f,
                                  OpenCVFaceDetection.NormalizedFacePositions[0].y,
                                  _camDistance);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (!applyingForce && (rb == null || rb.velocity.magnitude < .1f)) {
            transform.localPosition   = Vector3.Lerp(transform.localPosition, myCamera.ViewportToWorldPoint(pos), .1f);
        }
        transform.localScale = Vector3.one;

    }
    private void LateUpdate() {
        s_update_called = false;
    }
}