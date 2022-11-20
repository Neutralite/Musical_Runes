using System;
using System.Collections.Generic;
using UnityEngine;
using MusicalRunes;
using UnityEngine.UIElements;

[Serializable]
public class SaveData
{
    public int coinsAmount;
    public int highScore;

    [Serializable]
    private class PowerupSaveData
    {
        public PowerupType Type;
        public int Level;
    }

    [SerializeField]
    private List<PowerupSaveData> powerupSaveDataSerializable;
    private Dictionary<PowerupType,PowerupSaveData> powerupSaveData;

    public string Serialize()
    {
        powerupSaveDataSerializable.Clear();
        foreach (var pair in powerupSaveData)
        {
            powerupSaveDataSerializable.Add(pair.Value);
        }

        return JsonUtility.ToJson(this);
    }

    public static SaveData Deserialize(string jsonString)
    {
        SaveData newSaveData = JsonUtility.FromJson<SaveData>(jsonString);
        foreach (var data in newSaveData.powerupSaveDataSerializable)
        {
            newSaveData.powerupSaveData.Add(data.Type, data);
        }

        return newSaveData;

    }

    #region Constructors
    public SaveData()
    {
        powerupSaveDataSerializable = new List<PowerupSaveData>();
        powerupSaveData = new Dictionary<PowerupType, PowerupSaveData>();
    }
    public SaveData(bool createDefaults):this()
    {
        foreach (PowerupType upgradeableType in Enum.GetValues(typeof(PowerupType)))
        {
            powerupSaveData[upgradeableType] = new PowerupSaveData
            {
                Type = upgradeableType,
                Level = 0
            };
        }
    }
    #endregion

    public int GetUpgradeableLevel(PowerupType powerupType)
    {
        return powerupSaveData[powerupType].Level;
    }
    public void SetUpgradeableLevel(PowerupType powerupType,int level)
    {
        powerupSaveData[powerupType].Level = level;
    }
}
