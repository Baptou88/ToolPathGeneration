using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Security.Cryptography;
using NUnit.Framework;

[CustomEditor(typeof(ContourCreator))]
public class ContourEditor : Editor
{

    ContourCreator creator;
    Contour Contour;
    public LineRenderer cuttingPath;
    public Intersector intersect = new Intersector();
    List<Vector2> intersection = new();

    public List<MyClass> path = new List<MyClass>();

    public List<MyClass> pathN = new List<MyClass>();

    void OnSceneGUI()
    {
        Input();
        Draw();
        afficherPosAtT(Contour.t);
    }


    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Add segment");
            cuttingPath.positionCount++;
            cuttingPath.SetPosition(cuttingPath.positionCount-1, mousePos);
            
            Contour.AddSegment(mousePos);
        }

        
        
         cuttingPath.loop = Contour.closed;
        
       
    }

    public float lengthContourN()
    {

        float length = 0.0f;
        for (int i = 0; i < path.Count; i++)
        {
            length += path[i].Length();
        }
        return length;
    }

    public void afficherPosAtT(float t)
    {
        float total = lengthContourN();
        float pos = 0;
        float nextPos = 0;
        int index = 0;
        float tIndex = 0;
        for (int i = 0;i < path.Count;i++)
        {
            nextPos += path[i].Length() / total;
            if (t > nextPos)
            {
                pos = nextPos;
                continue;
            }
            else
            {
                index = i;
                //Debug.Log(i);
                tIndex = (t-pos) * total / path[i].Length();
                break;
            }
        }
        Vector3 toolPos = Vector3.zero;
        switch (path[index].GetTypeGeom()) {
            case TypeGeom.Line:
            {
                Line line = (Line)path[index];
                
                 toolPos = Vector2.Lerp(line.pta,line.ptb, tIndex);   
                
                break;
            }
            case TypeGeom.Circle:
            {
                Circle circle = (Circle)path[index]; 
                
                    toolPos = circle.Lerp(tIndex);
                
                break;
            }
        }
        Handles.color = Color.blue;
        Handles.DrawSolidDisc(toolPos, Vector3.forward, Contour.diameter);
        if (creator.cylindre != null)
        {
            creator.cylindre.transform.position = toolPos;
        }
    }

    private void FindSelfCuttingIntersection()
    {
        pathN.Clear();

        //tentative n°1 
        //for (int i = 0; i < path.Count; i++)
        //{
        //    intersection.Clear();
        //    for (int j = 0; j < i-1; j++)
        //    {
        //        intersection.Clear();
        //        intersection = intersect.Intersect(path[i], path[j]);
                
        //        for (int k = 0; k < intersection.Count; k++)
        //        {
        //            Handles.color = Color.white;
        //            Handles.DrawSolidDisc(intersection[k], Vector3.forward, .2f);
        //            Handles.color = Color.black;
        //            Handles.Label(intersection[k], "i " + i.ToString() + ", j " + j.ToString());


                    
        //        }
        //        //cherche le plus petit i 
        //        int firstIntersectInTraj = intersection[i];
        //        foreach (var item in intersection)
        //        {

        //        }


        //    }
        //    if (intersection.Count == 0)
        //    {
        //        pathN.Add(path[i]);
        //    }
        //    else
        //    {
        //        //for (int l = j; l < i; l++)
        //        //{
        //        //   pathN.RemoveAt(l);
        //        //}
        //    }

        //}


        //tentative n°2
        intersection.Clear();
        int j2 = -1;
        Vector2 ptintersection = Vector2.zero;
        for (int i = 0; i < path.Count; i++)
        {
            if (i < j2)
            {
                continue;
            }
            for (int j = path.Count-1; j > i+1; j--)
            {
                intersection.Clear();
                intersection = intersect.Intersect(path[i], path[j]);
                if (intersection.Count > 0)
                {
                    Handles.color = Color.blue;
                    Handles.DrawSolidDisc(intersection[0], Vector3.forward, 0.1f);
                    ptintersection = intersection[0];
                    j2 = j;
                    break;
                }
               
            }
            switch (path[i].GetTypeGeom())
            {
                case TypeGeom.Line:
                    Line l = (Line)path[i].Clone();
                    if (intersection.Count > 0)
                    {
                        l.ptb = ptintersection;
                    }
                    if (i == j2)
                    {
                        l.pta = ptintersection;
                        j2 = -1;
                    }
                    pathN.Add(l);

                    break;
                default:
                    pathN.Add(path[i]);
                    break;
            }
            
        }


        if (Contour.PathN)
        {
            for (int i = 0; i < pathN.Count; i++)
            {
                Handles.color = Color.magenta;
                pathN[i].Draw();
            }
        }
    }
    void Draw()
    {
        for (int i = 0; i < Contour.points.Count; i++)
        {
            Vector3 p = Contour.points[i];
            Vector3 p2 = p;
            p2.z = Contour.depth;
            Handles.color = Color.black;
            Handles.DrawLine(p, p2);
        }
        int numLines = Contour.closed ? Contour.points.Count : Contour.points.Count - 1;
        for (int i = 0; i < numLines ; i++)
        {
            
            cuttingPath.SetPosition(i,Contour.points[i]);
            
            Handles.color = Color.black;
            if(i>= Contour.points.Count-1)
            {
                Handles.DrawLine(Contour.points[i], Contour.points[0]);

                Vector3 vec3a = Contour.points[i];
                vec3a.z = Contour.depth;
                Vector3 vec3b = Contour.points[0];
                vec3b.z = Contour.depth;
                Handles.DrawLine(vec3a, vec3b);
            } else
            {
                Handles.DrawLine(Contour.points[i], Contour.points[i+1]);


                Vector3 vec3a = Contour.points[i];
                vec3a.z = Contour.depth;
                Vector3 vec3b = Contour.points[i+1];
                vec3b.z = Contour.depth;

                Handles.DrawLine(vec3a, vec3b);
            }
            
            
        }

        Handles.color = Color.red;
        for (int i = 0; i < Contour.NumPoints; i++)
        {
            Vector2 newPos = Handles.FreeMoveHandle(Contour[i], .1f, Vector2.zero, Handles.CylinderHandleCap);
            if (Contour[i] != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                Contour.MovePoint(i, newPos);
                cuttingPath.SetPosition(i,newPos);
            }
        }

        //Handles.color = Color.blue;
        //Vector2 tPoint = Vector2.Lerp(Contour.points[0], Contour.points[1], Contour.t);
        //Handles.FreeMoveHandle(tPoint, .1f, Vector2.zero, Handles.CylinderHandleCap);
    
        //Handles.color = Color.green;

        Vector2 direction = (Contour.points[1] - Contour.points[0]);
        Vector2 normal = new Vector2(-direction.y,direction.x).normalized;
        //Vector2 pointOnPerpendicular = tPoint + normal * Contour.diameter;
        //Handles.FreeMoveHandle(pointOnPerpendicular, .1f, Vector2.zero, Handles.CylinderHandleCap);

        //with tool offset
        Vector2 prevPoint = new Vector2();
        bool prevIsCircle = false;

        path.Clear();

        
        for (int i = 0; i < numLines; i++)
        {
            Vector2 previousPoint = i == 0 ? Contour.points[Contour.points.Count - 1] : Contour.points[i - 1];
            Vector2 Point = Contour.points[i];
            Vector2 nextPoint = i == Contour.points.Count - 1 ? Contour.points[0] : Contour.points[i + 1];
            Vector2 nextnextPoint = Contour.points[(i+2) % Contour.points.Count];

            
            direction = nextPoint - Point;

            if (Contour.offsetDirection == OffsetDir.Left)
            {
                normal = new  Vector2(-direction.y, direction.x).normalized;  
            }
            else
            {
                normal = new Vector2(direction.y, -direction.x).normalized;
            }
            

           
            Handles.color = Color.yellow;


            Vector2 newA = Point + normal * Contour.diameter;
            Vector2 newB = nextPoint + normal * Contour.diameter;


            if (i == 0 || prevIsCircle)
            {
                if (i == 0 && Contour.closed)
                {
                    Vector2 previousDirection = Point - previousPoint;
                    
                    Vector2 bissector = (direction.normalized + previousDirection.normalized).normalized;
                    
                    Vector2 bissectorNormal;
                    if (Contour.offsetDirection == OffsetDir.Right)
                    {
                        bissectorNormal = new(bissector.y, -bissector.x);
                    }
                    else
                    {
                        bissectorNormal = new(-bissector.y, bissector.x);
                    }
                    //Handles.DrawLine(Point, Point + bissectorNormal);
                    float angle2 = Vector2.Angle(bissector, previousDirection);
                    float d = Contour.diameter / Mathf.Cos((2 * Mathf.PI / 360) * angle2);
                    Vector2 p = Point + bissectorNormal * d;
                    Vector2 direction3 = (p - prevPoint);
                    //Handles.DrawSolidDisc(p, Vector3.forward, (float)0.2);
                    newA = p;
                }
                prevIsCircle = false;
                prevPoint = newA;
            }

            Handles.color = Color.magenta;

          

                Vector2 direction2 = nextnextPoint - nextPoint;
                float angle = (-1* Vector2.SignedAngle(direction,direction2));

                Handles.Label(nextPoint,angle.ToString());
               
                if ((Contour.offsetDirection == OffsetDir.Left && angle > 0) || (Contour.offsetDirection == OffsetDir.Right && angle <0) )
                {
                    path.Add(new Line(prevPoint,newB));
                    path.Add(new Circle(nextPoint, normal, -angle, Contour.diameter));
                    
                    prevPoint = newA;
                    prevIsCircle = true;
                } else
                {
                    Vector2 bissector = (direction.normalized + direction2.normalized).normalized;
                    
                    Vector2 bissectorNormal;
                    if (Contour.offsetDirection == OffsetDir.Right)
                    {
                        bissectorNormal = new(bissector.y, -bissector.x);
                    }
                    else
                    {
                        bissectorNormal = new(-bissector.y, bissector.x);
                    }
                    float angle2 = Vector2.Angle(bissector,direction2);
                    float d =  Contour.diameter / Mathf.Cos((2 * Mathf.PI/360)*angle2) ;
                   
                    Vector2 p = nextPoint + bissectorNormal * d;

                    if (!Contour.closed && i == Contour.points.Count-2)
                    {
                    //p = nextPoint + normal * d;
                    p = newB;
                    }
                    Vector2 direction3 = (p - prevPoint);

                    if (Vector2.Dot(direction.normalized, direction3)< .0f)
                    {
                        Handles.color= Color.cyan;
    
                    }
                    //Handles.DrawLine(prevPoint, p);
                    path.Add(new Line(prevPoint, p));
                    prevPoint = p;
                }
            //if (Contour.closed && i == Contour.points.Count -1)
            //{
                
            //    if (path[0].GetTypeGeom() == TypeGeom.Line)
            //    {
            //        Line l = (Line)path[0];
            //        l.pta = prevPoint;
            //    }
            //}

        }

        if (Contour.selfCuttingIntersecr)
        {
            FindSelfCuttingIntersection();
        }


        if (Contour.basicPath)
        {
            Handles.color = Color.yellow;
            for (int i = 0; i < path.Count; i++)
            {
                path[i].Draw();
            }
        }


        //Line line1 = new(new Vector2(1, 0), new Vector2(3.1f, 0.1f));
        //Line line2 = new(new Vector2(3, -1), new Vector2(3, 1));
        //Handles.color = Color.blue;
        //line1.Draw();
        //line2.Draw();
        //Debug.Log("r");
        //List<Vector2> intersection1 = intersect.Intersect(line1, line2);
        //for (int i = 0; i < intersection1.Count; i++)
        //{
        //    Debug.Log(intersection1[i]);
        //    Handles.DrawSolidDisc(intersection1[i], Vector3.forward, .2f);
        //}
        //Debug.Log("e");
    }

    public void removeLastPoint()
    {
        Contour.removeLastPoint();
    }
    void OnEnable()
    {
        creator = (ContourCreator)target;
        if (creator.contour == null)
        {
            creator.CreateContour();
        }
        Contour = creator.contour;

        if (creator.cuttingPath ==null)
        {
            Debug.Log("pas de line renderer");
        }
        cuttingPath = creator.cuttingPath;


        Vector3[] points = new Vector3[Contour.points.Count] ;
        cuttingPath.positionCount = Contour.points.Count;
        
        for (int i = 0; i < Contour.points.Count; i++)
        {
            points[i] = Contour.points[i];
        }
        cuttingPath.SetPositions(points);
    }
}


