using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInteraction : MonoBehaviour {
    // Avatar is not used anymore. Left in the scene for backwards compatibility
    public GameObject GuideAvatar { get; private set; }

    private Vector3 camStartPos;
    private Quaternion camStartRot;
    private Vector3 camEndPos;
    private Quaternion camEndRot;
    private float motionDuration = 1F; // time in seconds to finish the camera motion 
    private float elapsedT;
    private bool camMoving = false;

	void Start() {
        
        GuideAvatar = GameObject.Find("Avatar");
        PlaceAvatar();

    }
	
	void Update () {
        if (camMoving)
        {
            Camera cam = Camera.main;


            elapsedT += Time.deltaTime;
                                           
            transform.rotation = Quaternion.Slerp(transform.rotation ,camEndRot, elapsedT/motionDuration);


            transform.position = Vector3.Slerp(camStartPos ,camEndPos, elapsedT/motionDuration);


            if (elapsedT  >= motionDuration)
            {
                camMoving = false;
                PlaceAvatar();
                UnhideAvatar();
            }
        }

    }

    public void MoveCam(Vector3 pos, Quaternion rot)
    {
         if (transform.position != pos || transform.rotation != rot)
        {
            HideAvatar();
            camStartPos = transform.position;
            camStartRot = transform.rotation;
            camEndPos = pos;
            camEndRot = rot;
            elapsedT = 0;
            camMoving = true;
        }
    }

    public void PlaceAvatar()
    {
        GuideAvatar.transform.position = transform.position +
            transform.right * Utilities.Scaled(2.7F) +
            transform.up * Utilities.Scaled(-3F) +
            transform.forward * Utilities.Scaled(4.9F);
    }


    public void HideAvatar()
    {
        GuideAvatar.SetActive(false);
    }
    public void UnhideAvatar()
    {
        GuideAvatar.SetActive(true);
    }
}
