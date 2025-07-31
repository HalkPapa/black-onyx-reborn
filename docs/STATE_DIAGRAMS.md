# 🔄 ブラックオニキス復刻版 - ゲーム状態遷移図

## 📋 状態遷移設計概要

### 設計目的
ゲーム全体の状態管理を明確に定義し、状態間の遷移条件・処理を可視化する。

### 状態管理原則
- **明確な状態定義**: 各状態の責任範囲を明確化
- **遷移条件の明示**: 状態変更のトリガーを具体化
- **データ整合性**: 状態変更時のデータ保護
- **エラー復帰**: 異常状態からの復旧手順

## 🎮 メインゲーム状態遷移

### 1. アプリケーション全体状態

```mermaid
stateDiagram-v2
    [*] --> Loading
    Loading --> Title : アセット読み込み完了
    Loading --> Error : 読み込み失敗
    
    Title --> CharacterCreation : 新規ゲーム選択
    Title --> LoadGame : セーブデータ選択
    Title --> Settings : 設定選択
    Title --> Credits : クレジット選択
    
    CharacterCreation --> Gameplay : キャラクター作成完了
    CharacterCreation --> Title : キャンセル
    
    LoadGame --> Gameplay : ロード成功
    LoadGame --> Title : ロード失敗/キャンセル
    
    Settings --> Title : 設定完了
    Credits --> Title : クレジット終了
    
    Gameplay --> Battle : エンカウンター発生
    Gameplay --> Menu : メニューキー
    Gameplay --> GameOver : HP0
    
    Battle --> Gameplay : 戦闘終了
    Battle --> GameOver : 全滅
    
    Menu --> Gameplay : メニュー閉じる
    Menu --> SaveGame : セーブ選択
    Menu --> LoadGame : ロード選択
    Menu --> Title : タイトルに戻る
    
    SaveGame --> Menu : セーブ完了
    GameOver --> Title : ゲームオーバー処理完了
    
    Error --> Title : エラー回復
    Error --> [*] : 致命的エラー
```

### 2. ゲームプレイ詳細状態

```mermaid
stateDiagram-v2
    [*] --> DungeonExploration
    
    state DungeonExploration {
        [*] --> Idle
        Idle --> Moving : 移動入力
        Moving --> Idle : 移動完了
        Moving --> WallBlocked : 壁衝突
        WallBlocked --> Idle : 効果音再生完了
        
        Idle --> Searching : 調べる入力
        Searching --> SecretFound : 隠し要素発見
        Searching --> NothingFound : 何も発見されず
        SecretFound --> Idle : メッセージ表示完了
        NothingFound --> Idle : メッセージ表示完了
        
        Idle --> ObjectInteraction : オブジェクト調べる
        ObjectInteraction --> ItemGained : アイテム取得
        ObjectInteraction --> DoorOpened : 扉開放
        ObjectInteraction --> TrapTriggered : 罠発動
        ItemGained --> Idle : 取得処理完了
        DoorOpened --> Idle : 開放処理完了
        TrapTriggered --> Idle : 罠処理完了
        
        Idle --> EncounterCheck : 移動後判定
        EncounterCheck --> Idle : エンカウンターなし
        EncounterCheck --> [*] : エンカウンター発生
    }
    
    DungeonExploration --> InventoryScreen : インベントリキー
    DungeonExploration --> StatusScreen : ステータスキー
    DungeonExploration --> FloorChange : 階段使用
    
    InventoryScreen --> DungeonExploration : インベントリ閉じる
    StatusScreen --> DungeonExploration : ステータス閉じる
    FloorChange --> DungeonExploration : フロア移動完了
```

### 3. 戦闘状態遷移

