using UnityEngine;
using System.IO;

namespace BlackOnyxReborn
{
    /// <summary>
    /// セーブデータ管理マネージャー
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "blackonyx_save.json";
        [SerializeField] private bool useEncryption = false;
        
        // Save data structure
        [System.Serializable]
        public class SaveData
        {
            // Player stats
            public string playerName = "プレイヤー";
            public int playerLevel = 1;
            public int playerMaxHealth = 100;
            public int playerCurrentHealth = 100;
            public int playerAttack = 10;
            public int playerDefense = 5;
            public int playerExperience = 0;
            public int playerExperienceToNext = 100;
            public int playerGold = 0;
            
            // Player position
            public Vector2Int playerPosition = new Vector2Int(1, 1);
            public int currentFloor = 1;
            
            // Inventory data
            public InventoryItemSave[] inventoryItems = new InventoryItemSave[0];
            
            // Discovered dungeon data
            public DungeonFloorSave[] discoveredFloors = new DungeonFloorSave[0];
            
            // Game progress
            public float playTime = 0f;
            public string saveDateTime = "";
            public int maxFloorReached = 1;
            public int enemiesKilled = 0;
            public int itemsCollected = 0;
            
            // Game settings
            public float masterVolume = 1f;
            public float bgmVolume = 0.7f;
            public float seVolume = 0.8f;
            public bool fullscreen = false;
            public bool showFPS = false;
            public bool autoSave = true;
        }
        
        [System.Serializable]
        public class InventoryItemSave
        {
            public string itemId;
            public int quantity;
            public string slotKey;
        }
        
        [System.Serializable]
        public class DungeonFloorSave
        {
            public int floorNumber;
            public bool[] exploredCells; // Flattened 2D array
            public int width;
            public int height;
        }
        
        private SaveData currentSaveData;
        private string saveFilePath;
        
        void Awake()
        {
            InitializeSaveSystem();
        }
        
        void Start()
        {
            // Ensure initialization even if Awake failed
            if (currentSaveData == null)
            {
                Debug.LogWarning("⚠️ SaveManager not initialized, reinitializing...");
                InitializeSaveSystem();
            }
        }
        
        /// <summary>
        /// セーブシステム初期化
        /// </summary>
        private void InitializeSaveSystem()
        {
            try
            {
                // Set save file path
                saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
                
                // Initialize with default data
                currentSaveData = new SaveData();
                
                Debug.Log($"💾 Save system initialized. Save path: {saveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to initialize save system: {e.Message}");
                // Ensure currentSaveData is never null
                currentSaveData = new SaveData();
            }
        }
        
        /// <summary>
        /// ゲームデータの保存
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                // Update save data with current game state
                UpdateSaveData();
                
                // Convert to JSON
                string jsonData = JsonUtility.ToJson(currentSaveData, true);
                
                // Apply encryption if enabled
                if (useEncryption)
                {
                    jsonData = EncryptData(jsonData);
                }
                
                // Write to file
                File.WriteAllText(saveFilePath, jsonData);
                
                Debug.Log($"💾 Game saved successfully at {System.DateTime.Now}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save game: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ゲームデータの読み込み
        /// </summary>
        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.LogWarning("⚠️ Save file not found. Using default data.");
                    return false;
                }
                
                // Read from file
                string jsonData = File.ReadAllText(saveFilePath);
                
                // Apply decryption if enabled
                if (useEncryption)
                {
                    jsonData = DecryptData(jsonData);
                }
                
