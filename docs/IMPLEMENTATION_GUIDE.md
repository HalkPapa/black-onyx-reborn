# 🛠️ ブラックオニキス復刻版 - 開発者向け実装ガイド

## 📋 実装ガイド概要

### 目的
設計文書に基づいた具体的な実装手順と開発者向けガイドラインを提供し、効率的な開発を支援する。

### 対象読者
- フロントエンド開発者
- JavaScriptエンジニア
- ゲーム開発者
- 新規参加開発者

### 前提知識
- JavaScript ES6+
- HTML5 Canvas API
- Web Audio API
- LocalStorage/IndexedDB
- Promise/async-await

## 🚀 開発環境セットアップ

### 1. 開発環境構築

```bash
# プロジェクトディレクトリに移動
cd /Users/koikedaisuke/MyProjects/ClaudeRooms/1-1/projects/black-onyx-reborn

# 依存関係インストール（既にpackage.jsonが存在）
npm install

# 開発サーバー起動
npm run dev

# 別ターミナルでテスト実行
npm test

# コード品質チェック
npm run lint
```

### 2. 推奨開発ツール

```json
{
  "vscode_extensions": [
    "ms-vscode.vscode-typescript-next",
    "bradlc.vscode-tailwindcss",
    "esbenp.prettier-vscode",
    "ms-vscode.vscode-eslint",
    "streetsidesoftware.code-spell-checker"
  ],
  "browser_extensions": [
    "React Developer Tools",
    "Redux DevTools",
    "Lighthouse"
  ]
}
```

### 3. ディレクトリ構造理解

```
src/
├── core/              # コアゲームエンジン
│   ├── engine.js      # メインエンジンクラス
│   ├── state.js       # 状態管理
│   └── events.js      # イベントシステム
├── systems/           # ゲームシステム
│   ├── rendering/     # 描画システム
│   ├── audio/         # 音響システム
│   ├── input/         # 入力システム
│   └── save/          # セーブシステム
├── game/              # ゲームロジック
│   ├── player/        # プレイヤー管理
│   ├── battle/        # 戦闘システム
│   ├── dungeon/       # ダンジョン管理
│   └── inventory/     # インベントリ
├── ui/                # ユーザーインターフェース
│   ├── components/    # UIコンポーネント
│   ├── screens/       # 画面別UI
│   └── utils/         # UI utility
├── data/              # ゲームデータ
│   ├── maps/          # マップデータ
│   ├── items/         # アイテムデータ
│   └── monsters/      # モンスターデータ
└── utils/             # 共通ユーティリティ
    ├── math.js        # 数学関数
    ├── random.js      # 乱数生成
    └── helpers.js     # ヘルパー関数
```

## 🎮 コアシステム実装

### 1. メインゲームエンジン実装

