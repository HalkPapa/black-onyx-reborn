using UnityEngine;

namespace BlackOnyxReborn
{
    /// <summary>
    /// 既存のDungeonManagerと新しいBlackOnyxDungeonManagerの橋渡し
    /// 既存のUIやシステムが新しいダンジョンシステムとシームレスに動作するためのアダプター
    /// </summary>
    public class DungeonManagerBridge : MonoBehaviour
    {
        private GameManager gameManager;
        private BlackOnyxDungeonManager blackOnyxDungeonManager;
        
        void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                blackOnyxDungeonManager = gameManager.BlackOnyxDungeonManager;
            }
        }
        
        /// <summary>
        /// 既存のDungeonManagerAPIを新しいBlackOnyxDungeonManagerに転送
        /// </summary>
        public int GetCurrentFloorNumber()
        {
            return blackOnyxDungeonManager?.CurrentFloor ?? 1;
        }
        
        public Vector2Int GetPlayerPosition()
        {
            return blackOnyxDungeonManager?.PlayerPosition ?? Vector2Int.zero;
        }
        
        public Vector2Int GetDungeonSize()
        {
            return blackOnyxDungeonManager?.DungeonSize ?? new Vector2Int(20, 20);
        }
        
        public bool MovePlayer(Vector2Int direction)
        {
            return blackOnyxDungeonManager?.MovePlayer(direction) ?? false;
        }
        
        public DungeonCell GetCellAt(Vector2Int position)
        {
            return blackOnyxDungeonManager?.GetCellAt(position);
        }
        
        public void ChangeFloor(int newFloor)
        {
            blackOnyxDungeonManager?.ChangeFloor(newFloor);
        }
        
        public Vector2Int GetRandomWalkablePosition()
        {
            // 現在のフロアからランダムな歩行可能位置を取得
            var currentFloor = blackOnyxDungeonManager?.GetCurrentFloor();
            if (currentFloor == null) return Vector2Int.zero;
            
            System.Random random = new System.Random();
            int attempts = 100;
            
            while (attempts > 0)
            {
                int x = random.Next(1, blackOnyxDungeonManager.DungeonSize.x - 1);
                int y = random.Next(1, blackOnyxDungeonManager.DungeonSize.y - 1);
                
                var cell = currentFloor.GetCell(x, y);
                if (cell != null && cell.IsWalkable())
                {
                    return new Vector2Int(x, y);
                }
                
                attempts--;
            }
            
            return blackOnyxDungeonManager?.PlayerPosition ?? Vector2Int.zero;
        }
        
        /// <summary>
        /// フロア変更イベントを既存システム向けに転送
        /// </summary>
        public System.Action<int> OnFloorChanged 
        { 
            get => blackOnyxDungeonManager?.OnFloorChanged; 
            set { if (blackOnyxDungeonManager != null) blackOnyxDungeonManager.OnFloorChanged = value; }
        }
        
        public System.Action<Vector2Int> OnPlayerMoved 
        { 
            get => blackOnyxDungeonManager?.OnPlayerMoved; 
            set { if (blackOnyxDungeonManager != null) blackOnyxDungeonManager.OnPlayerMoved = value; }
        }
        
        public System.Action<DungeonCell> OnCellEntered 
        { 
            get => blackOnyxDungeonManager?.OnCellEntered; 
            set { if (blackOnyxDungeonManager != null) blackOnyxDungeonManager.OnCellEntered = value; }
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return blackOnyxDungeonManager?.GetDebugInfo() ?? "BlackOnyxDungeonManager not available";
        }
        
        /// <summary>
        /// 入口設定（Black Onyx固有機能）
        /// </summary>
        public void SetDungeonEntrance(BlackOnyxDungeonManager.DungeonEntrance entrance)
        {
            blackOnyxDungeonManager?.SetDungeonEntrance(entrance);
        }
        
        /// <summary>
        /// カラー迷路順序チェック（Black Onyx固有機能）
        /// </summary>
        public bool CheckColorMazeSequence(int colorCode)
        {
            return blackOnyxDungeonManager?.CheckColorMazeSequence(colorCode) ?? true;
        }
        
        /// <summary>
        /// 新規ゲーム初期化
        /// </summary>
        public void InitializeNewGame()
        {
            blackOnyxDungeonManager?.InitializeNewGame();
        }
    }
}