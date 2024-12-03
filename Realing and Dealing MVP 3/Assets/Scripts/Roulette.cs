using System.Collections;
using UnityEngine;
using TMPro;

public class Roulette : MonoBehaviour
{
    public float rotatePower = 500f; // Torque to spin the wheel
    public float stopPower = 50f; // Deceleration rate for stopping

    private Rigidbody2D rbody; // Rigidbody2D for handling rotation physics
    private bool isSpinning = false; // Whether the wheel is spinning
    private bool isStopped = false; // Whether the wheel has stopped

    public TextMeshProUGUI eventText; // UI Text to display event messages

    private float finalAngle = 0f; // To store the final angle after stopping

    public static GameManager gm;

    public bool luckyFish = false;
    public bool canSpin = true;
    public GameObject okButton;

    public GameManager gameManager; // Only for sound


    // Events triggered based on section
    private void Start()
    {
        gm = GameManager.instance;  // Access the GameManager instance

        rbody = GetComponent<Rigidbody2D>();

        okButton.SetActive(false);        

    }

    private void Update()
    {
        if (isSpinning)
        {
            // Gradually decrease angular velocity (deceleration)
            if (rbody.angularVelocity > 0)
            {
                rbody.angularVelocity -= stopPower * Time.deltaTime;
                if (rbody.angularVelocity < 0.1f) // Ensure it doesn't overshoot too much
                {
                    rbody.angularVelocity = 0;
                    isSpinning = false;
                    isStopped = true;
                }
            }
        }

        // Once the wheel stops, we map the final angle to a section
        if (isStopped)
        {
            MapToEvent();
            SnapToSelection(); // Snap the wheel to the selected section
            isStopped = false; // Reset the stop flag
        }
    }

    // Function to start the wheel spinning, use to call events
    public void Rotate()
    {
        gameManager.PlayClickSound();
        if ((!isSpinning) && (canSpin == true))
        {
            // Change torque for randomness
            float randomTorque = Random.Range(rotatePower * 0.8f, rotatePower * 1.2f);
            rbody.AddTorque(randomTorque);
            isSpinning = true;
            isStopped = false;
        }
    }
    private void MapToEvent()
    {
        finalAngle = transform.eulerAngles.z;

        // Normalize the angle to be between 0 and 360
        if (finalAngle < 0)
        {
            finalAngle += 360;
        }

        // Map the angle to one of 8 sections (45 degrees each)
        int sectionIndex = Mathf.FloorToInt((finalAngle + 22.5f) / 45f) % 8;

        // Trigger the event based on the section index
        switch (sectionIndex)
        {
            case 0: Event1(); break;
            case 1: Event2(); break;
            case 2: Event3(); break;
            case 3: Event4(); break;
            case 4: Event5(); break;
            case 5: Event6(); break;
            case 6: Event7(); break;
            case 7: Event8(); break;
            default: Debug.LogWarning("Unexpected section index: " + sectionIndex); break;
        }
    }

    // Snap the wheel to the closest section
    private void SnapToSelection()
    {
        // Round the final angle to the closest 45-degree section
        float snappedAngle = Mathf.Round(finalAngle / 45f) * 45f;
        transform.eulerAngles = new Vector3(0, 0, snappedAngle);
        Debug.Log("Snapped to angle: " + snappedAngle);
    }
    private void Event1() 
    {
        canSpin = false;
        eventText.text = "Flop on the deck: Lose 2 Trophy Points!";
        GameManager.playerController.points -= 2;
        GameManager.instance.UpdateTrophyPointsUI(GameManager.playerController.points);
        okButton.gameObject.SetActive(true);
    }

    private void Event2() 
    { 
        canSpin = false;
        luckyFish = true;
        eventText.text = "Lucky catch: Draw a card for free!"; 
        okButton.gameObject.SetActive(true);
    }

    private void Event3() 
    {
        canSpin = false;
        if (GameManager.playerController.baitCount < 2)
        {
            eventText.text = "Hungry Guppy: He thinks you're poor. Lose no bait.";
        } else {
            GameManager.playerController.baitCount -= 1;
            GameManager.instance.UpdateBaitUI(GameManager.playerController.baitCount);
            eventText.text = "Hungry Guppy: Lose 1 bait.";
        }
        okButton.gameObject.SetActive(true);
    }

    private void Event4() 
    { 
        canSpin = false;
        eventText.text = "Bait and switch: Gain 1 bait but lose 1 trophy point.";
        GameManager.playerController.baitCount += 1;
        GameManager.instance.UpdateBaitUI(GameManager.playerController.baitCount);
        GameManager.playerController.points -= 1;
        GameManager.instance.UpdateTrophyPointsUI(GameManager.playerController.points);
        eventText.gameObject.SetActive(true);
        okButton.gameObject.SetActive(true);
    }

    private void Event5() 
    { 
        canSpin = false;
        eventText.text = "Jackpot! Gain 3 bait and 3 trophy points!";
        GameManager.playerController.baitCount += 3;
        GameManager.instance.UpdateBaitUI(GameManager.playerController.baitCount);
        GameManager.playerController.points += 3;
        GameManager.instance.UpdateTrophyPointsUI(GameManager.playerController.points);
        eventText.gameObject.SetActive(true);
        okButton.gameObject.SetActive(true);
    }

    private void Event6()
    {
        eventText.text = "Repeat Wheel: Spin again.";
        eventText.gameObject.SetActive(true);
    }

    private void Event7() 
    {
        canSpin = false;
        eventText.text = "Can of worms: Gain 3 Bait!";
        GameManager.playerController.baitCount += 3;
        GameManager.instance.UpdateBaitUI(GameManager.playerController.baitCount);
        eventText.gameObject.SetActive(true);
        okButton.gameObject.SetActive(true);
    }
    
    private void Event8() 
    {
        canSpin = false;
        eventText.text = "Holy Mackeral: Gain 3 Trophy points!";
        GameManager.playerController.points += 3;
        GameManager.instance.UpdateTrophyPointsUI(GameManager.playerController.points);
        eventText.gameObject.SetActive(true);
        okButton.gameObject.SetActive(true);
    }

    public void okButtonClicked()
    {

        gameManager.PlayClickSound();
        gm.rouletteWheel.SetActive(false);
        canSpin = true;
        okButton.SetActive(false);        
    }
}