```mermaid
stateDiagram-v2
    [*] --> BattleStart
    BattleStart --> EncounterMessage : 戦闘開始
    EncounterMessage --> TurnStart : メッセージ確認
    
    state BattleTurn {
        [*] --> TurnStart
        TurnStart --> PlayerTurn : プレイヤーターン
        TurnStart --> EnemyTurn : 敵ターン
        
        state PlayerTurn {
            [*] --> CommandSelect
            CommandSelect --> AttackSelect : 攻撃選択
            CommandSelect --> MagicSelect : 魔法選択
            CommandSelect --> ItemSelect : アイテム選択
            CommandSelect --> DefendSelect : 防御選択
            CommandSelect --> RunAttempt : 逃走選択
            
            AttackSelect --> TargetSelect : 対象選択
            MagicSelect --> SpellSelect : 魔法選択
            SpellSelect --> TargetSelect : 対象選択
            ItemSelect --> ItemChoose : アイテム選択
            ItemChoose --> TargetSelect : 対象選択
            
            TargetSelect --> ActionExecute : 対象決定
            DefendSelect --> ActionExecute : 防御実行
            RunAttempt --> RunCheck : 逃走判定
            
            RunCheck --> [*] : 逃走成功
            RunCheck --> ActionExecute : 逃走失敗
            
            ActionExecute --> AnimationPlay : アクション実行
            AnimationPlay --> DamageCalc : アニメーション完了
            DamageCalc --> ResultShow : ダメージ適用
            ResultShow --> [*] : 結果表示完了
        }
        
        state EnemyTurn {
            [*] --> AIThink
            AIThink --> AIAction : 行動決定
            AIAction --> AIAnimation : AI行動実行
            AIAnimation --> AIDamage : アニメーション完了
            AIDamage --> AIResult : ダメージ適用
            AIResult --> [*] : AI結果表示完了
        }
        
        PlayerTurn --> BattleEndCheck : ターン終了
        EnemyTurn --> BattleEndCheck : ターン終了
        
        BattleEndCheck --> TurnStart : 戦闘継続
        BattleEndCheck --> [*] : 戦闘終了
    }
    
    BattleTurn --> Victory : 敵全滅
    BattleTurn --> Defeat : プレイヤー全滅
    BattleTurn --> Escaped : 逃走成功
    
    Victory --> RewardCalc : 勝利処理
    RewardCalc --> ExperienceGain : 報酬計算
    ExperienceGain --> LevelUpCheck : 経験値獲得
    LevelUpCheck --> LevelUp : レベルアップ
    LevelUpCheck --> [*] : レベルアップなし
    LevelUp --> [*] : レベルアップ処理完了
    
    Defeat --> GameOverScreen : 敗北処理
    GameOverScreen --> [*] : ゲームオーバー
    
    Escaped --> [*] : 逃走完了
```

### 4. キャラクター管理状態

```mermaid
stateDiagram-v2
    [*] --> CharacterCreation
    
    state CharacterCreation {
        [*] --> NameInput
        NameInput --> AppearanceSelect : 名前入力完了
        AppearanceSelect --> StatGeneration : 外見選択完了
        StatGeneration --> ConfirmCreation : ステータス生成完了
        ConfirmCreation --> [*] : 作成確認
        ConfirmCreation --> NameInput : やり直し
    }
    
    CharacterCreation --> CharacterActive : 作成完了
    
    state CharacterActive {
        [*] --> Normal
        Normal --> LevelUp : 経験値十分
        Normal --> StatusEffect : 状態異常付与
        Normal --> Equipment : 装備変更
        Normal --> ItemUse : アイテム使用
        
        LevelUp --> StatIncrease : レベルアップ処理
        StatIncrease --> Normal : ステータス上昇完了
        
        StatusEffect --> Poisoned : 毒状態
        StatusEffect --> Paralyzed : 麻痺状態
        StatusEffect --> Sleeping : 睡眠状態
        StatusEffect --> Strengthened : 強化状態
        
        Poisoned --> Normal : 毒回復
        Paralyzed --> Normal : 麻痺回復
        Sleeping --> Normal : 睡眠回復
        Strengthened --> Normal : 強化効果終了
        
        Equipment --> EquipmentChange : 装備処理
        EquipmentChange --> StatRecalc : 装備変更完了
        StatRecalc --> Normal : ステータス再計算完了
        
        ItemUse --> HealingItem : 回復アイテム
        ItemUse --> BuffItem : 強化アイテム
        HealingItem --> Normal : 回復処理完了
        BuffItem --> StatusEffect : 強化効果付与
        
        Normal --> KnockedOut : HP0
        KnockedOut --> Normal : 蘇生
        KnockedOut --> [*] : 完全死亡
    }
```

### 5. インベントリ管理状態

```mermaid
stateDiagram-v2
    [*] --> InventoryIdle
    
    InventoryIdle --> ItemAdd : アイテム獲得
    InventoryIdle --> ItemUse : アイテム使用
    InventoryIdle --> ItemDrop : アイテム破棄
    InventoryIdle --> ItemSort : インベントリ整理
    InventoryIdle --> Equipment : 装備操作
    
    state ItemAdd {
        [*] --> CapacityCheck
        CapacityCheck --> StackCheck : 容量OK
        CapacityCheck --> CapacityFull : 容量不足
        
        StackCheck --> AddToStack : スタック可能
        StackCheck --> CreateNew : 新規作成
        
        AddToStack --> [*] : スタック追加完了
        CreateNew --> [*] : 新規アイテム作成完了
        CapacityFull --> [*] : 取得失敗
    }
    
    state ItemUse {
        [*] --> UsabilityCheck
        UsabilityCheck --> ApplyEffect : 使用可能
        UsabilityCheck --> UseFailed : 使用不可
        
        ApplyEffect --> ConsumeCheck : 効果適用
        ConsumeCheck --> ItemConsume : 消費アイテム
        ConsumeCheck --> [*] : 非消費アイテム
        
        ItemConsume --> QuantityReduce : アイテム消費
        QuantityReduce --> ItemRemove : 残量0
        QuantityReduce --> [*] : 残量あり
        ItemRemove --> [*] : アイテム削除完了
        
        UseFailed --> [*] : 使用失敗
    }
    
    state Equipment {
        [*] --> EquipabilityCheck
        EquipabilityCheck --> UnequipCurrent : 装備可能
        EquipabilityCheck --> EquipFailed : 装備不可
        
        UnequipCurrent --> EquipNew : 現装備解除
        EquipNew --> StatUpdate : 新装備装着
        StatUpdate --> [*] : ステータス更新完了
        
        EquipFailed --> [*] : 装備失敗
    }
    
    ItemAdd --> InventoryIdle : 処理完了
    ItemUse --> InventoryIdle : 処理完了
    ItemDrop --> InventoryIdle : 処理完了
    ItemSort --> InventoryIdle : 処理完了
    Equipment --> InventoryIdle : 処理完了
```

