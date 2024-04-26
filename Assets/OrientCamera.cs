using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientCamera : MonoBehaviour
{

    public Transform target; // Référence à la caméra ou à un objet point de vue
    public float rotationSpeed = 5f;

    public void OrientCameraToXYPlane()
    {
        Vector3 targetPosition = target.position;
        targetPosition.x = 0;
        targetPosition.y = 0; // Met la composante Y à 0 pour le plan XZ
        targetPosition.z = -300;
        target.position = targetPosition;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward); // Oriente vers le bas (plan XY)
        target.rotation = Quaternion.Slerp(target.rotation, targetRotation,1);
        //target.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void OrientCameraToXZPlane()
    {
        Vector3 targetPosition = target.position;
        
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.down); // Oriente vers le bas (plan XY)
        target.rotation = Quaternion.Slerp(target.rotation, targetRotation, 1);
        targetPosition.x = 0;
        targetPosition.y = 300; // Met la composante Y à 0 pour le plan XZ
        targetPosition.z = 0;
        target.position = targetPosition;
        //target.rotation = Quaternion.Euler(90, 0, 00);
        //target.LookAt(Vector3.zero);
    }

    public void cameraAnimationBtn()
    {
        // https://stackoverflow.com/questions/57193130/i-want-to-lerp-my-camera-from-one-quaternion-to-another
        StartCoroutine(cameraAnimation());
    }
    public IEnumerator cameraAnimation()
    {
        Vector3 targetRot = new(45, 45, 0);
        Quaternion startRot = target.localRotation;
        Quaternion endRot = Quaternion.Euler(targetRot);

        var duration = Quaternion.Angle(startRot, endRot) / 90f;

        float timePassed = 0f;
        while (timePassed < duration)
        {
            float lerpFactor = timePassed / duration;


            var smoothLerpFactor = Mathf.SmoothStep(0,1,lerpFactor);

            target.localRotation = Quaternion.Lerp(startRot,endRot,smoothLerpFactor);

            timePassed += Mathf.Min(duration - timePassed, Time.deltaTime);
            yield return null;
        }
        target.localRotation = endRot;
    }

    public void deplaceCamera()
    {
        target.position += new Vector3(0,20,0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
