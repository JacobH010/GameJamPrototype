using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public Dictionary<GameObject, int> MasterLootDictionary = new Dictionary<GameObject, int>();
    private int nonDuplicateThreshold = 3;
    void Awake()
    {
        PopulateLootDictionary();
        //Debug.Log("Dictionary Populated ran");
    }
    public void PopulateLootDictionary()
    {
        //Loads all game objects within the Artifacts directory in Resources
        GameObject[] artifacts = Resources.LoadAll<GameObject>("Artifacts");

        foreach (GameObject artifact in artifacts)
        {
            ScannerItems itemScript = artifact.GetComponent<ScannerItems>();
            if (itemScript != null)
            {
                int commonality = itemScript.commonality;
                MasterLootDictionary.Add(artifact, commonality);
                //Debug.Log(artifact.ToString()  + " " + commonality + " Added to Dictionary"); 
            }
            else
            {
                Debug.LogWarning($"Prefab {artifact.name} does not have a ScannerItems component");
            }
        }
        Debug.Log($"MasterLootDictionary count: {MasterLootDictionary.Count}");
    }
    public List<GameObject> GetArtifactsByMinCommonality(int minCommonality)
    {
        //Debug.Log("GetArtifactsByMinCommonality ran");
        List<GameObject> filteredArtifacts = new List<GameObject>();
        Dictionary<GameObject, int> lootDictionary = MasterLootDictionary;
        foreach (var kvp in lootDictionary)
        {
            if (kvp.Value >= minCommonality)
            {
                filteredArtifacts.Add(kvp.Key);
            }
        }
        Debug.Log($"MasterLootDictionary count: {MasterLootDictionary.Count}");
        return filteredArtifacts;
    }
    //Gets a list of random artifacts from the dictionary. Prevents duplicates of rare items benetth the nonDuplicateThreshold
    public List<GameObject> SelectRandomArtifacts(List<GameObject> filteredArtifacts, int maxItems)
    {
        // Make a copy of the list to avoid modifying the original during the process
        List<GameObject> artifactsCopy = new List<GameObject>(filteredArtifacts);
        List<GameObject> selectedArtifacts = new List<GameObject>();
        HashSet<GameObject> lowRarityItems = new HashSet<GameObject>(); // Track low-rarity items

        for (int i = 0; i < Mathf.Min(maxItems, artifactsCopy.Count); i++)
        {
            // Pick a random artifact
            int randomIndex = Random.Range(0, artifactsCopy.Count);
            GameObject artifact = artifactsCopy[randomIndex];

            // Check if it's low commonality and already selected
            if (artifact.GetComponent<ScannerItems>().commonality <= nonDuplicateThreshold && lowRarityItems.Contains(artifact))
            {
                i--; // Retry
                continue;
            }

            // Add artifact to the selected list
            selectedArtifacts.Add(artifact);

            // If low commonality, add to the exclusion set
            if (artifact.GetComponent<ScannerItems>().commonality <= nonDuplicateThreshold)
            {
                lowRarityItems.Add(artifact);
            }

            // Remove from the local copy to avoid duplicates
            artifactsCopy.RemoveAt(randomIndex);
        }
        Debug.Log($"MasterLootDictionary count: {MasterLootDictionary.Count}");
        return selectedArtifacts;
    }
}
