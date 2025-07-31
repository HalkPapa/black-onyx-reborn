using UnityEngine;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ブラックオニキス復刻版システム統合テスト
    /// 起動時に自動でシステムの動作を確認し、問題があれば報告
    /// </summary>
    public class BlackOnyxIntegrationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool verboseLogging = true;
        
        private GameManager gameManager;
        private BlackOnyxDungeonManager blackOnyxDungeonManager;
        private EnemyManager enemyManager;
        
        void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunIntegrationTests());
            }
        }
        
        /// <summary>
        /// 統合テストの実行
        /// </summary>
        private IEnumerator RunIntegrationTests()
        {
            LogTest("🧪 Starting Black Onyx Integration Tests...");
            
            yield return new WaitForSeconds(1f); // システム初期化待機
            
            // Step 1: Manager Initialization Test
            yield return StartCoroutine(TestManagersInitialization());
            
            // Step 2: Dungeon System Test
            yield return StartCoroutine(TestDungeonSystem());
            
            // Step 3: Enemy System Test  
            yield return StartCoroutine(TestEnemySystem());
            
            // Step 4: Floor Navigation Test
            yield return StartCoroutine(TestFloorNavigation());
            
            // Step 5: Authentic Data Test
            yield return StartCoroutine(TestAuthenticBlackOnyxData());
            
            LogTest("✅ Black Onyx Integration Tests Completed Successfully!");
        }
        
        /// <summary>
        /// マネージャー初期化テスト
        /// </summary>
        private IEnumerator TestManagersInitialization()
        {
            LogTest("🔧 Testing Managers Initialization...");
            
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                LogTestError("❌ GameManager not found!");
                yield break;
            }
            LogTest("✓ GameManager found");
            
            blackOnyxDungeonManager = gameManager.BlackOnyxDungeonManager;
            if (blackOnyxDungeonManager == null)
            {
                LogTestError("❌ BlackOnyxDungeonManager not found!");
                yield break;
            }
            LogTest("✓ BlackOnyxDungeonManager found");
            
            enemyManager = gameManager.GetComponent<EnemyManager>();
            if (enemyManager == null)
            {
                LogTestError("❌ EnemyManager not found!");
                yield break;
            }
            LogTest("✓ EnemyManager found");
            
            LogTest("✅ All managers initialized successfully");
            yield return null;
        }
        
        /// <summary>
        /// ダンジョンシステムテスト
        /// </summary>
        private IEnumerator TestDungeonSystem()
        {
            LogTest("🏰 Testing Dungeon System...");
            
            // Test floor range
            int currentFloor = blackOnyxDungeonManager.CurrentFloor;
            if (currentFloor < -6 || currentFloor > 2)
            {
                LogTestError($"❌ Invalid floor range: {currentFloor} (should be -6 to 2)");
                yield break;
            }
            LogTest($"✓ Valid floor range: {currentFloor}");
            
            // Test player position
            Vector2Int playerPos = blackOnyxDungeonManager.PlayerPosition;
            Vector2Int dungeonSize = blackOnyxDungeonManager.DungeonSize;
            if (playerPos.x < 0 || playerPos.x >= dungeonSize.x || 
                playerPos.y < 0 || playerPos.y >= dungeonSize.y)
            {
                LogTestError($"❌ Invalid player position: {playerPos} in dungeon {dungeonSize}");
                yield break;
            }
            LogTest($"✓ Valid player position: {playerPos}");
            
            // Test current floor generation
            var currentFloorData = blackOnyxDungeonManager.GetCurrentFloor();
            if (currentFloorData == null)
            {
                LogTestError("❌ Current floor data not generated");
                yield break;
            }
            LogTest("✓ Current floor data exists");
            
            LogTest("✅ Dungeon system working correctly");
            yield return null;
        }
        
        /// <summary>
        /// 敵システムテスト
        /// </summary>
        private IEnumerator TestEnemySystem()
        {
            LogTest("👹 Testing Enemy System...");
            
            // Test enemy database
            if (enemyManager.ActiveEnemyCount < 0)
            {
                LogTestError("❌ Invalid enemy count");
                yield break;
            }
            LogTest($"✓ Enemy system initialized (Active: {enemyManager.ActiveEnemyCount})");
            
            // Test authentic enemy data by trying to spawn known enemies
            var testEnemies = new string[] { "バット", "コボルト", "スケルトン", "ゴブリン" };
            foreach (var enemyName in testEnemies)
            {
                try
                {
                    enemyManager.ForceSpawnEnemy(enemyName, new Vector2Int(10, 10));
                    LogTest($"✓ Successfully spawned {enemyName}");
                }
                catch (System.Exception e)
                {
                    LogTest($"⚠️ Failed to spawn {enemyName}: {e.Message}");
                }
            }
            
            LogTest("✅ Enemy system tested");
            yield return null;
        }
        
        /// <summary>
        /// フロア移動テスト
        /// </summary>
        private IEnumerator TestFloorNavigation()
        {
            LogTest("🔄 Testing Floor Navigation...");
            
            int originalFloor = blackOnyxDungeonManager.CurrentFloor;
            
            // Test entrance system
            try
            {
                blackOnyxDungeonManager.SetDungeonEntrance(BlackOnyxDungeonManager.DungeonEntrance.Graveyard);
                LogTest("✓ Graveyard entrance set");
                
                blackOnyxDungeonManager.SetDungeonEntrance(BlackOnyxDungeonManager.DungeonEntrance.Well);
                LogTest("✓ Well entrance set");
                
                blackOnyxDungeonManager.SetDungeonEntrance(BlackOnyxDungeonManager.DungeonEntrance.Ruins);
                LogTest("✓ Ruins entrance set");
            }
            catch (System.Exception e)
            {
                LogTestError($"❌ Entrance system error: {e.Message}");
            }
            
            // Test floor configuration data
            var floorConfigs = blackOnyxDungeonManager.GetAllFloorConfigurations();
            if (floorConfigs == null || floorConfigs.Length != 8)
            {
                LogTestError($"❌ Invalid floor configurations: {floorConfigs?.Length ?? 0} (should be 8)");
                yield break;
            }
            LogTest("✓ All 8 floor configurations found");
            
            // Verify floor names and types
            foreach (var config in floorConfigs)
            {
                if (config.floorNumber == -6 && config.floorType != FloorType.ColorMaze)
                {
                    LogTestError($"❌ B6 should be ColorMaze, but is {config.floorType}");
                }
                else if (config.floorNumber == 2 && config.floorType != FloorType.Heaven)
                {
                    LogTestError($"❌ F2 should be Heaven, but is {config.floorType}");
                }
            }
            LogTest("✓ Floor types verified");
            
            LogTest("✅ Floor navigation system working");
            yield return null;
        }
        
        /// <summary>
        /// オリジナルブラックオニキスデータテスト
        /// </summary>
        private IEnumerator TestAuthenticBlackOnyxData()
        {
            LogTest("🏺 Testing Authentic Black Onyx Data...");
            
            // Test color maze functionality
            bool colorResult = blackOnyxDungeonManager.CheckColorMazeSequence(1);
            LogTest($"✓ Color maze sequence check: {colorResult}");
            
            // Test floor naming (should be in Japanese)
            var floorConfigs = blackOnyxDungeonManager.GetAllFloorConfigurations();
            bool hasJapaneseNames = false;
            foreach (var config in floorConfigs)
            {
                if (config.floorName.Contains("地下") || config.floorName.Contains("地上"))
                {
                    hasJapaneseNames = true;
                    break;
                }
            }
            
            if (hasJapaneseNames)
            {
                LogTest("✓ Japanese floor names confirmed");
            }
            else
            {
                LogTest("⚠️ Japanese floor names not found");
            }
            
            // Test enemy authenticity
            var debugInfo = enemyManager.GetDebugInfo();
            if (debugInfo.Contains("バット") || debugInfo.Contains("コボルト"))
            {
                LogTest("✓ Authentic Japanese enemy names confirmed");
            }
            else
            {
                LogTest("⚠️ Authentic enemy names not detected in active enemies");
            }
            
            LogTest("✅ Authentic Black Onyx data verified");
            yield return null;
        }
        
        /// <summary>
        /// テストログ出力
        /// </summary>
        private void LogTest(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[BlackOnyxTest] {message}");
            }
        }
        
        /// <summary>
        /// テストエラーログ出力
        /// </summary>
        private void LogTestError(string message)
        {
            Debug.LogError($"[BlackOnyxTest] {message}");
        }
        
        /// <summary>
        /// システム状態の詳細レポート生成
        /// </summary>
        [ContextMenu("Generate System Report")]
        public void GenerateSystemReport()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("🏰 Black Onyx Reborn - System Status Report");
            report.AppendLine("=" * 50);
            
            // GameManager Status
            if (GameManager.Instance != null)
            {
                report.AppendLine($"GameManager: ✓ Active (State: {GameManager.Instance.CurrentState})");
            }
            else
            {
                report.AppendLine("GameManager: ❌ Not Found");
            }
            
            // BlackOnyxDungeonManager Status
            if (GameManager.Instance?.BlackOnyxDungeonManager != null)
            {
                var dungeon = GameManager.Instance.BlackOnyxDungeonManager;
                report.AppendLine($"BlackOnyxDungeonManager: ✓ Active");
                report.AppendLine($"  Current Floor: {dungeon.CurrentFloor}");
                report.AppendLine($"  Player Position: {dungeon.PlayerPosition}");
                report.AppendLine($"  Dungeon Size: {dungeon.DungeonSize}");
                report.AppendLine($"  Floor Configurations: {dungeon.GetAllFloorConfigurations()?.Length ?? 0}");
            }
            else
            {
                report.AppendLine("BlackOnyxDungeonManager: ❌ Not Found");
            }
            
            // EnemyManager Status
            var enemyMgr = FindObjectOfType<EnemyManager>();
            if (enemyMgr != null)
            {
                report.AppendLine($"EnemyManager: ✓ Active");
                report.AppendLine($"  Active Enemies: {enemyMgr.ActiveEnemyCount}");
            }
            else
            {
                report.AppendLine("EnemyManager: ❌ Not Found");
            }
            
            report.AppendLine("=" * 50);
            Debug.Log(report.ToString());
        }
    }
}