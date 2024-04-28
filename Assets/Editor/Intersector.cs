using System;
using System.Collections.Generic;
using UnityEngine;

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

