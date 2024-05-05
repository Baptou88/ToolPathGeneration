using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ContourCreator))]
public class ContourEditor : Editor
{

    ContourCreator creator;

    public LineRenderer topContourPath;
    public LineRenderer toolPath;


    bool contourChangedSinceLastRepaint = false;

    SelectionInfo selectionInfo;
    void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();


        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {

            HandleInput(guiEvent);
            if (contourChangedSinceLastRepaint)
            {
                contourChangedSinceLastRepaint = false;
                HandleUtility.Repaint();
            }
        }

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (GUILayout.Button("Reverse Path"))
        {
            SelectedContour.points.Reverse();
            contourChangedSinceLastRepaint = true;
        }
    }


    void HandleInput(Event guiEvent)
    {

        // Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        // float drawPlaneHeight = 0;
        // float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        // Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleLeftShiftMouseDown(mousePos);
        }
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDown(mousePos);
        }
        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
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

        //topContourPath.loop = Contour.forceClosed;


    }

    void HandleLeftShiftMouseDown(Vector3 pos)
    {
        if (selectionInfo.mouseIsOverPoint)
        {
            SelectContourUnderMouse();
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewContour();
            createNewPoint(pos);
        }
    }
    void CreateNewContour()
    {
        Undo.RecordObject(creator, "Create Contour");
        creator.contour.Add(new Contour(Vector2.zero));
        selectionInfo.selectedContourIndex = creator.contour.Count - 1;

    }
    void createNewPoint(Vector3 mousePosition)
    {
        bool mouseIsOverSelectedContour = selectionInfo.mouseOverContourIndex == selectionInfo.selectedContourIndex;
        int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedContour) ? selectionInfo.lineIndex + 1 : SelectedContour.points.Count;
        Undo.RecordObject(creator, "Add point at index " + newPointIndex);
        SelectedContour.points.Insert(newPointIndex, mousePosition);
        selectionInfo.pointIndex = newPointIndex;
        selectionInfo.mouseOverContourIndex = selectionInfo.selectedContourIndex;
        contourChangedSinceLastRepaint = true;
        SelectPointUnderMouse();
    }
    void SelectPointUnderMouse()
    {
        selectionInfo.pointIsSelected = true;
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;
        selectionInfo.positionAtStartOfDrag = SelectedContour.points[selectionInfo.pointIndex];
        contourChangedSinceLastRepaint = true;
    }
    void SelectContourUnderMouse()
    {
        if (selectionInfo.mouseOverContourIndex != -1)
        {
            selectionInfo.selectedContourIndex = selectionInfo.mouseOverContourIndex;
            contourChangedSinceLastRepaint = true;
        }
    }
    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (creator.contour.Count == 0)
        {

            CreateNewContour();
        }

        SelectContourUnderMouse();

        if (selectionInfo.mouseIsOverPoint)
        {
            SelectPointUnderMouse();

            // Contour.AddPoint(mousePosition);
            //topContourPath.positionCount++;
            //topContourPath.SetPosition(topContourPath.positionCount - 1, mousePosition);
        }
        else
        {
            createNewPoint(mousePosition);

        }

    }

    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {
            SelectedContour.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(creator, "Move Point");
            SelectedContour.points[selectionInfo.pointIndex] = mousePosition;

            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            contourChangedSinceLastRepaint = true;
        }
    }
    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (selectionInfo.pointIsSelected)
        {

            SelectedContour.points[selectionInfo.pointIndex] = mousePosition;
            contourChangedSinceLastRepaint = true;
        }
    }
    void updateMouseOverInfo(Vector3 mousePosition)
    {
        int mouseOverPointIndex = -1;
        int mouseOverContourIndex = -1;
        for (int contourIndex = 0; contourIndex < creator.contour.Count; contourIndex++)
        {
            Contour ct = creator.contour[contourIndex];
            for (int i = 0; i < ct.points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, ct.points[i]) < creator.handleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverContourIndex = contourIndex;
                    break;
                }
            }
        }
        if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverContourIndex != selectionInfo.mouseOverContourIndex)
        {
            selectionInfo.mouseOverContourIndex = mouseOverContourIndex;
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;
            contourChangedSinceLastRepaint = true;
        }
        if (selectionInfo.mouseIsOverPoint)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        }
        else
        {
            int mouseIsOverLineIndex = -1;
            float closestLineDst = creator.handleRadius;
            for (int contourIndex = 0; contourIndex < creator.contour.Count; contourIndex++)
            {
                Contour ct = creator.contour[contourIndex];
                for (int i = 0; i < ct.points.Count; i++)
                {
                    Vector3 nextPoint = ct.points[(i + 1) % ct.points.Count];
                    float dstFromMouseToLine = HandleUtility.DistancePointLine(mousePosition, ct.points[i], nextPoint);
                    if (dstFromMouseToLine < closestLineDst)
                    {
                        closestLineDst = dstFromMouseToLine;
                        mouseIsOverLineIndex = i;
                        mouseOverContourIndex = contourIndex;
                    }
                }
            }
            if (selectionInfo.lineIndex != mouseIsOverLineIndex || mouseOverContourIndex != selectionInfo.mouseOverContourIndex)
            {
                selectionInfo.mouseOverContourIndex = mouseOverContourIndex;
                selectionInfo.lineIndex = mouseIsOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseIsOverLineIndex != -1;
                contourChangedSinceLastRepaint = true;
            }
        }

    }

    void DeletePointUnderMouse()
    {
        Undo.RecordObject(creator, "DeletePoint");
        SelectedContour.points.RemoveAt(selectionInfo.pointIndex);
        selectionInfo.pointIsSelected = false;
        selectionInfo.mouseIsOverPoint = false;
        contourChangedSinceLastRepaint = true;
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
        for (int contourIndex = 0; contourIndex < creator.contour.Count; contourIndex++)
        {
            Contour ct = creator.contour[contourIndex];
            bool contourIsSelected = contourIndex == selectionInfo.selectedContourIndex;
            bool mouseIsOverContour = contourIndex == selectionInfo.mouseOverContourIndex;
            for (int i = 0; i < ct.points.Count; i++)
            {
                //affiche les lignes verticales
                Vector3 p = ct.points[i];
                Vector3 p2 = p;
                p2.z = ct.depth;
                Handles.color = Color.black;
                Handles.DrawLine(p, p2);

                // if (i == 0)
                // {
                //     Handles.color = Color.green;
                // }
                // else
                // {
                //     Handles.color = Color.red;
                // }
                if (i == selectionInfo.pointIndex && mouseIsOverContour)
                {
                    Handles.color = selectionInfo.pointIsSelected ? Color.white : Color.blue;
                }
                else
                {
                    Handles.color = contourIsSelected ? Color.green : Color.grey;
                }
                Vector2 newPos = Handles.FreeMoveHandle(ct[i], .1f, Vector2.zero, Handles.CylinderHandleCap);
                Handles.DrawSolidDisc(ct.points[i], Vector3.forward, creator.handleRadius);

            }


            int numLines = ct.getNumLines();
            for (int i = 0; i < numLines; i++)
            {

                //topContourPath.SetPosition(i, ct.points[i]);

                if (i == selectionInfo.lineIndex && mouseIsOverContour)
                {
                    Handles.color = Color.red;
                }
                else
                {
                    Handles.color = Color.black;
                }
                Handles.DrawLine(ct.points[i], ct.points[(i + 1) % ct.points.Count]);


                Vector3 vec3a = ct.points[i];
                vec3a.z = ct.depth;
                Vector3 vec3b = ct.points[(i + 1) % ct.points.Count];
                vec3b.z = ct.depth;

                Handles.DrawLine(vec3a, vec3b);



            }


            ct.calculBasicPath();


            ct.calculateApproche();

            ct.calculCorrectedPath();



            if (ct.basicPath)
            {
                Handles.color = Color.yellow;
                for (int i = 0; i < ct.path.Count; i++)
                {

                    ct.path[i].Draw();
                }
            }

            if (ct.PathN)
            {
                for (int i = 0; i < ct.pathCorrected.Count; i++)
                {
                    if (i == 0 && ct.typeApproche != TypeApproche.None)
                    {
                        Handles.color = Color.cyan;
                    }
                    else
                    {
                        Handles.color = Color.magenta;
                    }
                    ct.pathCorrected[i].Draw();
                }
            }


            if (ct.points.Count > 2)
            {


                if (ct.basicPath)
                {
                    Vector2 toolPos = GetPosAtT(ct.path, ct.t);
                    Handles.color = Color.yellow;
                    Handles.DrawWireDisc(toolPos, Vector3.forward, ct.diameter);
                }

                if (ct.PathN)
                {
                    Vector2 toolPosN = GetPosAtT(ct.pathCorrected, ct.t);
                    Handles.color = Color.magenta;
                    Handles.DrawWireDisc(toolPosN, Vector3.forward, ct.diameter);
                }
            }
            if (contourChangedSinceLastRepaint)
            {
                creator.UpdateMeshRenderer();
            }
            contourChangedSinceLastRepaint = false;
        }
    }


    void OnEnable()
    {
        contourChangedSinceLastRepaint = true;
        creator = (ContourCreator)target;

        selectionInfo = new();
        // if (creator.contour == null)
        // {
        //     creator.CreateContour();
        // }

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



        // Vector3[] points = new Vector3[Contour.points.Count];
        // topContourPath.positionCount = Contour.points.Count;

        // for (int i = 0; i < Contour.points.Count; i++)
        // {
        //     points[i] = Contour.points[i];
        // }
        // topContourPath.SetPositions(points);
    }


    Contour SelectedContour
    {
        get
        {
            return creator.contour[selectionInfo.selectedContourIndex];
        }
    }
    public class SelectionInfo
    {
        public int selectedContourIndex;
        public int mouseOverContourIndex;
        public int pointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
        public Vector3 positionAtStartOfDrag;


        public int lineIndex = -1;
        public bool mouseIsOverLine;

    }
}