```javascript
// src/core/engine.js
/**
 * メインゲームエンジンクラス
 * すべてのシステムを統括し、ゲームループを管理
 */
class BlackOnyxEngine {
    constructor(canvasElement) {
        this.canvas = canvasElement;
        this.ctx = canvasElement.getContext('2d');
        
        // システム初期化
        this.systems = new Map();
        this.isRunning = false;
        this.lastFrameTime = 0;
        this.targetFPS = 60;
        this.frameTime = 1000 / this.targetFPS;
        
        // パフォーマンス監視
        this.performanceMonitor = new PerformanceMonitor();
    }
    
    /**
     * エンジン初期化
     * 実装ポイント:
     * - 各システムを順序立てて初期化
     * - エラーハンドリングを適切に実装
     * - 初期化の進行状況を表示
     */
    async init() {
        try {
            console.log('🎮 Black Onyx Engine: 初期化開始');
            
            // 1. コアシステム初期化
            await this.initializeCoreSystem();
            
            // 2. レンダリングシステム初期化
            await this.initializeRenderingSystem();
            
            // 3. オーディオシステム初期化
            await this.initializeAudioSystem();
            
            // 4. 入力システム初期化
            await this.initializeInputSystem();
            
            // 5. ゲームデータ読み込み
            await this.loadGameData();
            
            // 6. UI初期化
            await this.initializeUI();
            
            console.log('✅ Black Onyx Engine: 初期化完了');
            return true;
            
        } catch (error) {
            console.error('❌ エンジン初期化エラー:', error);
            throw new EngineInitializationError(error.message);
        }
    }
    
    /**
     * ゲームループ開始
     * 実装ポイント:
     * - フレームレート制御
     * - パフォーマンス監視
     * - エラー復帰機能
     */
    startGameLoop() {
        if (this.isRunning) {
            console.warn('⚠️ ゲームループは既に実行中です');
            return;
        }
        
        this.isRunning = true;
        this.lastFrameTime = performance.now();
        
        console.log('🔄 ゲームループ開始');
        this.gameLoop();
    }
    
    /**
     * メインゲームループ
     */
    gameLoop() {
        if (!this.isRunning) return;
        
        const currentTime = performance.now();
        const deltaTime = currentTime - this.lastFrameTime;
        
        // フレームレート制御
        if (deltaTime >= this.frameTime) {
            try {
                // パフォーマンス測定開始
                this.performanceMonitor.startFrame();
                
                // 更新処理
                this.update(deltaTime);
                
                // 描画処理
                this.render();
                
                // パフォーマンス測定終了
                this.performanceMonitor.endFrame();
                
                this.lastFrameTime = currentTime;
                
            } catch (error) {
                console.error('❌ ゲームループエラー:', error);
                this.handleRuntimeError(error);
            }
        }
        
        // 次フレーム予約
        requestAnimationFrame(() => this.gameLoop());
    }
    
    /**
     * 更新処理
     * 実装ポイント:
     * - 各システムを適切な順序で更新
     * - 依存関係を考慮した更新
     */
    update(deltaTime) {
        // 1. 入力システム更新
        const inputSystem = this.systems.get('input');
        inputSystem?.update(deltaTime);
        
        // 2. ゲーム状態更新
        const stateManager = this.systems.get('state');
        stateManager?.update(deltaTime);
        
        // 3. ゲームロジック更新
        const gameLogic = this.systems.get('game');
        gameLogic?.update(deltaTime);
        
        // 4. オーディオ更新
        const audioSystem = this.systems.get('audio');
        audioSystem?.update(deltaTime);
        
        // 5. UI更新
        const uiSystem = this.systems.get('ui');
        uiSystem?.update(deltaTime);
    }
    
    /**
     * 描画処理
     */
    render() {
        // 画面クリア
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
        // レンダリングシステムで描画
        const renderer = this.systems.get('renderer');
        renderer?.render(this.ctx);
        
        // デバッグ情報表示（開発時のみ）
        if (process.env.NODE_ENV === 'development') {
            this.renderDebugInfo();
        }
    }
}

/**
 * カスタムエラークラス
 */
class EngineInitializationError extends Error {
    constructor(message) {
        super(message);
        this.name = 'EngineInitializationError';
    }
}

/**
 * パフォーマンス監視クラス
 */
class PerformanceMonitor {
    constructor() {
        this.frameTimes = [];
        this.maxSamples = 60;
        this.frameStart = 0;
    }
    
    startFrame() {
        this.frameStart = performance.now();
    }
    
    endFrame() {
        const frameTime = performance.now() - this.frameStart;
        this.frameTimes.push(frameTime);
        
        if (this.frameTimes.length > this.maxSamples) {
            this.frameTimes.shift();
        }
    }
    
    getAverageFrameTime() {
        if (this.frameTimes.length === 0) return 0;
        const sum = this.frameTimes.reduce((a, b) => a + b, 0);
        return sum / this.frameTimes.length;
    }
    
    getFPS() {
        const avgFrameTime = this.getAverageFrameTime();
        return avgFrameTime > 0 ? 1000 / avgFrameTime : 0;
    }
}
```

### 2. 状態管理システム実装

