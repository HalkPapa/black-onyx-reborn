# 💾 ブラックオニキス復刻版 - セーブ・データ管理システム完全ガイド

## 🎯 完全なセーブシステム実装完了！

### ✅ 実装された3層セーブシステム

1. **BlackOnyxSaveManager.cs** - メイン統合セーブマネージャー
2. **既存SaveManager.cs** - 基本ゲームデータ（互換性維持）
3. **ファイヤークリスタル・クロスゲーム専用データ** - 拡張連動機能

## 🗂️ セーブファイル構成

### 📁 保存場所
```
Application.persistentDataPath/
├── blackonyx_reborn_save.json      # メインセーブデータ
├── crystal_data.json               # ファイヤークリスタルデータ
├── crossgame_data.json            # クロスゲーム連動データ
├── Backups/                       # バックアップフォルダ
│   ├── backup_20250730_143022.json
│   ├── backup_20250730_142511.json
│   └── ... (最大5ファイル)
└── fire_crystal_data.json         # 他ゲーム連動用データ
```

### 🎮 各プラットフォーム保存場所

#### Windows
```
C:\Users\[ユーザー名]\AppData\LocalLow\[会社名]\BlackOnyxReborn\
```

#### Mac
```
~/Library/Application Support/[会社名]/BlackOnyxReborn/
```

#### Android
```
/storage/emulated/0/Android/data/[パッケージ名]/files/
```

#### iOS
```
/var/mobile/Containers/Data/Application/[GUID]/Documents/
```

## 💾 セーブデータ内容詳細

### 1. メインセーブデータ（blackonyx_reborn_save.json）

```json
{
  "playerName": "プレイヤー",
  "playerLevel": 5,
  "playerHP": 120,
  "playerMaxHP": 150,
  "playerAttack": 25,
  "playerDefense": 15,
  "playerExp": 250,
  "playerGold": 500,
  
  "currentFloor": -3,
  "playerPosition": {"x": 5, "y": 7},
  "currentEntrance": 2,
  "exploredFloors": [-6, -5, -4, -3, -2, -1],
  "colorMazeProgress": 3,
  
  "normalEndingAchieved": false,
  "trueEndingAchieved": false,
  "ultimateEndingAchieved": false,
  "hasBlackOnyx": false,
  "hasFireCrystal": true,
  "hasUltimateFusion": false,
  
  "playTime": 3600.5,
  "saveDateTime": "2025/07/30 14:30:22",
  "saveVersion": "2.0.0"
}
```

### 2. ファイヤークリスタルデータ（crystal_data.json）

```json
{
  "crystalStatus": {
    "hasCrystal": true,
    "level": 6,
    "power": 300,
    "energy": 85.5,
    "unlockedAbilities": 4,
    "discoveredCrystals": 12
  },
  "discoveredCrystals": [
    "ファイヤークリスタルの欠片",
    "虹色ファイヤークリスタル"
  ],
  "unlockedAbilities": [
    "炎の剣",
    "炎の盾", 
    "クリスタルの眼",
    "炎瞬移"
  ],
  "abilityUsageStats": {
    "炎の剣": 15,
    "炎の盾": 8,
    "クリスタルの眼": 3
  }
}
```

### 3. クロスゲーム連動データ（crossgame_data.json）

```json
{
  "crossGameStatus": {
    "isIntegrationEnabled": true,
    "hasFireCrystal": true,
    "unlockedContentCount": 3,
    "totalRewards": 2,
    "connectedGames": [
      "ASCII Treasure Guardian",
      "Oracle Query Composer"
    ]
  },
  "achievedRewards": [
    "古代文字の知識",
    "オラクルの叡智"
  ],
  "exportedData": [
    "FireCrystal_20250730"
  ],
  "connectionHistory": [
    "2025-07-30: ASCII Treasure Guardian connected",
    "2025-07-30: Oracle Query Composer connected"
  ]
}
```

## 🎮 セーブシステム使用方法

### 基本的なセーブ・ロード

```csharp
// BlackOnyxSaveManagerを取得
var saveManager = GameManager.Instance.GetComponent<BlackOnyxSaveManager>();

// 完全セーブ（全データ保存）
bool saved = saveManager.SaveGame();

// 完全ロード（全データ読み込み）
bool loaded = saveManager.LoadGame();

// 設定のみ保存・読み込み
saveManager.SaveSettings();
saveManager.LoadSettings();
```

### 自動セーブ機能

```csharp
// 自動セーブ有効化（デフォルト：ON）
saveManager.autoSaveEnabled = true;
saveManager.autoSaveInterval = 300f; // 5分間隔

// 手動自動セーブ実行
saveManager.AutoSave();
```

### バックアップ機能

