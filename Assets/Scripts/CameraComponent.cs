using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Dragging;
    void Start()
    {
        Dragging = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDragging(bool NewValue)
    {
        Dragging = NewValue;
    }
    
    public bool  GetDragging()
    {
        return Dragging;
    }
}
