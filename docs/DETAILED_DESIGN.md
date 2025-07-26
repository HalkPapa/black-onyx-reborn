# 🏗️ ブラックオニキス復刻版 - 詳細設計書

## 📋 設計文書概要

### 目的
ブラックオニキス復刻版の実装に必要な詳細技術設計を定義し、開発者が直接実装可能な仕様書を提供する。

### 対象読者
- フロントエンド開発者
- バックエンド開発者  
- UI/UXデザイナー
- QAエンジニア

### 設計方針
- **忠実性**: オリジナル版のゲーム体験を可能な限り再現
- **現代性**: 現代のWeb技術基準に準拠
- **拡張性**: 将来的な機能追加を考慮した設計
- **保守性**: メンテナンスしやすい構造

## 🎯 システムアーキテクチャ設計

### 全体アーキテクチャ
```
┌─────────────────┬─────────────────┬─────────────────┐
│   プレゼン層    │    ロジック層    │    データ層     │
├─────────────────┼─────────────────┼─────────────────┤
│ UI Components   │ Game Engine     │ LocalStorage    │
│ Canvas Renderer │ Battle System   │ IndexedDB       │
│ Audio Manager   │ Character Mgmt  │ SessionStorage  │
│ Input Handler   │ Dungeon Manager │ External APIs   │
└─────────────────┴─────────────────┴─────────────────┘
```

### モジュール依存関係
```
Application Entry Point
         ↓
   Game Engine Core
    ↙    ↓    ↘
Renderer System Manager Audio System
    ↓      ↓        ↓
 Canvas  Game State Web Audio
   2D     Manager    Context
```

## 🎮 ゲームエンジン詳細設計

### 1. コアエンジンクラス設計

```javascript
/**
 * ブラックオニキス復刻版メインゲームエンジン
 */
class BlackOnyxReborn {
    constructor(canvasElement) {
        // Core systems
        this.canvas = canvasElement;
        this.ctx = canvasElement.getContext('2d');
        
        // システム初期化
        this.systems = {
            renderer: new RenderingSystem(this.canvas),
            input: new InputSystem(),
            audio: new AudioSystem(),
            save: new SaveSystem(),
            ui: new UISystem(),
            game: new GameStateManager()
        };
        
        // ゲーム状態
        this.gameState = 'loading'; // loading, title, character_creation, game, battle, menu
        this.isRunning = false;
        this.lastFrameTime = 0;
        this.deltaTime = 0;
    }
    
    /**
     * ゲームエンジン初期化
     */
    async init() {
        try {
            // システム順次初期化
            await this.systems.audio.init();
            await this.systems.renderer.init();
            await this.systems.input.init();
            await this.systems.save.init();
            await this.systems.ui.init();
            
            // ゲームデータ読み込み
            await this.loadGameData();
            
            // 初期状態設定
            this.gameState = 'title';
            this.isRunning = true;
            
            // メインループ開始
            this.startGameLoop();
            
            console.log('Black Onyx Reborn: 初期化完了');
        } catch (error) {
            console.error('ゲーム初期化エラー:', error);
            this.handleInitError(error);
        }
    }
    
    /**
     * メインゲームループ
     */
    gameLoop(currentTime) {
        if (!this.isRunning) return;
        
        // デルタタイム計算
        this.deltaTime = currentTime - this.lastFrameTime;
        this.lastFrameTime = currentTime;
        
        // 更新処理
        this.update(this.deltaTime);
        
        // 描画処理
        this.render();
        
        // 次フレーム予約
        requestAnimationFrame((time) => this.gameLoop(time));
    }
    
    /**
     * ゲーム状態更新
     */
    update(deltaTime) {
        // 入力処理
        this.systems.input.update();
        
        // 状態別更新処理
        switch (this.gameState) {
            case 'title':
                this.updateTitleScreen(deltaTime);
                break;
            case 'character_creation':
                this.updateCharacterCreation(deltaTime);
                break;
            case 'game':
                this.updateGameplay(deltaTime);
                break;
            case 'battle':
                this.updateBattle(deltaTime);
                break;
            case 'menu':
                this.updateMenu(deltaTime);
                break;
        }
        
        // UI更新
        this.systems.ui.update(deltaTime);
    }
    
    /**
     * 描画処理
     */
    render() {
        // 画面クリア
        this.systems.renderer.clear();
        
        // 状態別描画
        switch (this.gameState) {
            case 'title':
                this.renderTitleScreen();
                break;
            case 'character_creation':
                this.renderCharacterCreation();
                break;
            case 'game':
                this.renderGameplay();
                break;
            case 'battle':
                this.renderBattle();
                break;
            case 'menu':
                this.renderMenu();
                break;
        }
        
        // UI描画
        this.systems.ui.render(this.ctx);
    }
}
```

