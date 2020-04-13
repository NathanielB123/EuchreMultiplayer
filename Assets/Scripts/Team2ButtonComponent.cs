using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team2ButtonComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Visible;
    private bool Selected;
    private GameObject LobbyManagerObject;
    private bool Unclicked = false;

    void Start()
    {
        SetVisible(false);
        LobbyManagerObject = GameObject.Find("LobbyManager");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Input.GetButton("Click") && Visible)
        {
            Unclicked = true;
        }
        if (Selected && Visible && Input.GetButton("Click") && Unclicked)
        {
            LobbyManagerObject.GetComponent<LobbyManagerComponent>().JoinTeam(1);
        }
    }

    public void SetVisible(bool NewVisible)
    {
        gameObject.GetComponent<Renderer>().enabled = NewVisible;
        Visible = NewVisible;
        GetComponent<CircleCollider2D>().enabled = NewVisible;
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
