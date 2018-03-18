using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Camera cam;

    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    private float rotX = 0.0f;
    private float rotY = 0.0f;

    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        rotX -= 0 * yMouseSensitivity * Time.deltaTime;
        rotY += 1 * xMouseSensitivity * Time.deltaTime;


        if (rotX < -90)
            rotX = -90;
        else if (rotX > 90)
            rotX = 90;

        if (rotY < -90)
            rotY = -90;
        else if (rotX > 90)
            rotY = 90;

        //this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
        cam.transform.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera

    }

    // Update is called once per frame
    void Update_Input () {
        rotX -= Input.GetAxisRaw("Vertical") * yMouseSensitivity * Time.deltaTime;
        rotY += Input.GetAxisRaw("Horizontal") * xMouseSensitivity * Time.deltaTime;

        if (rotX < -90)
            rotX = -90;
        else if (rotX > 90)
            rotX = 90;

        if (rotY < -90)
            rotY = -90;
        else if (rotX > 90)
            rotY = 90;

        //this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
        cam.transform.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera
    }
}
