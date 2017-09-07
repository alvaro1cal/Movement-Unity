using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSide : MonoBehaviour {

	public Transform target;
	public Vector3 offset;
	public float smooth;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate(){
		transform.position = Vector3.Slerp(transform.position, target.position + offset, smooth);
	}
}
