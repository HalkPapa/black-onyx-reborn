using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BlackOnyxReborn
{
    /// <summary>
    /// オリジナル ブラックオニキス準拠のダンジョン管理システム
    /// 地下6階〜地上2階の8フロア構成
    /// </summary>
    public class BlackOnyxDungeonManager : MonoBehaviour
    {
        [Header("Black Onyx Dungeon Settings")]
        [SerializeField] private int dungeonWidth = 20;
        [SerializeField] private int dungeonHeight = 20;
        [SerializeField] private bool debugMode = true;
        
        [Header("Floor Configuration")]
        [SerializeField] private FloorData[] floorConfigurations;
        
        // Floor numbering: -6 to 2 (B6 to Heaven)
        private const int MIN_FLOOR = -6;  // B6 (最下層)
        private const int MAX_FLOOR = 2;   // 天界
        private const int TOTAL_FLOORS = 8;
        
        // Current dungeon state
        private int currentFloor = -1; // B1から開始
        private Vector2Int playerPosition = Vector2Int.zero;
        private Dictionary<int, BlackOnyxFloor> floorCache = new Dictionary<int, BlackOnyxFloor>();
        private DungeonEntrance currentEntrance = DungeonEntrance.Ruins; // デフォルトは廃墟
        
        // Entrance system
        public enum DungeonEntrance
        {
            Graveyard,  // 墓場 → B1のみ
            Well,       // 井戸 → B5直行
            Ruins       // 廃墟 → 正規ルート
        }
        
        // Events
        public System.Action<int> OnFloorChanged;
        public System.Action<Vector2Int> OnPlayerMoved;
        public System.Action<DungeonCell> OnCellEntered;
        public System.Action<int> OnColorMazeEntered; // カラー迷路専用
        
        // Manager references
        private GameManager gameManager;
        
        // Properties
        public int CurrentFloor => currentFloor;
        public Vector2Int PlayerPosition => playerPosition;
        public Vector2Int DungeonSize => new Vector2Int(dungeonWidth, dungeonHeight);
        
        void Awake()
        {
            InitializeBlackOnyxDungeon();
        }
        
        /// <summary>
        /// ブラックオニキス準拠ダンジョンの初期化
        /// </summary>
        private void InitializeBlackOnyxDungeon()
        {
            gameManager = GameManager.Instance;
            
            // Initialize floor configurations
            InitializeFloorConfigurations();
            
            // Generate initial floor based on entrance
            SetDungeonEntrance(currentEntrance);
            
            Debug.Log($"🏰 Black Onyx Dungeon initialized - Starting at Floor {currentFloor}");
        }
        
        /// <summary>
        /// フロア設定の初期化
        /// </summary>
        private void InitializeFloorConfigurations()
        {
            floorConfigurations = new FloorData[TOTAL_FLOORS];
            
            // B6: カラー迷路フロア
            floorConfigurations[0] = new FloorData
            {
                floorNumber = -6,
                floorName = "地下6階（カラー迷路）",
                floorType = FloorType.ColorMaze,
                hasColorMaze = true,
                hasInvisibleWalls = true,
                hasOneWayWalls = true,
                roomDensity = 0.2f,
                difficulty = 6
            };
            
            // B5: 井戸直行フロア
            floorConfigurations[1] = new FloorData
            {
                floorNumber = -5,
                floorName = "地下5階（井戸）",
                floorType = FloorType.WellFloor,
                hasSpecialRoom = true,
                specialRoomType = "井戸の間",
                roomDensity = 0.3f,
                difficulty = 5
            };
            
            // B4: あまり意味のない階
            floorConfigurations[2] = new FloorData
            {
                floorNumber = -4,
                floorName = "地下4階",
                floorType = FloorType.Standard,
                roomDensity = 0.2f,
                difficulty = 4
            };
            
            // B3: 強敵フロア
            floorConfigurations[3] = new FloorData
            {
                floorNumber = -3,
                floorName = "地下3階（強敵）",
                floorType = FloorType.Dangerous,
                roomDensity = 0.4f,
                difficulty = 7
            };
            
            // B2: 標準フロア
            floorConfigurations[4] = new FloorData
            {
                floorNumber = -2,
                floorName = "地下2階",
                floorType = FloorType.Standard,
                roomDensity = 0.4f,
                difficulty = 3
            };
            
            // B1: 初心者フロア
            floorConfigurations[5] = new FloorData
            {
                floorNumber = -1,
                floorName = "地下1階（初心者）",
                floorType = FloorType.Beginner,
                roomDensity = 0.6f,
                difficulty = 1
            };
            
            // F1: ブラックタワー
            floorConfigurations[6] = new FloorData
            {
                floorNumber = 1,
                floorName = "地上1階（ブラックタワー）",
                floorType = FloorType.Tower,
                roomDensity = 0.3f,
                difficulty = 8
            };
            
            // F2: 天界
            floorConfigurations[7] = new FloorData
            {
                floorNumber = 2,
                floorName = "地上2階（天界）",
                floorType = FloorType.Heaven,
                hasSpecialRoom = true,
                specialRoomType = "ブラックオニキスの間",
                roomDensity = 0.5f,
                difficulty = 10
            };
        }
        
        /// <summary>
        /// ダンジョン入口の設定
        /// </summary>
        public void SetDungeonEntrance(DungeonEntrance entrance)
        {
            currentEntrance = entrance;
            
            switch (entrance)
            {
                case DungeonEntrance.Graveyard:
                    // 墓場 → B1のみ（地下1階だけのダンジョン）
                    currentFloor = -1;
                    playerPosition = new Vector2Int(1, 1);
                    GenerateFloor(-1, true); // 墓場モードで生成
                    break;
                    
                case DungeonEntrance.Well:
                    // 井戸 → B5直行
                    currentFloor = -5;
                    playerPosition = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2); // 井戸の中央
                    GenerateFloor(-5);
                    break;
                    
                case DungeonEntrance.Ruins:
                    // 廃墟 → 正規ルート（B1から開始）
                    currentFloor = -1;
                    playerPosition = new Vector2Int(1, 1);
                    GenerateFloor(-1);
                    break;
            }
            
            Debug.Log($"🚪 Entered dungeon via {entrance} - Floor {currentFloor}");
        }
        
        /// <summary>
        /// フロア生成
        /// </summary>
        private void GenerateFloor(int floorNumber, bool graveyardMode = false)
        {
            if (floorCache.ContainsKey(floorNumber))
            {
                Debug.Log($"🏰 Loading cached floor {floorNumber}");
                return;
            }
            
            if (floorNumber < MIN_FLOOR || floorNumber > MAX_FLOOR)
            {
                Debug.LogError($"❌ Invalid floor number: {floorNumber}");
                return;
            }
            
            var floorConfig = GetFloorConfiguration(floorNumber);
            var floor = new BlackOnyxFloor(dungeonWidth, dungeonHeight, floorNumber, floorConfig);
            
            // 特殊生成モード
            if (graveyardMode && floorNumber == -1)
            {
                floor.GenerateGraveyardFloor();
            }
            else
            {
                floor.Generate();
            }
            
            floorCache[floorNumber] = floor;
            
            Debug.Log($"🏰 Generated {floorConfig.floorName}");
        }
        
        /// <summary>
        /// フロア設定の取得
        /// </summary>
        private FloorData GetFloorConfiguration(int floorNumber)
        {
            int index = floorNumber - MIN_FLOOR;
            if (index >= 0 && index < floorConfigurations.Length)
            {
                return floorConfigurations[index];
            }
            
            // デフォルト設定
            return new FloorData
            {
                floorNumber = floorNumber,
                floorName = $"Floor {floorNumber}",
                floorType = FloorType.Standard,
                roomDensity = 0.3f,
                difficulty = Mathf.Abs(floorNumber)
            };
        }
        
        /// <summary>
        /// プレイヤー移動
        /// </summary>
        public bool MovePlayer(Vector2Int direction)
        {
            Vector2Int newPosition = playerPosition + direction;
            
            // 境界チェック
            if (newPosition.x < 0 || newPosition.x >= dungeonWidth ||
                newPosition.y < 0 || newPosition.y >= dungeonHeight)
            {
                return false;
            }
            
            var currentFloorData = GetCurrentFloor();
            if (currentFloorData == null) return false;
            
            DungeonCell targetCell = currentFloorData.GetCell(newPosition.x, newPosition.y);
            if (targetCell == null) return false;
            
            // 移動可能性チェック（一方通行壁、見えない壁対応）
            if (!CanMoveToCell(playerPosition, newPosition, targetCell))
            {
                return false;
            }
            
            // 移動実行
            playerPosition = newPosition;
            
            // セルを探索済みにマーク
            targetCell.isExplored = true;
            
            // イベント発火
            OnPlayerMoved?.Invoke(playerPosition);
            OnCellEntered?.Invoke(targetCell);
            
            // フロア移動チェック
            CheckFloorTransition(targetCell);
            
            // カラー迷路チェック
            if (currentFloor == -6 && targetCell.colorCode > 0)
            {
                OnColorMazeEntered?.Invoke(targetCell.colorCode);
            }
            
            return true;
        }
        
        /// <summary>
        /// セルへの移動可能性チェック（ギミック対応）
        /// </summary>
        private bool CanMoveToCell(Vector2Int from, Vector2Int to, DungeonCell targetCell)
        {
            if (!targetCell.IsWalkable()) return false;
            
            var currentFloorData = GetCurrentFloor();
            if (currentFloorData == null) return false;
            
            // 一方通行壁チェック
            if (currentFloorData.HasOneWayWall(from, to))
            {
                return false;
            }
            
            // 見えない壁チェック
            if (currentFloorData.HasInvisibleWall(from, to))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// フロア移動チェック
        /// </summary>
        private void CheckFloorTransition(DungeonCell cell)
        {
            int targetFloor = currentFloor;
            
            switch (cell.type)
            {
                case DungeonCellType.StairsUp:
                    targetFloor = currentFloor + 1;
                    break;
                    
                case DungeonCellType.StairsDown:
                    targetFloor = currentFloor - 1;
                    break;
                    
                case DungeonCellType.SpecialStairs:
                    // カラー迷路からブラックタワーへの特殊階段
                    if (currentFloor == -6)
                    {
                        targetFloor = 1; // ブラックタワーへ
                    }
                    break;
            }
            
            if (targetFloor != currentFloor && targetFloor >= MIN_FLOOR && targetFloor <= MAX_FLOOR)
            {
                ChangeFloor(targetFloor);
            }
        }
        
        /// <summary>
        /// フロア変更
        /// </summary>
        public void ChangeFloor(int newFloor)
        {
            if (newFloor < MIN_FLOOR || newFloor > MAX_FLOOR)
            {
                Debug.LogWarning($"⚠️ Cannot change to floor {newFloor} - out of range");
                return;
            }
            
            if (newFloor == currentFloor) return;
            
            int previousFloor = currentFloor;
            currentFloor = newFloor;
            
            // 新しいフロアを生成
            GenerateFloor(currentFloor);
            
            // プレイヤー位置を適切に設定
            SetPlayerPositionForNewFloor(previousFloor, newFloor);
            
            OnFloorChanged?.Invoke(currentFloor);
            OnPlayerMoved?.Invoke(playerPosition);
            
            var floorConfig = GetFloorConfiguration(currentFloor);
            Debug.Log($"🏰 Moved to {floorConfig.floorName}");
        }
        
        /// <summary>
        /// 新フロアでのプレイヤー位置設定
        /// </summary>
        private void SetPlayerPositionForNewFloor(int fromFloor, int toFloor)
        {
            var floorData = GetCurrentFloor();
            if (floorData == null) return;
            
            if (toFloor > fromFloor)
            {
                // 上の階に移動 → 階段下付近に配置
                playerPosition = floorData.GetEntrancePosition();
            }
            else
            {
                // 下の階に移動 → 階段上付近に配置
                playerPosition = floorData.GetExitPosition();
            }
        }
        
        /// <summary>
        /// 現在のフロアデータ取得
        /// </summary>
        public BlackOnyxFloor GetCurrentFloor()
        {
            return floorCache.ContainsKey(currentFloor) ? floorCache[currentFloor] : null;
        }
        
        /// <summary>
        /// 特定位置のセル取得
        /// </summary>
        public DungeonCell GetCellAt(Vector2Int position)
        {
            var floor = GetCurrentFloor();
            return floor?.GetCell(position.x, position.y);
        }
        
        /// <summary>
        /// 新規ゲーム初期化
        /// </summary>
        public void InitializeNewGame()
        {
            floorCache.Clear();
            SetDungeonEntrance(DungeonEntrance.Ruins); // デフォルトは廃墟から
            Debug.Log("🏰 New Black Onyx game initialized");
        }
        
        /// <summary>
        /// カラー迷路の順序チェック（B6専用）
        /// </summary>
        public bool CheckColorMazeSequence(int colorCode)
        {
            if (currentFloor != -6) return true;
            
            var floor = GetCurrentFloor();
            if (floor != null && floor.floorData.hasColorMaze)
            {
                return floor.ValidateColorSequence(colorCode);
            }
            
            return true;
        }
        
        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public string GetDebugInfo()
        {
            var floorConfig = GetFloorConfiguration(currentFloor);
            return $"Black Onyx Dungeon:\n" +
                   $"Current Floor: {floorConfig.floorName}\n" +
                   $"Player Position: {playerPosition}\n" +
                   $"Entrance: {currentEntrance}\n" +
                   $"Cached Floors: {floorCache.Count}\n" +
                   $"Floor Range: {MIN_FLOOR} to {MAX_FLOOR}";
        }
        
        /// <summary>
        /// フロア一覧の取得
        /// </summary>
        public FloorData[] GetAllFloorConfigurations()
        {
            return floorConfigurations;
        }
    }
    
    /// <summary>
    /// フロアデータ設定
    /// </summary>
    [System.Serializable]
    public class FloorData
    {
        public int floorNumber;
        public string floorName;
        public FloorType floorType;
        public float roomDensity = 0.3f;
        public int difficulty = 1;
        public bool hasColorMaze = false;
        public bool hasInvisibleWalls = false;
        public bool hasOneWayWalls = false;
        public bool hasSpecialRoom = false;
        public string specialRoomType = "";
    }
    
    /// <summary>
    /// フロアタイプ
    /// </summary>
    public enum FloorType
    {
        Beginner,   // 初心者向け
        Standard,   // 標準
        Dangerous,  // 危険
        ColorMaze,  // カラー迷路
        WellFloor,  // 井戸フロア
        Tower,      // ブラックタワー
        Heaven      // 天界
    }
}