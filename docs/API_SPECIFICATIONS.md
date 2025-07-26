# 🔌 ブラックオニキス復刻版 - API仕様書

## 📋 API設計概要

### 設計方針
- **クライアントサイド中心**: Web技術のみで完結するスタンドアロン設計
- **内部API**: ゲームエンジン内部のモジュール間通信API
- **プラガブル**: 将来的な外部サービス連携を考慮した設計
- **RESTful風**: 一貫性のあるインターフェース設計

### アーキテクチャ
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  UI Components  │ => │  Game Engine    │ => │  Data Layer     │
│                 │    │  Internal APIs  │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🎮 ゲームエンジンAPI

### 1. ゲーム状態管理API

```typescript
/**
 * ゲーム状態管理インターフェース
 */
interface GameStateAPI {
    // ゲーム状態取得
    getCurrentState(): GameState;
    
    // 状態変更
    setState(newState: GameState): Promise<void>;
    
    // 状態遷移
    transitionTo(targetState: GameState, options?: TransitionOptions): Promise<void>;
    
    // 状態履歴
    getStateHistory(): GameState[];
    
    // 状態ロールバック
    rollbackToState(stateId: string): Promise<void>;
}

interface GameState {
    id: string;
    name: string;  // 'loading' | 'title' | 'character_creation' | 'gameplay' | 'battle' | 'menu'
    timestamp: number;
    data?: any;
}

interface TransitionOptions {
    animation?: string;
    duration?: number;
    onComplete?: () => void;
    onError?: (error: Error) => void;
}

/**
 * 実装例
 */
class GameStateManager implements GameStateAPI {
    private currentState: GameState;
    private stateHistory: GameState[] = [];
    private listeners: Map<string, Function[]> = new Map();
    
    getCurrentState(): GameState {
        return { ...this.currentState };
    }
    
    async setState(newState: GameState): Promise<void> {
        const oldState = this.currentState;
        
        try {
            // 状態検証
            this.validateStateTransition(oldState, newState);
            
            // 状態保存
            this.stateHistory.push(oldState);
            this.currentState = newState;
            
            // リスナー通知
            this.notifyStateChange(oldState, newState);
            
        } catch (error) {
            throw new Error(`State transition failed: ${error.message}`);
        }
    }
    
    async transitionTo(targetState: GameState, options: TransitionOptions = {}): Promise<void> {
        return new Promise((resolve, reject) => {
            // アニメーション開始
            if (options.animation) {
                this.startTransitionAnimation(options.animation, options.duration || 500);
            }
            
            // 状態変更
            this.setState(targetState)
                .then(() => {
                    options.onComplete?.();
                    resolve();
                })
                .catch(error => {
                    options.onError?.(error);
                    reject(error);
                });
        });
    }
    
    // 状態変更リスナー登録
    onStateChange(callback: (oldState: GameState, newState: GameState) => void): void {
        if (!this.listeners.has('stateChange')) {
            this.listeners.set('stateChange', []);
        }
        this.listeners.get('stateChange')!.push(callback);
    }
}
```

### 2. プレイヤー管理API

