using MusicalRunes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MusicalRunes
{
    public class PowerUpgradePopup : MonoBehaviour, iLocalizable
    {
        [SerializeField]
        private TMP_Text powerNameText,
                         powerLevelText,
                         powerDescriptionText,
                         powerUpgradePriceText;
        [SerializeField]
        private Image coinIconImage;
        [SerializeField]
        private Button purchaseButton;
        [SerializeField]
        private Image purchaseButtonImage;

        public Color purchaseAvailableTextColor = new Color(80, 220, 65);
        public Color purchaseDisabledTextColor = new Color(230, 75, 90);
        public Color purchaseDisabledButtonColor = new Color(170, 170, 170);

        [SerializeField]
        private PowerupConfig config;
        private int currentLevel;

        public void Setup(PowerupConfig powerupConfig)
        {
            config = powerupConfig;
            currentLevel = GameManager.Instance.GetPowerupLevel(config.powerupType);
            powerNameText.text = config.PowerupName;
            powerLevelText.text = currentLevel.ToString();
            powerDescriptionText.text = config.Description;
            powerUpgradePriceText.text = config.GetUpgradePrice(currentLevel).ToString();

            // change to <= if that's correct
            var hasEnoughCoins = GameManager.Instance.coinsAmount >= config.GetUpgradePrice(currentLevel);
            powerUpgradePriceText.color = hasEnoughCoins ? purchaseAvailableTextColor : purchaseDisabledTextColor;
            purchaseButton.interactable = hasEnoughCoins;

            var tintColor = hasEnoughCoins ? Color.white : purchaseDisabledButtonColor;
            purchaseButtonImage.color = tintColor;
            coinIconImage.color = tintColor;
            purchaseButton.gameObject.SetActive(config.MaxLevel!=currentLevel);
            gameObject.SetActive(true);

            GameManager.Instance.isRuneChoosingTime = false;
        }

        public void ClosePopup()
        {
            gameObject.SetActive(false);
            GameManager.Instance.isRuneChoosingTime = true;
        }

        private void OnClick()
        {
            GameManager.Instance.UpgradePowerup(config.powerupType,config.GetUpgradePrice(currentLevel));
            ClosePopup();
        }

        public void LocaleChanged()
        {
            if (config == null) { return; }
            powerNameText.text = config.PowerupName;
            powerDescriptionText.text = config.Description;
        }

        private void Awake()
        {
            purchaseButton.onClick.AddListener(OnClick);
            //gameObject.SetActive(false);
            Localization.RegisterWatcher(this);
        }
        private void OnDestroy()
        {
            Localization.DeregisterWatcher(this);
        }

    }

}