### 2. レンダリングシステム設計

```javascript
/**
 * 疑似3Dダンジョン描画システム
 */
class RenderingSystem {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        
        // 描画設定
        this.viewDistance = 5;
        this.tileSize = 64;
        this.wallHeight = 400;
        this.floorHeight = 200;
        
        // カメラ設定
        this.camera = {
            x: 0,
            y: 0,
            direction: 0 // 0=北, 1=東, 2=南, 3=西
        };
        
        // アセット管理
        this.textures = new Map();
        this.sprites = new Map();
    }
    
    /**
     * 疑似3Dダンジョン描画
     */
    renderDungeon(player, dungeon) {
        // 深度バッファクリア
        this.clearDepthBuffer();
        
        // 距離別描画（遠→近の順）
        for (let distance = this.viewDistance; distance >= 0; distance--) {
            this.renderDepthLayer(player, dungeon, distance);
        }
        
        // 前景エフェクト描画
        this.renderForegroundEffects();
    }
    
    /**
     * 指定距離の描画レイヤー処理
     */
    renderDepthLayer(player, dungeon, distance) {
        const viewTiles = this.getVisibleTiles(player.position, player.direction, distance);
        
        viewTiles.forEach(tile => {
            const scale = this.calculateScale(distance);
            const screenPos = this.worldToScreen(tile.worldPos, distance);
            
            switch (tile.type) {
                case 'wall':
                    this.drawWall(screenPos, scale, tile.texture);
                    break;
                case 'door':
                    this.drawDoor(screenPos, scale, tile.isOpen);
                    break;
                case 'stairs':
                    this.drawStairs(screenPos, scale, tile.direction);
                    break;
                case 'treasure':
                    this.drawTreasure(screenPos, scale, tile.opened);
                    break;
            }
        });
    }
    
    /**
     * 壁面描画
     */
    drawWall(screenPos, scale, texture) {
        const wallWidth = this.tileSize * scale;
        const wallHeight = this.wallHeight * scale;
        
        // テクスチャ適用
        if (this.textures.has(texture)) {
            const img = this.textures.get(texture);
            this.ctx.drawImage(
                img,
                screenPos.x - wallWidth / 2,
                screenPos.y - wallHeight / 2,
                wallWidth,
                wallHeight
            );
        } else {
            // フォールバック描画
            this.ctx.fillStyle = '#808080';
            this.ctx.fillRect(
                screenPos.x - wallWidth / 2,
                screenPos.y - wallHeight / 2,
                wallWidth,
                wallHeight
            );
        }
        
        // 距離による暗化効果
        this.applyDistanceFog(screenPos, wallWidth, wallHeight, scale);
    }
    
    /**
     * 距離によるスケール計算
     */
    calculateScale(distance) {
        const scales = [1.0, 0.75, 0.5, 0.25, 0.125, 0.06];
        return distance < scales.length ? scales[distance] : scales[scales.length - 1];
    }
    
    /**
     * ワールド座標からスクリーン座標への変換
     */
    worldToScreen(worldPos, distance) {
        const centerX = this.canvas.width / 2;
        const centerY = this.canvas.height / 2;
        
        // 距離による遠近法適用
        const scale = this.calculateScale(distance);
        const perspective = 1.0 - (distance * 0.1);
        
        return {
            x: centerX + (worldPos.x * scale * perspective),
            y: centerY + (worldPos.y * scale * perspective)
        };
    }
}
```

### 3. 戦闘システム設計

