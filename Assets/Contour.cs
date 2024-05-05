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

    List<Vector2> intersection = new();
    public Intersector intersect = new Intersector();
    [SerializeField]
    public List<Vector2> points = new();

    [SerializeField]
    public int depth = 10;

    [SerializeField]
    public bool forceClosed = true;

    [Range(0.0f, 0.999999f)]
    public float t = .5f;

    [SerializeField]
    public OffsetDir offsetDirection = OffsetDir.Left;

    public int startVertex = 0;

    [Range(0.0f, 2.0f)]
    public float diameter = 1;

    [SerializeField]
    public bool selfCuttingIntersecr = false;
    [SerializeField]
    public bool basicPath = true;

    [SerializeField]
    public bool PathN = true;

    public bool smoothConvexe = true;

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


    public List<Geometry> path = new List<Geometry>();

    public List<Geometry> pathCorrected = new List<Geometry>();

    public Contour(Vector2 centre)
    {
        path = new List<Geometry>();
        points = new();
        pathCorrected = new();

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

    // public int NumSegments
    // {
    //     get
    //     {
    //         return (points.Count - 4) / 3 + 1;
    //     }
    // }

    public void AddPoint(Vector2 anchorPos)
    {
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
    }

    public void removeLastPoint()
    {
        points.RemoveAt(points.Count - 1);

    }

    public bool isClosed()
    {
        return forceClosed || points[0] == points[points.Count - 1];
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

    public int getNumLines()
    {
        return forceClosed ? points.Count : points.Count - 1;
    }
    public void calculBasicPath()
    {
        if (points.Count < 2)
        {
            return;
        }
        int numLines = getNumLines();
        Vector2 direction = points[1] - points[0];
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

        //with tool offset
        Vector2 prevPoint = new Vector2();
        bool prevIsCircle = false;
        path = new();
        path.Clear();
        for (int i = 0; i < numLines; i++)
        {
            //Vector2 previousPoint = i == 0 ? this.points[this.points.Count - 1] : this.points[i - 1];
            //Vector2 Point = this.points[i];
            //Vector2 nextPoint = i == this.points.Count - 1 ? this.points[0] : this.points[i + 1];
            //Vector2 nextnextPoint = this.points[(i + 2) % this.points.Count];


            Vector2 previousPoint = this.points[(i - 1 + this.points.Count + this.startVertex) % this.points.Count];
            Vector2 Point = this.points[(i + 0 + this.points.Count + this.startVertex) % this.points.Count];
            Vector2 nextPoint = this.points[(i + 1 + this.points.Count + this.startVertex) % this.points.Count];
            Vector2 nextnextPoint = this.points[(i + 2 + this.points.Count + this.startVertex) % this.points.Count];


            direction = nextPoint - Point;

            if (this.offsetDirection == OffsetDir.Left)
            {
                normal = new Vector2(-direction.y, direction.x).normalized;
            }
            else
            {
                normal = new Vector2(direction.y, -direction.x).normalized;
            }






            Vector2 newA = Point + normal * this.diameter;
            Vector2 newB = nextPoint + normal * this.diameter;


            if (i == 0 || prevIsCircle)
            {
                if (i == 0 && this.forceClosed)
                {
                    Vector2 previousDirection = Point - previousPoint;
                    float angleprevDirection = -1 * Vector2.SignedAngle(previousDirection, direction);
                    if ((this.offsetDirection == OffsetDir.Left && angleprevDirection < 0) || (this.offsetDirection == OffsetDir.Right && angleprevDirection > 0) || !this.smoothConvexe)
                    {

                        Vector2 bissector = (direction.normalized + previousDirection.normalized).normalized;

                        Vector2 bissectorNormal;
                        if (this.offsetDirection == OffsetDir.Right)
                        {
                            bissectorNormal = new(bissector.y, -bissector.x);
                        }
                        else
                        {
                            bissectorNormal = new(-bissector.y, bissector.x);
                        }
                        //Handles.DrawLine(Point, Point + bissectorNormal);
                        float angle2 = Vector2.Angle(bissector, previousDirection);
                        float d = this.diameter / Mathf.Cos((2 * Mathf.PI / 360) * angle2);
                        Vector2 p = Point + bissectorNormal * d;
                        Vector2 direction3 = (p - prevPoint);
                        //Handles.DrawSolidDisc(p, Vector3.forward, (float)0.2);
                        newA = p;
                    }
                }
                prevIsCircle = false;
                prevPoint = newA;
            }

            Vector2 direction2 = nextnextPoint - nextPoint;
            float angle = (-1 * Vector2.SignedAngle(direction, direction2));

            //Handles.Label(nextPoint, angle.ToString());

            if ((this.offsetDirection == OffsetDir.Left && angle > 0 && this.smoothConvexe) || (this.offsetDirection == OffsetDir.Right && angle < 0 && this.smoothConvexe))
            {
                this.path.Add(new Line(prevPoint, newB));
                if (!this.forceClosed && i == this.points.Count - 2)
                {

                }
                else
                {
                    this.path.Add(new Circle(nextPoint, normal, -angle, this.diameter));
                }


                prevPoint = newA;
                prevIsCircle = true;
            }
            else
            {
                Vector2 bissector = (direction.normalized + direction2.normalized).normalized;

                Vector2 bissectorNormal;
                if (this.offsetDirection == OffsetDir.Right)
                {
                    bissectorNormal = new(bissector.y, -bissector.x);
                }
                else
                {
                    bissectorNormal = new(-bissector.y, bissector.x);
                }
                float angle2 = Vector2.Angle(bissector, direction2);
                float d = this.diameter / Mathf.Cos((2 * Mathf.PI / 360) * angle2);

                Vector2 p = nextPoint + bissectorNormal * d;

                if (!this.forceClosed && i == this.points.Count - 2)
                {
                    p = newB;
                }

                //Handles.DrawLine(prevPoint, p);
                this.path.Add(new Line(prevPoint, p));
                prevPoint = p;
            }


        }

    }

    public void calculCorrectedPath()
    {
        if (points.Count < 2)
        {
            return;
        }
        pathCorrected = new();
        this.pathCorrected.Clear();
        intersection = new();
        intersection.Clear();
        int j2 = -1;
        Vector2 ptintersection = Vector2.zero;

        Approche app = this.typeApproche switch
        {
            TypeApproche.Perpendicular => new PerpendicularApproch(),
            TypeApproche.Circle => new CircularApproch(),
            _ => null,
        };
        if (app != null)
        {
            //pathCorrected.AddRange(app.calculateApproche(path[0],this.offsetDirection));
            List<Geometry> approcheEl = app.calculateApproche(this.path[0], this.offsetDirection);
            foreach (Geometry item in approcheEl)
            {
                this.pathCorrected.Add(item);
            }
        }
        //

        for (int i = 0; i < this.path.Count; i++)
        {
            if (i < j2)
            {
                continue;
            }
            for (int j = this.path.Count - 1; j > i + 1; j--)
            {
                intersection = new();
                intersection.Clear();
                intersect = new();
                intersection = intersect.Intersect(this.path[i], this.path[j]);
                if (intersection.Count > 0)
                {

                    if (intersection.Count > 1)
                    {
                        intersection = findNearestIntersectionPt(this.path[i], intersection);
                    }
                    if (this.selfCuttingIntersecr)
                    {
                        // Handles.color = Color.blue;
                        // for (int k = 0; k < intersection.Count; k++)
                        // {
                        //     Handles.DrawWireDisc(intersection[k], Vector3.forward, 0.1f);
                        //     Handles.Label(intersection[k], k.ToString());
                        // }
                    }
                    ptintersection = intersection[0];


                    j2 = j;
                    break;
                }

            }
            Geometry g2 = (Geometry)this.path[i].Clone();
            if (intersection.Count > 0 && i < j2)
            {
                // modifier le point de fin du cercle
                g2.modifyEndPoint(ptintersection);
            }
            if (i == j2)
            {
                // modifier le point de debut du cercle
                g2.modifyBeginPoint(ptintersection);
            }
            this.pathCorrected.Add(g2);



        }
    }

    public void calculateApproche()
    {
        if (points.Count < 2)
        {
            return;
        }
        // approche decalé
        if (this.isClosed())
        {
            switch (this.path[0].GetTypeGeom())
            {
                case TypeGeom.Line:
                    Line l = (Line)this.path[0];
                    Line l1 = new(l.Lerp(0.5f), l.ptb);
                    Line l2 = new(l.pta, l.Lerp(0.5f));
                    this.path[0] = l1;
                    this.path.Add(l2);
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }

    public List<Vector2> findNearestIntersectionPt(Geometry g, List<Vector2> p)
    {
        if (p.Count > 2)
        {
            throw new NotImplementedException("trop de point d'intersections");
        }
        switch (g.GetTypeGeom())
        {
            case TypeGeom.Line:
                Line l = (Line)g;
                float d1 = Vector2.Distance(l.pta, p[0]);
                float d2 = Vector2.Distance(l.pta, p[1]);
                if (d1 > d2)
                {
                    Vector2 temp = p[0];
                    p.RemoveAt(0);
                    p.Add(temp);
                    return p;
                }
                return p;


            case TypeGeom.Circle:
                Circle c = (Circle)g;
                float a1 = Vector2.Angle(c.normal, p[0] - c.center);
                float a2 = Vector2.Angle(c.normal, p[1] - c.center);
                if (c.angle < 0)
                {
                    if (a1 > a2)
                    {
                        Vector2 temp = p[0];
                        p.RemoveAt(0);
                        p.Add(temp);
                        return p;
                    }
                    return p;
                }
                else
                {
                    if (a1 > a2)
                    {
                        Vector2 temp = p[0];
                        p.RemoveAt(0);
                        p.Add(temp);
                    }
                    return p;
                }

            default:
                throw new NotImplementedException();

        }
        throw new NotImplementedException();
    }
}