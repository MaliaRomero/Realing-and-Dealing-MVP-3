using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{

    //RESOURCES USED
    // <shttps://www.youtube.com/watch?v=C5bnWShD6ng
    // https://www.youtube.com/watch?v=eti87kSD_9U

//----------THIS SCRIPT IS FOR TURN SEQUENCE AND DRAWING CARDS----

    //-----------------INITIAL VARIABLES---------------------------
    public static GameManager instance;

    public static PlayerController playerController;

    public static Roulette roulette;

    public Card card;
    public Deck deck;

    //-----------------VARIABLES FOR SOUND-------------------------

    //public AudioClip click;
    //public AudioClip error;
    //public AudioClip fishing;


    //-----------------VARIABLES FOR GAME OBJECTS-------------------
    public List<Deck> decks = new List<Deck>();

    public Transform[] cardSlots;
    
    public bool[] availableCardSlots;

    public int handIndex = 0; //for current card

    //----------------VARIABLES FOR PLAYER UI----------------------
    public TextMeshProUGUI deckSizeText;
    public TextMeshProUGUI discardPileText;

    //-----------------VARIABLES FOR TURNS-----------------------
    public TextMeshProUGUI tackleBoxText;
    public TextMeshProUGUI trophyPointsText;
    private bool isPlayerTurn = true;

    public GameObject skipDiscardButton;

    public GameObject rouletteWheel;

    //-----------------VARIABLES FOR WIN/LOSE-------------------
    public int cardCounter = 0;
    public GameObject winLosePanel;  // Win Screen
    public TextMeshProUGUI outOfBaitText;
    public TextMeshProUGUI fullHandText;

    //-------------Variables how to play----------
    public GameObject howToPanel;
    public GameObject goFishButton;


//***************************  FUNCTIONS *************************



//-------------------ON GAME START---------------------
    void Awake()
    {
        instance = this;

        skipDiscardButton.SetActive(false);//Turnn off skip discard button
        //eventText.gameObject.SetActive(false);//Turn off event text


        // Check if playerController is set
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>(); // Automatically finds the PlayerController in the scene
        }

        if (roulette == null)
        {
            roulette = FindObjectOfType<Roulette>(true);//Needs true because its not set active
            if (roulette == null)
                Debug.LogError("Roulette instance not found in the scene.");
        }

        // Initialize PlayerController.me from the GameManager
        if (playerController != null)
        {
            StartTurn(); //Lets the player take first turn
            Debug.Log("FIRST TURN");
        }
        else
        {
            Debug.LogError("PlayerController not found!");
        }
    }

//-----------------------How to play screen----------------------
    public void onGoFishClicked()
    {
        /*
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = click;
        audio.Play();
        */
        goFishButton.SetActive(false);
        howToPanel.SetActive(false);
    }

//-----------------STARTS TURN-------------------------
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;  // Basically checks if player can draw.
    }

    
    
    public void StartTurn()
    {
        isPlayerTurn = true;
        Debug.Log("Player's turn started. Click the draw button to draw a card.");
        // Player can click the button to manually draw a card when ready
    }

//---------------DRAWING ACTION-----------------------------
    public void DrawFromSpecificDeck(int deckIndex)
    {
        // THISSSSS  Invoke("HideEventText", 0);
        if(isPlayerTurn == true){//If allowed to draw
            DrawCard(deckIndex);
        }
        else{
                /*
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = error;
        audio.Play();
        */
            Debug.Log("Error! Not time to draw yet!");
        }
    }

    public void DrawCard(int deckIndex)
    {
        //Make sure correct number of decks
        if(deckIndex < 0 || deckIndex >= decks.Count)
        {
            Debug.LogError("Invalid deck index.");
            return;
        }

        // Select the specified deck
        Deck selectedDeck = decks[deckIndex];

        //For reseting deck
        Deck deck = decks[deckIndex];

        //Make sure theres enough cards in the specified deck, if not
        //move all cards from discard pile back to main deck, unlimited draws
        //Reset deck called from Deck
        if (selectedDeck.cards.Count <= 1)
        {
            selectedDeck.ResetDeck();
            Debug.Log("Reset Deck loop");
        }
        
        Card randCard = selectedDeck.cards[Random.Range(0, selectedDeck.cards.Count)];

        for (int i = 0; i < availableCardSlots.Length; i++)
        {
            if (availableCardSlots[i] == true)
            {
                int baitCost = GetBaitCost(deckIndex);

                if (roulette.luckyFish)
                {/*
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip = fish;//maybe new lucky fish sound?
                    audio.Play();*/
                    

                    randCard.gameObject.SetActive(true);
                    randCard.handIndex = i;
                    randCard.transform.position = cardSlots[i].position;
                    availableCardSlots[i] = false;

                    // Assign the deck to the card (so it knows its origin)
                    randCard.originDeck = selectedDeck;
                    selectedDeck.cards.Remove(randCard); // Remove from the deck
                                        // Don't add to discard pile yet

                    // Update bait count
                    UpdateBaitUI(playerController.baitCount);
                    roulette.luckyFish = false;

                    cardCounter++;
                    Debug.Log(cardCounter);

                    // Update deck and discard pile UI
                    UpdateDeckUI();
                    DiscardPhase();
                    return;

                }
                else if((playerController.baitCount < baitCost)){
                                        /*
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip = error;
                    audio.Play();
                    */
                    Debug.Log("Not enough Bait :( ");
                    isPlayerTurn = true; //lets the player draw again
                    return;
                }
                else
                {
                    /*
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip = fish;
                    audio.Play();
                    */
                    isPlayerTurn = false; //Doesn't let the player draw again

                    randCard.gameObject.SetActive(true);
                    randCard.handIndex = i;
                    randCard.transform.position = cardSlots[i].position;
                    availableCardSlots[i] = false;

                    selectedDeck.cards.Remove(randCard);

                    // Assign the deck to the card (so it knows its origin)
                    randCard.originDeck = selectedDeck;


                    selectedDeck.cards.Remove(randCard); // Remove from the deck
                                        // Don't add to discard pile yet

                    // Update bait count
                    playerController.baitCount -= baitCost;
                    UpdateBaitUI(playerController.baitCount);

                    cardCounter++;
                    Debug.Log(cardCounter);

                    // Update deck and discard pile UI
                    UpdateDeckUI();

                    DiscardPhase();
                    return;

                }
            }
        }
    }

