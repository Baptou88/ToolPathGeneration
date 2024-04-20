using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Vector3 CameraPosition;
    [Header("Camera Setting")]
    public float CameraSpeed;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        CameraPosition = this.transform.position; 
   //cam.orthographicSize
   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))    
        {
            CameraPosition.y += CameraSpeed / 100;
        }
        if (Input.GetKey(KeyCode.S))
        {
            CameraPosition.y -= CameraSpeed / 100;
        }

        if (Input.GetKey(KeyCode.A))
        {
            CameraPosition.x -= CameraSpeed / 100;
        }
        if (Input.GetKey(KeyCode.D))
        {
            CameraPosition.x += CameraSpeed / 100;
        }


        if(Input.GetKey(KeyCode.E))
        {
            //CameraPosition.z -= CameraSpeed / 100;
            cam.orthographicSize += CameraSpeed / 100;

        }
        if (Input.GetKey(KeyCode.Q))
        {
            //CameraPosition.z += CameraSpeed / 100;
            cam.orthographicSize -= CameraSpeed / 100;
        }    


        this.transform.position = CameraPosition;
    }
}
