using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component to be added to be the game object in order to rotate (zoom?) it with mouse
public class ObjectViewer : MonoBehaviour {
    
    public Vector3 InitialPos { get; private set; }
    public Quaternion InitialRot { get; private set; }
    public Vector3 InitialScl { get; private set; }

    private void Awake()
    {
        InitialPos = transform.position;
        //InitialPos = gameObject.GetComponent<Renderer>().bounds.center; 
        InitialRot = transform.rotation;
        InitialScl = transform.localScale;
    }

     private void Update () {
        if (Input.GetMouseButton(0))
        {
            //Vector3 center = GetComponent<Renderer>().bounds.center;
            Vector3 center = transform.position;
            float speed = 2 * Specs.CameraRotationSpeed;
            Vector3 axis = center + Camera.main.transform.up;
            transform.RotateAround(center, Camera.main.transform.up, -speed * Input.GetAxis("Mouse X"));
            transform.RotateAround(center, Camera.main.transform.right, speed * Input.GetAxis("Mouse Y"));
            //transform.rotation= Quaternion.AngleAxis( speed * Input.GetAxis("Mouse X"), axis);
        }
        else if (Input.GetMouseButton(1))
        {
        }
        else if (Input.GetMouseButton(2)) {
            float speed = 0.5F * Specs.CameraMotionSpeed;
            transform.position +=new Vector3(0, Input.GetAxis("Mouse Y"), 0) * speed;
        }
        else
        {
            float speed = 1F * Specs.CameraMotionSpeed;
            transform.localScale += Vector3.one*Input.GetAxis("Mouse ScrollWheel")* speed ;

        }
        
        //{
        //    //float speed = 20f;

        //    Camera cam = Camera.main;

        //    float fov = cam.fieldOfView;
        //    fov -= Input.GetAxis("Mouse ScrollWheel") * speed;
        //    fov = Mathf.Clamp(fov, 20, 90);
        //    cam.fieldOfView = fov;
        //}


    }
}
