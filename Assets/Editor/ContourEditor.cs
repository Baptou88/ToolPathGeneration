using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ContourCreator))]
public class ContourEditor : Editor
{

    ContourCreator creator;
    Contour Contour;
    public LineRenderer topContourPath;
    public LineRenderer toolPath;

    public Intersector intersect = new Intersector();
    List<Vector2> intersection = new();

    public List<Geometry> path = new List<Geometry>();

    public List<Geometry> pathCorrected = new List<Geometry>();

    void OnSceneGUI()
    {
        Input();
        Draw();
        if (Contour.basicPath)
        {
            Vector2 toolPos = GetPosAtT(path, Contour.t);
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(toolPos, Vector3.forward, Contour.diameter);
        }

        if (Contour.PathN)
        {
            Vector2 toolPosN = GetPosAtT(pathCorrected, Contour.t);
            Handles.color = Color.magenta;
            Handles.DrawWireDisc(toolPosN, Vector3.forward, Contour.diameter);
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (GUILayout.Button("Reverse Path"))
        {
            creator.reversePath();
        }
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Undo.RecordObject(creator, "Add segment");
            topContourPath.positionCount++;
            topContourPath.SetPosition(topContourPath.positionCount - 1, mousePos);

            Contour.AddSegment(mousePos);
        }



        topContourPath.loop = Contour.forceClosed;


    }

    //public float lengthContourN()
    //{

    //    float length = 0.0f;
    //    for (int i = 0; i < path.Count; i++)
    //    {
    //        length += path[i].Length();
    //    }
    //    return length;
    //}

    public float lengthContour(List<Geometry> pathe)
    {

        float length = 0.0f;
        for (int i = 0; i < pathe.Count; i++)
        {
            length += pathe[i].Length();
        }
        return length;
    }


    public Vector2 GetPosAtT(List<Geometry> pathG, float t)
    {
        float total = lengthContour(pathG);
        float pos = 0;
        float nextPos = 0;
        int index = 0;
        float tIndex = 0;
        for (int i = 0; i < path.Count; i++)
        {
            nextPos += pathG[i].Length() / total;
            if (t > nextPos)
            {
                pos = nextPos;
                continue;
            }
            else
            {
                index = i;
                //Debug.Log(i);
                tIndex = (t - pos) * total / pathG[i].Length();
                break;
            }
        }
        Vector3 toolPos;

        toolPos = pathG[index].Lerp(tIndex);
        return toolPos;
        //Handles.color = Color.yellow;
        //Handles.DrawWireDisc(toolPos, Vector3.forward, Contour.diameter);
        //if (creator.cylindre != null)
        //{
        //    creator.cylindre.transform.position = toolPos;
        //}
        //Handles.color = Color.blue;
    }

