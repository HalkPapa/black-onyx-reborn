using UnityEngine;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ブラックオニキス復刻版アイテムデータの定義
    /// 1984年オリジナル準拠の特殊アイテム対応
    /// </summary>
    [CreateAssetMenu(fileName = "New Item Data", menuName = "Black Onyx/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemId = "item_001";
        public string itemName = "Potion";
        public string description = "A healing potion";
        public char displayChar = 'p';
        public string displaySymbol = "🧪";
        
        [Header("Item Properties")]
        public ItemType itemType = ItemType.Consumable;
        public ItemRarity rarity = ItemRarity.Common;
        public int value = 10;
        public int maxStackSize = 10;
        public bool canDrop = true;
        public bool canSell = true;
        
        [Header("Effects")]
        public ItemEffect[] effects;
        
        [Header("Equipment Stats (if applicable)")]
        public int attackBonus = 0;
        public int defenseBonus = 0;
        public int healthBonus = 0;
        public float speedMultiplier = 1f;
        
        [Header("Black Onyx Special Properties")]
        public bool isInvisibilityCloak = false; // 透明マント
        public bool isBlackOnyx = false; // ブラックオニキス
        public bool isFireCrystal = false; // ファイヤークリスタル
        public bool isBlackFireOnyx = false; // 黒烎オニキス（究極合成）
        public float enemyDetectionReduction = 0f; // 敵の発見率減少
        public bool grantsInvisibility = false; // 透明化能力
        public int teleportPower = 0; // テレポート能力
        
        [Header("Usage Requirements")]
        public int minLevel = 1;
        public ItemType[] requiredItemsToUse;
        
        [Header("Spawn Settings")]
        public float spawnWeight = 1f;
        public int minFloor = 1;
        public int maxFloor = 10;
        
        /// <summary>
        /// アイテムの使用可能性チェック
        /// </summary>
        public bool CanUse(PlayerStats playerStats)
        {
            return playerStats.level >= minLevel;
        }
        
        /// <summary>
        /// このフロアにスポーン可能かチェック
        /// </summary>
        public bool CanSpawnOnFloor(int floor)
        {
            return floor >= minFloor && floor <= maxFloor;
        }
        
        /// <summary>
        /// レアリティに基づく色を取得
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.white;
                case ItemRarity.Uncommon: return Color.green;
                case ItemRarity.Rare: return Color.blue;
                case ItemRarity.Epic: return Color.magenta;
                case ItemRarity.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }
    }
    
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    public enum ItemType
    {
        Consumable,     // 消耗品（ポーション等）
        Weapon,         // 武器
        Armor,          // 防具
        Accessory,      // アクセサリー
        Equipment,      // 装備品（ブラックオニキス特殊装備）
        Key,            // 鍵アイテム
        Treasure,       // 宝物
        Material,       // 素材
        Quest,          // クエストアイテム
        Special         // 特殊アイテム（ブラックオニキス等）
    }
    
    /// <summary>
    /// アイテムレアリティ
    /// </summary>
    public enum ItemRarity
    {
        Common,         // 一般的
        Uncommon,       // 珍しい
        Rare,           // レア
        Epic,           // エピック
        Legendary       // 伝説的
    }
    
    /// <summary>
    /// アイテム効果定義
    /// </summary>
    [System.Serializable]
    public class ItemEffect
    {
        public ItemEffectType effectType;
        public int value;
        public float duration = 0f; // 0 = instant effect
        public string description;
        
        /// <summary>
        /// 効果を適用
        /// </summary>
        public void ApplyEffect(PlayerStats playerStats, CombatManager combatManager)
        {
            switch (effectType)
            {
                case ItemEffectType.HealHP:
                    combatManager.HealPlayer(value);
                    break;
                    
                case ItemEffectType.RestoreFullHP:
                    combatManager.FullHealPlayer();
                    break;
                    
                case ItemEffectType.IncreaseAttack:
                    playerStats.attack += value;
                    break;
                    
                case ItemEffectType.IncreaseDefense:
                    playerStats.defense += value;
                    break;
                    
                case ItemEffectType.IncreaseMaxHP:
                    playerStats.maxHealth += value;
                    break;
                    
                case ItemEffectType.GainExperience:
                    // Experience gain would be handled by CombatManager
                    break;
                    
                case ItemEffectType.GainGold:
                    playerStats.gold += value;
                    break;
                    
                // ブラックオニキス特殊効果
                case ItemEffectType.GrantInvisibility:
                    // 透明化効果の実装は別システムで処理
                    break;
                    
                case ItemEffectType.FloorTeleport:
                    // フロアテレポートの実装は別システムで処理
                    break;
                    
                case ItemEffectType.WinGame:
                    // ゲームクリアの実装は別システムで処理
                    break;
                    
                case ItemEffectType.FireCrystalPower:
                    // ファイヤークリスタルの力は別システムで処理
                    break;
                    
                case ItemEffectType.UltimateTransform:
                    // 究極変身は別システムで処理
                    break;
                    
                case ItemEffectType.ReduceEnemyDetection:
                    // 敵発見率減少は装備効果で処理
                    break;
            }
        }
    }
    
    /// <summary>
    /// アイテム効果タイプ
    /// </summary>
    public enum ItemEffectType
    {
        HealHP,             // HP回復
        RestoreFullHP,      // HP全回復
        IncreaseAttack,     // 攻撃力上昇
        IncreaseDefense,    // 防御力上昇
        IncreaseMaxHP,      // 最大HP上昇
        GainExperience,     // 経験値獲得
        GainGold,           // ゴールド獲得
        TemporaryBoost,     // 一時的な能力向上
        Teleport,           // テレポート
        Poison,             // 毒
        Antidote,           // 解毒
        // ブラックオニキス特殊効果
        GrantInvisibility,  // 透明化付与
        FloorTeleport,      // フロアテレポート
        WinGame,            // ゲームクリア
        FireCrystalPower,   // ファイヤークリスタルの力
        UltimateTransform,  // 究極変身（黒烎オニキス）
        ReduceEnemyDetection // 敵の発見率減少
    }
}