using UnityEngine;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ファイヤークリスタル連動システム
    /// ブラックオニキス復刻版とファイヤークリスタルの世界観・システム連動
    /// </summary>
    public class FireCrystalSystem : MonoBehaviour
    {
        [Header("Fire Crystal Settings")]
        [SerializeField] private bool enableFireCrystalIntegration = true;
        [SerializeField] private bool debugMode = true;
        
        [Header("Crystal Power Settings")]
        [SerializeField] private float crystalPowerMultiplier = 1.5f;
        [SerializeField] private int maxCrystalLevel = 10;
        [SerializeField] private float crystalRegenRate = 1.0f;
        
        // Crystal system state
        private int currentCrystalPower = 0;
        private int crystalLevel = 1;
        private float crystalEnergy = 100f;
        private bool hasFireCrystal = false;
        
        // Integration with other systems
        private GameManager gameManager;
        private BlackOnyxDungeonManager dungeonManager;
        private CombatManager combatManager;
        
        // Fire Crystal items and abilities
        private Dictionary<string, FireCrystalAbility> crystalAbilities = new Dictionary<string, FireCrystalAbility>();
        private List<FireCrystalItem> discoveredCrystals = new List<FireCrystalItem>();
        
        // Events
        public System.Action<int> OnCrystalPowerChanged;
        public System.Action<FireCrystalAbility> OnAbilityUnlocked;
        public System.Action<FireCrystalItem> OnCrystalDiscovered;
        
        void Start()
        {
            if (enableFireCrystalIntegration)
            {
                InitializeFireCrystalSystem();
            }
        }
        
        void Update()
        {
            if (enableFireCrystalIntegration && hasFireCrystal)
            {
                UpdateCrystalEnergy();
                ProcessCrystalEffects();
            }
        }
        
        /// <summary>
        /// ファイヤークリスタルシステム初期化
        /// </summary>
        private void InitializeFireCrystalSystem()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.BlackOnyxDungeonManager;
                combatManager = gameManager.GetComponent<CombatManager>();
            }
            
            // Initialize crystal abilities
            InitializeCrystalAbilities();
            
            // Subscribe to game events
            if (dungeonManager != null)
            {
                dungeonManager.OnCellEntered += OnCellEntered;
                dungeonManager.OnFloorChanged += OnFloorChanged;
            }
            
            if (debugMode)
            {
                Debug.Log("🔥💎 Fire Crystal system initialized - Integration with Black Onyx active");
            }
        }
        
        /// <summary>
        /// クリスタルアビリティの初期化
        /// </summary>
        private void InitializeCrystalAbilities()
        {
            // Fire-based abilities from Fire Crystal lore
            crystalAbilities["flame_sword"] = new FireCrystalAbility
            {
                name = "炎の剣",
                description = "武器に炎の力を宿し、攻撃力を大幅上昇",
                requiredLevel = 3,
                energyCost = 20f,
                duration = 30f,
                abilityType = CrystalAbilityType.Combat
            };
            
            crystalAbilities["fire_shield"] = new FireCrystalAbility
            {
                name = "炎の盾",
                description = "炎のバリアで敵の攻撃を無効化",
                requiredLevel = 2,
                energyCost = 15f,
                duration = 20f,
                abilityType = CrystalAbilityType.Defense
            };
            
            crystalAbilities["crystal_sight"] = new FireCrystalAbility
            {
                name = "クリスタルの眼",
                description = "見えない壁や隠された通路を発見",
                requiredLevel = 4,
                energyCost = 10f,
                duration = 60f,
                abilityType = CrystalAbilityType.Utility
            };
            
            crystalAbilities["flame_teleport"] = new FireCrystalAbility
            {
                name = "炎瞬移",
                description = "炎の力で瞬間移動（フロア内限定）",
                requiredLevel = 6,
                energyCost = 30f,
                duration = 0f,
                abilityType = CrystalAbilityType.Movement
            };
            
            crystalAbilities["dragon_call"] = new FireCrystalAbility
            {
                name = "ドラゴンコール",
                description = "古代ドラゴンの力を借りて強大な敵を倒す",
                requiredLevel = 8,
                energyCost = 50f,
                duration = 10f,
                abilityType = CrystalAbilityType.Ultimate
            };
            
            crystalAbilities["onyx_resonance"] = new FireCrystalAbility
            {
                name = "オニキス共鳴",
                description = "ブラックオニキスとファイヤークリスタルが共鳴し、全能力向上",
                requiredLevel = 10,
                energyCost = 80f,
                duration = 120f,
                abilityType = CrystalAbilityType.Ultimate
            };
        }
        
        /// <summary>
        /// セル進入時の処理（クリスタル発見等）
        /// </summary>
        private void OnCellEntered(DungeonCell cell)
        {
            if (!enableFireCrystalIntegration) return;
            
            // ファイヤークリスタルの欠片発見チェック
            if (cell.type == DungeonCellType.SpecialRoom && 
                cell.specialType.Contains("クリスタル"))
            {
                DiscoverCrystalFragment(cell);
            }
            
            // 特定フロアでの特殊クリスタルイベント
            CheckFloorSpecificCrystalEvents(cell);
        }
        
        /// <summary>
        /// フロア変更時の処理
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            if (!enableFireCrystalIntegration) return;
            
            // 特定フロアでのクリスタルイベント
            switch (newFloor)
            {
                case -6: // B6 カラー迷路
                    HandleColorMazeCrystalEvent();
                    break;
                    
                case -5: // B5 井戸フロア
                    HandleWellCrystalEvent();
                    break;
                    
                case 2: // 天界
                    HandleHeavenCrystalEvent();
                    break;
            }
        }
        
        /// <summary>
        /// クリスタルエネルギー更新
        /// </summary>
        private void UpdateCrystalEnergy()
        {
            if (crystalEnergy < 100f)
            {
                crystalEnergy += crystalRegenRate * Time.deltaTime;
                crystalEnergy = Mathf.Min(crystalEnergy, 100f);
            }
        }
        
        /// <summary>
        /// クリスタル効果処理
        /// </summary>
        private void ProcessCrystalEffects()
        {
            // アクティブなアビリティの効果処理
            foreach (var ability in crystalAbilities.Values)
            {
                if (ability.isActive && ability.remainingDuration > 0)
                {
                    ability.remainingDuration -= Time.deltaTime;
                    ProcessActiveAbility(ability);
                    
                    if (ability.remainingDuration <= 0)
                    {
                        DeactivateAbility(ability);
                    }
                }
            }
        }
        
        /// <summary>
        /// クリスタル欠片発見処理
        /// </summary>
        private void DiscoverCrystalFragment(DungeonCell cell)
        {
            var crystalItem = new FireCrystalItem
            {
                name = "ファイヤークリスタルの欠片",
                description = "古代の炎の力が宿った神秘的な結晶",
                powerValue = Random.Range(5, 15),
                crystalType = CrystalType.Fragment,
                floorFound = dungeonManager.CurrentFloor
            };
            
            discoveredCrystals.Add(crystalItem);
            currentCrystalPower += crystalItem.powerValue;
            
            // クリスタルレベル上昇チェック
            CheckCrystalLevelUp();
            
            OnCrystalDiscovered?.Invoke(crystalItem);
            
            if (debugMode)
            {
                Debug.Log($"🔥 クリスタル欠片発見: {crystalItem.name} (パワー+{crystalItem.powerValue})");
            }
        }
        
        /// <summary>
        /// カラー迷路でのクリスタルイベント
        /// </summary>
        private void HandleColorMazeCrystalEvent()
        {
            if (Random.Range(0f, 1f) < 0.3f) // 30%の確率
            {
                var specialCrystal = new FireCrystalItem
                {
                    name = "虹色ファイヤークリスタル",
                    description = "カラー迷路の力を吸収した特殊なクリスタル",
                    powerValue = 25,
                    crystalType = CrystalType.Rainbow,
                    floorFound = -6
                };
                
                discoveredCrystals.Add(specialCrystal);
                currentCrystalPower += specialCrystal.powerValue;
                
                if (debugMode)
                {
                    Debug.Log("🌈🔥 虹色ファイヤークリスタル発見！カラー迷路の力を獲得");
                }
            }
        }
        
        /// <summary>
        /// 井戸フロアでのクリスタルイベント
        /// </summary>
        private void HandleWellCrystalEvent()
        {
            if (HasCrystalOfType(CrystalType.Fragment))
            {
                // 井戸の水でクリスタルが浄化・強化される
                foreach (var crystal in discoveredCrystals)
                {
                    if (crystal.crystalType == CrystalType.Fragment)
                    {
                        crystal.powerValue = Mathf.RoundToInt(crystal.powerValue * 1.2f);
                        crystal.description += " (井戸で浄化済み)";
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log("💧🔥 井戸の力でクリスタルが浄化・強化されました");
                }
            }
        }
        
        /// <summary>
        /// 天界でのクリスタルイベント
        /// </summary>
        private void HandleHeavenCrystalEvent()
        {
            // ブラックオニキスとファイヤークリスタルの共鳴
            if (hasFireCrystal && currentCrystalPower >= 100)
            {
                UnlockAbility("onyx_resonance");
                
                if (debugMode)
                {
                    Debug.Log("⚫🔥 ブラックオニキスとファイヤークリスタルが共鳴！究極の力が開放された");
                }
            }
        }
        
        /// <summary>
        /// フロア固有クリスタルイベントチェック
        /// </summary>
        private void CheckFloorSpecificCrystalEvents(DungeonCell cell)
        {
            int currentFloor = dungeonManager.CurrentFloor;
            
            // B3（強敵フロア）でのクリスタル戦闘強化
            if (currentFloor == -3 && cell.hasEnemy && hasFireCrystal)
            {
                if (Random.Range(0f, 1f) < 0.2f) // 20%の確率
                {
                    ActivateAbility("flame_sword");
                }
            }
            
            // B6（カラー迷路）での透視能力
            if (currentFloor == -6 && hasFireCrystal)
            {
                if (crystalLevel >= 4)
                {
                    ActivateAbility("crystal_sight");
                }
            }
        }
        
        /// <summary>
        /// クリスタルレベルアップチェック
        /// </summary>
        private void CheckCrystalLevelUp()
        {
            int newLevel = Mathf.Min(currentCrystalPower / 50 + 1, maxCrystalLevel);
            
            if (newLevel > crystalLevel)
            {
                crystalLevel = newLevel;
                OnCrystalPowerChanged?.Invoke(currentCrystalPower);
                
                // 新しいアビリティアンロック
                foreach (var ability in crystalAbilities.Values)
                {
                    if (ability.requiredLevel == crystalLevel && !ability.isUnlocked)
                    {
                        UnlockAbility(ability);
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log($"🔥⬆️ クリスタルレベルアップ: Lv.{crystalLevel} (パワー: {currentCrystalPower})");
                }
            }
        }
        
        /// <summary>
        /// アビリティアンロック
        /// </summary>
        private void UnlockAbility(string abilityKey)
        {
            if (crystalAbilities.ContainsKey(abilityKey))
            {
                UnlockAbility(crystalAbilities[abilityKey]);
            }
        }
        
        private void UnlockAbility(FireCrystalAbility ability)
        {
            ability.isUnlocked = true;
            OnAbilityUnlocked?.Invoke(ability);
            
            if (debugMode)
            {
                Debug.Log($"🔓🔥 新アビリティ解放: {ability.name} - {ability.description}");
            }
        }
        
        /// <summary>
        /// アビリティ発動
        /// </summary>
        public bool ActivateAbility(string abilityKey)
        {
            if (!crystalAbilities.ContainsKey(abilityKey)) return false;
            
            var ability = crystalAbilities[abilityKey];
            
            if (!ability.isUnlocked || crystalEnergy < ability.energyCost)
            {
                return false;
            }
            
            crystalEnergy -= ability.energyCost;
            ability.isActive = true;
            ability.remainingDuration = ability.duration;
            
            ProcessActiveAbility(ability);
            
            if (debugMode)
            {
                Debug.Log($"🔥✨ アビリティ発動: {ability.name}");
            }
            
            return true;
        }
        
        /// <summary>
        /// アクティブアビリティ処理
        /// </summary>
        private void ProcessActiveAbility(FireCrystalAbility ability)
        {
            switch (ability.abilityType)
            {
                case CrystalAbilityType.Combat:
                    if (combatManager != null)
                    {
                        // 攻撃力強化
                        var stats = combatManager.PlayerStats;
                        if (ability.name == "炎の剣")
                        {
                            stats.attack = Mathf.RoundToInt(stats.attack * crystalPowerMultiplier);
                        }
                    }
                    break;
                    
                case CrystalAbilityType.Defense:
                    // 防御力強化
                    if (combatManager != null && ability.name == "炎の盾")
                    {
                        var stats = combatManager.PlayerStats;
                        stats.defense = Mathf.RoundToInt(stats.defense * crystalPowerMultiplier);
                    }
                    break;
                    
                case CrystalAbilityType.Utility:
                    // ユーティリティ効果
                    if (ability.name == "クリスタルの眼")
                    {
                        // 透視能力実装（UI側で対応）
                    }
                    break;
                    
                case CrystalAbilityType.Movement:
                    // 移動系アビリティ
                    break;
                    
                case CrystalAbilityType.Ultimate:
                    // 究極アビリティ
                    ProcessUltimateAbility(ability);
                    break;
            }
        }
        
        /// <summary>
        /// アビリティ無効化
        /// </summary>
        private void DeactivateAbility(FireCrystalAbility ability)
        {
            ability.isActive = false;
            ability.remainingDuration = 0f;
            
            // 効果解除処理
            if (combatManager != null)
            {
                // ステータスをリセット（基本値に戻す）
                combatManager.RecalculateStats();
            }
            
            if (debugMode)
            {
                Debug.Log($"🔥💨 アビリティ終了: {ability.name}");
            }
        }
        
        /// <summary>
        /// 究極アビリティ処理
        /// </summary>
        private void ProcessUltimateAbility(FireCrystalAbility ability)
        {
            switch (ability.name)
            {
                case "ドラゴンコール":
                    // 全敵に大ダメージ
                    var enemyManager = gameManager.GetComponent<EnemyManager>();
                    if (enemyManager != null)
                    {
                        var enemies = enemyManager.ActiveEnemies;
                        foreach (var enemy in enemies)
                        {
                            // ドラゴンの炎で大ダメージ
                            if (enemy != null && enemy.IsAlive)
                            {
                                // enemy.TakeDamage(999); // 実装依存
                            }
                        }
                    }
                    break;
                    
                case "オニキス共鳴":
                    // 全能力大幅強化
                    if (combatManager != null)
                    {
                        var stats = combatManager.PlayerStats;
                        stats.attack = Mathf.RoundToInt(stats.attack * 2.0f);
                        stats.defense = Mathf.RoundToInt(stats.defense * 2.0f);
                        stats.maxHealth = Mathf.RoundToInt(stats.maxHealth * 1.5f);
                        stats.currentHealth = stats.maxHealth;
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 特定タイプのクリスタル所持チェック
        /// </summary>
        private bool HasCrystalOfType(CrystalType type)
        {
            return discoveredCrystals.Exists(c => c.crystalType == type);
        }
        
        // Public API
        
        /// <summary>
        /// ファイヤークリスタル入手
        /// </summary>
        public void ObtainFireCrystal()
        {
            hasFireCrystal = true;
            crystalLevel = 1;
            currentCrystalPower = 50;
            crystalEnergy = 100f;
            
            // 基本アビリティをアンロック
            UnlockAbility("fire_shield");
            
            if (debugMode)
            {
                Debug.Log("🔥💎 ファイヤークリスタル入手！クリスタルシステム開始");
            }
        }
        
        /// <summary>
        /// 現在のクリスタル状態取得
        /// </summary>
        public FireCrystalStatus GetCrystalStatus()
        {
            return new FireCrystalStatus
            {
                hasCrystal = hasFireCrystal,
                level = crystalLevel,
                power = currentCrystalPower,
                energy = crystalEnergy,
                unlockedAbilities = crystalAbilities.Values.Count(a => a.isUnlocked),
                discoveredCrystals = discoveredCrystals.Count
            };
        }
        
        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public string GetDebugInfo()
        {
            var status = GetCrystalStatus();
            return $"Fire Crystal System:\n" +
                   $"Has Crystal: {status.hasCrystal}\n" +
                   $"Level: {status.level}/{maxCrystalLevel}\n" +
                   $"Power: {status.power}\n" +
                   $"Energy: {status.energy:F1}/100\n" +
                   $"Abilities: {status.unlockedAbilities}/{crystalAbilities.Count}\n" +
                   $"Crystals Found: {status.discoveredCrystals}";
        }
    }
    
    /// <summary>
    /// ファイヤークリスタルアビリティ
    /// </summary>
    [System.Serializable]
    public class FireCrystalAbility
    {
        public string name;
        public string description;
        public int requiredLevel;
        public float energyCost;
        public float duration;
        public CrystalAbilityType abilityType;
        
        [System.NonSerialized]
        public bool isUnlocked = false;
        [System.NonSerialized]
        public bool isActive = false;
        [System.NonSerialized]
        public float remainingDuration = 0f;
    }
    
    /// <summary>
    /// ファイヤークリスタルアイテム
    /// </summary>
    [System.Serializable]
    public class FireCrystalItem
    {
        public string name;
        public string description;
        public int powerValue;
        public CrystalType crystalType;
        public int floorFound;
    }
    
    /// <summary>
    /// クリスタル状態情報
    /// </summary>
    [System.Serializable]
    public class FireCrystalStatus
    {
        public bool hasCrystal;
        public int level;
        public int power;
        public float energy;
        public int unlockedAbilities;
        public int discoveredCrystals;
    }
    
    /// <summary>
    /// クリスタルアビリティタイプ
    /// </summary>
    public enum CrystalAbilityType
    {
        Combat,    // 戦闘
        Defense,   // 防御
        Utility,   // ユーティリティ
        Movement,  // 移動
        Ultimate   // 究極
    }
    
    /// <summary>
    /// クリスタルタイプ
    /// </summary>
    public enum CrystalType
    {
        Fragment,  // 欠片
        Rainbow,   // 虹色
        Pure,      // 純粋
        Ancient,   // 古代
        Legendary  // 伝説
    }
}