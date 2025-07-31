using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BlackOnyxReborn
{
    /// <summary>
    /// アイテムの生成・管理を行うマネージャー
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySlots = 20;
        [SerializeField] private ItemData[] itemDatabase;
        
        [Header("Spawn Settings - Black Onyx Original Balance")]
        [SerializeField] private float itemSpawnChance = 0.08f; // オリジナル準拠：アイテム出現率激減
        [SerializeField] private int maxItemsPerFloor = 2; // オリジナル準拠：1フロア最大2個まで
        [SerializeField] private float treasureSpawnChance = 0.02f; // オリジナル準拠：宝物出現率激減
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Inventory system
        private Dictionary<string, InventoryItem> inventory = new Dictionary<string, InventoryItem>();
        private List<Item> floorItems = new List<Item>();
        
        // Manager references
        private GameManager gameManager;
        private DungeonManager dungeonManager;
        private CombatManager combatManager;
        
        // Events
        public System.Action<ItemData, int> OnItemAdded;
        public System.Action<ItemData, int> OnItemRemoved;
        public System.Action<ItemData> OnItemUsed;
        public System.Action OnInventoryChanged;
        
        // Properties
        public int InventoryCount => inventory.Count;
        public int MaxInventorySlots => maxInventorySlots;
        public bool IsInventoryFull => inventory.Count >= maxInventorySlots;
        public List<InventoryItem> InventoryItems => inventory.Values.ToList();
        
        void Start()
        {
            InitializeItemManager();
        }
        
        /// <summary>
        /// アイテムマネージャーの初期化
        /// </summary>
        private void InitializeItemManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                combatManager = gameManager.GetComponent<CombatManager>();
            }
            
            // Load item database
            if (itemDatabase == null || itemDatabase.Length == 0)
            {
                LoadItemDatabase();
            }
            
            // Subscribe to dungeon events
            if (dungeonManager != null)
            {
                dungeonManager.OnFloorChanged += OnFloorChanged;
            }
            
            Debug.Log($"💎 Item Manager initialized with {(itemDatabase?.Length ?? 0)} item types");
        }
        
        /// <summary>
        /// アイテムデータベースの読み込み
        /// </summary>
        private void LoadItemDatabase()
        {
            // リソースからアイテムデータを読み込み
            itemDatabase = Resources.LoadAll<ItemData>("Items");
            
            if (itemDatabase.Length == 0)
            {
                CreateDefaultItemData();
            }
        }
        
        /// <summary>
        /// オリジナル ブラックオニキス（1984年）準拠アイテムデータの作成
        /// </summary>
        private void CreateDefaultItemData()
        {
            List<ItemData> defaultItems = new List<ItemData>();
            
            // 回復ポーション（オリジナル準拠：効果低下、価格上昇）
            var healingPotion = ScriptableObject.CreateInstance<ItemData>();
            healingPotion.itemId = "potion_heal";
            healingPotion.itemName = "回復ポーション";
            healingPotion.description = "HPを15回復する（オリジナル準拠）";
            healingPotion.displayChar = 'p';
            healingPotion.displaySymbol = "🧪";
            healingPotion.itemType = ItemType.Consumable;
            healingPotion.rarity = ItemRarity.Common;
            healingPotion.value = 120; // オリジナル準拠：価格大幅上昇
            healingPotion.maxStackSize = 5; // オリジナル準拠：所持制限
            healingPotion.effects = new ItemEffect[] 
            {
                new ItemEffect { effectType = ItemEffectType.HealHP, value = 15, description = "HP+15" } // オリジナル準拠：効果半減
            };
            defaultItems.Add(healingPotion);
            
            // 大回復ポーション（オリジナル準拠：価格高騰、入手困難）
            var greatHealingPotion = ScriptableObject.CreateInstance<ItemData>();
            greatHealingPotion.itemId = "potion_great_heal";
            greatHealingPotion.itemName = "大回復ポーション";
            greatHealingPotion.description = "HPを60回復する（オリジナル準拠）";
            greatHealingPotion.displayChar = 'P';
            greatHealingPotion.displaySymbol = "🍶";
            greatHealingPotion.itemType = ItemType.Consumable;
            greatHealingPotion.rarity = ItemRarity.Rare; // オリジナル準拠：希少度上昇
            greatHealingPotion.value = 600; // オリジナル準拠：価格高騰
            greatHealingPotion.maxStackSize = 2; // オリジナル準拠：所持制限厳しく
            greatHealingPotion.minFloor = -4; // オリジナル準拠：深層でのみ入手
            greatHealingPotion.spawnWeight = 0.1f; // オリジナル準拠：出現率極低
            greatHealingPotion.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.HealHP, value = 60, description = "HP+60" } // オリジナル準拠：完全回復廃止
            };
            defaultItems.Add(greatHealingPotion);
            
            // 力の薬（オリジナル準拠：効果微小、価格高騰）
            var strengthPotion = ScriptableObject.CreateInstance<ItemData>();
            strengthPotion.itemId = "potion_strength";
            strengthPotion.itemName = "力の薬";
            strengthPotion.description = "攻撃力を永続的に+1する（オリジナル準拠）";
            strengthPotion.displayChar = 's';
            strengthPotion.displaySymbol = "💪";
            strengthPotion.itemType = ItemType.Consumable;
            strengthPotion.rarity = ItemRarity.Epic; // オリジナル準拠：超希少
            strengthPotion.value = 1500; // オリジナル準拠：価格大幅高騰
            strengthPotion.maxStackSize = 1; // オリジナル準拠：1個のみ所持可
            strengthPotion.minFloor = -5; // オリジナル準拠：最深層でのみ
            strengthPotion.spawnWeight = 0.05f; // オリジナル準拠：超低確率
            strengthPotion.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.IncreaseAttack, value = 1, description = "ATK+1" } // オリジナル準拠：効果激減
            };
            defaultItems.Add(strengthPotion);
            
            // 守りの薬（オリジナル準拠：効果微小、価格高騰）
            var defensePotion = ScriptableObject.CreateInstance<ItemData>();
            defensePotion.itemId = "potion_defense";
            defensePotion.itemName = "守りの薬";
            defensePotion.description = "防御力を永続的に+1する（オリジナル準拠）";
            defensePotion.displayChar = 'd';
            defensePotion.displaySymbol = "🛡️";
            defensePotion.itemType = ItemType.Consumable;
            defensePotion.rarity = ItemRarity.Epic; // オリジナル準拠：超希少
            defensePotion.value = 1200; // オリジナル準拠：価格大幅高騰
            defensePotion.maxStackSize = 1; // オリジナル準拠：1個のみ所持可
            defensePotion.minFloor = -4; // オリジナル準拠：深層でのみ
            defensePotion.spawnWeight = 0.05f; // オリジナル準拠：超低確率
            defensePotion.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.IncreaseDefense, value = 1, description = "DEF+1" } // オリジナル準拠：効果激減
            };
            defaultItems.Add(defensePotion);
            
            // ゴールド袋（オリジナル準拠：少額、希少）
            var goldBag = ScriptableObject.CreateInstance<ItemData>();
            goldBag.itemId = "treasure_gold";
            goldBag.itemName = "ゴールド袋";
            goldBag.description = "20ゴールドが入った袋（オリジナル準拠）";
            goldBag.displayChar = '$';
            goldBag.displaySymbol = "💰";
            goldBag.itemType = ItemType.Treasure;
            goldBag.rarity = ItemRarity.Uncommon; // オリジナル準拠：希少度上昇
            goldBag.value = 20; // オリジナル準拠：金額激減
            goldBag.maxStackSize = 10; // オリジナル準拠：所持制限
            goldBag.spawnWeight = 0.3f; // オリジナル準拠：出現率低下
            goldBag.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.GainGold, value = 20, description = "Gold+20" } // オリジナル準拠：金額激減
            };
            defaultItems.Add(goldBag);
            
            // 宝石（オリジナル準拠：価値大幅低下、超希少）
            var gem = ScriptableObject.CreateInstance<ItemData>();
            gem.itemId = "treasure_gem";
            gem.itemName = "宝石";
            gem.description = "美しく輝く宝石（オリジナル準拠）";
            gem.displayChar = '*';
            gem.displaySymbol = "💎";
            gem.itemType = ItemType.Treasure;
            gem.rarity = ItemRarity.Legendary; // オリジナル準拠：超希少
            gem.value = 300; // オリジナル準拠：価値大幅低下
            gem.maxStackSize = 3; // オリジナル準拠：所持制限厳格
            gem.minFloor = -5; // オリジナル準拠：最深層でのみ
            gem.spawnWeight = 0.02f; // オリジナル準拠：超低確率
            defaultItems.Add(gem);
            
            itemDatabase = defaultItems.ToArray();
            
            // 透明マント（オリジナルブラックオニキス特殊アイテム）
            var invisibilityCloak = ScriptableObject.CreateInstance<ItemData>();
            invisibilityCloak.itemId = "special_invisibility_cloak";
            invisibilityCloak.itemName = "透明マント";
            invisibilityCloak.description = "ハイドが落とす伝説のマント。敵に発見されにくくなる";
            invisibilityCloak.displayChar = 'I';
            invisibilityCloak.displaySymbol = "👻";
            invisibilityCloak.itemType = ItemType.Equipment;
            invisibilityCloak.rarity = ItemRarity.Legendary;
            invisibilityCloak.value = 2000; // オリジナル準拠：超高価
            invisibilityCloak.maxStackSize = 1;
            invisibilityCloak.minFloor = -5; // B5でハイドがドロップ
            invisibilityCloak.spawnWeight = 0.01f; // 超低確率（ハイド専用ドロップ）
            invisibilityCloak.canDrop = false; // 通常スポーンしない
            invisibilityCloak.canSell = false; // 売却不可
            invisibilityCloak.isInvisibilityCloak = true; // 特殊フラグ
            invisibilityCloak.enemyDetectionReduction = 0.8f; // 敵発見率80%減少
            invisibilityCloak.grantsInvisibility = true; // 透明化能力
            invisibilityCloak.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.GrantInvisibility, value = 300, description = "透明化（5分間）" },
                new ItemEffect { effectType = ItemEffectType.ReduceEnemyDetection, value = 80, description = "敵発見率-80%" }
            };
            defaultItems.Add(invisibilityCloak);
            
            // ブラックオニキス（最終目標アイテム）
            var blackOnyx = ScriptableObject.CreateInstance<ItemData>();
            blackOnyx.itemId = "legendary_black_onyx";
            blackOnyx.itemName = "ブラックオニキス";
            blackOnyx.description = "伝説の漆黒の宝石。これを手に入れることが冒険の目的";
            blackOnyx.displayChar = '●';
            blackOnyx.displaySymbol = "⚫";
            blackOnyx.itemType = ItemType.Special;
            blackOnyx.rarity = ItemRarity.Legendary;
            blackOnyx.value = 50000; // 最高価値
            blackOnyx.maxStackSize = 1;
            blackOnyx.minFloor = -6; // B6カラー迷路でのみ発見可能
            blackOnyx.spawnWeight = 0.001f; // 極めて低確率
            blackOnyx.canDrop = false; // 通常スポーンしない
            blackOnyx.canSell = false; // 売却不可
            blackOnyx.isBlackOnyx = true; // 特殊フラグ
            blackOnyx.effects = new ItemEffect[]
            {
                new ItemEffect { effectType = ItemEffectType.WinGame, value = 1, description = "ゲームクリア" }
            };
            defaultItems.Add(blackOnyx);
            
            if (debugMode)
            {
                Debug.Log($"🏰 Created {itemDatabase.Length} authentic Black Onyx item types (1984 original balance)");
            }
        }
        
        /// <summary>
        /// インベントリにアイテムを追加
        /// </summary>
        public bool AddToInventory(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;
            
            // 既存のアイテムがある場合はスタック
            if (inventory.ContainsKey(itemData.itemId))
            {
                var existingItem = inventory[itemData.itemId];
                int newQuantity = existingItem.quantity + quantity;
                
                if (newQuantity <= itemData.maxStackSize)
                {
                    existingItem.quantity = newQuantity;
                    OnItemAdded?.Invoke(itemData, quantity);
                    OnInventoryChanged?.Invoke();
                    
                    if (debugMode)
                    {
                        Debug.Log($"💎 Stacked {itemData.itemName} x{quantity} (total: {newQuantity})");
                    }
                    
                    return true;
                }
                else if (existingItem.quantity < itemData.maxStackSize)
                {
                    // 部分的にスタック
                    int stackable = itemData.maxStackSize - existingItem.quantity;
                    existingItem.quantity = itemData.maxStackSize;
                    
                    // 残りを新しいスロットに
                    int remaining = quantity - stackable;
                    if (!IsInventoryFull && remaining > 0)
                    {
                        return AddNewItemSlot(itemData, remaining);
                    }
                    
                    OnItemAdded?.Invoke(itemData, stackable);
                    OnInventoryChanged?.Invoke();
                    return stackable == quantity;
                }
            }
            
            // 新しいスロットを作成
            if (!IsInventoryFull)
            {
                return AddNewItemSlot(itemData, quantity);
            }
            
            return false; // インベントリが満杯
        }
        
        /// <summary>
        /// 新しいアイテムスロットを追加
        /// </summary>
        private bool AddNewItemSlot(ItemData itemData, int quantity)
        {
            if (IsInventoryFull) return false;
            
            // 一意のキーを生成
            string slotKey = GenerateUniqueSlotKey(itemData.itemId);
            
            inventory[slotKey] = new InventoryItem
            {
                itemData = itemData,
                quantity = Mathf.Min(quantity, itemData.maxStackSize),
                slotKey = slotKey
            };
            
            OnItemAdded?.Invoke(itemData, quantity);
            OnInventoryChanged?.Invoke();
            
            if (debugMode)
            {
                Debug.Log($"💎 Added new item slot: {itemData.itemName} x{quantity}");
            }
            
            return true;
        }
        
        /// <summary>
        /// 一意のスロットキーを生成
        /// </summary>
        private string GenerateUniqueSlotKey(string itemId)
        {
            string baseKey = itemId;
            int counter = 0;
            
            while (inventory.ContainsKey(baseKey))
            {
                counter++;
                baseKey = $"{itemId}_{counter}";
            }
            
            return baseKey;
        }
        
        /// <summary>
        /// アイテムを使用
        /// </summary>
        public bool UseItem(string slotKey)
        {
            if (!inventory.ContainsKey(slotKey)) return false;
            
            var inventoryItem = inventory[slotKey];
            var itemData = inventoryItem.itemData;
            
            // 使用可能性チェック
            if (combatManager != null && !itemData.CanUse(combatManager.PlayerStats))
            {
                var uiController = FindObjectOfType<GameUIController>();
                if (uiController != null)
                {
                    uiController.AddMessage($"⚠️ {itemData.itemName} を使用できません");
                }
                return false;
            }
            
            // 効果を適用
            if (itemData.effects != null && combatManager != null)
            {
                foreach (var effect in itemData.effects)
                {
                    effect.ApplyEffect(combatManager.PlayerStats, combatManager);
                }
            }
            
            // UI メッセージ
            var uiController2 = FindObjectOfType<GameUIController>();
            if (uiController2 != null)
            {
                uiController2.AddMessage($"✨ {itemData.itemName} を使用した");
            }
            
            // 使用後の処理
            OnItemUsed?.Invoke(itemData);
            
            // 消耗品の場合は数量を減らす
            if (itemData.itemType == ItemType.Consumable)
            {
                RemoveFromInventory(slotKey, 1);
            }
            
            if (debugMode)
            {
                Debug.Log($"💎 Used item: {itemData.itemName}");
            }
            
            return true;
        }
        
        /// <summary>
        /// インベントリからアイテムを削除
        /// </summary>
        public bool RemoveFromInventory(string slotKey, int quantity = 1)
        {
            if (!inventory.ContainsKey(slotKey)) return false;
            
            var inventoryItem = inventory[slotKey];
            
            if (inventoryItem.quantity <= quantity)
            {
                // スロット全体を削除
                inventory.Remove(slotKey);
                OnItemRemoved?.Invoke(inventoryItem.itemData, inventoryItem.quantity);
            }
            else
            {
                // 数量のみ減らす
                inventoryItem.quantity -= quantity;
                OnItemRemoved?.Invoke(inventoryItem.itemData, quantity);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// フロアにアイテムをスポーン
        /// </summary>
        public void SpawnItem(string itemId, Vector2Int position, int quantity = 1)
        {
            var itemData = GetItemData(itemId);
            if (itemData == null) return;
            
            // アイテムオブジェクトを作成
            GameObject itemObj = new GameObject($"Item_{itemData.itemName}");
            Item itemComponent = itemObj.AddComponent<Item>();
            
            // 初期化
            itemComponent.InitializeWithData(itemData, position, quantity);
            itemComponent.OnItemPickedUp += OnItemPickedUp;
            
            // フロアアイテムリストに追加
            floorItems.Add(itemComponent);
            
            if (debugMode)
            {
                Debug.Log($"💎 Spawned {itemData.itemName} x{quantity} at {position}");
            }
        }
        
        /// <summary>
        /// ランダムアイテムのスポーン
        /// </summary>
        public void SpawnRandomItems(int floorNumber)
        {
            int itemsToSpawn = Random.Range(1, maxItemsPerFloor + 1);
            
            for (int i = 0; i < itemsToSpawn; i++)
            {
                // アイテムスポーンの判定
                if (Random.Range(0f, 1f) > itemSpawnChance) continue;
                
                // このフロアに適したアイテムを選択
                var itemData = SelectRandomItemForFloor(floorNumber);
                if (itemData == null) continue;
                
                // スポーン位置を決定
                Vector2Int spawnPosition = dungeonManager.GetRandomWalkablePosition();
                if (spawnPosition == Vector2Int.zero) continue;
                
                // 数量を決定
                int quantity = Random.Range(1, Mathf.Min(3, itemData.maxStackSize) + 1);
                
                // スポーン実行
                SpawnItem(itemData.itemId, spawnPosition, quantity);
            }
        }
        
        /// <summary>
        /// フロアに適したランダムアイテムを選択
        /// </summary>
        private ItemData SelectRandomItemForFloor(int floor)
        {
            var validItems = itemDatabase.Where(item => item.CanSpawnOnFloor(floor)).ToArray();
            if (validItems.Length == 0) return null;
            
            // 重み付きランダム選択
            float totalWeight = validItems.Sum(item => item.spawnWeight);
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var item in validItems)
            {
                currentWeight += item.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }
            
            return validItems[validItems.Length - 1];
        }
        
        /// <summary>
        /// アイテムデータを取得
        /// </summary>
        public ItemData GetItemData(string itemId)
        {
            return itemDatabase.FirstOrDefault(item => item.itemId == itemId);
        }
        
        /// <summary>
        /// アイテム取得イベント
        /// </summary>
        private void OnItemPickedUp(Item item)
        {
            floorItems.Remove(item);
        }
        
        /// <summary>
        /// フロア変更イベント
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            // 前のフロアのアイテムをクリア
            ClearFloorItems();
            
            // 新しいフロアにアイテムをスポーン
            SpawnRandomItems(newFloor);
            
            if (debugMode)
            {
                Debug.Log($"💎 Floor {newFloor} items spawned");
            }
        }
        
        /// <summary>
        /// フロアアイテムのクリア
        /// </summary>
        private void ClearFloorItems()
        {
            foreach (var item in floorItems.ToList())
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            floorItems.Clear();
        }
        
        /// <summary>
        /// インベントリのクリア
        /// </summary>
        public void ClearInventory()
        {
            inventory.Clear();
            OnInventoryChanged?.Invoke();
            
            Debug.Log("💎 Inventory cleared");
        }
        
        /// <summary>
        /// インベントリ情報の取得
        /// </summary>
        public string GetInventoryInfo()
        {
            if (inventory.Count == 0)
            {
                return "インベントリは空です";
            }
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"インベントリ ({inventory.Count}/{maxInventorySlots}):");
            
            foreach (var item in inventory.Values)
            {
                sb.AppendLine($"- {item.itemData.itemName} x{item.quantity}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Item Manager:\n" +
                   $"Inventory: {inventory.Count}/{maxInventorySlots}\n" +
                   $"Floor Items: {floorItems.Count}\n" +
                   $"Database: {(itemDatabase?.Length ?? 0)} item types";
        }
        
        void OnDestroy()
        {
            // イベント購読解除
            if (dungeonManager != null)
            {
                dungeonManager.OnFloorChanged -= OnFloorChanged;
            }
        }
    }
    
    /// <summary>
    /// インベントリアイテム
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int quantity;
        public string slotKey;
        
        public string GetDisplayText()
        {
            if (quantity > 1)
            {
                return $"{itemData.itemName} x{quantity}";
            }
            return itemData.itemName;
        }
    }
}