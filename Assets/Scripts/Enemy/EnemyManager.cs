using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 敵の生成・管理を行うマネージャー
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private EnemyData[] enemyDatabase;
        [SerializeField] private int maxEnemiesPerFloor = 8;
        [SerializeField] private int minEnemiesPerFloor = 3;
        [SerializeField] private float spawnCheckInterval = 5f;
        [SerializeField] private float playerProximitySpawnBlock = 3f;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showEnemyPositions = false;
        
        // Current enemies
        private Dictionary<int, List<Enemy>> enemiesByFloor = new Dictionary<int, List<Enemy>>();
        private List<Enemy> activeEnemies = new List<Enemy>();
        
        // Manager references
        private DungeonManager dungeonManager;
        private BlackOnyxDungeonManager blackOnyxDungeonManager;
        private DungeonManagerBridge dungeonBridge;
        private GameManager gameManager;
        
        // Spawn tracking
        private float lastSpawnCheck = 0f;
        private System.Random spawnRandom;
        
        // Events
        public System.Action<Enemy> OnEnemySpawned;
        public System.Action<Enemy> OnEnemyKilled;
        public System.Action<int> OnAllEnemiesKilled;
        
        // Properties
        public int ActiveEnemyCount => activeEnemies.Count;
        public List<Enemy> ActiveEnemies => new List<Enemy>(activeEnemies);
        
        void Awake()
        {
            InitializeEnemyManager();
        }
        
        void Start()
        {
            // Subscribe to dungeon events - use Black Onyx system
            if (blackOnyxDungeonManager != null)
            {
                blackOnyxDungeonManager.OnFloorChanged += OnFloorChanged;
            }
            else if (dungeonManager != null)
            {
                dungeonManager.OnFloorChanged += OnFloorChanged;
            }
        }
        
        void Update()
        {
            if (gameManager?.CurrentState != GameManager.GameState.InGame)
                return;
                
            ProcessSpawnLogic();
            
            if (showEnemyPositions)
            {
                DrawEnemyDebugInfo();
            }
        }
        
        /// <summary>
        /// 敵マネージャーの初期化
        /// </summary>
        private void InitializeEnemyManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                blackOnyxDungeonManager = gameManager.BlackOnyxDungeonManager;
                
                // Create or get the bridge for backward compatibility
                dungeonBridge = GetComponent<DungeonManagerBridge>() ?? gameObject.AddComponent<DungeonManagerBridge>();
            }
            
            // Initialize random seed
            spawnRandom = new System.Random();
            
            // Load enemy database if not assigned
            if (enemyDatabase == null || enemyDatabase.Length == 0)
            {
                LoadEnemyDatabase();
            }
            
            Debug.Log($"🧌 Enemy Manager initialized with {(enemyDatabase?.Length ?? 0)} enemy types");
        }
        
        /// <summary>
        /// 敵データベースの読み込み
        /// </summary>
        private void LoadEnemyDatabase()
        {
            // リソースから敵データを読み込み
            enemyDatabase = Resources.LoadAll<EnemyData>("Enemies");
            
            if (enemyDatabase.Length == 0)
            {
                // デフォルトの敵データを作成
                CreateDefaultEnemyData();
            }
        }
        
        /// <summary>
        /// オリジナル ブラックオニキス敵データの作成（1984年PC-8801版準拠）
        /// </summary>
        private void CreateDefaultEnemyData()
        {
            List<EnemyData> blackOnyxEnemies = new List<EnemyData>();
            
            // バット（最弱モンスター）- オリジナル準拠調整
            var bat = ScriptableObject.CreateInstance<EnemyData>();
            bat.enemyName = "バット";
            bat.description = "最も弱いモンスターの一つ。レベル1で簡単に倒せる";
            bat.displayChar = 'b';
            bat.displaySymbol = "🦇";
            bat.maxHealth = 3; // オリジナル準拠：より脆弱に
            bat.attack = 1; // オリジナル準拠：レベル1でも安全
            bat.defense = 0;
            bat.speed = 7;
            bat.experience = 1; // オリジナル準拠：経験値を厳しく
            bat.goldDrop = 0; // オリジナル準拠：金を持たない
            bat.aiType = EnemyAIType.Random;
            bat.detectionRange = 2f;
            bat.minFloor = -1; // B1
            bat.maxFloor = -1;
            bat.spawnWeight = 2.0f; // 最弱なので多く出現
            blackOnyxEnemies.Add(bat);
            
            // コボルト（最弱、5GP持ち）- オリジナル準拠調整
            var kobold = ScriptableObject.CreateInstance<EnemyData>();
            kobold.enemyName = "コボルト";
            kobold.description = "小さな亜人族、わずかな金を持つ。最弱クラス";
            kobold.displayChar = 'k';
            kobold.displaySymbol = "👺";
            kobold.maxHealth = 6; // オリジナル準拠：より弱く
            kobold.attack = 2; // オリジナル準拠：攻撃力下げ
            kobold.defense = 0; // オリジナル準拠：防御力なし
            kobold.speed = 5;
            kobold.experience = 2; // オリジナル準拠：経験値厳しく
            kobold.goldDrop = 5; // オリジナル通り5GP
            kobold.aiType = EnemyAIType.Passive;
            kobold.detectionRange = 2f;
            kobold.minFloor = -1; // B1
            kobold.maxFloor = -1;
            kobold.spawnWeight = 1.5f; // お金持ちなので価値あり
            blackOnyxEnemies.Add(kobold);
            
            // スケルトン（お金を持つ）- オリジナル準拠調整
            var skeleton = ScriptableObject.CreateInstance<EnemyData>();
            skeleton.enemyName = "スケルトン";
            skeleton.description = "動く骸骨、金を持っている";
            skeleton.displayChar = 's';
            skeleton.displaySymbol = "💀";
            skeleton.maxHealth = 10; // オリジナル準拠：適度な強さ
            skeleton.attack = 4; // オリジナル準拠：レベル2で危険
            skeleton.defense = 1; // オリジナル準拠：軽い防御
            skeleton.speed = 4;
            skeleton.experience = 4; // オリジナル準拠：経験値厳しく
            skeleton.goldDrop = 10; // オリジナル通り10GP
            skeleton.aiType = EnemyAIType.Patrol;
            skeleton.detectionRange = 3f;
            skeleton.minFloor = -1; // B1
            skeleton.maxFloor = -2;
            skeleton.spawnWeight = 1.0f;
            blackOnyxEnemies.Add(skeleton);
            
            // ゴブリン（お金を持つ）- オリジナル準拠調整
            var goblin = ScriptableObject.CreateInstance<EnemyData>();
            goblin.enemyName = "ゴブリン";
            goblin.description = "小柄な魔物、金を持っている";
            goblin.displayChar = 'g';
            goblin.displaySymbol = "👹";
            goblin.maxHealth = 8; // オリジナル準拠：適度な強さ
            goblin.attack = 3; // オリジナル準拠：スケルトンより弱く
            goblin.defense = 0; // オリジナル準拠：防御力なし
            goblin.speed = 6;
            goblin.experience = 3; // オリジナル準拠：経験値厳しく
            goblin.goldDrop = 8; // オリジナル通り8GP
            goblin.aiType = EnemyAIType.Aggressive;
            goblin.detectionRange = 3f;
            goblin.minFloor = -1; // B1
            goblin.maxFloor = -2;
            goblin.spawnWeight = 1.0f;
            blackOnyxEnemies.Add(goblin);
            
            // アステカ（お金を持つ）- オリジナル準拠調整
            var aztec = ScriptableObject.CreateInstance<EnemyData>();
            aztec.enemyName = "アステカ";
            aztec.description = "古代戦士の霊、財宝を守る";
            aztec.displayChar = 'A';
            aztec.displaySymbol = "🗿";
            aztec.maxHealth = 12; // オリジナル準拠：強めだが適度に
            aztec.attack = 5; // オリジナル準拠：レベル3で危険
            aztec.defense = 2; // オリジナル準拠：適度な防御
            aztec.speed = 4;
            aztec.experience = 5; // オリジナル準拠：経験値厳しく
            aztec.goldDrop = 12; // オリジナル通り12GP
            aztec.aiType = EnemyAIType.Guard;
            aztec.detectionRange = 4f;
            aztec.minFloor = -1; // B1
            aztec.maxFloor = -2;
            aztec.spawnWeight = 0.8f;
            blackOnyxEnemies.Add(aztec);
            
            // コブラ（B3の危険な敵）- オリジナル準拠調整
            var cobra = ScriptableObject.CreateInstance<EnemyData>();
            cobra.enemyName = "コブラ";
            cobra.description = "毒蛇、危険な相手。要注意";
            cobra.displayChar = 'C';
            cobra.displaySymbol = "🐍";
            cobra.maxHealth = 20; // オリジナル準拠：危険だが倒せる
            cobra.attack = 10; // オリジナル準拠：レベル4-5で危険
            cobra.defense = 1; // オリジナル準拠：軽い防御
            cobra.speed = 8;
            cobra.experience = 8; // オリジナル準拠：経験値厳しく
            cobra.goldDrop = 3; // オリジナル通り少額
            cobra.aiType = EnemyAIType.Aggressive;
            cobra.detectionRange = 5f;
            cobra.poisonous = true;
            cobra.minFloor = -3; // B3
            cobra.maxFloor = -3;
            cobra.spawnWeight = 0.6f;
            blackOnyxEnemies.Add(cobra);
            
            // ハイド（透明マントを落とす）- オリジナル準拠調整
            var hide = ScriptableObject.CreateInstance<EnemyData>();
            hide.enemyName = "ハイド";
            hide.description = "透明な怪物、透明マントを落とす特殊敵";
            hide.displayChar = 'H';
            hide.displaySymbol = "👻";
            hide.maxHealth = 25; // オリジナル準拠：強めだが倒せる
            hide.attack = 6; // オリジナル準拠：レベル5-6で戦える
            hide.defense = 1;
            hide.speed = 9;
            hide.experience = 10; // オリジナル準拠：経験値厳しく
            hide.goldDrop = 5;
            hide.aiType = EnemyAIType.Coward;
            hide.detectionRange = 4f;
            hide.minFloor = -5; // B5
            hide.maxFloor = -5;
            hide.spawnWeight = 0.3f;
            // 透明マントドロップは後でItemDropで設定
            blackOnyxEnemies.Add(hide);
            
            // オクトパス（井戸のボス）- オリジナル準拠調整
            var octopus = ScriptableObject.CreateInstance<EnemyData>();
            octopus.enemyName = "オクトパス";
            octopus.description = "井戸に潜む巨大な蛸。井戸から入ると遭遇";
            octopus.displayChar = 'O';
            octopus.displaySymbol = "🐙";
            octopus.maxHealth = 40; // オリジナル準拠：ボスだが倒せる
            octopus.attack = 15; // オリジナル準拠：レベル6-7で危険
            octopus.defense = 3; // オリジナル準拠：適度な防御
            octopus.speed = 3;
            octopus.experience = 20; // オリジナル準拠：経験値厳しく
            octopus.goldDrop = 30;
            octopus.aiType = EnemyAIType.Guard;
            octopus.detectionRange = 6f;
            octopus.minFloor = -5; // B5
            octopus.maxFloor = -5;
            octopus.spawnWeight = 0.1f;
            octopus.maxPerFloor = 1; // ボス級なので1体のみ
            blackOnyxEnemies.Add(octopus);
            
            // 巨人（天界の超強敵）- オリジナル準拠調整（最重要）
            var giant = ScriptableObject.CreateInstance<EnemyData>();
            giant.enemyName = "巨人";
            giant.description = "天界を守る巨大戦士。レベル9でも一撃で倒される";
            giant.displayChar = 'G';
            giant.displaySymbol = "🗿";
            giant.maxHealth = 80; // オリジナル準拠：最強だが倒せる
            giant.attack = 80; // オリジナル準拠：レベル9でも一撃死！
            giant.defense = 8; // オリジナル準拠：高い防御力
            giant.speed = 2;
            giant.experience = 50; // オリジナル準拠：経験値も厳しく
            giant.goldDrop = 100;
            giant.aiType = EnemyAIType.Guard;
            giant.detectionRange = 7f;
            giant.minFloor = 2; // 天界（地上2階）
            giant.maxFloor = 2;
            giant.spawnWeight = 0.5f;
            giant.maxPerFloor = 2;
            blackOnyxEnemies.Add(giant);
            
            enemyDatabase = blackOnyxEnemies.ToArray();
            
            if (debugMode)
            {
                Debug.Log($"🏰 Created {enemyDatabase.Length} authentic Black Onyx enemy types");
            }
        }
        
        /// <summary>
        /// スポーンロジックの処理
        /// </summary>
        private void ProcessSpawnLogic()
        {
            if (Time.time - lastSpawnCheck < spawnCheckInterval)
                return;
            
            lastSpawnCheck = Time.time;
            
            // Use Black Onyx dungeon system if available
            int currentFloor = blackOnyxDungeonManager?.CurrentFloor ?? dungeonBridge?.GetCurrentFloorNumber() ?? 1;
            CheckAndSpawnEnemies(currentFloor);
        }
        
        /// <summary>
        /// 敵のスポーンチェックと実行
        /// </summary>
        private void CheckAndSpawnEnemies(int floor)
        {
            // 現在のフロアの敵リストを取得
            if (!enemiesByFloor.ContainsKey(floor))
            {
                enemiesByFloor[floor] = new List<Enemy>();
            }
            
            var currentFloorEnemies = enemiesByFloor[floor];
            
            // 死んだ敵を削除
            currentFloorEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
            
            // 敵の数が最小数より少ない場合、追加スポーン
            int currentCount = currentFloorEnemies.Count;
            int targetCount = Random.Range(minEnemiesPerFloor, maxEnemiesPerFloor + 1);
            
            if (currentCount < targetCount)
            {
                int spawnCount = Mathf.Min(targetCount - currentCount, 2); // 一度に最大2体まで
                SpawnEnemies(floor, spawnCount);
            }
        }
        
        /// <summary>
        /// 敵のスポーン実行
        /// </summary>
        private void SpawnEnemies(int floor, int count)
        {
            Vector2Int playerPosition = blackOnyxDungeonManager?.PlayerPosition ?? dungeonBridge?.GetPlayerPosition() ?? Vector2Int.zero;
            
            for (int i = 0; i < count; i++)
            {
                // このフロアに適した敵を選択
                EnemyData enemyData = SelectEnemyForFloor(floor);
                if (enemyData == null) continue;
                
                // スポーン位置を決定
                Vector2Int spawnPosition = FindValidSpawnPosition(playerPosition);
                if (spawnPosition == Vector2Int.zero) continue;
                
                // 敵を生成
                Enemy newEnemy = SpawnEnemy(enemyData, spawnPosition);
                if (newEnemy != null)
                {
                    RegisterEnemy(newEnemy, floor);
                }
            }
        }
        
        /// <summary>
        /// フロアに適した敵の選択
        /// </summary>
        private EnemyData SelectEnemyForFloor(int floor)
        {
            // このフロアに出現可能な敵をフィルタリング
            var validEnemies = enemyDatabase.Where(enemy => enemy.CanSpawnOnFloor(floor)).ToArray();
            
            if (validEnemies.Length == 0)
                return null;
            
            // 重み付きランダム選択
            float totalWeight = validEnemies.Sum(enemy => enemy.spawnWeight);
            float randomValue = (float)spawnRandom.NextDouble() * totalWeight;
            
            float currentWeight = 0f;
            foreach (var enemy in validEnemies)
            {
                currentWeight += enemy.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemy;
                }
            }
            
            return validEnemies[validEnemies.Length - 1]; // フォールバック
        }
        
        /// <summary>
        /// 有効なスポーン位置の検索
        /// </summary>
        private Vector2Int FindValidSpawnPosition(Vector2Int playerPosition)
        {
            int attempts = 20;
            
            while (attempts > 0)
            {
                Vector2Int candidatePosition = dungeonBridge?.GetRandomWalkablePosition() ?? Vector2Int.zero;
                if (candidatePosition == Vector2Int.zero) break;
                
                // プレイヤーから十分離れているかチェック
                float distanceFromPlayer = Vector2Int.Distance(candidatePosition, playerPosition);
                if (distanceFromPlayer < playerProximitySpawnBlock)
                {
                    attempts--;
                    continue;
                }
                
                // 他の敵と重複していないかチェック
                if (IsPositionOccupiedByEnemy(candidatePosition))
                {
                    attempts--;
                    continue;
                }
                
                return candidatePosition;
            }
            
            return Vector2Int.zero; // 失敗
        }
        
        /// <summary>
        /// 敵の実際の生成
        /// </summary>
        private Enemy SpawnEnemy(EnemyData enemyData, Vector2Int position)
        {
            // 敵オブジェクトを作成
            GameObject enemyObj = new GameObject($"Enemy_{enemyData.enemyName}");
            Enemy enemy = enemyObj.AddComponent<Enemy>();
            
            // データと位置を設定
            enemy.InitializeWithData(enemyData, position);
            
            // イベント購読
            enemy.OnEnemyDeath += OnEnemyDied;
            enemy.OnEnemyMoved += OnEnemyPositionChanged;
            
            OnEnemySpawned?.Invoke(enemy);
            
            if (debugMode)
            {
                Debug.Log($"🧌 Spawned {enemyData.enemyName} at {position}");
            }
            
            return enemy;
        }
        
        /// <summary>
        /// 敵の登録
        /// </summary>
        private void RegisterEnemy(Enemy enemy, int floor)
        {
            if (!enemiesByFloor.ContainsKey(floor))
            {
                enemiesByFloor[floor] = new List<Enemy>();
            }
            
            enemiesByFloor[floor].Add(enemy);
            activeEnemies.Add(enemy);
        }
        
        /// <summary>
        /// 位置が敵に占領されているかチェック
        /// </summary>
        public bool IsPositionOccupiedByEnemy(Vector2Int position, Enemy excludeEnemy = null)
        {
            return activeEnemies.Any(enemy => 
                enemy != null && 
                enemy != excludeEnemy && 
                enemy.IsAlive && 
                enemy.Position == position);
        }
        
        /// <summary>
        /// 指定位置の敵を取得
        /// </summary>
        public Enemy GetEnemyAt(Vector2Int position)
        {
            return activeEnemies.FirstOrDefault(enemy => 
                enemy != null && 
                enemy.IsAlive && 
                enemy.Position == position);
        }
        
        /// <summary>
        /// フロア内の敵を取得
        /// </summary>
        public List<Enemy> GetEnemiesOnFloor(int floor)
        {
            if (enemiesByFloor.ContainsKey(floor))
            {
                return enemiesByFloor[floor].Where(enemy => enemy != null && enemy.IsAlive).ToList();
            }
            
            return new List<Enemy>();
        }
        
        /// <summary>
        /// フロア変更イベント
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            // アクティブ敵リストを更新
            UpdateActiveEnemyList(newFloor);
            
            if (debugMode)
            {
                Debug.Log($"🧌 Floor changed to {newFloor}. Active enemies: {activeEnemies.Count}");
            }
        }
        
        /// <summary>
        /// アクティブ敵リストの更新
        /// </summary>
        private void UpdateActiveEnemyList(int currentFloor)
        {
            activeEnemies.Clear();
            
            if (enemiesByFloor.ContainsKey(currentFloor))
            {
                activeEnemies.AddRange(enemiesByFloor[currentFloor].Where(enemy => 
                    enemy != null && enemy.IsAlive));
            }
        }
        
        /// <summary>
        /// 敵死亡イベント
        /// </summary>
        private void OnEnemyDied(Enemy deadEnemy)
        {
            // リストから削除
            activeEnemies.Remove(deadEnemy);
            
            foreach (var floorList in enemiesByFloor.Values)
            {
                floorList.Remove(deadEnemy);
            }
            
            OnEnemyKilled?.Invoke(deadEnemy);
            
            // フロアの敵が全滅したかチェック
            int currentFloor = blackOnyxDungeonManager?.CurrentFloor ?? dungeonBridge?.GetCurrentFloorNumber() ?? 1;
            if (GetEnemiesOnFloor(currentFloor).Count == 0)
            {
                OnAllEnemiesKilled?.Invoke(currentFloor);
            }
            
            if (debugMode)
            {
                Debug.Log($"💀 {deadEnemy.Data.enemyName} died. Remaining enemies: {activeEnemies.Count}");
            }
        }
        
        /// <summary>
        /// 敵位置変更イベント
        /// </summary>
        private void OnEnemyPositionChanged(Enemy enemy, Vector2Int newPosition)
        {
            // 必要に応じて位置追跡処理
        }
        
        /// <summary>
        /// 全ての敵をクリア
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies.ToList())
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            
            activeEnemies.Clear();
            enemiesByFloor.Clear();
            
            Debug.Log("🧌 All enemies cleared");
        }
        
        /// <summary>
        /// 特定フロアの敵をクリア
        /// </summary>
        public void ClearFloorEnemies(int floor)
        {
            if (enemiesByFloor.ContainsKey(floor))
            {
                foreach (var enemy in enemiesByFloor[floor].ToList())
                {
                    if (enemy != null)
                    {
                        activeEnemies.Remove(enemy);
                        Destroy(enemy.gameObject);
                    }
                }
                
                enemiesByFloor[floor].Clear();
            }
        }
        
        /// <summary>
        /// デバッグ情報の描画
        /// </summary>
        private void DrawEnemyDebugInfo()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    Vector2Int pos = enemy.Position;
                    Debug.DrawRay(new Vector3(pos.x, pos.y, 0), Vector3.up * 0.5f, Color.red, 0.1f);
                }
            }
        }
        
        /// <summary>
        /// 強制スポーン（デバッグ用）
        /// </summary>
        public void ForceSpawnEnemy(string enemyName, Vector2Int position)
        {
            var enemyData = enemyDatabase.FirstOrDefault(data => data.enemyName == enemyName);
            if (enemyData != null)
            {
                Enemy newEnemy = SpawnEnemy(enemyData, position);
                if (newEnemy != null)
                {
                    int currentFloor = blackOnyxDungeonManager?.CurrentFloor ?? dungeonBridge?.GetCurrentFloorNumber() ?? 1;
                    RegisterEnemy(newEnemy, currentFloor);
                }
            }
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            int currentFloor = blackOnyxDungeonManager?.CurrentFloor ?? dungeonBridge?.GetCurrentFloorNumber() ?? 1;
            var floorEnemies = GetEnemiesOnFloor(currentFloor);
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Enemy Manager - Floor {currentFloor}");
            sb.AppendLine($"Active Enemies: {activeEnemies.Count}");
            sb.AppendLine($"Floor Enemies: {floorEnemies.Count}");
            
            foreach (var enemy in floorEnemies)
            {
                sb.AppendLine($"- {enemy.GetDebugInfo()}");
            }
            
            return sb.ToString();
        }
        
        void OnDestroy()
        {
            // イベント購読解除
            if (blackOnyxDungeonManager != null)
            {
                blackOnyxDungeonManager.OnFloorChanged -= OnFloorChanged;
            }
            else if (dungeonManager != null)
            {
                dungeonManager.OnFloorChanged -= OnFloorChanged;
            }
        }
    }
}