using System;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    private void OnMouseOver()
    {
        meshRenderer.material.color = Color.blue;
    }

    private void OnMouseExit()
    {
        meshRenderer.material.color = Color.white;
    }
}
