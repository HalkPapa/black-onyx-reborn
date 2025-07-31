using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ダンジョンマップのテキストベース描画システム
    /// </summary>
    public class DungeonMapRenderer : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private TextMeshProUGUI mapText;
        [SerializeField] private int viewRadius = 5; // プレイヤー周囲の表示範囲
        [SerializeField] private bool showFullMap = false; // デバッグ用：全マップ表示
        [SerializeField] private bool enableFogOfWar = true; // 未探索エリアの表示制御
        
        [Header("Visual Settings")]
        [SerializeField] private Color exploredColor = Color.white;
        [SerializeField] private Color unexploredColor = Color.gray;
        [SerializeField] private Color playerColor = Color.yellow;
        [SerializeField] private Color wallColor = Color.gray;
        [SerializeField] private Color floorColor = Color.white;
        
        [Header("Map Characters")]
        [SerializeField] private string wallChar = "██";
        [SerializeField] private string floorChar = "  ";
        [SerializeField] private string playerChar = "🚶";
        [SerializeField] private string entranceChar = "🚪";
        [SerializeField] private string stairsUpChar = "⬆️";
        [SerializeField] private string stairsDownChar = "⬇️";
        [SerializeField] private string unknownChar = "▓▓";
        
        // Manager references
        private DungeonManager dungeonManager;
        private GameManager gameManager;
        
        // Display state
        private Vector2Int lastPlayerPosition = Vector2Int.zero;
        private int lastFloorNumber = -1;
        private bool needsRedraw = true;
        
        void Start()
        {
            InitializeRenderer();
        }
        
        void Update()
        {
            UpdateMapDisplay();
        }
        
        /// <summary>
        /// レンダラーの初期化
        /// </summary>
        private void InitializeRenderer()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
            }
            
            // Create map text component if not assigned
            if (mapText == null)
            {
                CreateMapTextComponent();
            }
            
            // Configure text settings
            SetupMapTextSettings();
            
            // Subscribe to dungeon events
            if (dungeonManager != null)
            {
                dungeonManager.OnPlayerMoved += OnPlayerMoved;
                dungeonManager.OnFloorChanged += OnFloorChanged;
            }
            
            Debug.Log("🗺️ Dungeon Map Renderer initialized");
        }
        
        /// <summary>
        /// マップテキストコンポーネントの作成
        /// </summary>
        private void CreateMapTextComponent()
        {
            GameObject textObj = new GameObject("MapText");
            textObj.transform.SetParent(transform, false);
            
            mapText = textObj.AddComponent<TextMeshProUGUI>();
            
            // Setup RectTransform
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        
        /// <summary>
        /// マップテキストの設定
        /// </summary>
        private void SetupMapTextSettings()
        {
            if (mapText == null) return;
            
            mapText.font = Resources.GetBuiltinResource<TMP_FontAsset>("LiberationSans SDF");
            mapText.fontSize = 12f;
            mapText.color = Color.white;
            mapText.alignment = TextAlignmentOptions.TopLeft;
            mapText.fontStyle = FontStyles.Normal;
            mapText.enableWordWrapping = false;
            mapText.overflowMode = TextOverflowModes.Overflow;
        }
        
        /// <summary>
        /// マップ表示の更新
        /// </summary>
        private void UpdateMapDisplay()
        {
            if (dungeonManager == null || mapText == null)
                return;
            
            // Check if redraw is needed
            Vector2Int currentPlayerPos = dungeonManager.GetPlayerPosition();
            int currentFloor = dungeonManager.GetCurrentFloorNumber();
            
            if (needsRedraw || 
                currentPlayerPos != lastPlayerPosition || 
                currentFloor != lastFloorNumber)
            {
                RedrawMap();
                lastPlayerPosition = currentPlayerPos;
                lastFloorNumber = currentFloor;
                needsRedraw = false;
            }
        }
        
        /// <summary>
        /// マップの再描画
        /// </summary>
        private void RedrawMap()
        {
            if (showFullMap)
            {
                RenderFullMap();
            }
            else
            {
                RenderViewportMap();
            }
        }
        
        /// <summary>
        /// 全マップの描画（デバッグ用）
        /// </summary>
        private void RenderFullMap()
        {
            DungeonFloor floor = dungeonManager.GetCurrentFloor();
            if (floor == null) return;
            
            Vector2Int dungeonSize = dungeonManager.GetDungeonSize();
            Vector2Int playerPos = dungeonManager.GetPlayerPosition();
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Floor {dungeonManager.GetCurrentFloorNumber()} - Full Map View");
            sb.AppendLine($"Player Position: ({playerPos.x}, {playerPos.y})");
            sb.AppendLine();
            
            // Render from top to bottom (reverse Y for display)
            for (int y = dungeonSize.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < dungeonSize.x; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    if (pos == playerPos)
                    {
                        sb.Append(playerChar);
                    }
                    else
                    {
                        DungeonCell cell = dungeonManager.GetCellAt(pos);
                        sb.Append(GetCellDisplayString(cell, true));
                    }
                }
                sb.AppendLine();
            }
            
            mapText.text = sb.ToString();
        }
        
        /// <summary>
        /// ビューポート範囲のマップ描画
        /// </summary>
        private void RenderViewportMap()
        {
            Vector2Int playerPos = dungeonManager.GetPlayerPosition();
            DungeonCell[,] viewCells = dungeonManager.GetCellsInRange(playerPos, viewRadius);
            
            if (viewCells == null) return;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Floor {dungeonManager.GetCurrentFloorNumber()}");
            sb.AppendLine($"Position: ({playerPos.x}, {playerPos.y})");
            sb.AppendLine();
            
            int size = viewRadius * 2 + 1;
            int centerX = viewRadius;
            int centerY = viewRadius;
            
            // Render from top to bottom
            for (int y = size - 1; y >= 0; y--)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x == centerX && y == centerY)
                    {
                        // Player position
                        sb.Append(playerChar);
                    }
                    else
                    {
                        DungeonCell cell = viewCells[x, y];
                        bool isExplored = cell != null && (!enableFogOfWar || cell.isExplored);
                        sb.Append(GetCellDisplayString(cell, isExplored));
                    }
                }
                sb.AppendLine();
            }
            
            // Add legend
            sb.AppendLine();
            sb.AppendLine("Legend:");
            sb.AppendLine($"{wallChar} Wall  {floorChar} Floor  {playerChar} You");
            sb.AppendLine($"{entranceChar} Entrance  {stairsUpChar} Up  {stairsDownChar} Down");
            
            mapText.text = sb.ToString();
        }
        
        /// <summary>
        /// セルの表示文字列取得
        /// </summary>
        private string GetCellDisplayString(DungeonCell cell, bool isExplored)
        {
            if (cell == null || (!isExplored && enableFogOfWar))
            {
                return unknownChar;
            }
            
            switch (cell.type)
            {
                case DungeonCellType.Wall:
                    return wallChar;
                case DungeonCellType.Floor:
                    return floorChar;
                case DungeonCellType.Entrance:
                    return entranceChar;
                case DungeonCellType.StairsUp:
                    return stairsUpChar;
                case DungeonCellType.StairsDown:
                    return stairsDownChar;
                case DungeonCellType.Door:
                    return "🚪";
                case DungeonCellType.Treasure:
                    return "💰";
                case DungeonCellType.Trap:
                    return "🕳️";
                default:
                    return "??";
            }
        }
        
        /// <summary>
        /// プレイヤー移動時のイベント処理
        /// </summary>
        private void OnPlayerMoved(Vector2Int newPosition)
        {
            // Mark current cell as explored
            DungeonCell currentCell = dungeonManager.GetCellAt(newPosition);
            if (currentCell != null)
            {
                currentCell.isExplored = true;
                
                // Also mark adjacent cells as explored (limited visibility)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int adjacentPos = newPosition + new Vector2Int(dx, dy);
                        DungeonCell adjacentCell = dungeonManager.GetCellAt(adjacentPos);
                        if (adjacentCell != null)
                        {
                            adjacentCell.isExplored = true;
                        }
                    }
                }
            }
            
            needsRedraw = true;
        }
        
        /// <summary>
        /// フロア変更時のイベント処理
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            needsRedraw = true;
        }
        
        /// <summary>
        /// 表示モードの切り替え
        /// </summary>
        public void ToggleFullMapView()
        {
            showFullMap = !showFullMap;
            needsRedraw = true;
            Debug.Log($"🗺️ Full map view: {(showFullMap ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// 戦場の霧の切り替え
        /// </summary>
        public void ToggleFogOfWar()
        {
            enableFogOfWar = !enableFogOfWar;
            needsRedraw = true;
            Debug.Log($"🌫️ Fog of war: {(enableFogOfWar ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// ビュー半径の設定
        /// </summary>
        public void SetViewRadius(int radius)
        {
            viewRadius = Mathf.Clamp(radius, 1, 10);
            needsRedraw = true;
            Debug.Log($"🗺️ View radius set to: {viewRadius}");
        }
        
        /// <summary>
        /// マップ文字設定の更新
        /// </summary>
        public void UpdateMapCharacters(string wall, string floor, string player)
        {
            wallChar = wall ?? wallChar;
            floorChar = floor ?? floorChar;
            playerChar = player ?? playerChar;
            needsRedraw = true;
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (dungeonManager != null)
            {
                dungeonManager.OnPlayerMoved -= OnPlayerMoved;
                dungeonManager.OnFloorChanged -= OnFloorChanged;
            }
        }
    }
}