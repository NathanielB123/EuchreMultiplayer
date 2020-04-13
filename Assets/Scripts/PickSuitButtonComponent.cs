using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickSuitButtonComponent : MonoBehaviour
{
    private bool Selected;
    private GameObject GameManagerObject;
    private string Suit;
    private bool Unclicked;
    // Start is called before the first frame update
    void Start()
    {
        GameManagerObject = GameObject.Find("GameManager");
        Unclicked = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Input.GetButton("Click")) //Used to prevent clicks hitting both the PickButton and the PickSuitButton
        {
            Unclicked = true;
        }
        if (Selected && Input.GetButton("Click") && Unclicked)
        {
            GameManagerObject.GetComponent<GameManagerComponent>().PickTrumpSuit(Suit);
        }
    }

    public void SetSuit(string NewSuit)
    {
        Dictionary<string, Sprite> PickSuitButtonImages = new Dictionary<string, Sprite> { { "C", Resources.Load<Sprite>("Images/PickClubs") },
        { "D", Resources.Load<Sprite>("Images/PickDiamonds") },{ "S", Resources.Load<Sprite>("Images/PickSpades") },
        { "H", Resources.Load<Sprite>("Images/PickHearts") }};
        Suit = NewSuit;
        GetComponent<SpriteRenderer>().sprite = PickSuitButtonImages[Suit];
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