```typescript
/**
 * プレイヤー管理インターフェース
 */
interface PlayerAPI {
    // プレイヤー情報取得
    getPlayer(): Promise<PlayerCharacter>;
    
    // プレイヤー作成
    createPlayer(playerData: PlayerCreationData): Promise<PlayerCharacter>;
    
    // プレイヤー更新
    updatePlayer(updates: Partial<PlayerCharacter>): Promise<PlayerCharacter>;
    
    // ステータス変更
    modifyStats(statChanges: StatModification[]): Promise<StatChangeResult>;
    
    // 経験値獲得
    gainExperience(amount: number): Promise<ExperienceGainResult>;
    
    // レベルアップ
    levelUp(): Promise<LevelUpResult>;
    
    // HP/MP変更
    modifyHP(amount: number): Promise<void>;
    modifyMP(amount: number): Promise<void>;
    
    // 状態異常管理
    addCondition(condition: StatusCondition): Promise<void>;
    removeCondition(conditionId: string): Promise<void>;
    getConditions(): Promise<StatusCondition[]>;
}

interface PlayerCreationData {
    name: string;
    appearance: CharacterAppearance;
    initialStats?: Partial<CharacterStats>;
}

interface StatModification {
    stat: keyof CharacterStats;
    change: number;
    temporary?: boolean;
    duration?: number;  // ミリ秒
}

interface StatChangeResult {
    success: boolean;
    changes: { [stat: string]: number };
    newStats: CharacterStats;
    message?: string;
}

interface ExperienceGainResult {
    experienceGained: number;
    totalExperience: number;
    leveledUp: boolean;
    levelUpResult?: LevelUpResult;
}

interface LevelUpResult {
    oldLevel: number;
    newLevel: number;
    statGains: { [stat: string]: number };
    hpIncrease: number;
    mpIncrease: number;
    newAbilities?: string[];
}

interface StatusCondition {
    id: string;
    type: string;  // 'poison' | 'paralysis' | 'sleep' | 'strengthen'
    duration: number;  // 残りターン数
    power: number;     // 効果の強さ
    source: string;    // 原因
}

/**
 * 実装例
 */
class PlayerManager implements PlayerAPI {
    private player: PlayerCharacter | null = null;
    
    async getPlayer(): Promise<PlayerCharacter> {
        if (!this.player) {
            throw new Error('Player not initialized');
        }
        return { ...this.player };
    }
    
    async createPlayer(playerData: PlayerCreationData): Promise<PlayerCharacter> {
        this.player = new PlayerCharacter({
            id: this.generatePlayerId(),
            name: playerData.name,
            level: 1,
            experience: 0,
            stats: playerData.initialStats || this.generateRandomStats(),
            appearance: playerData.appearance,
            inventory: [],
            gold: 100
        });
        
        return this.getPlayer();
    }
    
    async gainExperience(amount: number): Promise<ExperienceGainResult> {
        if (!this.player) throw new Error('Player not initialized');
        
        const oldExp = this.player.experience;
        const oldLevel = this.player.level;
        
        this.player.experience += amount;
        
        // レベルアップ判定
        let levelUpResult: LevelUpResult | undefined;
        while (this.player.experience >= this.calculateExpToNext(this.player.level)) {
            levelUpResult = await this.levelUp();
        }
        
        return {
            experienceGained: amount,
            totalExperience: this.player.experience,
            leveledUp: this.player.level > oldLevel,
            levelUpResult
        };
    }
    
    private generateRandomStats(): CharacterStats {
        return {
            strength: Math.floor(Math.random() * 15) + 5,
            intelligence: Math.floor(Math.random() * 15) + 5,
            agility: Math.floor(Math.random() * 15) + 5,
            vitality: Math.floor(Math.random() * 15) + 5
        };
    }
}
```

### 3. 戦闘システムAPI

