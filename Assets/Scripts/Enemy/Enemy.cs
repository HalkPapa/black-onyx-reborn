using UnityEngine;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 敵キャラクターの基底クラス
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private bool debugMode = false;
        
        // Current state
        private EnemyStats currentStats;
        private Vector2Int position;
        private EnemyState currentState = EnemyState.Idle;
        private Vector2Int lastKnownPlayerPosition = Vector2Int.zero;
        private float lastMoveTime = 0f;
        private float moveInterval = 1f; // 1秒ごとに行動判定
        
        // AI behavior
        private Vector2Int patrolStart;
        private Vector2Int patrolEnd;
        private bool patrolDirection = true;
        private int idleCounter = 0;
        
        // Manager references
        private DungeonManager dungeonManager;
        private GameManager gameManager;
        private EnemyManager enemyManager;
        
        // Events
        public System.Action<Enemy> OnEnemyDeath;
        public System.Action<Enemy, int> OnEnemyDamaged;
        public System.Action<Enemy, Vector2Int> OnEnemyMoved;
        
        // Properties
        public EnemyData Data => enemyData;
        public EnemyStats Stats => currentStats;
        public Vector2Int Position => position;
        public EnemyState State => currentState;
        public bool IsAlive => currentStats != null && currentStats.IsAlive;
        
        void Start()
        {
            InitializeEnemy();
        }
        
        void Update()
        {
            if (!IsAlive || gameManager?.CurrentState != GameManager.GameState.InGame)
                return;
                
            ProcessAI();
        }
        
        /// <summary>
        /// 敵の初期化
        /// </summary>
        public void InitializeEnemy()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                enemyManager = gameManager.GetComponent<EnemyManager>();
            }
            
            // Initialize stats based on current floor
            if (enemyData != null && dungeonManager != null)
            {
                int currentFloor = dungeonManager.GetCurrentFloorNumber();
                currentStats = enemyData.GetStatsForFloor(currentFloor);
                moveInterval = 2f - (currentStats.speed * 0.1f); // 速度に応じて移動間隔調整
                moveInterval = Mathf.Clamp(moveInterval, 0.5f, 3f);
            }
            
            // Set initial position
            if (position == Vector2Int.zero)
            {
                position = GetValidSpawnPosition();
            }
            
            // Initialize patrol points for patrol AI
            if (enemyData.aiType == EnemyAIType.Patrol)
            {
                SetupPatrolRoute();
            }
            
            if (debugMode)
            {
                Debug.Log($"🧌 {enemyData.enemyName} initialized at {position} with {currentStats.currentHealth} HP");
            }
        }
        
        /// <summary>
        /// データから敵を初期化
        /// </summary>
        public void InitializeWithData(EnemyData data, Vector2Int spawnPosition)
        {
            enemyData = data;
            position = spawnPosition;
            InitializeEnemy();
        }
        
        /// <summary>
        /// AI処理のメインループ
        /// </summary>
        private void ProcessAI()
        {
            if (Time.time - lastMoveTime < moveInterval)
                return;
            
            Vector2Int playerPosition = dungeonManager.GetPlayerPosition();
            float distanceToPlayer = Vector2Int.Distance(position, playerPosition);
            
            // プレイヤー検出
            bool canSeePlayer = CanSeePlayer(playerPosition, distanceToPlayer);
            
            if (canSeePlayer)
            {
                lastKnownPlayerPosition = playerPosition;
            }
            
            // AI behavior based on type
            Vector2Int targetDirection = Vector2Int.zero;
            
            switch (enemyData.aiType)
            {
                case EnemyAIType.Passive:
                    targetDirection = ProcessPassiveAI(canSeePlayer, distanceToPlayer);
                    break;
                    
                case EnemyAIType.Aggressive:
                    targetDirection = ProcessAggressiveAI(canSeePlayer, playerPosition, distanceToPlayer);
                    break;
                    
                case EnemyAIType.Patrol:
                    targetDirection = ProcessPatrolAI();
                    break;
                    
                case EnemyAIType.Guard:
                    targetDirection = ProcessGuardAI(canSeePlayer, playerPosition, distanceToPlayer);
                    break;
                    
                case EnemyAIType.Coward:
                    targetDirection = ProcessCowardAI(canSeePlayer, playerPosition);
                    break;
                    
                case EnemyAIType.Random:
                    targetDirection = ProcessRandomAI();
                    break;
            }
            
            // 移動試行
            if (targetDirection != Vector2Int.zero)
            {
                AttemptMove(targetDirection);
            }
            
            lastMoveTime = Time.time;
        }
        
        /// <summary>
        /// プレイヤーが見えるかチェック
        /// </summary>
        private bool CanSeePlayer(Vector2Int playerPosition, float distance)
        {
            if (distance > enemyData.detectionRange)
                return false;
            
            // 簡単な視線チェック（壁に遮られていないか）
            Vector2Int direction = playerPosition - position;
            Vector2Int current = position;
            
            while (current != playerPosition)
            {
                current += new Vector2Int(
                    direction.x != 0 ? (direction.x > 0 ? 1 : -1) : 0,
                    direction.y != 0 ? (direction.y > 0 ? 1 : -1) : 0
                );
                
                DungeonCell cell = dungeonManager.GetCellAt(current);
                if (cell == null || cell.type == DungeonCellType.Wall)
                {
                    return false; // 壁に遮られている
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// おとなしいAI処理
        /// </summary>
        private Vector2Int ProcessPassiveAI(bool canSeePlayer, float distanceToPlayer)
        {
            // 攻撃されるまで基本的に動かない
            if (currentState == EnemyState.Combat || currentState == EnemyState.Chasing)
            {
                // 戦闘状態なら追跡
                return GetDirectionToPlayer();
            }
            
            // 稀にランダム移動
            if (Random.Range(0f, 1f) < 0.1f)
            {
                return GetRandomDirection();
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// 積極的AI処理
        /// </summary>
        private Vector2Int ProcessAggressiveAI(bool canSeePlayer, Vector2Int playerPosition, float distanceToPlayer)
        {
            if (canSeePlayer)
            {
                currentState = EnemyState.Chasing;
                return GetDirectionTowards(playerPosition);
            }
            else if (currentState == EnemyState.Chasing && lastKnownPlayerPosition != Vector2Int.zero)
            {
                // 最後に見た場所に向かう
                if (position == lastKnownPlayerPosition)
                {
                    currentState = EnemyState.Searching;
                    lastKnownPlayerPosition = Vector2Int.zero;
                }
                else
                {
                    return GetDirectionTowards(lastKnownPlayerPosition);
                }
            }
            else if (currentState == EnemyState.Searching)
            {
                // 周辺をランダム探索
                idleCounter++;
                if (idleCounter > 5)
                {
                    currentState = EnemyState.Idle;
                    idleCounter = 0;
                }
                return GetRandomDirection();
            }
            
            // アイドル状態でもたまに動く
            if (Random.Range(0f, 1f) < enemyData.moveChance)
            {
                return GetRandomDirection();
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// 巡回AI処理
        /// </summary>
        private Vector2Int ProcessPatrolAI()
        {
            Vector2Int target = patrolDirection ? patrolEnd : patrolStart;
            
            if (position == target)
            {
                patrolDirection = !patrolDirection;
                return Vector2Int.zero; // 一回休み
            }
            
            return GetDirectionTowards(target);
        }
        
        /// <summary>
        /// 守備AI処理
        /// </summary>
        private Vector2Int ProcessGuardAI(bool canSeePlayer, Vector2Int playerPosition, float distanceToPlayer)
        {
            Vector2Int homePosition = patrolStart; // 守備位置
            
            if (canSeePlayer && distanceToPlayer <= enemyData.detectionRange)
            {
                currentState = EnemyState.Combat;
                return GetDirectionTowards(playerPosition);
            }
            else if (Vector2Int.Distance(position, homePosition) > 2)
            {
                // 守備位置に戻る
                currentState = EnemyState.Returning;
                return GetDirectionTowards(homePosition);
            }
            else
            {
                currentState = EnemyState.Idle;
                // 守備位置周辺をうろうろ
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    return GetRandomDirection();
                }
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// 臆病AI処理
        /// </summary>
        private Vector2Int ProcessCowardAI(bool canSeePlayer, Vector2Int playerPosition)
        {
            if (canSeePlayer)
            {
                currentState = EnemyState.Fleeing;
                // プレイヤーから逃げる方向
                Vector2Int fleeDirection = position - playerPosition;
                return new Vector2Int(
                    fleeDirection.x != 0 ? (fleeDirection.x > 0 ? 1 : -1) : 0,
                    fleeDirection.y != 0 ? (fleeDirection.y > 0 ? 1 : -1) : 0
                );
            }
            else if (currentState == EnemyState.Fleeing)
            {
                // まだ少し逃げる
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    return GetRandomDirection();
                }
                currentState = EnemyState.Idle;
            }
            
            // 基本的にランダム移動
            if (Random.Range(0f, 1f) < 0.4f)
            {
                return GetRandomDirection();
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// ランダムAI処理
        /// </summary>
        private Vector2Int ProcessRandomAI()
        {
            if (Random.Range(0f, 1f) < enemyData.moveChance)
            {
                return GetRandomDirection();
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// プレイヤーへの方向取得
        /// </summary>
        private Vector2Int GetDirectionToPlayer()
        {
            Vector2Int playerPosition = dungeonManager.GetPlayerPosition();
            return GetDirectionTowards(playerPosition);
        }
        
        /// <summary>
        /// 指定位置への方向取得
        /// </summary>
        private Vector2Int GetDirectionTowards(Vector2Int target)
        {
            Vector2Int direction = target - position;
            
            // 最も大きな軸を優先して移動
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return new Vector2Int(direction.x > 0 ? 1 : -1, 0);
            }
            else if (direction.y != 0)
            {
                return new Vector2Int(0, direction.y > 0 ? 1 : -1);
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// ランダム方向取得
        /// </summary>
        private Vector2Int GetRandomDirection()
        {
            Vector2Int[] directions = {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };
            
            return directions[Random.Range(0, directions.Length)];
        }
        
        /// <summary>
        /// 移動試行
        /// </summary>
        private void AttemptMove(Vector2Int direction)
        {
            Vector2Int newPosition = position + direction;
            
            // 境界チェック
            Vector2Int dungeonSize = dungeonManager.GetDungeonSize();
            if (newPosition.x < 0 || newPosition.x >= dungeonSize.x ||
                newPosition.y < 0 || newPosition.y >= dungeonSize.y)
            {
                return;
            }
            
            // セルチェック
            DungeonCell targetCell = dungeonManager.GetCellAt(newPosition);
            if (targetCell == null || (!enemyData.canMoveThroughWalls && !targetCell.IsWalkable()))
            {
                return;
            }
            
            // プレイヤーと同じ位置にいるかチェック
            Vector2Int playerPosition = dungeonManager.GetPlayerPosition();
            if (newPosition == playerPosition)
            {
                // 戦闘開始
                InitiateCombat();
                return;
            }
            
            // 他の敵と重複チェック
            if (enemyManager != null && enemyManager.IsPositionOccupiedByEnemy(newPosition, this))
            {
                return;
            }
            
            // 移動実行
            position = newPosition;
            OnEnemyMoved?.Invoke(this, position);
            
            if (debugMode)
            {
                Debug.Log($"🧌 {enemyData.enemyName} moved to {position}");
            }
        }
        
        /// <summary>
        /// 戦闘開始
        /// </summary>
        private void InitiateCombat()
        {
            currentState = EnemyState.Combat;
            
            // CombatManagerに戦闘開始を通知
            var combatManager = gameManager?.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                combatManager.InitiateCombat(this);
            }
            
            if (debugMode)
            {
                Debug.Log($"⚔️ {enemyData.enemyName} initiated combat!");
            }
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public int TakeDamage(int damage)
        {
            if (!IsAlive) return 0;
            
            int actualDamage = Mathf.Max(1, damage - currentStats.defense);
            currentStats.currentHealth -= actualDamage;
            
            OnEnemyDamaged?.Invoke(this, actualDamage);
            
            if (currentStats.currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 戦闘状態に移行
                currentState = EnemyState.Combat;
            }
            
            return actualDamage;
        }
        
        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            currentState = EnemyState.Dead;
            OnEnemyDeath?.Invoke(this);
            
            // アイテムドロップ処理
            DropItems();
            
            if (debugMode)
            {
                Debug.Log($"💀 {enemyData.enemyName} died at {position}");
            }
            
            // オブジェクト削除
            Destroy(gameObject, 0.1f);
        }
        
        /// <summary>
        /// アイテムドロップ処理
        /// </summary>
        private void DropItems()
        {
            if (enemyData.possibleDrops == null) return;
            
            foreach (var drop in enemyData.possibleDrops)
            {
                if (drop.ShouldDrop())
                {
                    int quantity = drop.GetDropQuantity();
                    // ItemManagerにアイテム生成を要請
                    var itemManager = gameManager?.GetComponent<ItemManager>();
                    if (itemManager != null)
                    {
                        itemManager.SpawnItem(drop.itemId, position, quantity);
                    }
                }
            }
        }
        
        /// <summary>
        /// 有効なスポーン位置を取得
        /// </summary>
        private Vector2Int GetValidSpawnPosition()
        {
            if (dungeonManager != null)
            {
                return dungeonManager.GetRandomWalkablePosition();
            }
            
            return new Vector2Int(1, 1);
        }
        
        /// <summary>
        /// 巡回ルートの設定
        /// </summary>
        private void SetupPatrolRoute()
        {
            patrolStart = position;
            
            // ランダムな巡回終点を決定
            Vector2Int offset = new Vector2Int(
                Random.Range(-3, 4),
                Random.Range(-3, 4)
            );
            
            patrolEnd = position + offset;
            
            // 有効な位置に調整
            Vector2Int dungeonSize = dungeonManager.GetDungeonSize();
            patrolEnd.x = Mathf.Clamp(patrolEnd.x, 1, dungeonSize.x - 2);
            patrolEnd.y = Mathf.Clamp(patrolEnd.y, 1, dungeonSize.y - 2);
        }
        
        /// <summary>
        /// 敵の状態をリセット
        /// </summary>
        public void ResetState()
        {
            currentState = EnemyState.Idle;
            lastKnownPlayerPosition = Vector2Int.zero;
            idleCounter = 0;
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{enemyData.enemyName} at {position} - HP: {currentStats.currentHealth}/{currentStats.maxHealth} - State: {currentState}";
        }
    }
    
    /// <summary>
    /// 敵の状態
    /// </summary>
    public enum EnemyState
    {
        Idle,           // 待機
        Patrolling,     // 巡回中
        Chasing,        // 追跡中
        Combat,         // 戦闘中
        Fleeing,        // 逃走中
        Searching,      // 探索中
        Returning,      // 帰還中
        Dead            // 死亡
    }
}