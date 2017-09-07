using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate(){
		float v = Input.GetAxis("Vertical");
		GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0.5f * v, 0,0));
	}
}