```javascript
/**
 * ターン制戦闘システム
 */
class BattleSystem {
    constructor() {
        this.state = 'none'; // none, encounter, command_select, action_execute, result, victory, defeat
        this.currentTurn = 0;
        this.turnOrder = [];
        
        // 戦闘参加者
        this.playerParty = [];
        this.enemies = [];
        this.allCombatants = [];
        
        // 戦闘UI
        this.battleUI = new BattleUI();
        this.animationQueue = [];
    }
    
    /**
     * 戦闘開始処理
     */
    startBattle(playerParty, enemyTypes) {
        this.state = 'encounter';
        this.playerParty = [...playerParty];
        this.enemies = this.generateEnemies(enemyTypes);
        
        // 全戦闘参加者をリスト化
        this.allCombatants = [...this.playerParty, ...this.enemies];
        
        // ターン順計算
        this.calculateTurnOrder();
        
        // 戦闘UI初期化
        this.battleUI.init(this.playerParty, this.enemies);
        
        // エンカウンターメッセージ
        const message = this.generateEncounterMessage();
        this.battleUI.showMessage(message);
        
        return {
            success: true,
            message: message,
            enemies: this.enemies.map(e => e.name)
        };
    }
    
    /**
     * ターン順計算（敏捷性ベース）
     */
    calculateTurnOrder() {
        this.turnOrder = this.allCombatants
            .filter(c => !c.isDead)
            .sort((a, b) => {
                // 敏捷性 + ランダム要素
                const aSpeed = a.stats.agility + Math.random() * 5;
                const bSpeed = b.stats.agility + Math.random() * 5;
                return bSpeed - aSpeed;
            });
    }
    
    /**
     * プレイヤーアクション処理
     */
    processPlayerAction(action) {
        const result = this.executeAction(action);
        
        // アニメーション追加
        this.queueAnimation(result.animation);
        
        // ターン進行
        this.advanceTurn();
        
        return result;
    }
    
    /**
     * アクション実行
     */
    executeAction(action) {
        switch (action.type) {
            case 'attack':
                return this.executePhysicalAttack(action.actor, action.target, action.weapon);
            case 'magic':
                return this.executeMagicAttack(action.actor, action.target, action.spell);
            case 'item':
                return this.executeItemUse(action.actor, action.target, action.item);
            case 'defend':
                return this.executeDefend(action.actor);
            case 'run':
                return this.executeRun();
            default:
                throw new Error(`Unknown action type: ${action.type}`);
        }
    }
    
    /**
     * 物理攻撃実行
     */
    executePhysicalAttack(attacker, target, weapon) {
        // 命中判定
        const hitChance = this.calculateHitChance(attacker, target, weapon);
        const hit = Math.random() < hitChance;
        
        if (!hit) {
            return {
                success: false,
                message: `${attacker.name}の攻撃は外れた！`,
                animation: { type: 'miss', target: target }
            };
        }
        
        // ダメージ計算
        const damage = this.calculatePhysicalDamage(attacker, target, weapon);
        const isCritical = Math.random() < 0.05; // 5%クリティカル
        const finalDamage = isCritical ? Math.floor(damage * 1.5) : damage;
        
        // ダメージ適用
        target.hp = Math.max(0, target.hp - finalDamage);
        
        return {
            success: true,
            damage: finalDamage,
            critical: isCritical,
            message: `${attacker.name}の攻撃！ ${finalDamage}のダメージ${isCritical ? '（会心の一撃！）' : ''}`,
            animation: { 
                type: isCritical ? 'critical_hit' : 'hit', 
                attacker: attacker,
                target: target,
                damage: finalDamage
            }
        };
    }
    
    /**
     * 物理ダメージ計算
     */
    calculatePhysicalDamage(attacker, target, weapon) {
        // 基本攻撃力
        const baseAttack = weapon ? weapon.attack : 1;
        const strengthBonus = Math.floor(attacker.stats.strength / 3);
        
        // 防御力
        const armor = target.equipment ? target.equipment.armor : null;
        const defense = armor ? armor.defense : 0;
        const vitalityReduction = Math.floor(target.stats.vitality / 5);
        
        // ランダム要素
        const randomFactor = 0.85 + Math.random() * 0.3; // 85-115%
        
        // 最終ダメージ計算
        const rawDamage = (baseAttack + strengthBonus) * randomFactor;
        const finalDamage = Math.max(1, Math.floor(rawDamage - defense - vitalityReduction));
        
        return finalDamage;
    }
    
    /**
     * 戦闘終了判定
     */
    checkBattleEnd() {
        const playersAlive = this.playerParty.some(p => p.hp > 0);
        const enemiesAlive = this.enemies.some(e => e.hp > 0);
        
        if (!playersAlive) {
            this.state = 'defeat';
            return { ended: true, result: 'defeat' };
        }
        
        if (!enemiesAlive) {
            this.state = 'victory';
            const rewards = this.calculateRewards();
            return { ended: true, result: 'victory', rewards: rewards };
        }
        
        return { ended: false };
    }
    
    /**
     * 報酬計算
     */
    calculateRewards() {
        let totalExp = 0;
        let totalGold = 0;
        let items = [];
        
        this.enemies.forEach(enemy => {
            totalExp += enemy.exp || 0;
            totalGold += enemy.gold || 0;
            
            // アイテムドロップ判定
            if (enemy.dropRate && Math.random() < enemy.dropRate) {
                const droppedItem = this.rollItemDrop(enemy);
                if (droppedItem) {
                    items.push(droppedItem);
                }
            }
        });
        
        return {
            experience: totalExp,
            gold: totalGold,
            items: items
        };
    }
}
```

