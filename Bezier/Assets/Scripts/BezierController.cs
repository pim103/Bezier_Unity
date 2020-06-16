using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierController : MonoBehaviour
{
    [SerializeField] private GameObject prefabLine;
    [SerializeField] private GameObject controlPointPrefab;
    
    private List<Bezier> bezierList;
    private Bezier currentBezier;

    private IEnumerator CalculPoints()
    {
        while (true)
        {
            foreach (Bezier bezier in bezierList)
            {
                bezier.ShowBezier();
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private void Start()
    {
        bezierList = new List<Bezier>();
        StartCoroutine(CalculPoints());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AddPointToBezier();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentBezier != null && currentBezier.CheckBezierValid())
            {
                currentBezier.CalculPoints();
                bezierList.Add(currentBezier);
                currentBezier = null;
            }
        }
    }

    private void AddPointToBezier()
    {
        if (currentBezier == null)
        {
            GameObject newLine = Instantiate(prefabLine);
            currentBezier = new Bezier(newLine);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        
        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject newControlPoint = Instantiate(controlPointPrefab);
            newControlPoint.transform.position = hitInfo.point;
            currentBezier.AddControlPoints(newControlPoint);
        }
    }
}
