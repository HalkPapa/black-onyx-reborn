/**
 * 🎮 Black Onyx Reborn - Main Entry Point
 * ブラックオニキス復刻版 メインエントリーポイント
 */

import { GameEngine } from './core/GameEngine.js';
import { ScreenManager } from './ui/ScreenManager.js';
import { AudioManager } from './audio/AudioManager.js';
import { SaveManager } from './systems/SaveManager.js';
import { Logger } from './utils/Logger.js';

class BlackOnyxReborn {
    constructor() {
        this.logger = new Logger('BlackOnyxReborn');
        this.gameEngine = null;
        this.screenManager = null;
        this.audioManager = null;
        this.saveManager = null;
        
        this.isInitialized = false;
        this.loadingProgress = 0;
        
        this.logger.info('🎮 Black Onyx Reborn starting...');
    }

    /**
     * アプリケーション初期化
     */
    async init() {
        try {
            this.logger.info('📋 Initializing application...');
            
            // ローディング画面表示
            this.showLoadingScreen();
            
            // コアシステム初期化
            await this.initializeCoreComponents();
            
            // UI初期化
            await this.initializeUI();
            
            // オーディオ初期化
            await this.initializeAudio();
            
            // セーブシステム初期化
            await this.initializeSaveSystem();
            
            // イベントリスナー設定
            this.setupEventListeners();
            
            // 初期化完了
            this.isInitialized = true;
            this.logger.info('✅ Application initialized successfully');
            
            // タイトル画面に遷移
            setTimeout(() => {
                this.showTitleScreen();
            }, 1000);
            
        } catch (error) {
            this.logger.error('❌ Failed to initialize application:', error);
            this.showErrorScreen(error);
        }
    }

    /**
     * ローディング画面表示
     */
    showLoadingScreen() {
        const loadingScreen = document.getElementById('loading-screen');
        loadingScreen.classList.add('active');
        this.updateLoadingProgress(0, 'システムを初期化中...');
    }

    /**
     * ローディング進捗更新
     */
    updateLoadingProgress(progress, message) {
        this.loadingProgress = Math.max(this.loadingProgress, progress);
        
        const progressBar = document.getElementById('loading-progress');
        const loadingText = document.getElementById('loading-text');
        
        if (progressBar) {
            progressBar.style.width = `${this.loadingProgress}%`;
        }
        
        if (loadingText && message) {
            loadingText.textContent = message;
        }
        
        this.logger.info(`📊 Loading progress: ${this.loadingProgress}% - ${message}`);
    }

    /**
     * コアコンポーネント初期化
     */
    async initializeCoreComponents() {
        this.updateLoadingProgress(20, 'ゲームエンジンを読み込み中...');
        
        // ゲームエンジン初期化
        const canvas = document.getElementById('game-canvas');
        this.gameEngine = new GameEngine(canvas);
        await this.gameEngine.init();
        
        this.updateLoadingProgress(40, 'ゲームシステムを準備中...');
    }

    /**
     * UI初期化
     */
    async initializeUI() {
        this.updateLoadingProgress(60, 'ユーザーインターフェースを構築中...');
        
        // スクリーンマネージャー初期化
        this.screenManager = new ScreenManager();
        await this.screenManager.init();
    }

    /**
     * オーディオシステム初期化
     */
    async initializeAudio() {
        this.updateLoadingProgress(75, 'オーディオシステムを初期化中...');
        
        try {
            this.audioManager = new AudioManager();
            await this.audioManager.init();
        } catch (error) {
            this.logger.warn('⚠️ Audio initialization failed:', error);
            // オーディオが利用できなくてもゲームは続行
        }
    }

    /**
     * セーブシステム初期化
     */
    async initializeSaveSystem() {
        this.updateLoadingProgress(90, 'セーブシステムを準備中...');
        
        this.saveManager = new SaveManager();
        await this.saveManager.init();
    }

    /**
     * イベントリスナー設定
     */
    setupEventListeners() {
        this.updateLoadingProgress(95, '最終設定中...');
        
        // タイトル画面のボタンイベント
        document.getElementById('new-game-btn')?.addEventListener('click', () => {
            this.startNewGame();
        });
        
        document.getElementById('load-game-btn')?.addEventListener('click', () => {
            this.loadGame();
        });
        
        document.getElementById('settings-btn')?.addEventListener('click', () => {
            this.showSettings();
        });
        
        document.getElementById('about-btn')?.addEventListener('click', () => {
            this.showAbout();
        });
        
        // 設定画面のボタンイベント
        document.getElementById('save-settings')?.addEventListener('click', () => {
            this.saveSettings();
        });
        
        document.getElementById('back-to-title')?.addEventListener('click', () => {
            this.showTitleScreen();
        });
        
        // キーボードイベント
        document.addEventListener('keydown', (event) => {
            this.handleKeyDown(event);
        });
        
        // ウィンドウイベント
        window.addEventListener('beforeunload', () => {
            this.cleanup();
        });
        
        this.updateLoadingProgress(100, '準備完了！');
    }

    /**
     * タイトル画面表示
     */
    showTitleScreen() {
        this.screenManager.showScreen('title-screen');
        
        // BGM再生
        if (this.audioManager) {
            this.audioManager.playBGM('title');
        }
        
        this.logger.info('📺 Title screen displayed');
    }

    /**
     * 新しいゲーム開始
     */
    async startNewGame() {
        this.logger.info('🆕 Starting new game...');
        
        try {
            // ゲーム初期化
            await this.gameEngine.startNewGame();
            
            // ゲーム画面に遷移
            this.screenManager.showScreen('game-screen');
            
            // BGM変更
            if (this.audioManager) {
                this.audioManager.playBGM('town');
            }
            
            this.logger.info('✅ New game started successfully');
            
        } catch (error) {
            this.logger.error('❌ Failed to start new game:', error);
            this.showError('ゲームを開始できませんでした。');
        }
    }

