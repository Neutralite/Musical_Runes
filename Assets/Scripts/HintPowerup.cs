using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MusicalRunes;

public class HintPowerup : Powerup
{
    [SerializeField]
    private int RuneHintAmount => ((HintPowerupConfig)powerupConfig).GetHintAmount(currentLevel);

    private List<int> selectedRuneIndexes;
    private bool isActive;

    protected override void PerformPowerupEffect()
    {
        isActive = true;
        GameManager manager = GameManager.Instance;

        selectedRuneIndexes = Enumerable.Range(0, manager.BoardRunes.Count).OrderBy(index=> index == manager.CurrentRuneIndex?2:Random.value).ToList();

        selectedRuneIndexes.RemoveAt(selectedRuneIndexes.Count - 1);
        var selectedHintAmount = System.Math.Min(RuneHintAmount, selectedRuneIndexes.Count);
        selectedRuneIndexes = selectedRuneIndexes.GetRange(0, selectedHintAmount);

        StartCoroutine(AnimateHintPowerup());
    }
    protected override void OnRuneActivated()
    {
        base.OnRuneActivated();

        if (!isActive)
        {
            return;
        }
        var runes = GameManager.Instance.BoardRunes;
        foreach (var runeIndex in selectedRuneIndexes)
        {
            runes[runeIndex].SetHintVisual(false);
        }
        isActive = false;
        selectedRuneIndexes = null;
    }
    private IEnumerator AnimateHintPowerup()
    {
        // collect the boardrunes
        var runes = GameManager.Instance.BoardRunes;

        // for each rune selected by the player, show the hint for the next remaining rune in the sequence
        for (int index = 0; index < selectedRuneIndexes.Count; index++)
        {
            var runeIndex = selectedRuneIndexes[index];
            var rune = runes[runeIndex];

            if (index == selectedRuneIndexes.Count-1)
            {
                yield return rune.SetHintVisual(true);
            }
            else
            {
                rune.SetHintVisual(true);
            }
        }
        // as the hint is playing, set the player interactivity
        GameManager.Instance.SetPlayerInteractivity(true);
    }
}
