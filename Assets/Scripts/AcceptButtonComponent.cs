﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcceptButtonComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private bool Visible;
    private bool Selected;
    private GameObject LobbyManagerObject;

    void Start()
    {
        LobbyManagerObject = GameObject.Find("LobbyManager");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Selected && Visible && Input.GetButton("Click"))
        {
            LobbyManagerObject.GetComponent<LobbyManagerComponent>().Accept();
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
