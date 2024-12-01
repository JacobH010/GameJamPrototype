using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager loadoutManager { get; private set; }
    public LoadingScreen loadingScreen;

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

    private void Awake()
    {
        if (loadoutManager != null && loadoutManager != this)
        {
            Destroy(gameObject);
            return;
        }

        loadoutManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadoutScene")
        {
            RebindComponents();
        }
    }

    private void InitializeUI()
    {
        if (healthPacksSlider != null)
        {
            healthPacksSlider.value = 0;
            o2TanksSlider.value = 0;
            ammoPacksSlider.value = 0;

            // Add listeners
            healthPacksSlider.onValueChanged.AddListener(UpdateLoadout);
            o2TanksSlider.onValueChanged.AddListener(UpdateLoadout);
            ammoPacksSlider.onValueChanged.AddListener(UpdateLoadout);

            startGameButton.onClick.AddListener(StartGame);
            UpdateLoadout(0); // Initial call to set values
        }
    }

    private void RebindComponents()
    {
        // Get the root GameObjects in the active scene
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            // Find the UI components dynamically
            if (root.name == "UI") // Replace with the appropriate parent object name if applicable
            {
                healthPacksSlider = root.transform.Find("HealthPacksSlider").GetComponent<Slider>();
                o2TanksSlider = root.transform.Find("O2TanksSlider").GetComponent<Slider>();
                ammoPacksSlider = root.transform.Find("AmmoPacksSlider").GetComponent<Slider>();

                healthPackText = root.transform.Find("HealthPackText").GetComponent<TextMeshProUGUI>();
                o2TankText = root.transform.Find("O2TankText").GetComponent<TextMeshProUGUI>();
                ammoPackText = root.transform.Find("AmmoPackText").GetComponent<TextMeshProUGUI>();

                startGameButton = root.transform.Find("StartGameButton").GetComponent<Button>();
                remainingSlotsSlider = root.transform.Find("RemainingSlotsSlider").GetComponent<Slider>();
                remainingSlotsText = root.transform.Find("RemainingSlotsText").GetComponent<TextMeshProUGUI>();

                InitializeUI(); // Reinitialize the UI functionality
            }
        }
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
            loadingScreen.LoadScene(SceneToLoad);
        }
        else
        {
            Debug.LogError("Not enough available slots to start the game.");
        }
    }

    public void ResetLoadout()
    {
        healthPacksSlider.value = 0;
        o2TanksSlider.value = 0;
        ammoPacksSlider.value = 0;

        UpdateLoadout(0); // Reset values and UI
    }
}