```javascript
// src/core/state.js
/**
 * ゲーム状態管理システム
 * 実装ポイント:
 * - 型安全性を重視
 * - 状態変更の追跡
 * - undo/redo機能
 */
class GameStateManager {
    constructor() {
        this.currentState = null;
        this.stateHistory = [];
        this.maxHistorySize = 100;
        this.listeners = new Map();
        
        // 状態定義
        this.states = {
            LOADING: 'loading',
            TITLE: 'title',
            CHARACTER_CREATION: 'character_creation',
            GAMEPLAY: 'gameplay',
            BATTLE: 'battle',
            MENU: 'menu',
            SAVE_LOAD: 'save_load',
            GAME_OVER: 'game_over'
        };
        
        // 有効な状態遷移定義
        this.validTransitions = new Map([
            [this.states.LOADING, [this.states.TITLE]],
            [this.states.TITLE, [this.states.CHARACTER_CREATION, this.states.GAMEPLAY]],
            [this.states.CHARACTER_CREATION, [this.states.GAMEPLAY, this.states.TITLE]],
            [this.states.GAMEPLAY, [this.states.BATTLE, this.states.MENU]],
            [this.states.BATTLE, [this.states.GAMEPLAY, this.states.GAME_OVER]],
            [this.states.MENU, [this.states.GAMEPLAY, this.states.SAVE_LOAD, this.states.TITLE]],
            [this.states.SAVE_LOAD, [this.states.MENU]],
            [this.states.GAME_OVER, [this.states.TITLE]]
        ]);
    }
    
    /**
     * 状態変更
     * @param {string} newState - 新しい状態
     * @param {object} data - 状態データ
     * @returns {Promise<boolean>} - 変更成功/失敗
     */
    async changeState(newState, data = {}) {
        // 有効性チェック
        if (!this.isValidTransition(this.currentState, newState)) {
            console.warn(`無効な状態遷移: ${this.currentState} -> ${newState}`);
            return false;
        }
        
        const oldState = this.currentState;
        
        try {
            // 現在状態の終了処理
            if (oldState) {
                await this.exitState(oldState);
            }
            
            // 状態履歴に追加
            this.addToHistory(oldState, newState, data);
            
            // 状態更新
            this.currentState = newState;
            
            // 新状態の開始処理
            await this.enterState(newState, data);
            
            // リスナーに通知
            this.notifyStateChange(oldState, newState, data);
            
            console.log(`状態遷移: ${oldState} -> ${newState}`);
            return true;
            
        } catch (error) {
            console.error('状態遷移エラー:', error);
            // ロールバック
            this.currentState = oldState;
            throw error;
        }
    }
    
    /**
     * 状態遷移の有効性チェック
     */
    isValidTransition(fromState, toState) {
        if (!fromState) return true; // 初期状態
        const validStates = this.validTransitions.get(fromState);
        return validStates ? validStates.includes(toState) : false;
    }
    
    /**
     * 状態開始処理
     */
    async enterState(state, data) {
        switch (state) {
            case this.states.LOADING:
                await this.enterLoadingState(data);
                break;
            case this.states.TITLE:
                await this.enterTitleState(data);
                break;
            case this.states.CHARACTER_CREATION:
                await this.enterCharacterCreationState(data);
                break;
            case this.states.GAMEPLAY:
                await this.enterGameplayState(data);
                break;
            case this.states.BATTLE:
                await this.enterBattleState(data);
                break;
            case this.states.MENU:
                await this.enterMenuState(data);
                break;
            default:
                console.warn(`未実装の状態: ${state}`);
        }
    }
    
    /**
     * 状態終了処理
     */
    async exitState(state) {
        switch (state) {
            case this.states.LOADING:
                await this.exitLoadingState();
                break;
            case this.states.GAMEPLAY:
                await this.exitGameplayState();
                break;
            case this.states.BATTLE:
                await this.exitBattleState();
                break;
            // その他の状態の終了処理
        }
    }
    
    // 個別状態処理メソッド
    async enterLoadingState(data) {
        // ローディング画面表示
        // アセット読み込み開始
        console.log('ローディング状態開始');
    }
    
    async enterBattleState(data) {
        // 戦闘BGM開始
        // 戦闘UI表示
        // 敵データ初期化
        console.log('戦闘状態開始:', data);
    }
    
    async exitBattleState() {
        // 戦闘BGM停止
        // 戦闘UI非表示
        // 戦闘データクリア
        console.log('戦闘状態終了');
    }
    
    /**
     * 状態変更リスナー登録
     */
    onStateChange(callback) {
        const id = Date.now() + Math.random();
        if (!this.listeners.has('stateChange')) {
            this.listeners.set('stateChange', new Map());
        }
        this.listeners.get('stateChange').set(id, callback);
        return id; // リスナー解除用
    }
    
    /**
     * リスナー解除
     */
    removeStateChangeListener(id) {
        const stateChangeListeners = this.listeners.get('stateChange');
        if (stateChangeListeners) {
            stateChangeListeners.delete(id);
        }
    }
    
    /**
     * 状態変更通知
     */
    notifyStateChange(oldState, newState, data) {
        const stateChangeListeners = this.listeners.get('stateChange');
        if (stateChangeListeners) {
            stateChangeListeners.forEach(callback => {
                try {
                    callback(oldState, newState, data);
                } catch (error) {
                    console.error('状態変更リスナーエラー:', error);
                }
            });
        }
    }
}
```

### 3. レンダリングシステム実装

