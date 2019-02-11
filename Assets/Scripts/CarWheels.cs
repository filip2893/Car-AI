using UnityEngine;
using System.Collections;

public class CarWheels : MonoBehaviour {

	public WheelCollider targetWheel;
	public Vector3 wheelPosition = new Vector3();
	public Quaternion wheelRotation = new Quaternion ();
		
	// Update is called once per frame
	private void Update () {
		targetWheel.GetWorldPose (out wheelPosition, out wheelRotation);
		transform.position = wheelPosition;
		transform.rotation = wheelRotation;
	}
}
