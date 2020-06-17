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
    
    [SerializeField] private Button circleButton;
    [SerializeField] private Button squareButton;

    private List<Bezier> bezierList;
    private Bezier currentBezier;

    private GameObject selectedControlPoint;
    private Bezier selectedBezier;

    private Vector3 positionPointed;

    public static float step;

    private bool wantToRepeatControlPoint;
    private bool wantToUseControlPoint;

    private int selectedCurveMeshIndex;
    private bool curveMode = true;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject movementCamera;

    private void Start()
    {
        step = sliderStep.value;
        bezierList = new List<Bezier>();
        buttonValidateStep.onClick.AddListener(ValidateStep);
        
        buttonRepeatControlPoint.onValueChanged.AddListener(ToggleRepeatControlPoint);
        buttonUseControlPoint.onClick.AddListener(UseControlPoint);
        circleButton.onClick.AddListener(delegate { SelectCurveMesh(0); });
        squareButton.onClick.AddListener(delegate { SelectCurveMesh(1); });
    }

    private void SelectCurveMesh(int index)
    {
        selectedCurveMeshIndex = index;
    }
    private void ValidateStep()
    {
        step = sliderStep.value;

        foreach (Bezier bezier in bezierList)
        {
            bezier.CalculPoints(curveShapes[bezier.selectedShape]);
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            curveMode = !curveMode;
            if (!curveMode)
            {
                mainCamera.SetActive(false);
                movementCamera.SetActive(true);
            }
            else
            {
                mainCamera.SetActive(true);
                movementCamera.SetActive(false);
            }
        }

        if (!curveMode)
        {
            return;
        }
        UpdateMousePosition();
        
        if (Input.GetMouseButtonDown(0))
        {
            CheckObjectClicked();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentBezier != null && currentBezier.CheckBezierValid())
            {
                currentBezier.selectedShape = selectedCurveMeshIndex;
                currentBezier.CalculPoints(curveShapes[currentBezier.selectedShape]);
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
                    selectedBezier.CalculPoints(curveShapes[selectedBezier.selectedShape]);
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

                    bezier.DuplicateControlPoint(hitInfo.collider.gameObject, Instantiate(controlPointPrefab), curveShapes[bezier.selectedShape]);
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