### 4. キャラクターシステム設計

```javascript
/**
 * キャラクター基底クラス
 */
class Character {
    constructor(data) {
        this.id = data.id || this.generateId();
        this.name = data.name;
        this.level = data.level || 1;
        
        // 基本能力値
        this.stats = {
            strength: data.stats?.strength || this.generateStat(),
            intelligence: data.stats?.intelligence || this.generateStat(),
            agility: data.stats?.agility || this.generateStat(),
            vitality: data.stats?.vitality || this.generateStat()
        };
        
        // 計算済みパラメータ
        this.maxHP = this.calculateMaxHP();
        this.maxMP = this.calculateMaxMP();
        this.hp = data.hp || this.maxHP;
        this.mp = data.mp || this.maxMP;
        
        // 経験値
        this.experience = data.experience || 0;
        this.experienceToNext = this.calculateExpToNext();
        
        // 装備
        this.equipment = {
            weapon: null,
            armor: null,
            shield: null,
            helmet: null,
            accessory: null
        };
        
        // インベントリ
        this.inventory = data.inventory || [];
        this.gold = data.gold || 100;
        
        // 状態
        this.conditions = new Set(); // poison, sleep, paralysis, etc.
        this.isDead = false;
        
        // 外見
        this.appearance = data.appearance || this.generateAppearance();
    }
    
    /**
     * 基本能力値生成（5-19の範囲）
     */
    generateStat() {
        return Math.floor(Math.random() * 15) + 5;
    }
    
    /**
     * 最大HP計算
     */
    calculateMaxHP() {
        const baseHP = this.stats.vitality * 2;
        const levelBonus = (this.level - 1) * 3;
        return baseHP + levelBonus;
    }
    
    /**
     * 最大MP計算
     */
    calculateMaxMP() {
        const baseMP = this.stats.intelligence;
        const levelBonus = Math.floor((this.level - 1) * 1.5);
        return baseMP + levelBonus;
    }
    
    /**
     * 次レベルまでの経験値計算
     */
    calculateExpToNext() {
        return this.level * this.level * 100;
    }
    
    /**
     * 経験値獲得
     */
    gainExperience(amount) {
        this.experience += amount;
        const results = [];
        
        // レベルアップ判定
        while (this.experience >= this.experienceToNext) {
            const levelUpResult = this.levelUp();
            results.push(levelUpResult);
        }
        
        return results;
    }
    
    /**
     * レベルアップ処理
     */
    levelUp() {
        this.experience -= this.experienceToNext;
        this.level++;
        
        // ステータス成長
        const growth = this.calculateStatGrowth();
        Object.keys(growth).forEach(stat => {
            this.stats[stat] += growth[stat];
        });
        
        // HP/MP更新
        const oldMaxHP = this.maxHP;
        const oldMaxMP = this.maxMP;
        this.maxHP = this.calculateMaxHP();
        this.maxMP = this.calculateMaxMP();
        
        // 現在HP/MP増加
        this.hp += (this.maxHP - oldMaxHP);
        this.mp += (this.maxMP - oldMaxMP);
        
        // 次レベル経験値更新
        this.experienceToNext = this.calculateExpToNext();
        
        return {
            level: this.level,
            statGrowth: growth,
            hpGain: this.maxHP - oldMaxHP,
            mpGain: this.maxMP - oldMaxMP,
            message: `${this.name}はレベル${this.level}になった！`
        };
    }
    
    /**
     * ステータス成長計算
     */
    calculateStatGrowth() {
        return {
            strength: Math.random() < 0.6 ? 1 : 0,
            intelligence: Math.random() < 0.6 ? 1 : 0,
            agility: Math.random() < 0.6 ? 1 : 0,
            vitality: Math.random() < 0.8 ? 1 : 0 // 体力は成長しやすい
        };
    }
    
    /**
     * 装備変更
     */
    equipItem(item, slot) {
        if (!this.canEquip(item, slot)) {
            return { success: false, message: 'この装備は使用できません。' };
        }
        
        // 既存装備を外す
        const oldItem = this.equipment[slot];
        if (oldItem) {
            this.inventory.push(oldItem);
        }
        
        // 新装備を装着
        this.equipment[slot] = item;
        this.removeFromInventory(item);
        
        // ステータス再計算
        this.recalculateStats();
        
        return {
            success: true,
            message: `${item.name}を装備しました。`,
            oldItem: oldItem
        };
    }
    
    /**
     * 装備可能判定
     */
    canEquip(item, slot) {
        // スロット適合性
        if (item.slot !== slot) return false;
        
        // レベル制限
        if (item.requiredLevel && this.level < item.requiredLevel) return false;
        
        // 能力値制限
        if (item.requiredStats) {
            for (const [stat, required] of Object.entries(item.requiredStats)) {
                if (this.stats[stat] < required) return false;
            }
        }
        
        return true;
    }
    
    /**
     * 外見生成（320パターン）
     */
    generateAppearance() {
        const hairStyles = ['short', 'long', 'curly', 'bald', 'ponytail']; // 5種
        const hairColors = ['black', 'brown', 'blonde', 'red']; // 4色
        const clothingColors = ['blue', 'red', 'green', 'yellow', 'purple', 'white', 'black', 'orange']; // 8色
        const genders = ['male', 'female']; // 2種
        
        return {
            hairStyle: hairStyles[Math.floor(Math.random() * hairStyles.length)],
            hairColor: hairColors[Math.floor(Math.random() * hairColors.length)],
            clothingColor: clothingColors[Math.floor(Math.random() * clothingColors.length)],
            gender: genders[Math.floor(Math.random() * genders.length)]
        };
        // 合計: 5 × 4 × 8 × 2 = 320パターン
    }
}

/**
 * プレイヤーキャラクタークラス
 */
class PlayerCharacter extends Character {
    constructor(data) {
        super(data);
        this.isPlayer = true;
        this.profession = data.profession || 'fighter';
        
        // 習得魔法
        this.knownSpells = data.knownSpells || [];
        
        // 冒険記録
        this.statistics = {
            battlesWon: 0,
            monstersDefeated: 0,
            treasuresFound: 0,
            floorsExplored: 0,
            timesPoisoned: 0,
            timesHealed: 0
        };
    }
    
    /**
     * 魔法習得
     */
    learnSpell(spell) {
        if (this.canLearnSpell(spell)) {
            this.knownSpells.push(spell);
            return { success: true, message: `${spell.name}を覚えた！` };
        }
        return { success: false, message: '魔法を覚えることができません。' };
    }
    
    /**
     * 魔法習得可能判定
     */
    canLearnSpell(spell) {
        // レベル要件
        if (this.level < spell.requiredLevel) return false;
        
        // 知能要件
        if (this.stats.intelligence < spell.requiredIntelligence) return false;
        
        // 既習得チェック
        if (this.knownSpells.some(s => s.id === spell.id)) return false;
        
        return true;
    }
}
```

