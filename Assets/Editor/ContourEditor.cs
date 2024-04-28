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

    bool needsRepaint = false;

    SelectionInfo selectionInfo;
    void OnSceneGUI()
    {
        Event guiEvent = Event.current;
        
        if (guiEvent.type == EventType.Repaint)
        {
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
            
        } else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        } else
        {
            
            HandleInput(guiEvent);
            if (needsRepaint)
            {
                needsRepaint = false;
                HandleUtility.Repaint();
            }
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

    void HandleInput(Event guiEvent)
    {
        // Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
		// float drawPlaneHeight = 0;
		// float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
		// Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDown(mousePos);
        }
        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseUp(mousePos);
        }
        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDrag(mousePos);
        }
        if (!selectionInfo.pointIsSelected)
        {
            updateMouseOverInfo(mousePos);
        }

        topContourPath.loop = Contour.forceClosed;


    }

    void HandleLeftMouseDown(Vector3 mousePosition){
        if (!selectionInfo.mouseIsOverPoint)
        {
            int newPointIndex = selectionInfo.mouseIsOverLine ? selectionInfo.lineIndex + 1 : Contour.points.Count; 
            Undo.RecordObject(creator, "Add segment");
            topContourPath.positionCount++;
            topContourPath.SetPosition(topContourPath.positionCount - 1, mousePosition);

            // Contour.AddPoint(mousePosition);
            Contour.points.Insert(newPointIndex,mousePosition);
            selectionInfo.pointIndex = newPointIndex;
             
        }
        selectionInfo.pointIsSelected = true;
        selectionInfo.positionAtStartOfDrag = mousePosition;
        needsRepaint = true;
    }
    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {
            Contour.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(creator,"Move Point");
            Contour.points[selectionInfo.pointIndex] = mousePosition;
            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            needsRepaint = true;
        }
    }
    void HandleLeftMouseDrag(Vector3 mousePosition){
        if (selectionInfo.pointIsSelected)
        {

            Contour.points[selectionInfo.pointIndex] = mousePosition;
            needsRepaint = true;
        }
    }
     void updateMouseOverInfo(Vector3 mousePosition){
        int mouseOverPointIndex = -1;
        for (int i = 0; i < Contour.points.Count; i++)
        {
            if (Vector3.Distance(mousePosition,Contour.points[i]) < creator.handleRadius )
            {
                mouseOverPointIndex = i;
                break;
            }
        }
        if (mouseOverPointIndex != selectionInfo.pointIndex)
        {
            
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;
            needsRepaint = true;
        }
        if (selectionInfo.mouseIsOverPoint)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        } else
        {
            int mouseIsOverLineIndex = -1;
            float closestLineDst = creator.handleRadius;
            for (int i = 0; i < Contour.points.Count; i++)
            {
              Vector3 nextPoint = Contour.points[(i+1)%Contour.points.Count];
              float dstFromMouseToLine = HandleUtility.DistancePointLine(mousePosition,Contour.points[i],nextPoint);  
              if (dstFromMouseToLine < closestLineDst)
              {
                mouseIsOverLineIndex = i;
              }
            }
            if (selectionInfo.lineIndex != mouseIsOverLineIndex)
            {
                selectionInfo.lineIndex = mouseIsOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseIsOverLineIndex != -1 ;
                needsRepaint = true;
            }
        }
    }

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
        for (int i = 0; i < pathG.Count; i++)
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
                tIndex = (t - pos) * total / pathG[i].Length();
                break;
            }
        }
        Vector3 toolPos;
        toolPos = pathG[index].Lerp(tIndex);
        return toolPos;
    }

    private void FindSelfCuttingIntersection()
    {
        pathCorrected.Clear();
        intersection.Clear();
        int j2 = -1;
        Vector2 ptintersection = Vector2.zero;

        Approche app = Contour.typeApproche switch
        {
            TypeApproche.Perpendicular => new PerpendicularApproch(),
            TypeApproche.Circle => new CircularApproch(),
            _ => null,
        };
        if (app != null)
        {
            //pathCorrected.AddRange(app.calculateApproche(path[0],Contour.offsetDirection));
            List<Geometry> approcheEl = app.calculateApproche(path[0],Contour.offsetDirection);
            foreach (Geometry item in approcheEl)
            {
                pathCorrected.Add(item);
            }
        }
        //

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
                    if (Contour.selfCuttingIntersecr)
                    {
                        Handles.color = Color.blue;
                        for (int k = 0; k < intersection.Count; k++)
                        {
                            Handles.DrawWireDisc(intersection[k], Vector3.forward, 0.1f);
                            Handles.Label(intersection[k], k.ToString());
                        }
                    }
                    ptintersection = intersection[0];
                        
                    
                    j2 = j;
                    break;
                }

            }
            Geometry g2 = (Geometry)path[i].Clone();
            if(intersection.Count > 0 && i < j2)
            {
                // modifier le point de fin du cercle
                g2.modifyEndPoint(ptintersection);
            }
            if(i == j2)
            {
                // modifier le point de debut du cercle
                g2.modifyBeginPoint(ptintersection);
            }
            pathCorrected.Add(g2);
        


        }



        if (Contour.PathN)
        {
            for (int i = 0; i < pathCorrected.Count; i++)
            {
                 if (i == 0 && Contour.typeApproche != TypeApproche.None)
                {
                    Handles.color = Color.cyan;
                } else
                {
                    Handles.color = Color.magenta;
                }
                pathCorrected[i].Draw();
            }
        }
    }
    void Draw()
    {
        for (int i = 0; i < Contour.points.Count; i++)
        {
            //affiche les lignes verticales
            Vector3 p = Contour.points[i];
            Vector3 p2 = p;
            p2.z = Contour.depth;
            Handles.color = Color.black;
            Handles.DrawLine(p, p2);

            if (i == 0)
            {
                Handles.color = Color.green;
            }
            else
            {
                Handles.color = Color.red;
            }
            if (i == selectionInfo.pointIndex)
            {
                Handles.color =  selectionInfo.pointIsSelected ?Color.white : Color.blue;
            }
            Vector2 newPos = Handles.FreeMoveHandle(Contour[i], .1f, Vector2.zero, Handles.CylinderHandleCap);
            Handles.DrawSolidDisc(Contour.points[i],Vector3.forward,creator.handleRadius);
        
        }

        int numLines = Contour.forceClosed ? Contour.points.Count : Contour.points.Count - 1;
        for (int i = 0; i < numLines; i++)
        {

            topContourPath.SetPosition(i, Contour.points[i]);

            if (i == selectionInfo.lineIndex)
            {
                Handles.color = Color.red;
            } else {
                Handles.color = Color.black;
            }
            Handles.DrawLine(Contour.points[i], Contour.points[(i + 1 ) % Contour.points.Count]);


            Vector3 vec3a = Contour.points[i];
            vec3a.z = Contour.depth;
            Vector3 vec3b = Contour.points[(i + 1) % Contour.points.Count];
            vec3b.z = Contour.depth;

            Handles.DrawLine(vec3a, vec3b);
            


        }

        Vector2 direction = Contour.points[1] - Contour.points[0];
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
                    float angleprevDirection = -1 * Vector2.SignedAngle(previousDirection, direction);
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

        // approche decalé
        if (Contour.isClosed())
        {
            switch (path[0].GetTypeGeom())
            {
                case TypeGeom.Line:
                    Line l = (Line)path[0];
                    Line l1 = new( l.Lerp(0.5f), l.ptb);
                    Line l2 = new(l.pta,l.Lerp(0.5f));
                    path[0] = l1;
                    path.Add(l2);
                break;
                default:
                    throw new NotImplementedException();
                break;
            }
        }
       

        FindSelfCuttingIntersection();
        


        if (Contour.basicPath)
        {
            Handles.color = Color.yellow;
            for (int i = 0; i < path.Count; i++)
            {
               
                path[i].Draw();
            }
        }
        needsRepaint = false;

    }

    public void removeLastPoint()
    {
        Contour.removeLastPoint();
    }
   
    void OnEnable()
    {
        selectionInfo = new();
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

    public class SelectionInfo
    {
        public int pointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
        public Vector3 positionAtStartOfDrag;


        public int lineIndex = -1;
        public bool mouseIsOverLine;

    }
}

