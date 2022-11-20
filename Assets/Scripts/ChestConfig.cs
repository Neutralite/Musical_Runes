using System;
using UnityEngine;


[CreateAssetMenu(fileName = "ChestConfig", menuName = "Configs/ChestConfig")]
public class ChestConfig : ScriptableObject
{
    public RewardType rewardType;
    public int[] probabilities;
}

[System.Serializable]
public enum RewardType
{
    Coal,
    Copper,
    Iron,
    Gold,
    Diamond
}