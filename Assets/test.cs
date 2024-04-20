using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Transform startTransform; // Transform du point de départ de l'arc
    public float radius = 1.0f; // Rayon de l'arc
    public float angle = 90.0f; // Angle de l'arc en degrés
    // Start is called before the first frame update
    void Start()
    {
        // Calculer le point de fin de l'arc
        Vector3 endPosition = CalculateArcEndPosition(startTransform.position, startTransform.forward, radius, angle);

        // Dessiner l'arc
        DrawArc(startTransform.position, endPosition);
    }

    // Calcule le point de fin de l'arc
    Vector3 CalculateArcEndPosition(Vector3 center, Vector3 normal, float radius, float angle)
    {
        // Convertir l'angle en radians
        float angleRad = Mathf.Deg2Rad * angle;

        // Calculer la direction du point de fin
        Vector3 endDirection = Quaternion.AngleAxis(angle, normal) * startTransform.forward;

        // Calculer la position du point de fin
        Vector3 endPosition = center + Quaternion.AngleAxis(angle, startTransform.up) * (startTransform.forward * radius);

        return endPosition;
    }
    // Update is called once per frame
    void Update()
    {
        
    }


    // Dessine l'arc entre le point de départ et le point de fin
    void DrawArc(Vector3 startPoint, Vector3 endPoint)
    {
        // Dessiner l'arc avec une ligne
        Debug.DrawLine(startPoint, endPoint, Color.red, 10.0f);
    }
}