```typescript
/**
 * 戦闘管理インターフェース
 */
interface BattleAPI {
    // 戦闘開始
    startBattle(enemies: string[], environment?: BattleEnvironment): Promise<BattleStartResult>;
    
    // 戦闘終了
    endBattle(result: BattleResult): Promise<BattleEndResult>;
    
    // アクション実行
    executeAction(action: BattleAction): Promise<ActionResult>;
    
    // ターン進行
    advanceTurn(): Promise<TurnResult>;
    
    // 戦闘状態取得
    getBattleState(): Promise<BattleState>;
    
    // 逃走判定
    attemptFlee(): Promise<FleeResult>;
    
    // AI行動
    executeAITurn(enemy: Enemy): Promise<ActionResult>;
}

interface BattleEnvironment {
    terrain: string;    // 'dungeon' | 'outdoors' | 'water'
    lighting: number;   // 0-100
    hazards?: string[]; // 環境ハザード
}

interface BattleStartResult {
    battleId: string;
    enemies: Enemy[];
    turnOrder: Combatant[];
    environment: BattleEnvironment;
    message: string;
}

interface BattleAction {
    actorId: string;
    type: 'attack' | 'magic' | 'item' | 'defend' | 'special';
    targetId?: string;
    targetIds?: string[];  // 複数対象
    data?: any;           // アクション固有データ
}

interface ActionResult {
    success: boolean;
    damage?: number;
    healing?: number;
    effects?: StatusEffect[];
    criticalHit?: boolean;
    message: string;
    animation?: AnimationData;
}

interface TurnResult {
    currentTurn: number;
    activeCharacter: Combatant;
    battleEnded: boolean;
    victor?: 'player' | 'enemy';
}

interface FleeResult {
    success: boolean;
    message: string;
    consequences?: {
        itemsLost?: ItemInstance[];
        experienceLost?: number;
    };
}

/**
 * 実装例
 */
class BattleManager implements BattleAPI {
    private currentBattle: Battle | null = null;
    
    async startBattle(enemyIds: string[], environment?: BattleEnvironment): Promise<BattleStartResult> {
        // 敵生成
        const enemies = enemyIds.map(id => this.createEnemy(id));
        
        // 戦闘初期化
        this.currentBattle = new Battle({
            id: this.generateBattleId(),
            playerParty: [await this.playerAPI.getPlayer()],
            enemies: enemies,
            environment: environment || { terrain: 'dungeon', lighting: 50 }
        });
        
        // ターン順計算
        const turnOrder = this.calculateTurnOrder();
        
        return {
            battleId: this.currentBattle.id,
            enemies: enemies,
            turnOrder: turnOrder,
            environment: this.currentBattle.environment,
            message: `${enemies.length}体の敵が現れた！`
        };
    }
    
    async executeAction(action: BattleAction): Promise<ActionResult> {
        if (!this.currentBattle) {
            throw new Error('No active battle');
        }
        
        const actor = this.currentBattle.findCombatant(action.actorId);
        const target = action.targetId ? this.currentBattle.findCombatant(action.targetId) : null;
        
        switch (action.type) {
            case 'attack':
                return this.executeAttack(actor, target!, action.data);
            case 'magic':
                return this.executeMagic(actor, target, action.data);
            case 'item':
                return this.executeItemUse(actor, target, action.data);
            case 'defend':
                return this.executeDefend(actor);
            default:
                throw new Error(`Unknown action type: ${action.type}`);
        }
    }
    
    private async executeAttack(attacker: Combatant, target: Combatant, weaponData?: any): Promise<ActionResult> {
        // 命中判定
        const accuracy = this.calculateAccuracy(attacker, target, weaponData);
        const hit = Math.random() < accuracy;
        
        if (!hit) {
            return {
                success: false,
                message: `${attacker.name}の攻撃は外れた！`,
                animation: { type: 'miss', target: target.id }
            };
        }
        
        // ダメージ計算
        const damage = this.calculateDamage(attacker, target, weaponData);
        const isCritical = Math.random() < 0.05;
        const finalDamage = isCritical ? Math.floor(damage * 1.5) : damage;
        
        // ダメージ適用
        target.hp = Math.max(0, target.hp - finalDamage);
        
        return {
            success: true,
            damage: finalDamage,
            criticalHit: isCritical,
            message: `${attacker.name}の攻撃！ ${finalDamage}のダメージ${isCritical ? '（会心の一撃！）' : ''}`,
            animation: {
                type: isCritical ? 'critical_attack' : 'attack',
                attacker: attacker.id,
                target: target.id,
                damage: finalDamage
            }
        };
    }
}
```

### 4. インベントリ管理API

