using UnityEngine;

public class O2TankManager : MonoBehaviour
{
    private void Update()
    {
        // Find all O2TankState components in the scene
        O2TankState[] o2Tanks = FindObjectsOfType<O2TankState>();

        bool foundActiveTank = false;

        foreach (var tank in o2Tanks)
        {
            if (tank.IsActive)
            {
                if (foundActiveTank)
                {
                    Debug.LogWarning($"Multiple active O2 tanks detected. Deactivating {tank.gameObject.name}.");
                    tank.IsActive = false; // Force deactivate additional tanks
                }
                else
                {
                    foundActiveTank = true;
                }
            }
        }

        if (!foundActiveTank)
        {
            Debug.LogWarning("No active O2 tank found in the scene.");
        }
    }
}
