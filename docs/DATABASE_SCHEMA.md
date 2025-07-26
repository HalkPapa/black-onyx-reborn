# 🗄️ ブラックオニキス復刻版 - データベーススキーマ設計書

## 📋 データベース設計概要

### 設計方針
- **クライアントサイド保存**: LocalStorage + IndexedDBによるハイブリッド構成
- **データ冗長性**: 重要データの複数箇所保存
- **バージョン管理**: スキーマ変更時の互換性維持
- **パフォーマンス**: 高速アクセスのためのインデックス設計

### 保存場所分担
```
LocalStorage (軽量・高速):
├── ゲーム設定
├── プレイヤープロファイル
├── 一時データ
└── 最新セーブスロット情報

IndexedDB (大容量・構造化):
├── セーブデータ（全スロット）
├── ゲーム進行状況
├── マップ探索データ
└── 統計・実績データ

SessionStorage (セッション限定):
├── 戦闘状態
├── UI一時状態
└── キャッシュデータ
```

## 🎮 ゲームデータスキーマ

### 1. メインセーブデータ構造

```typescript
interface SaveData {
    // メタデータ
    saveId: string;                    // 一意識別子
    version: string;                   // スキーマバージョン
    timestamp: number;                 // 最終更新時刻
    slotNumber: number;                // セーブスロット番号（1-10）
    slotName: string;                  // プレイヤー設定スロット名
    playtime: number;                  // 総プレイ時間（ミリ秒）
    
    // プレイヤーデータ
    player: PlayerData;
    
    // ワールド状態
    world: WorldState;
    
    // 進行状況
    progress: GameProgress;
    
    // 統計データ
    statistics: GameStatistics;
    
    // 設定（このセーブデータ固有）
    gameSettings: GameSettings;
}

interface PlayerData {
    // キャラクター情報
    character: {
        id: string;
        name: string;
        level: number;
        experience: number;
        experienceToNext: number;
        
        // 基本能力値
        stats: {
            strength: number;
            intelligence: number;
            agility: number;
            vitality: number;
        };
        
        // 現在値
        hp: number;
        maxHP: number;
        mp: number;
        maxMP: number;
        
        // 外見
        appearance: {
            hairStyle: string;          // 'short' | 'long' | 'curly' | 'bald' | 'ponytail'
            hairColor: string;          // 'black' | 'brown' | 'blonde' | 'red'
            clothingColor: string;      // 8色から選択
            gender: string;             // 'male' | 'female'
        };
        
        // 状態異常
        conditions: string[];           // ['poison', 'sleep', 'paralysis']
        conditionDurations: number[];   // 対応する継続時間
        
        // 職業（将来拡張用）
        profession: string;             // 'fighter'（現在は戦士のみ）
    };
    
    // 装備
    equipment: {
        weapon: ItemInstance | null;
        armor: ItemInstance | null;
        shield: ItemInstance | null;
        helmet: ItemInstance | null;
        accessory: ItemInstance | null;
    };
    
    // インベントリ
    inventory: ItemInstance[];
    gold: number;
    
    // 魔法（将来拡張用）
    knownSpells: string[];              // 習得魔法ID配列
}

interface ItemInstance {
    id: string;                         // アイテムマスターID
    instanceId: string;                 // インスタンス固有ID
    quantity: number;                   // 個数（スタック可能アイテム用）
    durability: number;                 // 現在耐久度
    maxDurability: number;              // 最大耐久度
    enchantments: Enchantment[];        // 付与効果（将来拡張用）
    acquisitionTime: number;            // 取得時刻
}

interface Enchantment {
    type: string;                       // 'attack_bonus' | 'defense_bonus' | etc.
    value: number;                      // 効果値
    duration: number;                   // 継続時間（-1で永続）
}
```

### 2. ワールド状態データ

