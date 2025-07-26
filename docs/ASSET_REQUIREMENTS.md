# 🎨 ブラックオニキス復刻版 - アセット要件仕様書

## 📋 概要
ブラックオニキス復刻版の開発に必要な全アセット（画像、音声、データ）の詳細仕様と制作要件を定義します。

## 🖼️ グラフィックアセット要件

### ダンジョン描画素材
```
壁面テクスチャ:
├── stone_wall_front.png (640x400px)
├── stone_wall_left.png (320x400px)  
├── stone_wall_right.png (320x400px)
├── door_closed.png (128x400px)
├── door_open.png (128x400px)
├── hidden_door.png (128x400px)
└── stairs_down.png (128x400px)

床・天井素材:
├── stone_floor.png (640x200px)
├── stone_ceiling.png (640x200px)
├── trap_floor.png (640x200px)
└── special_floor.png (640x200px)

距離別スケール:
├── Distance 0: 100% (最前面)
├── Distance 1: 75%
├── Distance 2: 50%
├── Distance 3: 25%
└── Distance 4+: 12.5% (遠景)
```

### キャラクター素材
```
プレイヤーキャラクター:
├── player_front.png (64x96px)
├── player_back.png (64x96px)
├── player_left.png (64x96px)
├── player_right.png (64x96px)
├── player_walk_anim/ (4フレーム歩行)
└── player_damage.png (被ダメージ状態)

外観バリエーション:
├── 髪型5種類 x 髪色4色 = 20パターン
├── 服装色8色バリエーション
├── 男女2パターン
└── 総計: 320キャラクター組み合わせ

装備表示:
├── weapon_overlay/ (武器7種類)
├── armor_overlay/ (鎧5種類)
├── shield_overlay/ (盾4種類)
└── helmet_overlay/ (兜3種類)
```

### モンスター素材
```
敵キャラクター:
├── bat.png (32x32px) - コウモリ
├── kobold.png (48x64px) - コボルト
├── cobra.png (64x32px) - コブラ
├── skeleton.png (48x64px) - スケルトン
├── goblin.png (48x64px) - ゴブリン
├── aztec.png (48x64px) - アステカ
├── kraken.png (128x128px) - クラーケン
└── giant.png (96x128px) - ジャイアント

アニメーション:
├── 待機状態 (2フレーム点滅)
├── 攻撃モーション (3フレーム)
├── 被ダメージ (1フレーム点滅)
└── 死亡エフェクト (4フレーム消失)
```

### アイテム・装備素材
```
武器アイコン (32x32px):
├── knife.png - ナイフ
├── club.png - クラブ
├── mace.png - メイス
├── short_sword.png - ショートソード
├── axe.png - アクス
├── spear.png - スピア
└── broad_sword.png - ブロードソード

防具アイコン (32x32px):
├── leather_armor.png - レザーアーマー
├── chain_mail.png - チェインメイル
├── plate_armor.png - プレートアーマー
├── buckler.png - バックラー
└── chain_coif.png - チェーンコイフ

消耗品アイコン (32x32px):
├── heal_potion.png - 回復薬
├── antidote.png - 解毒薬
├── holy_water.png - 聖水
└── scroll.png - 巻物
```

### UI素材
```
メニュー・ウィンドウ:
├── window_frame.9.png (9-slice対応)
├── button_normal.9.png
├── button_hover.9.png
├── button_pressed.9.png
├── progress_bar_bg.png
├── progress_bar_fill.png
└── message_bubble.9.png

アイコン素材 (24x24px):
├── icon_attack.png
├── icon_defend.png
├── icon_run.png
├── icon_inventory.png
├── icon_status.png
├── icon_save.png
├── icon_load.png
└── icon_settings.png

ステータス表示:
├── hp_bar_bg.png (200x20px)
├── hp_bar_fill.png (200x20px)
├── exp_bar_bg.png (200x12px)
├── exp_bar_fill.png (200x12px)
└── level_frame.png (40x40px)
```

### 特殊エフェクト
```
戦闘エフェクト:
├── slash_effect.png (64x64px, 4フレーム)
├── magic_cast.png (96x96px, 6フレーム)
├── heal_effect.png (64x64px, 4フレーム)
├── damage_numbers/ (0-9数字, 各24x32px)
└── critical_star.png (32x32px)

環境エフェクト:
├── treasure_sparkle.png (32x32px, 4フレーム)
├── door_open_effect.png (3フレーム)
├── trap_activate.png (2フレーム)
└── teleport_effect.png (8フレーム)
```

## 🔊 オーディオアセット要件