```javascript
// src/systems/rendering/renderer.js
/**
 * 疑似3Dダンジョンレンダリングシステム
 * オリジナル版の表現を現代技術で再現
 */
class DungeonRenderer {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        
        // 描画設定
        this.config = {
            viewDistance: 5,        // 表示距離
            wallHeight: 400,        // 壁の高さ
            floorHeight: 200,       // 床の高さ
            tileSize: 64,          // タイルサイズ
            
            // 遠近法設定
            perspectiveScales: [1.0, 0.75, 0.5, 0.25, 0.125, 0.06]
        };
        
        // テクスチャ管理
        this.textures = new Map();
        this.loadQueue = [];
        
        // 描画キャッシュ
        this.renderCache = new Map();
        this.cacheEnabled = true;
    }
    
    /**
     * 初期化
     * 実装ポイント:
     * - テクスチャの事前読み込み
     * - 描画設定の最適化
     */
    async init() {
        console.log('🎨 レンダリングシステム初期化開始');
        
        try {
            // 基本テクスチャ読み込み
            await this.loadEssentialTextures();
            
            // 描画設定最適化
            this.optimizeRenderingSettings();
            
            console.log('✅ レンダリングシステム初期化完了');
            
        } catch (error) {
            console.error('❌ レンダリングシステム初期化エラー:', error);
            throw error;
        }
    }
    
    /**
     * 必須テクスチャ読み込み
     */
    async loadEssentialTextures() {
        const essentialTextures = [
            'stone_wall_front',
            'stone_wall_left', 
            'stone_wall_right',
            'stone_floor',
            'stone_ceiling',
            'door_closed',
            'door_open'
        ];
        
        const loadPromises = essentialTextures.map(name => 
            this.loadTexture(name, `assets/images/dungeon/${name}.png`)
        );
        
        await Promise.all(loadPromises);
    }
    
    /**
     * テクスチャ読み込み
     */
    async loadTexture(name, url) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => {
                this.textures.set(name, img);
                console.log(`📷 テクスチャ読み込み完了: ${name}`);
                resolve(img);
            };
            img.onerror = () => {
                console.error(`❌ テクスチャ読み込み失敗: ${name}`);
                reject(new Error(`Failed to load texture: ${name}`));
            };
            img.src = url;
        });
    }
    
    /**
     * ダンジョン描画メイン処理
     * @param {object} player - プレイヤー情報
     * @param {object} dungeon - ダンジョンデータ
     */
    renderDungeon(player, dungeon) {
        // キャッシュキーの生成
        const cacheKey = this.generateCacheKey(player, dungeon);
        
        // キャッシュチェック
        if (this.cacheEnabled && this.renderCache.has(cacheKey)) {
            const cachedImage = this.renderCache.get(cacheKey);
            this.ctx.drawImage(cachedImage, 0, 0);
            return;
        }
        
        // 新規描画
        this.performDungeonRender(player, dungeon);
        
        // キャッシュに保存
        if (this.cacheEnabled) {
            this.cacheCurrentRender(cacheKey);
        }
    }
    
    /**
     * 実際のダンジョン描画処理
     */
    performDungeonRender(player, dungeon) {
        // 1. 画面クリア
        this.clearScreen();
        
        // 2. 背景（空・遠景）描画
        this.drawBackground();
        
        // 3. 距離別レイヤー描画（遠→近）
        for (let distance = this.config.viewDistance; distance >= 0; distance--) {
            this.renderDistanceLayer(player, dungeon, distance);
        }
        
        // 4. エフェクト描画
        this.renderEffects();
        
        // 5. UI オーバーレイ
        this.renderUIOverlay();
    }
    
    /**
     * 指定距離のレイヤー描画
     */
    renderDistanceLayer(player, dungeon, distance) {
        const scale = this.config.perspectiveScales[distance] || 0.06;
        const visibleTiles = this.getVisibleTiles(player, distance);
        
        visibleTiles.forEach(tile => {
            const screenPos = this.worldToScreen(tile.worldPos, distance);
            
            switch (tile.type) {
                case 'wall':
                    this.drawWall(screenPos, scale, tile);
                    break;
                case 'door':
                    this.drawDoor(screenPos, scale, tile);
                    break;
                case 'stairs':
                    this.drawStairs(screenPos, scale, tile);
                    break;
                case 'treasure':
                    this.drawTreasure(screenPos, scale, tile);
                    break;
            }
        });
    }
    
    /**
     * 壁面描画
     * 実装ポイント:
     * - テクスチャマッピング
     * - 距離による暗化効果
     * - パフォーマンス最適化
     */
    drawWall(screenPos, scale, tileData) {
        const wallWidth = this.config.tileSize * scale;
        const wallHeight = this.config.wallHeight * scale;
        
        // テクスチャ取得
        const texture = this.getWallTexture(tileData);
        
        if (texture) {
            // テクスチャ描画
            this.ctx.save();
            
            // 暗化効果適用
            this.applyDistanceFog(scale);
            
            this.ctx.drawImage(
                texture,
                screenPos.x - wallWidth / 2,
                screenPos.y - wallHeight / 2,
                wallWidth,
                wallHeight
            );
            
            this.ctx.restore();
        } else {
            // フォールバック描画
            this.drawFallbackWall(screenPos, wallWidth, wallHeight, scale);
        }
    }
    
    /**
     * 壁テクスチャ取得
     */
    getWallTexture(tileData) {
        // 壁の向きに応じてテクスチャ選択
        switch (tileData.facing) {
            case 'front':
                return this.textures.get('stone_wall_front');
            case 'left':
                return this.textures.get('stone_wall_left');
            case 'right':
                return this.textures.get('stone_wall_right');
            default:
                return this.textures.get('stone_wall_front');
        }
    }
    
    /**
     * 距離による暗化効果
     */
    applyDistanceFog(scale) {
        const alpha = Math.max(0.3, scale); // 最低30%の明度を保持
        this.ctx.globalAlpha = alpha;
        
        // 色調も調整
        const darkness = 1 - scale;
        if (darkness > 0) {
            this.ctx.fillStyle = `rgba(0, 0, 0, ${darkness * 0.3})`;
            this.ctx.globalCompositeOperation = 'multiply';
        }
    }
    
    /**
     * ワールド座標からスクリーン座標変換
     */
    worldToScreen(worldPos, distance) {
        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;
        
        const scale = this.config.perspectiveScales[distance] || 0.06;
        const perspective = Math.max(0.1, 1.0 - (distance * 0.15));
        
        return {
            x: centerX + (worldPos.x * scale * perspective),
            y: centerY + (worldPos.y * scale * perspective)
        };
    }
    
    /**
     * 可視タイル取得
     * プレイヤーの位置と向きから見えるタイルを計算
     */
    getVisibleTiles(player, distance) {
        const tiles = [];
        const centerX = player.position.x;
        const centerY = player.position.y;
        const direction = player.direction;
        
        // 方向ベクトル計算
        const dirVector = this.getDirectionVector(direction);
        const rightVector = this.getPerpendicularVector(dirVector);
        
        // 視野内のタイルを収集
        for (let x = -2; x <= 2; x++) {
            const worldX = centerX + (dirVector.x * distance) + (rightVector.x * x);
            const worldY = centerY + (dirVector.y * distance) + (rightVector.y * x);
            
            // タイルデータ取得
            const tileData = this.getTileData(worldX, worldY);
            if (tileData) {
                tiles.push({
                    worldPos: { x: worldX, y: worldY },
                    type: tileData.type,
                    facing: this.calculateFacing(x, distance),
                    ...tileData
                });
            }
        }
        
        return tiles;
    }
    
    /**
     * パフォーマンス最適化
     */
    optimizeRenderingSettings() {
        // Canvas最適化
        this.ctx.imageSmoothingEnabled = false; // ピクセルアート用
        
        // レンダリングヒント設定
        if (this.ctx.imageSmoothingQuality) {
            this.ctx.imageSmoothingQuality = 'high';
        }
        
        // キャッシュサイズ制限
        this.maxCacheSize = 50;
        
        console.log('🔧 レンダリング設定最適化完了');
    }
    
    /**
     * 描画キャッシュ管理
     */
    cacheCurrentRender(cacheKey) {
        // キャッシュサイズ制限
        if (this.renderCache.size >= this.maxCacheSize) {
            const firstKey = this.renderCache.keys().next().value;
            this.renderCache.delete(firstKey);
        }
        
        // 現在の描画をキャッシュ
        const cachedCanvas = document.createElement('canvas');
        cachedCanvas.width = this.canvas.width;
        cachedCanvas.height = this.canvas.height;
        const cachedCtx = cachedCanvas.getContext('2d');
        cachedCtx.drawImage(this.canvas, 0, 0);
        
        this.renderCache.set(cacheKey, cachedCanvas);
    }
}
```

