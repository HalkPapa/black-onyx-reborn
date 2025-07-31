# 🎨 UI システム構築ガイド

## ✅ 完了済み設定

### Unity タグ追加完了
- `MainUI` - メインUI用
- `HUD` - ゲーム中HUD用  
- `Menu` - メニュー画面用

## 🎮 次の手順: Unity Editor でのUI構築

### 1. MainMenu.unity のUI構築

#### Canvas 作成
```
MainMenu.unity を開く
Hierarchy で右クリック → UI → Canvas
Canvas の名前を「MainCanvas」に変更
Inspector で Canvas の Tag を「MainUI」に設定
```

#### タイトル画面UI要素作成
```
MainCanvas を右クリック → UI → Text - TextMeshPro
名前を「TitleText」に変更
Text 内容: "Black Onyx Reborn"
Position: (0, 100, 0)
Font Size: 48
Color: White

MainCanvas を右クリック → UI → Button - TextMeshPro  
名前を「StartButton」に変更
Position: (0, -50, 0)
Button Text: "ゲーム開始"

MainCanvas を右クリック → UI → Button - TextMeshPro
名前を「ExitButton」に変更
Position: (0, -120, 0)
Button Text: "終了"
```

### 2. GameScene.unity のUI構築

#### HUD Canvas 作成
```
GameScene.unity を開く
Hierarchy で右クリック → UI → Canvas
Canvas の名前を「HUDCanvas」に変更
Canvas の Tag を「HUD」に設定
Canvas の Render Mode を「Screen Space - Overlay」に設定
Canvas の Sort Order を「10」に設定
```

#### ゲームHUD要素作成
```
HUDCanvas を右クリック → UI → Text - TextMeshPro
名前を「StatusText」に変更
Anchor: Top Left
Position: (10, -10, 0)
Text 内容: "Floor: 1 | HP: 100/100"
Font Size: 16
Color: White

HUDCanvas を右クリック → UI → Panel
名前を「MessagePanel」に変更
Anchor: Bottom Center
Width: 600, Height: 100
Position: (0, 50, 0)
Background Color: (0, 0, 0, 0.7)

MessagePanel を右クリック → UI → Text - TextMeshPro
名前を「MessageText」に変更
Anchor: Stretch All
Text 内容: ""
Font Size: 14
Color: White
Text Alignment: Center
```

### 3. UIManager との連携設定

#### MainMenu UIManager 設定
```
MainMenu.unity で UIManager オブジェクトを選択
Inspector で以下を設定:
- Main Canvas: MainCanvas を drag & drop
- Title Screen: TitleText の親Panel（作成が必要）
- Message Panel: 後で作成
```

#### GameScene UIManager 設定
```
GameScene.unity で UIManager オブジェクトを選択
Inspector で以下を設定:
- HUD Canvas: HUDCanvas を drag & drop
- Message Panel: MessagePanel を drag & drop
- Message Text: MessageText を drag & drop
```

## 🔧 ボタンイベント設定

### StartButton イベント設定
```
StartButton を選択
Inspector の Button コンポーネント
On Click() に新しいイベント追加
Object: GameManager を drag & drop
Function: GameManager.NewGame を選択
```

### ExitButton イベント設定
```
ExitButton を選択
Inspector の Button コンポーネント
On Click() に新しいイベント追加
Object: GameManager を drag & drop
Function: GameManager.QuitGame を選択
```

## 🎯 完成後の確認事項

### MainMenu.unity
- [ ] MainCanvas が MainUI タグ設定済み
- [ ] TitleText が表示される
- [ ] StartButton, ExitButton が動作する
- [ ] UIManager に Canvas が設定済み

### GameScene.unity  
- [ ] HUDCanvas が HUD タグ設定済み
- [ ] StatusText が表示される
- [ ] MessagePanel が正常に表示/非表示
- [ ] UIManager に UI要素が設定済み

## 🚀 動作テスト

```
1. MainMenu.unity を開いて Play
2. タイトルとボタンが表示されることを確認
3. GameScene.unity を開いて Play
4. HUD要素が表示されることを確認
5. Console で UI Manager initialized ログ確認
```

---

**次の段階**: Audio システム構築（BGM・SE ファイル追加）