### 6. セーブ・ロード状態

```mermaid
stateDiagram-v2
    [*] --> SaveLoadIdle
    
    SaveLoadIdle --> SaveProcess : セーブ要求
    SaveLoadIdle --> LoadProcess : ロード要求
    
    state SaveProcess {
        [*] --> DataCollect
        DataCollect --> DataValidate : データ収集完了
        DataValidate --> DataCompress : データ検証OK
        DataValidate --> SaveError : データ検証NG
        DataCompress --> DatabaseWrite : データ圧縮完了
        DatabaseWrite --> SaveComplete : 書き込み成功
        DatabaseWrite --> SaveError : 書き込み失敗
        
        SaveComplete --> [*] : セーブ成功
        SaveError --> [*] : セーブ失敗
    }
    
    state LoadProcess {
        [*] --> SlotSelect
        SlotSelect --> DataLoad : スロット選択
        SlotSelect --> LoadCancel : キャンセル
        
        DataLoad --> DataValidate : データ読み込み完了
        DataLoad --> LoadError : データ読み込み失敗
        
        DataValidate --> DataMigrate : データ検証OK
        DataValidate --> LoadError : データ検証NG
        
        DataMigrate --> DataApply : マイグレーション完了
        DataApply --> LoadComplete : データ適用完了
        
        LoadComplete --> [*] : ロード成功
        LoadError --> [*] : ロード失敗
        LoadCancel --> [*] : ロードキャンセル
    }
    
    SaveProcess --> SaveLoadIdle : 処理完了
    LoadProcess --> SaveLoadIdle : 処理完了
```

### 7. オーディオシステム状態

```mermaid
stateDiagram-v2
    [*] --> AudioInit
    AudioInit --> AudioReady : 初期化成功
    AudioInit --> AudioError : 初期化失敗
    
    AudioError --> AudioInit : 再初期化
    AudioError --> AudioDisabled : 音響無効化
    
    state AudioReady {
        [*] --> Idle
        Idle --> BGMPlay : BGM再生要求
        Idle --> SEPlay : SE再生要求
        
        state BGMPlay {
            [*] --> BGMLoad
            BGMLoad --> BGMStart : 読み込み完了
            BGMLoad --> BGMError : 読み込み失敗
            BGMStart --> BGMPlaying : 再生開始
            BGMError --> [*] : エラー処理
        }
        
        state BGMPlaying {
            [*] --> Playing
            Playing --> Paused : 一時停止
            Playing --> Stopped : 停止
            Playing --> FadeOut : フェードアウト
            Paused --> Playing : 再開
            FadeOut --> Stopped : フェード完了
            Stopped --> [*] : 停止完了
        }
        
        state SEPlay {
            [*] --> SELoad
            SELoad --> SEStart : 読み込み完了
            SELoad --> SEError : 読み込み失敗
            SEStart --> SEPlaying : 再生開始
            SEPlaying --> SEFinish : 再生終了
            SEFinish --> [*] : SE完了
            SEError --> [*] : エラー処理
        }
        
        BGMPlay --> Idle : BGM処理完了
        SEPlay --> Idle : SE処理完了
    }
    
    AudioDisabled --> AudioReady : 音響有効化
```

## 🔧 状態管理実装クラス

### 状態管理基底クラス

