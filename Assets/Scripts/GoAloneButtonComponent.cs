using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoAloneButtonComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Visible;
    private bool Selected;
    private GameObject GameManagerObject;

    void Start()
    {
        Visible = false;
        gameObject.GetComponent<Renderer>().enabled = false;
        GameManagerObject = GameObject.Find("GameManager");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Selected && Visible && Input.GetButton("Click"))
        {
            GameManagerObject.GetComponent<GameManagerComponent>().GoAloneButtonClicked();
        }
    }

    public void SetVisible(bool NewVisible)
    {
        gameObject.GetComponent<Renderer>().enabled = NewVisible;
        Visible = NewVisible;
    }

    public void SetSelected(bool NewSelected)
    {
        Selected = NewSelected;
    }

    void OnMouseEnter()
    {
        Selected = true;
    }

    void OnMouseExit()
    {
        Selected = false;
    }
}
