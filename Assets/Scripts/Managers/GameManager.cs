using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ゲーム全体の状態管理を行うメインマネージャー
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private float targetFrameRate = 60f;
        
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        // Game state
        public enum GameState
        {
            Loading,
            MainMenu,
            InGame,
            Paused,
            GameOver,
            Settings
        }
        
        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.Loading;
        public GameState CurrentState => currentState;
        
        // Events
        public System.Action<GameState> OnStateChanged;
        
        // Scene loading state
        private bool isLoadingScene = false;
        
        // References to other managers
        public AudioManager AudioManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public DungeonManager DungeonManager { get; private set; }
        public BlackOnyxDungeonManager BlackOnyxDungeonManager { get; private set; }
        public SaveManager SaveManager { get; private set; }
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Initialize all managers
            InitializeManagers();
            
            // Set initial state
            ChangeState(GameState.MainMenu);
        }
        
        void Update()
        {
            // Handle input based on current state
            HandleInput();
            
            // Update managers
            UpdateManagers();
        }
        
        /// <summary>
        /// ゲーム基本設定の初期化
        /// </summary>
        private void InitializeGame()
        {
            // Set target frame rate
            Application.targetFrameRate = (int)targetFrameRate;
            
            // Set quality settings for 2D pixel art game
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            
            // Subscribe to scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            if (debugMode)
            {
                Debug.Log("🎮 Black Onyx Reborn - Game Manager Initialized");
            }
        }
        
        /// <summary>
        /// 各種マネージャーの初期化
        /// </summary>
        private void InitializeManagers()
        {
            // Find or create manager components
            AudioManager = GetComponent<AudioManager>() ?? gameObject.AddComponent<AudioManager>();
            UIManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();
            DungeonManager = GetComponent<DungeonManager>() ?? gameObject.AddComponent<DungeonManager>();
            BlackOnyxDungeonManager = GetComponent<BlackOnyxDungeonManager>() ?? gameObject.AddComponent<BlackOnyxDungeonManager>();
            SaveManager = GetComponent<SaveManager>() ?? gameObject.AddComponent<SaveManager>();
            
            if (debugMode)
            {
                Debug.Log("🏰 All managers initialized successfully - Using authentic Black Onyx dungeon system");
            }
        }
        
        /// <summary>
        /// ゲーム状態の変更
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;
            
            var previousState = currentState;
            currentState = newState;
            
            if (debugMode)
            {
                Debug.Log($"🔄 State changed: {previousState} → {newState}");
            }
            
            // Handle state change
            HandleStateChange(previousState, newState);
            
            // Handle audio changes based on state
            HandleAudioStateChange(newState);
            
            // Notify listeners
            OnStateChanged?.Invoke(newState);
        }
        
        /// <summary>
        /// 状態変更時の処理
        /// </summary>
        private void HandleStateChange(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Loading:
                    // Show loading screen
                    break;
                    
                case GameState.MainMenu:
                    // Show main menu UI
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.InGame:
                    // Start or resume game
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Paused:
                    // Pause game
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.GameOver:
                    // Handle game over
                    Time.timeScale = 0f;
                    break;
                    
                case GameState.Settings:
                    // Show settings menu
                    break;
            }
        }
        
        /// <summary>
        /// オーディオ状態変更処理
        /// </summary>
        private void HandleAudioStateChange(GameState newState)
        {
            if (AudioManager == null) return;
            
            switch (newState)
            {
                case GameState.MainMenu:
                    AudioManager.PlayBGM("title");
                    break;
                    
                case GameState.InGame:
                    AudioManager.PlayBGM("dungeon");
                    break;
                    
                case GameState.Paused:
                    AudioManager.PauseAll();
                    break;
                    
                case GameState.GameOver:
                    AudioManager.StopBGM();
                    break;
            }
        }
        
        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            // Escape key handling
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (currentState)
                {
                    case GameState.InGame:
                        ChangeState(GameState.Paused);
                        break;
                        
                    case GameState.Paused:
                    case GameState.Settings:
                        ChangeState(GameState.InGame);
                        break;
                }
            }
            
            // Debug keys (only in debug mode)
            if (debugMode)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    ToggleDebugInfo();
                }
            }
        }
        
        /// <summary>
        /// マネージャー更新処理
        /// </summary>
        private void UpdateManagers()
        {
            // Update managers based on current state
            if (currentState == GameState.InGame && !isLoadingScene)
            {
                // Game-specific updates
                // Managers are automatically updated via their own Update methods
            }
        }
        
        /// <summary>
        /// シーン読み込み完了時の処理
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isLoadingScene = false;
            
            if (debugMode)
            {
                Debug.Log($"🎬 Scene loaded: {scene.name}");
            }
        }
        
        /// <summary>
        /// シーンアンロード時の処理
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            if (debugMode)
            {
                Debug.Log($"🎬 Scene unloaded: {scene.name}");
            }
        }
        
        /// <summary>
        /// 新しいゲームを開始
        /// </summary>
        public void StartNewGame()
        {
            // Play button sound effect
            if (AudioManager != null)
            {
                AudioManager.PlaySE("button");
            }
            
            if (debugMode)
            {
                Debug.Log("🆕 Starting new game...");
            }
            
            // Initialize new game data with authentic Black Onyx system
            BlackOnyxDungeonManager?.InitializeNewGame();
            
            // Load game scene
            LoadGameScene();
        }
        
        /// <summary>
        /// ゲームを読み込み
        /// </summary>
        public void LoadGame()
        {
            if (debugMode)
            {
                Debug.Log("📁 Loading saved game...");
            }
            
            // Load game data
            if (SaveManager?.LoadGame() == true)
            {
                ChangeState(GameState.InGame);
            }
            else
            {
                Debug.LogWarning("⚠️ Failed to load game data");
            }
        }
        
        /// <summary>
        /// ゲームを保存
        /// </summary>
        public void SaveGame()
        {
            if (debugMode)
            {
                Debug.Log("💾 Saving game...");
            }
            
            SaveManager?.SaveGame();
        }
        
        
        /// <summary>
        /// ゲーム終了
        /// </summary>
        public void QuitGame()
        {
            // Play button sound effect
            if (AudioManager != null)
            {
                AudioManager.PlaySE("button");
            }
            
            if (debugMode)
            {
                Debug.Log("👋 Quitting game...");
            }
            
            SaveGame(); // Auto-save before quit
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Play button sound effect
            if (AudioManager != null)
            {
                AudioManager.PlaySE("button");
            }
            
            isLoadingScene = true;
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// ゲームシーンに移行
        /// </summary>
        public void LoadGameScene()
        {
            isLoadingScene = true;
            ChangeState(GameState.InGame);
            SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        /// デバッグ情報の表示切り替え
        /// </summary>
        private void ToggleDebugInfo()
        {
            // Toggle debug UI or console
            Debug.Log("🔍 Debug info toggled");
        }
        
        /// <summary>
        /// FPS情報の取得
        /// </summary>
        public float GetFPS()
        {
            return 1f / Time.unscaledDeltaTime;
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentState == GameState.InGame)
            {
                ChangeState(GameState.Paused);
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentState == GameState.InGame)
            {
                ChangeState(GameState.Paused);
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from scene events
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}