```typescript
/**
 * インベントリ管理インターフェース
 */
interface InventoryAPI {
    // アイテム取得
    getInventory(): Promise<ItemInstance[]>;
    
    // アイテム追加
    addItem(itemId: string, quantity?: number): Promise<AddItemResult>;
    
    // アイテム削除
    removeItem(instanceId: string, quantity?: number): Promise<RemoveItemResult>;
    
    // アイテム使用
    useItem(instanceId: string, target?: string): Promise<UseItemResult>;
    
    // 装備変更
    equipItem(instanceId: string, slot: string): Promise<EquipResult>;
    unequipItem(slot: string): Promise<UnequipResult>;
    
    // アイテム検索
    findItems(criteria: ItemSearchCriteria): Promise<ItemInstance[]>;
    
    // 重量・容量チェック
    getCapacityInfo(): Promise<CapacityInfo>;
    canCarryMore(weight: number): Promise<boolean>;
    
    // アイテム整理
    sortInventory(sortBy: string): Promise<void>;
    stackItems(): Promise<StackResult>;
}

interface AddItemResult {
    success: boolean;
    item: ItemInstance;
    message: string;
    overflowed?: boolean;  // 容量超過で一部のみ追加
    addedQuantity?: number;
}

interface RemoveItemResult {
    success: boolean;
    removedItem: ItemInstance;
    remainingQuantity: number;
    message: string;
}

interface UseItemResult {
    success: boolean;
    effects: ItemEffect[];
    consumeOnUse: boolean;
    message: string;
    remainingQuantity?: number;
}

interface EquipResult {
    success: boolean;
    equippedItem: ItemInstance;
    unequippedItem?: ItemInstance;
    statChanges: { [stat: string]: number };
    message: string;
}

interface ItemSearchCriteria {
    category?: string;
    rarity?: string;
    nameContains?: string;
    usable?: boolean;
    equipable?: boolean;
}

interface CapacityInfo {
    currentWeight: number;
    maxWeight: number;
    usedSlots: number;
    maxSlots: number;
    weightRatio: number;  // 0-1
    slotRatio: number;    // 0-1
}

interface StackResult {
    itemsStacked: number;
    slotsFreed: number;
    message: string;
}

/**
 * 実装例
 */
class InventoryManager implements InventoryAPI {
    private inventory: ItemInstance[] = [];
    private equipment: { [slot: string]: ItemInstance } = {};
    
    async addItem(itemId: string, quantity: number = 1): Promise<AddItemResult> {
        const itemTemplate = await this.getItemTemplate(itemId);
        
        // 容量チェック
        const canCarry = await this.canCarryMore(itemTemplate.weight * quantity);
        if (!canCarry) {
            return {
                success: false,
                item: null as any,
                message: '持ち物がいっぱいです。'
            };
        }
        
        // スタック可能アイテムの場合、既存アイテムを探す
        if (itemTemplate.stackable) {
            const existingItem = this.inventory.find(item => 
                item.id === itemId && item.quantity < itemTemplate.maxStack
            );
            
            if (existingItem) {
                const canStack = Math.min(quantity, itemTemplate.maxStack - existingItem.quantity);
                existingItem.quantity += canStack;
                
                return {
                    success: true,
                    item: existingItem,
                    message: `${itemTemplate.name}を${canStack}個手に入れた。`,
                    addedQuantity: canStack
                };
            }
        }
        
        // 新しいアイテムインスタンス作成
        const newItem: ItemInstance = {
            id: itemId,
            instanceId: this.generateInstanceId(),
            quantity: quantity,
            durability: itemTemplate.durability || 100,
            maxDurability: itemTemplate.durability || 100,
            enchantments: [],
            acquisitionTime: Date.now()
        };
        
        this.inventory.push(newItem);
        
        return {
            success: true,
            item: newItem,
            message: `${itemTemplate.name}を${quantity}個手に入れた。`
        };
    }
    
    async useItem(instanceId: string, targetId?: string): Promise<UseItemResult> {
        const item = this.inventory.find(i => i.instanceId === instanceId);
        if (!item) {
            return {
                success: false,
                effects: [],
                consumeOnUse: false,
                message: 'アイテムが見つかりません。'
            };
        }
        
        const itemTemplate = await this.getItemTemplate(item.id);
        const target = targetId ? await this.getTarget(targetId) : await this.playerAPI.getPlayer();
        
        // 使用効果実行
        const effects = await this.applyItemEffects(itemTemplate, target);
        
        // 消費処理
        if (itemTemplate.usageEffect?.consumeOnUse) {
            item.quantity--;
            if (item.quantity <= 0) {
                this.inventory = this.inventory.filter(i => i.instanceId !== instanceId);
            }
        }
        
        return {
            success: true,
            effects: effects,
            consumeOnUse: itemTemplate.usageEffect?.consumeOnUse || false,
            message: `${itemTemplate.name}を使用した。`,
            remainingQuantity: item.quantity
        };
    }
}
```

### 5. ダンジョン管理API