                // Parse JSON
                currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);
                
                // Apply loaded data to game
                ApplySaveData();
                
                Debug.Log($"📁 Game loaded successfully from {currentSaveData.saveDateTime}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load game: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// セーブファイルの存在確認
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(saveFilePath);
        }
        
        /// <summary>
        /// セーブファイルの削除
        /// </summary>
        public bool DeleteSaveFile()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log("🗑️ Save file deleted");
                    return true;
                }
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to delete save file: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 現在のゲーム状態でセーブデータを更新
        /// </summary>
        private void UpdateSaveData()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return;
            
            // Update player stats from CombatManager
            var combatManager = gameManager.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                var playerStats = combatManager.PlayerStats;
                currentSaveData.playerLevel = playerStats.level;
                currentSaveData.playerMaxHealth = playerStats.maxHealth;
                currentSaveData.playerCurrentHealth = playerStats.currentHealth;
                currentSaveData.playerAttack = playerStats.attack;
                currentSaveData.playerDefense = playerStats.defense;
                currentSaveData.playerExperience = playerStats.experience;
                currentSaveData.playerExperienceToNext = playerStats.experienceToNext;
                currentSaveData.playerGold = playerStats.gold;
            }
            
            // Update player position from DungeonManager
            var dungeonManager = gameManager.DungeonManager;
            if (dungeonManager != null)
            {
                currentSaveData.playerPosition = dungeonManager.GetPlayerPosition();
                currentSaveData.currentFloor = dungeonManager.GetCurrentFloorNumber();
                currentSaveData.maxFloorReached = Mathf.Max(currentSaveData.maxFloorReached, currentSaveData.currentFloor);
            }
            
            // Update inventory from ItemManager
            var itemManager = gameManager.GetComponent<ItemManager>();
            if (itemManager != null)
            {
                var inventoryItems = itemManager.InventoryItems;
                currentSaveData.inventoryItems = new InventoryItemSave[inventoryItems.Count];
                
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    var item = inventoryItems[i];
                    currentSaveData.inventoryItems[i] = new InventoryItemSave
                    {
                        itemId = item.itemData.itemId,
                        quantity = item.quantity,
                        slotKey = item.slotKey
                    };
                }
                
                currentSaveData.itemsCollected = inventoryItems.Count;
            }
            
            // Update discovered floors (simplified - just mark current floor as explored)
            SaveCurrentFloorExploration();
            
            // Update play time
            currentSaveData.playTime += Time.unscaledDeltaTime;
            
            // Update save date time
            currentSaveData.saveDateTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            
            // Update audio settings
            var audioManager = gameManager.AudioManager;
            if (audioManager != null)
            {
                // These would need getter methods in AudioManager
                // For now, keep the existing values
            }
        }
        
        /// <summary>
        /// セーブデータをゲームに適用
        /// </summary>
        private void ApplySaveData()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return;
            
            // Apply player stats to CombatManager
            var combatManager = gameManager.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                var playerStats = new PlayerStats
                {
                    level = currentSaveData.playerLevel,
                    maxHealth = currentSaveData.playerMaxHealth,
                    currentHealth = currentSaveData.playerCurrentHealth,
                    attack = currentSaveData.playerAttack,
                    defense = currentSaveData.playerDefense,
                    experience = currentSaveData.playerExperience,
                    experienceToNext = currentSaveData.playerExperienceToNext,
                    gold = currentSaveData.playerGold
                };
                
                combatManager.LoadPlayerStats(playerStats);
            }
            
            // Apply player position to DungeonManager
            var dungeonManager = gameManager.DungeonManager;
            if (dungeonManager != null)
            {
                // Note: This would require additional methods in DungeonManager
                // dungeonManager.SetPlayerPosition(currentSaveData.playerPosition);
                // dungeonManager.ChangeFloor(currentSaveData.currentFloor);
            }
            
            // Apply inventory to ItemManager
            var itemManager = gameManager.GetComponent<ItemManager>();
            if (itemManager != null)
            {
                // Clear current inventory
                itemManager.ClearInventory();
                
                // Restore saved inventory
                foreach (var savedItem in currentSaveData.inventoryItems)
                {
                    var itemData = itemManager.GetItemData(savedItem.itemId);
                    if (itemData != null)
                    {
                        itemManager.AddToInventory(itemData, savedItem.quantity);
                    }
                }
            }
            
            // Apply audio settings
            var audioManager = gameManager.AudioManager;
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(currentSaveData.masterVolume);
                audioManager.SetBGMVolume(currentSaveData.bgmVolume);
                audioManager.SetSEVolume(currentSaveData.seVolume);
            }
            
            // Apply display settings
            Screen.fullScreen = currentSaveData.fullscreen;
            
            // Load discovered floors exploration data
            LoadFloorExplorationData();
        }
        
        /// <summary>
        /// 設定のみ保存
        /// </summary>
        public bool SaveSettings()
        {
            try
            {
                // Update only settings data
                UpdateSettingsData();
                
                // Use PlayerPrefs for settings (lighter than full save)
                PlayerPrefs.SetFloat("MasterVolume", currentSaveData.masterVolume);
                PlayerPrefs.SetFloat("BGMVolume", currentSaveData.bgmVolume);
                PlayerPrefs.SetFloat("SEVolume", currentSaveData.seVolume);
                PlayerPrefs.SetInt("Fullscreen", currentSaveData.fullscreen ? 1 : 0);
                PlayerPrefs.SetInt("ShowFPS", currentSaveData.showFPS ? 1 : 0);
                PlayerPrefs.SetInt("AutoSave", currentSaveData.autoSave ? 1 : 0);
                PlayerPrefs.Save();
                
                Debug.Log("⚙️ Settings saved successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to save settings: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 設定のみ読み込み
        /// </summary>
        public bool LoadSettings()
        {
            try
            {
                currentSaveData.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                currentSaveData.bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
                currentSaveData.seVolume = PlayerPrefs.GetFloat("SEVolume", 0.8f);
                currentSaveData.fullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
                currentSaveData.showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
                currentSaveData.autoSave = PlayerPrefs.GetInt("AutoSave", 1) == 1;
                
                // Apply settings
                ApplySettingsData();
                
                Debug.Log("⚙️ Settings loaded successfully");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load settings: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 設定データの更新
        /// </summary>
        private void UpdateSettingsData()
        {
            // Get current settings from managers
            if (GameManager.Instance != null && GameManager.Instance.AudioManager != null)
            {
                // TODO: Get current audio settings
            }
        }
        
        /// <summary>
        /// 設定データの適用
        /// </summary>
        private void ApplySettingsData()
        {
            // Apply to managers
            if (GameManager.Instance != null && GameManager.Instance.AudioManager != null)
            {
                var audioManager = GameManager.Instance.AudioManager;
                audioManager.SetMasterVolume(currentSaveData.masterVolume);
                audioManager.SetBGMVolume(currentSaveData.bgmVolume);
                audioManager.SetSEVolume(currentSaveData.seVolume);
            }
            
            // Apply display settings
            Screen.fullScreen = currentSaveData.fullscreen;
        }
        
        /// <summary>
        /// データの暗号化
        /// </summary>
        private string EncryptData(string data)
        {
            // Simple XOR encryption (for basic obfuscation)
            const string key = "BlackOnyxReborn2025";
            char[] encrypted = new char[data.Length];
            
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = (char)(data[i] ^ key[i % key.Length]);
            }
            
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encrypted));
        }
        
        /// <summary>
        /// データの復号化
        /// </summary>
        private string DecryptData(string encryptedData)
        {
            try
            {
                const string key = "BlackOnyxReborn2025";
                byte[] encryptedBytes = System.Convert.FromBase64String(encryptedData);
                char[] encrypted = System.Text.Encoding.UTF8.GetChars(encryptedBytes);
                char[] decrypted = new char[encrypted.Length];
                
                for (int i = 0; i < encrypted.Length; i++)
                {
                    decrypted[i] = (char)(encrypted[i] ^ key[i % key.Length]);
                }
                
                return new string(decrypted);
            }
            catch
            {
                throw new System.Exception("Failed to decrypt save data");
            }
        }
        
        /// <summary>
        /// セーブデータの取得
        /// </summary>
        public SaveData GetSaveData()
        {
            return currentSaveData;
        }
        
        /// <summary>
        /// 自動セーブの実行
        /// </summary>
        public void AutoSave()
        {
            if (currentSaveData != null && currentSaveData.autoSave)
            {
                SaveGame();
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentSaveData != null)
            {
                AutoSave();
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && currentSaveData != null)
            {
                AutoSave();
            }
        }
        
        /// <summary>
        /// 現在のフロアの探索状況を保存
        /// </summary>
        private void SaveCurrentFloorExploration()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.DungeonManager == null) return;
            
            var dungeonManager = gameManager.DungeonManager;
            int currentFloor = dungeonManager.GetCurrentFloorNumber();
            var dungeonSize = dungeonManager.GetDungeonSize();
            
            // Find existing floor save or create new one
            DungeonFloorSave floorSave = null;
            var floorsList = new System.Collections.Generic.List<DungeonFloorSave>(currentSaveData.discoveredFloors);
            
            for (int i = 0; i < floorsList.Count; i++)
            {
                if (floorsList[i].floorNumber == currentFloor)
                {
                    floorSave = floorsList[i];
                    break;
                }
            }
            
            if (floorSave == null)
            {
                floorSave = new DungeonFloorSave
                {
                    floorNumber = currentFloor,
                    width = dungeonSize.x,
                    height = dungeonSize.y,
                    exploredCells = new bool[dungeonSize.x * dungeonSize.y]
                };
                floorsList.Add(floorSave);
            }
            
            // Update exploration data (simplified - mark all as explored for now)
            for (int i = 0; i < floorSave.exploredCells.Length; i++)
            {
                floorSave.exploredCells[i] = true; // In a full implementation, this would check actual exploration
            }
            
            currentSaveData.discoveredFloors = floorsList.ToArray();
        }
        
        /// <summary>
        /// フロア探索データの読み込み
        /// </summary>
        private void LoadFloorExplorationData()
        {
            // This would restore the exploration state of dungeon floors
            // Implementation would depend on how DungeonManager stores exploration data
            Debug.Log($"💾 Loaded exploration data for {currentSaveData.discoveredFloors.Length} floors");
        }
        
        /// <summary>
        /// セーブファイル情報の取得
        /// </summary>
        public SaveFileInfo GetSaveFileInfo()
        {
            if (!HasSaveFile()) return null;
            
            try
            {
                var fileInfo = new System.IO.FileInfo(saveFilePath);
                return new SaveFileInfo
                {
                    exists = true,
                    lastModified = fileInfo.LastWriteTime,
                    fileSize = fileInfo.Length,
                    playerName = currentSaveData?.playerName ?? "Unknown",
                    playerLevel = currentSaveData?.playerLevel ?? 1,
                    currentFloor = currentSaveData?.currentFloor ?? 1,
                    playTime = currentSaveData?.playTime ?? 0f,
                    saveDateTime = currentSaveData?.saveDateTime ?? "Unknown"
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to get save file info: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 新規ゲーム用データの初期化
        /// </summary>
        public void InitializeNewGameData()
        {
            currentSaveData = new SaveData();
            
            // Reset all data to defaults
            currentSaveData.playerName = "プレイヤー";
            currentSaveData.playerLevel = 1;
            currentSaveData.playerMaxHealth = 100;
            currentSaveData.playerCurrentHealth = 100;
            currentSaveData.playerAttack = 10;
            currentSaveData.playerDefense = 5;
            currentSaveData.playerExperience = 0;
            currentSaveData.playerExperienceToNext = 100;
            currentSaveData.playerGold = 0;
            currentSaveData.playerPosition = new Vector2Int(1, 1);
            currentSaveData.currentFloor = 1;
            currentSaveData.maxFloorReached = 1;
            currentSaveData.enemiesKilled = 0;
            currentSaveData.itemsCollected = 0;
            currentSaveData.playTime = 0f;
            currentSaveData.inventoryItems = new InventoryItemSave[0];
            currentSaveData.discoveredFloors = new DungeonFloorSave[0];
            
            Debug.Log("🆕 New game data initialized");
        }
        
        /// <summary>
        /// クイックセーブ
        /// </summary>
        public bool QuickSave()
        {
            Debug.Log("⚡ Quick save...");
            return SaveGame();
        }
        
        /// <summary>
        /// クイックロード
        /// </summary>
        public bool QuickLoad()
        {
            Debug.Log("⚡ Quick load...");
            return LoadGame();
        }
        
        /// <summary>
        /// セーブデータの統計情報取得
        /// </summary>
        public SaveStatistics GetSaveStatistics()
        {
            return new SaveStatistics
            {
                playTime = currentSaveData.playTime,
                playerLevel = currentSaveData.playerLevel,
                currentFloor = currentSaveData.currentFloor,
                maxFloorReached = currentSaveData.maxFloorReached,
                enemiesKilled = currentSaveData.enemiesKilled,
                itemsCollected = currentSaveData.itemsCollected,
                gold = currentSaveData.playerGold,
                inventoryCount = currentSaveData.inventoryItems.Length,
                exploredFloorsCount = currentSaveData.discoveredFloors.Length
            };
        }
    }
    
    /// <summary>
    /// セーブファイル情報
    /// </summary>
    [System.Serializable]
    public class SaveFileInfo
    {
        public bool exists;
        public System.DateTime lastModified;
        public long fileSize;
        public string playerName;
        public int playerLevel;
        public int currentFloor;
        public float playTime;
        public string saveDateTime;
        
        public string GetFormattedPlayTime()
        {
            int hours = Mathf.FloorToInt(playTime / 3600);
            int minutes = Mathf.FloorToInt((playTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(playTime % 60);
            
            if (hours > 0)
                return $"{hours:00}:{minutes:00}:{seconds:00}";
            else
                return $"{minutes:00}:{seconds:00}";
        }
        
        public string GetFormattedFileSize()
        {
            if (fileSize < 1024)
                return $"{fileSize} B";
            else if (fileSize < 1024 * 1024)
                return $"{(fileSize / 1024.0):F1} KB";
            else
                return $"{(fileSize / (1024.0 * 1024.0)):F1} MB";
        }
    }
    
    /// <summary>
    /// セーブ統計情報
    /// </summary>
    [System.Serializable]
    public class SaveStatistics
    {
        public float playTime;
        public int playerLevel;
        public int currentFloor;
        public int maxFloorReached;
        public int enemiesKilled;
        public int itemsCollected;
        public int gold;
        public int inventoryCount;
        public int exploredFloorsCount;
    }
}