using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathCreator : MonoBehaviour
{


    public Path path;

    public void CreatePath()
    {
        path = new Path(transform.position);
    }
}