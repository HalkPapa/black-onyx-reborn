# 🛠️ Unity Editor 手動設定ガイド

UnityのGUID参照問題により、Unity Editor内で手動設定が必要です。

## 🚀 Unity Editor での設定手順

### 1. Unity Hub でプロジェクト開く
```
Unity Hub → プロジェクトを開く → black-onyx-unity フォルダ選択
```

### 2. MainMenu.unity シーンの設定

#### シーンを開く
```
Project ウィンドウ → Assets/Scenes/MainMenu.unity をダブルクリック
```

#### Manager オブジェクト作成・設定

##### GameManager 作成
```
1. Hierarchy で右クリック → Create Empty
2. 名前を「GameManager」に変更
3. Inspector → Add Component → Scripts → Game Manager
```

##### AudioManager 作成
```
1. Hierarchy で右クリック → Create Empty
2. 名前を「AudioManager」に変更
3. Inspector → Add Component → Scripts → Audio Manager
4. Inspector → Add Component → Audio → Audio Source
```

##### SaveManager 作成
```
1. Hierarchy で右クリック → Create Empty
2. 名前を「SaveManager」に変更
3. Inspector → Add Component → Scripts → Save Manager
```

##### UIManager 作成
```
1. Hierarchy で右クリック → Create Empty
2. 名前を「UIManager」に変更
3. Inspector → Add Component → Scripts → UI Manager
```

#### 最終的なMainMenu Hierarchy
```
📋 MainMenu.unity Hierarchy
├── Main Camera
├── GameManager         ← GameManager.cs
├── AudioManager        ← AudioManager.cs + AudioSource
├── SaveManager         ← SaveManager.cs
└── UIManager           ← UIManager.cs
```

### 3. GameScene.unity シーンの設定

#### シーンを開く
```
Project ウィンドウ → Assets/Scenes/GameScene.unity をダブルクリック
```

#### Manager オブジェクト作成（MainMenuと同様）
上記と同じ手順で以下を作成：

##### 共通Manager（MainMenuと同じ）
- GameManager + GameManager.cs
- AudioManager + AudioManager.cs + AudioSource
- SaveManager + SaveManager.cs
- UIManager + UIManager.cs

##### DungeonManager（GameSceneのみ）
```
1. Hierarchy で右クリック → Create Empty
2. 名前を「DungeonManager」に変更
3. Inspector → Add Component → Scripts → Dungeon Manager
```

#### 最終的なGameScene Hierarchy
```
📋 GameScene.unity Hierarchy
├── Main Camera
├── GameManager         ← GameManager.cs
├── AudioManager        ← AudioManager.cs + AudioSource
├── SaveManager         ← SaveManager.cs
├── UIManager           ← UIManager.cs
└── DungeonManager      ← DungeonManager.cs
```

## ✅ 動作確認

### 1. コンパイルエラーチェック
```
Console ウィンドウを開いて、エラーがないことを確認
```

### 2. シーン動作テスト
```
1. MainMenu.unity または GameScene.unity が開いている状態
2. Play ボタン（▶️）をクリック
3. Console で以下のログを確認：
   🎮 Game Manager initialized
   🔊 Audio Manager initialized
   💾 Save Manager initialized
   🖥️ UI Manager initialized
   🏰 Dungeon Manager initialized (GameSceneのみ)
```

### 3. エラーが出た場合
```
1. Console でエラー内容を確認
2. 各Managerオブジェクトのスクリプトが正しくアタッチされているか確認
3. Refresh (Ctrl/Cmd + R) を実行
4. Unity Editor を再起動
```

## 🔧 Build Settings の設定

### シーンをビルドに追加
```
1. File → Build Settings
2. Add Open Scenes をクリック（現在開いているシーンを追加）
3. またはシーンファイルをドラッグ&ドロップ

推奨シーン順序：
[0] MainMenu.unity    (スタートシーン)
[1] GameScene.unity   (メインゲーム)
```

## 📝 設定確認チェックリスト

### MainMenu.unity
- [ ] GameManager オブジェクト作成済み
- [ ] GameManager.cs スクリプトアタッチ済み
- [ ] AudioManager オブジェクト作成済み
- [ ] AudioManager.cs + AudioSource アタッチ済み
- [ ] SaveManager オブジェクト作成済み
- [ ] SaveManager.cs スクリプトアタッチ済み
- [ ] UIManager オブジェクト作成済み
- [ ] UIManager.cs スクリプトアタッチ済み
- [ ] Play ボタンでエラーなく動作

### GameScene.unity
- [ ] 上記4つのManager + DungeonManager 作成済み
- [ ] DungeonManager.cs スクリプトアタッチ済み
- [ ] Play ボタンでエラーなく動作

### Build Settings
- [ ] MainMenu.unity がビルドに追加済み
- [ ] GameScene.unity がビルドに追加済み

## 🎯 完了後の状態

手動設定完了後、Unity Editor で以下が確認できます：

1. **コンパイルエラーなし** - Console にエラー表示されない
2. **Manager 初期化ログ** - Play 時に各Manager の初期化メッセージ表示
3. **シーン切り替え可能** - MainMenu ↔ GameScene 間の移動準備完了

---

**🏆 この設定完了で、Unity でゲーム開発を開始できます！**