```csharp
// バックアップ有効化（デフォルト：ON）
saveManager.useBackupSave = true;
saveManager.maxBackupFiles = 5; // 最大5ファイル保持
```

## ⚙️ 設定管理システム

### ゲーム設定項目

```csharp
public class GameSettings
{
    // 音声設定
    public float masterVolume = 1.0f;
    public float bgmVolume = 0.7f;
    public float seVolume = 0.8f;
    
    // セーブ設定
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 300f;
    
    // 表示設定
    public bool showFPS = false;
    public bool fullscreen = false;
    
    // ゲームプレイ設定
    public float textSpeed = 1.0f;
    public bool skipAnimations = false;
    public bool showTutorials = true;
}
```

### 設定の保存場所

- **ゲーム設定**: PlayerPrefs（レジストリ/plist）
- **セーブデータ**: JSONファイル（永続データパス）
- **一時データ**: メモリ内（セッション限定）

## 🔐 データ保護・セキュリティ

### バックアップシステム

```csharp
// バックアップファイル命名規則
backup_20250730_143022.json  // backup_YYYYMMDD_HHMMSS.json

// 自動バックアップタイミング
- セーブ実行前
- 重要なイベント前（ボス戦、エンディングなど）
- 定期的な自動バックアップ
```

### データ整合性チェック

```csharp
// セーブデータ検証
- JSONフォーマット検証
- バージョン互換性チェック
- 必須フィールド存在確認
- 値範囲チェック（レベル、HP等）
```

## 🚀 セーブシステムの使い分け

### 1. **BlackOnyxSaveManager** - 推奨（新システム）
```csharp
// 完全機能版
- ファイヤークリスタル連動
- クロスゲーム機能
- 自動バックアップ
- 複数ファイル管理
```

### 2. **SaveManager** - 互換性維持（既存システム）
```csharp
// 基本機能版
- 基本的なゲームデータのみ
- 既存コードとの互換性
- シンプルな1ファイル管理
```

## 🎯 セーブタイミング推奨

### 自動セーブタイミング
```
✅ フロア移動時
✅ 戦闘終了時
✅ アイテム取得時
✅ レベルアップ時
✅ 重要イベント発生時
✅ ゲーム終了時
✅ 一定時間間隔（5分）
```

### 手動セーブタイミング
```
🎮 プレイヤーがメニューからセーブ選択時
🎮 チェックポイント到達時
🎮 困難な場面の前
🎮 長時間プレイ前の保存
```

## 🔧 デバッグ・トラブルシューティング

### F12デバッグコンソールコマンド

```
save              # 手動セーブ実行
load              # 手動ロード実行
savestats         # セーブ統計表示
deletesave        # セーブファイル削除
backupinfo        # バックアップ情報表示
savemanager debug # セーブマネージャー状態表示
```

### 一般的な問題と解決方法

#### セーブファイルが見つからない
```
原因: ファイルパスの問題、権限不足
解決: パス確認、書き込み権限確認、新規ゲーム初期化
```

#### データが読み込まれない
```
原因: JSONフォーマット破損、バージョン不一致
解決: バックアップから復元、フォーマット修復
```

#### 自動セーブが動作しない
```
原因: 設定無効、エラー発生
解決: 設定確認、手動セーブテスト、ログ確認
```

## 📊 セーブデータ統計・分析

### 統計データ取得
```csharp
var stats = saveManager.GetSaveStatistics();

// 取得可能な統計
- プレイ時間
- プレイヤーレベル
- 現在フロア
- 探索済みフロア数
- ファイヤークリスタル所持状況
- ブラックオニキス所持状況
- 究極合成達成状況
- エンディング達成数
```

## 🌟 システムの特徴

### ✅ **完全性**
- 全ゲームデータの完全保存・復元
- ファイヤークリスタル・クロスゲーム連動対応
- バックアップによるデータ保護

### ✅ **利便性**
- 自動セーブによる手間軽減
- 複数セーブスロット対応準備
- 設定とゲームデータの分離管理

### ✅ **安全性**
- 自動バックアップシステム
- データ整合性チェック
- エラー時の復旧機能

### ✅ **拡張性**
- 新機能追加に対応する柔軟な構造
- バージョン管理による互換性維持
- モジュール式設計

**史上最高レベルのセーブ・データ管理システムが完成！** 🎉

プレイヤーの貴重なゲーム進捗を完全に保護し、ファイヤークリスタル連動やクロスゲーム機能まで含めた、真のゲーム愛好家のための最高のセーブシステムです！

---

**💾 Black Onyx Save System**  
**システムバージョン**: 2.0.0  
**完成日**: 2025年7月30日  
**対応機能**: 完全セーブ、ファイヤークリスタル連動、クロスゲーム統合