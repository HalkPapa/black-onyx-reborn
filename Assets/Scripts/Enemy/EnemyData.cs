using UnityEngine;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 敵キャラクターのデータ定義
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "Black Onyx/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName = "Goblin";
        public string description = "A small, vicious creature";
        public char displayChar = 'g';
        public string displaySymbol = "👹";
        
        [Header("Stats")]
        public int maxHealth = 30;
        public int attack = 8;
        public int defense = 3;
        public int speed = 5;
        public int experience = 10;
        public int goldDrop = 5;
        
        [Header("AI Behavior")]
        public EnemyAIType aiType = EnemyAIType.Aggressive;
        public float detectionRange = 3f;
        public float moveChance = 0.7f;
        public float attackChance = 0.8f;
        public bool canMoveThrough Walls = false;
        
        [Header("Spawning")]
        public int minFloor = 1;
        public int maxFloor = 5;
        public float spawnWeight = 1f;
        public int maxPerFloor = 3;
        
        [Header("Special Abilities")]
        public bool canTeleport = false;
        public bool canSummon = false;
        public bool poisonous = false;
        public bool regenerates = false;
        public float regenerationRate = 0.1f;
        
        [Header("Loot Table")]
        public ItemDrop[] possibleDrops;
        
        /// <summary>
        /// フロアに適したステータスに調整
        /// </summary>
        public EnemyStats GetStatsForFloor(int floor)
        {
            float multiplier = 1f + (floor - 1) * 0.2f; // フロアごとに20%強化
            
            return new EnemyStats
            {
                maxHealth = Mathf.RoundToInt(maxHealth * multiplier),
                currentHealth = Mathf.RoundToInt(maxHealth * multiplier),
                attack = Mathf.RoundToInt(attack * multiplier),
                defense = Mathf.RoundToInt(defense * multiplier),
                speed = speed,
                experience = Mathf.RoundToInt(experience * multiplier),
                goldDrop = Mathf.RoundToInt(goldDrop * multiplier)
            };
        }
        
        /// <summary>
        /// このフロアにスポーン可能かチェック
        /// </summary>
        public bool CanSpawnOnFloor(int floor)
        {
            return floor >= minFloor && floor <= maxFloor;
        }
    }
    
    /// <summary>
    /// 敵のAIタイプ
    /// </summary>
    public enum EnemyAIType
    {
        Passive,        // おとなしい（攻撃されるまで動かない）
        Aggressive,     // 積極的（プレイヤーを見つけると追いかける）
        Patrol,         // 巡回（決まったルートを歩く）
        Guard,          // 守備（特定エリアを守る）
        Coward,         // 臆病（プレイヤーから逃げる）
        Random          // ランダム（予測不可能な動き）
    }
    
    /// <summary>
    /// 敵の実際のステータス（ゲーム中で変動する）
    /// </summary>
    [System.Serializable]
    public class EnemyStats
    {
        public int maxHealth;
        public int currentHealth;
        public int attack;
        public int defense;
        public int speed;
        public int experience;
        public int goldDrop;
        
        public bool IsAlive => currentHealth > 0;
        public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }
    
    /// <summary>
    /// アイテムドロップ定義
    /// </summary>
    [System.Serializable]
    public class ItemDrop
    {
        public string itemId;
        public float dropChance = 0.1f; // 10%の確率
        public int minQuantity = 1;
        public int maxQuantity = 1;
        
        public bool ShouldDrop()
        {
            return Random.Range(0f, 1f) <= dropChance;
        }
        
        public int GetDropQuantity()
        {
            return Random.Range(minQuantity, maxQuantity + 1);
        }
    }
}