```typescript
interface WorldState {
    // 現在位置
    currentFloor: string;               // 'B1' | 'B2' | ... | 'TOWER1' | 'TOWER2'
    position: {
        x: number;
        y: number;
    };
    direction: number;                  // 0=北, 1=東, 2=南, 3=西
    
    // 探索済みエリア
    exploredTiles: ExploredTile[];
    
    // 発見済み隠し要素
    discoveredSecrets: DiscoveredSecret[];
    
    // オブジェクト状態（宝箱・扉等）
    objectStates: ObjectState[];
    
    // カラー迷路進行状況
    colorMazeProgress: {
        currentStep: number;            // 現在のステップ（0-3）
        playerSequence: string[];       // プレイヤーが通った色順序
        attempts: number;               // 挑戦回数
        isCompleted: boolean;           // クリア済みかどうか
    };
    
    // イベントフラグ
    eventFlags: { [key: string]: boolean };
    
    // 敵撃破記録
    defeatedEnemies: DefeatedEnemy[];
}

interface ExploredTile {
    floor: string;
    x: number;
    y: number;
    tileType: number;                   // タイルタイプ
    exploredAt: number;                 // 探索時刻
    hasBeenSearched: boolean;           // 調べる実行済み
}

interface DiscoveredSecret {
    floor: string;
    x: number;
    y: number;
    type: string;                       // 'hidden_door' | 'secret_treasure' | 'trap'
    discoveredAt: number;
    hasBeenUsed: boolean;
}

interface ObjectState {
    floor: string;
    x: number;
    y: number;
    objectId: string;
    state: string;                      // 'open' | 'closed' | 'locked' | 'broken'
    interactionCount: number;
    lastInteractionTime: number;
}

interface DefeatedEnemy {
    enemyId: string;
    floor: string;
    position: { x: number; y: number };
    defeatedAt: number;
    respawnTime: number;                // リスポーン時刻（-1で永続撃破）
}
```

### 3. ゲーム進行状況データ

```typescript
interface GameProgress {
    // メインクエスト進行
    mainQuestPhase: string;             // 'prologue' | 'exploration' | 'color_maze' | 'tower' | 'ending'
    completedPhases: string[];
    
    // 到達済みフロア
    reachedFloors: string[];
    
    // ボス撃破状況
    bossesDefeated: {
        [floorId: string]: {
            defeated: boolean;
            defeatedAt: number;
            attempts: number;
        };
    };
    
    // 重要アイテム取得状況
    keyItems: KeyItem[];
    
    // 達成実績
    achievements: Achievement[];
    
    // 難易度設定
    difficultyLevel: string;            // 'easy' | 'normal' | 'hard'
    
    // ゲーム設定フラグ
    gameOptions: {
        tutorialCompleted: boolean;
        autoSaveEnabled: boolean;
        quickSaveSlot: number;
    };
}

interface KeyItem {
    id: string;
    name: string;
    obtainedAt: number;
    obtainedFloor: string;
    isActive: boolean;                  // 現在有効かどうか
}

interface Achievement {
    id: string;
    unlockedAt: number;
    progress: number;                   // 進行度（0-100）
    isVisible: boolean;                 // プレイヤーに表示済み
}
```

### 4. 統計データ

```typescript
interface GameStatistics {
    // 基本統計
    totalPlaytime: number;              // 総プレイ時間
    saveCount: number;                  // セーブ回数
    loadCount: number;                  // ロード回数
    gameStartTime: number;              // ゲーム開始時刻
    lastPlayTime: number;               // 最終プレイ時刻
    
    // 戦闘統計
    battle: {
        totalBattles: number;           // 総戦闘回数
        battlesWon: number;             // 勝利回数
        battlesLost: number;            // 敗北回数
        battlesEscaped: number;         // 逃走成功回数
        totalDamageDealt: number;       // 与えた総ダメージ
        totalDamageTaken: number;       // 受けた総ダメージ
        criticalHits: number;           // クリティカルヒット回数
    };
    
    // 探索統計
    exploration: {
        tilesExplored: number;          // 探索済みタイル数
        secretsFound: number;           // 発見した秘密の数
        treasuresOpened: number;        // 開けた宝箱数
        trapsTriggered: number;         // 引っかかった罠数
        hiddenDoorsFound: number;       // 発見した隠し扉数
        stepsWalked: number;            // 歩行ステップ数
    };
    
    // アイテム統計
    items: {
        itemsAcquired: number;          // 取得アイテム総数
        goldEarned: number;             // 獲得総金額
        goldSpent: number;              // 使用総金額
        potionsUsed: number;            // 使用した回復薬数
        equipmentBroken: number;        // 破損した装備数
    };
    
    // モンスター統計
    monsters: {
        [monsterId: string]: {
            encountered: number;        // 遭遇回数
            defeated: number;           // 撃破回数
            escapedFrom: number;        // 逃走回数
            totalDamageDealt: number;   // この敵に与えた総ダメージ
            totalDamageTaken: number;   // この敵から受けた総ダメージ
        };
    };
    
    // レベル別統計
    levelProgress: {
        [level: number]: {
            reachedAt: number;          // レベル到達時刻
            timeSpent: number;          // このレベルでの滞在時間
            battlesAtThisLevel: number; // このレベルでの戦闘回数
        };
    };
}
```

