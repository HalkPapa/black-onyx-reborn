using UnityEngine;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// プレイヤーの入力とダンジョン内での移動を制御するコントローラー
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveDelay = 0.2f; // キー連打防止
        [SerializeField] private bool allowDiagonalMovement = false;
        [SerializeField] private float keyRepeatDelay = 0.5f; // キー長押し時の遅延
        [SerializeField] private float keyRepeatRate = 0.1f; // キー長押し時のリピート間隔
        
        [Header("Audio")]
        [SerializeField] private bool playWalkSound = true;
        [SerializeField] private bool playWallBumpSound = true;
        
        // Input tracking
        private float lastMoveTime = 0f;
        private Vector2Int lastDirection = Vector2Int.zero;
        private float keyHoldTime = 0f;
        private bool isKeyHeld = false;
        
        // Manager references
        private DungeonManager dungeonManager;
        private AudioManager audioManager;
        private GameManager gameManager;
        
        // Movement state
        private bool canMove = true;
        private Coroutine moveCoroutine;
        
        void Start()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                dungeonManager = gameManager.DungeonManager;
                audioManager = gameManager.AudioManager;
            }
            
            // Subscribe to game state changes
            if (gameManager != null)
            {
                gameManager.OnStateChanged += OnGameStateChanged;
            }
            
            Debug.Log("🎮 Player Controller initialized");
        }
        
        void Update()
        {
            // Only process input during gameplay
            if (gameManager == null || gameManager.CurrentState != GameManager.GameState.InGame)
                return;
                
            if (!canMove)
                return;
            
            HandleMovementInput();
        }
        
        /// <summary>
        /// 移動入力の処理
        /// </summary>
        private void HandleMovementInput()
        {
            Vector2Int inputDirection = GetInputDirection();
            
            if (inputDirection == Vector2Int.zero)
            {
                // No input - reset key hold state
                ResetKeyHoldState();
                return;
            }
            
            // Check if this is a new direction or continued input
            if (inputDirection != lastDirection)
            {
                // New direction - immediate move
                AttemptMove(inputDirection);
                lastDirection = inputDirection;
                keyHoldTime = 0f;
                isKeyHeld = false;
            }
            else
            {
                // Same direction - handle key repeat
                HandleKeyRepeat(inputDirection);
            }
        }
        
        /// <summary>
        /// 入力方向の取得
        /// </summary>
        private Vector2Int GetInputDirection()
        {
            Vector2Int direction = Vector2Int.zero;
            
            // Arrow keys and WASD
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                direction.y = 1;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                direction.y = -1;
            
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                direction.x = -1;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                direction.x = 1;
            
            // Handle diagonal movement
            if (!allowDiagonalMovement && direction.x != 0 && direction.y != 0)
            {
                // Prioritize vertical movement for 4-directional movement
                direction.x = 0;
            }
            
            // Numpad support (classic roguelike style)
            if (direction == Vector2Int.zero)
            {
                direction = GetNumpadDirection();
            }
            
            return direction;
        }
        
        /// <summary>
        /// テンキー入力の処理（ローグライク風）
        /// </summary>
        private Vector2Int GetNumpadDirection()
        {
            if (Input.GetKey(KeyCode.Keypad8)) return new Vector2Int(0, 1);   // North
            if (Input.GetKey(KeyCode.Keypad2)) return new Vector2Int(0, -1);  // South
            if (Input.GetKey(KeyCode.Keypad4)) return new Vector2Int(-1, 0);  // West
            if (Input.GetKey(KeyCode.Keypad6)) return new Vector2Int(1, 0);   // East
            
            if (allowDiagonalMovement)
            {
                if (Input.GetKey(KeyCode.Keypad7)) return new Vector2Int(-1, 1);  // Northwest
                if (Input.GetKey(KeyCode.Keypad9)) return new Vector2Int(1, 1);   // Northeast
                if (Input.GetKey(KeyCode.Keypad1)) return new Vector2Int(-1, -1); // Southwest
                if (Input.GetKey(KeyCode.Keypad3)) return new Vector2Int(1, -1);  // Southeast
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// キーリピート処理
        /// </summary>
        private void HandleKeyRepeat(Vector2Int direction)
        {
            keyHoldTime += Time.deltaTime;
            
            if (!isKeyHeld && keyHoldTime >= keyRepeatDelay)
            {
                // Start key repeat
                isKeyHeld = true;
                AttemptMove(direction);
            }
            else if (isKeyHeld && keyHoldTime >= keyRepeatRate)
            {
                // Continue key repeat
                AttemptMove(direction);
                keyHoldTime = 0f; // Reset for next repeat
            }
        }
        
        /// <summary>
        /// キー状態のリセット
        /// </summary>
        private void ResetKeyHoldState()
        {
            lastDirection = Vector2Int.zero;
            keyHoldTime = 0f;
            isKeyHeld = false;
        }
        
        /// <summary>
        /// 移動試行
        /// </summary>
        private void AttemptMove(Vector2Int direction)
        {
            // Check move delay
            if (Time.time - lastMoveTime < moveDelay)
                return;
            
            if (dungeonManager == null)
                return;
            
            // Try to move
            bool moveSuccessful = dungeonManager.MovePlayer(direction);
            
            if (moveSuccessful)
            {
                OnMoveSuccessful(direction);
            }
            else
            {
                OnMoveBlocked(direction);
            }
            
            lastMoveTime = Time.time;
        }
        
        /// <summary>
        /// 移動成功時の処理
        /// </summary>
        private void OnMoveSuccessful(Vector2Int direction)
        {
            // Play walking sound
            if (playWalkSound && audioManager != null)
            {
                audioManager.PlaySE("walk");
            }
            
            Debug.Log($"🚶 Player moved: {direction}");
        }
        
        /// <summary>
        /// 移動失敗時の処理
        /// </summary>
        private void OnMoveBlocked(Vector2Int direction)
        {
            // Play wall bump sound
            if (playWallBumpSound && audioManager != null)
            {
                audioManager.PlaySE("bump");
            }
            
            Debug.Log($"🚫 Movement blocked: {direction}");
        }
        
        /// <summary>
        /// ゲーム状態変更時の処理
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.InGame:
                    canMove = true;
                    break;
                    
                case GameManager.GameState.Paused:
                case GameManager.GameState.Settings:
                case GameManager.GameState.GameOver:
                    canMove = false;
                    ResetKeyHoldState();
                    break;
            }
        }
        
        /// <summary>
        /// 移動を一時的に無効化
        /// </summary>
        public void DisableMovement(float duration = 0f)
        {
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            
            canMove = false;
            ResetKeyHoldState();
            
            if (duration > 0f)
            {
                moveCoroutine = StartCoroutine(EnableMovementAfterDelay(duration));
            }
        }
        
        /// <summary>
        /// 移動を再有効化
        /// </summary>
        public void EnableMovement()
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            
            canMove = true;
        }
        
        /// <summary>
        /// 遅延後の移動再有効化
        /// </summary>
        private IEnumerator EnableMovementAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            canMove = true;
            moveCoroutine = null;
        }
        
        /// <summary>
        /// 対角移動の設定
        /// </summary>
        public void SetDiagonalMovement(bool enabled)
        {
            allowDiagonalMovement = enabled;
            Debug.Log($"🎮 Diagonal movement: {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// 移動速度の設定
        /// </summary>
        public void SetMoveDelay(float delay)
        {
            moveDelay = Mathf.Max(0.05f, delay);
            Debug.Log($"🎮 Move delay set to: {moveDelay}s");
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}