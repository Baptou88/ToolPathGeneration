using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParallelContours : MonoBehaviour
{
    public Vector2[] points = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(5, 0),
        new Vector2(5, 5),
        new Vector2(0, 5)
    };

    public float offsetDistance = 1.0f;

    public float stepSize = 0.1f; // pour ajuster la précision

    void Start()
    {
        GenerateParallelContours();
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < points.Length; i++)
        {
            //Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
        }
    }
    void GenerateParallelContours()
    {
        List<Vector2[]> parallelContours = new List<Vector2[]>();

        Vector2[] contour = points;
        while (IsInsideBoundary(contour))
        {
            Vector2[] parallelContour = OffsetContour(contour, offsetDistance);
            parallelContours.Add(parallelContour);
            contour = parallelContour;
        }

        // Dessiner les contours parallèles
        foreach (var parallelContour in parallelContours)
        {
            DrawContour(parallelContour);
        }
    }

    bool IsInsideBoundary(Vector2[] contour)
    {
        // Vérifie si le contour est complètement à l'intérieur du contour original
        foreach (var point in contour)
        {
            if (!IsInsidePolygon(point, points))
            {
                return false;
            }
        }
        return true;
    }

    Vector2[] OffsetContour(Vector2[] contour, float distance)
    {
        List<Vector2> offsetContour = new List<Vector2>();

        for (int i = 0; i < contour.Length; i++)
        {
            Vector2 p1 = contour[i];
            Vector2 p2 = contour[(i + 1) % contour.Length];

            Vector2 v = p2 - p1;
            v.Normalize();

            Vector2 n = new Vector2(-v.y, v.x);
            Vector2 offset = n * distance;

            Vector2 offsetP1 = p1 + offset;
            Vector2 offsetP2 = p2 + offset;

            offsetContour.Add(offsetP1);

            float dist = Vector2.Distance(offsetP1, offsetP2);
            int steps = Mathf.CeilToInt(dist / stepSize);

            for (int j = 1; j < steps; j++)
            {
                float t = (float)j / steps;
                Vector2 stepPoint = Vector2.Lerp(offsetP1, offsetP2, t);
                offsetContour.Add(stepPoint);
            }
        }

        return offsetContour.ToArray();
    }

    bool IsInsidePolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    void DrawContour(Vector2[] contour)
    {
        for (int i = 0; i < contour.Length; i++)
        {
            Debug.DrawLine(contour[i], contour[(i + 1) % contour.Length], Color.red);
            Handles.color = Color.red;
            Handles.DrawLine(contour[i], contour[(i+1)%contour.Length]);
            //Gizmos.DrawLine(contour[i], contour[(i+1)%contour.Length]);
        }
    }
}
