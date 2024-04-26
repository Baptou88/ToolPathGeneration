using TMPro;
using UnityEngine;

public class ContourCreator : MonoBehaviour
{

    public LineRenderer topContourPath;
    public LineRenderer toolPath;
    public GameObject cylindre;
    public Contour contour;
    public TextMeshProUGUI text;
    public bool SmoothConvexe;

    public void CreateContour()
    {
        contour = new Contour(transform.position);
    }

    public void Update()
    {
        if (text != null)
        {
            text.text = contour.points.Count.ToString();
        }
    }

    public void onDeleteButton()
    {
        contour.removeLastPoint();
        //Vector3[] allPoints ;

        //int a = cuttingPath.GetPositions(positions: allPoints);
        //allPoints.ToArray().;

    }

    public void reversePath()
    {
        contour.points.Reverse();
    }
}