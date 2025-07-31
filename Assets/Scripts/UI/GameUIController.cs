using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ブラックオニキス復刻版ゲーム画面のUI制御とレイアウト管理
    /// 1984年オリジナル準拠の日本語UI・メッセージシステム
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas gameCanvas;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button mapToggleButton;
        
        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI inventoryText;
        
        [Header("Message System")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private ScrollRect messageScrollRect;
        [SerializeField] private int maxMessages = 50;
        
        [Header("Layout Settings")]
        [SerializeField] private bool autoLayout = true;
        [SerializeField] private float mapPanelWidth = 400f;
        [SerializeField] private float statusPanelHeight = 150f;
        
        // Components
        private DungeonMapRenderer mapRenderer;
        private PlayerController playerController;
        
        // Manager references
        private GameManager gameManager;
        private DungeonManager dungeonManager;
        private AudioManager audioManager;
        
        // UI state
        private bool isMapVisible = true;
        private System.Collections.Generic.List<string> messageHistory = new System.Collections.Generic.List<string>();
        
        void Start()
        {
            InitializeGameUI();
        }
        
        void Update()
        {
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ゲームUIの初期化
        /// </summary>
        private void InitializeGameUI()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                audioManager = gameManager.AudioManager;
            }
            
            // Create UI elements if they don't exist
            CreateUIElements();
            
            // Setup layout
            if (autoLayout)
            {
                SetupAutoLayout();
            }
            
            // Initialize components
            InitializeMapRenderer();
            InitializePlayerController();
            
            // Setup button events
            SetupButtonEvents();
            
            // Subscribe to game events
            SubscribeToEvents();
            
            // Initial message - オリジナルブラックオニキス風
            AddMessage("📜 ブラックオニキス復刻版へようこそ");
            AddMessage("🏰 暗黒の塔への挑戦が始まります...");
            AddMessage("⚔️ 移動: 矢印キー または WASD");
            AddMessage("🗺️ マップ表示切替: M キー");
            AddMessage("💀 危険な旅路、気をつけて進んでください");
            
            Debug.Log("🎮 Game UI Controller initialized");
        }
        
        /// <summary>
        /// UI要素の作成
        /// </summary>
        private void CreateUIElements()
        {
            // Create main canvas if not exists
            if (gameCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                gameCanvas = canvasObj.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameCanvas.sortingOrder = 100;
                
                // Add Canvas Scaler
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create map panel
            if (mapPanel == null)
            {
                mapPanel = CreatePanel("MapPanel", gameCanvas.transform);
            }
            
            // Create status panel
            if (statusPanel == null)
            {
                statusPanel = CreatePanel("StatusPanel", gameCanvas.transform);
            }
            
            // Create message panel
            if (messagePanel == null)
            {
                messagePanel = CreatePanel("MessagePanel", gameCanvas.transform);
            }
            
            // Create buttons
            CreateButtons();
            
            // Create text elements
            CreateTextElements();
        }
        
        /// <summary>
        /// パネルの作成
        /// </summary>
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            // Add Image component for background
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
            
            // Add RectTransform
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            
            return panel;
        }
        
        /// <summary>
        /// ボタンの作成
        /// </summary>
        private void CreateButtons()
        {
            // Menu button
            if (menuButton == null)
            {
                menuButton = CreateButton("MenuButton", "メニュー", statusPanel.transform);
            }
            
            // Map toggle button
            if (mapToggleButton == null)
            {
                mapToggleButton = CreateButton("MapToggleButton", "地図", statusPanel.transform);
            }
        }
        
        /// <summary>
        /// ボタン作成ヘルパー
        /// </summary>
        private Button CreateButton(string name, string text, Transform parent)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            // Add Image component
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 16f;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // Setup RectTransform for text
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        /// <summary>
        /// テキスト要素の作成
        /// </summary>
        private void CreateTextElements()
        {
            // Status texts - 日本語表示
            floorText = CreateText("FloorText", "現在地: 地下1階", statusPanel.transform);
            positionText = CreateText("PositionText", "座標: (1, 1)", statusPanel.transform);
            healthText = CreateText("HealthText", "体力: 50/50", statusPanel.transform);
            inventoryText = CreateText("InventoryText", "所持品: 0/20", statusPanel.transform);
            
            // Message text
            messageText = CreateText("MessageText", "", messagePanel.transform);
            messageText.fontSize = 14f;
            messageText.alignment = TextAlignmentOptions.TopLeft;
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
            text.fontSize = 18f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MiddleLeft;
            
            return text;
        }
        
        /// <summary>
        /// 自動レイアウトの設定
        /// </summary>
        private void SetupAutoLayout()
        {
            // Map panel (left side)
            RectTransform mapRect = mapPanel.GetComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0, 0);
            mapRect.anchorMax = new Vector2(0, 1);
            mapRect.anchoredPosition = new Vector2(mapPanelWidth / 2, 0);
            mapRect.sizeDelta = new Vector2(mapPanelWidth, 0);
            
            // Status panel (top right)
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 1);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.anchoredPosition = new Vector2(mapPanelWidth / 2, -statusPanelHeight / 2);
            statusRect.sizeDelta = new Vector2(-mapPanelWidth, statusPanelHeight);
            
            // Message panel (bottom right)
            RectTransform messageRect = messagePanel.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0, 0);
            messageRect.anchorMax = new Vector2(1, 1);
            messageRect.anchoredPosition = new Vector2(mapPanelWidth / 2, statusPanelHeight / 2);
            messageRect.sizeDelta = new Vector2(-mapPanelWidth, -statusPanelHeight);
            
            // Layout status panel elements
            LayoutStatusElements();
        }
        
        /// <summary>
        /// ステータス要素のレイアウト
        /// </summary>
        private void LayoutStatusElements()
        {
            float elementHeight = 25f;
            float startY = -20f;
            
            // Position status texts
            SetTextPosition(floorText, new Vector2(10, startY), new Vector2(200, elementHeight));
            SetTextPosition(positionText, new Vector2(220, startY), new Vector2(200, elementHeight));
            SetTextPosition(healthText, new Vector2(10, startY - elementHeight), new Vector2(200, elementHeight));
            SetTextPosition(inventoryText, new Vector2(220, startY - elementHeight), new Vector2(200, elementHeight));
            
            // Position buttons
            SetButtonPosition(menuButton, new Vector2(-100, -70), new Vector2(80, 30));
            SetButtonPosition(mapToggleButton, new Vector2(-10, -70), new Vector2(80, 30));
        }
        
        /// <summary>
        /// テキスト位置設定ヘルパー
        /// </summary>
        private void SetTextPosition(TextMeshProUGUI text, Vector2 position, Vector2 size)
        {
            if (text == null) return;
            
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        
        /// <summary>
        /// ボタン位置設定ヘルパー
        /// </summary>
        private void SetButtonPosition(Button button, Vector2 position, Vector2 size)
        {
            if (button == null) return;
            
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
        
        /// <summary>
        /// マップレンダラーの初期化
        /// </summary>
        private void InitializeMapRenderer()
        {
            mapRenderer = mapPanel.GetComponent<DungeonMapRenderer>();
            if (mapRenderer == null)
            {
                mapRenderer = mapPanel.AddComponent<DungeonMapRenderer>();
            }
        }
        
        /// <summary>
        /// プレイヤーコントローラーの初期化
        /// </summary>
        private void InitializePlayerController()
        {
            // Find or create PlayerController
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                GameObject playerObj = new GameObject("PlayerController");
                playerController = playerObj.AddComponent<PlayerController>();
            }
        }
        
        /// <summary>
        /// ボタンイベントの設定
        /// </summary>
        private void SetupButtonEvents()
        {
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuButtonClicked);
            }
            
            if (mapToggleButton != null)
            {
                mapToggleButton.onClick.AddListener(OnMapToggleButtonClicked);
            }
        }
        
        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (dungeonManager != null)
            {
                dungeonManager.OnPlayerMoved += OnPlayerMoved;
                dungeonManager.OnFloorChanged += OnFloorChanged;
                dungeonManager.OnCellEntered += OnCellEntered;
            }
        }
        
        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (dungeonManager == null) return;
            
            // Update floor - オリジナルブラックオニキス風表示
            if (floorText != null)
            {
                int floor = dungeonManager.GetCurrentFloorNumber();
                string floorName = GetFloorName(floor);
                floorText.text = $"現在地: {floorName}";
            }
            
            // Update position - 日本語表示
            if (positionText != null)
            {
                Vector2Int pos = dungeonManager.GetPlayerPosition();
                positionText.text = $"座標: ({pos.x}, {pos.y})";
            }
            
            // Update health - 本格的な日本語表示
            if (healthText != null)
            {
                var combatManager = gameManager?.GetComponent<CombatManager>();
                if (combatManager != null)
                {
                    var stats = combatManager.PlayerStats;
                    healthText.text = $"体力: {stats.currentHealth}/{stats.maxHealth}";
                }
                else
                {
                    healthText.text = "体力: 100/100";
                }
            }
            
            // Update inventory - 日本語表示
            if (inventoryText != null)
            {
                var itemManager = gameManager?.GetComponent<ItemManager>();
                if (itemManager != null)
                {
                    inventoryText.text = $"所持品: {itemManager.InventoryCount}/{itemManager.MaxInventorySlots}";
                }
                else
                {
                    inventoryText.text = "所持品: 0/20";
                }
            }
        }
        
        /// <summary>
        /// メッセージの追加
        /// </summary>
        public void AddMessage(string message)
        {
            messageHistory.Add($"[{System.DateTime.Now.ToString("HH:mm:ss")}] {message}");
            
            // Limit message history
            if (messageHistory.Count > maxMessages)
            {
                messageHistory.RemoveAt(0);
            }
            
            // Update message display
            if (messageText != null)
            {
                messageText.text = string.Join("\n", messageHistory);
            }
            
            // Scroll to bottom
            if (messageScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                messageScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        /// <summary>
        /// メニューボタンクリック
        /// </summary>
        private void OnMenuButtonClicked()
        {
            if (audioManager != null)
                audioManager.PlaySE("button");
            
            if (gameManager != null)
                gameManager.ReturnToMainMenu();
        }
        
        /// <summary>
        /// マップ切替ボタンクリック
        /// </summary>
        private void OnMapToggleButtonClicked()
        {
            if (audioManager != null)
                audioManager.PlaySE("button");
            
            ToggleMapVisibility();
        }
        
        /// <summary>
        /// マップ表示切替
        /// </summary>
        public void ToggleMapVisibility()
        {
            isMapVisible = !isMapVisible;
            mapPanel.SetActive(isMapVisible);
            
            AddMessage($"🗺️ マップ表示: {(isMapVisible ? "表示" : "非表示")}");
            
            // Adjust layout when map is hidden
            if (!isMapVisible)
            {
                AdjustLayoutForHiddenMap();
            }
            else
            {
                RestoreNormalLayout();
            }
        }
        
        /// <summary>
        /// マップ非表示時のレイアウト調整
        /// </summary>
        private void AdjustLayoutForHiddenMap()
        {
            // Expand other panels to full width
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, -statusPanelHeight / 2);
            statusRect.sizeDelta = new Vector2(0, statusPanelHeight);
            
            RectTransform messageRect = messagePanel.GetComponent<RectTransform>();
            messageRect.anchoredPosition = new Vector2(0, statusPanelHeight / 2);
            messageRect.sizeDelta = new Vector2(0, -statusPanelHeight);
        }
        
        /// <summary>
        /// 通常レイアウトの復元
        /// </summary>
        private void RestoreNormalLayout()
        {
            SetupAutoLayout();
        }
        
        /// <summary>
        /// プレイヤー移動イベント
        /// </summary>
        private void OnPlayerMoved(Vector2Int newPosition)
        {
            // This is handled by UpdateStatusDisplay
        }
        
        /// <summary>
        /// フロア変更イベント
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            string floorName = GetFloorName(newFloor);
            string floorMessage = GetFloorChangeMessage(newFloor);
            AddMessage($"🚪 {floorName}に到着");
            AddMessage(floorMessage);
        }
        
        /// <summary>
        /// セル進入イベント
        /// </summary>
        private void OnCellEntered(DungeonCell cell)
        {
            switch (cell.type)
            {
                case DungeonCellType.Entrance:
                    AddMessage("🚪 暗黒の塔への入口... 引き返すなら今のうちだ");
                    break;
                case DungeonCellType.StairsUp:
                    AddMessage("⬆️ 光へ向かう階段が見える");
                    break;
                case DungeonCellType.StairsDown:
                    AddMessage("⬇️ 暗闇へと続く階段... 覚悟はいいか？");
                    break;
                case DungeonCellType.Door:
                    AddMessage("🚪 古い扉を発見。何かが待ち受けているかもしれない");
                    break;
                case DungeonCellType.Treasure:
                    AddMessage("✨ 宝箱だ！だが、罠の可能性もある...");
                    break;
                case DungeonCellType.Trap:
                    AddMessage("⚠️ 罠にかかった！気をつけろ！");
                    break;
                case DungeonCellType.Secret:
                    AddMessage("🕳️ 隠し部屋を発見！古の秘密が眠っているようだ");
                    break;
                case DungeonCellType.Altar:
                    AddMessage("⛪ 古の祭壇... 何かを捧げることができるかもしれない");
                    break;
                case DungeonCellType.Fountain:
                    AddMessage("⛲ 神秘的な泉... 傷を癒してくれるだろうか");
                    break;
            }
        }
        
        /// <summary>
        /// フロア名を取得（オリジナルブラックオニキス準拠）
        /// </summary>
        private string GetFloorName(int floor)
        {
            switch (floor)
            {
                case 2: return "天界（地上2階）";
                case 1: return "ブラックタワー（地上1階）";
                case -1: return "地下1階";
                case -2: return "地下2階";
                case -3: return "地下3階";
                case -4: return "地下4階";
                case -5: return "地下5階（井戸の底）";
                case -6: return "地下6階（カラー迷路）";
                default: return $"未知の階層（{floor}階）";
            }
        }
        
        /// <summary>
        /// フロア変更時のメッセージ取得（オリジナルブラックオニキス準拠）
        /// </summary>
        private string GetFloorChangeMessage(int floor)
        {
            switch (floor)
            {
                case 2: 
                    return "🌟 聖なる天界に足を踏み入れた。巨人が待ち受けている...";
                case 1: 
                    return "🏰 暗黒の塔の入口。ここから本当の冒険が始まる";
                case -1: 
                    return "⚔️ 地下1階。初心者向けのモンスターが徘徊している";
                case -2: 
                    return "💀 地下2階。敵が少し強くなってきた";
                case -3: 
                    return "🐍 地下3階。危険なコブラが出現する！要注意";
                case -4: 
                    return "😴 地下4階。あまり意味のない階層だが...";
                case -5: 
                    return "🐙 地下5階。井戸の底。オクトパスとハイドの住処";
                case -6: 
                    return "🌈 地下6階。伝説のカラー迷路！ブラックオニキスが眠る...";
                default: 
                    return "❓ 未知の領域に迷い込んでしまった...";
            }
        }
        
        /// <summary>
        /// 戦闘メッセージの追加（オリジナルブラックオニキス風）
        /// </summary>
        public void AddCombatMessage(string enemyName, string actionType, int value = 0)
        {
            switch (actionType)
            {
                case "encounter":
                    AddMessage($"⚔️ {enemyName}が現れた！");
                    break;
                case "player_attack":
                    AddMessage($"🗡️ あなたの攻撃！{enemyName}に{value}のダメージ！");
                    break;
                case "enemy_attack":
                    AddMessage($"💥 {enemyName}の攻撃！あなたは{value}のダメージを受けた！");
                    break;
                case "victory":
                    AddMessage($"🎉 {enemyName}を倒した！");
                    break;
                case "defeat":
                    AddMessage($"💀 {enemyName}に敗れた... 冒険は終わりを告げる");
                    break;
                case "escape":
                    AddMessage($"💨 {enemyName}から逃げ出した");
                    break;
            }
        }
        
        /// <summary>
        /// アイテム関連メッセージの追加（オリジナルブラックオニキス風）
        /// </summary>
        public void AddItemMessage(string itemName, string actionType, int value = 0)
        {
            switch (actionType)
            {
                case "found":
                    AddMessage($"💎 {itemName}を発見した！");
                    break;
                case "picked_up":
                    AddMessage($"✨ {itemName}を手に入れた");
                    break;
                case "used":
                    AddMessage($"🧪 {itemName}を使用した");
                    break;
                case "effect":
                    AddMessage($"⚡ {itemName}の効果：{value}");
                    break;
                case "inventory_full":
                    AddMessage($"⚠️ 荷物がいっぱいで{itemName}を持てない");
                    break;
            }
        }
        
        /// <summary>
        /// レベルアップメッセージの追加（オリジナルブラックオニキス風）
        /// </summary>
        public void AddLevelUpMessage(int newLevel, int hpGain, int attackGain, int defenseGain)
        {
            AddMessage($"🎊 レベルアップ！レベル{newLevel}になった！");
            AddMessage($"📈 体力+{hpGain}, 攻撃力+{attackGain}, 防御力+{defenseGain}");
            
            if (newLevel <= 3)
            {
                AddMessage($"⚠️ まだまだ弱い...強敵には近づくな");
            }
            else if (newLevel >= 9)
            {
                AddMessage($"💪 かなり強くなった！でも天界の巨人は危険...");
            }
            else if (newLevel == 5)
            {
                AddMessage($"⚔️ 中級者の仲間入り！でも油断は禁物");
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (dungeonManager != null)
            {
                dungeonManager.OnPlayerMoved -= OnPlayerMoved;
                dungeonManager.OnFloorChanged -= OnFloorChanged;
                dungeonManager.OnCellEntered -= OnCellEntered;
            }
        }
    }
}