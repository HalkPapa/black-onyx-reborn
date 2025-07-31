# 🔧 ブラックオニキス復刻版 - 技術仕様書

## 📋 システム要件・アーキテクチャ

### 復刻版技術スタック
```
フロントエンド:
├── HTML5 Canvas（ゲーム描画）
├── JavaScript ES6+（ゲームロジック）
├── CSS3（UI・レスポンシブ）
├── Web Audio API（音響システム）
└── Service Worker（PWA・オフライン対応）

データ管理:
├── LocalStorage（設定・軽量データ）
├── IndexedDB（セーブデータ・重要データ）
├── SessionStorage（一時データ）
└── File API（セーブファイルエクスポート）

開発・ビルド:
├── Webpack（モジュール管理）
├── Babel（ES6+トランスパイル）
├── ESLint + Prettier（コード品質）
└── Jest（テスティング）
```

## 🎮 ゲームエンジン設計

### コアエンジンアーキテクチャ
```javascript
// ゲームエンジン基本構造
class BlackOnyxEngine {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.systems = {
            renderer: new RenderingSystem(),
            input: new InputSystem(),
            audio: new AudioSystem(),
            save: new SaveSystem(),
            ui: new UISystem()
        };
    }
    
    init() {
        // システム初期化
        // ゲーム状態設定
        // メインループ開始
    }
    
    gameLoop() {
        // 入力処理
        // ゲーム状態更新
        // 描画処理
        requestAnimationFrame(() => this.gameLoop());
    }
}
```

### レンダリングシステム設計
```javascript
// 3Dダンジョン描画システム
class DungeonRenderer {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.viewDistance = 5; // 表示距離
        this.tileSize = 64;
    }
    
    renderDungeon(player, dungeon) {
        // 疑似3D表現実装
        this.clearCanvas();
        this.drawWalls(player.position, player.direction);
        this.drawFloor();
        this.drawCeiling();
        this.drawObjects(dungeon.getVisibleObjects());
    }
    
    // オリジナル版の疑似3D再現
    drawWalls(position, direction) {
        const visibleTiles = this.getVisibleTiles(position, direction);
        
        // 距離別描画（遠→近）
        for (let distance = this.viewDistance; distance >= 0; distance--) {
            this.drawWallLayer(visibleTiles[distance]);
        }
    }
}
```

## 🏰 ダンジョンシステム実装

### マップデータ構造
```javascript
// ダンジョンマップ定義
const DUNGEON_MAPS = {
    B1: {
        size: { width: 20, height: 20 },
        tiles: [
            // 0=床, 1=壁, 2=扉, 3=隠し扉, 4=宝箱, 5=階段
            [1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1],
            [1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,1],
            // ... マップデータ
        ],
        enemies: [
            { type: 'bat', x: 5, y: 7, level: 1 },
            { type: 'kobold', x: 10, y: 12, level: 1, group: 3 }
        ],
        items: [
            { type: 'treasure', x: 15, y: 8, contents: ['gold', 50] }
        ],
        specialAreas: [
            { type: 'stairs_down', x: 18, y: 18, target: 'B2' }
        ]
    }
    // B2-B6, TOWER1-2のマップ定義
};

// ダンジョン管理クラス
class DungeonManager {
    constructor() {
        this.currentFloor = 'B1';
        this.maps = DUNGEON_MAPS;
        this.visited = new Set();
    }
    
    getTile(floor, x, y) {
        return this.maps[floor].tiles[y][x];
    }
    
    isWall(floor, x, y) {
        return this.getTile(floor, x, y) === 1;
    }
    
    hasHiddenDoor(floor, x, y) {
        return this.getTile(floor, x, y) === 3;
    }
}
```

### カラー迷路システム
```javascript
// B6カラー迷路実装
class ColorMazeSystem {
    constructor() {
        // 機種別色順統一（PC-8801基準）
        this.colorSequence = ['red', 'blue', 'yellow', 'green'];
        this.playerSequence = [];
        this.correctSequence = [2, 0, 1, 3]; // イロイッカイズツ
    }
    
    processColorStep(color) {
        this.playerSequence.push(color);
        
        if (this.playerSequence.length === 4) {
            return this.checkSequence();
        }
        
        return 'continue';
    }
    
    checkSequence() {
        const isCorrect = this.playerSequence.every(
            (color, index) => color === this.correctSequence[index]
        );
        
        if (isCorrect) {
            return 'success'; // ブラックタワーへ
        } else {
            this.playerSequence = [];
            return 'reset'; // B1へ戻る
        }
    }
}
```

