using UnityEngine;

public class NextButton : MonoBehaviour
{
    public GameObject[] panels; // Array to hold the tutorial panels
    public GameObject oceanPanel;
    public GameObject nextButton;
    public GameObject skipTutorialButton;

    private int currentPanelIndex = 0; // Index of the current active panel

    public GameManager gameManager; // Reference to GameManager for sound and card management

    // Call this method on button press to go to the next panel
    public void NextPanel()
    {
        gameManager.PlayClickSound();

        if (currentPanelIndex < panels.Length) // If not at the end, deactivate current and move to the next
        {
            panels[currentPanelIndex].SetActive(false);
            currentPanelIndex++;
            if (currentPanelIndex < panels.Length)
            {
                panels[currentPanelIndex].SetActive(true);
            }
            else
            {
                oceanPanel.SetActive(false);
                nextButton.SetActive(false);
                skipTutorialButton.SetActive(false);
                ShowActiveCards(true); // Show cards when tutorial is complete
            }
        }
    }

    // Call this method to reset and restart the tutorial
    public void RestartTutorial()
    {
        // Reset index and deactivate all panels
        currentPanelIndex = 0;
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }

        // Reactivate the first panel and any related UI elements
        if (panels.Length > 0)
        {
            panels[0].SetActive(true);
        }

        oceanPanel.SetActive(true);
        nextButton.SetActive(true);
        skipTutorialButton.SetActive(true);

        // Optionally play a sound if desired
        gameManager.PlayClickSound();
        
        // Hide the cards while the tutorial is active
        ShowActiveCards(false);
    }

    // Call this method to skip the tutorial
    public void SkipTutorial()
    {
        // Deactivate all panels and associated UI elements
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }

        oceanPanel.SetActive(false);
        nextButton.SetActive(false);
        skipTutorialButton.SetActive(false);

        // Optionally play a sound for skipping
        gameManager.PlayClickSound();

        // Show cards after skipping the tutorial
        ShowActiveCards(true);
    }

    private void Start()
    {
        // Only the first panel is active at the start
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }

        // Initially hide cards during tutorial
        ShowActiveCards(false);
    }

    // Helper method to show or hide active cards
    private void ShowActiveCards(bool isActive)
    {
        if (gameManager != null && gameManager.activeCardsInHand != null)
        {
            foreach (Card card in gameManager.activeCardsInHand)
            {
                card.gameObject.SetActive(isActive);
            }
        }
    }
}