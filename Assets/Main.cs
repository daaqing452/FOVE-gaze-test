using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    static float PI = Mathf.Acos(-1);

    FoveInterface f;
    GameObject calibrationSphere, leftEyeSphere, rightEyeSphere;

    // debug
    Vector2 memAngle = Vector2.zero;
    Quaternion memRotation = Quaternion.identity;
    GameObject indicator;
    Text text1, text2, text3;

    // record
    bool recording = false;
    StreamWriter recorder;
    DateTime lastRecordTime = DateTime.MinValue;
    GameObject recordingLight;

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
        recordingLight = GameObject.Find("Recording Light");
    }

    void Update() {
        // get gaze
        FoveInterface.EyeRays eyeRays = f.GetGazeRays();
        Ray left = eyeRays.left;
        Ray right = eyeRays.right;
        leftEyeSphere.transform.position = left.origin + left.direction.normalized * 2.0f;
        rightEyeSphere.transform.position = right.origin + right.direction.normalized * 2.0f;

        // get angles and rotation of left, right, head
        Vector2 leftAngles = GetAngles(left.direction, f.transform.forward, f.transform.up);
        Vector2 rightAngles = GetAngles(right.direction, f.transform.forward, f.transform.up);
        Vector3 headAngles = GetAngles(f.transform.forward, Vector3.forward, Vector3.up);
        Quaternion leftRotation = Quaternion.FromToRotation(left.direction, f.transform.forward);
        Quaternion rightRotation = Quaternion.FromToRotation(right.direction, f.transform.forward);
        Quaternion headRotation = f.transform.rotation;

        // enable calibration
        if (Input.GetKeyDown(KeyCode.Space)) {
            calibrationSphere.SetActive(true);
        }

        // memorize angles and rotation
        if (Input.GetKeyDown(KeyCode.C)) {
            memAngle = leftAngles;
            memRotation = leftRotation;
        }

        // record the data
        if (Input.GetKeyDown(KeyCode.F)) {
            if (recording) {
                recorder.Close();
                recordingLight.GetComponent<Renderer>().material.color = Color.white;
                recording = false;
            } else {
                recorder = new StreamWriter(new FileStream("record.txt", FileMode.OpenOrCreate));
                recordingLight.GetComponent<Renderer>().material.color = Color.red;
                recording = true;
            }
        }
        DateTime now = DateTime.Now;
        if (recording && (now - lastRecordTime).TotalMilliseconds > 100) {
            Record(recorder, "left", leftAngles, leftRotation);
            Record(recorder, "right", rightAngles, rightRotation);
            Record(recorder, "head", headAngles, headRotation);
            lastRecordTime = now;
        }

        // show memorized angles and rotation
        if (Input.GetKeyDown(KeyCode.D)) {
            float depth = 1.0f;
            float theta = memAngle.x, phi = memAngle.y;
            float x = Mathf.Cos(phi / 180 * PI) * Mathf.Tan(theta / 180 * PI) * depth;
            float y = Mathf.Sin(phi / 180 * PI) * Mathf.Tan(theta / 180 * PI) * depth;
            indicator.transform.localPosition = new Vector3(x, y, depth);
            text1.text = theta.ToString();
            text2.text = phi.ToString();
            text3.text = Quaternion.Angle(memRotation, Quaternion.identity).ToString();
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

    Vector2 GetAngles(Vector3 direction, Vector3 forward, Vector3 up) {
        Vector3 directionOnPlane = Vector3.ProjectOnPlane(direction, forward);
        Vector3 upOnPlane = Vector3.ProjectOnPlane(up, forward);
        // angle between direct and forward
        float theta = Vector3.Angle(direction, forward);
        // angle on visual plane
        float phi = Vector3.Angle(directionOnPlane, upOnPlane);
        float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(directionOnPlane, upOnPlane), forward));
        phi = -(phi * sign - 90);
        return new Vector2(theta, phi);
    }

    void Record(StreamWriter recorder, string tag, Vector2 angles, Quaternion rotation) {
        recorder.Write(DateTime.Now.ToFileTime() + " ");
        recorder.Write(tag + " ");
        recorder.Write(angles.x + " " + angles.y + " ");
        recorder.Write(rotation.w + " " + rotation.x + " " + rotation.y + " " + rotation.z);
        recorder.WriteLine();
    }
}