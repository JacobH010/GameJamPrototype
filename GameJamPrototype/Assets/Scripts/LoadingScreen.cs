using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


    public class LoadingScreen : MonoBehaviour
    {
        // [SerializeField] public GameObject loadingScreen;
        [SerializeField] public Slider progressBar;

    public void Awake()
    {
        progressBar.value = 0;
    }
    public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            // Activate the loading screen
            //loadingScreen.SetActive(true);

            // Start the asynchronous scene loading
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                // Update the progress bar (progress ranges from 0 to 0.9)
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                progressBar.value = progress;

                // If the scene is loaded, activate it
                if (operation.progress >= 0.9f)
                {
                    // Optional: Wait for user input or a timed delay
                    yield return new WaitForSeconds(.5f);
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
