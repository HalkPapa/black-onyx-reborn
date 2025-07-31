# 🏗️ Unity Hierarchy 設定ガイド

## 📋 MainMenu.unity の設定

### 1. MainMenu シーンを開く
```
Assets/Scenes/MainMenu.unity をダブルクリック
```

### 2. GameManager オブジェクト作成
```
Hierarchy右クリック → Create Empty
名前を「GameManager」に変更
```

### 3. GameManager スクリプトをアタッチ
```
GameManager オブジェクト選択
Inspector → Add Component → Scripts → Game Manager
```

### 4. 他のManagerオブジェクト作成
同様の手順で以下を作成：

#### AudioManager
```
Create Empty → 名前「AudioManager」
Add Component → Audio Manager
Add Component → Audio Source（音声再生用）
```

#### SaveManager
```
Create Empty → 名前「SaveManager」
Add Component → Save Manager
```

#### UIManager
```
Create Empty → 名前「UIManager」
Add Component → UI Manager
```

## 📋 GameScene.unity の設定

### 1. GameScene シーンを開く
```
Assets/Scenes/GameScene.unity をダブルクリック
```

### 2. 全Managerオブジェクト作成
MainMenuと同じ手順で以下を作成：

#### GameManager
```
Create Empty → 名前「GameManager」
Add Component → Game Manager
```

#### AudioManager
```
Create Empty → 名前「AudioManager」
Add Component → Audio Manager
Add Component → Audio Source
```

#### SaveManager
```
Create Empty → 名前「SaveManager」
Add Component → Save Manager
```

#### UIManager
```
Create Empty → 名前「UIManager」
Add Component → UI Manager
```

#### DungeonManager（ゲームシーンのみ）
```
Create Empty → 名前「DungeonManager」
Add Component → Dungeon Manager
```

## 🎯 最終的なHierarchy構成

### MainMenu.unity
```
📋 Hierarchy
├── Main Camera          (既存)
├── GameManager          (新規作成)
├── AudioManager         (新規作成)
├── SaveManager          (新規作成)
└── UIManager            (新規作成)
```

### GameScene.unity
```
📋 Hierarchy
├── Main Camera          (既存)
├── GameManager          (新規作成)
├── AudioManager         (新規作成)
├── SaveManager          (新規作成)
├── UIManager            (新規作成)
└── DungeonManager       (新規作成)
```

## ⚙️ Inspector での設定確認

### GameManager
- **Game State**: Loading (初期値)
- **Target Frame Rate**: 60
- **Time Scale**: 1

### AudioManager
- **Master Volume**: 1
- **BGM Volume**: 0.7
- **SE Volume**: 0.8
- **Audio Source コンポーネント**も追加されていることを確認

### その他のManager
- 特別な設定は不要（デフォルト値でOK）

## 🚀 動作テスト方法

### 1. MainMenu シーンテスト
```
1. MainMenu.unity を開く
2. Play ボタンをクリック
3. Console で「🎮 Game Manager initialized」等のログ確認
4. エラーが出ないことを確認
```

### 2. GameScene シーンテスト
```
1. GameScene.unity を開く
2. Play ボタンをクリック
3. Console で「🏰 Dungeon Manager initialized」等のログ確認
4. 全Managerが正常に動作することを確認
```

## 🔧 トラブルシューティング

### スクリプトが見つからない場合
1. **Assets/Scripts/Managers/** フォルダを確認
2. **Refresh** (Ctrl/Cmd + R) を実行
3. Unity Editor を再起動

### Managerが動作しない場合
1. **Inspector** でスクリプトが正しくアタッチされているか確認
2. **Console** でエラーメッセージを確認
3. 各Managerオブジェクトが **Active** になっているか確認

## 📝 次のステップ

### Phase 3: UI要素追加
1. **Canvas** オブジェクト追加
2. **Button**, **Text** 等のUI要素作成
3. **UIManager** との連携設定

### Phase 4: Build Settings
1. **File > Build Settings**
2. **MainMenu** と **GameScene** をビルドに追加
3. **Platform Settings** 確認

---

**🎯 目標**: Play ボタンでエラーなく動作し、全Managerが正常に初期化される状態