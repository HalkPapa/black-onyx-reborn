# 🏰 ブラックオニキス復刻版 - 完全セットアップガイド

## 🎯 夜間開発完了！

睡眠中の夜間開発により、ブラックオニキス復刻版に以下の全システムが実装完了しました：

### ✅ 夜間開発成果

1. **基本的な敵システム** - AI搭載の敵キャラクター
2. **シンプルな戦闘システム** - 自動戦闘・レベルアップシステム
3. **アイテム収集システム** - インベントリ・アイテム効果
4. **ゲームオーバーシステム** - 統計表示・リトライ機能
5. **完全なセーブ・ロード機能** - プレイヤー進捗・設定保存
6. **パフォーマンス最適化** - FPS監視・メモリ管理・オブジェクトプール
7. **デバッグ機能強化** - インゲームコンソール・開発者コマンド

## 🚀 完全統合セットアップ手順

### 1. GameManagerの完全設定

#### GameManagerオブジェクト作成
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "GameManager" に変更
3. 以下のコンポーネントを追加：
   - GameManager
   - AudioManager
   - DungeonManager
   - SaveManager
   - CombatManager
   - EnemyManager
   - ItemManager
   - PerformanceOptimizer
   - DebugConsole
```

#### 設定値
```
GameManager:
- Debug Mode: true (開発時)
- Target Frame Rate: 60

AudioManager:
- Master Volume: 1.0
- BGM Volume: 0.7
- SE Volume: 0.8

DungeonManager:
- Dungeon Width: 16
- Dungeon Height: 16
- Max Floors: 10

CombatManager:
- Base Player Attack: 10
- Base Player Defense: 5
- Auto Resolve Combat: true

EnemyManager:
- Max Enemies Per Floor: 8
- Min Enemies Per Floor: 3

ItemManager:
- Max Inventory Slots: 20
- Item Spawn Chance: 0.3

PerformanceOptimizer:
- Enable Optimizations: true
- Show FPS Counter: false (リリース時)
- Auto Garbage Collection: true

DebugConsole:
- Toggle Key: F12
- Show On Start: false
```

### 2. GameSceneの完全構築

#### UI構築
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "GameUI" に変更
3. 以下のコンポーネントを追加：
   - GameUIController
   - DungeonMapRenderer
   - GameOverUI
```

#### PlayerController設定
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "PlayerController" に変更
3. PlayerController コンポーネント追加
4. 設定：
   - Move Delay: 0.2s
   - Allow Diagonal Movement: false
   - Play Walk Sound: true
```

### 3. 敵・アイテムデータベース構築

#### フォルダ構造
```
Assets/
├── Scripts/
│   ├── Managers/
│   ├── Enemy/
│   ├── Items/
│   ├── Combat/
│   ├── Player/
│   ├── UI/
│   └── Utils/
├── Audio/
│   ├── BGM/
│   └── SE/
├── Resources/
│   ├── Enemies/
│   └── Items/
└── Scenes/
    ├── MainMenu.unity
    └── GameScene.unity
```

#### デフォルトデータ作成
システムが自動でデフォルトの敵・アイテムデータを作成しますが、
カスタムデータを作成する場合：

```
右クリック → Create → Black Onyx → Enemy Data
右クリック → Create → Black Onyx → Item Data
```

### 4. オーディオシステム完全設定

#### 必要な音声ファイル
```
Assets/Audio/BGM/
- title.mp3      (タイトル音楽)
- dungeon.mp3    (ダンジョン音楽)
- battle.mp3     (戦闘音楽)

Assets/Audio/SE/
- button.wav     (ボタンクリック)
- walk.wav       (歩行音)
- attack.wav     (攻撃音)
- hit.wav        (被ダメージ音)
- item.wav       (アイテム取得音)
- levelup.wav    (レベルアップ音)
- gameover.wav   (ゲームオーバー音)
```

#### AudioManager Inspector設定
```
1. GameManager を選択
2. AudioManager コンポーネントで：
   - Title BGM: title.mp3 をドラッグ
   - Dungeon BGM: dungeon.mp3 をドラッグ
   - Battle BGM: battle.mp3 をドラッグ
   - SE Clips: 各種WAVファイルを配列に追加
