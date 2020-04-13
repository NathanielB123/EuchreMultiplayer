using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManagerComponent : MonoBehaviour
{
    private GameObject InputFieldObject;
    private GameObject TextObject;
    private GameObject MainTextObject;
    private GameObject AcceptButtonObject;
    private GameObject PublicButtonObject;
    private GameObject PrivateButtonObject;
    private GameObject PlaceholderObject;
    private GameObject Team1ButtonObject;
    private GameObject Team2ButtonObject;
    private string Name = "";
    private string RoomID = "";
    private int TeamAttempt = 0;
    // Start is called before the first frame update
    void Start()
    {
        AcceptButtonObject = GameObject.Find("AcceptButton");
        InputFieldObject= GameObject.Find("InputField");
        TextObject= GameObject.Find("Text");
        PlaceholderObject = GameObject.Find("Placeholder");
        MainTextObject = GameObject.Find("MainText");
        PublicButtonObject = GameObject.Find("PublicGameButton");
        PrivateButtonObject = GameObject.Find("PrivateGameButton");
        Team1ButtonObject = GameObject.Find("Team1Button");
        Team2ButtonObject = GameObject.Find("Team2Button");
        AcceptButtonObject.GetComponent<AcceptButtonComponent>().SetVisible(true);
    }

    // Update is called once per frame

    public int GetTeamAttempt()
    {
        return TeamAttempt;
    }

    public string GetName()
    {
        return Name;
    }

    public string GetRoomID()
    {
        return RoomID;
    }

    public void Accept()
    {
        if (Name.Length==0)
        {
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "";
            AcceptButtonObject.GetComponent<AcceptButtonComponent>().SetVisible(false);
            AcceptButtonObject.GetComponent<CircleCollider2D>().enabled = false;
            if (InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text.Length != 0)
            {
                Name = InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text;
                TextObject.GetComponent<UnityEngine.UI.Text>().enabled = false;
                InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text = "";
            }
            else
            {
                Name = "Anonymous";
            }
            PlaceholderObject.GetComponent<UnityEngine.UI.Text>().enabled = false;
            InputFieldObject.GetComponent<UnityEngine.UI.Image>().enabled = false;
            PublicButtonObject.GetComponent<PublicButtonComponent>().SetVisible(true);
            PrivateButtonObject.GetComponent<PrivateButtonComponent>().SetVisible(true);
        }
        else
        {
            if (InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text.Length !=0)
            {
                AcceptButtonObject.GetComponent<AcceptButtonComponent>().SetVisible(false);
                RoomID = InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text;
                TextObject.GetComponent<UnityEngine.UI.Text>().enabled = false;
                InputFieldObject.GetComponent<UnityEngine.UI.InputField>().text = "";
                PlaceholderObject.GetComponent<UnityEngine.UI.Text>().enabled = false;
                InputFieldObject.GetComponent<UnityEngine.UI.Image>().enabled = false;
                MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Select team to join (if it is full you will be automatically placed in the other team)";
                Team1ButtonObject.GetComponent<Team1ButtonComponent>().SetVisible(true);
                Team2ButtonObject.GetComponent<Team2ButtonComponent>().SetVisible(true);
            }
            else
            {
                MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Please enter a room ID code.";
            }
        }
    }
    public void Private()
    {
        PublicButtonObject.GetComponent<PublicButtonComponent>().SetVisible(false);
        PrivateButtonObject.GetComponent<PrivateButtonComponent>().SetVisible(false);
        MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Enter room ID code (users who enter the same code are placed in the same game).";
        InputFieldObject.GetComponent<UnityEngine.UI.Image>().enabled = true;        
        TextObject.GetComponent<UnityEngine.UI.Text>().enabled = true;
        PlaceholderObject.GetComponent<UnityEngine.UI.Text>().enabled = true;
        AcceptButtonObject.GetComponent<AcceptButtonComponent>().SetVisible(true);
    }
    public void Public()
    {
        PublicButtonObject.GetComponent<PublicButtonComponent>().SetVisible(false);
        PrivateButtonObject.GetComponent<PrivateButtonComponent>().SetVisible(false);
        MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Attempting to join an open public game...";
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadScene("EuchreGame");
    }

    public void JoinTeam(int Team)
    {
        TeamAttempt = Team;
        Team1ButtonObject.GetComponent<Team1ButtonComponent>().SetVisible(false);
        Team2ButtonObject.GetComponent<Team2ButtonComponent>().SetVisible(false);
        MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Attempting to join room of ID code: " + RoomID;
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("EuchreGame");
    }
}
