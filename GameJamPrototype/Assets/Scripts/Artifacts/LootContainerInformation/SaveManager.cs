using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        //Set the save file path in the persistent data folder
        savePath = Path.Combine(Application.persistentDataPath, "LootContainers.json");
    }
    public void SaveContainerData(LootContainerData containerData)
    {
        //Load existing data file
        List<LootContainerData> allContainers = LoadAllContainers();
        //Debug.Log(allContainers.ToString());
        //Replace or add the data for the current container
        int existingIndex = allContainers.FindIndex(data => data.containerID == containerData.containerID);
        if (existingIndex != -1)
        {
            allContainers[existingIndex] = containerData; //Update Existing
        }
        else
        {
            allContainers.Add(containerData);
        }

        //Convert the data list into JSON and save it
        string json = JsonUtility.ToJson(new LootContainerDataWrapper { containers = allContainers });
        File.WriteAllText(savePath, json);

        Debug.Log($"Saved container data to {savePath}");
    }

    public List<LootContainerData> LoadAllContainers()
    {
        if (!File.Exists(savePath))
        {
            return new List<LootContainerData>(); //No data in save yet
        }

        //Read JSON from file
        string json = File.ReadAllText(savePath);

        //Convert JSON back to data list
        LootContainerDataWrapper wrapper = JsonUtility.FromJson<LootContainerDataWrapper>(json);
        return wrapper?.containers ?? new List<LootContainerData>();
    }

    [System.Serializable]
    private class LootContainerDataWrapper
    {
        public List<LootContainerData> containers;
    }
}