    /**
     * ゲームロード
     */
    async loadGame() {
        this.logger.info('📁 Loading saved game...');
        
        try {
            const saveData = await this.saveManager.loadGame();
            
            if (!saveData) {
                this.showError('セーブデータが見つかりません。');
                return;
            }
            
            // ゲーム状態復元
            await this.gameEngine.loadGame(saveData);
            
            // ゲーム画面に遷移
            this.screenManager.showScreen('game-screen');
            
            this.logger.info('✅ Game loaded successfully');
            
        } catch (error) {
            this.logger.error('❌ Failed to load game:', error);
            this.showError('ゲームの読み込みに失敗しました。');
        }
    }

    /**
     * 設定画面表示
     */
    showSettings() {
        this.screenManager.showScreen('settings-screen');
        this.loadSettings();
    }

    /**
     * 設定読み込み
     */
    loadSettings() {
        const settings = this.saveManager.loadSettings();
        
        // 音量設定
        document.getElementById('bgm-volume').value = settings.bgmVolume || 70;
        document.getElementById('se-volume').value = settings.seVolume || 80;
        
        // 表示設定
        document.getElementById('fullscreen-mode').checked = settings.fullscreen || false;
        document.getElementById('show-fps').checked = settings.showFPS || false;
        
        // ゲーム設定
        document.getElementById('auto-save').checked = settings.autoSave !== false;
        document.getElementById('skip-animations').checked = settings.skipAnimations || false;
    }

    /**
     * 設定保存
     */
    saveSettings() {
        const settings = {
            bgmVolume: parseInt(document.getElementById('bgm-volume').value),
            seVolume: parseInt(document.getElementById('se-volume').value),
            fullscreen: document.getElementById('fullscreen-mode').checked,
            showFPS: document.getElementById('show-fps').checked,
            autoSave: document.getElementById('auto-save').checked,
            skipAnimations: document.getElementById('skip-animations').checked,
        };
        
        this.saveManager.saveSettings(settings);
        
        // 設定を適用
        this.applySettings(settings);
        
        this.showMessage('設定を保存しました。');
        this.logger.info('💾 Settings saved');
    }

    /**
     * 設定適用
     */
    applySettings(settings) {
        if (this.audioManager) {
            this.audioManager.setBGMVolume(settings.bgmVolume / 100);
            this.audioManager.setSEVolume(settings.seVolume / 100);
        }
        
        if (this.gameEngine) {
            this.gameEngine.setShowFPS(settings.showFPS);
            this.gameEngine.setSkipAnimations(settings.skipAnimations);
        }
    }

    /**
     * About画面表示
     */
    showAbout() {
        const aboutHTML = `
            <div class="about-content">
                <h2>🎮 Black Onyx Reborn について</h2>
                <p>日本初の本格的コンピューターRPG「ブラックオニキス」(1984年)の復刻版です。</p>
                <p>オリジナルの魅力を現代のWeb技術で蘇らせました。</p>
                <div class="credits">
                    <h3>開発チーム</h3>
                    <p>Black Onyx Reborn Development Team</p>
                    <h3>オリジナル</h3>
                    <p>© 1984 Henex/Heartsoft</p>
                </div>
                <button class="btn btn-primary" onclick="blackOnyxReborn.showTitleScreen()">閉じる</button>
            </div>
        `;
        
        this.showModal(aboutHTML);
    }

    /**
     * キー入力処理
     */
    handleKeyDown(event) {
        // ESCキーでタイトルに戻る（設定画面などから）
        if (event.key === 'Escape') {
            const currentScreen = this.screenManager.getCurrentScreen();
            if (currentScreen === 'settings-screen') {
                this.showTitleScreen();
            }
        }
        
        // ゲーム中のキー入力はゲームエンジンに委譲
        if (this.gameEngine && this.screenManager.getCurrentScreen() === 'game-screen') {
            this.gameEngine.handleKeyDown(event);
        }
    }

    /**
     * エラー表示
     */
    showError(message) {
        console.error('❌', message);
        alert(message); // 本来はモーダルダイアログを使用
    }

    /**
     * メッセージ表示
     */
    showMessage(message) {
        console.info('💬', message);
        // 本来は通知システムを使用
    }

    /**
     * モーダル表示
     */
    showModal(content) {
        // 本来はモーダルシステムを実装
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.innerHTML = content;
        document.body.appendChild(modal);
    }

    /**
     * エラー画面表示
     */
    showErrorScreen(error) {
        document.body.innerHTML = `
            <div class="error-screen">
                <h1>❌ エラーが発生しました</h1>
                <p>申し訳ございません。ゲームの初期化中にエラーが発生しました。</p>
                <details>
                    <summary>エラー詳細</summary>
                    <pre>${error.stack || error.message}</pre>
                </details>
                <button onclick="location.reload()">再読み込み</button>
            </div>
        `;
    }

    /**
     * クリーンアップ処理
     */
    cleanup() {
        this.logger.info('🧹 Cleaning up...');
        
        if (this.gameEngine) {
            this.gameEngine.cleanup();
        }
        
        if (this.audioManager) {
            this.audioManager.cleanup();
        }
    }
}

// グローバルインスタンス作成
const blackOnyxReborn = new BlackOnyxReborn();

// DOM読み込み完了後に初期化
document.addEventListener('DOMContentLoaded', () => {
    blackOnyxReborn.init();
});

// デバッグ用にグローバルに公開
window.blackOnyxReborn = blackOnyxReborn;

export default blackOnyxReborn;