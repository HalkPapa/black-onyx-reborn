using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ゲームオーバー画面のUI制御
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitle;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float delayBeforeButtons = 2f;
        [SerializeField] private bool playGameOverSound = true;
        
        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] private Color titleColor = Color.red;
        [SerializeField] private Color textColor = Color.white;
        
        // Manager references
        private GameManager gameManager;
        private AudioManager audioManager;
        private CombatManager combatManager;
        private SaveManager saveManager;
        
        // State
        private bool isShowing = false;
        private GameOverStats finalStats;
        
        void Start()
        {
            InitializeGameOverUI();
        }
        
        /// <summary>
        /// ゲームオーバーUIの初期化
        /// </summary>
        private void InitializeGameOverUI()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                audioManager = gameManager.AudioManager;
                combatManager = gameManager.GetComponent<CombatManager>();
                saveManager = gameManager.GetComponent<SaveManager>();
            }
            
            // Create UI elements if they don't exist
            CreateUIElements();
            
            // Setup button events
            SetupButtonEvents();
            
            // Subscribe to game events
            if (gameManager != null)
            {
                gameManager.OnStateChanged += OnGameStateChanged;
            }
            
            // Hide initially
            HideGameOverUI();
            
            Debug.Log("💀 Game Over UI initialized");
        }
        
        /// <summary>
        /// UI要素の作成
        /// </summary>
        private void CreateUIElements()
        {
            // Create canvas if not exists
            if (gameOverCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameOverCanvas");
                gameOverCanvas = canvasObj.AddComponent<Canvas>();
                gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameOverCanvas.sortingOrder = 200; // Above other UI
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create main panel
            if (gameOverPanel == null)
            {
                gameOverPanel = CreatePanel("GameOverPanel", gameOverCanvas.transform);
                
                // Set full screen
                RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                // Set background color
                Image panelImage = gameOverPanel.GetComponent<Image>();
                panelImage.color = backgroundColor;
            }
            
            // Create title text
            if (gameOverTitle == null)
            {
                gameOverTitle = CreateText("GameOverTitle", "GAME OVER", gameOverPanel.transform);
                gameOverTitle.fontSize = 72f;
                gameOverTitle.color = titleColor;
                gameOverTitle.alignment = TextAlignmentOptions.Center;
                gameOverTitle.fontStyle = FontStyles.Bold;
                
                RectTransform titleRect = gameOverTitle.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.7f);
                titleRect.anchorMax = new Vector2(1, 0.9f);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;
            }
            
            // Create stats text
            if (statsText == null)
            {
                statsText = CreateText("StatsText", "", gameOverPanel.transform);
                statsText.fontSize = 24f;
                statsText.color = textColor;
                statsText.alignment = TextAlignmentOptions.Center;
                
                RectTransform statsRect = statsText.GetComponent<RectTransform>();
                statsRect.anchorMin = new Vector2(0.2f, 0.4f);
                statsRect.anchorMax = new Vector2(0.8f, 0.7f);
                statsRect.offsetMin = Vector2.zero;
                statsRect.offsetMax = Vector2.zero;
            }
            
            // Create message text
            if (messageText == null)
            {
                messageText = CreateText("MessageText", "", gameOverPanel.transform);
                messageText.fontSize = 18f;
                messageText.color = Color.gray;
                messageText.alignment = TextAlignmentOptions.Center;
                
                RectTransform messageRect = messageText.GetComponent<RectTransform>();
                messageRect.anchorMin = new Vector2(0.1f, 0.25f);
                messageRect.anchorMax = new Vector2(0.9f, 0.4f);
                messageRect.offsetMin = Vector2.zero;
                messageRect.offsetMax = Vector2.zero;
            }
            
            // Create buttons
            CreateButtons();
        }
        
        /// <summary>
        /// パネル作成ヘルパー
        /// </summary>
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            Image image = panel.AddComponent<Image>();
            image.color = Color.clear;
            
            return panel;
        }
        
        /// <summary>
        /// テキスト作成ヘルパー
        /// </summary>
        private TextMeshProUGUI CreateText(string name, string content, Transform parent)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.color = Color.white;
            
            return text;
        }
        
        /// <summary>
        /// ボタンの作成
        /// </summary>
        private void CreateButtons()
        {
            // Button container
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(gameOverPanel.transform, false);
            
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.05f);
            containerRect.anchorMax = new Vector2(0.7f, 0.25f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Vertical layout
            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            
            // Create buttons
            retryButton = CreateButton("RetryButton", "もう一度プレイ", buttonContainer.transform);
            mainMenuButton = CreateButton("MainMenuButton", "メインメニューに戻る", buttonContainer.transform);
            quitButton = CreateButton("QuitButton", "ゲーム終了", buttonContainer.transform);
            
            // Set button sizes
            SetButtonSize(retryButton, new Vector2(300, 50));
            SetButtonSize(mainMenuButton, new Vector2(300, 50));
            SetButtonSize(quitButton, new Vector2(300, 50));
            
            // Initially hide buttons
            retryButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// ボタン作成ヘルパー
        /// </summary>
        private Button CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            Button button = buttonObj.AddComponent<Button>();
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 18f;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        /// <summary>
        /// ボタンサイズ設定
        /// </summary>
        private void SetButtonSize(Button button, Vector2 size)
        {
            if (button == null) return;
            
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            
            // Add LayoutElement for proper layout group behavior
            LayoutElement layoutElement = button.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
        }
        
        /// <summary>
        /// ボタンイベントの設定
        /// </summary>
        private void SetupButtonEvents()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryButtonClicked);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
        }
        
        /// <summary>
        /// ゲーム状態変更イベント
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.GameOver)
            {
                ShowGameOverUI();
            }
            else
            {
                HideGameOverUI();
            }
        }
        
        /// <summary>
        /// ゲームオーバーUI表示
        /// </summary>
        public void ShowGameOverUI()
        {
            if (isShowing) return;
            
            isShowing = true;
            
            // Collect final stats
            CollectFinalStats();
            
            // Update UI content
            UpdateUIContent();
            
            // Show UI
            gameOverCanvas.gameObject.SetActive(true);
            
            // Play game over sound
            if (playGameOverSound && audioManager != null)
            {
                audioManager.StopBGM();
                audioManager.PlaySE("gameover");
            }
            
            // Start fade in animation
            StartCoroutine(FadeInAnimation());
            
            Debug.Log("💀 Game Over UI shown");
        }
        
        /// <summary>
        /// 最終統計の収集
        /// </summary>
        private void CollectFinalStats()
        {
            finalStats = new GameOverStats();
            
            if (combatManager != null)
            {
                var playerStats = combatManager.PlayerStats;
                finalStats.level = playerStats.level;
                finalStats.experience = playerStats.experience;
                finalStats.gold = playerStats.gold;
                finalStats.maxHealth = playerStats.maxHealth;
                finalStats.attack = playerStats.attack;
                finalStats.defense = playerStats.defense;
            }
            
            // Additional stats could be collected from other managers
            // finalStats.enemiesKilled = enemyManager.GetKillCount();
            // finalStats.itemsCollected = itemManager.GetItemsCollectedCount();
            // finalStats.floorsExplored = dungeonManager.GetMaxFloorReached();
            
            finalStats.playTime = Time.time; // Simplified - would use proper time tracking
        }
        
        /// <summary>
        /// UI内容の更新
        /// </summary>
        private void UpdateUIContent()
        {
            // Update stats text
            if (statsText != null && finalStats != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"最終レベル: {finalStats.level}");
                sb.AppendLine($"獲得経験値: {finalStats.experience}");
                sb.AppendLine($"所持ゴールド: {finalStats.gold}");
                sb.AppendLine($"最大HP: {finalStats.maxHealth}");
                sb.AppendLine($"攻撃力: {finalStats.attack}");
                sb.AppendLine($"防御力: {finalStats.defense}");
                sb.AppendLine();
                sb.AppendLine($"プレイ時間: {FormatPlayTime(finalStats.playTime)}");
                
                statsText.text = sb.ToString();
            }
            
            // Update message text
            if (messageText != null)
            {
                messageText.text = "ダンジョンの奥深くで力尽きた...\n" +
                                  "しかし、あなたの冒険は永遠に語り継がれるだろう。";
            }
        }
        
        /// <summary>
        /// プレイ時間のフォーマット
        /// </summary>
        private string FormatPlayTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            
            if (hours > 0)
            {
                return $"{hours:00}:{minutes:00}:{secs:00}";
            }
            else
            {
                return $"{minutes:00}:{secs:00}";
            }
        }
        
        /// <summary>
        /// フェードインアニメーション
        /// </summary>
        private IEnumerator FadeInAnimation()
        {
            // Initially hide everything
            CanvasGroup canvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            
            // Fade in
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            
            // Wait before showing buttons
            yield return new WaitForSecondsRealtime(delayBeforeButtons);
            
            // Show buttons with animation
            StartCoroutine(ShowButtonsAnimation());
        }
        
        /// <summary>
        /// ボタン表示アニメーション
        /// </summary>
        private IEnumerator ShowButtonsAnimation()
        {
            Button[] buttons = { retryButton, mainMenuButton, quitButton };
            
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(true);
                    
                    // Scale animation
                    Transform buttonTransform = button.transform;
                    buttonTransform.localScale = Vector3.zero;
                    
                    float animTime = 0.3f;
                    float elapsedTime = 0f;
                    
                    while (elapsedTime < animTime)
                    {
                        elapsedTime += Time.unscaledDeltaTime;
                        float scale = Mathf.Lerp(0f, 1f, elapsedTime / animTime);
                        buttonTransform.localScale = Vector3.one * scale;
                        yield return null;
                    }
                    
                    buttonTransform.localScale = Vector3.one;
                    
                    // Small delay between buttons
                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }
        }
        
        /// <summary>
        /// ゲームオーバーUI非表示
        /// </summary>
        public void HideGameOverUI()
        {
            if (!isShowing) return;
            
            isShowing = false;
            gameOverCanvas.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// リトライボタンクリック
        /// </summary>
        private void OnRetryButtonClicked()
        {
            if (audioManager != null)
            {
                audioManager.PlaySE("button");
            }
            
            Debug.Log("🔄 Retry game requested");
            
            // Reset game state and start new game
            if (gameManager != null)
            {
                gameManager.StartNewGame();
            }
        }
        
        /// <summary>
        /// メインメニューボタンクリック
        /// </summary>
        private void OnMainMenuButtonClicked()
        {
            if (audioManager != null)
            {
                audioManager.PlaySE("button");
            }
            
            Debug.Log("🏠 Return to main menu requested");
            
            if (gameManager != null)
            {
                gameManager.ReturnToMainMenu();
            }
        }
        
        /// <summary>
        /// 終了ボタンクリック
        /// </summary>
        private void OnQuitButtonClicked()
        {
            if (audioManager != null)
            {
                audioManager.PlaySE("button");
            }
            
            Debug.Log("👋 Quit game requested");
            
            if (gameManager != null)
            {
                gameManager.QuitGame();
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
    
    /// <summary>
    /// ゲームオーバー時の統計情報
    /// </summary>
    [System.Serializable]
    public class GameOverStats
    {
        public int level;
        public int experience;
        public int gold;
        public int maxHealth;
        public int attack;
        public int defense;
        public int enemiesKilled;
        public int itemsCollected;
        public int floorsExplored;
        public float playTime;
    }
}