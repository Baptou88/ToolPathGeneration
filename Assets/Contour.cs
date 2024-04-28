using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum OffsetDir
{
    Left,
    Right,
}
[System.Serializable]
public class Contour
{

    [SerializeField]
    public List<Vector2> points;

    [SerializeField]
    public int depth = 10;

    [SerializeField]
    public bool forceClosed = true;

    [Range(0.0f, 0.999999f)]
    public float t = .5f;

    [SerializeField]
    public OffsetDir offsetDirection = OffsetDir.Left;

    public int startVertex = 0;

    [Range(0.0f , 2.0f)]
    public float diameter = 1;

    [SerializeField]
    public bool selfCuttingIntersecr = false;
    [SerializeField]
    public bool basicPath = true;
    
    [SerializeField]
    public bool PathN = false;

    [SerializeField]
    public TypeApproche typeApproche;
    [SerializeField]
    public PerpendicularApproch approche = new();
    // public Approche approche = typeApproche switch
    // {
    //     TypeApproche.Perpendicular => new PerpendicularApproch(),
    //     TypeApproche.Circle => new CircularApproch(),
    //     _ => null,
    // };

    public Contour(Vector2 centre)
    {
        points = new List<Vector2>
        {
            centre+Vector2.left,
            centre+(Vector2.left+Vector2.up)*.5f,
            centre + (Vector2.right+Vector2.down)*.5f,
            centre + Vector2.right
        };
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return (points.Count - 4) / 3 + 1;
        }
    }

    public void AddPoint(Vector2 anchorPos)
    {
        //points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        //points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i];
        points[i] = pos;

        //if (i % 3 == 0)
        //{
        //    if (i + 1 < points.Count)
        //    {
        //        points[i + 1] += deltaMove;
        //    }
        //    if (i - 1 >= 0)
        //    {
        //        points[i - 1] += deltaMove;
        //    }
        //}
        //else
        //{
        //    bool nextPointIsAnchor = (i + 1) % 3 == 0;
        //    int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
        //    int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

        //    if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count)
        //    {
        //        float dst = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
        //        Vector2 dir = (points[anchorIndex] - pos).normalized;
        //        points[correspondingControlIndex] = points[anchorIndex] + dir * dst;
        //    }
        //}
    }

    public void removeLastPoint()
    {
        points.RemoveAt(points.Count - 1);
        
    }

    public bool  isClosed()
    {
        return forceClosed || points[0] == points[points.Count-1];
    }

    public bool isCounterClockwize()
    {
        
        float area = 0;

        // Appliquer la formule du shoelace
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 currentVertex = points[i];
            Vector2 nextVertex = points[(i + 1) % points.Count];
            area += (nextVertex.x + currentVertex.x) * (nextVertex.y - currentVertex.y);
        }

        // Prendre la valeur absolue et diviser par 2
        //area = Mathf.Abs(area) / 2f;

        return area > 0;
    }

}