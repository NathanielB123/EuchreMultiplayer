using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicButtonComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Visible;
    private bool Selected;
    private bool Unclicked=false;
    private GameObject LobbyManagerObject;

    void Start()
    {
        Visible = false;
        gameObject.GetComponent<Renderer>().enabled = false;
        LobbyManagerObject = GameObject.Find("LobbyManager");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Input.GetButton("Click") &&Visible) //Used to prevent clicks hitting both the PickButton and the PickSuitButton
        {
            Unclicked = true;
        }
        if (Selected && Visible && Input.GetButton("Click")&& Unclicked)
        {
            LobbyManagerObject.GetComponent<LobbyManagerComponent>().Public();
        }
    }

    public void SetVisible(bool NewVisible)
    {
        gameObject.GetComponent<Renderer>().enabled = NewVisible;
        Visible = NewVisible;
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
