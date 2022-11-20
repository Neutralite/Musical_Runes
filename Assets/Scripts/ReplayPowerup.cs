using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayPowerup : Powerup
{
    [SerializeField]
    private int playedRunesRequired = 2;
    protected override void PerformPowerupEffect()
    {
        if (GameManager.Instance.CurrentPlayIndex>=playedRunesRequired)
        {
            GameManager.Instance.PlaySequencePreview();
        }
        else
        {
            //for (int i = 0; i < cooldownDuration; i++)
            //{
            //    OnSequenceCompleted();
            //}
            //Debug.Log($"Play {playedRunesRequired - GameManager.Instance.CurrentPlayIndex} more rune(s)!");
        }
    }
}
