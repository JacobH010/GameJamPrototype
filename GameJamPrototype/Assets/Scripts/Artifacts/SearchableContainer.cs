using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchableContainer : MonoBehaviour
{
    private static HashSet<string> generatedIDs = new HashSet<string>();

    public string containerID;

    private void Start()
    {
        containerID = GenerateUniqueID();
        
    }
    private string GenerateUniqueID()
    {
        string newID;
        do
        {
            string part1 = Random.Range(0, 500).ToString();
            string part2 = Random.Range(0, 500).ToString();
            string part3 = Random.Range(0, 500).ToString();
            string part4 = Random.Range(0, 500).ToString();

            newID = $"{part1}-{part2}-{part3}-{part4}";
        } while (generatedIDs.Contains(newID));
        generatedIDs.Add(newID);
        return newID;
    }
    public void OpenContainer()
    {
        Debug.Log("Opened container with " + containerID);
    }
}
