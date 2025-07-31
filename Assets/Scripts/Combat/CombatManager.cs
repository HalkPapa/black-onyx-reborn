using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 戦闘システムの管理を行うマネージャー
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float combatAnimationDuration = 0.5f;
        [SerializeField] private float combatMessageDelay = 1f;
        [SerializeField] private bool autoResolveCombat = true;
        [SerializeField] private int basePlayerAttack = 6; // オリジナル準拠：初期攻撃力低下
        [SerializeField] private int basePlayerDefense = 2; // オリジナル準拠：初期防御力低下
        
        [Header("Experience Settings - Black Onyx Original Balance")]
        [SerializeField] private int baseExperienceRequired = 300; // オリジナル準拠：厳しく
        [SerializeField] private float experienceMultiplier = 2.2f; // オリジナル準拠：急激な上昇
        
        [Header("Audio")]
        [SerializeField] private bool playAttackSounds = true;
        [SerializeField] private bool playHitSounds = true;
        
        // Combat state
        private bool inCombat = false;
        private Enemy currentEnemy;
        private Queue<CombatAction> combatQueue = new Queue<CombatAction>();
        private Coroutine combatCoroutine;
        
        // Player stats (simplified)
        private PlayerStats playerStats = new PlayerStats();
        
        // Manager references
        private GameManager gameManager;
        private AudioManager audioManager;
        private DungeonManager dungeonManager;
        private EnemyManager enemyManager;
        private GameUIController uiController;
        
        // Events
        public System.Action<Enemy> OnCombatStarted;
        public System.Action<Enemy, bool> OnCombatEnded; // bool = player won
        public System.Action<int> OnPlayerLevelUp;
        public System.Action<int, int> OnPlayerDamaged; // damage, current hp
        public System.Action<Enemy, int> OnEnemyDamaged; // enemy, damage
        
        // Properties
        public bool InCombat => inCombat;
        public PlayerStats PlayerStats => playerStats;
        
        void Start()
        {
            InitializeCombatManager();
        }
        
        /// <summary>
        /// 戦闘マネージャーの初期化
        /// </summary>
        private void InitializeCombatManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                audioManager = gameManager.AudioManager;
                dungeonManager = gameManager.DungeonManager;
                enemyManager = gameManager.GetComponent<EnemyManager>();
                uiController = FindObjectOfType<GameUIController>();
            }
            
            // Initialize player stats
            InitializePlayerStats();
            
            Debug.Log("⚔️ Combat Manager initialized");
        }
        
        /// <summary>
        /// プレイヤーステータスの初期化（オリジナル ブラックオニキス準拠）
        /// </summary>
        private void InitializePlayerStats()
        {
            playerStats.level = 1;
            playerStats.maxHealth = 50; // オリジナル準拠：初期HP低く
            playerStats.currentHealth = playerStats.maxHealth;
            playerStats.attack = 6; // オリジナル準拠：初期攻撃力低く
            playerStats.defense = 2; // オリジナル準拠：初期防御力低く
            playerStats.experience = 0;
            playerStats.experienceToNext = baseExperienceRequired;
            playerStats.gold = 0;
            
            if (gameManager.debugMode)
            {
                Debug.Log($"⚔️ Player stats initialized (Black Onyx Original): HP {playerStats.currentHealth}/{playerStats.maxHealth}, ATK {playerStats.attack}, DEF {playerStats.defense}");
                Debug.Log($"📊 Level {playerStats.level} → EXP required: {playerStats.experienceToNext} (Harsh Balance)");
            }
        }
        
        /// <summary>
        /// 戦闘の開始
        /// </summary>
        public void InitiateCombat(Enemy enemy)
        {
            if (inCombat || enemy == null || !enemy.IsAlive)
                return;
            
            currentEnemy = enemy;
            inCombat = true;
            
            // 戦闘開始イベント
            OnCombatStarted?.Invoke(enemy);
            
            // UI message
            if (uiController != null)
            {
                uiController.AddMessage($"⚔️ {enemy.Data.enemyName} との戦闘開始！");
            }
            
            // BGM change to battle
            if (audioManager != null)
            {
                audioManager.PlayBGM("battle");
            }
            
            // Start combat sequence
            if (autoResolveCombat)
            {
                combatCoroutine = StartCoroutine(AutoResolveCombat());
            }
            else
            {
                // Manual combat (turn-based) - for future implementation
                StartManualCombat();
            }
            
            Debug.Log($"⚔️ Combat initiated with {enemy.Data.enemyName}");
        }
        
        /// <summary>
        /// 自動戦闘解決
        /// </summary>
        private IEnumerator AutoResolveCombat()
        {
            yield return new WaitForSeconds(combatAnimationDuration);
            
            while (inCombat && currentEnemy != null && currentEnemy.IsAlive && playerStats.IsAlive)
            {
                // Player attacks first
                yield return StartCoroutine(ExecutePlayerAttack());
                
                if (!currentEnemy.IsAlive)
                    break;
                
                yield return new WaitForSeconds(combatMessageDelay);
                
                // Enemy counter-attacks
                yield return StartCoroutine(ExecuteEnemyAttack());
                
                if (!playerStats.IsAlive)
                    break;
                
                yield return new WaitForSeconds(combatMessageDelay);
            }
            
            // Combat resolution
            EndCombat();
        }
        
        /// <summary>
        /// プレイヤー攻撃の実行
        /// </summary>
        private IEnumerator ExecutePlayerAttack()
        {
            if (currentEnemy == null || !currentEnemy.IsAlive)
                yield break;
            
            // Calculate damage
            int baseDamage = Random.Range(playerStats.attack - 2, playerStats.attack + 3);
            int actualDamage = currentEnemy.TakeDamage(baseDamage);
            
            // Play attack sound
            if (playAttackSounds && audioManager != null)
            {
                audioManager.PlaySE("attack");
            }
            
            // UI message
            if (uiController != null)
            {
                uiController.AddMessage($"⚔️ あなたの攻撃！ {currentEnemy.Data.enemyName} に {actualDamage} のダメージ！");
                
                if (!currentEnemy.IsAlive)
                {
                    uiController.AddMessage($"💀 {currentEnemy.Data.enemyName} を倒した！");
                }
            }
            
            // Damage event
            OnEnemyDamaged?.Invoke(currentEnemy, actualDamage);
            
            yield return new WaitForSeconds(combatAnimationDuration);
        }
        
        /// <summary>
        /// 敵攻撃の実行
        /// </summary>
        private IEnumerator ExecuteEnemyAttack()
        {
            if (currentEnemy == null || !currentEnemy.IsAlive)
                yield break;
            
            // Calculate damage
            int baseDamage = Random.Range(currentEnemy.Stats.attack - 1, currentEnemy.Stats.attack + 2);
            int actualDamage = TakePlayerDamage(baseDamage);
            
            // Play hit sound
            if (playHitSounds && audioManager != null)
            {
                audioManager.PlaySE("hit");
            }
            
            // UI message
            if (uiController != null)
            {
                uiController.AddMessage($"🔥 {currentEnemy.Data.enemyName} の攻撃！ あなたは {actualDamage} のダメージを受けた！");
                
                if (!playerStats.IsAlive)
                {
                    uiController.AddMessage("💀 あなたは倒れた...");
                }
            }
            
            // Damage event
            OnPlayerDamaged?.Invoke(actualDamage, playerStats.currentHealth);
            
            yield return new WaitForSeconds(combatAnimationDuration);
        }
        
        /// <summary>
        /// プレイヤーダメージ処理
        /// </summary>
        private int TakePlayerDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - playerStats.defense);
            playerStats.currentHealth -= actualDamage;
            
            if (playerStats.currentHealth < 0)
                playerStats.currentHealth = 0;
            
            return actualDamage;
        }
        
        /// <summary>
        /// 戦闘終了処理
        /// </summary>
        private void EndCombat()
        {
            bool playerWon = playerStats.IsAlive;
            
            if (playerWon && currentEnemy != null)
            {
                // Experience and gold rewards
                GainExperience(currentEnemy.Stats.experience);
                GainGold(currentEnemy.Stats.goldDrop);
                
                if (uiController != null)
                {
                    uiController.AddMessage($"✨ {currentEnemy.Stats.experience} 経験値と {currentEnemy.Stats.goldDrop} ゴールドを獲得！");
                }
            }
            else if (!playerWon)
            {
                // Player died - trigger game over
                if (gameManager != null)
                {
                    gameManager.ChangeState(GameManager.GameState.GameOver);
                }
            }
            
            // Combat end event
            OnCombatEnded?.Invoke(currentEnemy, playerWon);
            
            // Reset combat state
            inCombat = false;
            currentEnemy = null;
            
            if (combatCoroutine != null)
            {
                StopCoroutine(combatCoroutine);
                combatCoroutine = null;
            }
            
            // Restore dungeon BGM
            if (audioManager != null && playerWon)
            {
                audioManager.PlayBGM("dungeon");
            }
            
            Debug.Log($"⚔️ Combat ended. Player won: {playerWon}");
        }
        
        /// <summary>
        /// 経験値獲得
        /// </summary>
        private void GainExperience(int exp)
        {
            playerStats.experience += exp;
            
            // Level up check
            while (playerStats.experience >= playerStats.experienceToNext)
            {
                LevelUp();
            }
        }
        
        /// <summary>
        /// レベルアップ処理（オリジナル ブラックオニキス準拠）
        /// </summary>
        private void LevelUp()
        {
            playerStats.experience -= playerStats.experienceToNext;
            playerStats.level++;
            
            // オリジナル準拠：控えめなステータス上昇
            int healthIncrease = Random.Range(4, 8); // 厳しく：HP上昇量を半減
            int attackIncrease = Random.Range(1, 3); // 厳しく：攻撃力上昇量を半減
            int defenseIncrease = Random.Range(0, 2); // 厳しく：防御力上昇量を半減、時々上がらない
            
            playerStats.maxHealth += healthIncrease;
            
            // オリジナル準拠：レベルアップ時の回復量を制限
            int healAmount = Mathf.RoundToInt(healthIncrease * 0.8f); // 80%回復のみ
            playerStats.currentHealth = Mathf.Min(playerStats.maxHealth, playerStats.currentHealth + healAmount);
            playerStats.attack += attackIncrease;
            playerStats.defense += defenseIncrease;
            
            // オリジナル準拠：次レベルまでの経験値要求量が急激に増加
            playerStats.experienceToNext = Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceMultiplier, playerStats.level - 1));
            
            // UI message - オリジナル準拠のメッセージ
            if (uiController != null)
            {
                uiController.AddMessage($"🎉 レベルアップ！ Lv.{playerStats.level}");
                uiController.AddMessage($"📈 HP+{healthIncrease}, ATK+{attackIncrease}, DEF+{defenseIncrease}");
                
                // 厳しいバランスの警告メッセージ
                if (playerStats.level <= 3)
                {
                    uiController.AddMessage($"⚠️ まだまだ弱い... 強敵に注意！");
                }
                else if (playerStats.level >= 9)
                {
                    uiController.AddMessage($"💪 かなり強くなった！でも巨人は危険...");
                }
            }
            
            // Level up event
            OnPlayerLevelUp?.Invoke(playerStats.level);
            
            // Play level up sound
            if (audioManager != null)
            {
                audioManager.PlaySE("levelup");
            }
            
            Debug.Log($"🎉 Player leveled up to {playerStats.level}! (Black Onyx Original Balance)");
            Debug.Log($"📈 Next level EXP required: {playerStats.experienceToNext} (Harsh requirement)");
        }
        
        /// <summary>
        /// ゴールド獲得
        /// </summary>
        private void GainGold(int gold)
        {
            playerStats.gold += gold;
        }
        
        /// <summary>
        /// 手動戦闘開始（将来の実装用）
        /// </summary>
        private void StartManualCombat()
        {
            // Turn-based combat UI would be implemented here
            Debug.Log("⚔️ Manual combat not yet implemented - falling back to auto combat");
            combatCoroutine = StartCoroutine(AutoResolveCombat());
        }
        
        /// <summary>
        /// プレイヤーの回復
        /// </summary>
        public void HealPlayer(int amount)
        {
            int actualHeal = Mathf.Min(amount, playerStats.maxHealth - playerStats.currentHealth);
            playerStats.currentHealth += actualHeal;
            
            if (uiController != null && actualHeal > 0)
            {
                uiController.AddMessage($"💚 {actualHeal} HP回復！");
            }
        }
        
        /// <summary>
        /// プレイヤーの完全回復
        /// </summary>
        public void FullHealPlayer()
        {
            int healAmount = playerStats.maxHealth - playerStats.currentHealth;
            if (healAmount > 0)
            {
                playerStats.currentHealth = playerStats.maxHealth;
                
                if (uiController != null)
                {
                    uiController.AddMessage($"✨ 完全回復！ HP: {playerStats.currentHealth}/{playerStats.maxHealth}");
                }
            }
        }
        
        /// <summary>
        /// 戦闘を強制終了
        /// </summary>
        public void ForceCombatEnd()
        {
            if (inCombat)
            {
                if (combatCoroutine != null)
                {
                    StopCoroutine(combatCoroutine);
                }
                
                EndCombat();
            }
        }
        
        /// <summary>
        /// プレイヤーステータスのリセット
        /// </summary>
        public void ResetPlayerStats()
        {
            InitializePlayerStats();
            
            if (uiController != null)
            {
                uiController.AddMessage("🔄 プレイヤーステータスをリセットしました");
            }
        }
        
        /// <summary>
        /// セーブデータからステータス読み込み
        /// </summary>
        public void LoadPlayerStats(PlayerStats savedStats)
        {
            playerStats = new PlayerStats
            {
                level = savedStats.level,
                maxHealth = savedStats.maxHealth,
                currentHealth = savedStats.currentHealth,
                attack = savedStats.attack,
                defense = savedStats.defense,
                experience = savedStats.experience,
                experienceToNext = savedStats.experienceToNext,
                gold = savedStats.gold
            };
            
            Debug.Log($"⚔️ Player stats loaded: Lv.{playerStats.level} HP:{playerStats.currentHealth}/{playerStats.maxHealth}");
        }
        
        /// <summary>
        /// セーブ用ステータス取得
        /// </summary>
        public PlayerStats GetSaveablePlayerStats()
        {
            return new PlayerStats
            {
                level = playerStats.level,
                maxHealth = playerStats.maxHealth,
                currentHealth = playerStats.currentHealth,
                attack = playerStats.attack,
                defense = playerStats.defense,
                experience = playerStats.experience,
                experienceToNext = playerStats.experienceToNext,
                gold = playerStats.gold
            };
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Combat Manager - In Combat: {inCombat}\n" +
                   $"Player: Lv.{playerStats.level} HP:{playerStats.currentHealth}/{playerStats.maxHealth} ATK:{playerStats.attack} DEF:{playerStats.defense}\n" +
                   $"EXP: {playerStats.experience}/{playerStats.experienceToNext} Gold: {playerStats.gold}";
        }
    }
    
    /// <summary>
    /// プレイヤーステータス
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        public int level = 1;
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int attack = 10;
        public int defense = 5;
        public int experience = 0;
        public int experienceToNext = 100;
        public int gold = 0;
        
        public bool IsAlive => currentHealth > 0;
        public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        public float ExperiencePercentage => experienceToNext > 0 ? (float)experience / experienceToNext : 0f;
    }
    
    /// <summary>
    /// 戦闘アクション
    /// </summary>
    public enum CombatActionType
    {
        PlayerAttack,
        EnemyAttack,
        PlayerSkill,
        ItemUse,
        Flee
    }
    
    /// <summary>
    /// 戦闘アクション情報
    /// </summary>
    public class CombatAction
    {
        public CombatActionType actionType;
        public object actionData;
        public float delay;
        
        public CombatAction(CombatActionType type, object data = null, float actionDelay = 0f)
        {
            actionType = type;
            actionData = data;
            delay = actionDelay;
        }
    }
}