using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using MusicalRunes;
using System.Threading;
using System.Linq;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    private readonly string saveKey = "SaveKey";

    public static GameManager Instance
    {
        get;
        private set;
    }

    [Header("Rune Settings")]
    public int initialSequenceSize = 3;
    [SerializeField]
    private float delayBetweeenRunePreview = 0.3f;
    [SerializeField]
    private int initialBoardSize = 4;

    // A5 variable: extra rune per x completed sequences
    [SerializeField]
    int extraRuneRate = 5;
    [SerializeField]
    int shuffleRuneRate = 5;

    [SerializeField]
    private RectTransform runesHolder;
    [SerializeField]
    private List<Rune> availableRunePrefabs;
    [SerializeField]
    public List<Rune> BoardRunes
    {
        get;
        private set;
    }

    [SerializeField]
    private List<Rune> instantiatedBoardRunes;

    /// <summary>
    /// Keep track of the current rune sequence
    /// </summary>
    [SerializeField]
    private List<int> currentRuneSequence;

    /// <summary>
    /// Current index of the Rune that's been played
    /// </summary>
    /// 
    private int currentPlayIndex;
    public int CurrentPlayIndex
    {
        get => currentPlayIndex;
    }

    [SerializeField]
    float timeLimit = 7f;
    [NonSerialized]
    public bool isRuneChoosingTime;
    private float remainingTime;
    public int CurrentRuneIndex => currentRuneSequence[currentPlayIndex];

    [Header("Coin Settings")]
    [SerializeField]
    private int coinsPerRune = 1;
    [SerializeField]
    private int coinsPerRound = 10;
    [SerializeField]
    int comboCoinsPerRune = 5;
    float chooseTimeForCombo = 2f;

    [Header("Preview Settings")]
    [SerializeField]
    private GameObject[] spinlights;

    [Header("Powerup Settings")]
    [SerializeField]
    private List<Powerup> powerups;

    [Header("UI References")]
    [SerializeField]
    private TMP_Text coinsAmountText;
    [SerializeField]
    private TMP_Text highScoreText;
    [SerializeField]
    private TMP_Text remainingTimeText;
    [SerializeField]
    private TMP_Text remainingLivesText;
    [SerializeField]
    private Announcer announcer;


    public Action<int> coinsChanged;
    public Action sequenceCompleted;
    public Action runeActivated;

    public delegate void OnPowerupUpgradedDelegate(PowerupType upgradePowerup, int newLevel);

    public OnPowerupUpgradedDelegate powerupUpgraded;

    public int coinsAmount
    {
        get => saveData.coinsAmount;
        set
        {
            saveData.coinsAmount = value;
            coinsAmountText.text = coinsAmount.ToString();

            // trigger the coins changed action
            coinsChanged?.Invoke(value);
        }
    }

    private int highScore
    {
        get => saveData.highScore;
        set
        {
            saveData.highScore = value;
            highScoreText.text = highScore.ToString();
        }
    }

    [SerializeField]
    int initialLives = 3;
    private int remainingLives;
    public int RemainingLives
    {
        get => remainingLives;
        private set
        {
            remainingLives = value;
            remainingLivesText.text = remainingLives.ToString();
        }
    }

    private int currentRound;
    private SaveData saveData;

    public int GetPowerupLevel(PowerupType powerupType)
    {
        return saveData.GetUpgradeableLevel(powerupType);
    }
    public void UpgradePowerup(PowerupType powerupType, int price)
    {
        if (price > coinsAmount)
        {
            throw new Exception("Ain't got funds!");
        }

        coinsAmount -= price;
        var newLevel = GetPowerupLevel(powerupType) + 1;
        saveData.SetUpgradeableLevel(powerupType, newLevel);
        Save();

        powerupUpgraded?.Invoke(powerupType, newLevel);
    }
    void Awake()
    {
        if (Instance != null)
        {
            throw new System.Exception($"Multiple game managers in scene! {Instance} :: {this}");
        }
        Instance = this;
        LoadSaveData();
        ResetSaveData();
        InitializeBoard();
        InitializeSequence();
        InitializeUI();
        RemainingLives = initialLives;
        StartCoroutine(PlaySequencePreviewCoroutine(2));
    }
    void ResetSaveData()
    {
        saveData.coinsAmount = 0;
        saveData.highScore = 0;
        saveData = new SaveData(true);
        Save();
    }
    private void InitializeUI()
    {
        highScoreText.text = saveData.highScore.ToString();
        coinsAmountText.text = coinsAmount.ToString();
        remainingTime = timeLimit;
        remainingTimeText.text = remainingTime.ToString("F1");
    }

    private void Reset()
    {
        for (int i = runesHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(runesHolder.GetChild(i).gameObject);
        }

        //if(instantiatedBoardRunes.Count>0)
        availableRunePrefabs.AddRange(instantiatedBoardRunes);

        InitializeBoard();
        InitializeSequence();
        RemainingLives = initialLives;
    }

    private void AddRandomRuneToBoard()
    {
        var runePrefab = availableRunePrefabs[UnityEngine.Random.Range(0, availableRunePrefabs.Count)];

        availableRunePrefabs.Remove(runePrefab);

        instantiatedBoardRunes.Add(runePrefab);

        var rune = Instantiate(runePrefab, runesHolder);

        rune.SetUp(BoardRunes.Count);

        BoardRunes.Add(rune);
    }

    private void InitializeBoard()
    {
        BoardRunes = new List<Rune>(initialBoardSize);
        instantiatedBoardRunes = new List<Rune>();

        for (int i = 0; i < initialBoardSize; i++)
        {
            AddRandomRuneToBoard();
        }
    }

    public void OnRuneActivated(int index)
    {

        // TODO: prevent rune clicks when sequence is finished
        if (currentPlayIndex >= currentRuneSequence.Count)
        {
            return;
        }

        if (currentRuneSequence[currentPlayIndex] == index)
        {
            CorrectRuneSelected();
        }
        else
        {
            FailedChoice();
        }
    }

    private void InitializeSequence()
    {
        currentRuneSequence = new List<int>(initialSequenceSize);

        for (int i = 0; i < initialSequenceSize; i++)
        {
            currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        }
    }

    public Coroutine PlaySequencePreview(float startDelay = 1, bool resetPlayIndex = true)
    {
        
        if (resetPlayIndex)
        {
            currentPlayIndex = 0;
        }

        return StartCoroutine(PlaySequencePreviewCoroutine(startDelay));
    }

    private IEnumerator PlaySequencePreviewCoroutine(float startDelay = 1)
    {
        isRuneChoosingTime = false;
        InitializeUI();
        SetPlayerInteractivity(false);
        yield return new WaitForSeconds(startDelay);

        // TODO: Animate each rune in turn
        EnablePreviewFeedback();
        string sequence = "Sequence: ";

        foreach (var index in currentRuneSequence)
        {
            yield return BoardRunes[index].ActivateRuneCoroutine();
            yield return new WaitForSeconds(delayBetweeenRunePreview);
            sequence += $"{index}, ";
        }

        Debug.Log(sequence);
        DisablePreviewFeedback();
        SetPlayerInteractivity(true);
        isRuneChoosingTime = true;
    }

    public void SetPlayerInteractivity(bool interactable)
    {
        foreach (var rune in BoardRunes)
        {
            if (interactable)
            {
                rune.EnableInteraction();
            }
            else
            {
                rune.DisableInteraction();
            }
        }

        foreach (var powerup in powerups)
        {
            powerup.Interactable = interactable;
        }
    }

    /// <summary>
    /// Sequence has finished and is incorrect
    /// </summary>
    private IEnumerator FailedSequence(bool choseWrongRune)
    {
        isRuneChoosingTime = false;
        SetPlayerInteractivity(false);

        if (choseWrongRune)
        {
            announcer.ShowWrongRuneText();
        }
        else
        {
            announcer.ShowFailedByTimeoutText();
        }
        yield return new WaitForSeconds(2);

        if (currentRound > highScore)
        {
            highScore = currentRound;
            announcer.ShowHighScoreText(highScore);
            Save();
            yield return new WaitForSeconds(3);
        }

        Reset();

        currentPlayIndex = 0;
        currentRound = 0;

        yield return PlaySequencePreview(2);

    }

    /// <summary>
    /// When your sequence has finished
    /// </summary>
    private void CompletedSequence()
    {
        coinsAmount += coinsPerRound;

        currentRound++;
        Save();




        // trigger the sequence completed action
        sequenceCompleted?.Invoke();

        // A5 function change: attempt to add an extra rune
        AddExtraRune();

        ShuffleBoard();

        currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        currentPlayIndex = 0;

        StartCoroutine(PlaySequencePreviewCoroutine(2));
    }

    private void ShuffleBoard()
    {
        if (currentRound % shuffleRuneRate == 0)
        {
            var newOrder = Enumerable.Range(0, BoardRunes.Count).OrderBy(_ => UnityEngine.Random.value).ToList();

            BoardRunes = BoardRunes.OrderBy(rune => newOrder.FindIndex(order=>order==rune.Index)).ToList();

            for (var sequenceIndex = 0; sequenceIndex<currentRuneSequence.Count;sequenceIndex++)
            {
                var runeIndex = currentRuneSequence[sequenceIndex];
                currentRuneSequence[sequenceIndex] = newOrder.FindIndex(order => order == runeIndex);
            }
            for (int index = 0; index < BoardRunes.Count; index++)
            {
                BoardRunes[index].SetUp(index);
            }
        }
    }

    /// <summary>
    /// When the player has selected the right Rune
    /// </summary>
    private void CorrectRuneSelected()
    {

        // detect your quick rune choice combo
        var combo = remainingTime > timeLimit - chooseTimeForCombo && currentPlayIndex > 0;

        //add those coins
        coinsAmount += combo ? comboCoinsPerRune :coinsPerRune;

        runeActivated?.Invoke();
        InitializeUI();
        currentPlayIndex++;

        if (currentPlayIndex >= currentRuneSequence.Count)
        {
            CompletedSequence();
        }
        else
        {
            Save();
        }
    }

    private void EnablePreviewFeedback()
    {
        foreach (var spinlight in spinlights)
        {
            spinlight.SetActive(true);
        }
        announcer.ShowPreviewText();
    }

    private void DisablePreviewFeedback()
    {
        foreach (var spinlight in spinlights)
        {
            spinlight.SetActive(false);
        }
        announcer.ShowSequenceText();
    }

    private void Update()
    {
        if (!isRuneChoosingTime)
        {
            return;
        }
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0, remainingTime);

        remainingTimeText.text = remainingTime.ToString("F1");

        if (Mathf.Approximately(remainingTime, 0))
        {
            FailedChoice(false);
        }
    }

    private void FailedChoice(bool choseWrongRune = true)
    {
        // implement remaining lives
        RemainingLives--;
        if (RemainingLives == 0)
        {
            StartCoroutine(FailedSequence(choseWrongRune));
        }
        
        // show announcer text
        if (choseWrongRune)
        {
            announcer.ShowWrongRuneText();
        }
        else
        {
            announcer.ShowTimeoutText();
        }

        //reset the rune timer
        InitializeUI();
    }

    private void LoadSaveData()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string serializedSaveData = PlayerPrefs.GetString(saveKey);
            saveData = SaveData.Deserialize(serializedSaveData);
            Debug.Log("Loaded: " + serializedSaveData);

            return;
        }
        saveData = new SaveData(true);
    }
    private void Save()
    {
        string serializedSaveData = saveData.Serialize();
        PlayerPrefs.SetString(saveKey, serializedSaveData);
        Debug.Log("Saved: " + serializedSaveData);
    }

    // A5 function: add extra rune every x completed sequences, if there are available runes to add
    private void AddExtraRune()
    {
        if(currentRound%extraRuneRate == 0 && availableRunePrefabs.Count>0)
        {
            AddRandomRuneToBoard();
        }
    }
}
