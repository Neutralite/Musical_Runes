using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestPowerup : MonoBehaviour
{
    [SerializeField]
    ChestConfig[] chestConfigs;
    [SerializeField]
    ChestConfig[] activeChestConfigs = new ChestConfig[3];
    [SerializeField]
    Color[] colors;

    [SerializeField]
    GameObject rewardPopup,content,background;
    [SerializeField]
    TMP_Text rewardText;
    [SerializeField]
    Image rewardCoin;
    public void Setup()
    {

        for (int i = 0; i < 3; i++)
        {
            var temp = Random.Range(1, 3);
            activeChestConfigs[i] = chestConfigs[temp];
        }
        gameObject.SetActive(true);
        content.SetActive(true);
        background.SetActive(true);
    }
    public void Roll(int chestIndex)
    {
        var temp = Random.Range(0, 100);

        for (int i = 0; i < 5; i++)
        {
            if (temp > 0)
            {
                temp -= activeChestConfigs[chestIndex].probabilities[i];
            }
            if (temp <=0)
            {
                activeChestConfigs[chestIndex].rewardType = (RewardType)i;
                break;
            }
        }
        rewardText.text = ((int)activeChestConfigs[chestIndex].rewardType).ToString();
        rewardCoin.color = colors[(int)activeChestConfigs[chestIndex].rewardType];
        content.SetActive(false);
        background.SetActive(false);
        StartCoroutine(ShowReward());
        GameManager.Instance.coinsAmount += (int)activeChestConfigs[chestIndex].rewardType;
    }

    IEnumerator ShowReward()
    {
        rewardPopup.SetActive(true);
        yield return new WaitForSeconds(3);
        rewardPopup.SetActive(false);
        gameObject.SetActive(false);
    }
}