```typescript
/**
 * 状態管理基底クラス
 */
abstract class StateMachine<TState extends string> {
    protected currentState: TState;
    protected previousState: TState | null = null;
    protected stateHistory: TState[] = [];
    protected transitions: Map<TState, TState[]> = new Map();
    protected stateData: Map<TState, any> = new Map();
    protected listeners: Map<string, Function[]> = new Map();
    
    constructor(initialState: TState) {
        this.currentState = initialState;
        this.stateHistory.push(initialState);
    }
    
    /**
     * 状態遷移定義
     */
    protected defineTransition(from: TState, to: TState[]): void {
        this.transitions.set(from, to);
    }
    
    /**
     * 状態変更
     */
    async transitionTo(newState: TState, data?: any): Promise<boolean> {
        // 遷移可能性チェック
        if (!this.canTransitionTo(newState)) {
            console.warn(`Invalid transition: ${this.currentState} -> ${newState}`);
            return false;
        }
        
        const oldState = this.currentState;
        
        try {
            // 現在状態の終了処理
            await this.onStateExit(oldState);
            
            // 状態更新
            this.previousState = oldState;
            this.currentState = newState;
            this.stateHistory.push(newState);
            
            // 状態データ設定
            if (data) {
                this.stateData.set(newState, data);
            }
            
            // 新状態の開始処理
            await this.onStateEnter(newState, data);
            
            // リスナー通知
            this.notifyStateChange(oldState, newState);
            
            return true;
        } catch (error) {
            // エラー時はロールバック
            this.currentState = oldState;
            throw error;
        }
    }
    
    /**
     * 遷移可能性判定
     */
    protected canTransitionTo(newState: TState): boolean {
        const allowedTransitions = this.transitions.get(this.currentState);
        return allowedTransitions ? allowedTransitions.includes(newState) : false;
    }
    
    /**
     * 状態開始処理（サブクラスで実装）
     */
    protected abstract onStateEnter(state: TState, data?: any): Promise<void>;
    
    /**
     * 状態終了処理（サブクラスで実装）
     */
    protected abstract onStateExit(state: TState): Promise<void>;
    
    /**
     * 現在状態取得
     */
    getCurrentState(): TState {
        return this.currentState;
    }
    
    /**
     * 状態履歴取得
     */
    getStateHistory(): TState[] {
        return [...this.stateHistory];
    }
    
    /**
     * 状態変更リスナー登録
     */
    onStateChange(callback: (oldState: TState, newState: TState) => void): void {
        if (!this.listeners.has('stateChange')) {
            this.listeners.set('stateChange', []);
        }
        this.listeners.get('stateChange')!.push(callback);
    }
    
    private notifyStateChange(oldState: TState, newState: TState): void {
        const callbacks = this.listeners.get('stateChange') || [];
        callbacks.forEach(callback => callback(oldState, newState));
    }
}

/**
 * ゲームメイン状態管理
 */
type GameState = 'loading' | 'title' | 'character_creation' | 'gameplay' | 'battle' | 'menu' | 'game_over';

class GameStateMachine extends StateMachine<GameState> {
    constructor() {
        super('loading');
        this.setupTransitions();
    }
    
    private setupTransitions(): void {
        this.defineTransition('loading', ['title', 'error']);
        this.defineTransition('title', ['character_creation', 'gameplay', 'settings']);
        this.defineTransition('character_creation', ['gameplay', 'title']);
        this.defineTransition('gameplay', ['battle', 'menu']);
        this.defineTransition('battle', ['gameplay', 'game_over']);
        this.defineTransition('menu', ['gameplay', 'title']);
        this.defineTransition('game_over', ['title']);
    }
    
    protected async onStateEnter(state: GameState, data?: any): Promise<void> {
        switch (state) {
            case 'loading':
                await this.handleLoadingEnter();
                break;
            case 'title':
                await this.handleTitleEnter();
                break;
            case 'character_creation':
                await this.handleCharacterCreationEnter();
                break;
            case 'gameplay':
                await this.handleGameplayEnter(data);
                break;
            case 'battle':
                await this.handleBattleEnter(data);
                break;
            case 'menu':
                await this.handleMenuEnter();
                break;
            case 'game_over':
                await this.handleGameOverEnter();
                break;
        }
    }
    
    protected async onStateExit(state: GameState): Promise<void> {
        switch (state) {
            case 'loading':
                await this.handleLoadingExit();
                break;
            case 'gameplay':
                await this.handleGameplayExit();
                break;
            case 'battle':
                await this.handleBattleExit();
                break;
            // その他の状態の終了処理
        }
    }
    
    private async handleLoadingEnter(): Promise<void> {
        // アセット読み込み開始
        console.log('Loading assets...');
    }
    
    private async handleBattleEnter(battleData: any): Promise<void> {
        // 戦闘UI初期化
        // BGM変更
        console.log('Battle started:', battleData);
    }
    
    // その他のハンドラー実装...
}
```

---

**ゲーム状態遷移図バージョン**: 1.0  
**最終更新**: 2025年7月26日  
**承認者**: Black Onyx Reborn Development Team  
**実装方針**: 状態遷移図に基づき、明確な状態管理とエラーハンドリングを実装すること