## ⚔️ 戦闘システム実装

### 戦闘管理システム
```javascript
class BattleSystem {
    constructor() {
        this.state = 'none'; // none, encounter, battle, victory, defeat
        this.enemies = [];
        this.party = [];
        this.turnOrder = [];
    }
    
    startBattle(enemyTypes, playerParty) {
        this.state = 'encounter';
        this.enemies = this.generateEnemies(enemyTypes);
        this.party = playerParty;
        this.calculateTurnOrder();
        
        return {
            message: `${this.enemies.length}体の敵が現れた！`,
            enemies: this.enemies.map(e => e.name)
        };
    }
    
    processPlayerAction(action) {
        switch(action.type) {
            case 'attack':
                return this.executeAttack(action.attacker, action.target);
            case 'defend':
                return this.executeDefend(action.character);
            case 'run':
                return this.attemptRun();
        }
    }
    
    executeAttack(attacker, target) {
        const weapon = attacker.equipment.weapon;
        const damage = this.calculateDamage(weapon, attacker, target);
        
        target.hp -= damage;
        
        return {
            success: true,
            damage: damage,
            message: `${attacker.name}の攻撃！ ${damage}のダメージ！`
        };
    }
    
    calculateDamage(weapon, attacker, target) {
        // オリジナル版に忠実な固定ダメージ制
        const baseDamage = weapon ? weapon.attack : 1;
        const randomFactor = Math.random() * 0.3 + 0.85; // ±15%のランダム
        const defense = target.equipment ? target.equipment.armor?.defense || 0 : 0;
        
        return Math.max(1, Math.floor(baseDamage * randomFactor - defense));
    }
}
```

### 敵データベース
```javascript
const MONSTER_DATABASE = {
    bat: {
        name: 'コウモリ',
        hp: 3,
        attack: 1,
        defense: 0,
        exp: 1,
        gold: 1,
        dropRate: 0.1
    },
    kobold: {
        name: 'コボルト',
        hp: 8,
        attack: 3,
        defense: 1,
        exp: 5,
        gold: 8,
        dropRate: 0.2,
        groupSize: [1, 5] // 1-5体で出現
    },
    cobra: {
        name: 'コブラ',
        hp: 15,
        attack: 8,
        defense: 2,
        exp: 25,
        gold: 40,
        dropRate: 0.3,
        special: 'poison' // 毒攻撃
    },
    giant: {
        name: 'ジャイアント',
        hp: 200,
        attack: 50,
        defense: 10,
        exp: 500,
        gold: 1000,
        dropRate: 0.8,
        boss: true
    }
};
```

## 👤 キャラクターシステム実装

### キャラクター管理クラス
```javascript
class Character {
    constructor(name, appearance) {
        this.name = name;
        this.appearance = appearance;
        
        // 基本パラメータ（オリジナル版準拠）
        this.stats = {
            strength: this.generateStat(),
            dexterity: this.generateStat(),
            health: this.generateStat()
        };
        
        // 計算パラメータ
        this.level = 1;
        this.hp = this.calculateMaxHP();
        this.maxHP = this.hp;
        this.exp = 0;
        
        // 装備
        this.equipment = {
            weapon: null,
            armor: null,
            shield: null,
            helmet: null
        };
        
        // インベントリ
        this.inventory = [];
        this.gold = 100; // 初期所持金
    }
    
    generateStat() {
        // オリジナル版の隠しパラメータ再現
        return Math.floor(Math.random() * 15) + 5; // 5-19
    }
    
    calculateMaxHP() {
        return this.stats.health + (this.level - 1) * 3;
    }
    
    gainExperience(amount) {
        this.exp += amount;
        
        // レベルアップ判定（簡略化）
        const nextLevelExp = this.level * this.level * 10;
        if (this.exp >= nextLevelExp) {
            this.levelUp();
        }
    }
    
    levelUp() {
        this.level++;
        const oldMaxHP = this.maxHP;
        this.maxHP = this.calculateMaxHP();
        this.hp += (this.maxHP - oldMaxHP); // 全回復
        
        return {
            message: `${this.name}はレベル${this.level}になった！`,
            hpGain: this.maxHP - oldMaxHP
        };
    }
}
```

