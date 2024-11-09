using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private float playerHealth = 100f;
    private float playerO2 = 100f;
    private float o2Decay = .14f; //Decay rate per half second. 
    private float o2DecayRate = .5f;//Rate of o2 Decay in seconds
    public bool o2DecayOn = true;
    private float kickO2Drain = 4;
    private float sprintMult = 2;
    public bool isSprinting;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Decrement02());
    }

    // Update is called once per frame
    
    IEnumerator Decrement02()
    {
        while (o2DecayOn)
        {
            yield return new WaitForSeconds(o2DecayRate);
            if (isSprinting)
            {
                playerO2 -= o2Decay * sprintMult;
            }
            else
            {
                playerO2 -= o2Decay;
            }
            Debug.Log("Player o2 decremented to " + playerO2);
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
    public void HurtPlayer(float damage)
    {
        playerHealth -= damage;
        if (playerHealth <= 0)
        {
            GameOver();
        }
    }
    public void GameOver()
    {
        Debug.Log("Game Over!!");
        Time.timeScale = 0f;
    }
}
