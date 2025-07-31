# 🔧 Unity UI パッケージインストールガイド

## ❌ 問題: UI メニューが表示されない

「Hierarchy で右クリック → UI → Canvas」が選択できない場合、Unity UI パッケージがインストールされていません。

## ✅ 解決方法: Unity UI パッケージのインストール

### 手順 1: Package Manager を開く
```
Unity Editor のメニューバー
Window → Package Manager
```

### 手順 2: Unity Registry に切り替え
```
Package Manager ウィンドウ左上の dropdown
「In Project」から「Unity Registry」に変更
```

### 手順 3: UI パッケージを検索・インストール
```
検索ボックスに「UI」と入力

以下のパッケージをインストール:
1. UI Toolkit (推奨・最新)
   - 名前: "UI Toolkit"
   - 説明: "Unity's new UI system"
   - 右下の「Install」ボタンをクリック

2. Legacy UI (従来のUI・互換性重視)
   - 名前: "Legacy UI" または "UI Elements"  
   - 説明: "Unity's traditional UI system"
   - 右下の「Install」ボタンをクリック
```

### 手順 4: TextMeshPro パッケージもインストール
```
検索ボックスに「TextMeshPro」と入力

見つからない場合は以下の名前で検索:
- "TextMeshPro"
- "Text Mesh Pro" 
- "TMP"
- "Unity TextMeshPro"

または、以下の代替方法:
1. 検索ボックスを空にして全パッケージを表示
2. 「T」で始まるパッケージを探す
3. 「Unity UI」または「Core 2D」パッケージを確認
```

## 🎯 インストール完了確認

### 確認方法
```
1. Package Manager で「In Project」に切り替え
2. 以下のパッケージが表示されることを確認:
   - UI Toolkit
   - Legacy UI (または UI Elements)
   - TextMeshPro

3. Unity Editor に戻る
4. Hierarchy で右クリック
5. 「UI」メニューが表示されることを確認
```

### UI メニューに表示される項目
```
UI →
├── Canvas
├── Panel  
├── Button
├── Text - TextMeshPro
├── Image
├── Raw Image
├── Slider
├── Scrollbar
├── Dropdown - TextMeshPro
├── Input Field - TextMeshPro
└── Toggle
```

## 🔄 トラブルシューティング

### パッケージが見つからない場合
```
1. Unity Hub で Unity のバージョンを確認
2. Unity 2022.3.62f1 LTS であることを確認
3. インターネット接続を確認
4. Unity Editor を再起動
```

### インストール後も UI メニューが出ない場合
```
1. Unity Editor を完全に再起動
2. プロジェクトを一度閉じて再度開く
3. Window → Package Manager で「In Project」を確認
4. 必要に応じて「Refresh」ボタンをクリック
```

## 📋 インストール後の次のステップ

### UI構築再開
パッケージインストール完了後、**UI_SETUP_GUIDE.md** の手順を実行：

```
1. MainMenu.unity を開く
2. Hierarchy で右クリック → UI → Canvas ✅
3. Canvas を「MainCanvas」に名前変更
4. Inspector で Tag を「MainUI」に設定
5. UI要素（Text, Button）を追加
```

---

**🎯 UI パッケージインストール後、完全なUI構築が可能になります**