```typescript
/**
 * ダンジョン管理インターフェース
 */
interface DungeonAPI {
    // マップデータ取得
    getMapData(floorId: string): Promise<MapData>;
    
    // プレイヤー移動
    movePlayer(direction: Direction): Promise<MoveResult>;
    
    // タイル情報取得
    getTileInfo(floorId: string, x: number, y: number): Promise<TileInfo>;
    
    // オブジェクト操作
    interactWithObject(objectId: string, action?: string): Promise<InteractionResult>;
    
    // 隠し要素発見
    searchCurrentTile(): Promise<SearchResult>;
    
    // エンカウンター判定
    checkRandomEncounter(): Promise<EncounterResult>;
    
    // フロア移動
    changeFloor(targetFloor: string, position?: Position): Promise<FloorChangeResult>;
    
    // 探索状況取得
    getExplorationStatus(floorId: string): Promise<ExplorationStatus>;
}

interface MoveResult {
    success: boolean;
    newPosition: Position;
    newDirection: Direction;
    tileEntered: TileInfo;
    encounter?: EncounterResult;
    message?: string;
    animations?: AnimationData[];
}

interface TileInfo {
    type: number;
    hasWall: boolean;
    hasObject: boolean;
    objects: MapObject[];
    hasBeenExplored: boolean;
    specialProperties?: { [key: string]: any };
}

interface InteractionResult {
    success: boolean;
    newState?: string;
    itemsGained?: ItemInstance[];
    goldGained?: number;
    experienceGained?: number;
    message: string;
    animation?: AnimationData;
    followUpActions?: string[];
}

interface SearchResult {
    foundSomething: boolean;
    discoveries: Discovery[];
    message: string;
}

interface Discovery {
    type: 'hidden_door' | 'secret_treasure' | 'trap' | 'clue';
    description: string;
    data?: any;
}

interface EncounterResult {
    hasEncounter: boolean;
    enemies?: string[];
    encounterType?: 'normal' | 'ambush' | 'preemptive';
    message?: string;
}

interface FloorChangeResult {
    success: boolean;
    newFloor: string;
    newPosition: Position;
    mapData: MapData;
    message: string;
    requiresLoading?: boolean;
}

interface ExplorationStatus {
    tilesExplored: number;
    totalTiles: number;
    explorationPercentage: number;
    secretsFound: number;
    totalSecrets: number;
    treasuresFound: number;
    totalTreasures: number;
}

/**
 * 実装例
 */
class DungeonManager implements DungeonAPI {
    private currentFloor: string = 'B1';
    private playerPosition: Position = { x: 1, y: 1 };
    private playerDirection: Direction = Direction.North;
    private mapCache: Map<string, MapData> = new Map();
    
    async movePlayer(direction: Direction): Promise<MoveResult> {
        const currentMap = await this.getMapData(this.currentFloor);
        let newPosition = { ...this.playerPosition };
        let newDirection = this.playerDirection;
        
        // 方向転換または移動の判定
        if (direction !== this.playerDirection) {
            // 方向転換のみ
            newDirection = direction;
        } else {
            // 前進
            switch (direction) {
                case Direction.North:
                    newPosition.y--;
                    break;
                case Direction.East:
                    newPosition.x++;
                    break;
                case Direction.South:
                    newPosition.y++;
                    break;
                case Direction.West:
                    newPosition.x--;
                    break;
            }
            
            // 移動可能性チェック
            const canMove = await this.canMoveTo(currentMap, newPosition);
            if (!canMove) {
                return {
                    success: false,
                    newPosition: this.playerPosition,
                    newDirection: newDirection,
                    tileEntered: await this.getTileInfo(this.currentFloor, this.playerPosition.x, this.playerPosition.y),
                    message: 'そちらには進めません。'
                };
            }
        }
        
        // 位置更新
        this.playerPosition = newPosition;
        this.playerDirection = newDirection;
        
        // タイル情報取得
        const tileInfo = await this.getTileInfo(this.currentFloor, newPosition.x, newPosition.y);
        
        // エンカウンター判定
        const encounterResult = await this.checkRandomEncounter();
        
        return {
            success: true,
            newPosition: newPosition,
            newDirection: newDirection,
            tileEntered: tileInfo,
            encounter: encounterResult.hasEncounter ? encounterResult : undefined,
            message: encounterResult.hasEncounter ? encounterResult.message : undefined
        };
    }
    
    async interactWithObject(objectId: string, action: string = 'use'): Promise<InteractionResult> {
        const currentMap = await this.getMapData(this.currentFloor);
        const mapObject = currentMap.objects.find(obj => obj.id === objectId);
        
        if (!mapObject) {
            return {
                success: false,
                message: 'そのオブジェクトは見つかりません。'
            };
        }
        
        // インタラクション実行
        const interaction = mapObject.interactions.find(i => i.action === action);
        if (!interaction) {
            return {
                success: false,
                message: 'そのアクションは実行できません。'
            };
        }
        
        // 条件チェック
        if (interaction.conditions) {
            const conditionsMet = await this.checkConditions(interaction.conditions);
            if (!conditionsMet) {
                return {
                    success: false,
                    message: '条件を満たしていません。'
                };
            }
        }
        
        // 結果適用
        return this.applyInteractionResult(interaction.result);
    }
    
    private async applyInteractionResult(result: InteractionResult): Promise<InteractionResult> {
        switch (result.type) {
            case 'item_gain':
                const addResult = await this.inventoryAPI.addItem(result.data.itemId, result.data.quantity);
                return {
                    success: addResult.success,
                    itemsGained: [addResult.item],
                    message: addResult.message
                };
                
            case 'gold_gain':
                await this.playerAPI.modifyGold(result.data.amount);
                return {
                    success: true,
                    goldGained: result.data.amount,
                    message: `${result.data.amount}ゴールドを手に入れた！`
                };
                
            case 'teleport':
                const floorChangeResult = await this.changeFloor(result.data.targetFloor, result.data.position);
                return {
                    success: floorChangeResult.success,
                    message: floorChangeResult.message,
                    followUpActions: floorChangeResult.requiresLoading ? ['load_floor'] : undefined
                };
                
            default:
                return {
                    success: false,
                    message: '不明な結果タイプです。'
                };
        }
    }
}
```