## 🏗️ マスターデータ構造

### 1. アイテムマスターデータ

```typescript
interface ItemMaster {
    id: string;                         // 一意識別子
    name: string;                       // 表示名
    description: string;                // 説明文
    
    // 分類
    category: string;                   // 'weapon' | 'armor' | 'consumable' | 'key_item'
    subcategory: string;                // 'sword' | 'axe' | 'light_armor' | 'potion'
    rarity: string;                     // 'common' | 'uncommon' | 'rare' | 'legendary'
    
    // 基本プロパティ
    stackable: boolean;                 // スタック可能か
    maxStack: number;                   // 最大スタック数
    weight: number;                     // 重量
    basePrice: number;                  // 基本価格
    
    // 装備品プロパティ
    equipSlot?: string;                 // 装備スロット
    attackPower?: number;               // 攻撃力
    defensePower?: number;              // 防御力
    accuracy?: number;                  // 命中率
    durability?: number;                // 耐久度
    
    // 使用効果
    usageEffect?: {
        type: string;                   // 'heal' | 'restore_mp' | 'cure_poison'
        power: number;                  // 効果量
        target: string;                 // 'self' | 'single' | 'all'
        consumeOnUse: boolean;          // 使用時に消費するか
    };
    
    // 要求値
    requirements?: {
        level?: number;                 // 必要レベル
        stats?: {
            strength?: number;
            intelligence?: number;
            agility?: number;
            vitality?: number;
        };
    };
    
    // アセット情報
    sprites: {
        icon: string;                   // アイコン画像
        equipped?: string;              // 装備時画像
        dropped?: string;               // ドロップ時画像
    };
    
    // 音響
    sounds?: {
        equip?: string;                 // 装備音
        use?: string;                   // 使用音
        break?: string;                 // 破損音
    };
}
```

### 2. モンスターマスターデータ

```typescript
interface MonsterMaster {
    id: string;
    name: string;
    description: string;
    
    // 基本ステータス
    level: number;
    stats: {
        hp: number;
        attack: number;
        defense: number;
        agility: number;
    };
    
    // 戦闘行動
    behavior: {
        aggressiveness: number;         // 攻撃性（0-100）
        intelligence: number;           // 知能（行動選択に影響）
        groupSize: [number, number];    // 出現数の範囲
        fleeThreshold: number;          // 逃走開始HP比率
    };
    
    // 特殊能力
    specialAbilities?: SpecialAbility[];
    
    // ドロップ・報酬
    rewards: {
        experience: number;
        gold: number;
        dropRate: number;               // アイテムドロップ確率
        possibleDrops: DropItem[];      // ドロップ可能アイテム
    };
    
    // 出現条件
    spawnConditions: {
        floors: string[];               // 出現フロア
        encounterRate: number;          // 遭遇率
        timeOfDay?: string;             // 時間帯制限（将来拡張用）
        prerequisites?: string[];       // 出現前提条件
    };
    
    // アセット
    sprites: {
        idle: string;
        attack: string;
        hurt: string;
        death: string;
        special?: string;
    };
    
    sounds: {
        encounter: string;
        attack: string;
        hurt: string;
        death: string;
        special?: string;
    };
}

interface SpecialAbility {
    id: string;
    name: string;
    type: string;                       // 'attack' | 'defense' | 'status' | 'summon'
    power: number;
    mpCost: number;
    cooldown: number;                   // クールダウン（ターン数）
    targetType: string;                 // 'single' | 'all' | 'self'
    conditions?: string[];              // 発動条件
    effects: AbilityEffect[];
}

interface AbilityEffect {
    type: string;                       // 'damage' | 'heal' | 'poison' | 'stun'
    value: number;
    duration: number;                   // 継続ターン数
    probability: number;                // 発動確率
}

interface DropItem {
    itemId: string;
    quantity: [number, number];         // 個数範囲
    probability: number;                // ドロップ確率
    conditions?: string[];              // ドロップ条件
}
```