public enum TypeGeom
{
    None,
    Line,
    Circle
}
public abstract class MyClass
{

    abstract public void Draw();
    abstract public TypeGeom GetTypeGeom();

    abstract public float Length();

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public class Line : MyClass
{
    public Vector2 pta;
    public Vector2 ptb;
    public Line(Vector2 pta, Vector2 ptb)
    {
        this.pta = pta;
        this.ptb = ptb;
    }
    public override void Draw()
    {
        Handles.DrawLine(pta, ptb);
    }
    public Vector2 dir()
    {
        return ptb - pta;
    }

    public override TypeGeom GetTypeGeom()
    {
        return TypeGeom.Line;
    }

    public override float Length()
    {
        return Vector3.Distance(pta, ptb);
    }
}

public class Circle : MyClass
{
    public Vector2 center;
    public float radius;
    float angle;
    Vector2 normal;
    public Circle(Vector2 center, Vector2 normal, float angle, float radius)
    {
        this.center = center;
        this.radius = radius;
        this.angle = angle;
        this.normal = normal;
    }

    public override void Draw()
    {
        
        Vector2 start = center + normal * radius;

        Handles.DrawWireArc(center, Vector3.forward, normal, angle, radius, 0);

       
        
        
    }

    public Vector2 endDirection ()
    {
        return (Quaternion.AngleAxis(angle, Vector3.forward) * normal).normalized * radius;
    }
    public Vector2 endPoint ()
    {
        return center + endDirection() ;
    }
    public Vector2 Lerp(float t)
    {
        Vector3 cent = center;
        //Vector3 pointOnArc = cent + Quaternion.AngleAxis(angle * t, normal) * (Vector3.right * radius);
        Vector3 pointOnArc = cent + radius*(Quaternion.AngleAxis(angle*t, Vector3.forward) * normal ).normalized;

        //Vector3 pointOnArc = cent + Quaternion.AngleAxis(angle , Vector3.up) * (Vector3.right * radius);

        return pointOnArc;
    }
    public override TypeGeom GetTypeGeom() { return TypeGeom.Circle; }

