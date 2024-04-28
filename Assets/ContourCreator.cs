using TMPro;
using UnityEngine;

public class ContourCreator : MonoBehaviour
{

    public LineRenderer topContourPath;
    public LineRenderer toolPath;
    public GameObject cylindre;
    public TextMeshProUGUI text;
    public Contour contour;
    public bool SmoothConvexe;

    private GameObject selectedSphere;
    private bool isDragging;

    public float handleRadius = 0.5f;
    public void CreateContour()
    {
        contour = new Contour(transform.position);
    }

    public void Start()
    {
        foreach (Vector2 p in contour.points)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 positionSphere = p;
            sphere.tag = "Sphere";
            sphere.transform.position =  positionSphere;
            sphere.transform.parent = transform ;
        }
        
    }
    public void Update()
    {
        if (text != null)
        {
            text.text = contour.points.Count.ToString();
        }



        // Détecter le clic gauche de la souris
        if (Input.GetMouseButtonDown(0))
        {
            // Lancer un rayon depuis la position de la souris
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Si le rayon touche un objet
            if (Physics.Raycast(ray, out hit))
            {
                // Vérifier si l'objet touché est une sphère
                if (hit.collider.gameObject.CompareTag("Sphere"))
                {
                    selectedSphere = hit.collider.gameObject;
                    isDragging = true;
                }
            }
        } 
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && selectedSphere != null)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPos.z = selectedSphere.transform.position.z;
            Vector2 posSphere = selectedSphere.transform.position;
            for (int i = 0; i < contour.points.Count; i++)
            {
                if (posSphere == contour.points[i])
                {
                    contour.MovePoint(i,newPos);
                    topContourPath.SetPosition(i,newPos);
                    
                }
            }
            selectedSphere.transform.position = newPos;
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