### 3. マップデータ構造

```typescript
interface MapData {
    id: string;                         // 'B1', 'B2', 'TOWER1' 等
    name: string;                       // 表示名
    description: string;                // 説明
    
    // マップサイズ
    dimensions: {
        width: number;
        height: number;
    };
    
    // タイルデータ
    tiles: number[][];                  // タイプマトリックス
    
    // 配置オブジェクト
    objects: MapObject[];
    
    // 敵出現データ
    encounters: EncounterZone[];
    
    // 環境設定
    environment: {
        bgm: string;                    // BGMファイル
        ambientSound?: string;          // 環境音
        lightLevel: number;             // 明度（0-100）
        temperature: number;            // 温度（効果音用）
        hazards?: EnvironmentHazard[];  // 環境ハザード
    };
    
    // 接続情報
    connections: MapConnection[];
    
    // 特殊ルール
    specialRules?: {
        colorMaze?: ColorMazeRule;      // カラー迷路ルール
        bossFloor?: BossFloorRule;      // ボスフロアルール
        puzzles?: PuzzleRule[];         // パズルルール
    };
}

interface MapObject {
    id: string;
    type: string;                       // 'door' | 'treasure' | 'stairs' | 'switch'
    position: { x: number; y: number };
    
    // 状態プロパティ
    initialState: string;               // 初期状態
    possibleStates: string[];           // 取りうる状態
    
    // インタラクション
    interactions: ObjectInteraction[];
    
    // 条件
    requirements?: {
        keyItem?: string;               // 必要キーアイテム
        level?: number;                 // 必要レベル
        flags?: string[];               // 必要フラグ
    };
    
    // アセット
    sprites: {
        [state: string]: string;        // 状態別スプライト
    };
    
    sounds?: {
        [interaction: string]: string;  // インタラクション別音響
    };
}

interface ObjectInteraction {
    action: string;                     // 'open' | 'search' | 'use' | 'push'
    result: InteractionResult;
    conditions?: string[];              // 実行条件
    consumeOnUse?: boolean;             // 一回限りか
}

interface InteractionResult {
    type: string;                       // 'state_change' | 'item_gain' | 'teleport' | 'battle'
    data: any;                          // 結果データ
    message?: string;                   // 表示メッセージ
}

interface EncounterZone {
    area: {
        x: number;
        y: number;
        width: number;
        height: number;
    };
    
    encounters: {
        enemies: string[];              // 敵ID配列
        probability: number;            // 遭遇確率
        conditions?: string[];          // 遭遇条件
    }[];
    
    encounterRate: number;              // 基本遭遇率（ステップあたり）
}

interface MapConnection {
    from: { x: number; y: number };
    to: {
        mapId: string;
        x: number;
        y: number;
        direction?: number;             // プレイヤーの向き
    };
    
    type: string;                       // 'stairs_down' | 'stairs_up' | 'teleporter'
    requirements?: string[];            // 移動条件
    animation?: string;                 // 移動アニメーション
}
```

## 💾 データアクセス層設計

### 1. データベースアクセスクラス

