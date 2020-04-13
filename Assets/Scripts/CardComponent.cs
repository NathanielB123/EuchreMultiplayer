using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardComponent : MonoBehaviour
{
    private readonly Dictionary<string, string> Types = new Dictionary<string, string>() { { "9", "9" }, { "10", "T" }, { "jack", "J" },
        { "queen", "Q" }, { "king", "K" }, { "ace", "A" }};
    private readonly Dictionary<string, string> Suits = new Dictionary<string, string>() { {"diamonds","D"}, { "hearts", "H" }, { "spades", "S" },
        {"clubs","C"}};
    private Dictionary<string, Sprite> CardImages = new Dictionary<string, Sprite>();
    private Vector3 TargetPosition = new Vector3();
    private bool Selected = false;
    private bool Dragging = false;
    private Vector3 OldPosition = new Vector3();
    private Camera CameraObject;
    private string CardType;
    private GameObject GameManagerObject;
    private List<bool> Unlocked = new List<bool> {false,false,false }; //0 - Hand, 1 - Discard, 2 - Play
    void Start()
    {
        CameraObject = FindObjectOfType<Camera>();
        GameManagerObject = GameObject.Find("GameManager");
        if (CardImages.Count == 0)
        {
            BuildCardDictionary();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Unlocked.Contains(true))
        {
            if (Input.GetButton("Click"))
            {
                if (Dragging)
                {
                    Vector3 Temp = CameraObject.ScreenToWorldPoint(Input.mousePosition);
                    Temp.z = -9;
                    SetTargetPosition(Temp);
                }
                else if (Selected && !CameraObject.GetComponent<CameraComponent>().GetDragging())
                {
                    OldPosition = new Vector3(TargetPosition.x, TargetPosition.y, TargetPosition.z);
                    transform.position = new Vector3(transform.position.x, transform.position.y, -9);
                    Dragging = true;
                    CameraObject.GetComponent<CameraComponent>().SetDragging(true);
                }
            }
            else if (Dragging)
            {
                Dragging = false;
                CameraObject.GetComponent<CameraComponent>().SetDragging(false);
                float Distance1 = Mathf.Sqrt(Mathf.Pow(transform.position.x-1050, 2) + Mathf.Pow(transform.position.y + 600, 2));
                float Distance2 = Mathf.Sqrt(Mathf.Pow(transform.position.x+1500, 2) + Mathf.Pow(transform.position.y + 500, 2));
                float Distance3 = Mathf.Sqrt(Mathf.Pow(GameManagerObject.GetComponent<GameManagerComponent>().GetPlayedCardsCount() * 100 - transform.position.x, 2) + Mathf.Pow(transform.position.y - 300, 2));
                if (Unlocked[0] && Distance1 < 400)
                {
                    TargetPosition = new Vector3(1050, -500, -6);
                    Unlocked = new List<bool> { false, false, false };
                } 
                else if (Unlocked[1] && Distance2 < 200)
                {
                    TargetPosition = new Vector3(-1500, -500, -1);
                    Unlocked = new List<bool> { false, false, false };
                    GameManagerObject.GetComponent<GameManagerComponent>().Discarded(CardType);
                }
                else if (Unlocked[2] && Distance3 < 200 && GameManagerObject.GetComponent<GameManagerComponent>().CheckIfValid(CardType))
                {
                    TargetPosition = new Vector3(-100 + GameManagerObject.GetComponent<GameManagerComponent>().GetPlayedCardsCount() * 100, 300, 
                        GameManagerObject.GetComponent<GameManagerComponent>().GetPlayedCardsCount() * -1 - 1);
                    Unlocked = new List<bool> { false, false, false };
                }
                else
                {
                    TargetPosition = OldPosition;
                }
                if (!Selected)
                {
                    TargetPosition = new Vector3(TargetPosition.x, TargetPosition.y - 100, TargetPosition.z);
                }
            }
        }
        MoveToTarget();
    }

    public void BuildCardDictionary()
    {
        foreach (string Type in Types.Keys)
        {
            foreach (string Suit in Suits.Keys)
            {
                CardImages.Add(Types[Type] + Suits[Suit], Resources.Load<Sprite>("Images/Cards/" + Type + "_of_" + Suit));
            }
        }
        CardImages.Add("CB", Resources.Load<Sprite>("Images/Cards/card_back"));
    }
    public void SetCard(string Card)
    {
        CardType = Card;
        GetComponent<SpriteRenderer>().sprite = CardImages[Card];
    }

    public string GetCardType()
    {
        return CardType;
    }

    public void SetTargetPosition(Vector3 NewTargetPosition)
    {
        TargetPosition = NewTargetPosition;
    }

    public Vector3 GetTargetPosition()
    {
        return TargetPosition;
    }

    void MoveToTarget()
    {
        transform.position += new Vector3((TargetPosition.x - transform.position.x) / 5,
            (TargetPosition.y - transform.position.y) / 5,
            (TargetPosition.z - transform.position.z) / 5);
    }

    void OnMouseEnter()
    {
        if (Unlocked.Contains(true))
        {
            Selected = true;
            TargetPosition.y += 100;
        }
    }

    void OnMouseExit()
    {
        if (Unlocked.Contains(true))
        {
            Selected = false;
            TargetPosition.y -= 100;
            if (TargetPosition.y == -700)
            {
                TargetPosition.y = -600;
            }
        }
    }

    public void SetUnlocked(List<bool> NewLocked)
    {
        Unlocked = NewLocked;
    }
}