//Discard "on mouse down" in card class
//-------------------SKIP DISCARD BUTTON-------------------------------------

    public void DiscardPhase()
    {
        ShowSkipDiscardButton();
    }

    public void OnSkipDiscardClicked()
    {
        /*
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = click;
        audio.Play();
        */
        HideSkipDiscardButton();
        // If skipping the discard, proceed directly to the event and end the turn.
        rouletteWheel.SetActive(true);
        EndTurn();
    }

    public void HideSkipDiscardButton ()
    {
        skipDiscardButton.SetActive(false);//Turnn off skip discard button
    }

    public void ShowSkipDiscardButton ()
    {
        skipDiscardButton.SetActive(true);//Turnn off skip discard button
    }

//-----------------Reset Deck--------------------------------------------

//----------------------EVENT ACTION--------------------------------------------

    //-------------------------UPDATE UI-----------------------------------------
    public void UpdateDeckUI()
    {
        for (int i = 0; i < decks.Count; i++)
        {
            deckSizeText.text = $"Remaining in deck {i+1}: {decks[i].cards.Count}";
            discardPileText.text = $"Discard Pile {i+1}: {decks[i].discardPile.Count}";
        }
    }

    public void UpdateTrophyPointsUI(int points)
    {
        trophyPointsText.text = "Trophy Points: " + points.ToString();
    }

    public void UpdateBaitUI(int baitCount)
    {
        if(baitCount < 2)
        {
            tackleBoxText.color = Color.red;
        }
        else
        {
            tackleBoxText.color = Color.white;
        }
        
        tackleBoxText.text = "Bait: " + baitCount.ToString();
    }

    void IncreaseBait()
    {
        playerController.baitCount++;
        UpdateBaitUI(playerController.baitCount);
    }

    public int GetBaitCost(int deckIndex)
    {
        if (deckIndex == 0) return 1; // Deck 1 costs 1 bait
        if (deckIndex == 1) return 2; // Deck 2 costs 2 bait
        if (deckIndex == 2) return 3; // Deck 3 costs 3 bait
        return 0; // Default cost
    }

    //------------------------END AND RESET-------------------
    public void EndTurn()
    {
        if(cardCounter >= 5)
        {
            Debug.Log("Out of hand space.");
            rouletteWheel.SetActive(false);
            winLosePanel.SetActive(true);
            outOfBaitText.gameObject.SetActive(false);
            Leaderboard.instance.SetLeaderboardEntry(playerController.points);
            Leaderboard.instance.leaderboardCanvas.SetActive(true);
            Leaderboard.instance.DisplayLeaderboard();
            fullHandText.text = "Boy howdy, you've reached your fishing quota!\nFinal Catch: " + playerController.points + " Trophy Points!";
        }
        else if(playerController.baitCount <= 0)
        {
            Debug.Log("Out of bait.");
            rouletteWheel.SetActive(false);
            winLosePanel.SetActive(true);
            fullHandText.gameObject.SetActive(false);
            Leaderboard.instance.SetLeaderboardEntry(playerController.points);
            Leaderboard.instance.leaderboardCanvas.SetActive(true);
            Leaderboard.instance.DisplayLeaderboard();
            outOfBaitText.text = "Looks like them fishies emptied your tackle box.\n Final Catch: " + playerController.points + " Trophy Points!";
        } else {
            isPlayerTurn = false;  // Set the turn flag to false when ending the turn
            Debug.Log("Turn Over, starting new turn");
            StartTurn();
        }
    }
}