    private void FindSelfCuttingIntersection()
    {
        pathCorrected.Clear();
        intersection.Clear();
        int j2 = -1;
        Vector2 ptintersection = Vector2.zero;
        for (int i = 0; i < path.Count; i++)
        {
            if (i < j2)
            {
                continue;
            }
            for (int j = path.Count - 1; j > i + 1; j--)
            {
                intersection.Clear();
                intersection = intersect.Intersect(path[i], path[j]);
                if (intersection.Count > 0)
                {
                    
                    if (intersection.Count>1)
                    {
                         intersection = findNearestIntersectionPt(path[i], intersection);
                    }
                    Handles.color = Color.blue;
                    for (int k = 0; k < intersection.Count; k++)
                    {
                        Handles.DrawWireDisc(intersection[k], Vector3.forward, 0.1f);
                        Handles.Label(intersection[k], k.ToString());
                    }
                    ptintersection = intersection[0];
                        
                    
                    j2 = j;
                    break;
                }

            }
            switch (path[i].GetTypeGeom())
            {
                case TypeGeom.Line:
                    Line l = (Line)path[i].Clone();
                    if (intersection.Count > 0 && i < j2)
                    {
                        l.ptb = ptintersection;
                    }
                    if (i == j2)
                    {
                        l.pta = ptintersection;
                        j2 = -1;
                    }
                    pathCorrected.Add(l);

                    break;
                case TypeGeom.Circle:
                    Circle c = (Circle)path[i].Clone();
                    
                    if(intersection.Count > 0 && i !=j2)
                    {
                        // modifier le point de fin du cercle
                        c.modifyEndPoint(ptintersection);
                    }
                    if(i == j2)
                    {
                        // modifier le point de debut du cercle
                        c.modifyBeginPoint(ptintersection);
                    }
                    pathCorrected.Add(c);
                    break;
                default:
                    pathCorrected.Add(path[i]);
                    break;
            }

        }

        Vector2 normal = pathCorrected[0].getNormal(Contour.offsetDirection).normalized;
        switch (pathCorrected[0].GetTypeGeom())
        {
            case TypeGeom.Line:
                Line l = (Line)pathCorrected[0];
                Handles.color = Color.cyan;
                Handles.DrawLine(l.pta, l.pta + normal * 1);
            break;
            default:

            break;
        }

        if (Contour.PathN)
        {
            for (int i = 0; i < pathCorrected.Count; i++)
            {
                Handles.color = Color.magenta;
                pathCorrected[i].Draw();
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
        int numLines = Contour.forceClosed ? Contour.points.Count : Contour.points.Count - 1;
        for (int i = 0; i < numLines; i++)
        {

            topContourPath.SetPosition(i, Contour.points[i]);

            Handles.color = Color.black;
            if (i >= Contour.points.Count - 1)
            {
                Handles.DrawLine(Contour.points[i], Contour.points[0]);

                Vector3 vec3a = Contour.points[i];
                vec3a.z = Contour.depth;
                Vector3 vec3b = Contour.points[0];
                vec3b.z = Contour.depth;
                Handles.DrawLine(vec3a, vec3b);
            }
            else
            {
                Handles.DrawLine(Contour.points[i], Contour.points[i + 1]);


                Vector3 vec3a = Contour.points[i];
                vec3a.z = Contour.depth;
                Vector3 vec3b = Contour.points[i + 1];
                vec3b.z = Contour.depth;

                Handles.DrawLine(vec3a, vec3b);
            }


        }

        // affiche les points du contour et gere leurs deplacements
        for (int i = 0; i < Contour.NumPoints; i++)
        {
            if (i == 0)
            {
                Handles.color = Color.green;
            }
            else
            {
                Handles.color = Color.red;

            }
            Vector2 newPos = Handles.FreeMoveHandle(Contour[i], .1f, Vector2.zero, Handles.CylinderHandleCap);
            
            if (Contour[i] != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                Contour.MovePoint(i, newPos);
                topContourPath.SetPosition(i, newPos);
            }
        }



        Vector2 direction = (Contour.points[1] - Contour.points[0]);
        Vector2 normal = new Vector2(-direction.y, direction.x).normalized;

        //with tool offset
        Vector2 prevPoint = new Vector2();
        bool prevIsCircle = false;

        path.Clear();


        for (int i = 0 ; i < numLines ; i++)
        {
            //Vector2 previousPoint = i == 0 ? Contour.points[Contour.points.Count - 1] : Contour.points[i - 1];
            //Vector2 Point = Contour.points[i];
            //Vector2 nextPoint = i == Contour.points.Count - 1 ? Contour.points[0] : Contour.points[i + 1];
            //Vector2 nextnextPoint = Contour.points[(i + 2) % Contour.points.Count];

            
            Vector2 previousPoint =     Contour.points[(i - 1 + Contour.points.Count + Contour.startVertex) % Contour.points.Count];
            Vector2 Point =             Contour.points[(i + 0 + Contour.points.Count + Contour.startVertex) % Contour.points.Count];
            Vector2 nextPoint =         Contour.points[(i + 1 + Contour.points.Count + Contour.startVertex) % Contour.points.Count];
            Vector2 nextnextPoint =     Contour.points[(i + 2 + Contour.points.Count + Contour.startVertex) % Contour.points.Count];


            direction = nextPoint - Point;

            if (Contour.offsetDirection == OffsetDir.Left)
            {
                normal = new Vector2(-direction.y, direction.x).normalized;
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
                if (i == 0 && Contour.forceClosed)
                {
                    Vector2 previousDirection = Point - previousPoint;
                    float angleprevDirection = (-1 * Vector2.SignedAngle(previousDirection, direction));
                    if ((Contour.offsetDirection == OffsetDir.Left && angleprevDirection < 0) || (Contour.offsetDirection == OffsetDir.Right && angleprevDirection > 0) || !creator.SmoothConvexe )
                    {

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
                }
                prevIsCircle = false;
                prevPoint = newA;
            }

            Handles.color = Color.magenta;



            Vector2 direction2 = nextnextPoint - nextPoint;
            float angle = (-1 * Vector2.SignedAngle(direction, direction2));

            //Handles.Label(nextPoint, angle.ToString());

            if ((Contour.offsetDirection == OffsetDir.Left && angle > 0 && creator.SmoothConvexe) || (Contour.offsetDirection == OffsetDir.Right && angle < 0 && creator.SmoothConvexe))
            {
                path.Add(new Line(prevPoint, newB));
                if (!Contour.forceClosed && i == Contour.points.Count - 2)
                {

                }
                else
                {
                    path.Add(new Circle(nextPoint, normal, -angle, Contour.diameter));
                }


                prevPoint = newA;
                prevIsCircle = true;
            }
            else
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
                float angle2 = Vector2.Angle(bissector, direction2);
                float d = Contour.diameter / Mathf.Cos((2 * Mathf.PI / 360) * angle2);

                Vector2 p = nextPoint + bissectorNormal * d;

                if (!Contour.forceClosed && i == Contour.points.Count - 2)
                {
                    p = newB;
                }
                Vector2 direction3 = (p - prevPoint);

                if (Vector2.Dot(direction.normalized, direction3) < .0f)
                {
                    Handles.color = Color.cyan;

                }
                //Handles.DrawLine(prevPoint, p);
                path.Add(new Line(prevPoint, p));
                prevPoint = p;
            }


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

        if (creator.topContourPath == null)
        {
            Debug.Log("pas de line renderer");
        }
        topContourPath = creator.topContourPath;

        if (creator.toolPath == null)
        {
            Debug.Log("pas de line renderer");
        }
        toolPath = creator.toolPath;



        Vector3[] points = new Vector3[Contour.points.Count];
        topContourPath.positionCount = Contour.points.Count;

        for (int i = 0; i < Contour.points.Count; i++)
        {
            points[i] = Contour.points[i];
        }
        topContourPath.SetPositions(points);
    }

    public List<Vector2> findNearestIntersectionPt(Geometry g, List<Vector2> p)
    {
        if (p.Count>2)
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






public class Intersector
{
    public List<Vector2> Intersect(Geometry m1, Geometry m2)
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
                list.Add(l1.pta + t * dir1);
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
                if (PointisInArcCircle(circle,intersectionPoint1))
                {
                    list.Add(intersectionPoint1);
                }
                
            }

            if (t2 >= 0 && t2 <= 1)
            {
                Vector2 intersectionPoint2 = line.pta + t2 * segmentDir;
                if (PointisInArcCircle(circle,intersectionPoint2))
                {
                    list.Add(intersectionPoint2);
                }
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

        if (d < circle1.radius + circle2.radius)
        {
            // 2 points
            double a = (circle1.radius * circle1.radius - circle2.radius * circle2.radius + d * d) / (2 * d);
            double h = Math.Sqrt(circle1.radius * circle1.radius - a * a);

            // Find P2.
            double cx2 = circle1.center.x + a * (circle2.center.x - circle1.center.x) / d;
            double cy2 = circle1.center.y + a * (circle2.center.y - circle1.center.y) / d;

            // Get the points P3.
            Vector2 intersection1 = new Vector2((float)(cx2 + h * (circle2.center.y - circle1.center.y) / d), (float)(cy2 - h * (circle2.center.x - circle1.center.x) / d));
            Vector2 intersection2 = new Vector2((float)(cx2 - h * (circle2.center.y - circle1.center.y) / d), (float)(cy2 + h * (circle2.center.x - circle1.center.x) / d));

            if (PointisInArcCircle(circle1,intersection1))
            {
                list.Add(intersection1);
            }
            if (PointisInArcCircle(circle1, intersection2))
            {
                list.Add(intersection2);
            }


        }
        if (Mathf.Abs(d - (circle1.radius + circle2.radius)) < 0.001)
        {
            // 1 point

            list.Add(Vector2.Lerp(circle1.center, circle2.center, circle1.radius / (circle1.radius + circle2.radius)));
        }
        return list;
    }

    public bool PointisInArcCircle(Circle c, Vector2 p)
    {
        
        float angleDebut = Vector2.SignedAngle(c.normal,  p-c.center);
        float angleFin = Vector2.SignedAngle(c.endDirection(), p-c.center);

        //Handles.DrawLine(c.center, c.center + c.endDirection());
        //Debug.Log(angleFin + " " + angleDebut);
        //return true;
        if (c.angle > 0)
        {
            if (angleDebut < c.angle && angleFin < c.angle)
            {
                return true;
            }
            return false;
        }
        else
        {
            if (angleDebut > c.angle && angleFin > c.angle)
            {
                return true;
            }
            return false;
        }
    }
}