## 🗃️ データベース設計

### 1. ゲームデータ構造

```javascript
/**
 * ゲームセーブデータ構造
 */
const SAVE_DATA_SCHEMA = {
    version: "1.0.0",
    saveId: "string",
    timestamp: "number",
    player: {
        characters: [], // PlayerCharacter[]
        currentCharacter: "string", // character ID
        party: [], // character IDs
        gold: "number",
        inventory: [] // Item[]
    },
    world: {
        currentFloor: "string",
        position: { x: "number", y: "number" },
        direction: "number", // 0-3
        visitedTiles: [], // {floor: string, x: number, y: number}[]
        discoveredSecrets: [], // secret door/treasure locations
        completedQuests: [],
        gameFlags: {} // misc boolean flags
    },
    statistics: {
        playtime: "number", // milliseconds
        battlesWon: "number",
        monstersDefeated: "number",
        treasuresFound: "number",
        floorsExplored: "number",
        deathCount: "number"
    },
    settings: {
        difficulty: "string", // easy, normal, hard
        audioVolume: { bgm: "number", se: "number" },
        displayOptions: {
            showHP: "boolean",
            showMP: "boolean",
            autoMap: "boolean"
        }
    }
};

/**
 * マップデータ構造
 */
const MAP_DATA_SCHEMA = {
    id: "string", // "B1", "B2", etc.
    name: "string",
    size: { width: "number", height: "number" },
    tiles: [], // number[][] - tile type matrix
    entities: [
        {
            type: "string", // "enemy", "treasure", "stairs", "door"
            id: "string",
            position: { x: "number", y: "number" },
            properties: {} // entity-specific data
        }
    ],
    encounters: [
        {
            position: { x: "number", y: "number" },
            enemies: [], // enemy type strings
            rate: "number" // encounter probability
        }
    ],
    music: "string", // BGM filename
    ambientLight: "number", // 0-1
    specialRules: {} // floor-specific game rules
};
```

