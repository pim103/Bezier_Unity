using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CurveController : MonoBehaviour
{
    [SerializeField] private GameObject prefabLine;
    [SerializeField] private GameObject controlPointPrefab;

    [SerializeField] private Slider sliderStep;
    [SerializeField] private Button buttonValidateStep;
    
    [SerializeField] private GameObject[] curveShapes;

    [SerializeField] private Toggle buttonRepeatControlPoint;
    [SerializeField] private Toggle toggleBezierMode;
    [SerializeField] private Toggle toggleChaikinMode;
    [SerializeField] private Button buttonUseControlPoint;
    
    [SerializeField] private Button circleButton;
    [SerializeField] private Button squareButton;
    [SerializeField] private Button resetButton;
    
    [SerializeField] private InputField uInput;
    [SerializeField] private InputField vInput;
    [SerializeField] private InputField cutInput;

    private List<Curve> curveList;
    private Curve currentCurve;

    private GameObject selectedControlPoint;
    private Curve selectedCurve;

    private Vector3 positionPointed;

    public static float step;

    private bool wantToRepeatControlPoint;
    private bool bezierMode;
    private bool chaikinMode;
    private bool wantToUseControlPoint;

    private int selectedCurveMeshIndex;
    private bool curveMode = true;
    private bool wantToStartFromcontrolPoint;

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject movementCamera;

    private void Start()
    {
        step = sliderStep.value;
        curveList = new List<Curve>();
        buttonValidateStep.onClick.AddListener(ValidateStep);
        
        buttonRepeatControlPoint.onValueChanged.AddListener(ToggleRepeatControlPoint);
        toggleBezierMode.onValueChanged.AddListener(ToggleBezierMode);
        toggleChaikinMode.onValueChanged.AddListener(ToggleChaikinMode);
        buttonUseControlPoint.onClick.AddListener(UseControlPoint);
        circleButton.onClick.AddListener(delegate { SelectCurveMesh(0); });
        squareButton.onClick.AddListener(delegate { SelectCurveMesh(1); });
        resetButton.onClick.AddListener(delegate { SceneManager.LoadScene("SampleScene"); });
        /*uInput.onValueChanged.AddListener(ChangeUValue);
        uInput.onValueChanged.AddListener(ChangeVValue);*/
    }

    private void SelectCurveMesh(int index)
    {
        selectedCurveMeshIndex = index;
    }
    private void ValidateStep()
    {
        step = sliderStep.value;

        foreach (Curve bezier in curveList)
        {
            bezier.CalculPointsBezier(curveShapes[bezier.selectedShape]);
        }
    }

    private void ToggleRepeatControlPoint(bool isOn)
    {
        wantToRepeatControlPoint = isOn;
    }
    private void ToggleBezierMode(bool isOn)
    {
        bezierMode = isOn;
    }
    private void ToggleChaikinMode(bool isOn)
    {
        chaikinMode = isOn;
    }
    private void ChangeUValue(string input)
    {
        currentCurve.uValue = float.Parse(input);
    }
    private void ChangeVValue(string input)
    {
        currentCurve.vValue = float.Parse(input);
    }

    private void UseControlPoint()
    {
        wantToStartFromcontrolPoint = true;
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
            if (bezierMode && currentCurve != null && currentCurve.CheckCurveValid())
            {
                currentCurve.selectedShape = selectedCurveMeshIndex;
                currentCurve.CalculPointsBezier(curveShapes[currentCurve.selectedShape]);
                curveList.Add(currentCurve);
                currentCurve = null;
            }
            if (chaikinMode && currentCurve != null && currentCurve.CheckCurveValid())
            {
                currentCurve.selectedShape = selectedCurveMeshIndex;
                currentCurve.uValue = float.Parse(uInput.text);
                currentCurve.vValue = float.Parse(vInput.text);
                currentCurve.cutNb = int.Parse(cutInput.text);
                currentCurve.CalculPointsChaikin(curveShapes[currentCurve.selectedShape]);
                curveList.Add(currentCurve);
                currentCurve = null;
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

        if (Physics.Raycast(ray, out hitInfo, 1000, LayerMask.GetMask("grid","controlPoint")))
        {
            int layer = hitInfo.collider.gameObject.layer;

            if (selectedControlPoint != null)
            {
                selectedControlPoint = null;
                if (selectedCurve.curveIsSet)
                {
                    selectedCurve.CalculPointsBezier(curveShapes[selectedCurve.selectedShape]);
                }
                selectedCurve = null;

                return;
            }

            if (layer == layerGrid)
            {
                AddPointToBezier(positionPointed);
            } else if (layer == layerControlPoint)
            {
                if (wantToRepeatControlPoint)
                {
                    Curve curve = GetBezierFromControlPoint(hitInfo.collider.gameObject);

                    if (curve == null)
                    {
                        curve = currentCurve;
                    }

                    curve.DuplicateControlPoint(hitInfo.collider.gameObject, Instantiate(controlPointPrefab), curveShapes[curve.selectedShape]);
                } 
                else if (wantToStartFromcontrolPoint)
                {
                    wantToStartFromcontrolPoint = false;
                    AddSameControlPointToBezier(hitInfo.collider.gameObject);
                }
                else
                {
                    SelectControlPoint(hitInfo.collider.gameObject);
                }
            }
        }
    }

    private void AddPointToBezier(Vector3 position)
    {
        if (currentCurve == null)
        {
            GameObject newLine = Instantiate(prefabLine);
            currentCurve = new Curve(newLine);
        }

        GameObject newControlPoint = Instantiate(controlPointPrefab);
        newControlPoint.transform.position = position;
        currentCurve.AddControlPoints(newControlPoint);
    }

    private void AddSameControlPointToBezier(GameObject controlPoint)
    {
        if (currentCurve == null)
        {
            GameObject newLine = Instantiate(prefabLine);
            currentCurve = new Curve(newLine);
        }

        currentCurve.AddControlPoints(controlPoint);
    }

    private void SelectControlPoint(GameObject controlPoint)
    {
        selectedControlPoint = controlPoint;

        selectedCurve = GetBezierFromControlPoint(selectedControlPoint);

        if (selectedCurve == null)
        {
            selectedCurve = currentCurve;
        }
    }

    private Curve GetBezierFromControlPoint(GameObject controlPoint)
    {
        foreach (Curve bezier in curveList)
        {
            if (bezier.GetControlPoints().Contains(controlPoint))
            {
                selectedCurve = bezier;
            }
        }

        return selectedCurve;
    }
}
