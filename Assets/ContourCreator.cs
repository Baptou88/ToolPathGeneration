using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ContourCreator : MonoBehaviour
{

    public LineRenderer cuttingPath;
    public GameObject cylindre;
    public Contour contour;

    public void CreateContour()
    {
        contour = new Contour(transform.position);
    }
}