### 2. アイテム・装備データベース

```javascript
/**
 * アイテムマスターデータ
 */
const ITEM_DATABASE = {
    weapons: {
        knife: {
            id: "knife",
            name: "ナイフ",
            type: "weapon",
            subtype: "blade",
            attack: 2,
            accuracy: 90,
            weight: 1,
            price: 10,
            requiredLevel: 1,
            requiredStats: {},
            durability: 100,
            maxDurability: 100,
            description: "小さな刃物。軽くて扱いやすい。",
            rarity: "common",
            sprite: "knife.png"
        },
        short_sword: {
            id: "short_sword",
            name: "ショートソード",
            type: "weapon",
            subtype: "sword",
            attack: 10,
            accuracy: 90,
            weight: 3,
            price: 80,
            requiredLevel: 3,
            requiredStats: { strength: 8 },
            durability: 150,
            maxDurability: 150,
            description: "短い剣。バランスが良い。",
            rarity: "common",
            sprite: "short_sword.png"
        },
        broad_sword: {
            id: "broad_sword",
            name: "ブロードソード",
            type: "weapon",
            subtype: "sword",
            attack: 20,
            accuracy: 80,
            weight: 9,
            price: 640,
            requiredLevel: 7,
            requiredStats: { strength: 15 },
            durability: 200,
            maxDurability: 200,
            description: "幅広の剣。高い攻撃力を持つ。",
            rarity: "uncommon",
            sprite: "broad_sword.png"
        }
    },
    armor: {
        leather_armor: {
            id: "leather_armor",
            name: "レザーアーマー",
            type: "armor",
            subtype: "light",
            defense: 2,
            weight: 3,
            price: 40,
            requiredLevel: 1,
            durability: 100,
            maxDurability: 100,
            description: "革製の軽装鎧。",
            rarity: "common",
            sprite: "leather_armor.png"
        },
        chain_mail: {
            id: "chain_mail",
            name: "チェインメイル",
            type: "armor",
            subtype: "medium",
            defense: 5,
            weight: 8,
            price: 320,
            requiredLevel: 4,
            durability: 180,
            maxDurability: 180,
            description: "鎖帷子。優れた防御力。",
            rarity: "uncommon",
            sprite: "chain_mail.png"
        }
    },
    consumables: {
        heal_potion: {
            id: "heal_potion",
            name: "回復薬",
            type: "consumable",
            subtype: "healing",
            effect: "heal",
            power: 20,
            weight: 0.1,
            price: 50,
            stackable: true,
            maxStack: 99,
            description: "HPを20回復する。",
            rarity: "common",
            sprite: "heal_potion.png"
        }
    }
};

/**
 * モンスターマスターデータ
 */
const MONSTER_DATABASE = {
    bat: {
        id: "bat",
        name: "コウモリ",
        level: 1,
        stats: {
            hp: 3,
            attack: 1,
            defense: 0,
            agility: 15
        },
        rewards: {
            experience: 1,
            gold: 1,
            dropRate: 0.1,
            possibleDrops: ["heal_potion"]
        },
        behavior: {
            aggressive: false,
            groupSize: [1, 3],
            fleeThreshold: 0.2
        },
        sprite: {
            idle: "bat_idle.png",
            attack: "bat_attack.png",
            hurt: "bat_hurt.png"
        },
        sounds: {
            encounter: "bat_screech.wav",
            attack: "bat_attack.wav",
            death: "bat_death.wav"
        }
    },
    kobold: {
        id: "kobold",
        name: "コボルト",
        level: 2,
        stats: {
            hp: 8,
            attack: 3,
            defense: 1,
            agility: 10
        },
        rewards: {
            experience: 5,
            gold: 8,
            dropRate: 0.2,
            possibleDrops: ["knife", "leather_armor"]
        },
        behavior: {
            aggressive: true,
            groupSize: [1, 5],
            fleeThreshold: 0.1
        },
        sprite: {
            idle: "kobold_idle.png",
            attack: "kobold_attack.png",
            hurt: "kobold_hurt.png"
        }
    },
    giant: {
        id: "giant",
        name: "ジャイアント",
        level: 10,
        stats: {
            hp: 200,
            attack: 50,
            defense: 10,
            agility: 5
        },
        rewards: {
            experience: 500,
            gold: 1000,
            dropRate: 0.8,
            possibleDrops: ["broad_sword", "chain_mail", "rare_gem"]
        },
        behavior: {
            aggressive: true,
            groupSize: [1, 1],
            boss: true,
            specialAttacks: ["stomp", "boulder_throw"]
        },
        sprite: {
            idle: "giant_idle.png",
            attack: "giant_attack.png",
            special: "giant_stomp.png"
        }
    }
};
```

