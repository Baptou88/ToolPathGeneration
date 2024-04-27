using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public enum TypeGeom
{
    None,
    Line,
    Circle
}

public abstract class Geometry 
{
    abstract public Vector2 Lerp(float t);
    abstract public void Draw();
    abstract public TypeGeom GetTypeGeom();

    abstract public float Length();

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    abstract public Vector2 getNormal(OffsetDir dir);
    
}

public class Line : Geometry
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

    public override Vector2 Lerp(float t)
    {
        return Vector2.Lerp(pta, ptb, t);
    }

    public override Vector2 getNormal(OffsetDir dir)
    {
        Vector2 d = ptb - pta;
        if (dir == OffsetDir.Right)
        {
            return new Vector2(d.y,-d.x);
        }
        return new Vector2(-d.y,d.x);
    }
}


public class Circle : Geometry
{
    public Vector2 center;
    public float radius;
    public float angle;
    public Vector2 normal;
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
        //Handles.DrawDottedLine(center, center + endDirection(), 0.1f);
    }

    public bool PointOnArc(Vector2 p)
    {
        Vector2 a = p - center;
        float angle = Vector2.Angle(normal, a);

        return angle < this.angle;
    }
    public Vector2 endDirection()
    {
        return (Quaternion.AngleAxis(angle, Vector3.forward) * normal).normalized * radius;
    }
    public Vector2 endPoint()
    {
        return center + endDirection();
    }
    public override Vector2 Lerp(float t)
    {
        Vector3 cent = center;
        //Vector3 pointOnArc = cent + Quaternion.AngleAxis(angle * t, normal) * (Vector3.right * radius);
        Vector3 pointOnArc = cent + radius * (Quaternion.AngleAxis(angle * t, Vector3.forward) * normal).normalized;

        //Vector3 pointOnArc = cent + Quaternion.AngleAxis(angle , Vector3.up) * (Vector3.right * radius);

        return pointOnArc;
    }
    public override TypeGeom GetTypeGeom() { return TypeGeom.Circle; }

    public override float Length()
    {

        return Mathf.PI * radius * Mathf.Abs(angle) / 180;
    }

    public void modifyEndPoint(Vector2 ptintersection)
    {
        float angle = Vector2.Angle(normal, ptintersection - this.center);
        if (this.angle > 0)
        {
            this.angle = angle;
            return;
        }
        this.angle = -angle;
        //throw new NotImplementedException();
    }

    public void modifyBeginPoint(Vector2 ptintersection)
    {

        //calcul angle entre normal et ptintersection
        float angleModif = Vector2.SignedAngle(this.normal, ptintersection - this.center);



        if (this.angle > 0)
        {
            this.angle -= angleModif;

        }
        else
        {
            this.angle -= angleModif; /// a voir 
        }
        this.normal = ptintersection - this.center;
        Handles.DrawDottedLine(this.center, this.center + this.normal, 0.1f);

    }

    public override Vector2 getNormal(OffsetDir dir)
    {
        throw new System.NotImplementedException();
    }
}

