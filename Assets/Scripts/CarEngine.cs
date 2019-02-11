using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarEngine : MonoBehaviour {

	public Transform path;
	public float maxSteerAngle = 45f;
	public float turnSpeed = 5f;
	public float maxMotorTorque = 80f;
	public float currentSpeed;
	public float maxSpeed = 100f;
	public float maxBrakeTorque = 150f;
	public WheelCollider wheelFL;
	public WheelCollider wheelFR;
	public WheelCollider wheelBL;
	public WheelCollider wheelBR;
	public bool avoiding = false;

	public float sensorLength = 10f;
	public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
	public float frontSideSensorPosition = 0.3f;
	public float frontSensorAngle = 30f;

	private List<Transform> nodes;
	private int currentNode = 0;
	private float targerSteerAngle = 0;


	// Use this for initialization
	private void Start () {
		Transform[] pathTransform = path.GetComponentsInChildren<Transform> ();
		nodes  = new List<Transform>();

		for (int i = 0; i < pathTransform.Length; i++) {
			if (pathTransform[i] != path.transform) {
				nodes.Add (pathTransform [i]);
			}
		}
	}
	
	// Update is called once per frame
	private void FixedUpdate () {
		Sensors ();
		ApplySteer ();
		Drive ();
		CheckWayDistance ();
		LerpToSteerAngle ();
	}

	private void Sensors (){
		RaycastHit hit;
		Vector3 sensorStartPosition = transform.position;
		sensorStartPosition += transform.forward * frontSensorPosition.z;
		sensorStartPosition += transform.up * frontSensorPosition.y;
		float avoidMultiplier = 0;
		avoiding = false;

		//prednji desni senzor
		sensorStartPosition += transform.right * frontSideSensorPosition;
		if (Physics.Raycast(sensorStartPosition, transform.forward, out hit, sensorLength)) {
			if (hit.collider.CompareTag("Obstacle")) {
				Debug.DrawLine (sensorStartPosition, hit.point);
				avoiding = true;
				avoidMultiplier -= 1f;
			}
		}
		//prednji desni kutni senzor
		if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength)) {
			if (hit.collider.CompareTag("Obstacle")) {
				Debug.DrawLine (sensorStartPosition, hit.point);
				avoiding = true;
				avoidMultiplier -= 0.5f;
			}
		}
		//prednji lijevi senzor
		sensorStartPosition -= transform.right * frontSideSensorPosition * 2;
		if (Physics.Raycast(sensorStartPosition, transform.forward, out hit, sensorLength)) {
			if (hit.collider.CompareTag("Obstacle")) {
				Debug.DrawLine (sensorStartPosition, hit.point);
				avoiding = true;
				avoidMultiplier += 1f;
			}
		}
		//prednji lijevi kutni senzor
		if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength)) {
			if (hit.collider.CompareTag("Obstacle")) {
				Debug.DrawLine (sensorStartPosition, hit.point);
				avoiding = true;
				avoidMultiplier += 0.5f;
			}
		}
		//cetralni senzor
		if (avoidMultiplier == 0) {
			if (Physics.Raycast(sensorStartPosition, transform.forward, out hit, sensorLength)) {
				if (!hit.collider.CompareTag("Obstacle")) {
					Debug.DrawLine (sensorStartPosition, hit.point);
					avoiding = true;
					if (hit.normal.x < 0) {
						avoidMultiplier = -1;
					} else {
						avoidMultiplier = 1;
					}
				}
			}
		}

		if (avoiding) {
			targerSteerAngle = maxSteerAngle * avoidMultiplier;
		}
	}

	private void ApplySteer(){
		if (avoiding)return;
		Vector3 relativeVector = transform.InverseTransformPoint (nodes [currentNode].position);
		float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

		targerSteerAngle = newSteer;
	}

	private void Drive(){
		currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;

		if (currentSpeed < maxSpeed) {
			wheelFL.motorTorque = maxMotorTorque;
			wheelFR.motorTorque = maxMotorTorque;
		} else {
			wheelFL.motorTorque = 0;
			wheelFR.motorTorque = 0;		
		}			
	}

	private void CheckWayDistance (){
		if (Vector3.Distance(transform.position, nodes[currentNode].position) < 10f) {
			if (currentNode == nodes.Count - 1) {
				currentNode = 0;
				currentSpeed = currentSpeed / 3;
				sensorLength = 15f;
			}else{				
				currentNode++;
			}
		}
	}

	private void LerpToSteerAngle(){
		wheelFL.steerAngle = Mathf.Lerp (wheelFL.steerAngle, targerSteerAngle, Time.deltaTime * turnSpeed);
		wheelFR.steerAngle = Mathf.Lerp (wheelFR.steerAngle, targerSteerAngle, Time.deltaTime * turnSpeed);	
	}
}