## ⚔️ 戦闘システム実装

### 戦闘管理クラス実装例

```javascript
// src/game/battle/battle-manager.js
/**
 * 戦闘システム管理クラス
 * オリジナル版の戦闘システムを忠実に再現
 */
class BattleManager {
    constructor() {
        this.state = 'inactive';
        this.currentBattle = null;
        this.turnQueue = [];
        this.currentTurnIndex = 0;
        
        // 戦闘設定
        this.config = {
            escapeBaseRate: 0.5,        // 基本逃走確率
            criticalHitRate: 0.05,      // クリティカル確率
            maxTurns: 100,              // 最大ターン数
            turnTimeLimit: 30000        // ターン制限時間（ms）
        };
        
        // イベントリスナー
        this.listeners = new Map();
        
        // アニメーションキュー
        this.animationQueue = [];
        this.isAnimating = false;
    }
    
    /**
     * 戦闘開始
     * @param {Array} playerParty - プレイヤーパーティ
     * @param {Array} enemyTypes - 敵タイプ配列
     * @param {Object} battleEnvironment - 戦闘環境
     */
    async startBattle(playerParty, enemyTypes, battleEnvironment = {}) {
        console.log('⚔️ 戦闘開始:', enemyTypes);
        
        try {
            // 戦闘データ初期化
            this.currentBattle = new Battle({
                id: this.generateBattleId(),
                playerParty: [...playerParty],
                enemies: this.generateEnemies(enemyTypes),
                environment: battleEnvironment,
                turnCount: 0,
                startTime: Date.now()
            });
            
            // ターン順計算
            this.calculateTurnOrder();
            
            // 戦闘状態変更
            this.state = 'active';
            
            // UI初期化
            await this.initializeBattleUI();
            
            // BGM変更
            await this.startBattleMusic();
            
            // 戦闘開始イベント通知
            this.emit('battle-start', {
                enemies: this.currentBattle.enemies,
                environment: battleEnvironment
            });
            
            // 最初のターン開始
            this.startTurn();
            
            return {
                success: true,
                battleId: this.currentBattle.id,
                enemies: this.currentBattle.enemies.map(e => e.name)
            };
            
        } catch (error) {
            console.error('❌ 戦闘開始エラー:', error);
            throw new BattleError('戦闘開始に失敗', error);
        }
    }
    
    /**
     * ターン順計算
     * オリジナル版と同様の敏捷性ベース計算
     */
    calculateTurnOrder() {
        const allCombatants = [
            ...this.currentBattle.playerParty,
            ...this.currentBattle.enemies
        ];
        
        // 敏捷性＋ランダム要素でソート
        this.turnQueue = allCombatants
            .filter(combatant => combatant.hp > 0)
            .map(combatant => ({
                ...combatant,
                turnPriority: combatant.stats.agility + (Math.random() * 10)
            }))
            .sort((a, b) => b.turnPriority - a.turnPriority);
        
        console.log('📋 ターン順:', this.turnQueue.map(c => c.name));
    }
    
    /**
     * ターン開始
     */
    startTurn() {
        if (this.state !== 'active') return;
        
        // 戦闘終了チェック
        const battleResult = this.checkBattleEnd();
        if (battleResult.ended) {
            this.endBattle(battleResult.result);
            return;
        }
        
        // 現在のアクター取得
        const currentActor = this.getCurrentActor();
        if (!currentActor) {
            this.advanceTurn();
            return;
        }
        
        console.log(`🎯 ${currentActor.name}のターン`);
        
        // アクタータイプ別処理
        if (currentActor.isPlayer) {
            this.handlePlayerTurn(currentActor);
        } else {
            this.handleEnemyTurn(currentActor);
        }
    }
    
    /**
     * プレイヤーターン処理
     */
    async handlePlayerTurn(player) {
        this.state = 'waiting-for-player-input';
        
        // UI更新
        await this.showPlayerCommandMenu(player);
        
        // ターンタイマー開始（オプション）
        if (this.config.turnTimeLimit > 0) {
            this.startTurnTimer();
        }
        
        // プレイヤー入力待ち
        this.emit('player-turn-start', { player });
    }
    
    /**
     * プレイヤーアクション実行
     * @param {Object} action - プレイヤーアクション
     */
    async executePlayerAction(action) {
        if (this.state !== 'waiting-for-player-input') {
            console.warn('⚠️ プレイヤー入力待ち状態ではありません');
            return;
        }
        
        this.clearTurnTimer();
        this.state = 'executing-action';
        
        try {
            // アクション実行
            const result = await this.executeAction(action);
            
            // アニメーション再生
            await this.playActionAnimation(result);
            
            // 結果適用
            await this.applyActionResult(result);
            
            // ターン終了
            this.advanceTurn();
            
        } catch (error) {
            console.error('❌ アクション実行エラー:', error);
            this.state = 'waiting-for-player-input';
            this.emit('action-error', { error: error.message });
        }
    }
    
    /**
     * 敵ターン処理
     */
    async handleEnemyTurn(enemy) {
        this.state = 'enemy-action';
        
        // AI思考時間演出
        await this.delay(500 + Math.random() * 1000);
        
        // AI行動決定
        const action = this.getEnemyAIAction(enemy);
        
        console.log(`🤖 ${enemy.name}の行動:`, action.type);
        
        // アクション実行
        const result = await this.executeAction(action);
        
        // アニメーション再生
        await this.playActionAnimation(result);
        
        // 結果適用
        await this.applyActionResult(result);
        
        // ターン終了
        this.advanceTurn();
    }
    
    /**
     * アクション実行
     * @param {Object} action - 実行するアクション
     */
    async executeAction(action) {
        switch (action.type) {
            case 'attack':
                return this.executeAttack(action);
            case 'magic':
                return this.executeMagic(action);
            case 'item':
                return this.executeItemUse(action);
            case 'defend':
                return this.executeDefend(action);
            case 'escape':
                return this.executeEscape(action);
            default:
                throw new Error(`未知のアクションタイプ: ${action.type}`);
        }
    }
    
    /**
     * 物理攻撃実行
     * オリジナル版の固定ダメージ制を再現
     */
    async executeAttack(action) {
        const attacker = action.actor;
        const target = action.target;
        const weapon = attacker.equipment?.weapon;
        
        // 命中判定
        const accuracy = this.calculateAccuracy(attacker, target, weapon);
        const hit = Math.random() < accuracy;
        
        if (!hit) {
            return {
                type: 'attack',
                success: false,
                attacker: attacker,
                target: target,
                message: `${attacker.name}の攻撃は外れた！`,
                animation: 'miss'
            };
        }
        
        // ダメージ計算
        const baseDamage = weapon ? weapon.attack : 1;
        const strengthBonus = Math.floor(attacker.stats.strength / 3);
        const randomFactor = 0.85 + (Math.random() * 0.3); // ±15%
        
        const rawDamage = (baseDamage + strengthBonus) * randomFactor;
        
        // 防御力計算
        const armor = target.equipment?.armor;
        const defense = armor ? armor.defense : 0;
        const vitalityReduction = Math.floor(target.stats.vitality / 5);
        
        // 最終ダメージ
        const damage = Math.max(1, Math.floor(rawDamage - defense - vitalityReduction));
        
        // クリティカル判定
        const critical = Math.random() < this.config.criticalHitRate;
        const finalDamage = critical ? Math.floor(damage * 1.5) : damage;
        
        // ダメージ適用
        target.hp = Math.max(0, target.hp - finalDamage);
        
        return {
            type: 'attack',
            success: true,
            attacker: attacker,
            target: target,
            damage: finalDamage,
            critical: critical,
            message: `${attacker.name}の攻撃！ ${finalDamage}のダメージ${critical ? '（会心の一撃！）' : ''}`,
            animation: critical ? 'critical-hit' : 'hit'
        };
    }
    
    /**
     * 戦闘終了チェック
     */
    checkBattleEnd() {
        const playersAlive = this.currentBattle.playerParty.some(p => p.hp > 0);
        const enemiesAlive = this.currentBattle.enemies.some(e => e.hp > 0);
        
        if (!playersAlive) {
            return { ended: true, result: 'defeat' };
        }
        
        if (!enemiesAlive) {
            return { ended: true, result: 'victory' };
        }
        
        // 最大ターン数チェック
        if (this.currentBattle.turnCount >= this.config.maxTurns) {
            return { ended: true, result: 'draw' };
        }
        
        return { ended: false };
    }
    
    /**
     * 戦闘終了処理
     */
    async endBattle(result) {
        console.log(`🏁 戦闘終了: ${result}`);
        
        this.state = 'ending';
        
        try {
            switch (result) {
                case 'victory':
                    await this.handleVictory();
                    break;
                case 'defeat':
                    await this.handleDefeat();
                    break;
                case 'escape':
                    await this.handleEscape();
                    break;
                case 'draw':
                    await this.handleDraw();
                    break;
            }
            
            // 戦闘後処理
            await this.cleanup();
            
            // イベント発火
            this.emit('battle-end', { 
                result: result,
                duration: Date.now() - this.currentBattle.startTime
            });
            
        } catch (error) {
            console.error('❌ 戦闘終了処理エラー:', error);
        } finally {
            this.state = 'inactive';
            this.currentBattle = null;
        }
    }
    
    /**
     * 勝利処理
     */
    async handleVictory() {
        // 経験値・金・アイテム計算
        const rewards = this.calculateRewards();
        
        // BGM変更
        await this.playVictoryMusic();
        
        // UI表示
        await this.showVictoryScreen(rewards);
        
        // 報酬適用
        await this.applyRewards(rewards);
        
        console.log('🎉 勝利報酬:', rewards);
    }
    
    /**
     * 報酬計算
     */
    calculateRewards() {
        let totalExp = 0;
        let totalGold = 0;
        const items = [];
        
        this.currentBattle.enemies.forEach(enemy => {
            totalExp += enemy.rewards?.experience || 0;
            totalGold += enemy.rewards?.gold || 0;
            
            // ドロップ判定
            if (enemy.rewards?.dropRate && Math.random() < enemy.rewards.dropRate) {
                const droppedItem = this.rollItemDrop(enemy);
                if (droppedItem) {
                    items.push(droppedItem);
                }
            }
        });
        
        // パーティサイズボーナス
        const partyBonus = this.currentBattle.playerParty.length > 1 ? 1.2 : 1.0;
        
        return {
            experience: Math.floor(totalExp * partyBonus),
            gold: Math.floor(totalGold * partyBonus),
            items: items
        };
    }
    
    // ユーティリティメソッド
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    
    generateBattleId() {
        return 'battle_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }
    
    emit(eventName, data) {
        const callbacks = this.listeners.get(eventName) || [];
        callbacks.forEach(callback => {
            try {
                callback(data);
            } catch (error) {
                console.error(`イベントリスナーエラー (${eventName}):`, error);
            }
        });
    }
    
    on(eventName, callback) {
        if (!this.listeners.has(eventName)) {
            this.listeners.set(eventName, []);
        }
        this.listeners.get(eventName).push(callback);
    }
}

/**
 * 戦闘用カスタムエラー
 */
class BattleError extends Error {
    constructor(message, cause) {
        super(message);
        this.name = 'BattleError';
        this.cause = cause;
    }
}
```