## 💾 データ管理API

### 1. セーブ・ロードAPI

```typescript
/**
 * セーブ・ロード管理インターフェース
 */
interface SaveLoadAPI {
    // セーブ操作
    saveGame(slotNumber: number, slotName?: string): Promise<SaveResult>;
    quickSave(): Promise<SaveResult>;
    autoSave(): Promise<SaveResult>;
    
    // ロード操作
    loadGame(slotNumber: number): Promise<LoadResult>;
    quickLoad(): Promise<LoadResult>;
    
    // セーブデータ管理
    getSaveList(): Promise<SaveSlot[]>;
    deleteSave(slotNumber: number): Promise<DeleteResult>;
    copySave(fromSlot: number, toSlot: number): Promise<CopyResult>;
    
    // エクスポート・インポート
    exportSave(slotNumber: number): Promise<ExportResult>;
    importSave(saveData: string, slotNumber: number): Promise<ImportResult>;
    
    // バックアップ管理
    createBackup(): Promise<BackupResult>;
    restoreBackup(backupId: string): Promise<RestoreResult>;
}

interface SaveResult {
    success: boolean;
    slotNumber: number;
    slotName: string;
    saveSize: number;  // bytes
    timestamp: number;
    message: string;
    error?: string;
}

interface LoadResult {
    success: boolean;
    saveData?: SaveData;
    message: string;
    error?: string;
    migrationRequired?: boolean;
}

interface SaveSlot {
    slotNumber: number;
    slotName: string;
    playerName: string;
    level: number;
    floor: string;
    playtime: number;
    timestamp: number;
    fileSize: number;
    hasData: boolean;
}

interface ExportResult {
    success: boolean;
    exportData?: string;  // JSON string
    filename: string;
    fileSize: number;
    message: string;
}

/**
 * 実装例
 */
class SaveLoadManager implements SaveLoadAPI {
    private databaseManager: DatabaseManager;
    private compressionEnabled: boolean = true;
    
    async saveGame(slotNumber: number, slotName?: string): Promise<SaveResult> {
        try {
            // 現在のゲーム状態収集
            const gameState = await this.collectGameState();
            
            // セーブデータ作成
            const saveData: SaveData = {
                saveId: this.generateSaveId(),
                version: '1.0.0',
                timestamp: Date.now(),
                slotNumber: slotNumber,
                slotName: slotName || `セーブデータ ${slotNumber}`,
                playtime: gameState.playtime,
                player: gameState.player,
                world: gameState.world,
                progress: gameState.progress,
                statistics: gameState.statistics,
                gameSettings: gameState.settings
            };
            
            // データ圧縮（オプション）
            const finalData = this.compressionEnabled 
                ? await this.compressData(saveData)
                : saveData;
            
            // データベースに保存
            await this.databaseManager.saveSaveData(finalData);
            
            // 統計更新
            await this.updateSaveStatistics(gameState.statistics);
            
            return {
                success: true,
                slotNumber: slotNumber,
                slotName: saveData.slotName,
                saveSize: JSON.stringify(finalData).length,
                timestamp: saveData.timestamp,
                message: 'ゲームを保存しました。'
            };
            
        } catch (error) {
            return {
                success: false,
                slotNumber: slotNumber,
                slotName: slotName || '',
                saveSize: 0,
                timestamp: Date.now(),
                message: '保存に失敗しました。',
                error: error.message
            };
        }
    }
    
    async loadGame(slotNumber: number): Promise<LoadResult> {
        try {
            // セーブデータ取得
            const saveSlots = await this.getSaveList();
            const targetSlot = saveSlots.find(slot => slot.slotNumber === slotNumber);
            
            if (!targetSlot || !targetSlot.hasData) {
                return {
                    success: false,
                    message: 'セーブデータが見つかりません。'
                };
            }
            
            // データベースからロード
            const saveData = await this.databaseManager.loadSaveData(targetSlot.slotNumber.toString());
            
            if (!saveData) {
                return {
                    success: false,
                    message: 'セーブデータの読み込みに失敗しました。'
                };
            }
            
            // データ展開（圧縮されている場合）
            const decompressedData = this.compressionEnabled 
                ? await this.decompressData(saveData)
                : saveData;
            
            // マイグレーション判定
            const migrationManager = new DataMigrationManager();
            const migratedData = await migrationManager.migrateIfNeeded(decompressedData);
            
            return {
                success: true,
                saveData: migratedData,
                message: 'ゲームを読み込みました。',
                migrationRequired: decompressedData.version !== migratedData.version
            };
            
        } catch (error) {
            return {
                success: false,
                message: '読み込みに失敗しました。',
                error: error.message
            };
        }
    }
    
    private async collectGameState(): Promise<any> {
        return {
            playtime: await this.getPlaytime(),
            player: await this.playerAPI.getPlayer(),
            world: await this.dungeonAPI.getWorldState(),
            progress: await this.getGameProgress(),
            statistics: await this.getGameStatistics(),
            settings: await this.getGameSettings()
        };
    }
}
```