    public override float Length()
    {
        
        return Mathf.PI * radius * Mathf.Abs(angle) / 180;
    }
}

public class Intersector
{
    public List<Vector2> Intersect(MyClass m1,MyClass m2)
    {
        if (m1 is Line)
        {
            if (m2 is Line)
            {
                return Intersect((Line)m1, (Line)m2); 
            }
            else
            {
                return Intersect((Line)m1, (Circle)m2);
            }


        }
        else
        {
            if (m2 is Line)
            {
                return Intersect((Circle)m1, (Line)m2);
            }
            else
            {
                return Intersect((Circle)m1, (Circle)m2);
            }
        }
        
    }
    public List<Vector2> Intersect(Line l1, Line l2)
    {
        List<Vector2> list = new List<Vector2>();
        Vector2 dir1 = l1.dir();
        Vector2 dir2 = l2.dir();
        //float denominator = dir1.x * dir2.y - dir1.y * dir2.x;
        //Debug.Log("denom: " + denominator.ToString());
        //if (denominator != 0)
        //{
        //    //Vector2 diff = l2.pta - l1.pta;
        //    //float t = (diff.x * dir2.y - diff.y * dir2.x) / denominator;


        //    Debug.Log("t " + t.ToString());

        //    if (t >= 0 && t <= 1)
        //    {
        //        Vector2 intersectionPoint = l1.pta + t * dir1;
        //        list.Add(intersectionPoint);
        //    }
        //    else
        //    {

        //    }
        //}

        //float t = Vector3.Cross(l2.pta - l1.pta, dir2).magnitude / Vector3.Cross(dir1, dir2).magnitude;
        //float u = Vector3.Cross(l1.pta - l2.pta, dir1).magnitude / Vector3.Cross(dir2, dir1).magnitude;
        //if (t > 0 && t < 1 && u > 0 && u < 1)
        //{

        //    Vector3 intersection = l1.pta + t * dir1;
        //    list.Add(intersection);
        //}
        float det = dir1.x * dir2.y - dir1.y * dir2.x;

        if (det != 0)
        {
            float t = ((l2.pta.x - l1.pta.x) * dir2.y - (l2.pta.y - l1.pta.y) * dir2.x) / det;
            float u = ((l2.pta.x - l1.pta.x) * dir1.y - (l2.pta.y - l1.pta.y) * dir1.x) / det;

            if (t > 0 && t < 1 && u > 0 && u < 1)
            {
                list.Add ( l1.pta + t * dir1);
            }
        }
        return list;
    }
    public List<Vector2> Intersect(Line line, Circle circle)
    {

        List<Vector2> list = new List<Vector2>();

        Vector2 segmentDir = line.dir();
        Vector2 circleToSegmentStart = line.pta - circle.center;

        float a = Vector2.Dot(segmentDir, segmentDir);
        float b = 2f * Vector2.Dot(circleToSegmentStart, segmentDir);
        float c = Vector2.Dot(circleToSegmentStart, circleToSegmentStart) - circle.radius * circle.radius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant >= 0)
        {
            float t1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
            {
                Vector2 intersectionPoint1 = line.pta + t1 * segmentDir;
                
                list.Add(intersectionPoint1);
            }

            if (t2 >= 0 && t2 <= 1)
            {
                Vector2 intersectionPoint2 = line.pta + t2 * segmentDir;

                list.Add(intersectionPoint2);
            }
        }
        else
        {

        }
        return list;
    }

    public List<Vector2> Intersect(Circle circle, Line line)
    {
        return Intersect(line, circle);
    }

    public List<Vector2> Intersect(Circle circle1, Circle circle2)
    {
        List<Vector2> list = new List<Vector2>();
        float d = Vector2.Distance(circle1.center, circle2.center);
        //Debug.Log(d);
        Debug.Log(circle2.radius);
        if (d < circle1.radius + circle2.radius )
        {
            // 2 points
            //Debug.Log("qkfsrojgkd");
        }
        if(Mathf.Abs(d - (circle1.radius + circle2.radius)) < 0.001)
        {
            // 1 point
            Debug.Log("peqkfsrojgkd");
            list.Add( Vector2.Lerp(circle1.center,circle2.center, circle1.radius / (circle1.radius + circle2.radius)));
        }
        return list;
    }
}