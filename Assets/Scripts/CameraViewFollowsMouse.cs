using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewFollowsMouse : MonoBehaviour
{
    [SerializeField] float speedH = 2.0f;
    [SerializeField] float speedV = 2.0f;

    [SerializeField] bool limitYaw = false;
    [SerializeField] float yawMin = -90.0f;
    [SerializeField] float yawMax = 90.0f;

    [SerializeField] bool limitPitch = false;
    [SerializeField] float pitchMin = -90.0f;
    [SerializeField] float pitchMax = 90.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start() {
       Cursor.visible = false;
    }

    void Update () {
        yaw += speedH * Input.GetAxis("Mouse X");
        if (limitYaw)
            yaw = Mathf.Clamp(yaw, yawMin, yawMax);

        pitch -= speedV * Input.GetAxis("Mouse Y");
        if (limitPitch)
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
