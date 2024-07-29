using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    [SerializeField] Animator[] canvasGroups;
    [SerializeField][Range(0f, 1.3f)] float lerpTimeValue;
    [SerializeField] Button AggrementContinueButton;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Transform leaderboardContent;
    [SerializeField] GameObject leaderboardEntryPrefab;

    private int currentIndex; // Updated variable name

    public float timeRemaining = .5f;
    public TextMeshProUGUI countdownText;
    private float time;

    [SerializeField] Image[] playingButtons;
    [SerializeField] Button[] buttons;

    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;

    private int currentButtonIndex;
    [HideInInspector] public int newRandomButton;
    [SerializeField] int AddScoreOnClick;
    [SerializeField] TextMeshProUGUI ScoreText;
    [SerializeField] TextMeshProUGUI completeScoreText;
    private int currentScore;
    private bool timerIsRunning = false;

    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField phoneNumberInputField;
    public TMP_InputField countryInputField;

    private List<PlayerData> players = new List<PlayerData>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        canvasGroups[0].SetBool("fadein", true);
        time = timeRemaining;
        ScoreText.text = "0";

        leaderboardButton.onClick.AddListener(ShowLeaderboard);

        // Load data if exists
        LoadPlayerData();
    }

    public void ActivatePanel(int index)
    {
        foreach (var cg in canvasGroups)
        {
            if (cg.GetBool("fadein"))
            {
                cg.SetBool("fadein", false);
                break; // Exit the loop once the active panel is found
            }
        }

        currentIndex = index;
        Invoke(nameof(FadeInNewPanel), lerpTimeValue);
    }

    private void FadeInNewPanel()
    {
        canvasGroups[currentIndex].SetBool("fadein", true);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.C)) {
            PlayerPrefs.DeleteAll();
            //Debug.Log(("gfjf"));
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateCountdownText(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                UpdateCountdownText(timeRemaining);
                OnCountdownFinished();
            }
        }
    }

    public void SelectRandomButton(bool isAddScore)
    {
        if (!timerIsRunning) return;

        ResetButtons();

        do
        {
            newRandomButton = Random.Range(0, playingButtons.Length);
        } while (newRandomButton == currentButtonIndex);

        currentButtonIndex = newRandomButton;
        ActivateButton(currentButtonIndex);

        if (isAddScore)
        {
            AddScore();
        }

        // Start coroutine only once per button change
        StopAllCoroutines(); // Stop any ongoing coroutines before starting a new one
        StartCoroutine(DelayedLightChange());
    }

    private void ResetButtons()
    {
        for (int i = 0; i < playingButtons.Length; i++)
        {
            buttons[i].interactable = false;
            playingButtons[i].sprite = offSprite;
            playingButtons[i].SetNativeSize();
        }
    }

    private void ActivateButton(int index)
    {
        buttons[index].interactable = true;
        playingButtons[index].sprite = onSprite;
        playingButtons[index].SetNativeSize();

        // Send UDP message
        UdpClientScript.instance.SendMessageToDevice(index );
    }

    private void AddScore()
    {
        currentScore += AddScoreOnClick;
        ScoreText.text = currentScore.ToString();
    }

    private IEnumerator DelayedLightChange()
    {
        yield return new WaitForSeconds(10f); // Adjust this time for desired delay
        playingButtons[currentButtonIndex].sprite = offSprite;
        playingButtons[currentButtonIndex].SetNativeSize();
        buttons[currentButtonIndex].interactable = false;
        SelectRandomButton(false);
    }

    private void UpdateCountdownText(float time)
    {
        float seconds = Mathf.FloorToInt(time % 60);
        countdownText.text = string.Format("{0} seconds", seconds);
    }

    private void OnCountdownFinished()
    {
        completeScoreText.text = currentScore.ToString();
        SavePlayerData();
        ActivatePanel(4);
    }

    public void ActivatePlayPanel()
    {
        timerIsRunning = true;
        SelectRandomButton(false);
    }

    public void NewGame()
    {
        currentScore = 0;
        ScoreText.text = "0";
        timeRemaining = time;
        timerIsRunning = false;

        ResetButtons();

        nameInputField.text = "";
        emailInputField.text = "";
        phoneNumberInputField.text = "";
        countryInputField.text = "";

        ActivatePanel(0);
    }

    public void Agrre(GameObject AgreeIcon)
    {
        AgreeIcon.SetActive(true);
        AggrementContinueButton.interactable = true;
    }

    public void HandleButtonPress(int buttonIndex, bool isAddScore)
    {
        SelectRandomButton(isAddScore);
    }

    private void SavePlayerData()
    {
        string playerName = nameInputField.text;
        int playerScore = currentScore;

        // Save player data to the database
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();
        dbManager.InsertPlayerData(playerName, playerScore);
    }

    private void LoadPlayerData()
    {
        // Load players list from the database
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();
        players = dbManager.GetTopPlayers();

        if (players.Count > 0)
        {
            // Use the first player for input fields and score display
            var firstPlayer = players[0];
            //nameInputField.text = firstPlayer.Name;
            currentScore = firstPlayer.Score;
            ScoreText.text = currentScore.ToString();
        }
    }

    private void ShowLeaderboard()
    {
        if (leaderboardContent == null)
        {
            Debug.LogError("leaderboardContent is not assigned.");
            return;
        }

        if (leaderboardEntryPrefab == null)
        {
            Debug.LogError("leaderboardEntryPrefab is not assigned.");
            return;
        }

        // Clear existing entries
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // Get top players from the database
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();
        var topPlayers = dbManager.GetTopPlayers();

        // Populate leaderboard with player data
        foreach (var player in topPlayers)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContent);

            // Find the text components in the prefab
            TextMeshProUGUI nameText = entry.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entry.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null && scoreText != null)
            {
                nameText.text = player.Name;
                scoreText.text = player.Score.ToString();
            }
            else
            {
                Debug.LogError("One or more text components are missing in the leaderboard entry prefab.");
            }
        }

        // Optionally update the layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardContent.GetComponent<RectTransform>());

        ActivatePanel(5); // Activate the leaderboard panel
    }

    public class PlayerData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
