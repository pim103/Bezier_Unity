using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Curve
{
    private GameObject lineGameObject;
    private LineRenderer lineRenderer;

    private List<GameObject> controlPoints;
    private List<Vector3> calculatedPoints;

    public bool curveIsSet = false;

    public List<GameObject> curveObjects;

    public int selectedShape;
    public float uValue;
    public float vValue;
    public int cutNb;
    
    public Curve(GameObject line)
    {
        lineGameObject = line;
        lineRenderer = line.GetComponent<LineRenderer>();
        
        controlPoints = new List<GameObject>();
        calculatedPoints = new List<Vector3>();
        curveObjects = new List<GameObject>();
    }
    
    public void AddControlPoints(GameObject controlPoint)
    {
        controlPoints.Add(controlPoint);
    }

    public List<GameObject> GetControlPoints()
    {
        return controlPoints;
    }

    public bool CheckCurveValid()
    {
        return controlPoints.Count > 2;
    }

    public void CalculPointsBezier(GameObject curveShape)
    {
        int nbPoints = controlPoints.Count;

        float tSubdiv = 0;
	
        calculatedPoints.Clear();

        List<Vector3> controlsPointVector = PositionOfControlPoints();

//        calculatedPoints.Add(controlsPointVector[0]);

        List<Vector3> points;

        for (float t = 0; t < 1; t += CurveController.step) {
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

            if (calculatedPoints.Count > 0 && calculatedPoints[calculatedPoints.Count - 1] == points[0])
            {
                break;
            }

            calculatedPoints.Add(points[0]);
        }

        calculatedPoints.Add(controlsPointVector[controlsPointVector.Count - 1]);

        curveIsSet = true;
        ShowCurve();
        ExtrudeCurve(curveShape);
    }

    public void CalculPointsChaikin(GameObject curveShape)
    {
        int nbPoints = controlPoints.Count;
        calculatedPoints.Clear();
        List<Vector3> controlsPointVector = PositionOfControlPoints();
        Vector3 firstControlPoint = controlsPointVector[0];
        Vector3 lastControlPoint = controlsPointVector[controlsPointVector.Count-1];

        for (int k = 0; k < 3; ++k)
        {
            calculatedPoints.Clear();
            nbPoints = controlsPointVector.Count;
            for (int i = 0; i < nbPoints - 1; ++i)
            {
                calculatedPoints.Add(new Vector3(
                    controlsPointVector[i].x + ((controlsPointVector[i + 1].x - controlsPointVector[i].x) * uValue),
                    -50,
                    controlsPointVector[i].z + ((controlsPointVector[i + 1].z - controlsPointVector[i].z) * uValue)));
                calculatedPoints.Add(new Vector3(
                    controlsPointVector[i].x +
                    ((controlsPointVector[i + 1].x - controlsPointVector[i].x) * (1 - vValue)),
                    -50,
                    controlsPointVector[i].z +
                    ((controlsPointVector[i + 1].z - controlsPointVector[i].z) * (1 - vValue))));
            }
            controlsPointVector.Clear();
            controlsPointVector.AddRange(calculatedPoints);
        }
        calculatedPoints.Insert(0,firstControlPoint);
        calculatedPoints.Add(lastControlPoint);
        curveIsSet = true;
        ShowCurve();
        //ShowPoints(curveShape);
        //ExtrudeCurve(curveShape);
        Debug.Log("done");
    }

    private void ShowCurve()
    {
        lineRenderer.positionCount = calculatedPoints.Count;
        lineRenderer.loop = false;

        for (int i = 0; i < calculatedPoints.Count; ++i)
        {
            lineRenderer.SetPosition(i, calculatedPoints[i]);
        }
    }
    private void ShowPoints(GameObject curveShape)
    {
        for (int i = 0; i < calculatedPoints.Count; ++i)
        {
            GameObject newShape = Object.Instantiate(curveShape, calculatedPoints[i], Quaternion.identity);
        }
    }

    private void ExtrudeCurve(GameObject curveShape)
    {
        foreach (var curveObject in curveObjects)
        {
            curveObject.SetActive(false);
        }
        curveObjects = new List<GameObject>();
        for (int i = 1; i < calculatedPoints.Count-1; i++)
        {
            GameObject newShape = Object.Instantiate(curveShape, calculatedPoints[i], Quaternion.identity);
            curveObjects.Add(newShape);

            Vector3 tangent = calculatedPoints[i-1] - calculatedPoints[i];
            newShape.transform.rotation = Quaternion.LookRotation(tangent.normalized,Vector3.right);
            float zScale = Vector3.Distance(calculatedPoints[i-1], calculatedPoints[i])*10;
            if (zScale > 20)
            {
                zScale = 20;
            }
            newShape.transform.localScale = new Vector3(newShape.transform.localScale.x,newShape.transform.localScale.y, zScale);
        }
        
        /*Mesh curveMesh = new Mesh();
        lineRenderer.gameObject.AddComponent<MeshFilter>();
        lineRenderer.gameObject.GetComponent<MeshFilter>().mesh = curveMesh;

        Vector3[] curveObject1Vertices = curveObjects[0].transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] curveObject2Vertices = curveObjects[1].transform.GetChild(0).GetComponent<MeshFilter>().mesh.vertices;
        
        Vector3[] vertices = new Vector3[]
        {
            curveObject1Vertices[0],
            curveObject1Vertices[1],
            curveObject2Vertices[0]
        };

        int[] triangles = new int[]
        {
            0,1,2
        };
        
        curveMesh.Clear();
        curveMesh.vertices = vertices;
        curveMesh.triangles = triangles;*/
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

    public void DuplicateControlPoint(GameObject controlPointToDuplicate, GameObject newControlPoint, GameObject curveShape)
    {
        newControlPoint.transform.position = controlPointToDuplicate.transform.position;

        int index = controlPoints.IndexOf(controlPointToDuplicate);
        
        if (index == -1)
        {
            return;
        }
        
        controlPoints.Insert(controlPoints.IndexOf(controlPointToDuplicate), newControlPoint);

        if (curveIsSet)
        {
            CalculPointsBezier(curveShape);
        }
    }
}