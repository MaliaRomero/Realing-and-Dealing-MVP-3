using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
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

    public GameObject musicObject;
    private AudioSource bgMusicSource; // Example for background music
    public AudioSource clickAudioSource;
    public AudioSource errorAudioSource;
    public AudioSource fishingAudioSource;
    public AudioMixer main;
    public string volumeParameter = "SFXVolume";
    public Slider slider;



    //-----------------VARIABLES FOR GAME OBJECTS-------------------
    public List<Deck> decks = new List<Deck>();
    public Transform[] cardSlots;
    public bool[] availableCardSlots;
    public int handIndex = 0; //for current card
    public List<Card> activeCardsInHand = new List<Card>();

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

//***************************  FUNCTIONS *************************



//-------------------ON GAME START---------------------
    void Awake()
    {
        instance = this;

        skipDiscardButton.SetActive(false);//Turn off skip discard button

        // Check if playerController is set
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>(); // Automatically finds the PlayerController in the scene
        }

        if (roulette == null)
        {
            roulette = FindObjectOfType<Roulette>(true);//Needs true because its not set active
        }

        // Initialize PlayerController.me from the GameManager
        if (playerController != null)
        {
            StartTurn(); //Lets the player take first turn
            Debug.Log("FIRST TURN");
        }

        //Check and set audio clips, compatible with slider
        if (musicObject != null)
        {
            var audioSources = musicObject.GetComponents<AudioSource>();
            if (audioSources.Length >= 4)
            {
                bgMusicSource = audioSources[0];
                clickAudioSource = audioSources[1];
                errorAudioSource = audioSources[2];
                fishingAudioSource = audioSources[3];
            }
            else
            {
                Debug.LogError("SOmething weird with the sounds");
            }
        }
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
        if(isPlayerTurn == true){//If allowed to draw
            DrawCard(deckIndex);
        }
        else{
            PlayErrorSound();
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

                if (roulette.luckyFish)//Only for lucky catch event, draw for free
                {
                    PlayFishingSound();
                    AddCardToHand(randCard);//Add card to current hand- list for Resetting the game
                    
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

                    // Update deck and discard pile UI
                    UpdateDeckUI();
                    DiscardPhase();
                    return;

                }
                else if((playerController.baitCount < baitCost))
                {
                    PlayErrorSound();
                    Debug.Log("Not enough Bait :( ");
                    isPlayerTurn = true; //lets the player draw again
                    return;
                }
                else
                {
                    PlayFishingSound();
                    isPlayerTurn = false; //Doesn't let the player draw again
                    AddCardToHand(randCard);//Add card to current hand- list for Resetting the game

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
                    // Update deck and discard pile UI
                    UpdateDeckUI();

                    DiscardPhase();
                    return;

                }
            }
        }
    }

    public void AddCardToHand(Card card)
    {
        activeCardsInHand.Add(card);
    }

    public void RemoveCardFromHand(Card card)
    {
        if (activeCardsInHand.Contains(card))
        {
            activeCardsInHand.Remove(card);
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
        PlayClickSound();
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

    //---------------SFX FUNCTIONS=====================

        public void PlayClickSound()
    {
        if (clickAudioSource != null)
        {
            clickAudioSource.Play();
        }
    }

    public void PlayErrorSound()
    {
        if (errorAudioSource != null)
        {
            errorAudioSource.Play();
        }
    }

    public void PlayFishingSound()
    {
        if (fishingAudioSource != null)
        {
            fishingAudioSource.Play();
        }
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
            fullHandText.gameObject.SetActive(true);
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
            outOfBaitText.gameObject.SetActive(true);
            Leaderboard.instance.SetLeaderboardEntry(playerController.points);
            Leaderboard.instance.leaderboardCanvas.SetActive(true);
            Leaderboard.instance.DisplayLeaderboard();
            outOfBaitText.text = "Looks like them fishies emptied your tackle box.\n Final Catch: " + playerController.points + " Trophy Points!";
        } else {
            isPlayerTurn = false;  // Set the turn flag to false when ending the turn
            StartTurn();
        }
    }

//---------------------------Reset Game---------------------------


    public void ResetAllCardsInHand()
    {
        foreach (Card card in new List<Card>(activeCardsInHand)) // Create a copy to avoid issues with removal
        {
            card.ResetCard();
        }
    }

    // Resets all decks
    public void ResetAllDecks()
    {
        foreach (Deck deck in decks)
        {
            deck.ResetDeck();
        }
    }

    // Main reset method
    public void ResetGame()
    {   //Leaderboard.instance.SetLeaderboardEntry(playerController.points);
        //rouletteWheel.SetActive(false);
        PlayClickSound();
        isPlayerTurn = true;
        ResetAllCardsInHand();
        ResetAllDecks();
        activeCardsInHand.Clear();
        playerController.baitCount = 3; // Reset bait count
        UpdateBaitUI(playerController.baitCount);
        playerController.points = 0; // Reset trophy points
        UpdateTrophyPointsUI(playerController.points);
        winLosePanel.SetActive(false);
        Leaderboard.instance.leaderboardCanvas.SetActive(false);
        Debug.Log("Game has been reset!");
    }
}