```typescript
class DatabaseManager {
    private indexedDB: IDBDatabase | null = null;
    private readonly DB_NAME = 'BlackOnyxReborn';
    private readonly DB_VERSION = 1;
    
    async initialize(): Promise<void> {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.DB_NAME, this.DB_VERSION);
            
            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.indexedDB = request.result;
                resolve();
            };
            
            request.onupgradeneeded = (event) => {
                const db = (event.target as IDBOpenDBRequest).result;
                this.createObjectStores(db);
            };
        });
    }
    
    private createObjectStores(db: IDBDatabase): void {
        // セーブデータストア
        if (!db.objectStoreNames.contains('saves')) {
            const saveStore = db.createObjectStore('saves', { keyPath: 'saveId' });
            saveStore.createIndex('slotNumber', 'slotNumber', { unique: true });
            saveStore.createIndex('timestamp', 'timestamp', { unique: false });
        }
        
        // 統計データストア
        if (!db.objectStoreNames.contains('statistics')) {
            const statsStore = db.createObjectStore('statistics', { keyPath: 'saveId' });
        }
        
        // 設定データストア
        if (!db.objectStoreNames.contains('settings')) {
            const settingsStore = db.createObjectStore('settings', { keyPath: 'key' });
        }
        
        // キャッシュストア
        if (!db.objectStoreNames.contains('cache')) {
            const cacheStore = db.createObjectStore('cache', { keyPath: 'key' });
            cacheStore.createIndex('expires', 'expires', { unique: false });
        }
    }
    
    // セーブデータ操作
    async saveSaveData(saveData: SaveData): Promise<void> {
        const transaction = this.indexedDB!.transaction(['saves'], 'readwrite');
        const store = transaction.objectStore('saves');
        
        await new Promise<void>((resolve, reject) => {
            const request = store.put(saveData);
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
        
        // LocalStorageにも最新情報を保存
        localStorage.setItem(`quickSave_${saveData.slotNumber}`, JSON.stringify({
            saveId: saveData.saveId,
            timestamp: saveData.timestamp,
            playerName: saveData.player.character.name,
            level: saveData.player.character.level,
            floor: saveData.world.currentFloor,
            playtime: saveData.playtime
        }));
    }
    
    async loadSaveData(saveId: string): Promise<SaveData | null> {
        const transaction = this.indexedDB!.transaction(['saves'], 'readonly');
        const store = transaction.objectStore('saves');
        
        return new Promise<SaveData | null>((resolve, reject) => {
            const request = store.get(saveId);
            request.onsuccess = () => resolve(request.result || null);
            request.onerror = () => reject(request.error);
        });
    }
    
    async getSaveList(): Promise<SaveSummary[]> {
        const transaction = this.indexedDB!.transaction(['saves'], 'readonly');
        const store = transaction.objectStore('saves');
        const index = store.index('timestamp');
        
        return new Promise<SaveSummary[]>((resolve, reject) => {
            const request = index.getAll();
            request.onsuccess = () => {
                const saves = request.result
                    .map(save => ({
                        saveId: save.saveId,
                        slotNumber: save.slotNumber,
                        slotName: save.slotName,
                        playerName: save.player.character.name,
                        level: save.player.character.level,
                        floor: save.world.currentFloor,
                        playtime: save.playtime,
                        timestamp: save.timestamp
                    }))
                    .sort((a, b) => b.timestamp - a.timestamp);
                
                resolve(saves);
            };
            request.onerror = () => reject(request.error);
        });
    }
    
    // 統計データ操作
    async saveStatistics(saveId: string, stats: GameStatistics): Promise<void> {
        const transaction = this.indexedDB!.transaction(['statistics'], 'readwrite');
        const store = transaction.objectStore('statistics');
        
        await new Promise<void>((resolve, reject) => {
            const request = store.put({ saveId, ...stats });
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    }
    
    // 設定データ操作
    async saveSetting(key: string, value: any): Promise<void> {
        localStorage.setItem(`setting_${key}`, JSON.stringify(value));
    }
    
    async loadSetting<T>(key: string, defaultValue: T): Promise<T> {
        const stored = localStorage.getItem(`setting_${key}`);
        return stored ? JSON.parse(stored) : defaultValue;
    }
}

interface SaveSummary {
    saveId: string;
    slotNumber: number;
    slotName: string;
    playerName: string;
    level: number;
    floor: string;
    playtime: number;
    timestamp: number;
}
```

### 2. キャッシュ管理システム

