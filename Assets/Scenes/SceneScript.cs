using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneScript : MonoBehaviour
{
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void onQuitButton()
    {
        Application.Quit();
    }

    public void GetCanvaPosition()
    {
        // // Obtenir la position du coin inférieur gauche du Canvas par rapport à l'écran
        // Vector2 canvasPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);

        // // Afficher la position du Canvas dans la console
        // Debug.Log("Position du Canvas : " + canvasPosition);

        if (mainCamera==null)
        {
            return;
        }

        // Vérifier si la caméra est de type orthographique
        if (mainCamera.orthographic)
        {
            // Calculer la hauteur de la caméra
            float cameraHeight = mainCamera.orthographicSize * 2.0f;

            // Calculer la largeur de la caméra en utilisant l'aspect ratio
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Afficher les dimensions dans la console
            Debug.Log("Largeur de la caméra : " + cameraWidth + " units");
            Debug.Log("Hauteur de la caméra : " + cameraHeight + " units");
            // Récupérer la position de la caméra
            Vector3 cameraPosition = mainCamera.transform.position;
            Debug.Log("Position de la caméra : " + cameraPosition);
        }
        else
        {
            Debug.Log("La caméra n'est pas en mode orthographique.");
        }
    }
}
