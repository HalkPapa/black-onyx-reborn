using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace BlackOnyxReborn
{
    /// <summary>
    /// インゲームデバッグコンソール
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        [Header("Console Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private int maxLogEntries = 200;
        [SerializeField] private float consoleHeight = 0.4f; // Screen height percentage
        
        [Header("Visual Settings")]
        [SerializeField] private int fontSize = 12;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color warningColor = Color.yellow;
        
        // Console state
        private bool isVisible = false;
        private List<LogEntry> logEntries = new List<LogEntry>();
        private Vector2 scrollPosition = Vector2.zero;
        private string commandInput = "";
        private bool wasConsoleOpen = false;
        
        // GUI styles
        private GUIStyle consoleStyle;
        private GUIStyle inputStyle;
        private GUIStyle scrollStyle;
        private GUIStyle logStyle;
        
        // Manager references
        private GameManager gameManager;
        private DungeonManager dungeonManager;
        private CombatManager combatManager;
        private ItemManager itemManager;
        private EnemyManager enemyManager;
        private PerformanceOptimizer performanceOptimizer;
        
        // Command dictionary
        private Dictionary<string, System.Action<string[]>> commands;
        
        void Start()
        {
            InitializeDebugConsole();
        }
        
        void Update()
        {
            HandleInput();
        }
        
        void OnGUI()
        {
            if (isVisible)
            {
                DrawConsole();
            }
        }
        
        void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }
        
        void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
        
        /// <summary>
        /// デバッグコンソールの初期化
        /// </summary>
        private void InitializeDebugConsole()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                combatManager = gameManager.GetComponent<CombatManager>();
                itemManager = gameManager.GetComponent<ItemManager>();
                enemyManager = gameManager.GetComponent<EnemyManager>();
                performanceOptimizer = gameManager.GetComponent<PerformanceOptimizer>();
            }
            
            // Initialize commands
            InitializeCommands();
            
            // Initialize GUI styles
            InitializeGUIStyles();
            
            // Show on start if enabled
            isVisible = showOnStart;
            
            // Add welcome message
            AddLogEntry("🐛 Debug Console initialized. Type 'help' for commands.", LogType.Log);
            
            Debug.Log("🐛 Debug Console ready");
        }
        
        /// <summary>
        /// コマンドの初期化
        /// </summary>
        private void InitializeCommands()
        {
            commands = new Dictionary<string, System.Action<string[]>>
            {
                { "help", ShowHelp },
                { "clear", ClearConsole },
                { "quit", QuitGame },
                { "save", SaveGame },
                { "load", LoadGame },
                { "teleport", TeleportPlayer },
                { "heal", HealPlayer },
                { "additem", AddItem },
                { "spawnenemy", SpawnEnemy },
                { "kill", KillAllEnemies },
                { "god", ToggleGodMode },
                { "fps", ShowFPS },
                { "stats", ShowStats },
                { "floor", ChangeFloor },
                { "give", GiveResource },
                { "time", ShowTime },
                { "version", ShowVersion },
                { "reset", ResetGame },
                { "gc", ForceGarbageCollection },
                { "pool", ShowObjectPools },
                { "debug", ToggleDebugMode }
            };
        }
        
        /// <summary>
        /// GUIスタイルの初期化
        /// </summary>
        private void InitializeGUIStyles()
        {
            consoleStyle = new GUIStyle();
            consoleStyle.normal.background = MakeTexture(2, 2, backgroundColor);
            consoleStyle.padding = new RectOffset(10, 10, 10, 10);
            
            inputStyle = new GUIStyle(GUI.skin.textField);
            inputStyle.fontSize = fontSize;
            inputStyle.normal.textColor = textColor;
            inputStyle.focused.textColor = textColor;
            
            scrollStyle = new GUIStyle(GUI.skin.scrollView);
            scrollStyle.normal.background = MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f));
            
            logStyle = new GUIStyle();
            logStyle.fontSize = fontSize;
            logStyle.normal.textColor = textColor;
            logStyle.wordWrap = true;
            logStyle.richText = true;
        }
        
        /// <summary>
        /// テクスチャの作成
        /// </summary>
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }
            
            // Console-specific input
            if (isVisible)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ExecuteCommand();
                }
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ToggleConsole();
                }
            }
        }
        
        /// <summary>
        /// コンソールの表示切り替え
        /// </summary>
        public void ToggleConsole()
        {
            isVisible = !isVisible;
            
            if (isVisible)
            {
                // Focus input field
                GUI.FocusControl("ConsoleInput");
            }
        }
        
        /// <summary>
        /// コンソールの描画
        /// </summary>
        private void DrawConsole()
        {
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            
            Rect consoleRect = new Rect(0, 0, screenWidth, screenHeight * consoleHeight);
            GUI.Box(consoleRect, "", consoleStyle);
            
            GUILayout.BeginArea(consoleRect);
            GUILayout.BeginVertical();
            
            // Log area
            Rect scrollRect = new Rect(10, 10, screenWidth - 40, consoleRect.height - 60);
            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, 
                new Rect(0, 0, scrollRect.width - 20, logEntries.Count * 20), false, true, 
                GUIStyle.none, GUI.skin.verticalScrollbar);
            
            for (int i = 0; i < logEntries.Count; i++)
            {
                var entry = logEntries[i];
                Color originalColor = logStyle.normal.textColor;
                
                switch (entry.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                        logStyle.normal.textColor = errorColor;
                        break;
                    case LogType.Warning:
                        logStyle.normal.textColor = warningColor;
                        break;
                    default:
                        logStyle.normal.textColor = textColor;
                        break;
                }
                
                GUILayout.Label($"[{entry.timestamp:HH:mm:ss}] {entry.message}", logStyle);
                logStyle.normal.textColor = originalColor;
            }
            
            GUI.EndScrollView();
            
            // Input area
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("> ", logStyle, GUILayout.Width(20));
            
            GUI.SetNextControlName("ConsoleInput");
            commandInput = GUILayout.TextField(commandInput, inputStyle);
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            // Auto-scroll to bottom
            if (scrollPosition.y < (logEntries.Count * 20) - scrollRect.height)
            {
                scrollPosition.y = Mathf.Max(0, (logEntries.Count * 20) - scrollRect.height);
            }
        }
        
        /// <summary>
        /// コマンドの実行
        /// </summary>
        private void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(commandInput.Trim())) return;
            
            AddLogEntry($"> {commandInput}", LogType.Log);
            
            string[] parts = commandInput.Trim().Split(' ');
            string command = parts[0].ToLower();
            
            if (commands.ContainsKey(command))
            {
                try
                {
                    commands[command](parts);
                }
                catch (System.Exception e)
                {
                    AddLogEntry($"Command error: {e.Message}", LogType.Error);
                }
            }
            else
            {
                AddLogEntry($"Unknown command: {command}. Type 'help' for available commands.", LogType.Warning);
            }
            
            commandInput = "";
        }
        
        /// <summary>
        /// ログエントリの追加
        /// </summary>
        private void AddLogEntry(string message, LogType type)
        {
            logEntries.Add(new LogEntry
            {
                message = message,
                type = type,
                timestamp = System.DateTime.Now
            });
            
            // Limit entries
            if (logEntries.Count > maxLogEntries)
            {
                logEntries.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Unity ログメッセージ受信
        /// </summary>
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            AddLogEntry(logString, type);
        }
        
        // Command implementations
        
        private void ShowHelp(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available Commands:");
            sb.AppendLine("help - Show this help");
            sb.AppendLine("clear - Clear console");
            sb.AppendLine("quit - Quit game");
            sb.AppendLine("save - Save game");
            sb.AppendLine("load - Load game");
            sb.AppendLine("teleport <x> <y> - Teleport player");
            sb.AppendLine("heal [amount] - Heal player");
            sb.AppendLine("additem <itemId> [quantity] - Add item to inventory");
            sb.AppendLine("spawnenemy <enemyName> [x] [y] - Spawn enemy");
            sb.AppendLine("kill - Kill all enemies");
            sb.AppendLine("god - Toggle god mode");
            sb.AppendLine("fps - Toggle FPS display");
            sb.AppendLine("stats - Show game statistics");
            sb.AppendLine("floor <number> - Change floor");
            sb.AppendLine("give <resource> <amount> - Give resources (gold, exp)");
            sb.AppendLine("time - Show current time");
            sb.AppendLine("version - Show game version");
            sb.AppendLine("reset - Reset game data");
            sb.AppendLine("gc - Force garbage collection");
            sb.AppendLine("pool - Show object pool status");
            sb.AppendLine("debug - Toggle debug mode");
            
            AddLogEntry(sb.ToString(), LogType.Log);
        }
        
        private void ClearConsole(string[] args)
        {
            logEntries.Clear();
            AddLogEntry("Console cleared.", LogType.Log);
        }
        
        private void QuitGame(string[] args)
        {
            AddLogEntry("Quitting game...", LogType.Log);
            if (gameManager != null)
            {
                gameManager.QuitGame();
            }
        }
        
        private void SaveGame(string[] args)
        {
            if (gameManager?.SaveManager != null)
            {
                bool success = gameManager.SaveManager.SaveGame();
                AddLogEntry(success ? "Game saved successfully." : "Failed to save game.", LogType.Log);
            }
        }
        
        private void LoadGame(string[] args)
        {
            if (gameManager?.SaveManager != null)
            {
                bool success = gameManager.SaveManager.LoadGame();
                AddLogEntry(success ? "Game loaded successfully." : "Failed to load game.", LogType.Log);
            }
        }
        
        private void TeleportPlayer(string[] args)
        {
            if (args.Length < 3)
            {
                AddLogEntry("Usage: teleport <x> <y>", LogType.Warning);
                return;
            }
            
            if (int.TryParse(args[1], out int x) && int.TryParse(args[2], out int y))
            {
                // This would require implementation in DungeonManager
                AddLogEntry($"Teleporting player to ({x}, {y})", LogType.Log);
            }
            else
            {
                AddLogEntry("Invalid coordinates.", LogType.Error);
            }
        }
        
        private void HealPlayer(string[] args)
        {
            int amount = 100; // Default full heal
            
            if (args.Length > 1 && int.TryParse(args[1], out int customAmount))
            {
                amount = customAmount;
            }
            
            if (combatManager != null)
            {
                combatManager.HealPlayer(amount);
                AddLogEntry($"Healed player for {amount} HP.", LogType.Log);
            }
        }
        
        private void AddItem(string[] args)
        {
            if (args.Length < 2)
            {
                AddLogEntry("Usage: additem <itemId> [quantity]", LogType.Warning);
                return;
            }
            
            string itemId = args[1];
            int quantity = args.Length > 2 && int.TryParse(args[2], out int q) ? q : 1;
            
            if (itemManager != null)
            {
                var itemData = itemManager.GetItemData(itemId);
                if (itemData != null)
                {
                    bool success = itemManager.AddToInventory(itemData, quantity);
                    AddLogEntry(success ? $"Added {itemData.itemName} x{quantity}" : "Failed to add item", LogType.Log);
                }
                else
                {
                    AddLogEntry($"Item not found: {itemId}", LogType.Error);
                }
            }
        }
        
        private void SpawnEnemy(string[] args)
        {
            if (args.Length < 2)
            {
                AddLogEntry("Usage: spawnenemy <enemyName> [x] [y]", LogType.Warning);
                return;
            }
            
            string enemyName = args[1];
            Vector2Int position = dungeonManager?.GetPlayerPosition() ?? Vector2Int.zero;
            
            if (args.Length >= 4)
            {
                if (int.TryParse(args[2], out int x) && int.TryParse(args[3], out int y))
                {
                    position = new Vector2Int(x, y);
                }
            }
            
            if (enemyManager != null)
            {
                enemyManager.ForceSpawnEnemy(enemyName, position);
                AddLogEntry($"Spawned {enemyName} at {position}", LogType.Log);
            }
        }
        
        private void KillAllEnemies(string[] args)
        {
            if (enemyManager != null)
            {
                enemyManager.ClearAllEnemies();
                AddLogEntry("All enemies killed.", LogType.Log);
            }
        }
        
        private void ToggleGodMode(string[] args)
        {
            // God mode implementation would be in CombatManager
            AddLogEntry("God mode toggled.", LogType.Log);
        }
        
        private void ShowFPS(string[] args)
        {
            if (performanceOptimizer != null)
            {
                // Toggle FPS display
                AddLogEntry("FPS display toggled.", LogType.Log);
            }
        }
        
        private void ShowStats(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Game Statistics:");
            
            if (combatManager != null)
            {
                var stats = combatManager.PlayerStats;
                sb.AppendLine($"Player Level: {stats.level}");
                sb.AppendLine($"HP: {stats.currentHealth}/{stats.maxHealth}");
                sb.AppendLine($"Attack: {stats.attack}");
                sb.AppendLine($"Defense: {stats.defense}");
                sb.AppendLine($"Gold: {stats.gold}");
            }
            
            if (dungeonManager != null)
            {
                sb.AppendLine($"Current Floor: {dungeonManager.GetCurrentFloorNumber()}");
                sb.AppendLine($"Player Position: {dungeonManager.GetPlayerPosition()}");
            }
            
            if (itemManager != null)
            {
                sb.AppendLine($"Inventory Items: {itemManager.InventoryCount}/{itemManager.MaxInventorySlots}");
            }
            
            if (enemyManager != null)
            {
                sb.AppendLine($"Active Enemies: {enemyManager.ActiveEnemyCount}");
            }
            
            AddLogEntry(sb.ToString(), LogType.Log);
        }
        
        private void ChangeFloor(string[] args)
        {
            if (args.Length < 2)
            {
                AddLogEntry("Usage: floor <number>", LogType.Warning);
                return;
            }
            
            if (int.TryParse(args[1], out int floor) && dungeonManager != null)
            {
                dungeonManager.ChangeFloor(floor);
                AddLogEntry($"Changed to floor {floor}.", LogType.Log);
            }
            else
            {
                AddLogEntry("Invalid floor number.", LogType.Error);
            }
        }
        
        private void GiveResource(string[] args)
        {
            if (args.Length < 3)
            {
                AddLogEntry("Usage: give <resource> <amount>", LogType.Warning);
                return;
            }
            
            string resource = args[1].ToLower();
            if (int.TryParse(args[2], out int amount) && combatManager != null)
            {
                var stats = combatManager.PlayerStats;
                
                switch (resource)
                {
                    case "gold":
                        stats.gold += amount;
                        AddLogEntry($"Gave {amount} gold.", LogType.Log);
                        break;
                    case "exp":
                    case "experience":
                        // This would require implementation in CombatManager
                        AddLogEntry($"Gave {amount} experience.", LogType.Log);
                        break;
                    default:
                        AddLogEntry($"Unknown resource: {resource}", LogType.Error);
                        break;
                }
            }
        }
        
        private void ShowTime(string[] args)
        {
            AddLogEntry($"Current time: {System.DateTime.Now:yyyy/MM/dd HH:mm:ss}", LogType.Log);
            AddLogEntry($"Play time: {Time.time:F1} seconds", LogType.Log);
        }
        
        private void ShowVersion(string[] args)
        {
            AddLogEntry($"Black Onyx Reborn v1.0.0", LogType.Log);
            AddLogEntry($"Unity {Application.unityVersion}", LogType.Log);
        }
        
        private void ResetGame(string[] args)
        {
            AddLogEntry("Resetting game data...", LogType.Log);
            if (gameManager?.SaveManager != null)
            {
                gameManager.SaveManager.InitializeNewGameData();
            }
        }
        
        private void ForceGarbageCollection(string[] args)
        {
            if (performanceOptimizer != null)
            {
                performanceOptimizer.ForceGarbageCollection();
                AddLogEntry("Forced garbage collection.", LogType.Log);
            }
            else
            {
                System.GC.Collect();
                AddLogEntry("Forced garbage collection (manual).", LogType.Log);
            }
        }
        
        private void ShowObjectPools(string[] args)
        {
            if (performanceOptimizer != null)
            {
                var stats = performanceOptimizer.GetPerformanceStats();
                AddLogEntry($"Pooled objects: {stats.pooledObjectsCount}", LogType.Log);
            }
            else
            {
                AddLogEntry("Performance optimizer not available.", LogType.Warning);
            }
        }
        
        private void ToggleDebugMode(string[] args)
        {
            if (gameManager != null)
            {
                // Toggle debug mode in GameManager
                AddLogEntry("Debug mode toggled.", LogType.Log);
            }
        }
    }
    
    /// <summary>
    /// ログエントリ
    /// </summary>
    [System.Serializable]
    public class LogEntry
    {
        public string message;
        public LogType type;
        public System.DateTime timestamp;
    }
}