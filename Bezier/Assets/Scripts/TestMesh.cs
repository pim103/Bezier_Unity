using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMesh : MonoBehaviour
{
    [SerializeField] private GameObject cube1;
    [SerializeField] private GameObject cube2;
    // Start is called before the first frame update
    void Start()
    {
        Mesh curveMesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = curveMesh;

        //cube1.transform.GetComponent<MeshFilter>().mesh.vertices[0]+= new Vector3(100,100,100);
        Vector3[] curveObject1Vertices = cube1.transform.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] curveObject2Vertices = cube2.transform.GetComponent<MeshFilter>().mesh.vertices;
        Debug.Log(curveObject1Vertices[0]);
        Debug.Log(curveObject1Vertices[1]);
        Debug.Log(curveObject2Vertices[0]);
        Debug.Log(cube1.transform.position+curveObject1Vertices[0]);
        Debug.Log(cube1.transform.position+curveObject1Vertices[1]);
        Debug.Log(cube2.transform.position+curveObject2Vertices[0]);
        Vector3[] vertices = new Vector3[]
        {
            /*new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,0)*/
            
            cube1.transform.position+curveObject1Vertices[0], 
            cube1.transform.position+curveObject1Vertices[1],
            cube2.transform.position+curveObject2Vertices[1]
        };

        int[] triangles = new int[]
        {
            0,1,2
        };
        
        curveMesh.Clear();
        curveMesh.vertices = vertices;
        curveMesh.triangles = triangles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
