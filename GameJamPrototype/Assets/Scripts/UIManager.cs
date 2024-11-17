using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public float playerHealth = 100f;
    private float playerO2 = 100f;
    private float o2Decay = .1f; //Decay rate per half second. 
    private float o2DecayRate = .5f;//Rate of o2 Decay in seconds
    public bool o2DecayOn = true;
    private float o2HealthDecay = 2.5f;
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


    public Slider healthSlider;
    public Slider o2Slider;

    private LoadoutManager loadoutManager;
    
    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    
    IEnumerator Decrement02()
    {
        while (o2DecayOn)
        {
            yield return new WaitForSeconds(o2DecayRate);
            if (playerO2 >= 0)
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
            }
            else if (playerO2 < 0)
            {
                playerHealth -= o2HealthDecay ;
                healthSlider.value = playerHealth;
            }
            o2Slider.value = playerO2;
            yield return null;
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
        healthPacksRemaining.text = healthPacks.ToString();
        o2Remaining.text = o2Tanks.ToString();

        Debug.Log("health packs selected = " + healthPacks + ". O2 tanks selected = " + o2Tanks + ". Ammo Selected = " + ammoPacks + ".");
    }
}