## 🎨 ユーザーインターフェース設計

### 1. UI コンポーネント階層

```javascript
/**
 * UIシステム基底クラス
 */
class UISystem {
    constructor() {
        this.components = new Map();
        this.activeWindows = [];
        this.focusedComponent = null;
        
        // UI状態
        this.isVisible = true;
        this.theme = 'retro';
        this.scale = 1.0;
    }
    
    /**
     * UIコンポーネント基底クラス
     */
    createComponent(type, config) {
        switch (type) {
            case 'window':
                return new UIWindow(config);
            case 'button':
                return new UIButton(config);
            case 'text':
                return new UIText(config);
            case 'bar':
                return new UIBar(config);
            case 'menu':
                return new UIMenu(config);
            default:
                throw new Error(`Unknown UI component type: ${type}`);
        }
    }
}

/**
 * ゲーム内ウィンドウクラス
 */
class UIWindow {
    constructor(config) {
        this.id = config.id;
        this.title = config.title || '';
        this.position = config.position || { x: 0, y: 0 };
        this.size = config.size || { width: 400, height: 300 };
        this.isVisible = config.visible !== false;
        this.isModal = config.modal || false;
        this.canClose = config.canClose !== false;
        
        this.children = [];
        this.background = config.background || '#000080';
        this.border = config.border || '#FFFF00';
    }
    
    addChild(component) {
        this.children.push(component);
        component.parent = this;
    }
    
    render(ctx) {
        if (!this.isVisible) return;
        
        // ウィンドウ背景
        ctx.fillStyle = this.background;
        ctx.fillRect(this.position.x, this.position.y, this.size.width, this.size.height);
        
        // ウィンドウ枠
        ctx.strokeStyle = this.border;
        ctx.lineWidth = 2;
        ctx.strokeRect(this.position.x, this.position.y, this.size.width, this.size.height);
        
        // タイトルバー
        if (this.title) {
            ctx.fillStyle = this.border;
            ctx.fillRect(this.position.x, this.position.y, this.size.width, 24);
            
            ctx.fillStyle = this.background;
            ctx.font = '16px "Courier New", monospace';
            ctx.fillText(this.title, this.position.x + 8, this.position.y + 18);
        }
        
        // 子コンポーネント描画
        this.children.forEach(child => child.render(ctx));
    }
}

/**
 * ステータスバークラス
 */
class UIBar {
    constructor(config) {
        this.position = config.position;
        this.size = config.size || { width: 200, height: 20 };
        this.value = config.value || 0;
        this.maxValue = config.maxValue || 100;
        this.colors = config.colors || {
            background: '#404040',
            fill: '#FF0000',
            border: '#FFFFFF'
        };
        this.label = config.label || '';
    }
    
    setValue(value, maxValue) {
        this.value = Math.max(0, Math.min(value, maxValue || this.maxValue));
        if (maxValue !== undefined) {
            this.maxValue = maxValue;
        }
    }
    
    render(ctx) {
        const fillWidth = (this.value / this.maxValue) * this.size.width;
        
        // 背景
        ctx.fillStyle = this.colors.background;
        ctx.fillRect(this.position.x, this.position.y, this.size.width, this.size.height);
        
        // 値部分
        ctx.fillStyle = this.colors.fill;
        ctx.fillRect(this.position.x, this.position.y, fillWidth, this.size.height);
        
        // 枠線
        ctx.strokeStyle = this.colors.border;
        ctx.strokeRect(this.position.x, this.position.y, this.size.width, this.size.height);
        
        // ラベル
        if (this.label) {
            ctx.fillStyle = '#FFFFFF';
            ctx.font = '12px "Courier New", monospace';
            ctx.fillText(this.label, this.position.x, this.position.y - 4);
        }
    }
}

/**
 * 戦闘UI管理クラス
 */
class BattleUI {
    constructor() {
        this.playerStatusPanels = [];
        this.enemyDisplays = [];
        this.commandMenu = null;
        this.messageWindow = null;
        this.animationQueue = [];
    }
    
    init(playerParty, enemies) {
        // プレイヤーステータスパネル初期化
        playerParty.forEach((character, index) => {
            const panel = this.createStatusPanel(character, {
                x: 20,
                y: 20 + (index * 100)
            });
            this.playerStatusPanels.push(panel);
        });
        
        // 敵表示初期化
        enemies.forEach((enemy, index) => {
            const display = this.createEnemyDisplay(enemy, {
                x: 500 + (index * 60),
                y: 200
            });
            this.enemyDisplays.push(display);
        });
        
        // コマンドメニュー初期化
        this.commandMenu = this.createCommandMenu();
        
        // メッセージウィンドウ初期化
        this.messageWindow = this.createMessageWindow();
    }
    
    createStatusPanel(character, position) {
        const panel = new UIWindow({
            id: `status_${character.id}`,
            position: position,
            size: { width: 200, height: 90 },
            title: character.name
        });
        
        // HPバー
        const hpBar = new UIBar({
            position: { x: position.x + 10, y: position.y + 35 },
            size: { width: 180, height: 15 },
            value: character.hp,
            maxValue: character.maxHP,
            colors: { background: '#400000', fill: '#FF0000', border: '#FFFFFF' },
            label: 'HP'
        });
        panel.addChild(hpBar);
        
        // MPバー（魔法使いの場合）
        if (character.maxMP > 0) {
            const mpBar = new UIBar({
                position: { x: position.x + 10, y: position.y + 55 },
                size: { width: 180, height: 15 },
                value: character.mp,
                maxValue: character.maxMP,
                colors: { background: '#000040', fill: '#0000FF', border: '#FFFFFF' },
                label: 'MP'
            });
            panel.addChild(mpBar);
        }
        
        return panel;
    }
    
    update(deltaTime) {
        // アニメーション更新
        this.updateAnimations(deltaTime);
        
        // ステータス更新
        this.updateStatusDisplays();
    }
    
    render(ctx) {
        // 全UI要素描画
        this.playerStatusPanels.forEach(panel => panel.render(ctx));
        this.enemyDisplays.forEach(display => display.render(ctx));
        
        if (this.commandMenu?.isVisible) {
            this.commandMenu.render(ctx);
        }
        
        if (this.messageWindow?.isVisible) {
            this.messageWindow.render(ctx);
        }
        
        // アニメーション描画
        this.renderAnimations(ctx);
    }
}
```

---

**詳細設計書バージョン**: 1.0  
**最終更新**: 2025年7月26日  
**承認者**: Black Onyx Reborn Development Team  
**実装開始予定**: 設計書承認後