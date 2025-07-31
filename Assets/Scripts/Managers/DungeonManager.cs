using UnityEngine;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ダンジョンシステムの管理を行うマネージャー
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        [SerializeField] private int dungeonWidth = 16;
        [SerializeField] private int dungeonHeight = 16;
        [SerializeField] private int maxFloors = 10;
        
        [Header("Generation Settings")]
        [SerializeField] private float roomDensity = 0.3f;
        [SerializeField] private int minRoomSize = 3;
        [SerializeField] private int maxRoomSize = 8;
        [SerializeField] private int corridorWidth = 1;
        
        // Current dungeon state
        private int currentFloor = 1;
        private Vector2Int playerPosition = Vector2Int.zero;
        private DungeonFloor[,] dungeonData;
        private Dictionary<int, DungeonFloor> floorCache = new Dictionary<int, DungeonFloor>();
        
        // Events
        public System.Action<int> OnFloorChanged;
        public System.Action<Vector2Int> OnPlayerMoved;
        public System.Action<DungeonCell> OnCellEntered;
        
        void Awake()
        {
            InitializeDungeon();
        }
        
        /// <summary>
        /// ダンジョン初期化
        /// </summary>
        private void InitializeDungeon()
        {
            dungeonData = new DungeonFloor[dungeonWidth, dungeonHeight];
            GenerateFloor(currentFloor);
            
            // Find starting position (entrance)
            playerPosition = FindEntrancePosition();
            
            Debug.Log($"🏰 Dungeon Manager initialized - Floor {currentFloor}");
        }
        
        /// <summary>
        /// 新規ゲーム初期化
        /// </summary>
        public void InitializeNewGame()
        {
            // Reset to floor 1
            currentFloor = 1;
            floorCache.Clear();
            
            // Generate initial floor
            GenerateFloor(currentFloor);
            
            // Reset player position
            playerPosition = FindEntrancePosition();
            
            Debug.Log("🏰 New game initialized in dungeon");
        }
        
        /// <summary>
        /// フロア生成
        /// </summary>
        private void GenerateFloor(int floorNumber)
        {
            if (floorCache.ContainsKey(floorNumber))
            {
                Debug.Log($"🏰 Loading cached floor {floorNumber}");
                return;
            }
            
            DungeonFloor floor = new DungeonFloor(dungeonWidth, dungeonHeight, floorNumber);
            floor.Generate(roomDensity, minRoomSize, maxRoomSize, corridorWidth);
            
            floorCache[floorNumber] = floor;
            Debug.Log($"🏰 Generated floor {floorNumber}");
        }
        
        /// <summary>
        /// 入口位置の検索
        /// </summary>
        private Vector2Int FindEntrancePosition()
        {
            DungeonFloor currentFloorData = GetCurrentFloor();
            if (currentFloorData != null)
            {
                return currentFloorData.GetEntrancePosition();
            }
            
            return new Vector2Int(1, 1); // Fallback position
        }
        
        /// <summary>
        /// 現在のフロアデータ取得
        /// </summary>
        public DungeonFloor GetCurrentFloor()
        {
            return floorCache.ContainsKey(currentFloor) ? floorCache[currentFloor] : null;
        }
        
        /// <summary>
        /// 指定位置のセル取得
        /// </summary>
        public DungeonCell GetCellAt(Vector2Int position)
        {
            DungeonFloor floor = GetCurrentFloor();
            return floor?.GetCell(position.x, position.y);
        }
        
        /// <summary>
        /// プレイヤー移動
        /// </summary>
        public bool MovePlayer(Vector2Int direction)
        {
            Vector2Int newPosition = playerPosition + direction;
            
            // Bounds check
            if (newPosition.x < 0 || newPosition.x >= dungeonWidth ||
                newPosition.y < 0 || newPosition.y >= dungeonHeight)
            {
                return false;
            }
            
            DungeonCell targetCell = GetCellAt(newPosition);
            if (targetCell == null || !targetCell.IsWalkable())
            {
                return false;
            }
            
            // Move player
            playerPosition = newPosition;
            OnPlayerMoved?.Invoke(playerPosition);
            OnCellEntered?.Invoke(targetCell);
            
            // Check for floor transitions
            CheckFloorTransition(targetCell);
            
            return true;
        }
        
        /// <summary>
        /// フロア移動チェック
        /// </summary>
        private void CheckFloorTransition(DungeonCell cell)
        {
            switch (cell.type)
            {
                case DungeonCellType.StairsUp:
                    if (currentFloor > 1)
                    {
                        ChangeFloor(currentFloor - 1);
                    }
                    break;
                    
                case DungeonCellType.StairsDown:
                    if (currentFloor < maxFloors)
                    {
                        ChangeFloor(currentFloor + 1);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// フロア変更
        /// </summary>
        public void ChangeFloor(int newFloor)
        {
            if (newFloor < 1 || newFloor > maxFloors)
                return;
                
            currentFloor = newFloor;
            GenerateFloor(currentFloor);
            
            // Update player position for new floor
            if (newFloor > currentFloor) // Going down
            {
                playerPosition = FindEntrancePosition();
            }
            else // Going up
            {
                playerPosition = FindExitPosition();
            }
            
            OnFloorChanged?.Invoke(currentFloor);
            OnPlayerMoved?.Invoke(playerPosition);
        }
        
        /// <summary>
        /// 出口位置の検索
        /// </summary>
        private Vector2Int FindExitPosition()
        {
            DungeonFloor currentFloorData = GetCurrentFloor();
            if (currentFloorData != null)
            {
                return currentFloorData.GetExitPosition();
            }
            
            return new Vector2Int(dungeonWidth - 2, dungeonHeight - 2); // Fallback position
        }
        
        /// <summary>
        /// 現在のフロア番号取得
        /// </summary>
        public int GetCurrentFloorNumber()
        {
            return currentFloor;
        }
        
        /// <summary>
        /// プレイヤー位置取得
        /// </summary>
        public Vector2Int GetPlayerPosition()
        {
            return playerPosition;
        }
        
        /// <summary>
        /// ダンジョンサイズ取得
        /// </summary>
        public Vector2Int GetDungeonSize()
        {
            return new Vector2Int(dungeonWidth, dungeonHeight);
        }
        
        /// <summary>
        /// 指定範囲のセル取得
        /// </summary>
        public DungeonCell[,] GetCellsInRange(Vector2Int center, int range)
        {
            int size = range * 2 + 1;
            DungeonCell[,] cells = new DungeonCell[size, size];
            
            DungeonFloor floor = GetCurrentFloor();
            if (floor == null) return cells;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2Int worldPos = center + new Vector2Int(x - range, y - range);
                    cells[x, y] = floor.GetCell(worldPos.x, worldPos.y);
                }
            }
            
            return cells;
        }
        
        /// <summary>
        /// ランダム位置の空いているセル取得
        /// </summary>
        public Vector2Int GetRandomWalkablePosition()
        {
            DungeonFloor floor = GetCurrentFloor();
            if (floor == null) return Vector2Int.zero;
            
            int attempts = 100;
            while (attempts > 0)
            {
                Vector2Int randomPos = new Vector2Int(
                    Random.Range(1, dungeonWidth - 1),
                    Random.Range(1, dungeonHeight - 1)
                );
                
                DungeonCell cell = floor.GetCell(randomPos.x, randomPos.y);
                if (cell != null && cell.IsWalkable())
                {
                    return randomPos;
                }
                
                attempts--;
            }
            
            return playerPosition; // Fallback
        }
        
        /// <summary>
        /// デバッグ情報表示
        /// </summary>
        public void DebugPrintFloor()
        {
            DungeonFloor floor = GetCurrentFloor();
            if (floor != null)
            {
                floor.DebugPrint();
            }
        }
    }
    
    /// <summary>
    /// ダンジョンフロアクラス
    /// </summary>
    public class DungeonFloor
    {
        public int width, height, floorNumber;
        private DungeonCell[,] cells;
        private Vector2Int entrancePos, exitPos;
        
        public DungeonFloor(int w, int h, int floor)
        {
            width = w;
            height = h;
            floorNumber = floor;
            cells = new DungeonCell[width, height];
        }
        
        public void Generate(float roomDensity, int minRoomSize, int maxRoomSize, int corridorWidth)
        {
            // Initialize all cells as walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new DungeonCell(DungeonCellType.Wall, x, y);
                }
            }
            
            // Generate basic walkable areas
            GenerateBasicLayout();
            
            // Place entrance and exit
            PlaceEntranceAndExit();
        }
        
        private void GenerateBasicLayout()
        {
            // Create a simple layout with some rooms and corridors
            for (int x = 1; x < width - 1; x += 2)
            {
                for (int y = 1; y < height - 1; y += 2)
                {
                    if (Random.Range(0f, 1f) < 0.6f)
                    {
                        cells[x, y] = new DungeonCell(DungeonCellType.Floor, x, y);
                        
                        // Add some adjacent floor tiles
                        if (Random.Range(0f, 1f) < 0.5f && x + 1 < width - 1)
                            cells[x + 1, y] = new DungeonCell(DungeonCellType.Floor, x + 1, y);
                        if (Random.Range(0f, 1f) < 0.5f && y + 1 < height - 1)
                            cells[x, y + 1] = new DungeonCell(DungeonCellType.Floor, x, y + 1);
                    }
                }
            }
        }
        
        private void PlaceEntranceAndExit()
        {
            // Place entrance at top-left area
            entrancePos = new Vector2Int(1, 1);
            cells[1, 1] = new DungeonCell(DungeonCellType.Entrance, 1, 1);
            
            // Place exit at bottom-right area
            exitPos = new Vector2Int(width - 2, height - 2);
            cells[width - 2, height - 2] = new DungeonCell(DungeonCellType.StairsDown, width - 2, height - 2);
        }
        
        public DungeonCell GetCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return null;
            return cells[x, y];
        }
        
        public Vector2Int GetEntrancePosition() => entrancePos;
        public Vector2Int GetExitPosition() => exitPos;
        
        public void DebugPrint()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Floor {floorNumber}:");
            
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    DungeonCell cell = cells[x, y];
                    switch (cell.type)
                    {
                        case DungeonCellType.Wall: sb.Append("██"); break;
                        case DungeonCellType.Floor: sb.Append("  "); break;
                        case DungeonCellType.Entrance: sb.Append("🚪"); break;
                        case DungeonCellType.StairsUp: sb.Append("⬆️"); break;
                        case DungeonCellType.StairsDown: sb.Append("⬇️"); break;
                        default: sb.Append("??"); break;
                    }
                }
                sb.AppendLine();
            }
            
            Debug.Log(sb.ToString());
        }
    }
    
    /// <summary>
    /// ダンジョンセルクラス
    /// </summary>
    public class DungeonCell
    {
        public DungeonCellType type;
        public Vector2Int position;
        public bool hasItem = false;
        public bool hasEnemy = false;
        public bool isExplored = false;
        
        public DungeonCell(DungeonCellType cellType, int x, int y)
        {
            type = cellType;
            position = new Vector2Int(x, y);
        }
        
        public bool IsWalkable()
        {
            return type != DungeonCellType.Wall;
        }
        
        public char GetDisplayChar()
        {
            switch (type)
            {
                case DungeonCellType.Wall: return '#';
                case DungeonCellType.Floor: return '.';
                case DungeonCellType.Entrance: return 'E';
                case DungeonCellType.StairsUp: return '<';
                case DungeonCellType.StairsDown: return '>';
                default: return '?';
            }
        }
    }
    
    /// <summary>
    /// ダンジョンセルタイプ
    /// </summary>
    public enum DungeonCellType
    {
        Wall,
        Floor,
        Entrance,
        StairsUp,
        StairsDown,
        Door,
        Treasure,
        Trap
    }
}