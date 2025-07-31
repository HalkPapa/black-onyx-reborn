# 📝 TextMeshPro が見つからない場合の対処法

## 🔍 TextMeshPro 検索方法

### 1. 異なる検索キーワードを試す
```
Package Manager の検索ボックスで以下を試す:
- TextMeshPro
- Text Mesh Pro
- TMP
- Text
- Unity TextMeshPro
- com.unity.textmeshpro
```

### 2. パッケージ一覧から探す
```
1. Package Manager で検索ボックスを空にする
2. 全パッケージリストを表示
3. アルファベット順に「T」の部分を探す
4. "TextMeshPro" または "Text Mesh Pro" を探す
```

### 3. Unity バージョン確認
```
Unity 2022.3.62f1 では TextMeshPro が標準で含まれている場合があります
Window → TextMeshPro メニューがあるか確認
```

## 🔄 代替手順: TextMeshPro なしでUI構築

TextMeshPro が見つからない場合、従来の Text コンポーネントを使用できます。

### UI構築手順（TextMeshPro なし版）

#### MainMenu.unity での作業
```
1. Hierarchy で右クリック → UI → Canvas
2. Canvas 名前を「MainCanvas」に変更
3. Canvas の Tag を「MainUI」に設定

4. MainCanvas 右クリック → UI → Text (Legacy)
   - 名前: TitleText
   - Text: "Black Onyx Reborn"
   - Position: (0, 100, 0)
   - Font Size: 48

5. MainCanvas 右クリック → UI → Button (Legacy)
   - 名前: StartButton
   - Position: (0, -50, 0)
   - Button 内の Text: "ゲーム開始"

6. MainCanvas 右クリック → UI → Button (Legacy)
   - 名前: ExitButton
   - Position: (0, -120, 0)
   - Button 内の Text: "終了"
```

#### GameScene.unity での作業
```
1. Hierarchy で右クリック → UI → Canvas
2. Canvas 名前を「HUDCanvas」に変更
3. Canvas の Tag を「HUD」に設定

4. HUDCanvas 右クリック → UI → Text (Legacy)
   - 名前: StatusText
   - Text: "Floor: 1 | HP: 100/100"
   - Anchor: Top Left
   - Position: (10, -10, 0)

5. HUDCanvas 右クリック → UI → Panel
   - 名前: MessagePanel
   - Anchor: Bottom Center
   - Position: (0, 50, 0)

6. MessagePanel 右クリック → UI → Text (Legacy)
   - 名前: MessageText
   - Text: ""
   - Text Alignment: Center
```

## 🎯 TextMeshPro 手動インポート方法

### Window メニューから
```
1. Unity Editor メニューバー
2. Window → TextMeshPro → Import TMP Essential Resources
3. 「Import」ボタンをクリック
4. 追加で Window → TextMeshPro → Import TMP Examples & Extras
```

### Assets メニューから
```
1. Project ウィンドウで右クリック
2. Import Package → Custom Package
3. Unity インストールフォルダの TextMeshPro パッケージファイルを選択
```

## ✅ 動作確認

### TextMeshPro インストール成功の確認
```
1. Hierarchy で右クリック → UI
2. 以下が表示されることを確認:
   - Text - TextMeshPro
   - Button - TextMeshPro
   - Dropdown - TextMeshPro
   - Input Field - TextMeshPro
```

### Legacy UI での動作確認
```
1. Hierarchy で右クリック → UI
2. 以下が表示されることを確認:
   - Text (Legacy)
   - Button (Legacy)
   - Dropdown (Legacy)
   - Input Field (Legacy)
```

---

**🎯 TextMeshPro が見つからない場合も、Legacy UI で完全なUI構築が可能です**