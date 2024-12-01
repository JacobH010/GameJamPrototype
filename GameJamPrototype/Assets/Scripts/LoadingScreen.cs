using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] public Slider progressBar;
    public AudioSource elevatorOpen;

    private bool hasButtonBeenPressed = false; // Tracks if the button was pressed

    private void Awake()
    {
        progressBar.value = 0;
        Debug.Log("LoadingScreen Awake: ProgressBar initialized to 0.");
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (hasButtonBeenPressed)
        {
            Debug.LogWarning("Start game button has already been pressed!");
            return; // Exit if the button was already pressed
        }

        hasButtonBeenPressed = true; // Set the flag to true
        Debug.Log($"LoadScene called with scene name: {sceneName}");
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"Start loading scene: {sceneName}");

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        Debug.Log($"SceneManager started async loading for scene: {sceneName}. allowSceneActivation is set to false.");

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            Debug.Log($"Loading progress: {progress * 100}%");

            if (operation.progress >= 0.9f)
            {
                Debug.Log($"Scene {sceneName} is 90% loaded. Preparing to activate the scene.");

                if (elevatorOpen != null)
                {
                    Debug.Log("Playing elevator open sound.");
                    elevatorOpen.Play();

                    Debug.Log("Waiting for elevator open sound to finish.");
                    yield return new WaitForSeconds(1.25f);

                    Debug.Log("1.25 seconds passed after elevator open sound.");

                    if (elevatorOpen.gameObject != null)
                    {
                        Debug.Log("Ensuring elevatorOpen GameObject is active.");
                        elevatorOpen.gameObject.SetActive(true);
                    }
                }

                Debug.Log("Waiting additional 0.75 seconds before scene activation.");
                yield return new WaitForSeconds(0.75f);

                operation.allowSceneActivation = true;
                Debug.Log($"Scene {sceneName} activation allowed.");
            }

            yield return null;
        }

        Debug.Log($"Scene {sceneName} loading completed.");
    }
}
