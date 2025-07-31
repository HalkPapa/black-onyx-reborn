# 🎵 オーディオファイル設定ガイド

## 🔊 現在の状況

コンソールに以下のメッセージが表示されている場合、オーディオファイルが設定されていません：
```
🎵 BGM 'title' not available - Add audio files to Assets/Audio/BGM/
🔊 SE 'button' not available - Add audio files to Assets/Audio/SE/
```

**これは正常な動作です。** ゲームはオーディオなしでも正常に動作します。

## 📁 必要なオーディオファイル

### BGM (背景音楽)
以下のファイルを `Assets/Audio/BGM/` に配置：
```
- title.mp3      (タイトル画面BGM)
- dungeon.mp3    (ダンジョンBGM)
- battle.mp3     (戦闘BGM)
```

### SE (効果音)
以下のファイルを `Assets/Audio/SE/` に配置：
```
- button.wav     (ボタンクリック音)
- walk.wav       (歩行音)
- attack.wav     (攻撃音)  
- item.wav       (アイテム取得音)
```

## 🎼 推奨オーディオ形式

### BGM設定
- **形式**: MP3, OGG, WAV
- **品質**: 128-192 kbps (MP3)
- **長さ**: 30秒～2分 (ループ想定)
- **Unity設定**: Load Type = Streaming

### SE設定
- **形式**: WAV, OGG
- **品質**: 44.1kHz, 16bit
- **長さ**: 0.1～3秒
- **Unity設定**: Load Type = Decompress On Load

## 🔧 Unity Editor での設定手順

### 1. オーディオファイルの配置
```
1. オーディオファイルをUnity Projectビューにドラッグ&ドロップ
2. 適切なフォルダ (Assets/Audio/BGM/ または Assets/Audio/SE/) に移動
3. ファイルを選択してImport Settings を確認
```

### 2. AudioManager Inspector 設定
```
1. Hierarchy で AudioManager オブジェクトを選択
2. Inspector の Audio Manager コンポーネントを確認
3. Audio Clips セクションで以下を設定:
   - Title BGM: title.mp3 をドラッグ&ドロップ
   - Dungeon BGM: dungeon.mp3 をドラッグ&ドロップ
   - Battle BGM: battle.mp3 をドラッグ&ドロップ
   - SE Clips: button.wav, walk.wav, attack.wav, item.wav を配列に追加
```

### 3. Import Settings 最適化
```
BGM ファイル選択時:
- Load Type: Streaming
- Compression Format: Vorbis (OGG) または MP3
- Quality: 70-100%

SE ファイル選択時:
- Load Type: Decompress On Load
- Compression Format: PCM または Vorbis
- Quality: 100%
```

## 🎮 テスト手順

### オーディオ設定後の確認
```
1. MainMenu.unity で Play
2. コンソールで以下が表示されることを確認:
   ✅ 🎵 Playing BGM: title
   
3. 「ゲーム開始」ボタンクリック時:
   ✅ 🔊 Playing SE: button
   
4. GameScene 遷移時:
   ✅ 🎵 Playing BGM: dungeon
```

## 🆓 フリー音源サイト (参考)

### BGM
- **フリーBGM DOVA-SYNDROME**: https://dova-s.jp/
- **魔王魂**: https://maou.audio/
- **煉獄庭園**: https://www.rengoku-teien.com/

### 効果音
- **効果音ラボ**: https://soundeffect-lab.info/
- **フリー効果音**: http://taira-komori.jpn.org/
- **SOUNDJAY**: https://www.soundjay.com/

## ⚠️ 重要な注意事項

### ライセンスについて
- **商用利用**: ライセンスを必ず確認
- **クレジット表記**: 必要な場合は適切に表記
- **配布制限**: 再配布の可否を確認

### ファイルサイズ
- **BGM**: 1ファイル 3MB以下推奨
- **SE**: 1ファイル 500KB以下推奨
- **総容量**: 音声ファイル全体で50MB以下推奨

## 🎯 オーディオなしでのテスト

現在の状態でも完全にゲームテストが可能です：
- UI操作・シーン遷移は正常動作
- オーディオメッセージは情報提供のみ
- ゲームプレイに影響なし

---

**🎵 オーディオファイル追加でより没入感のあるゲーム体験が可能になります！**