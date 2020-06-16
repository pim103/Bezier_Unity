using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BezierController : MonoBehaviour
{
    [SerializeField] private GameObject prefabLine;
    [SerializeField] private GameObject controlPointPrefab;

    [SerializeField] private Slider sliderStep;
    [SerializeField] private Button buttonValidateStep;
    
    [SerializeField] private GameObject[] curveShapes;

    [SerializeField] private Toggle buttonRepeatControlPoint;
    [SerializeField] private Button buttonUseControlPoint;

    private List<Bezier> bezierList;
    private Bezier currentBezier;

    private GameObject selectedControlPoint;
    private Bezier selectedBezier;

    private Vector3 positionPointed;

    public static float step;

    private bool wantToRepeatControlPoint;
    private bool wantToUseControlPoint;

    private void Start()
    {
        step = sliderStep.value;
        bezierList = new List<Bezier>();
        buttonValidateStep.onClick.AddListener(ValidateStep);
        
        buttonRepeatControlPoint.onValueChanged.AddListener(ToggleRepeatControlPoint);
        buttonUseControlPoint.onClick.AddListener(UseControlPoint);
    }

    private void ValidateStep()
    {
        step = sliderStep.value;

        foreach (Bezier bezier in bezierList)
        {
            bezier.CalculPoints(curveShapes[0]);
        }
    }

    private void ToggleRepeatControlPoint(bool isOn)
    {
        wantToRepeatControlPoint = isOn;
    }

    private void UseControlPoint()
    {
        
    }

    private void Update()
    {
        UpdateMousePosition();
        
        if (Input.GetMouseButtonDown(0))
        {
            CheckObjectClicked();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentBezier != null && currentBezier.CheckBezierValid())
            {
                currentBezier.CalculPoints(curveShapes[0]);
                bezierList.Add(currentBezier);
                currentBezier = null;
            }
        }

        if (selectedControlPoint != null)
        {
            selectedControlPoint.transform.position = positionPointed;
        }
    }

    private void UpdateMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100, LayerMask.GetMask("grid")))
        {
            positionPointed = hitInfo.point;
        }
    }

    private void CheckObjectClicked()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int layerControlPoint = LayerMask.NameToLayer("controlPoint");
        int layerLineRenderer = LayerMask.NameToLayer("lineRenderer");
        int layerGrid = LayerMask.NameToLayer("grid");
        int layerUi = LayerMask.NameToLayer("UI");

        if (Physics.Raycast(ray, out hitInfo))
        {
            int layer = hitInfo.collider.gameObject.layer;

            if (selectedControlPoint != null)
            {
                selectedControlPoint = null;
                if (selectedBezier.bezierIsSet)
                {
                    selectedBezier.CalculPoints(curveShapes[0]);
                }
                selectedBezier = null;

                return;
            }
            
            if (layer == layerGrid)
            {
                AddPointToBezier();
            } else if (layer == layerControlPoint)
            {
                if (wantToRepeatControlPoint)
                {
                    Bezier bezier = GetBezierFromControlPoint(hitInfo.collider.gameObject);

                    if (bezier == null)
                    {
                        bezier = currentBezier;
                    }
                    
                    bezier.DuplicateControlPoint(hitInfo.collider.gameObject, Instantiate(controlPointPrefab), curveShapes[0]);
                }
                else
                {
                    SelectControlPoint(hitInfo.collider.gameObject);
                }
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

        GameObject newControlPoint = Instantiate(controlPointPrefab);
        newControlPoint.transform.position = positionPointed;
        currentBezier.AddControlPoints(newControlPoint);
    }

    private void SelectControlPoint(GameObject controlPoint)
    {
        selectedControlPoint = controlPoint;

        selectedBezier = GetBezierFromControlPoint(selectedControlPoint);

        if (selectedBezier == null)
        {
            selectedBezier = currentBezier;
        }
    }

    private Bezier GetBezierFromControlPoint(GameObject controlPoint)
    {
        foreach (Bezier bezier in bezierList)
        {
            if (bezier.GetControlPoints().Contains(controlPoint))
            {
                selectedBezier = bezier;
            }
        }

        return selectedBezier;
    }
}