### BGM（背景音楽）
```
楽曲リスト:
├── title.mp3 (2分30秒ループ) - タイトル画面
├── town.mp3 (3分ループ) - 町・安全地帯
├── dungeon_b1-b3.mp3 (4分ループ) - 序盤ダンジョン
├── dungeon_b4-b6.mp3 (4分ループ) - 深層ダンジョン
├── tower.mp3 (3分30秒ループ) - ブラックタワー
├── battle.mp3 (2分ループ) - 戦闘
├── boss_battle.mp3 (3分ループ) - ボス戦
├── victory.mp3 (30秒) - 勝利ファンファーレ
├── defeat.mp3 (15秒) - 敗北
└── ending.mp3 (5分) - エンディング

技術仕様:
├── フォーマット: MP3 (44.1kHz, 128kbps)
├── ループポイント設定済み
├── フェードイン・アウト対応
└── 総サイズ: 約50MB
```

### SE（効果音）
```
システム音:
├── menu_select.wav - メニュー選択
├── menu_confirm.wav - 決定
├── menu_cancel.wav - キャンセル
├── page_turn.wav - ページ切り替え
├── save_complete.wav - セーブ完了
└── error.wav - エラー

移動・探索音:
├── footstep.wav - 足音
├── door_open.wav - 扉開放
├── door_close.wav - 扉閉鎖
├── stairs.wav - 階段移動
├── treasure_open.wav - 宝箱開放
├── trap_activate.wav - 罠発動
└── hidden_door.wav - 隠し扉発見

戦闘音:
├── sword_attack.wav - 剣攻撃
├── axe_attack.wav - 斧攻撃
├── bow_attack.wav - 弓攻撃
├── magic_cast.wav - 魔法詠唱
├── magic_hit.wav - 魔法命中
├── hit_normal.wav - 通常ダメージ
├── hit_critical.wav - クリティカルヒット
├── heal.wav - 回復
├── level_up.wav - レベルアップ
└── death.wav - 戦闘不能

環境音:
├── wind.wav - 風音（ダンジョン環境音）
├── water_drop.wav - 水滴音
├── echo.wav - エコー効果
└── monster_roar.wav - モンスターの唸り声

技術仕様:
├── フォーマット: WAV (44.1kHz, 16bit)
├── 平均長さ: 0.5-3秒
├── 音量正規化済み
└── 総サイズ: 約20MB
```

## 📊 データアセット要件

### ゲームデータ定義
```javascript
// マップデータ (JSON)
maps/
├── B1.json (地下1階マップ)
├── B2.json (地下2階マップ)
├── B3.json (地下3階マップ)
├── B4.json (地下4階マップ)
├── B5.json (地下5階マップ)
├── B6.json (地下6階・カラー迷路)
├── TOWER1.json (ブラックタワー1階)
└── TOWER2.json (ブラックタワー2階)

// アイテムデータ (JSON)
items/
├── weapons.json (武器データベース)
├── armor.json (防具データベース)
├── consumables.json (消耗品データベース)
└── special.json (特殊アイテム)

// モンスターデータ (JSON)  
monsters/
├── enemies.json (敵ステータス)
├── encounter_tables.json (出現テーブル)
└── boss_data.json (ボス固有データ)

// ゲームバランス (JSON)
balance/
├── exp_tables.json (経験値テーブル)
├── level_stats.json (レベル別ステータス)
├── drop_rates.json (アイテムドロップ率)
└── difficulty_settings.json (難易度設定)
```

### テキストデータ
```
text/
├── ja/
│   ├── messages.json (ゲーム内メッセージ)
│   ├── items.json (アイテム名・説明)
│   ├── monsters.json (モンスター名)
│   ├── ui.json (UI文言)
│   └── story.json (ストーリーテキスト)
├── en/
│   └── (同構造の英語版)
└── fonts/
    ├── pixel_jp.woff2 (日本語ピクセルフォント)
    └── pixel_en.woff2 (英語ピクセルフォント)
```

## 🎨 アートスタイル仕様

### カラーパレット
```css
/* オリジナル版リスペクトカラー */
:root {
  /* ダンジョン色調 */
  --stone-gray: #808080;
  --stone-dark: #404040;
  --stone-light: #C0C0C0;
  
  /* UI色調 */
  --ui-bg: #000080;
  --ui-frame: #FFFF00;
  --ui-text: #FFFFFF;
  
  /* ステータス色調 */
  --hp-color: #FF0000;
  --exp-color: #00FF00;
  --level-color: #FFFF00;
  
  /* キャラクター色調 */
  --skin-tone: #FFDBAC;
  --hair-brown: #8B4513;
  --cloth-blue: #0000FF;
  --cloth-red: #FF0000;
}
```

### ピクセルアート仕様
```
解像度標準:
├── キャラクター: 32x32px, 48x64px, 64x96px
├── アイテム: 16x16px, 24x24px, 32x32px
├── UI要素: 24x24px (アイコン)
├── エフェクト: 32x32px, 64x64px
└── 背景: 640x400px基準

ピクセルアート原則:
├── アンチエイリアスなし
├── 限定カラーパレット使用
├── 1:1ピクセル比維持
├── 明確な輪郭線
└── レトロゲーム美学準拠
```

