using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    const float SPEED = 0.04f;

    FoveInterface f;

	// Use this for initialization
	void Start () {
		f = GetComponent<FoveInterface>();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W)) {
            MoveIt(f.transform.forward);
        }
        if (Input.GetKey(KeyCode.S)) {
            MoveIt(-f.transform.forward);
        }
        if (Input.GetKey(KeyCode.A)) {
            MoveIt(-f.transform.right);
        }
        if (Input.GetKey(KeyCode.D)) {
            MoveIt(f.transform.right);
        }
    }
    
    void MoveIt(Vector3 v) {
        v = Vector3.ProjectOnPlane(v, Vector3.up);
        f.transform.parent.transform.Translate(v * SPEED);
    }
}
