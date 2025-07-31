using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BlackOnyxReborn
{
    /// <summary>
    /// オリジナル ブラックオニキス準拠のフロア生成・管理クラス
    /// カラー迷路、一方通行壁、見えない壁などの特殊ギミックを実装
    /// </summary>
    public class BlackOnyxFloor
    {
        // Floor properties
        public int width { get; private set; }
        public int height { get; private set; }
        public int floorNumber { get; private set; }
        public FloorData floorData { get; private set; }
        
        // Grid data
        private DungeonCell[,] cells;
        private bool[,] oneWayWalls; // 一方通行壁
        private bool[,] invisibleWalls; // 見えない壁
        
        // Color maze (B6専用)
        private int[] colorSequence = { 1, 2, 3, 4, 5, 6, 7, 8 }; // PC-8801カラーコード順
        private int currentColorIndex = 0;
        private Vector2Int[] coloredPositions;
        
        // Special rooms
        private Vector2Int entrancePosition;
        private Vector2Int exitPosition;
        private List<Vector2Int> specialRoomPositions = new List<Vector2Int>();
        
        public BlackOnyxFloor(int width, int height, int floorNumber, FloorData floorData)
        {
            this.width = width;
            this.height = height;
            this.floorNumber = floorNumber;
            this.floorData = floorData;
            
            // Initialize grids
            cells = new DungeonCell[width, height];
            oneWayWalls = new bool[width, height];
            invisibleWalls = new bool[width, height];
            
            // Initialize all cells as walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new DungeonCell(x, y, DungeonCellType.Wall);
                }
            }
        }
        
        /// <summary>
        /// フロア生成（標準）
        /// </summary>
        public void Generate()
        {
            switch (floorData.floorType)
            {
                case FloorType.ColorMaze:
                    GenerateColorMazeFloor();
                    break;
                case FloorType.WellFloor:
                    GenerateWellFloor();
                    break;
                case FloorType.Heaven:
                    GenerateHeavenFloor();
                    break;
                case FloorType.Tower:
                    GenerateTowerFloor();
                    break;
                default:
                    GenerateStandardFloor();
                    break;
            }
            
            // Add special walls and gimmicks
            if (floorData.hasOneWayWalls)
            {
                GenerateOneWayWalls();
            }
            
            if (floorData.hasInvisibleWalls)
            {
                GenerateInvisibleWalls();
            }
            
            // Place stairs
            PlaceStairs();
            
            // Place special rooms
            if (floorData.hasSpecialRoom)
            {
                PlaceSpecialRoom();
            }
        }
        
        /// <summary>
        /// カラー迷路フロア生成（B6専用）
        /// </summary>
        public void GenerateColorMazeFloor()
        {
            // B6は特殊なカラー迷路構造
            GenerateMazeStructure();
            
            // カラーコードを配置
            PlaceColorCodes();
            
            // 特殊階段（ブラックタワーへの直行）
            PlaceSpecialStairsToTower();
            
            Debug.Log("🌈 Color Maze Floor (B6) generated");
        }
        
        /// <summary>
        /// 井戸フロア生成（B5専用）
        /// </summary>
        public void GenerateWellFloor()
        {
            GenerateRoomBasedFloor();
            
            // 井戸の間を中央に配置
            Vector2Int wellRoom = new Vector2Int(width / 2, height / 2);
            CreateRoom(wellRoom.x - 2, wellRoom.y - 2, 5, 5);
            cells[wellRoom.x, wellRoom.y].type = DungeonCellType.SpecialRoom;
            cells[wellRoom.x, wellRoom.y].specialType = "井戸の間";
            specialRoomPositions.Add(wellRoom);
            
            Debug.Log("🏛️ Well Floor (B5) generated");
        }
        
        /// <summary>
        /// 天界フロア生成（地上2階専用）
        /// </summary>
        public void GenerateHeavenFloor()
        {
            GenerateOpenFloor();
            
            // ブラックオニキスの間を配置
            Vector2Int onyxRoom = new Vector2Int(width - 3, height - 3);
            CreateRoom(onyxRoom.x - 2, onyxRoom.y - 2, 5, 5);
            cells[onyxRoom.x, onyxRoom.y].type = DungeonCellType.SpecialRoom;
            cells[onyxRoom.x, onyxRoom.y].specialType = "ブラックオニキスの間";
            specialRoomPositions.Add(onyxRoom);
            
            Debug.Log("☁️ Heaven Floor (F2) generated");
        }
        
        /// <summary>
        /// ブラックタワーフロア生成（地上1階専用）
        /// </summary>
        public void GenerateTowerFloor()
        {
            GenerateTowerStructure();
            Debug.Log("🏰 Tower Floor (F1) generated");
        }
        
        /// <summary>
        /// 墓場フロア生成（B1特殊モード）
        /// </summary>
        public void GenerateGraveyardFloor()
        {
            // 墓場は小さなダンジョン
            GenerateSmallDungeonStructure();
            
            // 墓石を配置
            PlaceGravestones();
            
            Debug.Log("⚰️ Graveyard Floor (B1) generated");
        }
        
        /// <summary>
        /// 標準フロア生成
        /// </summary>
        private void GenerateStandardFloor()
        {
            GenerateRoomBasedFloor();
        }
        
        /// <summary>
        /// 迷路構造生成
        /// </summary>
        private void GenerateMazeStructure()
        {
            // シンプルな迷路アルゴリズム
            System.Random random = new System.Random();
            
            // 外周を壁に
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        cells[x, y].type = DungeonCellType.Wall;
                    }
                    else
                    {
                        cells[x, y].type = (random.NextDouble() < 0.4) ? DungeonCellType.Wall : DungeonCellType.Floor;
                    }
                }
            }
            
            // 通路を確保
            EnsureConnectivity();
        }
        
        /// <summary>
        /// 部屋ベースのフロア生成
        /// </summary>
        private void GenerateRoomBasedFloor()
        {
            System.Random random = new System.Random();
            
            // 部屋を配置
            int roomCount = Mathf.RoundToInt(width * height * floorData.roomDensity / 100f);
            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = random.Next(3, 6);
                int roomHeight = random.Next(3, 6);
                int roomX = random.Next(1, width - roomWidth - 1);
                int roomY = random.Next(1, height - roomHeight - 1);
                
                CreateRoom(roomX, roomY, roomWidth, roomHeight);
            }
            
            // 通路でつなぐ
            ConnectRooms();
        }
        
        /// <summary>
        /// オープンフロア生成（天界用）
        /// </summary>
        private void GenerateOpenFloor()
        {
            // 大部分をフロアに
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    cells[x, y].type = DungeonCellType.Floor;
                }
            }
            
            // いくつかの障害物を配置
            System.Random random = new System.Random();
            int obstacleCount = random.Next(5, 10);
            for (int i = 0; i < obstacleCount; i++)
            {
                int x = random.Next(2, width - 2);
                int y = random.Next(2, height - 2);
                cells[x, y].type = DungeonCellType.Wall;
            }
        }
        
        /// <summary>
        /// タワー構造生成
        /// </summary>
        private void GenerateTowerStructure()
        {
            // 中央に大きな部屋
            int centerX = width / 2;
            int centerY = height / 2;
            CreateRoom(centerX - 4, centerY - 4, 8, 8);
            
            // 周囲に小部屋
            CreateRoom(2, 2, 4, 4);
            CreateRoom(width - 6, 2, 4, 4);
            CreateRoom(2, height - 6, 4, 4);
            CreateRoom(width - 6, height - 6, 4, 4);
            
            // 部屋を通路で接続
            ConnectRooms();
        }
        
        /// <summary>
        /// 小さなダンジョン構造生成（墓場用）
        /// </summary>
        private void GenerateSmallDungeonStructure()
        {
            // 中央に小さな部屋群
            CreateRoom(width / 2 - 3, height / 2 - 3, 6, 6);
            CreateRoom(2, 2, 4, 3);
            CreateRoom(width - 6, height - 4, 4, 3);
            
            ConnectRooms();
        }
        
        /// <summary>
        /// 部屋作成
        /// </summary>
        private void CreateRoom(int startX, int startY, int roomWidth, int roomHeight)
        {
            for (int x = startX; x < startX + roomWidth && x < width; x++)
            {
                for (int y = startY; y < startY + roomHeight && y < height; y++)
                {
                    if (x >= 0 && y >= 0)
                    {
                        cells[x, y].type = DungeonCellType.Floor;
                    }
                }
            }
        }
        
        /// <summary>
        /// カラーコード配置（B6専用）
        /// </summary>
        private void PlaceColorCodes()
        {
            List<Vector2Int> floorPositions = new List<Vector2Int>();
            
            // フロアセルを収集
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].type == DungeonCellType.Floor)
                    {
                        floorPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            // カラーコードを配置
            System.Random random = new System.Random();
            coloredPositions = new Vector2Int[8];
            
            for (int i = 0; i < 8 && i < floorPositions.Count; i++)
            {
                int index = random.Next(floorPositions.Count);
                Vector2Int pos = floorPositions[index];
                
                cells[pos.x, pos.y].colorCode = colorSequence[i];
                coloredPositions[i] = pos;
                
                floorPositions.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// 一方通行壁生成
        /// </summary>
        private void GenerateOneWayWalls()
        {
            System.Random random = new System.Random();
            int wallCount = random.Next(3, 8);
            
            for (int i = 0; i < wallCount; i++)
            {
                int x = random.Next(1, width - 1);
                int y = random.Next(1, height - 1);
                
                if (cells[x, y].type == DungeonCellType.Floor)
                {
                    oneWayWalls[x, y] = true;
                }
            }
        }
        
        /// <summary>
        /// 見えない壁生成
        /// </summary>
        private void GenerateInvisibleWalls()
        {
            System.Random random = new System.Random();
            int wallCount = random.Next(2, 6);
            
            for (int i = 0; i < wallCount; i++)
            {
                int x = random.Next(1, width - 1);
                int y = random.Next(1, height - 1);
                
                if (cells[x, y].type == DungeonCellType.Floor)
                {
                    invisibleWalls[x, y] = true;
                }
            }
        }
        
        /// <summary>
        /// 階段配置
        /// </summary>
        private void PlaceStairs()
        {
            System.Random random = new System.Random();
            List<Vector2Int> floorPositions = GetFloorPositions();
            
            if (floorPositions.Count >= 2)
            {
                // 上り階段
                if (floorNumber < 2) // 天界まで
                {
                    var stairPos = floorPositions[random.Next(floorPositions.Count)];
                    cells[stairPos.x, stairPos.y].type = DungeonCellType.StairsUp;
                    exitPosition = stairPos;
                }
                
                // 下り階段
                if (floorNumber > -6) // B6まで
                {
                    var stairPos = floorPositions[random.Next(floorPositions.Count)];
                    while (stairPos == exitPosition && floorPositions.Count > 1)
                    {
                        stairPos = floorPositions[random.Next(floorPositions.Count)];
                    }
                    cells[stairPos.x, stairPos.y].type = DungeonCellType.StairsDown;
                    entrancePosition = stairPos;
                }
            }
        }
        
        /// <summary>
        /// 特殊階段配置（B6からブラックタワーへ）
        /// </summary>
        private void PlaceSpecialStairsToTower()
        {
            var floorPositions = GetFloorPositions();
            if (floorPositions.Count > 0)
            {
                System.Random random = new System.Random();
                var specialStairPos = floorPositions[random.Next(floorPositions.Count)];
                cells[specialStairPos.x, specialStairPos.y].type = DungeonCellType.SpecialStairs;
                cells[specialStairPos.x, specialStairPos.y].specialType = "ブラックタワーへの階段";
            }
        }
        
        /// <summary>
        /// 特殊部屋配置
        /// </summary>
        private void PlaceSpecialRoom()
        {
            var floorPositions = GetFloorPositions();
            if (floorPositions.Count > 0)
            {
                System.Random random = new System.Random();
                var roomPos = floorPositions[random.Next(floorPositions.Count)];
                cells[roomPos.x, roomPos.y].type = DungeonCellType.SpecialRoom;
                cells[roomPos.x, roomPos.y].specialType = floorData.specialRoomType;
                specialRoomPositions.Add(roomPos);
            }
        }
        
        /// <summary>
        /// 墓石配置
        /// </summary>
        private void PlaceGravestones()
        {
            System.Random random = new System.Random();
            var floorPositions = GetFloorPositions();
            int graveCount = random.Next(3, 6);
            
            for (int i = 0; i < graveCount && i < floorPositions.Count; i++)
            {
                var gravePos = floorPositions[random.Next(floorPositions.Count)];
                cells[gravePos.x, gravePos.y].type = DungeonCellType.SpecialRoom;
                cells[gravePos.x, gravePos.y].specialType = "墓石";
                floorPositions.Remove(gravePos);
            }
        }
        
        /// <summary>
        /// 接続性確保
        /// </summary>
        private void EnsureConnectivity()
        {
            // 簡単な接続確保（すべてのフロアセルから到達可能にする）
            var floorPositions = GetFloorPositions();
            if (floorPositions.Count < 2) return;
            
            // 基本的な通路を作る
            Vector2Int start = floorPositions[0];
            for (int i = 1; i < floorPositions.Count; i++)
            {
                CreatePath(start, floorPositions[i]);
                start = floorPositions[i];
            }
        }
        
        /// <summary>
        /// 部屋間接続
        /// </summary>
        private void ConnectRooms()
        {
            var floorPositions = GetFloorPositions();
            if (floorPositions.Count < 2) return;
            
            // 部屋の中心を見つけて接続
            var roomCenters = FindRoomCenters();
            for (int i = 0; i < roomCenters.Count - 1; i++)
            {
                CreatePath(roomCenters[i], roomCenters[i + 1]);
            }
        }
        
        /// <summary>
        /// 通路作成
        /// </summary>
        private void CreatePath(Vector2Int from, Vector2Int to)
        {
            Vector2Int current = from;
            
            // 水平移動
            while (current.x != to.x)
            {
                current.x += (current.x < to.x) ? 1 : -1;
                if (current.x >= 0 && current.x < width && current.y >= 0 && current.y < height)
                {
                    cells[current.x, current.y].type = DungeonCellType.Floor;
                }
            }
            
            // 垂直移動
            while (current.y != to.y)
            {
                current.y += (current.y < to.y) ? 1 : -1;
                if (current.x >= 0 && current.x < width && current.y >= 0 && current.y < height)
                {
                    cells[current.x, current.y].type = DungeonCellType.Floor;
                }
            }
        }
        
        /// <summary>
        /// 部屋中心位置検索
        /// </summary>
        private List<Vector2Int> FindRoomCenters()
        {
            List<Vector2Int> centers = new List<Vector2Int>();
            bool[,] visited = new bool[width, height];
            
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (cells[x, y].type == DungeonCellType.Floor && !visited[x, y])
                    {
                        var roomCells = FloodFill(x, y, visited);
                        if (roomCells.Count > 6) // 十分大きな部屋のみ
                        {
                            Vector2Int center = CalculateCenter(roomCells);
                            centers.Add(center);
                        }
                    }
                }
            }
            
            return centers;
        }
        
        /// <summary>
        /// フラッドフィル
        /// </summary>
        private List<Vector2Int> FloodFill(int startX, int startY, bool[,] visited)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            
            while (queue.Count > 0)
            {
                Vector2Int pos = queue.Dequeue();
                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;
                if (visited[pos.x, pos.y] || this.cells[pos.x, pos.y].type != DungeonCellType.Floor) continue;
                
                visited[pos.x, pos.y] = true;
                cells.Add(pos);
                
                queue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                queue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
            }
            
            return cells;
        }
        
        /// <summary>
        /// 中心位置計算
        /// </summary>
        private Vector2Int CalculateCenter(List<Vector2Int> positions)
        {
            if (positions.Count == 0) return Vector2Int.zero;
            
            int sumX = positions.Sum(p => p.x);
            int sumY = positions.Sum(p => p.y);
            
            return new Vector2Int(sumX / positions.Count, sumY / positions.Count);
        }
        
        /// <summary>
        /// フロア位置取得
        /// </summary>
        private List<Vector2Int> GetFloorPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y].type == DungeonCellType.Floor)
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return positions;
        }
        
        // Public methods for access
        
        /// <summary>
        /// セル取得
        /// </summary>
        public DungeonCell GetCell(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return cells[x, y];
            }
            return null;
        }
        
        /// <summary>
        /// 一方通行壁チェック
        /// </summary>
        public bool HasOneWayWall(Vector2Int from, Vector2Int to)
        {
            if (to.x >= 0 && to.x < width && to.y >= 0 && to.y < height)
            {
                return oneWayWalls[to.x, to.y];
            }
            return false;
        }
        
        /// <summary>
        /// 見えない壁チェック
        /// </summary>
        public bool HasInvisibleWall(Vector2Int from, Vector2Int to)
        {
            if (to.x >= 0 && to.x < width && to.y >= 0 && to.y < height)
            {
                return invisibleWalls[to.x, to.y];
            }
            return false;
        }
        
        /// <summary>
        /// カラー順序検証（B6専用）
        /// </summary>
        public bool ValidateColorSequence(int colorCode)
        {
            if (colorSequence[currentColorIndex] == colorCode)
            {
                currentColorIndex++;
                if (currentColorIndex >= colorSequence.Length)
                {
                    currentColorIndex = 0; // リセット
                    return true; // 完全クリア
                }
                return true; // 正解
            }
            else
            {
                currentColorIndex = 0; // リセット
                return false; // 不正解
            }
        }
        
        /// <summary>
        /// 入口位置取得
        /// </summary>
        public Vector2Int GetEntrancePosition()
        {
            return entrancePosition != Vector2Int.zero ? entrancePosition : new Vector2Int(1, 1);
        }
        
        /// <summary>
        /// 出口位置取得
        /// </summary>
        public Vector2Int GetExitPosition()
        {
            return exitPosition != Vector2Int.zero ? exitPosition : new Vector2Int(width - 2, height - 2);
        }
        
        /// <summary>
        /// 特殊部屋位置リスト取得
        /// </summary>
        public List<Vector2Int> GetSpecialRoomPositions()
        {
            return new List<Vector2Int>(specialRoomPositions);
        }
        
        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public string GetDebugInfo()
        {
            var floorPositions = GetFloorPositions();
            return $"BlackOnyxFloor {floorNumber} ({floorData.floorName}):\n" +
                   $"Size: {width}x{height}\n" +
                   $"Floor cells: {floorPositions.Count}\n" +
                   $"Special rooms: {specialRoomPositions.Count}\n" +
                   $"Color maze: {floorData.hasColorMaze}\n" +
                   $"One-way walls: {floorData.hasOneWayWalls}\n" +
                   $"Invisible walls: {floorData.hasInvisibleWalls}";
        }
    }
    
    /// <summary>
    /// ダンジョンセル拡張（ブラックオニキス用）
    /// </summary>
    public class DungeonCell
    {
        public int x, y;
        public DungeonCellType type;
        public bool isExplored = false;
        public bool isVisible = false;
        public int colorCode = 0; // カラー迷路用
        public string specialType = ""; // 特殊部屋・オブジェクト用
        
        public DungeonCell(int x, int y, DungeonCellType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
        
        public bool IsWalkable()
        {
            return type == DungeonCellType.Floor || 
                   type == DungeonCellType.StairsUp || 
                   type == DungeonCellType.StairsDown ||
                   type == DungeonCellType.SpecialStairs ||
                   type == DungeonCellType.SpecialRoom;
        }
        
        public char GetDisplayChar()
        {
            switch (type)
            {
                case DungeonCellType.Wall: return '#';
                case DungeonCellType.Floor: return colorCode > 0 ? colorCode.ToString()[0] : '.';
                case DungeonCellType.StairsUp: return '<';
                case DungeonCellType.StairsDown: return '>';
                case DungeonCellType.SpecialStairs: return '≡';
                case DungeonCellType.SpecialRoom: return specialType.Contains("井戸") ? '○' : 
                                                       specialType.Contains("ブラックオニキス") ? '◆' :
                                                       specialType.Contains("墓石") ? '†' : '□';
                default: return '?';
            }
        }
    }
    
    /// <summary>
    /// ダンジョンセルタイプ（拡張版）
    /// </summary>
    public enum DungeonCellType
    {
        Wall,
        Floor,
        StairsUp,
        StairsDown,
        SpecialStairs, // 特殊階段（カラー迷路→ブラックタワー等）
        SpecialRoom    // 特殊部屋（井戸の間、ブラックオニキスの間等）
    }
}