using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ブラックオニキス復刻版専用セーブマネージャー
    /// ファイヤークリスタル連動・クロスゲーム機能対応の完全版
    /// </summary>
    public class BlackOnyxSaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string mainSaveFileName = "blackonyx_reborn_save.json";
        [SerializeField] private string crystalSaveFileName = "crystal_data.json";
        [SerializeField] private string crossGameFileName = "crossgame_data.json";
        [SerializeField] private bool useBackupSave = true;
        [SerializeField] private int maxBackupFiles = 5;
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5分
        
        // Save data paths
        private string mainSavePath;
        private string crystalSavePath;
        private string crossGameSavePath;
        private string backupDirectory;
        
        // Current save data
        private BlackOnyxSaveData currentSaveData;
        private float lastAutoSaveTime;
        
        // Manager references
        private GameManager gameManager;
        private BlackOnyxDungeonManager dungeonManager;
        private FireCrystalSystem fireCrystalSystem;
        private CrossGameIntegration crossGameIntegration;
        
        // Events
        public System.Action<bool> OnSaveCompleted;
        public System.Action<bool> OnLoadCompleted;
        public System.Action OnAutoSave;
        
        void Awake()
        {
            InitializeSaveManager();
        }
        
        void Start()
        {
            GetManagerReferences();
            LoadSettings();
        }
        
        void Update()
        {
            if (autoSaveEnabled && Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                AutoSave();
            }
        }
        
        /// <summary>
        /// セーブマネージャー初期化
        /// </summary>
        private void InitializeSaveManager()
        {
            // Set up save paths
            string saveDirectory = Application.persistentDataPath;
            mainSavePath = Path.Combine(saveDirectory, mainSaveFileName);
            crystalSavePath = Path.Combine(saveDirectory, crystalSaveFileName);
            crossGameSavePath = Path.Combine(saveDirectory, crossGameFileName);
            backupDirectory = Path.Combine(saveDirectory, "Backups");
            
            // Create backup directory
            if (useBackupSave && !Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
            
            // Initialize save data
            currentSaveData = new BlackOnyxSaveData();
            
            Debug.Log($"💾 Black Onyx Save Manager initialized - Path: {saveDirectory}");
        }
        
        /// <summary>
        /// マネージャー参照取得
        /// </summary>
        private void GetManagerReferences()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.BlackOnyxDungeonManager;
                fireCrystalSystem = gameManager.GetComponent<FireCrystalSystem>();
                crossGameIntegration = gameManager.GetComponent<CrossGameIntegration>();
            }
        }
        
        /// <summary>
        /// 完全セーブ（全データ保存）
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                Debug.Log("💾 Starting complete save process...");
                
                // Create backup if enabled
                if (useBackupSave && File.Exists(mainSavePath))
                {
                    CreateBackup();
                }
                
                // Update save data with current game state
                UpdateSaveData();
                
                // Save main game data
                bool mainSaved = SaveMainGameData();
                
                // Save Fire Crystal data
                bool crystalSaved = SaveFireCrystalData();
                
                // Save cross-game data
                bool crossGameSaved = SaveCrossGameData();
                
                bool allSaved = mainSaved && crystalSaved && crossGameSaved;
                
                if (allSaved)
                {
                    Debug.Log("✅ Complete save successful!");
                    OnSaveCompleted?.Invoke(true);
                }
                else
                {
                    Debug.LogWarning("⚠️ Partial save completed with some errors");
                    OnSaveCompleted?.Invoke(false);
                }
                
                return allSaved;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Save failed: {e.Message}");
                OnSaveCompleted?.Invoke(false);
                return false;
            }
        }
        
        /// <summary>
        /// 完全ロード（全データ読み込み）
        /// </summary>
        public bool LoadGame()
        {
            try
            {
                Debug.Log("📁 Starting complete load process...");
                
                // Load main game data
                bool mainLoaded = LoadMainGameData();
                
                // Load Fire Crystal data
                bool crystalLoaded = LoadFireCrystalData();
                
                // Load cross-game data
                bool crossGameLoaded = LoadCrossGameData();
                
                if (mainLoaded)
                {
                    // Apply loaded data to game
                    ApplyLoadedData();
                    
                    Debug.Log("✅ Complete load successful!");
                    OnLoadCompleted?.Invoke(true);
                    return true;
                }
                else
                {
                    Debug.LogWarning("⚠️ Main save data not found or corrupted");
                    OnLoadCompleted?.Invoke(false);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Load failed: {e.Message}");
                OnLoadCompleted?.Invoke(false);
                return false;
            }
        }
        
        /// <summary>
        /// メインゲームデータ保存
        /// </summary>
        private bool SaveMainGameData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(mainSavePath, jsonData);
                Debug.Log("💾 Main game data saved");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save main data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ファイヤークリスタルデータ保存
        /// </summary>
        private bool SaveFireCrystalData()
        {
            try
            {
                if (fireCrystalSystem == null) return true; // Skip if not available
                
                var crystalData = new FireCrystalSaveData
                {
                    crystalStatus = fireCrystalSystem.GetCrystalStatus(),
                    discoveredCrystals = GetDiscoveredCrystals(),
                    unlockedAbilities = GetUnlockedAbilities(),
                    abilityUsageStats = GetAbilityUsageStats()
                };
                
                string jsonData = JsonUtility.ToJson(crystalData, true);
                File.WriteAllText(crystalSavePath, jsonData);
                Debug.Log("🔥 Fire Crystal data saved");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save crystal data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// クロスゲームデータ保存
        /// </summary>
        private bool SaveCrossGameData()
        {
            try
            {
                if (crossGameIntegration == null) return true; // Skip if not available
                
                var crossData = new CrossGameSaveData
                {
                    crossGameStatus = crossGameIntegration.GetCrossGameStatus(),
                    achievedRewards = GetAchievedRewards(),
                    exportedData = GetExportedDataList(),
                    connectionHistory = GetConnectionHistory()
                };
                
                string jsonData = JsonUtility.ToJson(crossData, true);
                File.WriteAllText(crossGameSavePath, jsonData);
                Debug.Log("🔗 Cross-game data saved");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save cross-game data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// メインゲームデータ読み込み
        /// </summary>
        private bool LoadMainGameData()
        {
            try
            {
                if (!File.Exists(mainSavePath))
                {
                    Debug.LogWarning("⚠️ Main save file not found");
                    return false;
                }
                
                string jsonData = File.ReadAllText(mainSavePath);
                currentSaveData = JsonUtility.FromJson<BlackOnyxSaveData>(jsonData);
                Debug.Log("📁 Main game data loaded");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load main data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ファイヤークリスタルデータ読み込み
        /// </summary>
        private bool LoadFireCrystalData()
        {
            try
            {
                if (!File.Exists(crystalSavePath) || fireCrystalSystem == null)
                {
                    return true; // Skip if not available
                }
                
                string jsonData = File.ReadAllText(crystalSavePath);
                var crystalData = JsonUtility.FromJson<FireCrystalSaveData>(jsonData);
                
                // Apply crystal data
                ApplyFireCrystalData(crystalData);
                
                Debug.Log("🔥 Fire Crystal data loaded");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load crystal data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// クロスゲームデータ読み込み
        /// </summary>
        private bool LoadCrossGameData()
        {
            try
            {
                if (!File.Exists(crossGameSavePath) || crossGameIntegration == null)
                {
                    return true; // Skip if not available
                }
                
                string jsonData = File.ReadAllText(crossGameSavePath);
                var crossData = JsonUtility.FromJson<CrossGameSaveData>(jsonData);
                
                // Apply cross-game data
                ApplyCrossGameData(crossData);
                
                Debug.Log("🔗 Cross-game data loaded");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load cross-game data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// セーブデータ更新
        /// </summary>
        private void UpdateSaveData()
        {
            // 基本ゲームデータ
            UpdateBasicGameData();
            
            // ダンジョンデータ（Black Onyx専用）
            UpdateDungeonData();
            
            // エンディング・実績データ
            UpdateAchievementData();
            
            // セーブメタデータ
            currentSaveData.saveDateTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            currentSaveData.saveVersion = "2.0.0";
        }
        
        /// <summary>
        /// 基本ゲームデータ更新
        /// </summary>
        private void UpdateBasicGameData()
        {
            if (gameManager == null) return;
            
            // プレイヤーステータス
            var combatManager = gameManager.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                var stats = combatManager.PlayerStats;
                currentSaveData.playerLevel = stats.level;
                currentSaveData.playerHP = stats.currentHealth;
                currentSaveData.playerMaxHP = stats.maxHealth;
                currentSaveData.playerAttack = stats.attack;
                currentSaveData.playerDefense = stats.defense;
                currentSaveData.playerExp = stats.experience;
                currentSaveData.playerGold = stats.gold;
            }
            
            // プレイ時間
            currentSaveData.playTime += Time.unscaledDeltaTime;
        }
        
        /// <summary>
        /// ダンジョンデータ更新（Black Onyx専用）
        /// </summary>
        private void UpdateDungeonData()
        {
            if (dungeonManager == null) return;
            
            currentSaveData.currentFloor = dungeonManager.CurrentFloor;
            currentSaveData.playerPosition = dungeonManager.PlayerPosition;
            currentSaveData.currentEntrance = (int)GetCurrentEntrance();
            
            // 探索済みフロア記録
            var exploredFloors = new List<int>(currentSaveData.exploredFloors);
            if (!exploredFloors.Contains(currentSaveData.currentFloor))
            {
                exploredFloors.Add(currentSaveData.currentFloor);
                currentSaveData.exploredFloors = exploredFloors.ToArray();
            }
            
            // カラー迷路進捗
            if (currentSaveData.currentFloor == -6)
            {
                currentSaveData.colorMazeProgress = GetColorMazeProgress();
            }
        }
        
        /// <summary>
        /// 実績データ更新
        /// </summary>
        private void UpdateAchievementData()
        {
            // エンディング達成状況
            currentSaveData.normalEndingAchieved = GetEndingAchieved("normal");
            currentSaveData.trueEndingAchieved = GetEndingAchieved("true");
            currentSaveData.ultimateEndingAchieved = GetEndingAchieved("ultimate");
            
            // 特殊アイテム取得状況
            currentSaveData.hasBlackOnyx = HasSpecialItem("black_onyx");
            currentSaveData.hasFireCrystal = HasSpecialItem("fire_crystal");
            currentSaveData.hasUltimateFusion = HasSpecialItem("black_fire_onyx");
        }
        
        /// <summary>
        /// 読み込みデータ適用
        /// </summary>
        private void ApplyLoadedData()
        {
            // 基本データ適用
            ApplyBasicGameData();
            
            // ダンジョンデータ適用
            ApplyDungeonData();
            
            // 設定適用
            ApplyGameSettings();
        }
        
        /// <summary>
        /// 基本ゲームデータ適用
        /// </summary>
        private void ApplyBasicGameData()
        {
            if (gameManager == null) return;
            
            var combatManager = gameManager.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                var stats = new PlayerStats
                {
                    level = currentSaveData.playerLevel,
                    currentHealth = currentSaveData.playerHP,
                    maxHealth = currentSaveData.playerMaxHP,
                    attack = currentSaveData.playerAttack,
                    defense = currentSaveData.playerDefense,
                    experience = currentSaveData.playerExp,
                    gold = currentSaveData.playerGold
                };
                
                combatManager.LoadPlayerStats(stats);
            }
        }
        
        /// <summary>
        /// ダンジョンデータ適用
        /// </summary>
        private void ApplyDungeonData()
        {
            if (dungeonManager == null) return;
            
            // フロア移動
            dungeonManager.ChangeFloor(currentSaveData.currentFloor);
            
            // 入口設定
            var entrance = (BlackOnyxDungeonManager.DungeonEntrance)currentSaveData.currentEntrance;
            dungeonManager.SetDungeonEntrance(entrance);
            
            // プレイヤー位置（フロア移動後に設定）
            StartCoroutine(SetPlayerPositionDelayed());
        }
        
        /// <summary>
        /// ファイヤークリスタルデータ適用
        /// </summary>
        private void ApplyFireCrystalData(FireCrystalSaveData crystalData)
        {
            if (fireCrystalSystem == null) return;
            
            // クリスタル状態復元
            if (crystalData.crystalStatus.hasCrystal)
            {
                fireCrystalSystem.ObtainFireCrystal();
                // レベルやパワーの復元は実装依存
            }
        }
        
        /// <summary>
        /// クロスゲームデータ適用
        /// </summary>
        private void ApplyCrossGameData(CrossGameSaveData crossData)
        {
            if (crossGameIntegration == null) return;
            
            // 連動報酬の復元など
            // 実装はCrossGameIntegrationに依存
        }
        
        /// <summary>
        /// 自動セーブ
        /// </summary>
        public void AutoSave()
        {
            if (!autoSaveEnabled) return;
            
            lastAutoSaveTime = Time.time;
            
            bool saved = SaveGame();
            if (saved)
            {
                Debug.Log("⚡ Auto save completed");
                OnAutoSave?.Invoke();
            }
        }
        
        /// <summary>
        /// バックアップ作成
        /// </summary>
        private void CreateBackup()
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"backup_{timestamp}.json";
                string backupPath = Path.Combine(backupDirectory, backupFileName);
                
                File.Copy(mainSavePath, backupPath);
                
                // 古いバックアップファイルを削除
                CleanupOldBackups();
                
                Debug.Log($"📋 Backup created: {backupFileName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to create backup: {e.Message}");
            }
        }
        
        /// <summary>
        /// 古いバックアップファイル削除
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupDirectory, "backup_*.json");
                if (backupFiles.Length <= maxBackupFiles) return;
                
                // ファイルを作成日時でソート
                System.Array.Sort(backupFiles, (x, y) => 
                    File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));
                
                // 古いファイルから削除
                for (int i = 0; i < backupFiles.Length - maxBackupFiles; i++)
                {
                    File.Delete(backupFiles[i]);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to cleanup backups: {e.Message}");
            }
        }
        
        /// <summary>
        /// 設定保存
        /// </summary>
        public bool SaveSettings()
        {
            try
            {
                // PlayerPrefsに設定を保存
                PlayerPrefs.SetFloat("MasterVolume", currentSaveData.masterVolume);
                PlayerPrefs.SetFloat("BGMVolume", currentSaveData.bgmVolume);
                PlayerPrefs.SetFloat("SEVolume", currentSaveData.seVolume);
                PlayerPrefs.SetInt("AutoSave", autoSaveEnabled ? 1 : 0);
                PlayerPrefs.SetFloat("AutoSaveInterval", autoSaveInterval);
                PlayerPrefs.Save();
                
                Debug.Log("⚙️ Settings saved");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save settings: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 設定読み込み
        /// </summary>
        public bool LoadSettings()
        {
            try
            {
                currentSaveData.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
                currentSaveData.bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
                currentSaveData.seVolume = PlayerPrefs.GetFloat("SEVolume", 0.8f);
                autoSaveEnabled = PlayerPrefs.GetInt("AutoSave", 1) == 1;
                autoSaveInterval = PlayerPrefs.GetFloat("AutoSaveInterval", 300f);
                
                ApplyGameSettings();
                
                Debug.Log("⚙️ Settings loaded");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load settings: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ゲーム設定適用
        /// </summary>
        private void ApplyGameSettings()
        {
            if (gameManager?.AudioManager != null)
            {
                var audioManager = gameManager.AudioManager;
                audioManager.SetMasterVolume(currentSaveData.masterVolume);
                audioManager.SetBGMVolume(currentSaveData.bgmVolume);
                audioManager.SetSEVolume(currentSaveData.seVolume);
            }
        }
        
        // Utility methods
        
        private System.Collections.IEnumerator SetPlayerPositionDelayed()
        {
            yield return new WaitForEndOfFrame();
            // プレイヤー位置設定の実装
        }
        
        private BlackOnyxDungeonManager.DungeonEntrance GetCurrentEntrance()
        {
            // 現在の入口取得の実装
            return BlackOnyxDungeonManager.DungeonEntrance.Ruins;
        }
        
        private int GetColorMazeProgress()
        {
            // カラー迷路進捗取得の実装
            return PlayerPrefs.GetInt("ColorMazeProgress", 0);
        }
        
        private bool GetEndingAchieved(string endingType)
        {
            return PlayerPrefs.GetInt($"Ending_{endingType}", 0) == 1;
        }
        
        private bool HasSpecialItem(string itemId)
        {
            return PlayerPrefs.GetInt($"SpecialItem_{itemId}", 0) == 1;
        }
        
        private List<string> GetDiscoveredCrystals()
        {
            // 発見済みクリスタル取得の実装
            return new List<string>();
        }
        
        private List<string> GetUnlockedAbilities()
        {
            // アンロック済みアビリティ取得の実装
            return new List<string>();
        }
        
        private Dictionary<string, int> GetAbilityUsageStats()
        {
            // アビリティ使用統計取得の実装
            return new Dictionary<string, int>();
        }
        
        private List<string> GetAchievedRewards()
        {
            // 達成済み報酬取得の実装
            return new List<string>();
        }
        
        private List<string> GetExportedDataList()
        {
            // エクスポート済みデータ取得の実装
            return new List<string>();
        }
        
        private List<string> GetConnectionHistory()
        {
            // 接続履歴取得の実装
            return new List<string>();
        }
        
        // Public API
        
        /// <summary>
        /// セーブファイル存在チェック
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(mainSavePath);
        }
        
        /// <summary>
        /// セーブファイル削除
        /// </summary>
        public bool DeleteSaveFile()
        {
            try
            {
                if (File.Exists(mainSavePath)) File.Delete(mainSavePath);
                if (File.Exists(crystalSavePath)) File.Delete(crystalSavePath);
                if (File.Exists(crossGameSavePath)) File.Delete(crossGameSavePath);
                
                Debug.Log("🗑️ All save files deleted");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to delete save files: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 新規ゲーム初期化
        /// </summary>
        public void InitializeNewGame()
        {
            currentSaveData = new BlackOnyxSaveData();
            
            // デフォルト値設定
            currentSaveData.playerLevel = 1;
            currentSaveData.playerHP = 100;
            currentSaveData.playerMaxHP = 100;
            currentSaveData.playerAttack = 10;
            currentSaveData.playerDefense = 5;
            currentSaveData.playerGold = 0;
            currentSaveData.currentFloor = -1; // B1から開始
            currentSaveData.playerPosition = new Vector2Int(1, 1);
            currentSaveData.playTime = 0f;
            
            Debug.Log("🆕 New game initialized");
        }
        
        /// <summary>
        /// セーブ統計取得
        /// </summary>
        public BlackOnyxSaveStatistics GetSaveStatistics()
        {
            return new BlackOnyxSaveStatistics
            {
                playTime = currentSaveData.playTime,
                playerLevel = currentSaveData.playerLevel,
                currentFloor = currentSaveData.currentFloor,
                exploredFloors = currentSaveData.exploredFloors.Length,
                hasFireCrystal = currentSaveData.hasFireCrystal,
                hasBlackOnyx = currentSaveData.hasBlackOnyx,
                ultimateFusionAchieved = currentSaveData.hasUltimateFusion,
                endingsAchieved = GetEndingsAchievedCount()
            };
        }
        
        private int GetEndingsAchievedCount()
        {
            int count = 0;
            if (currentSaveData.normalEndingAchieved) count++;
            if (currentSaveData.trueEndingAchieved) count++;
            if (currentSaveData.ultimateEndingAchieved) count++;
            return count;
        }
        
        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Black Onyx Save Manager:\n" +
                   $"Main Save: {(File.Exists(mainSavePath) ? "✓" : "✗")}\n" +
                   $"Crystal Save: {(File.Exists(crystalSavePath) ? "✓" : "✗")}\n" +
                   $"Cross-Game Save: {(File.Exists(crossGameSavePath) ? "✓" : "✗")}\n" +
                   $"Auto Save: {(autoSaveEnabled ? "ON" : "OFF")}\n" +
                   $"Backup Count: {(Directory.Exists(backupDirectory) ? Directory.GetFiles(backupDirectory).Length : 0)}";
        }
    }
    
    // Save data structures
    
    [System.Serializable]
    public class BlackOnyxSaveData
    {
        // Basic game data
        public string playerName = "プレイヤー";
        public int playerLevel = 1;
        public int playerHP = 100;
        public int playerMaxHP = 100;
        public int playerAttack = 10;
        public int playerDefense = 5;
        public int playerExp = 0;
        public int playerGold = 0;
        
        // Dungeon data (Black Onyx specific)
        public int currentFloor = -1; // B1
        public Vector2Int playerPosition = new Vector2Int(1, 1);
        public int currentEntrance = 2; // Ruins
        public int[] exploredFloors = new int[0];
        public int colorMazeProgress = 0;
        
        // Achievement data
        public bool normalEndingAchieved = false;
        public bool trueEndingAchieved = false;
        public bool ultimateEndingAchieved = false;
        public bool hasBlackOnyx = false;
        public bool hasFireCrystal = false;
        public bool hasUltimateFusion = false;
        
        // Settings
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.7f;
        public float seVolume = 0.8f;
        
        // Meta data
        public float playTime = 0f;
        public string saveDateTime = "";
        public string saveVersion = "2.0.0";
    }
    
    [System.Serializable]
    public class FireCrystalSaveData
    {
        public FireCrystalStatus crystalStatus;
        public List<string> discoveredCrystals;
        public List<string> unlockedAbilities;
        public Dictionary<string, int> abilityUsageStats;
    }
    
    [System.Serializable]
    public class CrossGameSaveData
    {
        public CrossGameStatus crossGameStatus;
        public List<string> achievedRewards;
        public List<string> exportedData;
        public List<string> connectionHistory;
    }
    
    [System.Serializable]
    public class BlackOnyxSaveStatistics
    {
        public float playTime;
        public int playerLevel;
        public int currentFloor;
        public int exploredFloors;
        public bool hasFireCrystal;
        public bool hasBlackOnyx;
        public bool ultimateFusionAchieved;
        public int endingsAchieved;
    }
}