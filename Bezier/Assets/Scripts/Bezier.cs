using System.Collections.Generic;
using UnityEngine;

public class Bezier
{
    private GameObject lineGameObject;
    private LineRenderer lineRenderer;

    private List<GameObject> controlPoints;
    private List<Vector3> calculatedPoints;

    public bool bezierIsSet = false;

    public Bezier(GameObject line)
    {
        lineGameObject = line;
        lineRenderer = line.GetComponent<LineRenderer>();
        
        controlPoints = new List<GameObject>();
        calculatedPoints = new List<Vector3>();
    }
    
    public void AddControlPoints(GameObject controlPoint)
    {
        controlPoints.Add(controlPoint);
    }

    public List<GameObject> GetControlPoints()
    {
        return controlPoints;
    }

    public bool CheckBezierValid()
    {
        return controlPoints.Count > 2;
    }

    public void CalculPoints()
    {
        int nbPoints = controlPoints.Count;

        float tSubdiv = 0;
	
        calculatedPoints.Clear();

        List<Vector3> controlsPointVector = PositionOfControlPoints();

        calculatedPoints.Add(controlsPointVector[0]);

        List<Vector3> points;

        for (float t = 0; t < 1; t += BezierController.step) {
            points = controlsPointVector;

            for (int j = 1; j < nbPoints; j++) {
                for (int i = 0; i < nbPoints - j; i++) {
                    points[i] = new Vector3(
                        (1 - t) * points[i].x + t * points[i + 1].x,
                        -50,
                        (1 - t) * points[i].z + t * points[i + 1].z
                    );
                }
            }

            calculatedPoints.Add(points[0]);
        }

        calculatedPoints.Add(controlsPointVector[controlsPointVector.Count - 1]);

        bezierIsSet = true;
        ShowBezier();
    }

    public void ShowBezier()
    {
        lineRenderer.positionCount = calculatedPoints.Count;
        lineRenderer.loop = false;

        for (int i = 0; i < calculatedPoints.Count; ++i)
        {
            lineRenderer.SetPosition(i, calculatedPoints[i]);
        }
    }

    private List<Vector3> PositionOfControlPoints()
    {
        List<Vector3> positions = new List<Vector3>();
        controlPoints.ForEach(point =>
        {
            Vector3 pos = point.transform.position;
            pos.y = -50;
            positions.Add(pos);
        });

        return positions;
    }
}