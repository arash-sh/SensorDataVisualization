using UnityEngine;

public class CamControl : MonoBehaviour {
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }
    void Update () {
        if (Specs.MouseControlsCamera)
        {
            pitch = cam.transform.eulerAngles.x;
            yaw = cam.transform.eulerAngles.y;
            Mouse();
        }
        Keyboard();
	}

    void Mouse()
    {
        float speed = Specs.CameraRotationSpeed;

        if (Input.GetMouseButton(0))
        {
            yaw += speed * Input.GetAxis("Mouse X");
            pitch -= speed * Input.GetAxis("Mouse Y");
            yaw = Mathf.Clamp(yaw, -360, 360);

            cam.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
        else
        {
            float fov = cam.fieldOfView;
            fov -= Input.GetAxis("Mouse ScrollWheel") * speed;
            fov = Mathf.Clamp(fov, 20, 90);
            cam.fieldOfView = fov;
        }

    }

    void Keyboard()
    {
        //Camera cam = Camera.main;
        float speed = Specs.CameraMotionSpeed;//0.8F;
        // spatial navigation
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown("d"))
        {
            cam.transform.Translate(speed * cam.transform.right);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKeyDown("a"))
        {
            cam.transform.Translate(-speed * cam.transform.right);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKeyDown("s"))
        {
            cam.transform.Translate(-speed * cam.transform.forward);
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKeyDown("w"))
        {
            cam.transform.Translate(speed * cam.transform.forward);
        }
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus) || Input.GetKeyDown("e"))
        {
            cam.transform.Translate(speed * cam.transform.up);
        }
        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus) || Input.GetKeyDown("q"))
        {
            cam.transform.Translate(-speed * cam.transform.up);
        }

        // visualization modes
        if (Input.GetKey(KeyCode.Alpha1))
            Specs.ThisVizMode = Specs.VIZ_MODE.TEXTURE;
        else if (Input.GetKey(KeyCode.Alpha2))
            Specs.ThisVizMode = Specs.VIZ_MODE.PARTICLE_COLOR;
        else if (Input.GetKey(KeyCode.Alpha3))
            Specs.ThisVizMode = Specs.VIZ_MODE.PARTICLE_RADIUS;
        else if (Input.GetKey(KeyCode.Alpha4))
            Specs.ThisVizMode = Specs.VIZ_MODE.TEXTURE_PAINT;
        else if (Input.GetKey(KeyCode.Alpha5))
            Specs.ThisVizMode = Specs.VIZ_MODE.TUBES;
        else if (Input.GetKey(KeyCode.Alpha6))
            Specs.ThisVizMode = Specs.VIZ_MODE.PARTICLE_LOOSE;


        // game control
        if (Input.GetKeyDown("p"))
        {
            Time.timeScale = (Time.timeScale +1) % 2; // toggle pause
        }
    }


    float ClampAngle(float angle, float min, float max)
    {
        if (angle < min)
            angle += 360F;
        if (angle > max)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

 

}