```typescript
class CacheManager {
    private memoryCache = new Map<string, CacheEntry>();
    private readonly MAX_MEMORY_CACHE_SIZE = 100;
    
    async get<T>(key: string): Promise<T | null> {
        // メモリキャッシュを先に確認
        if (this.memoryCache.has(key)) {
            const entry = this.memoryCache.get(key)!;
            if (entry.expires > Date.now()) {
                return entry.data as T;
            } else {
                this.memoryCache.delete(key);
            }
        }
        
        // SessionStorageを確認
        const sessionData = sessionStorage.getItem(`cache_${key}`);
        if (sessionData) {
            const parsed = JSON.parse(sessionData);
            if (parsed.expires > Date.now()) {
                this.setMemoryCache(key, parsed.data, parsed.expires);
                return parsed.data as T;
            } else {
                sessionStorage.removeItem(`cache_${key}`);
            }
        }
        
        return null;
    }
    
    async set<T>(key: string, data: T, ttlMs: number = 300000): Promise<void> {
        const expires = Date.now() + ttlMs;
        
        // メモリキャッシュに保存
        this.setMemoryCache(key, data, expires);
        
        // SessionStorageに保存
        sessionStorage.setItem(`cache_${key}`, JSON.stringify({
            data,
            expires
        }));
    }
    
    private setMemoryCache<T>(key: string, data: T, expires: number): void {
        // サイズ制限チェック
        if (this.memoryCache.size >= this.MAX_MEMORY_CACHE_SIZE) {
            const oldestKey = Array.from(this.memoryCache.keys())[0];
            this.memoryCache.delete(oldestKey);
        }
        
        this.memoryCache.set(key, { data, expires });
    }
    
    clear(): void {
        this.memoryCache.clear();
        
        // SessionStorageのキャッシュエントリをクリア
        Object.keys(sessionStorage)
            .filter(key => key.startsWith('cache_'))
            .forEach(key => sessionStorage.removeItem(key));
    }
}

interface CacheEntry {
    data: any;
    expires: number;
}
```

## 🔧 データマイグレーション

### バージョン管理・マイグレーション

```typescript
class DataMigrationManager {
    private readonly CURRENT_VERSION = '1.0.0';
    
    async migrateIfNeeded(saveData: any): Promise<SaveData> {
        const version = saveData.version || '0.0.0';
        
        if (version === this.CURRENT_VERSION) {
            return saveData as SaveData;
        }
        
        console.log(`Migrating save data from ${version} to ${this.CURRENT_VERSION}`);
        
        let migrated = saveData;
        
        // バージョン別マイグレーション
        if (this.compareVersions(version, '1.0.0') < 0) {
            migrated = await this.migrateTo1_0_0(migrated);
        }
        
        migrated.version = this.CURRENT_VERSION;
        return migrated as SaveData;
    }
    
    private async migrateTo1_0_0(oldData: any): Promise<any> {
        // 初期バージョンからの移行処理
        return {
            ...oldData,
            // 新しいフィールドのデフォルト値設定
            progress: oldData.progress || {
                mainQuestPhase: 'prologue',
                completedPhases: [],
                reachedFloors: ['B1'],
                bossesDefeated: {},
                keyItems: [],
                achievements: [],
                difficultyLevel: 'normal',
                gameOptions: {
                    tutorialCompleted: false,
                    autoSaveEnabled: true,
                    quickSaveSlot: 1
                }
            },
            statistics: oldData.statistics || this.createDefaultStatistics()
        };
    }
    
    private compareVersions(a: string, b: string): number {
        const aVersions = a.split('.').map(Number);
        const bVersions = b.split('.').map(Number);
        
        for (let i = 0; i < Math.max(aVersions.length, bVersions.length); i++) {
            const aVersion = aVersions[i] || 0;
            const bVersion = bVersions[i] || 0;
            
            if (aVersion < bVersion) return -1;
            if (aVersion > bVersion) return 1;
        }
        
        return 0;
    }
    
    private createDefaultStatistics(): GameStatistics {
        return {
            totalPlaytime: 0,
            saveCount: 0,
            loadCount: 0,
            gameStartTime: Date.now(),
            lastPlayTime: Date.now(),
            battle: {
                totalBattles: 0,
                battlesWon: 0,
                battlesLost: 0,
                battlesEscaped: 0,
                totalDamageDealt: 0,
                totalDamageTaken: 0,
                criticalHits: 0
            },
            exploration: {
                tilesExplored: 0,
                secretsFound: 0,
                treasuresOpened: 0,
                trapsTriggered: 0,
                hiddenDoorsFound: 0,
                stepsWalked: 0
            },
            items: {
                itemsAcquired: 0,
                goldEarned: 0,
                goldSpent: 0,
                potionsUsed: 0,
                equipmentBroken: 0
            },
            monsters: {},
            levelProgress: {}
        };
    }
}
```

---

**データベーススキーマ設計書バージョン**: 1.0  
**最終更新**: 2025年7月26日  
**承認者**: Black Onyx Reborn Development Team  
**実装開始**: 詳細設計承認後