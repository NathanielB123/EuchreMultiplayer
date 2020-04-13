using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team1ButtonComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Visible;
    private bool Selected;
    private GameObject LobbyManagerObject;

    void Start()
    {
        SetVisible(false);
        LobbyManagerObject = GameObject.Find("LobbyManager");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Selected && Visible && Input.GetButton("Click"))
        {
            LobbyManagerObject.GetComponent<LobbyManagerComponent>().JoinTeam(0);
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