## 🎵 オーディオAPI

### 1. 音響管理API

```typescript
/**
 * オーディオ管理インターフェース
 */
interface AudioAPI {
    // BGM制御
    playBGM(trackName: string, fadeIn?: number): Promise<void>;
    stopBGM(fadeOut?: number): Promise<void>;
    pauseBGM(): Promise<void>;
    resumeBGM(): Promise<void>;
    
    // SE制御
    playSE(soundName: string, volume?: number): Promise<void>;
    stopSE(soundName: string): Promise<void>;
    stopAllSE(): Promise<void>;
    
    // 音量制御
    setBGMVolume(volume: number): Promise<void>;
    setSEVolume(volume: number): Promise<void>;
    setMasterVolume(volume: number): Promise<void>;
    
    // 音声ファイル管理
    preloadAudio(fileList: string[]): Promise<PreloadResult>;
    unloadAudio(fileName: string): Promise<void>;
    
    // 状態取得
    getAudioState(): Promise<AudioState>;
    isPlaying(trackName: string): Promise<boolean>;
}

interface PreloadResult {
    success: boolean;
    loadedFiles: string[];
    failedFiles: string[];
    totalSize: number;
    message: string;
}

interface AudioState {
    bgm: {
        currentTrack: string | null;
        isPlaying: boolean;
        isPaused: boolean;
        volume: number;
        position: number;  // seconds
        duration: number;  // seconds
    };
    se: {
        activeSounds: string[];
        volume: number;
    };
    masterVolume: number;
    isInitialized: boolean;
}

/**
 * 実装例
 */
class AudioManager implements AudioAPI {
    private audioContext: AudioContext | null = null;
    private bgmGain: GainNode | null = null;
    private seGain: GainNode | null = null;
    private currentBGM: AudioBufferSourceNode | null = null;
    private audioBuffers: Map<string, AudioBuffer> = new Map();
    private activeSounds: Map<string, AudioBufferSourceNode> = new Map();
    
    async playBGM(trackName: string, fadeIn: number = 0): Promise<void> {
        if (!this.audioContext) {
            throw new Error('Audio system not initialized');
        }
        
        // 現在のBGMを停止
        if (this.currentBGM) {
            if (fadeIn > 0) {
                await this.fadeOutBGM(500);  // 0.5秒でフェードアウト
            } else {
                this.currentBGM.stop();
            }
        }
        
        // 音声ファイル読み込み
        let audioBuffer = this.audioBuffers.get(trackName);
        if (!audioBuffer) {
            audioBuffer = await this.loadAudioFile(`assets/audio/bgm/${trackName}.mp3`);
            this.audioBuffers.set(trackName, audioBuffer);
        }
        
        // 再生開始
        const source = this.audioContext.createBufferSource();
        source.buffer = audioBuffer;
        source.loop = true;
        source.connect(this.bgmGain!);
        
        // フェードイン処理
        if (fadeIn > 0) {
            this.bgmGain!.gain.setValueAtTime(0, this.audioContext.currentTime);
            this.bgmGain!.gain.linearRampToValueAtTime(
                this.bgmVolume, 
                this.audioContext.currentTime + (fadeIn / 1000)
            );
        }
        
        source.start(0);
        this.currentBGM = source;
        
        // 終了時のクリーンアップ
        source.addEventListener('ended', () => {
            if (this.currentBGM === source) {
                this.currentBGM = null;
            }
        });
    }
    
    async playSE(soundName: string, volume: number = 1.0): Promise<void> {
        if (!this.audioContext) return;
        
        // 音声ファイル読み込み
        let audioBuffer = this.audioBuffers.get(soundName);
        if (!audioBuffer) {
            audioBuffer = await this.loadAudioFile(`assets/audio/se/${soundName}.wav`);
            this.audioBuffers.set(soundName, audioBuffer);
        }
        
        // 再生
        const source = this.audioContext.createBufferSource();
        const gainNode = this.audioContext.createGain();
        
        source.buffer = audioBuffer;
        gainNode.gain.value = volume * this.seVolume;
        
        source.connect(gainNode);
        gainNode.connect(this.seGain!);
        
        source.start(0);
        
        // アクティブサウンド管理
        const soundId = `${soundName}_${Date.now()}`;
        this.activeSounds.set(soundId, source);
        
        source.addEventListener('ended', () => {
            this.activeSounds.delete(soundId);
        });
    }
    
    private async loadAudioFile(url: string): Promise<AudioBuffer> {
        const response = await fetch(url);
        const arrayBuffer = await response.arrayBuffer();
        return await this.audioContext!.decodeAudioData(arrayBuffer);
    }
    
    private async fadeOutBGM(duration: number): Promise<void> {
        if (!this.currentBGM || !this.audioContext) return;
        
        return new Promise((resolve) => {
            const currentGain = this.bgmGain!.gain.value;
            this.bgmGain!.gain.setValueAtTime(currentGain, this.audioContext!.currentTime);
            this.bgmGain!.gain.linearRampToValueAtTime(0, this.audioContext!.currentTime + (duration / 1000));
            
            setTimeout(() => {
                if (this.currentBGM) {
                    this.currentBGM.stop();
                    this.currentBGM = null;
                }
                this.bgmGain!.gain.value = this.bgmVolume;  // 音量を元に戻す
                resolve();
            }, duration);
        });
    }
}
```

---

**API仕様書バージョン**: 1.0  
**最終更新**: 2025年7月26日  
**承認者**: Black Onyx Reborn Development Team  
**実装ガイドライン**: 各APIは型安全性を重視し、エラーハンドリングを適切に実装すること