using UnityEngine;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ダンジョン内に配置されるアイテムオブジェクト
    /// </summary>
    public class Item : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;
        [SerializeField] private bool autoPickup = true;
        [SerializeField] private float pickupRange = 1f;
        
        // State
        private Vector2Int position;
        private bool isPickedUp = false;
        
        // Manager references
        private GameManager gameManager;
        private DungeonManager dungeonManager;
        private ItemManager itemManager;
        
        // Events
        public System.Action<Item> OnItemPickedUp;
        
        // Properties
        public ItemData Data => itemData;
        public int Quantity => quantity;
        public Vector2Int Position => position;
        public bool IsPickedUp => isPickedUp;
        
        void Start()
        {
            InitializeItem();
        }
        
        void Update()
        {
            if (!isPickedUp && autoPickup)
            {
                CheckPlayerProximity();
            }
        }
        
        /// <summary>
        /// アイテムの初期化
        /// </summary>
        private void InitializeItem()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                itemManager = gameManager.GetComponent<ItemManager>();
            }
            
            // Set name for identification
            if (itemData != null)
            {
                gameObject.name = $"Item_{itemData.itemName}_{quantity}";
            }
            
            Debug.Log($"💎 Item {itemData?.itemName} (x{quantity}) initialized at {position}");
        }
        
        /// <summary>
        /// データからアイテムを初期化
        /// </summary>
        public void InitializeWithData(ItemData data, Vector2Int spawnPosition, int itemQuantity = 1)
        {
            itemData = data;
            position = spawnPosition;
            quantity = itemQuantity;
            InitializeItem();
        }
        
        /// <summary>
        /// プレイヤー接近チェック
        /// </summary>
        private void CheckPlayerProximity()
        {
            if (dungeonManager == null) return;
            
            Vector2Int playerPosition = dungeonManager.GetPlayerPosition();
            float distance = Vector2Int.Distance(position, playerPosition);
            
            if (distance <= pickupRange)
            {
                PickupItem();
            }
        }
        
        /// <summary>
        /// アイテム取得処理
        /// </summary>
        public void PickupItem()
        {
            if (isPickedUp || itemManager == null) return;
            
            // Try to add to inventory
            bool addedToInventory = itemManager.AddToInventory(itemData, quantity);
            
            if (addedToInventory)
            {
                isPickedUp = true;
                OnItemPickedUp?.Invoke(this);
                
                // Play pickup sound
                var audioManager = gameManager?.AudioManager;
                if (audioManager != null)
                {
                    audioManager.PlaySE("item");
                }
                
                // Show pickup message
                var uiController = FindObjectOfType<GameUIController>();
                if (uiController != null)
                {
                    string message = quantity > 1 
                        ? $"💎 {itemData.itemName} x{quantity} を手に入れた！"
                        : $"💎 {itemData.itemName} を手に入れた！";
                    uiController.AddMessage(message);
                }
                
                Debug.Log($"💎 Picked up {itemData.itemName} x{quantity}");
                
                // Remove from dungeon
                Destroy(gameObject, 0.1f);
            }
            else
            {
                // Inventory full message
                var uiController = FindObjectOfType<GameUIController>();
                if (uiController != null)
                {
                    uiController.AddMessage("🎒 インベントリがいっぱいです！");
                }
            }
        }
        
        /// <summary>
        /// 手動取得（プレイヤーアクション）
        /// </summary>
        public void ManualPickup()
        {
            if (!isPickedUp)
            {
                PickupItem();
            }
        }
        
        /// <summary>
        /// アイテム位置の設定
        /// </summary>
        public void SetPosition(Vector2Int newPosition)
        {
            position = newPosition;
        }
        
        /// <summary>
        /// 数量の設定
        /// </summary>
        public void SetQuantity(int newQuantity)
        {
            quantity = Mathf.Max(1, newQuantity);
        }
        
        /// <summary>
        /// アイテムの表示文字取得
        /// </summary>
        public char GetDisplayChar()
        {
            return itemData?.displayChar ?? '?';
        }
        
        /// <summary>
        /// アイテムの表示シンボル取得
        /// </summary>
        public string GetDisplaySymbol()
        {
            return itemData?.displaySymbol ?? "❓";
        }
        
        /// <summary>
        /// アイテム情報の取得
        /// </summary>
        public string GetItemInfo()
        {
            if (itemData == null) return "Unknown Item";
            
            string info = $"{itemData.itemName}";
            if (quantity > 1)
            {
                info += $" x{quantity}";
            }
            
            if (!string.IsNullOrEmpty(itemData.description))
            {
                info += $"\n{itemData.description}";
            }
            
            if (itemData.value > 0)
            {
                info += $"\n価値: {itemData.value}G";
            }
            
            return info;
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{itemData?.itemName ?? "Unknown"} x{quantity} at {position} - Picked up: {isPickedUp}";
        }
    }
}