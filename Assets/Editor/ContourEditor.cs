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
                Vector2 toolPos = GetPosAtT(Contour.path, Contour.t);
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(toolPos, Vector3.forward, Contour.diameter);
            }

            if (Contour.PathN)
            {
                Vector2 toolPosN = GetPosAtT(Contour.pathCorrected, Contour.t);
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

        int numLines = Contour.getNumLines();
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


        creator.contour.calculBasicPath();

        
       creator.contour.calculateApproche();

        Contour.calculCorrectedPath();
        


        if (Contour.basicPath)
        {
            Handles.color = Color.yellow;
            for (int i = 0; i < Contour.path.Count; i++)
            {
               
                Contour.path[i].Draw();
            }
        }

        if (Contour.PathN)
        {
            for (int i = 0; i < Contour.pathCorrected.Count; i++)
            {
                 if (i == 0 && Contour.typeApproche != TypeApproche.None)
                {
                    Handles.color = Color.cyan;
                } else
                {
                    Handles.color = Color.magenta;
                }
                Contour.pathCorrected[i].Draw();
            }
        }

        needsRepaint = false;

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