## 🎯 実装チェックリスト

### Phase 1: 基盤システム実装
- [ ] メインゲームエンジン実装
- [ ] 状態管理システム実装
- [ ] 基本レンダリングシステム実装
- [ ] 入力システム実装
- [ ] オーディオシステム基礎実装

### Phase 2: ゲームロジック実装
- [ ] プレイヤー管理システム実装
- [ ] ダンジョン管理システム実装
- [ ] 戦闘システム実装
- [ ] インベントリシステム実装
- [ ] セーブ・ロードシステム実装

### Phase 3: UI・UX実装
- [ ] タイトル画面実装
- [ ] キャラクター作成画面実装
- [ ] ゲームプレイUI実装
- [ ] 戦闘UI実装
- [ ] メニューシステム実装

### Phase 4: 最適化・調整
- [ ] パフォーマンス最適化
- [ ] ゲームバランス調整
- [ ] バグ修正・安定化
- [ ] アクセシビリティ対応

## 🐛 デバッグ・テスト指針

### デバッグツール実装

```javascript
// src/utils/debug.js
/**
 * デバッグ支援ツール
 */
class DebugTools {
    constructor() {
        this.enabled = process.env.NODE_ENV === 'development';
        this.logLevel = 'info'; // 'debug', 'info', 'warn', 'error'
        this.overlay = null;
        
        if (this.enabled) {
            this.createDebugOverlay();
            this.setupKeyboardShortcuts();
        }
    }
    
    createDebugOverlay() {
        this.overlay = document.createElement('div');
        this.overlay.id = 'debug-overlay';
        this.overlay.style.cssText = `
            position: fixed;
            top: 10px;
            right: 10px;
            background: rgba(0,0,0,0.8);
            color: white;
            padding: 10px;
            font-family: monospace;
            font-size: 12px;
            z-index: 9999;
            max-width: 300px;
            display: none;
        `;
        document.body.appendChild(this.overlay);
    }
    
    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            if (e.key === 'F12' && e.ctrlKey) {
                this.toggleOverlay();
            }
            if (e.key === 'F11' && e.ctrlKey) {
                this.takeScreenshot();
            }
        });
    }
    
    log(level, message, data = null) {
        if (!this.enabled) return;
        
        const levels = ['debug', 'info', 'warn', 'error'];
        const currentLevelIndex = levels.indexOf(this.logLevel);
        const messageLevelIndex = levels.indexOf(level);
        
        if (messageLevelIndex >= currentLevelIndex) {
            const timestamp = new Date().toISOString().substr(11, 12);
            const logMessage = `[${timestamp}] ${level.toUpperCase()}: ${message}`;
            
            console[level](logMessage, data || '');
            
            if (this.overlay && this.overlay.style.display !== 'none') {
                this.updateOverlay(logMessage);
            }
        }
    }
    
    // 使いやすいラッパーメソッド
    debug(message, data) { this.log('debug', message, data); }
    info(message, data) { this.log('info', message, data); }
    warn(message, data) { this.log('warn', message, data); }
    error(message, data) { this.log('error', message, data); }
}

// グローバルインスタンス
const debug = new DebugTools();
export default debug;
```

