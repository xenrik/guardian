using System;
using UnityEngine;

[System.Serializable]
public class PIDController {
	public float Kp = 0.2f;
	public float Ki = 0.05f;
	public float Kd = 1f;

	private Vector3 lastError;

	private Vector3 integral;
	private Vector3 derivative;

	public PIDController (float Kp = 0.2f, float Ki = 0.05f, float Kd = 1f) {
		this.Kp = Kp;
		this.Ki = Ki;
		this.Kd = Kd;
	}

	public Vector3 Update(Vector3 error, float dt) {
		integral += error * dt;
		derivative = (error - lastError) / dt;
		lastError = error;

		return error * Kp + integral * Ki + derivative * Kd;
	}
}

