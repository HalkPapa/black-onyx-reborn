# 🎵 ブラックオニキス風オーディオ作成ガイド

## 🚫 重要な制約

**私（Claude）は直接音楽ファイルを生成・作成することはできません。**

しかし、以下の方法でブラックオニキス風の音楽を作成できます：

## 🎛️ 推奨ツール・方法

### 🆓 無料ツール

#### 1. **BeepBox** (ブラウザベース)
```
URL: https://beepbox.co/
特徴: 
- チップチューン音楽作成に最適
- ブラウザで直接作成・再生
- レトロゲーム風音源
- 簡単操作でメロディ作成
```

#### 2. **FamiTracker** (PC)
```
特徴:
- ファミコン音源エミュレーション
- 本格的なチップチューン作成
- MIDIエクスポート対応
- 上級者向け
```

#### 3. **Audacity** (SE作成)
```
用途: 効果音作成・編集
特徴:
- 波形生成機能
- エフェクト豊富
- 録音・編集
- 無料で高機能
```

### 💰 有料ツール

#### 1. **FL Studio** / **Ableton Live**
```
用途: 本格的な音楽制作
特徴:
- プロ用DAW
- 豊富な音源・エフェクト
- MIDI対応
```

## 🎼 実際の作成手順

### BGM作成（BeepBoxを使用）

#### 1. Title BGM作成
```
1. BeepBox (https://beepbox.co/) にアクセス
2. 以下の設定を行う:
   - Tempo: 90 BPM
   - Key: C minor
   - Scale: Minor

3. メロディ作成:
   - Channel 1: メインメロディ (Square Wave)
   - Channel 2: ベースライン (Triangle Wave)
   - Channel 3: ハーモニー (Sawtooth Wave)
   - Channel 4: ドラム (Noise)

4. パターン:
   小節1-8: 静かな導入
   小節9-16: メロディ展開
   小節17-24: クライマックス
   小節25-32: ループポイント
```

#### 2. Dungeon BGM作成
```
設定:
- Tempo: 80 BPM
- Key: A minor
- よりアンビエント的に

構成:
- 低音ドローン (長く続く低い音)
- 軽いアルペジオメロディ
- 環境音的な要素
```

### SE作成（Audacity使用）

#### 1. Button Click音
```
1. Audacityを開く
2. Generate → Tone
3. 設定:
   - Frequency: 800 Hz
   - Duration: 0.1 seconds
   - Waveform: Sine
4. Effect → Fade Out (最後0.05秒)
5. Export → WAV
```

#### 2. Attack音
```
1. Generate → Tone
2. 設定:
   - Frequency: 200 Hz (低音)
   - Duration: 0.3 seconds
   - Waveform: Sawtooth
3. Effect → Amplify → +6dB
4. Generate → Noise (0.2秒) → ミックス
5. Export → WAV
```

## 🎯 AI音楽生成ツール

### 🤖 AI作曲サービス

#### 1. **AIVA** (https://aiva.ai/)
```
特徴:
- AI作曲サービス
- ゲーム音楽に対応
- スタイル指定可能
- 有料プランあり
```

#### 2. **Boomy** (https://boomy.com/)
```
特徴:
- 簡単AI作曲
- スタイル選択
- 無料版あり
```

#### 3. **Soundraw** (https://soundraw.io/)
```
特徴:
- ゲーム音楽特化
- カスタマイズ可能
- 商用利用可能（プランによる）
```

## 📋 作成指示書（AI音楽生成用）

### BGM生成プロンプト例

#### Title BGM
```
Create a retro fantasy RPG title theme music in the style of 1980s computer game music (PC-8801 era). 
- 60 seconds duration, seamless loop
- C minor key, BPM 90
- Nostalgic and mysterious atmosphere
- Simple melody with bass and light percussion
- Inspired by classic dungeon crawler games
- Chiptune/FM synthesis style
```

#### Dungeon BGM
```
Create ambient dungeon exploration music for retro RPG:
- 90 seconds, seamless loop
- A minor key, BPM 80
- Dark, mysterious, atmospheric
- Minimal melody, focus on ambience
- Low drone + light arpeggios
- 1980s computer game style
```

### SE生成プロンプト例
```
Create retro game sound effects:
1. UI button click - 0.1s, clean beep sound
2. Footstep - 0.3s, stone floor walking
3. Sword attack - 0.5s, blade slash + impact
4. Item pickup - 0.8s, ascending chime melody
All in 8-bit/chiptune style, WAV format
```

## 🎵 フリー音源での代替案

### レトロゲーム風音源サイト
```
1. Freesound.org - チップチューン素材検索
2. Zapsplat - ゲーム音楽素材
3. OpenGameArt.org - ゲーム素材専門
4. 魔王魂 - 日本のフリー音楽素材
```

### 検索キーワード
```
- "chiptune"
- "8-bit music"
- "retro game music"
- "dungeon crawler bgm"
- "fantasy rpg music"
```

## 🔧 Unity組み込み手順

### 作成後の作業
```
1. 音楽ファイルをUnityプロジェクトにインポート
2. 適切なフォルダに配置:
   - BGM → Assets/Audio/BGM/
   - SE → Assets/Audio/SE/
3. AudioManager Inspectorで割り当て
4. インポート設定最適化
5. テスト再生
```

## 🎯 推奨アプローチ

**最も実用的な方法:**
1. **BeepBox**でチップチューン風BGM作成
2. **Audacity**で基本的なSE作成
3. 必要に応じて**AI音楽生成ツール**を活用
4. **フリー音源**で補完

この方法で、オリジナルブラックオニキスの雰囲気を再現した音楽を作成できます！

---

**🎵 音楽作成後、AUDIO_SETUP_GUIDE.mdに従ってUnityに組み込んでください**