## 📱 プラットフォーム別アセット要件

### デスクトップ版
```
解像度対応:
├── 1024x768 (4:3, 最小対応)  
├── 1280x720 (16:9, HD)
├── 1920x1080 (16:9, FHD)
└── 2560x1440 (16:9, 2K)

アセット倍率:
├── @1x (基本サイズ)
├── @2x (高DPI対応)
└── @3x (4K対応, 一部)
```

### モバイル版
```
解像度対応:
├── 375x667 (iPhone SE)
├── 414x896 (iPhone 11)
├── 360x640 (Android標準)
└── 412x915 (Android大画面)

タッチ対応:
├── 最小タッチターゲット: 44x44px
├── UI要素拡大: 1.5倍
├── フォントサイズ: 16px以上
└── ジェスチャー対応素材
```

## 🔧 技術仕様

### ファイル形式
```
画像:
├── PNG (透過・UI要素)
├── WebP (圧縮・大きい画像)
├── SVG (ベクター・アイコン)
└── Sprite Atlas (パフォーマンス最適化)

音声:
├── MP3 (BGM, 広範な対応)
├── OGG (代替形式)
├── WAV (短いSE)
└── 圧縮率: BGM 128kbps, SE 192kbps
```

### パフォーマンス要件
```
ファイルサイズ制限:
├── 単一画像: 最大2MB
├── 単一音声: 最大5MB
├── 総アセットサイズ: 最大200MB
└── 初回ロード: 最大50MB

読み込み速度:
├── 起動時間: 3秒以内
├── 画面遷移: 1秒以内
├── 音声開始: 0.5秒以内
└── プリロード対応必須
```

## 📋 制作工程・品質管理

### アセット制作フロー
```
企画・仕様策定:
├── アートディレクション決定
├── スタイルガイド作成
├── アセットリスト確定
└── スケジュール策定

制作段階:
├── ラフスケッチ・コンセプト
├── 詳細デザイン・承認
├── アセット制作・実装
└── 品質チェック・修正

実装・テスト:
├── ゲーム内組み込み
├── 表示確認・動作テスト
├── パフォーマンス測定
└── 最終調整・承認
```

### 品質基準
```
画像品質:
├── 解像度・アスペクト比正確
├── 色調・コントラスト適切
├── ファイルサイズ最適化
├── 透過処理適切
└── レトロ感統一

音声品質:
├── 音量レベル統一
├── ノイズ・歪み除去
├── ループポイント正確
├── フォーマット統一
└── 圧縮品質適切

データ品質:
├── JSON構文正確
├── 文字エンコードUTF-8
├── バランス値適切
├── 関連性整合
└── バージョン管理統一
```

## 📂 ファイル構成・命名規則

### ディレクトリ構造
```
assets/
├── images/
│   ├── characters/
│   ├── monsters/
│   ├── items/
│   ├── ui/
│   ├── environments/
│   └── effects/
├── audio/
│   ├── bgm/
│   ├── se/
│   └── voice/ (将来拡張)
├── data/
│   ├── maps/
│   ├── items/
│   ├── monsters/
│   └── balance/
└── fonts/
    ├── pixel/
    └── ui/
```

### 命名規則
```
画像ファイル:
├── 小文字・アンダースコア区切り
├── [カテゴリ]_[名前]_[状態].png
├── 例: character_player_walk_01.png
└── 連番: 01, 02, 03...

音声ファイル:
├── [種類]_[名前].拡張子
├── BGM例: bgm_dungeon_b1.mp3
├── SE例: se_sword_attack.wav
└── 統一ボリューム: -12dB

データファイル:
├── [機能]_[内容].json
├── 例: map_b1_layout.json
├── items_weapons_data.json
└── UTF-8エンコード必須
```

## 🎯 優先度・スケジュール

### 開発優先度
```
Phase 1 (必須):
├── 基本キャラクター素材
├── ダンジョン描画素材
├── 基本UI素材
├── システム音響
└── 基礎データファイル

Phase 2 (重要):
├── 全モンスター素材
├── 全アイテム素材
├── 戦闘エフェクト
├── BGM楽曲
└── マップデータ完成

Phase 3 (拡張):
├── 高解像度対応
├── 追加アニメーション
├── 環境エフェクト
├── 多言語対応
└── プラットフォーム最適化
```

### 制作見積り
```
工数見積り:
├── グラフィック: 200時間
├── オーディオ: 100時間  
├── データ作成: 50時間
├── 実装・テスト: 80時間
└── 品質管理: 70時間

必要リソース:
├── ピクセルアーティスト: 1名
├── サウンドデザイナー: 1名
├── データデザイナー: 1名
└── QAテスター: 1名
```

---

**アセット要件仕様書バージョン**: 1.0  
**最終更新**: 2025年7月25日  
**承認者**: Black Onyx Reborn Development Team  
**次回更新**: アセット制作開始時