using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public float playerHealth = 100f;
    private float playerO2 = 100f;
    private float o2Decay = .33f; //Decay rate per half second. 
    private float o2DecayRate = .5f;//Rate of o2 Decay in seconds
    public bool o2DecayOn = true;
    private float o2HealthDecay = 1f;
    private float kickO2Drain = 4;
    private float sprintMult = 2;
    public bool isSprinting;
    public Button useHealth;
    public Button useO2;
    public TextMeshProUGUI healthPacksRemaining;
    public TextMeshProUGUI o2Remaining;
    public float healthPacks;
    public float o2Tanks;
    public float ammoPacks;

    private AudioSource o2DecayAudioSource; // AudioSource for O2 decay sound
    public AudioClip o2DecayClip; // Assign this in the Unity Inspector
    private bool isHealthDecaying = false;

    [Header("Force Settings")]
    public float xForce = 100f; // Force to apply on the X-axis
    public float zTorque = 50f; // Torque to apply for Z rotation



    public Slider healthSlider;
    public Slider o2Slider;

    public ScannerClickManager scannerClickManager;

    private LoadoutManager loadoutManager;

    [Header("Game Over Reference")]
    public TextMeshProUGUI gameOverUI;

    
    // Start is called before the first frame update
    
    void Start()
    {
        gameOverUI.gameObject.SetActive(false);
        loadoutManager = LoadoutManager.loadoutManager;
        if (loadoutManager == null)
        {
            Debug.LogError("Loadout MAnager NULL");
        }
        AcceptLoadoutVariables();
        StartCoroutine(Decrement02());
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider Null at start");
        }
        o2DecayAudioSource = gameObject.AddComponent<AudioSource>();
        o2DecayAudioSource.clip = o2DecayClip;
        o2DecayAudioSource.loop = true; // Loop the sound while o2HealthDecay is active
    }



    // Update is called once per frame

    IEnumerator Decrement02()
    {
        while (o2DecayOn)
        {
            yield return new WaitForSeconds(o2DecayRate);

            bool healthDecayingNow = false; // Tracks if health is decaying this iteration

            if (o2Slider != null)
            {
                O2TankState o2TankState = o2Slider.GetComponentInParent<O2TankState>();
                if (o2TankState != null && o2TankState.IsActive)
                {
                    if (playerO2 > 0)
                    {
                        if (isSprinting)
                        {
                            playerO2 -= o2Decay * sprintMult;
                        }
                        else
                        {
                            playerO2 -= o2Decay;
                        }

                        Debug.Log("Player o2 decremented to " + playerO2);
                        o2Slider.value = playerO2;
                    }
                    else
                    {
                        healthDecayingNow = true; // Health is decaying
                        playerHealth -= o2HealthDecay;
                        healthSlider.value = playerHealth;

                        if (!o2DecayAudioSource.isPlaying)
                        {
                            o2DecayAudioSource.Play();
                        }

                        if (playerHealth <= 0)
                        {
                            GameOver();
                        }
                    }
                }
            }

            if (!IsAnyO2TankActive() && !scannerClickManager.containerOpen)
            {
                healthDecayingNow = true; // Health is decaying rapidly
                playerHealth -= o2HealthDecay * 2;
                healthSlider.value = playerHealth;

                if (!o2DecayAudioSource.isPlaying)
                {
                    o2DecayAudioSource.Play();
                }

                if (playerHealth <= 0)
                {
                    GameOver();
                }
            }

            // Stop the sound if health is no longer decaying
            if (isHealthDecaying && !healthDecayingNow)
            {
                o2DecayAudioSource.Stop();
            }

            isHealthDecaying = healthDecayingNow; // Update the state
            yield return null;
        }

        // Ensure the audio stops when the coroutine ends
        if (o2DecayAudioSource.isPlaying)
        {
            o2DecayAudioSource.Stop();
        }
    }


    private void ApplyForceToDraggable(DraggableImage draggableImage)
    {
        draggableImage.isXAxisLocked = false; // Unlock the X-axis

        Rigidbody2D rb2D = draggableImage.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.AddForce(new Vector2(xForce, 0), ForceMode2D.Impulse); // Apply force on X-axis
            rb2D.AddTorque(zTorque, ForceMode2D.Impulse); // Apply torque for Z rotation
        }

        Debug.Log($"Force applied: X={xForce}, Z rotation torque={zTorque}");
    }

    private bool IsAnyO2TankActive()
    {
        O2TankState[] o2Tanks = FindObjectsOfType<O2TankState>();
        foreach (var tank in o2Tanks)
        {
            if (tank.IsActive)
            {
                Debug.Log("O2 Tank Active");
                return true;
            }
        }
        Debug.Log("O2 Tank Not Found");
        return false;
        
    }


    public void UpdateO2FromSlider()
    {
        if (o2Slider != null)
        {
            playerO2 = o2Slider.value; // Sync the O2 value with the new slider
            Debug.Log($"Player O2 updated to {playerO2} from new slider.");
        }
        else
        {
            Debug.LogError("O2 Slider is null. Cannot update player O2.");
        }
    }

    public void PlayerKickO2Consume()
    {
        if (o2DecayOn)
        {
            playerO2 -= (o2Decay * kickO2Drain);
        }
    }
    public void RefillPlayerO2()
    {
        playerO2 = 100f;
    }
    
    public void HurtPlayer(float damage)
    {
        playerHealth -= damage;
        if (healthSlider == null)
        {
            Debug.Log("Health Slider VAlue is null");
        }
        else
        {


            healthSlider.value = playerHealth;
            if (playerHealth <= 0)
            {
                GameOver();
            }
        }
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void HealPlayer()
    {
        if (healthPacks > 0)
        {
            healthPacks -= 1;
            playerHealth = 100;
            healthSlider.value = playerHealth;
            healthPacksRemaining.text = healthPacks.ToString();
        }
    }
    public void RefillO2()
    {
        if (o2Tanks > 0)
        {
            o2Tanks -= 1;
            playerO2 = 100;
            o2Slider.value = playerO2;
            o2Remaining.text = o2Tanks.ToString();
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!!");
        gameOverUI.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
    public void AcceptLoadoutVariables()
    {
        healthPacks = loadoutManager.healthPacks;
        if (healthPacks == null)
        {
            Debug.Log("Health Packs Variable null");
        }
        o2Tanks = loadoutManager.o2Tanks;
        ammoPacks = loadoutManager.ammoPacks;

        Debug.Log("health packs selected = " + healthPacks + ". O2 tanks selected = " + o2Tanks + ". Ammo Selected = " + ammoPacks + ".");
    }
}
