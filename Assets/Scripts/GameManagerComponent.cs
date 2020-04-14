using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;


//Note, this was my first proper programming using C sharp and OOP and therefore the code is very messy as I tried to learn what to do, I'll probably try and clean it up at some point.
public class GameManagerComponent : MonoBehaviourPunCallbacks
{
    private readonly List<string> Types = new List<string>() { "9", "T", "J", "Q", "K", "A" };
    private readonly List<string> Suits = new List<string>() { "D", "H", "S", "C" };
    private List<string> Deck = new List<string>();
    private List<string> Hands = new List<string>();
    private List<string> PlayedCards = new List<string>();
    private List<GameObject> DisplayedCards = new List<GameObject>();
    private readonly List<GameObject> PickSuitButtons = new List<GameObject>();
    private GameObject Card;
    private GameObject PickSuitButton;
    private GameObject MainTextObject;
    private GameObject StatsTextObject;
    private bool Host;
    private int PlayerNum;
    private int Players;
    private bool Loaded;
    private int Turn;
    private int Dealer;
    private int Team1Score;
    private int Team2Score;
    private int Team1Tricks;
    private int Team2Tricks;
    private char Trump;
    private int TrumpPickerTeam;
    private int Cycled;
    private GameObject PassButtonObject;
    private GameObject PickButtonObject;
    private GameObject GoAloneButtonObject;
    private GameObject LobbyManagerObject;
    private readonly Dictionary<char, char> AlternateSuits = new Dictionary<char, char> { {'S', 'C'}, {'C', 'S' }, { 'H', 'D' }, { 'D', 'H' } };
    private int Ticks =0;
    private readonly Queue<List<string>> Waits = new Queue<List<string>>();
    private Dictionary<int,string> PlayerNames = new Dictionary<int,string>();
    private bool Skip;
    private string PlayerID;
    private int PlayerToSkip = -1;
    private readonly List<int> PlayersJoined = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        Card = Resources.Load<GameObject>("Prefabs/Card");
        PickSuitButton = Resources.Load<GameObject>("Prefabs/PickSuitButton");
        MainTextObject = GameObject.Find("MainText");
        StatsTextObject = GameObject.Find("StatsText");
        PassButtonObject = GameObject.Find("PassButton");
        PickButtonObject = GameObject.Find("PickButton");
        GoAloneButtonObject = GameObject.Find("GoAloneButton");
        LobbyManagerObject = GameObject.Find("LobbyManager");
        Host = false;
        Loaded = false;
        PhotonNetwork.ConnectUsingSettings();
        Team1Score = 0;
        Team2Score = 0;
    }

    void FixedUpdate()
    {
        if (Waits.Count>0)
        {
            Ticks += 1;
            if (Ticks>120)
            {
                Skip = true;
                if (Waits.Peek()[0]=="Score")
                {
                    ScoreTrick();
                }
                else if (Waits.Peek()[0]=="JoinTimeout")
                {
                    if (Loaded)
                    {
                    }
                    else
                    {
                        RoomOptions roomOptions = new RoomOptions{IsVisible = true, MaxPlayers = 4};
                        string RoomID = "";
                        for (int _ = 0; _ < 10; _++)
                        {
                            RoomID += Random.Range(0, 9).ToString();
                        }
                        PhotonNetwork.JoinOrCreateRoom(RoomID, roomOptions, TypedLobby.Default);
                    }
                } 
                else if (Waits.Peek()[0]=="Play")
                {
                    PlayCard(Waits.Peek()[1]);
                    Ticks = 60;
                }
                else if (Waits.Peek()[0] == "Sync")
                {
                    SyncOtherPlayer(Waits.Peek()[1], Waits.Peek()[2],int.Parse(Waits.Peek()[3]),Waits.Peek()[4],int.Parse(Waits.Peek()[5]),int.Parse(Waits.Peek()[6]));
                }
                else if (Waits.Peek()[0]=="Start")
                {
                    GameStart(Waits.Peek()[1], Waits.Peek()[2], Waits.Peek()[3], Waits.Peek()[4]);
                }
                else if (Waits.Peek()[0]=="Picked")
                {
                    PickedUp(int.Parse(Waits.Peek()[1]),int.Parse(Waits.Peek()[2]));
                }
                else if (Waits.Peek()[0]=="Phase")
                {
                    PhaseOne();
                }
                else if (Waits.Peek()[0]=="Begin")
                {
                    BeginRounds(Waits.Peek()[1],int.Parse(Waits.Peek()[2]));
                }
                else if (Waits.Peek()[0] == "Turnover")
                {
                    TurnoverTrump(int.Parse(Waits.Peek()[1]));
                } else if (Waits.Peek()[0] == "Join")
                {
                    PlayerJoin(Waits.Peek()[1],Waits.Peek()[2],int.Parse(Waits.Peek()[3]));
                }
                Waits.Dequeue();
                Ticks = 0;
                Skip = false;
            }
        }
    }

    public void ScoreTrick()
    {
        List<int> Best = new List<int> { 0, 0 }; // 0th - best player, 1st - best card
        char StartingSuit = PlayedCards[0][1];
        if (PlayedCards[0][0]=='J' && PlayedCards[0][1]==AlternateSuits[Trump])
        {
            StartingSuit = AlternateSuits[Trump];
        }
        PlayedCards.Reverse();
        for (int i = 0; i < PlayedCards.Count; i++)
        {
            if (EvaluateCard(PlayedCards[i], StartingSuit) > Best[1])
            {
                Best[1] = EvaluateCard(PlayedCards[i], StartingSuit);
                Best[0] = Turn;
                for (int i2 = 0; i2 < i; i2++)
                {
                    Best[0] -= 1;
                    if (Best[0] < 0)
                    {
                        Best[0] += 4;
                    }
                    else if (Best[0] > 3)
                    {
                        Best[0] -= 4;
                    }
                    if (Best[0]==PlayerToSkip)
                    {
                        Best[0] -= 1;
                    }
                    if (Best[0] < 0)
                    {
                        Best[0] += 4;
                    }
                    else if (Best[0] > 3)
                    {
                        Best[0] -= 4;
                    }
                }
            }
        }
        Turn = Best[0] - 1;
        if ((Best[0] % 2) == 0)
        {
            Team1Tricks += 1;
        }
        else
        {
            Team2Tricks += 1;
        }
        PlayedCards = new List<string>();
        int Removed = 0;
        int Temp = DisplayedCards.Count;
        for (int i = 0; i < Temp; i++)
        {
            if (!(DisplayedCards[i - Removed].GetComponent<CardComponent>().GetTargetPosition().y == -600))
            {
                Destroy(DisplayedCards[i - Removed]);
                DisplayedCards.RemoveAt(i - Removed);
                Removed += 1;
            }
        }
        if (Team1Tricks+Team2Tricks==5)
        {
            int Temp2 = DisplayedCards.Count;
            for (int i=0;i<Temp2;i++)
            {
                Destroy(DisplayedCards[0]);
                DisplayedCards.RemoveAt(0);
            }
            if (TrumpPickerTeam == 0)
            {
                if (Team1Tricks == 5)
                {
                    if (PlayerToSkip != -1)
                    {
                        Team1Score += 4;
                    }
                    else
                    {
                        Team1Score += 2;
                    }
                }
                else if (Team1Tricks > 2)
                {
                    Team1Score += 1;
                }
                else
                {
                    Team2Score += 2;
                }
            }
            else
            {
                if (Team2Tricks == 5)
                {
                    if (PlayerToSkip != -1)
                    {
                        Team2Score += 4;
                    }
                    else
                    {
                        Team2Score += 2;
                    }
                }
                else if (Team2Tricks > 2)
                {
                    Team2Score += 1;
                }
                else
                {
                    Team1Score += 2;
                }
            }
            Loaded = false;
            Deck = new List<string>();
            Hands = new List<string>();
            PlayedCards = new List<string>();
            DisplayedCards = new List<GameObject>();
            if (Host)
            {
                Initialise();
                PhotonView.Get(this).RPC("SyncOtherPlayer", RpcTarget.Others, string.Join("", Hands), string.Join("", Deck), -1,"",-1,-2);
                PhotonView.Get(this).RPC("GameStart", RpcTarget.All, PlayerNames[0], PlayerNames[1], PlayerNames[2], PlayerNames[3]);
            }
        } 
        else
        {
            DoRound();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetRoomID().Length==0)
        {
            PhotonNetwork.JoinRandomRoom();
            Waits.Enqueue(new List<string> { "JoinTimeout" });
        }
        else
        {
            RoomOptions roomOptions = new RoomOptions {IsVisible = false, MaxPlayers = 4};
            PhotonNetwork.JoinOrCreateRoom(LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetRoomID(), roomOptions, TypedLobby.Default);
        }
    }

    [PunRPC]
    public void PlayerJoin(string NewName, string JoiningPlayerID, int JoiningTeamAttempt)
    {
        if (Waits.Count > 0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Join", NewName, JoiningPlayerID,JoiningTeamAttempt.ToString() });
        }
        else
        {
            if (Players < 4) //Prevents issues with Photon letting too many people any a game
            {
                Players += 1;
                MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Waiting for players (" + Players.ToString() + "/4)";
                if (Host)
                {
                    int Temp;
                    if (PlayersJoined.Contains(JoiningTeamAttempt))
                    {
                        if (PlayersJoined.Contains(JoiningTeamAttempt+2))
                        {
                            if (PlayersJoined.Contains(JoiningTeamAttempt + 1))
                            {
                                if (JoiningTeamAttempt + 3>3) 
                                {
                                    Temp = JoiningTeamAttempt - 1;
                                }
                                else
                                {
                                    Temp = JoiningTeamAttempt + 3;
                                }
                            }
                            else
                            {
                                Temp = JoiningTeamAttempt + 1;
                            }
                        } else
                        {
                            Temp = JoiningTeamAttempt + 2;
                        }
                    } else
                    {
                        Temp = JoiningTeamAttempt;
                    }
                    PlayerNames[Temp] = NewName;
                    PlayersJoined.Add(Temp);
                    PhotonView.Get(this).RPC("SyncOtherPlayer", RpcTarget.Others, string.Join("", Hands), string.Join("", Deck), Players, JoiningPlayerID,Temp,Dealer);
                    if (Players == 4)
                    {
                        PhotonView.Get(this).RPC("GameStart", RpcTarget.All, PlayerNames[0], PlayerNames[1], PlayerNames[2], PlayerNames[3]);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void GameStart(string P1Name, string P2Name, string P3Name, string P4Name)
    {
        if (Waits.Count>0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Start",P1Name,P2Name,P3Name,P4Name });
        }
        else
        {
            PlayerNames = new Dictionary<int,string> { { 0, P1Name }, { 1, P2Name }, { 2, P3Name }, { 3, P4Name } };
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Start!";
            Team1Tricks = 0;
            Team2Tricks = 0;
            PlayerToSkip = -1;
            Trump = 'N';
            Dealer += 1;
            if (Dealer>3)
            {
                Dealer = 0;
            }
            Turn = Dealer;
            Cycled = 0;
            PhaseOne();
        }
    }

    [PunRPC]
    public void PhaseOne()
    {
        if (Waits.Count > 0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Phase" });
        }
        else
        {
            Turn += 1;
            if (Turn == Dealer + 1)
            {
                Cycled += 1;
                if (Cycled == 2)
                {
                    TurnoverTrump(-1); //-1 is placeholder as turnover trump and set picker team are not seperate yet
                }
            }
            if (Turn > 3)
            {
                Turn = 0;
            }
            DisplayStatsText();
            DisplayTurnText();
            if (Turn == PlayerNum)
            {
                PickButtonObject.GetComponent<PickButtonComponent>().SetVisible(true);
                GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetVisible(true);
                if (Cycled != 2 || Turn != Dealer)
                {
                    PassButtonObject.GetComponent<PassButtonComponent>().SetVisible(true);
                }
            }
        }
    }
    public void Pass()
    {
        PassButtonObject.GetComponent<PassButtonComponent>().SetVisible(false);
        PassButtonObject.GetComponent<PassButtonComponent>().SetSelected(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetSelected(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetVisible(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetSelected(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetVisible(false);
        PhotonView.Get(this).RPC("PhaseOne", RpcTarget.All);
    }

    public void PickButtonClicked()
    {
        PassButtonObject.GetComponent<PassButtonComponent>().SetSelected(false);
        PassButtonObject.GetComponent<PassButtonComponent>().SetVisible(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetSelected(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetVisible(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetSelected(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetVisible(false);
        if (Cycled==2)
        {
            List<Vector3> TempPositions = new List<Vector3> {new Vector3 (-1000,525,0), new Vector3(-500, 525, 0), new Vector3(-750, 100, 0) };
            foreach (string Suit in Suits)
            {
                if (Suit != Deck[0][1].ToString()) 
                {
                    PickSuitButtons.Add(Instantiate(PickSuitButton, TempPositions[0], transform.rotation));
                    PickSuitButtons[PickSuitButtons.Count - 1].GetComponent<PickSuitButtonComponent>().SetSuit(Suit);
                    TempPositions.RemoveAt(0);
                }
            }
        }
        else
        {
            PhotonView.Get(this).RPC("PickedUp", RpcTarget.All, PlayerNum % 2, PlayerToSkip);
        }
    }

    public void GoAloneButtonClicked()
    {
        PassButtonObject.GetComponent<PassButtonComponent>().SetSelected(false);
        PassButtonObject.GetComponent<PassButtonComponent>().SetVisible(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetSelected(false);
        PickButtonObject.GetComponent<PickButtonComponent>().SetVisible(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetSelected(false);
        GoAloneButtonObject.GetComponent<GoAloneButtonComponent>().SetVisible(false);
        PlayerToSkip = PlayerNum + 2;
        if (PlayerToSkip>3)
        {
            PlayerToSkip -= 4;
        }
        if (Cycled == 2)
        {
            List<Vector3> TempPositions = new List<Vector3> { new Vector3(-1000, 525, 0), new Vector3(-500, 525, 0), new Vector3(-750, 100, 0) };
            foreach (string Suit in Suits)
            {
                if (Suit != Deck[0][1].ToString())
                {
                    PickSuitButtons.Add(Instantiate(PickSuitButton, TempPositions[0], transform.rotation));
                    PickSuitButtons[PickSuitButtons.Count - 1].GetComponent<PickSuitButtonComponent>().SetSuit(Suit);
                    TempPositions.RemoveAt(0);
                }
            }
        }
        else
        {
            PhotonView.Get(this).RPC("PickedUp", RpcTarget.All, PlayerNum % 2,PlayerToSkip);
        }
    }

    public void PickTrumpSuit(string Suit)
    {
        int Temp = PickSuitButtons.Count;
        for (int i=0; i<Temp; i++)
        {
            Destroy(PickSuitButtons[0]);
            PickSuitButtons.RemoveAt(0);
        }
        Trump = Suit[0];
        TrumpPickerTeam = PlayerNum % 2;
        PhotonView.Get(this).RPC("BeginRounds", RpcTarget.All, Trump.ToString(),PlayerToSkip);
    }

    [PunRPC]
    public void PickedUp(int NewTrumpPickerTeam,int NewPlayerToSkip)
    {
        if (Waits.Count > 0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Picked", NewTrumpPickerTeam.ToString(),NewPlayerToSkip.ToString() });
        }
        else
        {
            if (PlayerNum == Dealer)
            {
                DisplayedCards[DisplayedCards.Count - 1].GetComponent<CardComponent>().SetTargetPosition(new Vector3(1050, -600, -6));
                Trump = Deck[0][1];
                TrumpPickerTeam = NewTrumpPickerTeam;
                PlayerToSkip = NewPlayerToSkip;
                Hands.Insert((PlayerNum + 1) * (Hands.Count / 4) - 1, Deck[0]);
                MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Discard a card in your hand";
                DisplayStatsText();
                foreach (GameObject Card in DisplayedCards)
                {
                    Card.GetComponent<CardComponent>().SetUnlocked(new List<bool> { false, true, false });
                }
                PhotonView.Get(this).RPC("TurnoverTrump", RpcTarget.Others, TrumpPickerTeam);
            }
            else
            {
                MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Dealer is discarding a card";
            }
        }
    }
    public void Discarded(string CardType)
    {
        Hands.Remove(CardType);
        foreach (GameObject Card in DisplayedCards)
        {
            Destroy(Card);
        }
        DisplayedCards = new List<GameObject>();
        CreateHand(PlayerNum);
        Trump = Deck[0][1];
        PhotonView.Get(this).RPC("BeginRounds", RpcTarget.All, Trump.ToString(),PlayerToSkip);
    }

    [PunRPC]
    public void BeginRounds(string TrumpSuit, int NewPlayerToSkip)
    {
        if (Waits.Count<0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Begin", TrumpSuit,NewPlayerToSkip.ToString() });
        }
        else
        {
            PlayerToSkip = NewPlayerToSkip;
            Trump = char.Parse(TrumpSuit);
            Turn = Dealer;
            DoRound();
        }
    }

    [PunRPC]
    public void PlayCard(string CardType)
    {
        if (Waits.Count>0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Play", CardType });
        } 
        else
        {
            PlayedCards.Add(CardType);
            if (!(Turn == PlayerNum))
            {
                Vector3 Position = new Vector3(-3000, 300, PlayedCards.Count * -1 - 1);
                if ((Turn+1)%2==(PlayerNum+1)%2)
                {
                    Position = new Vector3(-100 + PlayedCards.Count * 100, 2000, PlayedCards.Count * -1 - 1);
                }
                if (PlayerNum==0 && Turn == 3)
                {
                    Position = new Vector3(3000, 300, PlayedCards.Count * -1 - 1);
                }
                else if (PlayerNum==3 && Turn==2)
                {
                    Position = new Vector3(3000, 300, PlayedCards.Count * -1 - 1);
                }
                else if ((PlayerNum==1 || PlayerNum==2) && Turn<PlayerNum)
                {
                    Position = new Vector3(3000, 300, PlayedCards.Count * -1 - 1);
                }
                PlaceCard(CardType, Position, new Vector3(-100 + PlayedCards.Count * 100, 300, PlayedCards.Count * -1 - 1));
            }
            int Num = 4;
            if (PlayerToSkip != -1)
            {
                Num -= 1;
            }
            if (PlayedCards.Count == Num)
            {
                Ticks = 0;
                Waits.Enqueue(new List<string> { "Score" });
            }
            else
            {
                DoRound();
            }
        }
    }
    public int EvaluateCard(string CardType, char StartSuit)
    {
        if (CardType[1]==AlternateSuits[Trump] && CardType[0]=='J')
        {
            return 14;
        }
        else if (CardType[1] == Trump)
        {
            if (CardType[0]=='J')
            {
                return 15;
            } 
            else if (CardType[0]=='A')
            {
                return 13;
            }
            else if (CardType[0] == 'K')
            {
                return 12;
            }
            else if (CardType[0] == 'Q')
            {
                return 11;
            }
            else if (CardType[0] == 'T')
            {
                return 10;
            }
            else if (CardType[0] == '9')
            {
                return 9;
            }
            else if (CardType[0] == '8')
            {
                return 8;
            }
        }
        else if (CardType[1]==StartSuit)
        {
            if (CardType[0] == 'A')
            {
                return 7;
            }
            else if (CardType[0] == 'K')
            {
                return 6;
            }
            else if (CardType[0] == 'Q')
            {
                return 5;
            }
            else if (CardType[0] == 'J')
            {
                return 4;
            }
            else if (CardType[0] == 'T')
            {
                return 3;
            }
            else if (CardType[0] == '9')
            {
                return 2;
            }
            else if (CardType[0] == '8')
            {
                return 1;
            }
        }
        return 0;
    }

    public int GetPlayedCardsCount()
    {
        return PlayedCards.Count;
    }

    public void DoRound()
    {
        Turn += 1;
        if (Turn>3)
        {
            Turn = 0;
        }
        if (Turn==PlayerToSkip)
        {
            Turn += 1;
            if (Turn > 3)
            {
                Turn = 0;
            }
        }
        DisplayStatsText();
        DisplayTurnText();
        if (Turn==PlayerNum)
        {
            foreach (GameObject Card in DisplayedCards)
            {
                if (Card.GetComponent<CardComponent>().GetTargetPosition().y == -600)
                {
                    Card.GetComponent<CardComponent>().SetUnlocked(new List<bool> { false, false, true });
                }
            }
        }
    }

    [PunRPC]
    public void TurnoverTrump(int NewTrumpPickerTeam)
    {
        if (Waits.Count > 0 && !Skip)
        {
            Waits.Enqueue(new List<string> { "Turnover", NewTrumpPickerTeam.ToString() });
        }
        else
        {
            TrumpPickerTeam = NewTrumpPickerTeam;
            Destroy(DisplayedCards[DisplayedCards.Count - 1]);
            DisplayedCards.RemoveAt(DisplayedCards.Count - 1);
        }
    }
    public void DisplayTurnText()
    {
        if (Turn == PlayerNum)
        {
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Your turn";
        }
        else
        {
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Player " + (Turn + 1).ToString() +" ("+PlayerNames[Turn]+ "'s) turn";
        }
    }

    public void DisplayStatsText()
    {
        string TrumpText = "Undecided";
        if (Trump == 'C')
        {
            TrumpText = "Clubs";
        }
        else if (Trump == 'H')
        {
            TrumpText = "Hearts";
        }
        else if (Trump == 'D')
        {
            TrumpText = "Diamonds";
        }
        else if (Trump == 'S')
        {
            TrumpText = "Spades";
        }
        StatsTextObject.GetComponent<UnityEngine.UI.Text>().text = "You are player: " + (PlayerNum + 1).ToString() +
            "\n\nYou are in: Team: " + (2 - ((PlayerNum + 1) % 2)).ToString() +
            "\n\nDealer is: Player "+(Dealer+1).ToString()+" ("+PlayerNames[Dealer]+")"+
            "\n\nTrump is: " + TrumpText + 
            "\nPicked by: Team " +(TrumpPickerTeam+1).ToString()+
            "\n\nTeam 1 Tricks: " + Team1Tricks.ToString() +
            "\nTeam 2 Tricks: " + Team2Tricks.ToString() +
            "\n\nTeam 1 Score: " + Team1Score.ToString() +
            "\nTeam 2 Score: " + Team2Score.ToString();
    }

    public override void OnCreatedRoom()
    {
        Host = true;
        Loaded = true;
        PlayerNum = LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetTeamAttempt();
        PlayersJoined.Add(PlayerNum);
        Players = 1;
        Dealer = Random.Range(-1, 3);
        MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Waiting for players (" + Players.ToString() + "/4)";
        PlayerNames[LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetTeamAttempt()]= LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetName();
        Initialise();
    }

    public override void OnJoinedRoom()
    {
        if (!Host)
        {
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Waiting for players";
            PlayerID = "";
            for (int _ = 0; _ < 10; _++)
            {
                PlayerID += Random.Range(0, 9).ToString();
            }
            PhotonView.Get(this).RPC("PlayerJoin", RpcTarget.Others, LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetName(),PlayerID,LobbyManagerObject.GetComponent<LobbyManagerComponent>().GetTeamAttempt());
        }
    }

    public void Initialise()
    {
        foreach (string Type in Types)
        {
            foreach (string Suit in Suits)
            {
                Deck.Add(Type + Suit);
            }
        }
        for (int _ = 0; _ < 4; _++)
        {
            for (int __ = 0; __ < 5; __++)
            {
                int CardNum = Random.Range(0, Deck.Count);
                Hands.Add(Deck[CardNum]);
                Deck.RemoveAt(CardNum);
            }
        }
        CreateHand(PlayerNum);
        PlaceCard(Deck[0], new Vector3(-1500, 500, 0), new Vector3(-1500, 500, 0));
    }

    [PunRPC]
    public void SyncOtherPlayer(string NewHands, string NewDeck, int NewPlayers, string JoiningPlayerID, int NewPlayerNum, int NewDealer)
    {
        if (Waits.Count>0 && Waits.Peek()[0]!="JoinTimeout" && !Skip)
        {
            Waits.Enqueue(new List<string> { "Sync", NewHands, NewDeck, NewPlayers.ToString(),JoiningPlayerID,NewPlayerNum.ToString(),NewDealer.ToString() });
        }
        else
        {
            if (NewPlayers != -1)
            {
                Players = NewPlayers;
            }
            MainTextObject.GetComponent<UnityEngine.UI.Text>().text = "Waiting for players (" + Players.ToString() + "/4)";
            if (!Loaded && (JoiningPlayerID==PlayerID || JoiningPlayerID.Length ==0))
            {
                List<string> TempNewHands = new List<string>();
                for (int i = 0; i < (NewHands.Length / 2); i++)
                {
                    TempNewHands.Add(NewHands[i * 2].ToString() + NewHands[i * 2 + 1].ToString());
                }
                Hands = TempNewHands;
                List<string> TempNewDeck = new List<string>();
                for (int i = 0; i < (NewDeck.Length / 2); i++)
                {
                    TempNewDeck.Add(NewDeck[i * 2].ToString() + NewDeck[i * 2 + 1].ToString());
                }
                Deck = TempNewDeck;
                if (NewPlayers != -1)
                {
                    PlayerNum = NewPlayerNum;
                }
                CreateHand(PlayerNum);
                PlaceCard(Deck[0], new Vector3(-1500, 500, 0), new Vector3(-1500, 500, 0));
                Loaded = true;
                if (NewDealer!=-2)
                {
                    Dealer = NewDealer;
                }
            }
        }
    }
    public void PlaceCard(string CardType,Vector3 StartPosition, Vector3 TargetPosition)
    {
        DisplayedCards.Add(Instantiate(Card, StartPosition, transform.rotation));
        DisplayedCards[DisplayedCards.Count - 1].GetComponent<CardComponent>().BuildCardDictionary();
        DisplayedCards[DisplayedCards.Count - 1].GetComponent<CardComponent>().SetTargetPosition(TargetPosition);
        DisplayedCards[DisplayedCards.Count - 1].GetComponent<CardComponent>().SetCard(CardType);
        DisplayedCards[DisplayedCards.Count - 1].GetComponent<CardComponent>().SetUnlocked(new List<bool> {false, false, false });
    }

    public void CreateHand(int PlayerNum)
    {
        for (int i=0; i < (Hands.Count / 4); i++)
        {
            PlaceCard(Hands[i+PlayerNum*(Hands.Count/4)], new Vector3(-700, -600, 0), new Vector3(-700+350*i, -600, -1*i));
        }
    }

    public bool CheckIfValid(string CardType)
    {
        char StartingSuit= 'N';
        if (PlayedCards.Count> 0)
        {
            if (PlayedCards[0][0]=='J' && PlayedCards[0][1]==AlternateSuits[Trump])
            {
                StartingSuit = Trump;
            } else
            {
                StartingSuit = PlayedCards[0][1];
            }
        }
        if (!((PlayedCards.Count == 0) || (StartingSuit == CardType[1] && !(CardType[0] == 'J' && CardType[1] == AlternateSuits[Trump] && StartingSuit==AlternateSuits[Trump]))
            || (CardType[1] == AlternateSuits[Trump] && CardType[0] == 'J' && StartingSuit==Trump)))
        {
            foreach (GameObject Card in DisplayedCards)
            {
                if ((Card.GetComponent<CardComponent>().GetTargetPosition().y == -600) && 
                    (((Card.GetComponent<CardComponent>().GetCardType()[1]== StartingSuit) && 
                    !(Card.GetComponent<CardComponent>().GetCardType()[0] == 'J' &&
                    Card.GetComponent<CardComponent>().GetCardType()[1] == AlternateSuits[Trump] && 
                    StartingSuit== AlternateSuits[Trump])) || (Card.GetComponent<CardComponent>().GetCardType()[0] == 'J' &&
                    Card.GetComponent<CardComponent>().GetCardType()[1] == AlternateSuits[Trump] &&
                    StartingSuit == Trump)))
                {
                    return false;
                }
            }
        }
        PhotonView.Get(this).RPC("PlayCard", RpcTarget.Others, CardType);
        PlayedCards.Add(CardType);
        int Num = 4;
        if (PlayerToSkip != -1)
        {
            Num -= 1;
        }
        if (PlayedCards.Count == Num)
        {
            Ticks = 0;
            Waits.Enqueue(new List<string> { "Score" });
        }
        else
        {
            DoRound();
        }
        if (PlayedCards.Count > 0) //If the player who plays the card begins the new trick, don't make all their cards locked
        {
            foreach (GameObject Card in DisplayedCards)
            {
                Card.GetComponent<CardComponent>().SetUnlocked(new List<bool> { false, false, false });
            }
        }
        return true;
    }
    public List<GameObject> GetDisplayedCards()
    {
        return DisplayedCards;
    }
}