## 🎒 アイテム・装備システム

### アイテムデータベース
```javascript
const ITEM_DATABASE = {
    weapons: {
        knife: {
            name: 'ナイフ',
            attack: 2,
            accuracy: 90,
            weight: 1,
            price: 10,
            description: '小さな刃物。軽くて扱いやすい。'
        },
        short_sword: {
            name: 'ショートソード',
            attack: 10,
            accuracy: 90,
            weight: 3,
            price: 80,
            description: '短い剣。バランスが良い。'
        },
        broad_sword: {
            name: 'ブロードソード',
            attack: 20,
            accuracy: 80,
            weight: 9,
            price: 640,
            description: '幅広の剣。高い攻撃力を持つ。'
        }
    },
    armor: {
        leather_armor: {
            name: 'レザーアーマー',
            defense: 2,
            weight: 3,
            price: 40,
            description: '革製の軽装鎧。'
        },
        chain_mail: {
            name: 'チェインメイル',
            defense: 5,
            weight: 8,
            price: 320,
            description: '鎖帷子。優れた防御力。'
        }
    },
    consumables: {
        heal_potion: {
            name: '回復薬',
            effect: 'heal',
            power: 20,
            price: 50,
            description: 'HPを20回復する。'
        }
    }
};

// インベントリ管理システム
class InventorySystem {
    constructor(character) {
        this.character = character;
        this.maxWeight = character.stats.strength * 10;
    }
    
    canCarry(item) {
        const currentWeight = this.getCurrentWeight();
        return currentWeight + item.weight <= this.maxWeight;
    }
    
    addItem(itemId, quantity = 1) {
        const item = { ...ITEM_DATABASE.getItem(itemId), quantity };
        
        if (!this.canCarry(item)) {
            return { success: false, message: '重すぎて持てません。' };
        }
        
        this.character.inventory.push(item);
        return { success: true, message: `${item.name}を手に入れた。` };
    }
}
```

## 💾 セーブ・ロードシステム

### データ永続化システム
```javascript
class SaveSystem {
    constructor() {
        this.SAVE_KEY = 'black_onyx_save';
        this.CONFIG_KEY = 'black_onyx_config';
    }
    
    saveGame(gameState) {
        const saveData = {
            version: '1.0',
            timestamp: Date.now(),
            player: this.serializePlayer(gameState.player),
            party: gameState.party.map(char => this.serializeCharacter(char)),
            dungeon: {
                currentFloor: gameState.currentFloor,
                position: gameState.playerPosition,
                direction: gameState.playerDirection,
                visited: Array.from(gameState.visitedTiles)
            },
            inventory: gameState.inventory,
            flags: gameState.gameFlags
        };
        
        try {
            // メインセーブ（IndexedDB）
            this.saveToIndexedDB(saveData);
            
            // バックアップ（LocalStorage）
            localStorage.setItem(this.SAVE_KEY, JSON.stringify(saveData));
            
            return { success: true, message: 'ゲームを保存しました。' };
        } catch (error) {
            return { success: false, message: '保存に失敗しました。' };
        }
    }
    
    loadGame() {
        try {
            // メインからロード
            const saveData = this.loadFromIndexedDB() || 
                            JSON.parse(localStorage.getItem(this.SAVE_KEY));
            
            if (!saveData) {
                return null;
            }
            
            return this.deserializeSaveData(saveData);
        } catch (error) {
            console.error('Load error:', error);
            return null;
        }
    }
    
    async saveToIndexedDB(data) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open('BlackOnyxDB', 1);
            
            request.onsuccess = (event) => {
                const db = event.target.result;
                const transaction = db.transaction(['saves'], 'readwrite');
                const store = transaction.objectStore('saves');
                
                store.put({ id: 1, data: data });
                transaction.oncomplete = () => resolve();
            };
            
            request.onerror = () => reject(request.error);
        });
    }
}
```

## 🔊 オーディオシステム

