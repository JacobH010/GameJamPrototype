using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartAIStageScene()
    {
        SceneManager.LoadScene("AIStageing");
    }
    public void StartGamePrototype()
    {
        SceneManager.LoadScene("LoadoutSelection");
    }
    public void StartMovementStage()
    {
        Debug.Log("Movement stage not initialized in Main Menu Manager");
    }
}
