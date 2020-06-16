using UnityEngine;
using System.Collections;
 
public class MovementCamera : MonoBehaviour {
    
    float mainSpeed = 10.0f;
    private Vector3 lastMouse = new Vector3(255, 255, 255);
     
    void Update () {
        lastMouse = Input.mousePosition - lastMouse ;
        lastMouse = new Vector3(-lastMouse.y * 0.25f, lastMouse.x * 0.25f, 0 );
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        
        p = p * mainSpeed;
        
       
        p = p * Time.deltaTime;
        transform.Translate(p);

    }
     
    private Vector3 GetBaseInput() {
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.Z)){
            p_Velocity += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S)){
            p_Velocity += Vector3.back;
        }
        if (Input.GetKey(KeyCode.Q)){
            p_Velocity += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D)){
            p_Velocity += Vector3.right;
        }
        return p_Velocity;
    }
}