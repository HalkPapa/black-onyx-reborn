using UnityEngine;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 1984年オリジナルブラックオニキス特殊アイテム管理システム
    /// 透明マント、ブラックオニキス、ファイヤークリスタル、究極合成等を管理
    /// </summary>
    public class SpecialItemManager : MonoBehaviour
    {
        [Header("Special Item Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float invisibilityDuration = 300f; // 透明化持続時間（秒）
        [SerializeField] private float enemyDetectionReduction = 0.8f; // 敵発見率80%減少
        
        // Manager references
        private GameManager gameManager;
        private ItemManager itemManager;
        private CombatManager combatManager;
        private BlackOnyxDungeonManager dungeonManager;
        private FireCrystalSystem fireCrystalSystem;
        private GameUIController uiController;
        
        // Special item states
        private Dictionary<string, SpecialItemState> specialItemStates = new Dictionary<string, SpecialItemState>();
        private bool isInvisible = false;
        private float invisibilityTimeRemaining = 0f;
        
        // Events
        public System.Action<string> OnSpecialItemObtained;
        public System.Action<string> OnSpecialItemUsed;
        public System.Action OnInvisibilityActivated;
        public System.Action OnInvisibilityDeactivated;
        public System.Action OnGameWon;
        
        // Properties
        public bool IsInvisible => isInvisible;
        public float InvisibilityTimeRemaining => invisibilityTimeRemaining;
        public bool HasBlackOnyx => HasSpecialItem("black_onyx");
        public bool HasFireCrystal => HasSpecialItem("fire_crystal");
        public bool HasInvisibilityCloak => HasSpecialItem("invisibility_cloak");
        public bool HasUltimateFusion => HasSpecialItem("black_fire_onyx");
        
        void Start()
        {
            InitializeSpecialItemManager();
        }
        
        void Update()
        {
            UpdateInvisibilityEffect();
        }
        
        /// <summary>
        /// 特殊アイテムマネージャーの初期化
        /// </summary>
        private void InitializeSpecialItemManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                itemManager = gameManager.GetComponent<ItemManager>();
                combatManager = gameManager.GetComponent<CombatManager>();
                dungeonManager = gameManager.BlackOnyxDungeonManager;
                fireCrystalSystem = gameManager.GetComponent<FireCrystalSystem>();
                uiController = FindObjectOfType<GameUIController>();
            }
            
            // Initialize special item states
            InitializeSpecialItemStates();
            
            // Subscribe to item events
            if (itemManager != null)
            {
                itemManager.OnItemUsed += OnItemUsed;
                itemManager.OnItemAdded += OnItemAdded;
            }
            
            Debug.Log("✨ Special Item Manager initialized");
        }
        
        /// <summary>
        /// 特殊アイテム状態の初期化
        /// </summary>
        private void InitializeSpecialItemStates()
        {
            // 透明マント
            specialItemStates["invisibility_cloak"] = new SpecialItemState
            {
                itemId = "invisibility_cloak",
                isObtained = false,
                isEquipped = false,
                specialProperties = new Dictionary<string, object>
                {
                    {"detection_reduction", 0.8f},
                    {"invisibility_duration", 300f}
                }
            };
            
            // ブラックオニキス
            specialItemStates["black_onyx"] = new SpecialItemState
            {
                itemId = "black_onyx",
                isObtained = false,
                isEquipped = false,
                specialProperties = new Dictionary<string, object>
                {
                    {"win_condition", true},
                    {"ultimate_power", true}
                }
            };
            
            // ファイヤークリスタル
            specialItemStates["fire_crystal"] = new SpecialItemState
            {
                itemId = "fire_crystal",
                isObtained = false,
                isEquipped = false,
                specialProperties = new Dictionary<string, object>
                {
                    {"cross_game_power", true},
                    {"fire_abilities", true}
                }
            };
            
            // 黒炎オニキス（究極合成）
            specialItemStates["black_fire_onyx"] = new SpecialItemState
            {
                itemId = "black_fire_onyx",
                isObtained = false,
                isEquipped = false,
                specialProperties = new Dictionary<string, object>
                {
                    {"ultimate_fusion", true},
                    {"ultimate_ending", true}
                }
            };
        }
        
        /// <summary>
        /// アイテム追加イベント
        /// </summary>
        private void OnItemAdded(ItemData itemData, int quantity)
        {
            if (IsSpecialItem(itemData))
            {
                HandleSpecialItemObtained(itemData);
            }
        }
        
        /// <summary>
        /// アイテム使用イベント
        /// </summary>
        private void OnItemUsed(ItemData itemData)
        {
            if (IsSpecialItem(itemData))
            {
                HandleSpecialItemUsed(itemData);
            }
        }
        
        /// <summary>
        /// 特殊アイテムかどうかをチェック
        /// </summary>
        private bool IsSpecialItem(ItemData itemData)
        {
            return itemData.isInvisibilityCloak || 
                   itemData.isBlackOnyx || 
                   itemData.isFireCrystal || 
                   itemData.isBlackFireOnyx ||
                   itemData.itemType == ItemType.Special;
        }
        
        /// <summary>
        /// 特殊アイテム取得処理
        /// </summary>
        private void HandleSpecialItemObtained(ItemData itemData)
        {
            string itemId = GetSpecialItemId(itemData);
            if (!string.IsNullOrEmpty(itemId) && specialItemStates.ContainsKey(itemId))
            {
                specialItemStates[itemId].isObtained = true;
                OnSpecialItemObtained?.Invoke(itemId);
                
                // 特殊メッセージ
                if (uiController != null)
                {
                    string message = GetSpecialItemObtainedMessage(itemData);
                    uiController.AddMessage(message);
                }
                
                // 特殊処理
                HandleSpecialItemSpecialEffects(itemData, "obtained");
                
                if (debugMode)
                {
                    Debug.Log($"✨ Special item obtained: {itemData.itemName}");
                }
            }
        }
        
        /// <summary>
        /// 特殊アイテム使用処理
        /// </summary>
        private void HandleSpecialItemUsed(ItemData itemData)
        {
            string itemId = GetSpecialItemId(itemData);
            if (!string.IsNullOrEmpty(itemId))
            {
                OnSpecialItemUsed?.Invoke(itemId);
                
                // 特殊効果実行
                ExecuteSpecialItemEffect(itemData);
                
                if (debugMode)
                {
                    Debug.Log($"✨ Special item used: {itemData.itemName}");
                }
            }
        }
        
        /// <summary>
        /// 特殊アイテムIDの取得
        /// </summary>
        private string GetSpecialItemId(ItemData itemData)
        {
            if (itemData.isInvisibilityCloak) return "invisibility_cloak";
            if (itemData.isBlackOnyx) return "black_onyx";
            if (itemData.isFireCrystal) return "fire_crystal";
            if (itemData.isBlackFireOnyx) return "black_fire_onyx";
            return itemData.itemId;
        }
        
        /// <summary>
        /// 特殊アイテム取得メッセージ
        /// </summary>
        private string GetSpecialItemObtainedMessage(ItemData itemData)
        {
            if (itemData.isInvisibilityCloak)
                return "👻 伝説の透明マント！ハイドからの貴重な贈り物だ";
            if (itemData.isBlackOnyx)
                return "💎 ついにブラックオニキスを発見！伝説の宝石が手の中に...";
            if (itemData.isFireCrystal)
                return "🔥 ファイヤークリスタル！炎の力が宿る神秘の結晶";
            if (itemData.isBlackFireOnyx)
                return "⚫🔥 黒炎オニキス！究極の合成が完了した！";
            
            return $"✨ {itemData.itemName}を手に入れた！";
        }
        
        /// <summary>
        /// 特殊アイテム効果実行
        /// </summary>
        private void ExecuteSpecialItemEffect(ItemData itemData)
        {
            if (itemData.isInvisibilityCloak)
            {
                ActivateInvisibility();
            }
            else if (itemData.isBlackOnyx)
            {
                TriggerNormalEnding();
            }
            else if (itemData.isFireCrystal)
            {
                ActivateFireCrystalPower();
            }
            else if (itemData.isBlackFireOnyx)
            {
                TriggerUltimateEnding();
            }
        }
        
        /// <summary>
        /// 透明化効果の発動
        /// </summary>
        private void ActivateInvisibility()
        {
            isInvisible = true;
            invisibilityTimeRemaining = invisibilityDuration;
            
            OnInvisibilityActivated?.Invoke();
            
            if (uiController != null)
            {
                uiController.AddMessage("👻 透明マントの力で姿が見えなくなった！");
                uiController.AddMessage($"⏰ 効果時間: {invisibilityDuration}秒");
            }
        }
        
        /// <summary>
        /// 透明化効果の更新
        /// </summary>
        private void UpdateInvisibilityEffect()
        {
            if (isInvisible)
            {
                invisibilityTimeRemaining -= Time.deltaTime;
                
                if (invisibilityTimeRemaining <= 0f)
                {
                    DeactivateInvisibility();
                }
            }
        }
        
        /// <summary>
        /// 透明化効果の解除
        /// </summary>
        private void DeactivateInvisibility()
        {
            isInvisible = false;
            invisibilityTimeRemaining = 0f;
            
            OnInvisibilityDeactivated?.Invoke();
            
            if (uiController != null)
            {
                uiController.AddMessage("👻 透明マントの効果が切れた");
            }
        }
        
        /// <summary>
        /// ノーマルエンディングのトリガー
        /// </summary>
        private void TriggerNormalEnding()
        {
            if (uiController != null)
            {
                uiController.AddMessage("🏆 ブラックオニキスを手に入れた！");
                uiController.AddMessage("🎉 おめでとう！ノーマルエンディング達成！");
            }
            
            // ゲーム勝利状態に設定
            OnGameWon?.Invoke();
            
            // エンディングシーケンス開始
            StartCoroutine(ShowNormalEnding());
        }
        
        /// <summary>
        /// ファイヤークリスタルの力発動
        /// </summary>
        private void ActivateFireCrystalPower()
        {
            if (fireCrystalSystem != null)
            {
                fireCrystalSystem.ObtainFireCrystal();
            }
            
            if (uiController != null)
            {
                uiController.AddMessage("🔥 ファイヤークリスタルの力が覚醒した！");
                uiController.AddMessage("🔗 クロスゲーム連動機能がアンロック！");
            }
        }
        
        /// <summary>
        /// 究極エンディングのトリガー
        /// </summary>
        private void TriggerUltimateEnding()
        {
            if (uiController != null)
            {
                uiController.AddMessage("⚫🔥 黒炎オニキス！究極の力が宿った！");
                uiController.AddMessage("🌟 伝説の究極エンディング達成！");
            }
            
            // ゲーム勝利状態に設定
            OnGameWon?.Invoke();
            
            // 究極エンディングシーケンス開始
            StartCoroutine(ShowUltimateEnding());
        }
        
        /// <summary>
        /// 究極合成の実行
        /// </summary>
        public bool AttemptUltimateFusion()
        {
            if (HasBlackOnyx && HasFireCrystal)
            {
                if (uiController != null)
                {
                    uiController.AddMessage("✨ ブラックオニキスとファイヤークリスタルが共鳴している...");
                    uiController.AddMessage("🔥 究極合成を開始！");
                }
                
                // 黒炎オニキス生成
                CreateBlackFireOnyx();
                return true;
            }
            else
            {
                if (uiController != null)
                {
                    uiController.AddMessage("❌ 究極合成には ブラックオニキス と ファイヤークリスタル が必要");
                }
                return false;
            }
        }
        
        /// <summary>
        /// 黒炎オニキスの生成
        /// </summary>
        private void CreateBlackFireOnyx()
        {
            // アイテムマネージャーに黒炎オニキスを追加
            if (itemManager != null)
            {
                // 黒炎オニキスのデータ作成
                var blackFireOnyxData = CreateBlackFireOnyxData();
                itemManager.AddToInventory(blackFireOnyxData, 1);
            }
        }
        
        /// <summary>
        /// 黒炎オニキスデータの作成
        /// </summary>
        private ItemData CreateBlackFireOnyxData()
        {
            var blackFireOnyx = ScriptableObject.CreateInstance<ItemData>();
            blackFireOnyx.itemId = "black_fire_onyx";
            blackFireOnyx.itemName = "黒炎オニキス";
            blackFireOnyx.description = "ブラックオニキスとファイヤークリスタルの究極合成。全ての力を秘めた伝説の宝石";
            blackFireOnyx.displayChar = '⚫';
            blackFireOnyx.displaySymbol = "⚫🔥";
            blackFireOnyx.itemType = ItemType.Special;
            blackFireOnyx.rarity = ItemRarity.Legendary;
            blackFireOnyx.value = 10000;
            blackFireOnyx.maxStackSize = 1;
            blackFireOnyx.canDrop = false;
            blackFireOnyx.canSell = false;
            blackFireOnyx.isBlackFireOnyx = true;
            blackFireOnyx.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.UltimateTransform, value = 1, description = "究極変身" }
            };
            
            return blackFireOnyx;
        }
        
        /// <summary>
        /// 特殊アイテム所持チェック
        /// </summary>
        public bool HasSpecialItem(string itemId)
        {
            return specialItemStates.ContainsKey(itemId) && specialItemStates[itemId].isObtained;
        }
        
        /// <summary>
        /// 敵の発見率減少効果
        /// </summary>
        public float GetEnemyDetectionReduction()
        {
            float reduction = 0f;
            
            if (isInvisible)
            {
                reduction += 0.9f; // 透明化中は90%減少
            }
            else if (HasInvisibilityCloak)
            {
                reduction += enemyDetectionReduction; // 透明マント装備時は設定値分減少
            }
            
            return Mathf.Clamp01(reduction);
        }
        
        /// <summary>
        /// 特殊アイテム特殊効果処理
        /// </summary>
        private void HandleSpecialItemSpecialEffects(ItemData itemData, string eventType)
        {
            // イベントに応じた特殊処理
            if (eventType == "obtained")
            {
                // 取得時の特殊処理
                if (itemData.isBlackOnyx && HasFireCrystal)
                {
                    if (uiController != null)
                    {
                        uiController.AddMessage("✨ ブラックオニキスとファイヤークリスタル... 何かが起こりそうだ");
                    }
                }
            }
        }
        
        /// <summary>
        /// ノーマルエンディング表示
        /// </summary>
        private System.Collections.IEnumerator ShowNormalEnding()
        {
            yield return new WaitForSeconds(2f);
            
            if (gameManager != null)
            {
                gameManager.ShowEnding("normal");
            }
        }
        
        /// <summary>
        /// 究極エンディング表示
        /// </summary>
        private System.Collections.IEnumerator ShowUltimateEnding()
        {
            yield return new WaitForSeconds(2f);
            
            if (gameManager != null)
            {
                gameManager.ShowEnding("ultimate");
            }
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Special Item Manager:");
            sb.AppendLine($"Invisibility: {(isInvisible ? $"ACTIVE ({invisibilityTimeRemaining:F1}s)" : "OFF")}");
            sb.AppendLine($"Has Black Onyx: {HasBlackOnyx}");
            sb.AppendLine($"Has Fire Crystal: {HasFireCrystal}");
            sb.AppendLine($"Has Invisibility Cloak: {HasInvisibilityCloak}");
            sb.AppendLine($"Has Ultimate Fusion: {HasUltimateFusion}");
            sb.AppendLine($"Enemy Detection Reduction: {GetEnemyDetectionReduction():P0}");
            
            return sb.ToString();
        }
        
        void OnDestroy()
        {
            // イベント購読解除
            if (itemManager != null)
            {
                itemManager.OnItemUsed -= OnItemUsed;
                itemManager.OnItemAdded -= OnItemAdded;
            }
        }
    }
    
    /// <summary>
    /// 特殊アイテム状態
    /// </summary>
    [System.Serializable]
    public class SpecialItemState
    {
        public string itemId;
        public bool isObtained;
        public bool isEquipped;
        public Dictionary<string, object> specialProperties = new Dictionary<string, object>();
    }
}