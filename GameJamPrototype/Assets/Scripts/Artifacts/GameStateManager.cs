using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager gameStateManager;

    private Dictionary<string, ContainerState> containerStates = new Dictionary<string, ContainerState>();
    // Start is called before the first frame update
    void Awake()
    {
        if (gameStateManager == null) gameStateManager = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SaveContainerState(ContainerState state)
    {
        containerStates[state.name] = state;
    }
    public ContainerState GetContainerState(string containerID)
    {
        if (containerStates.ContainsKey(containerID))
            return containerStates[containerID];
        return null;
    }
    public void SaveAllStatesToFile()
    {
        //Convirt Dictionary to JSON and save to file
    }
    public void LoadAllStatesFromFile()
    {
        //Load JSON and populate containerStates
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