### Web Audio API実装
```javascript
class AudioSystem {
    constructor() {
        this.audioContext = null;
        this.bgmGain = null;
        this.seGain = null;
        this.currentBGM = null;
        
        this.bgmVolume = 0.7;
        this.seVolume = 0.8;
    }
    
    async init() {
        try {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            
            // ゲインノード作成
            this.bgmGain = this.audioContext.createGain();
            this.seGain = this.audioContext.createGain();
            
            this.bgmGain.connect(this.audioContext.destination);
            this.seGain.connect(this.audioContext.destination);
            
            this.setBGMVolume(this.bgmVolume);
            this.setSEVolume(this.seVolume);
            
            // 音声ファイル事前読み込み
            await this.preloadAudio();
            
        } catch (error) {
            console.warn('Audio initialization failed:', error);
        }
    }
    
    async playBGM(trackName) {
        if (!this.audioContext) return;
        
        try {
            // 現在のBGM停止
            if (this.currentBGM) {
                this.currentBGM.stop();
            }
            
            const audioBuffer = await this.loadAudioFile(`assets/audio/bgm/${trackName}.mp3`);
            const source = this.audioContext.createBufferSource();
            
            source.buffer = audioBuffer;
            source.loop = true;
            source.connect(this.bgmGain);
            source.start(0);
            
            this.currentBGM = source;
            
        } catch (error) {
            console.warn('BGM playback failed:', error);
        }
    }
    
    playSE(soundName) {
        if (!this.audioContext) return;
        
        this.loadAudioFile(`assets/audio/se/${soundName}.mp3`)
            .then(audioBuffer => {
                const source = this.audioContext.createBufferSource();
                source.buffer = audioBuffer;
                source.connect(this.seGain);
                source.start(0);
            })
            .catch(error => console.warn('SE playback failed:', error));
    }
}
```

## 📱 レスポンシブ・PWA対応

### Progressive Web App設定
```javascript
// Service Worker登録
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js')
        .then(registration => {
            console.log('SW registered:', registration);
        })
        .catch(error => {
            console.log('SW registration failed:', error);
        });
}

// Service Worker実装（sw.js）
const CACHE_NAME = 'black-onyx-v1';
const urlsToCache = [
    '/',
    '/index.html',
    '/src/main.js',
    '/public/css/main.css',
    '/assets/audio/bgm/title.mp3'
    // 必要なアセット
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});
```

### レスポンシブデザイン
```css
/* モバイル対応CSS */
@media (max-width: 768px) {
    .game-container {
        flex-direction: column;
        height: 100vh;
    }
    
    #game-canvas {
        width: 100%;
        height: 60vh;
        touch-action: none;
    }
    
    .game-ui {
        height: 40vh;
        overflow-y: auto;
    }
    
    .command-menu {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 10px;
        padding: 10px;
    }
    
    .command-btn {
        padding: 15px;
        font-size: 16px;
        touch-action: manipulation;
    }
}

/* タッチ操作対応 */
.touch-controls {
    position: fixed;
    bottom: 20px;
    right: 20px;
    display: none;
}

@media (hover: none) and (pointer: coarse) {
    .touch-controls {
        display: block;
    }
}
```

## 🧪 テスト・品質保証

### ユニットテスト例
```javascript
// Jest テスト例
describe('BattleSystem', () => {
    let battleSystem;
    
    beforeEach(() => {
        battleSystem = new BattleSystem();
    });
    
    test('should calculate damage correctly', () => {
        const weapon = { attack: 10 };
        const attacker = { stats: { strength: 15 } };
        const target = { equipment: { armor: { defense: 2 } } };
        
        const damage = battleSystem.calculateDamage(weapon, attacker, target);
        
        expect(damage).toBeGreaterThan(0);
        expect(damage).toBeLessThan(weapon.attack);
    });
    
    test('should handle turn order correctly', () => {
        const party = [
            { name: 'Player1', stats: { dexterity: 10 } },
            { name: 'Player2', stats: { dexterity: 15 } }
        ];
        
        const turnOrder = battleSystem.calculateTurnOrder(party, []);
        
        expect(turnOrder[0].name).toBe('Player2'); // 高敏捷が先行
    });
});
```

---

**技術仕様書バージョン**: 1.0  
**最終更新**: 2025年7月25日  
**対象開発者**: Black Onyx Reborn Development Team