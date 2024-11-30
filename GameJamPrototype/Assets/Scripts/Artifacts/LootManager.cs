using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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
            // Calculate total weight of all remaining artifacts
            int totalWeight = 0;
            foreach (var artifact in artifactsCopy)
            {
                totalWeight += artifact.GetComponent<ScannerItems>().commonality;
            }

            // Select a random weight value
            int randomWeight = Random.Range(0, totalWeight);
            GameObject selectedArtifact = null;

            // Iterate to find the artifact corresponding to the random weight
            int cumulativeWeight = 0;
            foreach (var artifact in artifactsCopy)
            {
                cumulativeWeight += artifact.GetComponent<ScannerItems>().commonality;
                if (randomWeight < cumulativeWeight)
                {
                    selectedArtifact = artifact;
                    break;
                }
            }

            // Check if the selected artifact is low commonality and already selected
            if (selectedArtifact.GetComponent<ScannerItems>().commonality <= nonDuplicateThreshold && lowRarityItems.Contains(selectedArtifact))
            {
                i--; // Retry
                continue;
            }

            // Add the selected artifact to the result list
            selectedArtifacts.Add(selectedArtifact);

            // If low commonality, add to the exclusion set
            if (selectedArtifact.GetComponent<ScannerItems>().commonality <= nonDuplicateThreshold)
            {
                lowRarityItems.Add(selectedArtifact);
            }

            // Remove the selected artifact from the local copy
            artifactsCopy.Remove(selectedArtifact);
        }

        return selectedArtifacts;
    }
}
