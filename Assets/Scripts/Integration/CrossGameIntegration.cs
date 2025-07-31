using UnityEngine;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ゲーム間連動システム
    /// ブラックオニキス復刻版と他のゲーム（ファイヤークリスタル等）との連動機能
    /// </summary>
    public class CrossGameIntegration : MonoBehaviour
    {
        [Header("Cross-Game Integration Settings")]
        [SerializeField] private bool enableCrossGameFeatures = true;
        [SerializeField] private string saveDataPath = "";
        [SerializeField] private bool debugMode = true;
        
        [Header("Fire Crystal Integration")]
        [SerializeField] private bool enableFireCrystalIntegration = true;
        [SerializeField] private string fireCrystalDataFile = "fire_crystal_data.json";
        
        // Integration systems
        private FireCrystalSystem fireCrystalSystem;
        private GameManager gameManager;
        private BlackOnyxDungeonManager dungeonManager;
        
        // Cross-game data
        private CrossGameData crossGameData;
        private List<UnlockableContent> unlockedContent = new List<UnlockableContent>();
        
        // Events
        public System.Action<string> OnCrossGameContentUnlocked;
        public System.Action<CrossGameReward> OnRewardReceived;
        
        void Start()
        {
            if (enableCrossGameFeatures)
            {
                InitializeCrossGameIntegration();
            }
        }
        
        /// <summary>
        /// ゲーム間連動システム初期化
        /// </summary>
        private void InitializeCrossGameIntegration()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.BlackOnyxDungeonManager;
            }
            
            // Initialize Fire Crystal integration
            if (enableFireCrystalIntegration)
            {
                fireCrystalSystem = GetComponent<FireCrystalSystem>() ?? gameObject.AddComponent<FireCrystalSystem>();
                InitializeFireCrystalIntegration();
            }
            
            // Load cross-game data
            LoadCrossGameData();
            
            // Check for existing rewards
            CheckForCrossGameRewards();
            
            if (debugMode)
            {
                Debug.Log("🔗 Cross-Game Integration initialized");
            }
        }
        
        /// <summary>
        /// ファイヤークリスタル連動初期化
        /// </summary>
        private void InitializeFireCrystalIntegration()
        {
            if (fireCrystalSystem != null)
            {
                // Subscribe to Fire Crystal events
                fireCrystalSystem.OnCrystalDiscovered += OnFireCrystalDiscovered;
                fireCrystalSystem.OnAbilityUnlocked += OnFireCrystalAbilityUnlocked;
                
                // Check if player has Fire Crystal from other game
                if (HasFireCrystalFromOtherGame())
                {
                    fireCrystalSystem.ObtainFireCrystal();
                    UnlockFireCrystalContent();
                }
            }
            
            if (debugMode)
            {
                Debug.Log("🔥💎 Fire Crystal integration initialized");
            }
        }
        
        /// <summary>
        /// ファイヤークリスタル発見イベント
        /// </summary>
        private void OnFireCrystalDiscovered(FireCrystalItem crystal)
        {
            // Record discovery for cross-game rewards
            RecordCrossGameProgress("fire_crystal_discovered", crystal.name);
            
            // Check for special combinations
            CheckFireCrystalCombinations();
        }
        
        /// <summary>
        /// ファイヤークリスタルアビリティアンロックイベント
        /// </summary>
        private void OnFireCrystalAbilityUnlocked(FireCrystalAbility ability)
        {
            // Record ability unlock for cross-game progression
            RecordCrossGameProgress("ability_unlocked", ability.name);
            
            // Special rewards for ultimate abilities
            if (ability.abilityType == CrystalAbilityType.Ultimate)
            {
                GrantUltimateAbilityReward(ability);
            }
        }
        
        /// <summary>
        /// ブラックオニキス×ファイヤークリスタル特殊連動
        /// </summary>
        private void CheckFireCrystalCombinations()
        {
            if (dungeonManager?.CurrentFloor == 2) // 天界
            {
                var crystalStatus = fireCrystalSystem?.GetCrystalStatus();
                if (crystalStatus != null && crystalStatus.level >= 8)
                {
                    // ブラックオニキス＋ファイヤークリスタル究極合成
                    UnlockUltimateCombination();
                }
            }
        }
        
        /// <summary>
        /// 究極合成アンロック
        /// </summary>
        private void UnlockUltimateCombination()
        {
            var ultimateReward = new CrossGameReward
            {
                id = "black_onyx_fire_crystal_fusion",
                name = "ブラック・ファイヤー・オニキス",
                description = "ブラックオニキスとファイヤークリスタルが融合した究極の宝石",
                rewardType = RewardType.UltimateItem,
                powerValue = 1000,
                unlockCondition = "天界でファイヤークリスタルLv8以上"
            };
            
            GrantCrossGameReward(ultimateReward);
            
            // 特殊エンディングフラグ
            RecordCrossGameProgress("ultimate_fusion_achieved", "true");
            
            if (debugMode)
            {
                Debug.Log("🏆⚫🔥 究極合成達成：ブラック・ファイヤー・オニキス生成！");
            }
        }
        
        /// <summary>
        /// 他ゲームからのファイヤークリスタル所持チェック
        /// </summary>
        private bool HasFireCrystalFromOtherGame()
        {
            // 実際の実装では他ゲームのセーブデータをチェック
            string otherGameSavePath = GetOtherGameSavePath("FireCrystal");
            if (System.IO.File.Exists(otherGameSavePath))
            {
                try
                {
                    string saveData = System.IO.File.ReadAllText(otherGameSavePath);
                    return saveData.Contains("fire_crystal_obtained=true");
                }
                catch (System.Exception e)
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"Failed to read Fire Crystal save data: {e.Message}");
                    }
                }
            }
            
            // デバッグ用：PlayerPrefsで簡易チェック
            return PlayerPrefs.GetInt("FireCrystal_Obtained", 0) == 1;
        }
        
        /// <summary>
        /// ファイヤークリスタル連動コンテンツアンロック
        /// </summary>
        private void UnlockFireCrystalContent()
        {
            var content = new List<UnlockableContent>
            {
                new UnlockableContent
                {
                    id = "fire_crystal_floors",
                    name = "炎のフロア",
                    description = "ファイヤークリスタルの力で新たなフロアが開放",
                    contentType = ContentType.NewArea
                },
                new UnlockableContent
                {
                    id = "crystal_enemies",
                    name = "クリスタルモンスター",
                    description = "炎の力を持つ特殊な敵が出現",
                    contentType = ContentType.NewEnemies
                },
                new UnlockableContent
                {
                    id = "fusion_abilities",
                    name = "融合アビリティ",
                    description = "ブラックオニキスの力と合わせた特殊能力",
                    contentType = ContentType.NewAbilities
                }
            };
            
            foreach (var item in content)
            {
                unlockedContent.Add(item);
                OnCrossGameContentUnlocked?.Invoke(item.name);
            }
            
            // 特殊フロア生成
            GenerateFireCrystalFloors();
        }
        
        /// <summary>
        /// ファイヤークリスタル専用フロア生成
        /// </summary>
        private void GenerateFireCrystalFloors()
        {
            if (dungeonManager != null)
            {
                // 地下7階：炎の洞窟（隠しフロア）
                var flameFloorConfig = new FloorData
                {
                    floorNumber = -7,
                    floorName = "地下7階（炎の洞窟）",
                    floorType = FloorType.Standard, // カスタムタイプが必要
                    hasSpecialRoom = true,
                    specialRoomType = "ファイヤークリスタルの祭壇",
                    roomDensity = 0.6f,
                    difficulty = 12 // 超高難易度
                };
                
                if (debugMode)
                {
                    Debug.Log("🔥🏰 隠しフロア「炎の洞窟」が開放されました");
                }
            }
        }
        
        /// <summary>
        /// 究極アビリティ報酬付与
        /// </summary>
        private void GrantUltimateAbilityReward(FireCrystalAbility ability)
        {
            var reward = new CrossGameReward
            {
                id = $"ultimate_ability_{ability.name}",
                name = $"究極称号：{ability.name}マスター",
                description = $"{ability.name}を極めた証",
                rewardType = RewardType.Title,
                powerValue = 0
            };
            
            GrantCrossGameReward(reward);
        }
        
        /// <summary>
        /// ゲーム間進捗記録
        /// </summary>
        private void RecordCrossGameProgress(string key, string value)
        {
            if (crossGameData == null)
            {
                crossGameData = new CrossGameData();
            }
            
            crossGameData.progressData[key] = value;
            crossGameData.lastUpdated = System.DateTime.Now.ToString();
            
            // Save to persistent data path
            SaveCrossGameData();
            
            // Also record in PlayerPrefs for easy access
            PlayerPrefs.SetString($"CrossGame_{key}", value);
            
            if (debugMode)
            {
                Debug.Log($"📊 Cross-game progress recorded: {key} = {value}");
            }
        }
        
        /// <summary>
        /// ゲーム間報酬付与
        /// </summary>
        private void GrantCrossGameReward(CrossGameReward reward)
        {
            if (crossGameData == null)
            {
                crossGameData = new CrossGameData();
            }
            
            crossGameData.rewards.Add(reward);
            OnRewardReceived?.Invoke(reward);
            
            // Save reward data
            SaveCrossGameData();
            
            if (debugMode)
            {
                Debug.Log($"🎁 Cross-game reward granted: {reward.name}");
            }
        }
        
        /// <summary>
        /// ゲーム間報酬チェック
        /// </summary>
        private void CheckForCrossGameRewards()
        {
            // Check for rewards from other games
            CheckAsciiTreasureGuardianRewards();
            CheckOracleQueryComposerRewards();
            CheckGeminiAIPersonalitiesRewards();
        }
        
        /// <summary>
        /// ASCII Treasure Guardian連動報酬
        /// </summary>
        private void CheckAsciiTreasureGuardianRewards()
        {
            if (PlayerPrefs.GetInt("AsciiTreasureGuardian_Completed", 0) == 1)
            {
                var reward = new CrossGameReward
                {
                    id = "ascii_guardian_reward",
                    name = "古代文字の知識",
                    description = "ASCII Treasure Guardianをクリアした証。ダンジョンマップが詳細表示される",
                    rewardType = RewardType.Ability,
                    powerValue = 0
                };
                
                GrantCrossGameReward(reward);
            }
        }
        
        /// <summary>
        /// Oracle Query Composer連動報酬
        /// </summary>
        private void CheckOracleQueryComposerRewards()
        {
            if (PlayerPrefs.GetInt("OracleQueryComposer_MasterLevel", 0) >= 5)
            {
                var reward = new CrossGameReward
                {
                    id = "oracle_wisdom_reward",
                    name = "オラクルの叡智",
                    description = "Oracle Query Composerで高度なクエリを習得。敵の詳細情報が表示される",
                    rewardType = RewardType.Ability,
                    powerValue = 0
                };
                
                GrantCrossGameReward(reward);
            }
        }
        
        /// <summary>
        /// Gemini AI Personalities連動報酬
        /// </summary>
        private void CheckGeminiAIPersonalitiesRewards()
        {
            if (PlayerPrefs.GetInt("GeminiAI_PersonalitiesCreated", 0) >= 10)
            {
                var reward = new CrossGameReward
                {
                    id = "ai_companion_reward",
                    name = "AI仲間召喚",
                    description = "多数のAI個性を作成した証。戦闘時にAI仲間が支援",
                    rewardType = RewardType.Companion,
                    powerValue = 50
                };
                
                GrantCrossGameReward(reward);
            }
        }
        
        /// <summary>
        /// 他ゲームセーブパス取得
        /// </summary>
        private string GetOtherGameSavePath(string gameName)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{gameName}_crossgame_data.json");
        }
        
        /// <summary>
        /// ゲーム間データ保存
        /// </summary>
        private void SaveCrossGameData()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Application.persistentDataPath, "black_onyx_crossgame_data.json");
                string jsonData = JsonUtility.ToJson(crossGameData, true);
                System.IO.File.WriteAllText(dataPath, jsonData);
            }
            catch (System.Exception e)
            {
                if (debugMode)
                {
                    Debug.LogError($"Failed to save cross-game data: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// ゲーム間データ読み込み
        /// </summary>
        private void LoadCrossGameData()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Application.persistentDataPath, "black_onyx_crossgame_data.json");
                if (System.IO.File.Exists(dataPath))
                {
                    string jsonData = System.IO.File.ReadAllText(dataPath);
                    crossGameData = JsonUtility.FromJson<CrossGameData>(jsonData);
                }
                else
                {
                    crossGameData = new CrossGameData();
                }
            }
            catch (System.Exception e)
            {
                if (debugMode)
                {
                    Debug.LogError($"Failed to load cross-game data: {e.Message}");
                }
                crossGameData = new CrossGameData();
            }
        }
        
        // Public API
        
        /// <summary>
        /// ゲーム間データエクスポート
        /// </summary>
        public void ExportCrossGameData(string targetGame)
        {
            var exportData = new CrossGameExportData
            {
                sourceGame = "BlackOnyxReborn",
                targetGame = targetGame,
                playerLevel = GetPlayerLevel(),
                achievedEndings = GetAchievedEndings(),
                specialItems = GetSpecialItems(),
                fireCrystalStatus = fireCrystalSystem?.GetCrystalStatus(),
                exportTimestamp = System.DateTime.Now.ToString()
            };
            
            string exportPath = GetOtherGameSavePath($"Export_To_{targetGame}");
            string jsonData = JsonUtility.ToJson(exportData, true);
            System.IO.File.WriteAllText(exportPath, jsonData);
            
            if (debugMode)
            {
                Debug.Log($"📤 Cross-game data exported to: {targetGame}");
            }
        }
        
        /// <summary>
        /// 現在のクロスゲーム状態取得
        /// </summary>
        public CrossGameStatus GetCrossGameStatus()
        {
            return new CrossGameStatus
            {
                isIntegrationEnabled = enableCrossGameFeatures,
                hasFireCrystal = fireCrystalSystem?.GetCrystalStatus().hasCrystal ?? false,
                unlockedContentCount = unlockedContent.Count,
                totalRewards = crossGameData?.rewards.Count ?? 0,
                connectedGames = GetConnectedGames()
            };
        }
        
        private int GetPlayerLevel()
        {
            // 実装依存：プレイヤーレベル取得
            return PlayerPrefs.GetInt("PlayerLevel", 1);
        }
        
        private List<string> GetAchievedEndings()
        {
            // 実装依存：達成エンディング取得
            var endings = new List<string>();
            if (PlayerPrefs.GetInt("NormalEnding", 0) == 1) endings.Add("Normal");
            if (PlayerPrefs.GetInt("TrueEnding", 0) == 1) endings.Add("True");
            if (PlayerPrefs.GetInt("UltimateEnding", 0) == 1) endings.Add("Ultimate");
            return endings;
        }
        
        private List<string> GetSpecialItems()
        {
            // 実装依存：特殊アイテム取得
            var items = new List<string>();
            if (HasFireCrystalFromOtherGame()) items.Add("FireCrystal");
            return items;
        }
        
        private List<string> GetConnectedGames()
        {
            var games = new List<string>();
            if (PlayerPrefs.HasKey("AsciiTreasureGuardian_Completed")) games.Add("ASCII Treasure Guardian");
            if (PlayerPrefs.HasKey("OracleQueryComposer_MasterLevel")) games.Add("Oracle Query Composer");
            if (PlayerPrefs.HasKey("GeminiAI_PersonalitiesCreated")) games.Add("Gemini AI Personalities");
            return games;
        }
        
        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public string GetDebugInfo()
        {
            var status = GetCrossGameStatus();
            return $"Cross-Game Integration:\n" +
                   $"Integration: {status.isIntegrationEnabled}\n" +
                   $"Fire Crystal: {status.hasFireCrystal}\n" +
                   $"Unlocked Content: {status.unlockedContentCount}\n" +
                   $"Total Rewards: {status.totalRewards}\n" +
                   $"Connected Games: {string.Join(", ", status.connectedGames)}";
        }
    }
    
    // Data classes for cross-game integration
    
    [System.Serializable]
    public class CrossGameData
    {
        public Dictionary<string, string> progressData = new Dictionary<string, string>();
        public List<CrossGameReward> rewards = new List<CrossGameReward>();
        public string lastUpdated;
    }
    
    [System.Serializable]
    public class CrossGameReward
    {
        public string id;
        public string name;
        public string description;
        public RewardType rewardType;
        public int powerValue;
        public string unlockCondition;
    }
    
    [System.Serializable]
    public class UnlockableContent
    {
        public string id;
        public string name;
        public string description;
        public ContentType contentType;
    }
    
    [System.Serializable]
    public class CrossGameExportData
    {
        public string sourceGame;
        public string targetGame;
        public int playerLevel;
        public List<string> achievedEndings;
        public List<string> specialItems;
        public FireCrystalStatus fireCrystalStatus;
        public string exportTimestamp;
    }
    
    [System.Serializable]
    public class CrossGameStatus
    {
        public bool isIntegrationEnabled;
        public bool hasFireCrystal;
        public int unlockedContentCount;
        public int totalRewards;
        public List<string> connectedGames;
    }
    
    public enum RewardType
    {
        Item,          // アイテム
        Ability,       // 能力
        Title,        // 称号
        Companion,    // 仲間
        UltimateItem  // 究極アイテム
    }
    
    public enum ContentType
    {
        NewArea,      // 新エリア
        NewEnemies,   // 新敵
        NewAbilities, // 新能力
        NewEnding     // 新エンディング
    }
}