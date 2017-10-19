using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    static float PI = Mathf.Acos(-1);

    FoveInterface f;
    GameObject calibrationSphere, leftEyeSphere, rightEyeSphere;
    Text text1, text2, text3;
    GameObject indicator;

    void Start () {
        f = GetComponent<FoveInterface>();
        calibrationSphere = GameObject.Find("Calibration Sphere");
        calibrationSphere.SetActive(false);
        leftEyeSphere = GameObject.Find("Left Gaze Sphere");
        rightEyeSphere = GameObject.Find("Right Gaze Sphere");
        text1 = GameObject.Find("Text 1").GetComponent<Text>();
        text2 = GameObject.Find("Text 2").GetComponent<Text>();
        text3 = GameObject.Find("Text 3").GetComponent<Text>();
        indicator = GameObject.Find("Indicator");
    }

    float memTheta, memPhi;
    Quaternion memQ;
    
	void Update () {
        // get gaze
        FoveInterface.EyeRays eyeRays = f.GetGazeRays();
        Ray left = eyeRays.left;
        Ray right = eyeRays.right;
        leftEyeSphere.transform.position = left.origin + left.direction.normalized * 2.0f;
        rightEyeSphere.transform.position = right.origin + right.direction.normalized * 2.0f;
        
        Vector3 gazeOnPlane = Vector3.ProjectOnPlane(left.direction, f.transform.forward);
        Vector3 upOnPlane = Vector3.ProjectOnPlane(f.transform.up, f.transform.forward);
        // angle of eye gaze
        float theta = Vector3.Angle(left.direction, f.transform.forward);
        // angle on visual plane
        float phi = Vector3.Angle(gazeOnPlane, upOnPlane);
        float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(gazeOnPlane, upOnPlane), f.transform.forward));
        phi = -(phi * sign - 90);
        
        Quaternion gazeRotation = Quaternion.FromToRotation(left.direction, f.transform.forward);
        Quaternion headRotation = f.transform.rotation;

        // enable calibration
        if (Input.GetKeyDown(KeyCode.Space)) {
            calibrationSphere.SetActive(true);
        }

        // show angle between gaze and forward
        if (Input.GetKeyDown(KeyCode.C)) {
            memTheta = theta;
            memPhi = phi;
            memQ = gazeRotation;
        }

        if (Input.GetKeyDown(KeyCode.D)) {
            float depth = 1.0f;
            float x = Mathf.Cos(memPhi / 180 * PI) * Mathf.Tan(memTheta / 180 * PI) * depth;
            float y = Mathf.Sin(memPhi / 180 * PI) * Mathf.Tan(memTheta / 180 * PI) * depth;
            indicator.transform.localPosition = new Vector3(x, y, depth);
            text1.text = memTheta.ToString();
            text2.text = memPhi.ToString();
            text3.text = Quaternion.Angle(memQ, Quaternion.identity).ToString();
            indicator.transform.SetParent(f.transform);
        }

        // render calibration point
        if (calibrationSphere.activeSelf) {
            /*RaycastHit hit;
            Ray ray = new Ray(f.transform.position, f.transform.forward);
            if (Physics.Raycast(ray, out hit, 10.0f) && false) {
                calibSphere.transform.position = hit.point;
            }*/
            calibrationSphere.transform.position = f.transform.position + f.transform.forward * 3.0f;
        }

        // calibrate
        if (Input.GetKeyUp(KeyCode.Space)) {
            calibrationSphere.SetActive(false);
            f.ManualDriftCorrection3D(calibrationSphere.transform.localPosition);
        }
    }
}