using UnityEngine;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// UI全体の管理を行うマネージャー
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Canvas References")]
        [SerializeField] private GameObject mainCanvas;
        [SerializeField] private GameObject hudCanvas;
        [SerializeField] private GameObject menuCanvas;
        
        [Header("Screen Panels")]
        [SerializeField] private GameObject titleScreen;
        [SerializeField] private GameObject gameScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject settingsScreen;
        [SerializeField] private GameObject gameOverScreen;
        
        [Header("HUD Elements")]
        [SerializeField] private GameObject messageText;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private GameObject[] commandButtons;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeTime = 0.5f;
        [SerializeField] private float messageDisplayTime = 3f;
        
        // Current active screen
        private GameObject currentScreen;
        
        // Message queue
        private System.Collections.Generic.Queue<string> messageQueue = new System.Collections.Generic.Queue<string>();
        private bool isShowingMessage = false;
        
        void Awake()
        {
            InitializeUI();
        }
        
        void Start()
        {
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
            
            // Show initial screen
            ShowScreen("Title");
        }
        
        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            // Find Canvas components if not assigned
            if (mainCanvas == null)
            {
                var mainCanvasObj = GameObject.FindGameObjectWithTag("MainUI");
                if (mainCanvasObj != null) mainCanvas = mainCanvasObj;
            }
            if (hudCanvas == null)
            {
                var hudCanvasObj = GameObject.FindGameObjectWithTag("HUD");
                if (hudCanvasObj != null) hudCanvas = hudCanvasObj;
            }
            if (menuCanvas == null)
            {
                var menuCanvasObj = GameObject.FindGameObjectWithTag("Menu");
                if (menuCanvasObj != null) menuCanvas = menuCanvasObj;
            }
            
            // Initialize message panel
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
            
            Debug.Log("🖥️ UI Manager initialized");
        }
        
        /// <summary>
        /// ゲーム状態変更時の処理
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    ShowScreen("Title");
                    break;
                    
                case GameManager.GameState.InGame:
                    ShowScreen("Game");
                    ShowHUD(true);
                    break;
                    
                case GameManager.GameState.Paused:
                    ShowScreen("Pause");
                    break;
                    
                case GameManager.GameState.Settings:
                    ShowScreen("Settings");
                    break;
                    
                case GameManager.GameState.GameOver:
                    ShowScreen("GameOver");
                    ShowHUD(false);
                    break;
            }
        }
        
        /// <summary>
        /// 指定した画面を表示
        /// </summary>
        public void ShowScreen(string screenName)
        {
            // Hide current screen
            if (currentScreen != null)
            {
                StartCoroutine(FadeOut(currentScreen));
            }
            
            // Show new screen
            GameObject newScreen = GetScreenByName(screenName);
            if (newScreen != null)
            {
                currentScreen = newScreen;
                StartCoroutine(FadeIn(newScreen));
            }
        }
        
        /// <summary>
        /// 画面名から GameObject を取得
        /// </summary>
        private GameObject GetScreenByName(string screenName)
        {
            switch (screenName.ToLower())
            {
                case "title": return titleScreen;
                case "game": return gameScreen;
                case "pause": return pauseScreen;
                case "settings": return settingsScreen;
                case "gameover": return gameOverScreen;
                default:
                    Debug.LogWarning($"⚠️ Unknown screen: {screenName}");
                    return null;
            }
        }
        
        /// <summary>
        /// HUDの表示/非表示
        /// </summary>
        public void ShowHUD(bool show)
        {
            if (hudCanvas != null)
            {
                hudCanvas.SetActive(show);
            }
        }
        
        /// <summary>
        /// メッセージを表示
        /// </summary>
        public void ShowMessage(string message)
        {
            messageQueue.Enqueue(message);
            
            if (!isShowingMessage)
            {
                StartCoroutine(ProcessMessageQueue());
            }
        }
        
        /// <summary>
        /// メッセージキューの処理
        /// </summary>
        private IEnumerator ProcessMessageQueue()
        {
            isShowingMessage = true;
            
            while (messageQueue.Count > 0)
            {
                string message = messageQueue.Dequeue();
                yield return StartCoroutine(DisplayMessage(message));
            }
            
            isShowingMessage = false;
        }
        
        /// <summary>
        /// メッセージの表示
        /// </summary>
        private IEnumerator DisplayMessage(string message)
        {
            if (messageText != null && messagePanel != null)
            {
                // Basic text display without UI dependencies
                Debug.Log($"📝 Message: {message}");
                messagePanel.SetActive(true);
                
                // Fade in
                yield return StartCoroutine(FadeIn(messagePanel));
                
                // Wait
                yield return new WaitForSeconds(messageDisplayTime);
                
                // Fade out
                yield return StartCoroutine(FadeOut(messagePanel));
                
                messagePanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// フェードイン効果
        /// </summary>
        private IEnumerator FadeIn(GameObject target)
        {
            if (target == null) yield break;
            
            target.SetActive(true);
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<CanvasGroup>();
            
            float elapsedTime = 0f;
            canvasGroup.alpha = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// フェードアウト効果
        /// </summary>
        private IEnumerator FadeOut(GameObject target)
        {
            if (target == null) yield break;
            
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                target.SetActive(false);
                yield break;
            }
            
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeTime);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            target.SetActive(false);
        }
        
        /// <summary>
        /// ボタンUIの設定 (現在は基本実装のみ)
        /// </summary>
        public void SetupCommandButtons(System.Action<int> onButtonClick)
        {
            if (commandButtons != null)
            {
                Debug.Log($"🔘 Setting up {commandButtons.Length} command buttons");
                // UI パッケージ導入後に実装
            }
        }
        
        /// <summary>
        /// ボタンの有効/無効切り替え (現在は基本実装のみ)
        /// </summary>
        public void SetButtonInteractable(int buttonIndex, bool interactable)
        {
            if (commandButtons != null && buttonIndex >= 0 && buttonIndex < commandButtons.Length)
            {
                Debug.Log($"🔘 Button {buttonIndex} interactable: {interactable}");
                // UI パッケージ導入後に実装
            }
        }
        
        /// <summary>
        /// UI要素のアニメーション
        /// </summary>
        public void AnimateUI(GameObject target, Vector3 targetScale, float duration = 0.3f)
        {
            if (target != null)
            {
                StartCoroutine(ScaleAnimation(target, targetScale, duration));
            }
        }
        
        /// <summary>
        /// スケールアニメーション
        /// </summary>
        private IEnumerator ScaleAnimation(GameObject target, Vector3 targetScale, float duration)
        {
            Vector3 startScale = target.transform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / duration;
                target.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            target.transform.localScale = targetScale;
        }
        
        /// <summary>
        /// 確認ダイアログの表示
        /// </summary>
        public void ShowConfirmDialog(string message, System.Action onConfirm, System.Action onCancel = null)
        {
            // TODO: Implement confirmation dialog
            Debug.Log($"📋 Confirm Dialog: {message}");
            onConfirm?.Invoke();
        }
        
        /// <summary>
        /// ボタンクリック音の再生
        /// </summary>
        public void PlayButtonSound()
        {
            if (GameManager.Instance?.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlaySE("button");
            }
        }
        
        /// <summary>
        /// UI効果音の再生
        /// </summary>
        public void PlayUISound(string soundName)
        {
            if (GameManager.Instance?.AudioManager != null)
            {
                GameManager.Instance.AudioManager.PlaySE(soundName);
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}