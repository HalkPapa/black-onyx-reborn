# 🏗️ Unity Build Settings 設定ガイド

## ✅ 必要な設定

### 1. Build Settings でシーンを追加

```
Unity Editor メニューバー
File → Build Settings
```

### 2. シーンの追加

#### 追加するシーン（順番通りに）
```
1. MainMenu.unity     (Index: 0)
2. GameScene.unity    (Index: 1)
```

#### 手順
```
1. Build Settings ウィンドウで「Add Open Scenes」をクリック
2. または直接シーンファイルを Scenes In Build エリアにドラッグ&ドロップ

Assets/Scenes/MainMenu.unity  → Scene Index 0
Assets/Scenes/GameScene.unity → Scene Index 1
```

### 3. Player Settings 確認

```
Build Settings → Player Settings...

Product Name: Black Onyx Reborn
Company Name: [あなたの名前]
Version: 1.0.0
```

### 4. 動作確認

#### テスト手順
```
1. MainMenu.unity を開いて Play
2. 「ゲーム開始」ボタンをクリック
3. GameScene.unity に遷移することを確認
4. コンソールでオーディオメッセージを確認:
   - 🎵 Playing BGM: title (MainMenuで)
   - 🔊 Playing SE: button (ボタンクリック時)
   - 🎵 Playing BGM: dungeon (GameSceneで)
```

## 🔧 トラブルシューティング

### シーン遷移エラーの場合
```
1. Build Settings でシーンが正しく追加されているか確認
2. シーン名のスペルミスがないか確認
3. Scene Index が正しいか確認
```

### オーディオが再生されない場合
```
1. AudioManager が各シーンに配置されているか確認
2. オーディオファイルが Assets/Audio/BGM/ と Assets/Audio/SE/ に配置されているか確認
3. AudioManager Inspector でオーディオファイルが設定されているか確認
```

---

**🎯 Build Settings 設定完了後、完全なゲームフロー動作確認が可能になります**