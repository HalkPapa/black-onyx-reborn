# 🎵 Audio Assets

## 📁 フォルダ構造

```
Assets/Audio/
├── BGM/           # 背景音楽ファイル
│   ├── title.mp3      # タイトル画面BGM
│   ├── dungeon.mp3    # ダンジョンBGM
│   └── battle.mp3     # 戦闘BGM
└── SE/            # 効果音ファイル
    ├── button.wav     # ボタンクリック音
    ├── walk.wav       # 歩行音
    ├── attack.wav     # 攻撃音
    └── item.wav       # アイテム取得音
```

## 🎼 推奨オーディオ設定

### BGM設定
- **形式**: MP3 または OGG
- **品質**: 128-192 kbps
- **ループ**: Yes
- **3D Sound**: No
- **Load Type**: Streaming

### SE設定  
- **形式**: WAV または OGG
- **品質**: 44.1kHz, 16bit
- **ループ**: No
- **3D Sound**: No  
- **Load Type**: Decompress On Load

## 🔧 AudioManager への設定

### Unity Editor での設定手順
```
1. AudioManager オブジェクトを選択
2. Inspector で Audio Manager コンポーネントを確認
3. 以下のフィールドに対応するオーディオファイルを drag & drop:
   - Title BGM: title.mp3
   - Dungeon BGM: dungeon.mp3  
   - Battle BGM: battle.mp3
   - SE Clips: button.wav, walk.wav, attack.wav, item.wav
```

## 🎮 使用方法

### C# コードでの音声再生
```csharp
// BGM再生
AudioManager.Instance.PlayBGM("title");
AudioManager.Instance.PlayBGM("dungeon");

// SE再生  
AudioManager.Instance.PlaySE("button");
AudioManager.Instance.PlaySE("walk");

// フェード付きBGM切り替え
AudioManager.Instance.FadeToBGM("battle", 2.0f);
```

---

**オーディオファイルを上記フォルダに配置後、Unity Editor で AudioManager に設定してください**