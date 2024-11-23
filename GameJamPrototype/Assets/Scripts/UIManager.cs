using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public float playerHealth = 100f;
    private float playerO2 = 100f;
    private float o2Decay = .1f; // Decay rate per half second
    private float o2DecayRate = .5f; // Rate of O2 decay in seconds
    public bool o2DecayOn = true;
    private float o2HealthDecay = 0.2f;
    private float kickO2Drain = 4;
    private float sprintMult = 2;
    public bool isSprinting;
    public Button useHealth;
    public Button useO2;
    public TextMeshProUGUI healthPacksRemaining;
    public TextMeshProUGUI o2Remaining;
    private float healthPacks;
    private float o2Tanks;
    private float ammoPacks;

    [Header("Force Settings")]
    public float xForce = 100f; // Force to apply on the X-axis
    public float zTorque = 50f; // Torque to apply for Z rotation

    public Slider healthSlider;
    public Slider o2Slider;

    private LoadoutManager loadoutManager;

    void Start()
    {
        Debug.Log("UIManager started.");
        loadoutManager = LoadoutManager.loadoutManager;

        if (loadoutManager == null)
        {
            Debug.LogError("LoadoutManager is NULL. Please ensure it is set correctly.");
        }
        else
        {
            Debug.Log("LoadoutManager successfully loaded.");
        }

        AcceptLoadoutVariables();
        StartCoroutine(Decrement02());

        if (healthSlider == null)
        {
            Debug.LogError("Health Slider is NULL at start.");
        }
        else
        {
            Debug.Log("Health Slider successfully initialized.");
        }
    }

    IEnumerator Decrement02()
    {
        Debug.Log("Starting O2 decrement coroutine.");
        while (o2DecayOn)
        {
            yield return new WaitForSeconds(o2DecayRate);

            if (o2Slider != null)
            {
                O2TankState o2TankState = o2Slider.GetComponentInParent<O2TankState>();

                if (o2TankState != null && o2TankState.IsActive)
                {
                    Debug.Log("Active O2 tank detected.");

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

                        Debug.Log($"Player O2 decremented to {playerO2}.");
                        o2Slider.value = playerO2;
                    }
                    else
                    {
                        Debug.LogWarning("Player O2 depleted. Applying force to draggable.");
                        DraggableImage draggableImage = o2Slider.GetComponentInParent<DraggableImage>();

                        if (draggableImage != null)
                        {
                            ApplyForceToDraggable(draggableImage);
                        }

                        playerHealth -= o2HealthDecay;
                        healthSlider.value = playerHealth;

                        Debug.Log($"Player health decreased to {playerHealth} due to O2 depletion.");

                        if (playerHealth <= 0)
                        {
                            GameOver();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No active O2 tank detected.");
                }
            }
            else
            {
                Debug.LogError("O2 Slider is NULL. Cannot decrement O2.");
            }

            if (!IsAnyO2TankActive())
            {
                playerHealth -= o2HealthDecay * 2; // Decrease health rapidly
                healthSlider.value = playerHealth;

                Debug.Log($"No active O2 tank! Rapid health decay. Current health: {playerHealth}");

                if (playerHealth <= 0)
                {
                    GameOver();
                }
            }
        }
    }

    private void ApplyForceToDraggable(DraggableImage draggableImage)
    {
        if (draggableImage != null)
        {
            draggableImage.isXAxisLocked = false; // Unlock the X-axis

            Rigidbody2D rb2D = draggableImage.GetComponent<Rigidbody2D>();
            if (rb2D != null)
            {
                rb2D.AddForce(new Vector2(xForce, 0), ForceMode2D.Impulse);
                rb2D.AddTorque(zTorque, ForceMode2D.Impulse);

                Debug.Log($"Force applied to draggable: X={xForce}, Z torque={zTorque}");
            }
            else
            {
                Debug.LogError("Rigidbody2D is NULL on draggable image.");
            }
        }
        else
        {
            Debug.LogError("DraggableImage is NULL. Cannot apply force.");
        }
    }

    private bool IsAnyO2TankActive()
    {
        O2TankState[] o2Tanks = FindObjectsOfType<O2TankState>();
        foreach (var tank in o2Tanks)
        {
            if (tank.IsActive)
            {
                Debug.Log("Active O2 tank found.");
                return true;
            }
        }

        Debug.LogWarning("No active O2 tanks found.");
        return false;
    }

    public void UpdateO2FromSlider()
    {
        if (o2Slider != null)
        {
            playerO2 = o2Slider.value;
            Debug.Log($"Player O2 updated from slider to {playerO2}.");
        }
        else
        {
            Debug.LogError("O2 Slider is NULL. Cannot update O2.");
        }
    }

    public void PlayerKickO2Consume()
    {
        if (o2DecayOn)
        {
            playerO2 -= (o2Decay * kickO2Drain);
            Debug.Log($"Player O2 consumed during kick. Current O2: {playerO2}");
        }
    }

    public void HurtPlayer(float damage)
    {
        playerHealth -= damage;
        Debug.Log($"Player hurt. Damage: {damage}, Current Health: {playerHealth}");

        if (healthSlider == null)
        {
            Debug.LogError("Health Slider is NULL. Cannot update health.");
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

    public void HealPlayer()
    {
        if (healthPacks > 0)
        {
            healthPacks--;
            playerHealth = 100;
            healthSlider.value = playerHealth;
            healthPacksRemaining.text = healthPacks.ToString();

            Debug.Log($"Player healed. Health packs remaining: {healthPacks}");
        }
        else
        {
            Debug.LogWarning("No health packs remaining. Cannot heal player.");
        }
    }

    public void RefillO2()
    {
        if (o2Tanks > 0)
        {
            o2Tanks--;
            playerO2 = 100;
            o2Slider.value = playerO2;
            o2Remaining.text = o2Tanks.ToString();

            Debug.Log($"O2 refilled. O2 tanks remaining: {o2Tanks}");
        }
        else
        {
            Debug.LogWarning("No O2 tanks remaining. Cannot refill O2.");
        }
    }

    public void GameOver()
    {
        Debug.LogError("Game Over triggered. Stopping time.");
        Time.timeScale = 0f;
    }

    public void AcceptLoadoutVariables()
    {
        healthPacks = loadoutManager.healthPacks;
        o2Tanks = loadoutManager.o2Tanks;
        ammoPacks = loadoutManager.ammoPacks;

        Debug.Log($"Loadout variables accepted. Health packs: {healthPacks}, O2 tanks: {o2Tanks}, Ammo packs: {ammoPacks}");
    }
}