### ユニットテスト例

```javascript
// tests/battle-system.test.js
import { BattleManager } from '../src/game/battle/battle-manager.js';
import { PlayerCharacter } from '../src/game/player/player-character.js';

describe('BattleManager', () => {
    let battleManager;
    let testPlayer;
    
    beforeEach(() => {
        battleManager = new BattleManager();
        testPlayer = new PlayerCharacter({
            name: 'テストプレイヤー',
            level: 5,
            stats: { strength: 12, agility: 10, vitality: 14, intelligence: 8 }
        });
    });
    
    describe('damage calculation', () => {
        test('should calculate physical damage correctly', () => {
            const weapon = { attack: 10 };
            const target = { 
                hp: 30,
                stats: { vitality: 10 },
                equipment: { armor: { defense: 2 } }
            };
            
            const damage = battleManager.calculatePhysicalDamage(
                testPlayer, 
                target, 
                weapon
            );
            
            expect(damage).toBeGreaterThan(0);
            expect(damage).toBeLessThan(weapon.attack + 10);
        });
        
        test('should apply minimum damage of 1', () => {
            const weakWeapon = { attack: 1 };
            const strongTarget = {
                hp: 50,
                stats: { vitality: 20 },
                equipment: { armor: { defense: 10 } }
            };
            
            const damage = battleManager.calculatePhysicalDamage(
                testPlayer,
                strongTarget,
                weakWeapon
            );
            
            expect(damage).toBe(1);
        });
    });
    
    describe('turn order calculation', () => {
        test('should order combatants by agility', () => {
            const slowEnemy = { name: '遅い敵', stats: { agility: 5 }, hp: 10 };
            const fastEnemy = { name: '速い敵', stats: { agility: 15 }, hp: 10 };
            
            battleManager.currentBattle = {
                playerParty: [testPlayer],
                enemies: [slowEnemy, fastEnemy]
            };
            
            battleManager.calculateTurnOrder();
            
            // 最初のアクターは最も敏捷性が高いはず
            expect(battleManager.turnQueue[0].stats.agility).toBeGreaterThanOrEqual(
                battleManager.turnQueue[1].stats.agility
            );
        });
    });
});
```

---

**開発者向け実装ガイドバージョン**: 1.0  
**最終更新**: 2025年7月26日  
**承認者**: Black Onyx Reborn Development Team  
**実装開始**: 設計文書レビュー完了後