```

## 🎮 完全機能一覧

### コア機能
- **ダンジョン探索**: テキストマップでの探索
- **プレイヤー移動**: 4方向移動・キー入力処理
- **敵システム**: AI付き敵キャラクター・スポーン管理
- **戦闘システム**: 自動戦闘・ダメージ計算・レベルアップ
- **アイテムシステム**: インベントリ管理・アイテム効果
- **セーブ・ロード**: 完全な進捗保存・設定保存

### UI機能
- **ゲーム画面**: リアルタイムステータス・マップ表示・メッセージログ
- **ゲームオーバー**: 統計表示・リトライ・メニュー復帰
- **インベントリ**: アイテム管理・使用・情報表示

### 開発・デバッグ機能
- **デバッグコンソール**: F12で開く・豊富なコマンド
- **パフォーマンス監視**: FPS表示・メモリ管理
- **オブジェクトプール**: メモリ効率化

## 🕹️ 操作方法

### 基本操作
```
矢印キー / WASD  : プレイヤー移動
テンキー         : ローグライク風移動
ESC             : ポーズ・メニュー
M               : マップ表示切替
```

### デバッグコマンド（F12コンソール）
```
help            : コマンド一覧表示
save / load     : セーブ・ロード
heal [amount]   : HP回復
god             : 無敵モード切替
teleport <x> <y>: テレポート
additem <id>    : アイテム追加
spawnenemy <name>: 敵生成
kill            : 全敵削除
stats           : 統計表示
floor <number>  : フロア移動
fps             : FPS表示切替
clear           : コンソールクリア
```

## 🎯 テスト・デバッグ手順

### 1. 基本動作確認
```
1. GameScene でプレイ開始
2. コンソールログで初期化確認：
   ✅ Game Manager Initialized
   ✅ Dungeon Manager initialized
   ✅ Enemy Manager initialized
   ✅ Item Manager initialized
   ✅ Combat Manager initialized
   ✅ Performance Optimizer initialized
   ✅ Debug Console ready
```

### 2. システム動作テスト
```
移動テスト:
- 矢印キーで移動
- 壁での移動ブロック確認
- マップ更新確認

戦闘テスト:
- 敵に接触して戦闘開始
- 自動戦闘進行確認
- レベルアップ確認

アイテムテスト:
- アイテム自動取得確認
- インベントリ表示確認
- アイテム使用確認

セーブテスト:
- F12 → save コマンド
- ゲーム終了→再起動
- F12 → load コマンド
```

### 3. パフォーマンステスト
```
- FPS監視（60FPS維持確認）
- メモリ使用量監視
- 長時間プレイでの安定性確認
```

## 🛠️ カスタマイズ・拡張

### ゲームバランス調整
```csharp
// CombatManager
basePlayerAttack = 15;      // プレイヤー攻撃力
basePlayerDefense = 8;      // プレイヤー防御力

// EnemyManager  
maxEnemiesPerFloor = 10;    // フロア内最大敵数

// ItemManager
maxInventorySlots = 30;     // インベントリサイズ
```

### UI・表示カスタマイズ
```csharp
// DungeonMapRenderer
viewRadius = 7;             // 視界範囲拡大
enableFogOfWar = false;     // 戦場の霧無効

// GameUIController
mapPanelWidth = 500f;       // マップパネル幅
```

### パフォーマンス調整
```csharp
// PerformanceOptimizer
targetFrameRate = 30;       // 省電力モード
autoGarbageCollection = false; // 手動GC
```

## 🚨 トラブルシューティング

### コンパイルエラー
```
問題: 型が見つからない
解決: 名前空間 BlackOnyxReborn が正しく設定されているか確認
```

### 実行時エラー
```
問題: NullReferenceException
解決: GameManager が正しく初期化されているか確認
     Singleton パターンが正常に動作しているか確認
```

### パフォーマンス問題
```
問題: FPS低下
解決: F12 → fps コマンドで監視
     PerformanceOptimizer設定確認
     F12 → gc コマンドで手動GC
```

### セーブ・ロード問題
```
問題: セーブファイルが見つからない
解決: Application.persistentDataPath の読み書き権限確認
     F12 → save → load で動作テスト
```

## 🎉 開発完了状態

**ブラックオニキス復刻版は夜間開発により完全なゲームとして実装完了しました！**

### 📊 実装完了システム（14システム）
1. ✅ ダンジョン探索システム
2. ✅ プレイヤー移動システム  
3. ✅ マップ表示システム
4. ✅ 敵システム（AI・スポーン・管理）
5. ✅ 戦闘システム（自動戦闘・レベルアップ）
6. ✅ アイテムシステム（収集・インベントリ・効果）
7. ✅ ゲームオーバーシステム
8. ✅ セーブ・ロードシステム（完全版）
9. ✅ オーディオシステム（BGM・SE）
10. ✅ UIシステム（ゲーム画面・統計・メッセージ）
11. ✅ パフォーマンス最適化
12. ✅ デバッグシステム（コンソール・コマンド）
13. ✅ メモリ管理（オブジェクトプール・GC）
14. ✅ 設定管理（表示・音量・操作）

### 🎮 プレイ可能な完全なゲーム体験
- オリジナルブラックオニキスの探索感を再現
- 現代的なUI・UX
- 安定したパフォーマンス
- 豊富なデバッグ・開発支援機能
- 完全なセーブ・ロード機能

**起床後は完成したゲームをお楽しみください！** 🌟

---

**🏰 Black Onyx Reborn - Complete Edition**  
**夜間開発バージョン 2.0.0**  
**開発完了日時**: 2025年7月29日 深夜