using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum TypeApproche
{
    None,
    Perpendicular,
    Circle
}
[System.Serializable]
public abstract class Approche 
{
    public abstract  List<Geometry> calculateApproche(Geometry g,OffsetDir decalage);
    abstract public TypeApproche GetTypeApproche();
}

[System.Serializable]
public class PerpendicularApproch : Approche
{
    [SerializeField]
    public float distance = 0.2f;

    public override List<Geometry> calculateApproche(Geometry g,OffsetDir decalage)
    {
        Vector2 normal = g.getNormal(decalage).normalized;
        switch (g.GetTypeGeom())
        {
            case TypeGeom.Line:
                Line l = (Line)g;
                Line approche = new(l.pta + normal * distance ,l.pta);
 
                List<Geometry> list = new()
                {
                    approche
                };
                return list;
            default:
                throw new System.NotImplementedException();
            //break;
        }
    }

    public override TypeApproche GetTypeApproche()
    {
        return TypeApproche.Perpendicular;
    }
}

[System.Serializable]
public class CircularApproch : Approche
{
    float radius = 0.5f;
    float angle = 180f;
    public override List<Geometry> calculateApproche(Geometry g, OffsetDir decalage)
    {
        Circle c ;
        Vector2 normal = g.getNormal(decalage).normalized;
        
        switch (g.GetTypeGeom())
        {
            case TypeGeom.Line:
                Line l = (Line)g;
                Vector2 centre = l.pta + normal * radius;
                float a = decalage == OffsetDir.Left ? angle : -angle;
                Vector2 normalCercle = Quaternion.AngleAxis(180- a,Vector3.forward) * g.getNormal(decalage);
                c = new(centre,normalCercle,a,radius);
                List<Geometry> list = new()
                {
                    c
                };
                return list; 
            

            default:
            throw new NotImplementedException();
        }

        throw new System.NotImplementedException();
    }

    public override TypeApproche GetTypeApproche()
    {
        return TypeApproche.Circle;
    }
}