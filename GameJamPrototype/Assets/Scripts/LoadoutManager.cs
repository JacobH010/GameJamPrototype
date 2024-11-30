using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager loadoutManager { get; private set; }
    // Inventory settings
    public float healthPacks { get; private set; }
    public float o2Tanks { get; private set; }
    public float ammoPacks { get; private set; }
    private float availableSlots;
    public Color defaultTextColor;
    public string SceneToLoad;
    [Header("UI Components")]
    public Slider healthPacksSlider;
    public Slider o2TanksSlider;
    public Slider ammoPacksSlider;
    public TextMeshProUGUI healthPackText;
    public TextMeshProUGUI o2TankText;
    public TextMeshProUGUI ammoPackText;
    public Button startGameButton;
    public Slider remainingSlotsSlider;
    public TextMeshProUGUI remainingSlotsText;

   
    

    private PlayerController playerController;

    private void Awake()
    {
        if (loadoutManager == null)
        {
            loadoutManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    void Start()
    {
        healthPacksSlider.value = 0;
        o2TanksSlider.value = 0;
        ammoPacksSlider.value = 0;

        playerController = FindObjectOfType<PlayerController>();

        // Add listener for sliders
        healthPacksSlider.onValueChanged.AddListener(UpdateLoadout);
        o2TanksSlider.onValueChanged.AddListener(UpdateLoadout);
        ammoPacksSlider.onValueChanged.AddListener(UpdateLoadout);

        // Add listener for the start game button
        startGameButton.onClick.AddListener(StartGame);

        UpdateLoadout(0); // Initial call to set values
    }

    private void UpdateLoadout(float value)
    {
        // Retrieve current values from sliders
        healthPacks = healthPacksSlider.value;
        o2Tanks = o2TanksSlider.value;
        ammoPacks = ammoPacksSlider.value;

        // Calculate available slots based on max allowed slots (12)
        availableSlots = 12f - (healthPacks + (o2Tanks * 2f) + ammoPacks);

        // Display updated values if they are within allowed limits
        if (availableSlots >= 0)
        {
            healthPackText.text = healthPacks.ToString();
            o2TankText.text = o2Tanks.ToString();
            ammoPackText.text = ammoPacks.ToString();
            remainingSlotsText.color = defaultTextColor;
            remainingSlotsSlider.value = availableSlots;
            
        }
        else
        {
            remainingSlotsText.color = Color.red;
            Debug.LogWarning("Cannot exceed available slots.");
        }

    }

    private void StartGame()
    {
        if (availableSlots >= 0)
        {
            // Find the MainMenuMusic GameObject and stop/destroy it
            GameObject mainMenuMusic = GameObject.Find("MainMenuMusic");
            if (mainMenuMusic != null)
            {
                Destroy(mainMenuMusic); // This will stop the music and remove the object
            }

            // Load the new scene
            SceneManager.LoadScene(SceneToLoad);
        }
        else
        {
            Debug.LogError("Not enough available slots to start the game.");
        }
    }

}