using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using MusicalRunes;
public abstract class Powerup : MonoBehaviour
{
    [SerializeField]
    protected PowerupConfig powerupConfig;


    [SerializeField]
    private Button powerupButton;
    [SerializeField]
    private RectTransform cooldownBar;
    protected int currentLevel;
    private int cooldownDuration => powerupConfig.GetCooldown(currentLevel);
    private float cooldownBarHeight;
    private int currentCooldown;

    public bool Interactable
    {
        get => powerupButton.interactable;

        set => powerupButton.interactable = IsAvailable && value;
    }

    protected bool IsAvailable => currentLevel>0 && currentCooldown <= 0;

    // Start is called before the first frame update
    void Start()
    {
        cooldownBarHeight = cooldownBar.sizeDelta.y;

        SetCooldownBarHeight();

        currentLevel = GameManager.Instance.GetPowerupLevel(powerupConfig.powerupType);

        powerupButton.onClick.AddListener(OnClick);

        GameManager.Instance.sequenceCompleted += OnSequenceCompleted;
        GameManager.Instance.runeActivated += OnRuneActivated;
    }

    private void OnPowerupUpgraded(PowerupType upgradedPowerup, int newLevel)
    {
        if (upgradedPowerup!=powerupConfig.powerupType)
        {
            return;
        }
        currentLevel = newLevel;

        Interactable = false;

    }

    protected abstract void PerformPowerupEffect();

    private void OnClick()
    {
        ResetCooldown();
        
        Interactable = false;

        PerformPowerupEffect();

        Debug.Assert(IsAvailable, "Sod off, power up unavailable", gameObject);
    }

    private void ResetCooldown()
    {
        currentCooldown = cooldownDuration;
        SetCooldownBarHeight();
    }

    protected virtual void OnSequenceCompleted()
    {
        if (powerupConfig.decreaseCooldownOnRuneActivation)
        {
            return;
        }

        DecreaseCooldown();
    }

    protected virtual void OnRuneActivated()
    {
        if (!powerupConfig.decreaseCooldownOnRuneActivation)
        {
            return;
        }

        DecreaseCooldown();
    }

    private void DecreaseCooldown()
    {
        // first check if cooldown is available
        if (IsAvailable)
        {
            return;
        }
        // decrease cooldown
        currentCooldown--;
        // make sure currentCooldown is between 0 and currentCooldown value
        currentCooldown = Mathf.Max(0, currentCooldown);
        // set the bar height visual
        SetCooldownBarHeight();
        // when cooldown is unavailable/available, set it's interactivity
        Interactable = IsAvailable;

    }

    private void SetCooldownBarHeight()
    {
        // normalizing the height from 0 -> 1
        var fraction = (float)currentCooldown / cooldownDuration;
        // apply the fraction calculated to cooldownbar size
        cooldownBar.sizeDelta = new Vector2(cooldownBar.sizeDelta.x, fraction * cooldownBarHeight);
    }

    private void OnDestroy()
    {
        GameManager.Instance.sequenceCompleted -= OnSequenceCompleted;
        GameManager.Instance.runeActivated -= OnRuneActivated;
    }

}
