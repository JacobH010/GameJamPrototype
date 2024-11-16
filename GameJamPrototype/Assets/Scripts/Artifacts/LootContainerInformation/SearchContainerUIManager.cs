using UnityEngine;
using UnityEngine.UI;

public class SearchContainerUIManager : MonoBehaviour
{
    public static SearchContainerUIManager searchUIManager { get; private set; }
    public Button closeButton;

    private void Awake()
    {
        if (searchUIManager != null && searchUIManager != this)
        {
            Destroy(gameObject);
            return;
        }
        searchUIManager = this;
    }

    public void SetCloseButton(UnityEngine.Events.UnityAction action)
    {
        closeButton.onClick.RemoveAllListeners(); // Clear previous listeners
        closeButton.onClick.AddListener(action); // Add the new action
    }
}