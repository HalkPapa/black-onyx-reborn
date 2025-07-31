# 🏰 ダンジョン探索システム セットアップガイド

## 📋 実装完了システム

### ✅ 完成済みコンポーネント
1. **DungeonManager.cs** - ダンジョン生成・プレイヤー移動管理
2. **PlayerController.cs** - キー入力とプレイヤー移動制御
3. **DungeonMapRenderer.cs** - テキストベースマップ描画
4. **GameUIController.cs** - ゲーム画面UI統合制御

## 🎮 GameScene セットアップ手順

### 1. GameSceneの作成・設定

#### シーン作成
```
1. File → New Scene
2. シーン名: "GameScene"
3. File → Save Scene As → GameScene.unity
4. Assets/Scenes/ フォルダに保存
```

#### GameManagerオブジェクト配置
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "GameManager" に変更
3. Inspector で Add Component:
   - GameManager
   - AudioManager  
   - DungeonManager
   - SaveManager
```

### 2. ゲームUI構築

#### GameUIControllerセットアップ
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "GameUIController" に変更
3. Add Component → GameUIController
4. Auto Layout を有効にして自動レイアウト使用
```

#### PlayerControllerセットアップ
```
1. Hierarchy で右クリック → Create Empty
2. 名前を "PlayerController" に変更  
3. Add Component → PlayerController
4. Inspector で移動設定を調整:
   - Move Delay: 0.2s (キー連打防止)
   - Allow Diagonal Movement: false (4方向移動)
   - Play Walk Sound: true
```

## 🎛️ コンポーネント設定詳細

### DungeonManager 設定
```
Dungeon Settings:
- Dungeon Width: 16
- Dungeon Height: 16  
- Max Floors: 10

Generation Settings:
- Room Density: 0.3
- Min Room Size: 3
- Max Room Size: 8
- Corridor Width: 1
```

### PlayerController 設定
```
Movement Settings:
- Move Delay: 0.2s
- Allow Diagonal Movement: false
- Key Repeat Delay: 0.5s
- Key Repeat Rate: 0.1s

Audio:
- Play Walk Sound: true
- Play Wall Bump Sound: true
```

### DungeonMapRenderer 設定
```
Display Settings:
- View Radius: 5 (プレイヤー周囲5マス表示)
- Show Full Map: false (通常は視界限定)
- Enable Fog of War: true (未探索エリア非表示)

Visual Settings:
- Wall Char: "██"
- Floor Char: "  " 
- Player Char: "🚶"
- Entrance Char: "🚪"
- Stairs Up Char: "⬆️"
- Stairs Down Char: "⬇️"
```

## 🎮 操作方法

### 基本移動
```
矢印キー     : プレイヤー移動
WASD        : プレイヤー移動（代替）
テンキー     : ローグライク風移動
ESC         : ポーズメニュー
```

### デバッグキー
```
F1          : デバッグ情報表示切替
M           : フルマップ表示切替（将来実装予定）
```

## 🔊 オーディオ連携

### 必要な効果音
```
SE名        用途
---------------------
walk        歩行音
bump        壁衝突音（将来実装）
button      ボタンクリック音
```

### BGM自動切り替え
```
title       → タイトル画面
dungeon     → ダンジョン探索時
battle      → 戦闘時（将来実装）
```

## 🎯 テスト手順

### 1. 基本動作確認
```
1. Play ボタンでGameScene実行
2. Console で初期化メッセージを確認:
   ✅ 🎮 Black Onyx Reborn - Game Manager Initialized
   ✅ 🏰 Dungeon Manager initialized - Floor 1
   ✅ 🎮 Player Controller initialized
   ✅ 🗺️ Dungeon Map Renderer initialized
   ✅ 🎮 Game UI Controller initialized
```

### 2. 移動テスト
```
1. 矢印キーでプレイヤー移動
2. Console で移動ログを確認:
   ✅ 🚶 Player moved: (0, 1)
3. マップ表示でプレイヤー位置（🚶）が更新されることを確認
4. 壁に向かって移動し、移動がブロックされることを確認:
   ✅ 🚫 Movement blocked: (1, 0)
```

### 3. UI表示確認
```
1. 左側にテキストマップが表示される
2. 右上にステータス情報が表示される:
   - Floor: 1
   - Position: (x, y)  
   - HP: 100/100
   - Items: 0/10
3. 右下にメッセージログが表示される
4. Menu・Mapボタンが機能する
```

### 4. フロア移動テスト
```
1. マップ上の階段（⬇️）を見つける
2. 階段に移動してフロア変更を確認:
   ✅ Floor 2 に移動しました
3. ステータス表示のFloor番号が更新される
```

## 🛠️ カスタマイズ設定

### マップ表示カスタマイズ
```csharp
// DungeonMapRenderer で文字変更
wallChar = "##";        // 壁
floorChar = "..";       // 床
playerChar = "@";       // プレイヤー
```

### 移動速度調整
```csharp
// PlayerController で速度変更
moveDelay = 0.1f;       // 高速移動
moveDelay = 0.5f;       // 低速移動
```

### ダンジョンサイズ変更
```csharp
// DungeonManager で大きさ変更
dungeonWidth = 32;      // より大きなダンジョン
dungeonHeight = 32;
maxFloors = 20;         // より多くのフロア
```

## 🔧 トラブルシューティング

### コンソールエラーが出る場合
```
問題: NullReferenceException
解決: GameManagerが正しく初期化されているか確認
　　　シーン実行前にGameManagerオブジェクトが存在するか確認
```

### マップが表示されない場合
```
問題: マップパネルが空白
解決: GameUIController の Auto Layout が有効になっているか確認
　　　DungeonMapRenderer が正しくアタッチされているか確認
```

### プレイヤーが移動しない場合
```
問題: キー入力が反応しない
解決: PlayerController がアクティブになっているか確認
　　　GameManager の CurrentState が InGame になっているか確認
```

### オーディオが再生されない場合
```
問題: SE・BGMが鳴らない
解決: AUDIO_SETUP_GUIDE.md を参照してオーディオファイルを設定
　　　AudioManager が正しく初期化されているか確認
```

## 🎯 完成状態での体験

### プレイヤー体験
- **探索感**: テキストマップでダンジョンを探索
- **進歩感**: フロア移動で深層を目指す
- **情報管理**: リアルタイムステータス・メッセージログ
- **操作感**: レスポンシブなキー入力・移動制御

### 技術的達成
- **統合設計**: 各システムが連携して動作
- **拡張性**: 追加機能（戦闘・アイテム）の基盤完成
- **安定性**: エラーハンドリング・null安全性確保
- **ユーザビリティ**: 直感的なUI・操作系

---

**🏰 ブラックオニキス風ダンジョン探索システム実装完了！**

**次のステップ**: 戦闘システム・アイテム収集・敵AI など のゲームプレイ要素追加