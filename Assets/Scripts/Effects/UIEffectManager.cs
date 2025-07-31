using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// UIエフェクト・演出管理システム
    /// ブラックオニキス復刻版用の華やかな演出効果
    /// </summary>
    public class UIEffectManager : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private bool enableEffects = true;
        [SerializeField] private float effectIntensity = 1.0f;
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private bool enableScreenShake = true;
        
        [Header("Text Effects")]
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private Transform floatingTextContainer;
        [SerializeField] private Font effectFont;
        [SerializeField] private float floatingTextDuration = 2f;
        
        [Header("Screen Effects")]
        [SerializeField] private Image screenFlashOverlay;
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem levelUpParticles;
        [SerializeField] private ParticleSystem combatHitParticles;
        [SerializeField] private ParticleSystem itemGetParticles;
        [SerializeField] private ParticleSystem specialEffectParticles;
        
        // Manager references
        private GameManager gameManager;
        private AudioManager audioManager;
        private Camera mainCamera;
        private Canvas uiCanvas;
        
        // Effect state
        private Queue<FloatingTextData> pendingFloatingTexts = new Queue<FloatingTextData>();
        private List<GameObject> activeFloatingTexts = new List<GameObject>();
        private Coroutine screenShakeCoroutine;
        private Vector3 originalCameraPosition;
        
        // Effect pools
        private Queue<GameObject> floatingTextPool = new Queue<GameObject>();
        private const int FLOATING_TEXT_POOL_SIZE = 20;
        
        void Start()
        {
            InitializeUIEffectManager();
        }
        
        void Update()
        {
            ProcessPendingEffects();
            CleanupFinishedEffects();
        }
        
        /// <summary>
        /// UIエフェクトマネージャーの初期化
        /// </summary>
        private void InitializeUIEffectManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                audioManager = gameManager.AudioManager;
            }
            
            // Get camera and canvas
            mainCamera = Camera.main ?? FindObjectOfType<Camera>();
            uiCanvas = FindObjectOfType<Canvas>();
            
            // Create screen flash overlay if not assigned
            CreateScreenFlashOverlay();
            
            // Create floating text container
            CreateFloatingTextContainer();
            
            // Initialize particle systems
            InitializeParticleSystems();
            
            // Create floating text pool
            CreateFloatingTextPool();
            
            // Store original camera position
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.position;
            }
            
            // Subscribe to game events
            SubscribeToGameEvents();
            
            Debug.Log("✨ UI Effect Manager initialized");
        }
        
        /// <summary>
        /// スクリーンフラッシュオーバーレイの作成
        /// </summary>
        private void CreateScreenFlashOverlay()
        {
            if (screenFlashOverlay == null && uiCanvas != null)
            {
                GameObject flashObj = new GameObject("ScreenFlashOverlay");
                flashObj.transform.SetParent(uiCanvas.transform, false);
                
                screenFlashOverlay = flashObj.AddComponent<Image>();
                screenFlashOverlay.color = new Color(1, 1, 1, 0);
                screenFlashOverlay.raycastTarget = false;
                
                // Set to full screen
                RectTransform rect = flashObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                // Set high sorting order
                flashObj.transform.SetAsLastSibling();
            }
        }
        
        /// <summary>
        /// フローティングテキストコンテナの作成
        /// </summary>
        private void CreateFloatingTextContainer()
        {
            if (floatingTextContainer == null && uiCanvas != null)
            {
                GameObject containerObj = new GameObject("FloatingTextContainer");
                containerObj.transform.SetParent(uiCanvas.transform, false);
                floatingTextContainer = containerObj.transform;
                
                // Set sorting order
                containerObj.transform.SetSiblingIndex(uiCanvas.transform.childCount - 2);
            }
        }
        
        /// <summary>
        /// パーティクルシステムの初期化
        /// </summary>
        private void InitializeParticleSystems()
        {
            // Create default particle systems if not assigned
            if (levelUpParticles == null)
            {
                levelUpParticles = CreateParticleSystem("LevelUpParticles", Color.yellow);
            }
            
            if (combatHitParticles == null)
            {
                combatHitParticles = CreateParticleSystem("CombatHitParticles", Color.red);
            }
            
            if (itemGetParticles == null)
            {
                itemGetParticles = CreateParticleSystem("ItemGetParticles", Color.cyan);
            }
            
            if (specialEffectParticles == null)
            {
                specialEffectParticles = CreateParticleSystem("SpecialEffectParticles", Color.magenta);
            }
        }
        
        /// <summary>
        /// パーティクルシステムの作成
        /// </summary>
        private ParticleSystem CreateParticleSystem(string name, Color color)
        {
            GameObject particleObj = new GameObject(name);
            particleObj.transform.SetParent(transform, false);
            
            ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = color;
            main.startLifetime = 1f;
            main.startSpeed = 5f;
            main.maxParticles = 50;
            
            var emission = particles.emission;
            emission.rateOverTime = 0; // Burst only
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            
            return particles;
        }
        
        /// <summary>
        /// フローティングテキストプールの作成
        /// </summary>
        private void CreateFloatingTextPool()
        {
            for (int i = 0; i < FLOATING_TEXT_POOL_SIZE; i++)
            {
                GameObject floatingTextObj = CreateFloatingTextObject();
                floatingTextObj.SetActive(false);
                floatingTextPool.Enqueue(floatingTextObj);
            }
        }
        
        /// <summary>
        /// フローティングテキストオブジェクトの作成
        /// </summary>
        private GameObject CreateFloatingTextObject()
        {
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.SetParent(floatingTextContainer, false);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 24f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            
            // Add outline
            text.fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
            
            return textObj;
        }
        
        /// <summary>
        /// ゲームイベントの購読
        /// </summary>
        private void SubscribeToGameEvents()
        {
            // Combat events
            var combatManager = gameManager?.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                combatManager.OnPlayerLevelUp += OnPlayerLevelUp;
                combatManager.OnPlayerDamaged += OnPlayerDamaged;
                combatManager.OnEnemyDamaged += OnEnemyDamaged;
            }
            
            // Item events
            var itemManager = gameManager?.GetComponent<ItemManager>();
            if (itemManager != null)
            {
                itemManager.OnItemAdded += OnItemAdded;
                itemManager.OnItemUsed += OnItemUsed;
            }
            
            // Special item events
            var specialItemManager = gameManager?.GetComponent<SpecialItemManager>();
            if (specialItemManager != null)
            {
                specialItemManager.OnSpecialItemObtained += OnSpecialItemObtained;
                specialItemManager.OnInvisibilityActivated += OnInvisibilityActivated;
            }
        }
        
        /// <summary>
        /// 待機中エフェクトの処理
        /// </summary>
        private void ProcessPendingEffects()
        {
            while (pendingFloatingTexts.Count > 0)
            {
                var textData = pendingFloatingTexts.Dequeue();
                CreateFloatingText(textData);
            }
        }
        
        /// <summary>
        /// 終了したエフェクトのクリーンアップ
        /// </summary>
        private void CleanupFinishedEffects()
        {
            for (int i = activeFloatingTexts.Count - 1; i >= 0; i--)
            {
                if (activeFloatingTexts[i] == null || !activeFloatingTexts[i].activeInHierarchy)
                {
                    activeFloatingTexts.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// フローティングテキストの表示
        /// </summary>
        public void ShowFloatingText(string text, Vector3 worldPosition, Color color, float size = 24f, FloatingTextType type = FloatingTextType.Normal)
        {
            if (!enableEffects) return;
            
            var textData = new FloatingTextData
            {
                text = text,
                worldPosition = worldPosition,
                screenPosition = Vector3.zero,
                color = color,
                size = size,
                type = type,
                useWorldPosition = true
            };
            
            pendingFloatingTexts.Enqueue(textData);
        }
        
        /// <summary>
        /// フローティングテキストの表示（スクリーン座標）
        /// </summary>
        public void ShowFloatingTextAtScreen(string text, Vector2 screenPosition, Color color, float size = 24f, FloatingTextType type = FloatingTextType.Normal)
        {
            if (!enableEffects) return;
            
            var textData = new FloatingTextData
            {
                text = text,
                worldPosition = Vector3.zero,
                screenPosition = screenPosition,
                color = color,
                size = size,
                type = type,
                useWorldPosition = false
            };
            
            pendingFloatingTexts.Enqueue(textData);
        }
        
        /// <summary>
        /// フローティングテキストの作成
        /// </summary>
        private void CreateFloatingText(FloatingTextData data)
        {
            GameObject textObj = GetPooledFloatingText();
            if (textObj == null) return;
            
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = data.text;
            text.color = data.color;
            text.fontSize = data.size * effectIntensity;
            
            // Position
            RectTransform rect = textObj.GetComponent<RectTransform>();
            if (data.useWorldPosition)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, data.worldPosition);
                rect.position = screenPos;
            }
            else
            {
                rect.position = data.screenPosition;
            }
            
            textObj.SetActive(true);
            activeFloatingTexts.Add(textObj);
            
            // Start animation
            StartCoroutine(AnimateFloatingText(textObj, data.type));
        }
        
        /// <summary>
        /// プールからフローティングテキストを取得
        /// </summary>
        private GameObject GetPooledFloatingText()
        {
            if (floatingTextPool.Count > 0)
            {
                return floatingTextPool.Dequeue();
            }
            
            // Create new one if pool is empty
            return CreateFloatingTextObject();
        }
        
        /// <summary>
        /// フローティングテキストをプールに返却
        /// </summary>
        private void ReturnFloatingTextToPool(GameObject textObj)
        {
            textObj.SetActive(false);
            floatingTextPool.Enqueue(textObj);
        }
        
        /// <summary>
        /// フローティングテキストのアニメーション
        /// </summary>
        private IEnumerator AnimateFloatingText(GameObject textObj, FloatingTextType type)
        {
            RectTransform rect = textObj.GetComponent<RectTransform>();
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos;
            Color startColor = text.color;
            Color endColor = startColor;
            endColor.a = 0f;
            
            float duration = floatingTextDuration;
            float moveDistance = 100f * effectIntensity;
            
            // Type-specific animations
            switch (type)
            {
                case FloatingTextType.Damage:
                    endPos.y += moveDistance;
                    endPos.x += Random.Range(-20f, 20f);
                    break;
                case FloatingTextType.Healing:
                    endPos.y += moveDistance * 0.8f;
                    break;
                case FloatingTextType.Experience:
                    endPos.y += moveDistance * 1.2f;
                    endPos.x += Random.Range(-30f, 30f);
                    break;
                case FloatingTextType.Special:
                    duration *= 1.5f;
                    endPos.y += moveDistance * 0.5f;
                    // Add pulse effect
                    StartCoroutine(PulseText(text, duration));
                    break;
                default:
                    endPos.y += moveDistance;
                    break;
            }
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                text.color = Color.Lerp(startColor, endColor, t);
                
                yield return null;
            }
            
            ReturnFloatingTextToPool(textObj);
        }
        
        /// <summary>
        /// テキストの脈動エフェクト
        /// </summary>
        private IEnumerator PulseText(TextMeshProUGUI text, float duration)
        {
            float elapsed = 0f;
            float originalSize = text.fontSize;
            
            while (elapsed < duration && text != null)
            {
                elapsed += Time.deltaTime;
                float pulse = 1f + 0.2f * Mathf.Sin(elapsed * 8f);
                text.fontSize = originalSize * pulse;
                
                yield return null;
            }
        }
        
        /// <summary>
        /// スクリーンフラッシュエフェクト
        /// </summary>
        public void FlashScreen(Color flashColor, float intensity = 1f)
        {
            if (!enableEffects || screenFlashOverlay == null) return;
            
            StartCoroutine(FlashScreenCoroutine(flashColor, intensity));
        }
        
        /// <summary>
        /// スクリーンフラッシュのコルーチン
        /// </summary>
        private IEnumerator FlashScreenCoroutine(Color flashColor, float intensity)
        {
            float adjustedIntensity = intensity * effectIntensity;
            flashColor.a = adjustedIntensity;
            
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                float alpha = flashCurve.Evaluate(t) * adjustedIntensity;
                
                Color currentColor = flashColor;
                currentColor.a = alpha;
                screenFlashOverlay.color = currentColor;
                
                yield return null;
            }
            
            screenFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        }
        
        /// <summary>
        /// スクリーンシェイクエフェクト
        /// </summary>
        public void ShakeScreen(float intensity = 1f, float duration = 0.5f)
        {
            if (!enableEffects || !enableScreenShake || mainCamera == null) return;
            
            if (screenShakeCoroutine != null)
            {
                StopCoroutine(screenShakeCoroutine);
            }
            
            screenShakeCoroutine = StartCoroutine(ShakeScreenCoroutine(intensity, duration));
        }
        
        /// <summary>
        /// スクリーンシェイクのコルーチン
        /// </summary>
        private IEnumerator ShakeScreenCoroutine(float intensity, float duration)
        {
            float adjustedIntensity = intensity * effectIntensity;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                float currentIntensity = adjustedIntensity * (1f - elapsed / duration);
                Vector3 randomOffset = Random.insideUnitSphere * currentIntensity;
                randomOffset.z = 0;
                
                mainCamera.transform.position = originalCameraPosition + randomOffset;
                
                yield return null;
            }
            
            mainCamera.transform.position = originalCameraPosition;
            screenShakeCoroutine = null;
        }
        
        /// <summary>
        /// パーティクルエフェクトの再生
        /// </summary>
        public void PlayParticleEffect(ParticleEffectType effectType, Vector3 position)
        {
            if (!enableEffects || !enableParticles) return;
            
            ParticleSystem particles = GetParticleSystem(effectType);
            if (particles != null)
            {
                particles.transform.position = position;
                particles.Emit(20);
            }
        }
        
        /// <summary>
        /// パーティクルシステムの取得
        /// </summary>
        private ParticleSystem GetParticleSystem(ParticleEffectType effectType)
        {
            switch (effectType)
            {
                case ParticleEffectType.LevelUp:
                    return levelUpParticles;
                case ParticleEffectType.CombatHit:
                    return combatHitParticles;
                case ParticleEffectType.ItemGet:
                    return itemGetParticles;
                case ParticleEffectType.Special:
                    return specialEffectParticles;
                default:
                    return null;
            }
        }
        
        // Event handlers
        
        private void OnPlayerLevelUp(int newLevel)
        {
            ShowFloatingTextAtScreen($"LEVEL UP! {newLevel}", new Vector2(Screen.width * 0.5f, Screen.height * 0.6f), Color.yellow, 32f, FloatingTextType.Special);
            FlashScreen(Color.yellow, 0.3f);
            ShakeScreen(0.5f, 0.3f);
            PlayParticleEffect(ParticleEffectType.LevelUp, Vector3.zero);
        }
        
        private void OnPlayerDamaged(int damage, int currentHP)
        {
            ShowFloatingTextAtScreen($"-{damage}", new Vector2(Screen.width * 0.3f, Screen.height * 0.5f), Color.red, 28f, FloatingTextType.Damage);
            FlashScreen(Color.red, 0.2f);
            ShakeScreen(0.3f, 0.2f);
        }
        
        private void OnEnemyDamaged(Enemy enemy, int damage)
        {
            ShowFloatingTextAtScreen($"-{damage}", new Vector2(Screen.width * 0.7f, Screen.height * 0.5f), Color.orange, 24f, FloatingTextType.Damage);
            PlayParticleEffect(ParticleEffectType.CombatHit, Vector3.zero);
        }
        
        private void OnItemAdded(ItemData itemData, int quantity)
        {
            string text = quantity > 1 ? $"{itemData.itemName} x{quantity}" : itemData.itemName;
            ShowFloatingTextAtScreen($"GET: {text}", new Vector2(Screen.width * 0.5f, Screen.height * 0.3f), Color.cyan, 20f, FloatingTextType.Normal);
            PlayParticleEffect(ParticleEffectType.ItemGet, Vector3.zero);
        }
        
        private void OnItemUsed(ItemData itemData)
        {
            ShowFloatingTextAtScreen($"USED: {itemData.itemName}", new Vector2(Screen.width * 0.5f, Screen.height * 0.4f), Color.green, 18f, FloatingTextType.Normal);
        }
        
        private void OnSpecialItemObtained(string itemId)
        {
            ShowFloatingTextAtScreen("✨ SPECIAL ITEM! ✨", new Vector2(Screen.width * 0.5f, Screen.height * 0.7f), Color.magenta, 36f, FloatingTextType.Special);
            FlashScreen(Color.magenta, 0.5f);
            ShakeScreen(0.8f, 0.5f);
            PlayParticleEffect(ParticleEffectType.Special, Vector3.zero);
        }
        
        private void OnInvisibilityActivated()
        {
            ShowFloatingTextAtScreen("👻 INVISIBLE! 👻", new Vector2(Screen.width * 0.5f, Screen.height * 0.8f), Color.white, 30f, FloatingTextType.Special);
            FlashScreen(Color.white, 0.4f);
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            var combatManager = gameManager?.GetComponent<CombatManager>();
            if (combatManager != null)
            {
                combatManager.OnPlayerLevelUp -= OnPlayerLevelUp;
                combatManager.OnPlayerDamaged -= OnPlayerDamaged;
                combatManager.OnEnemyDamaged -= OnEnemyDamaged;
            }
            
            var itemManager = gameManager?.GetComponent<ItemManager>();
            if (itemManager != null)
            {
                itemManager.OnItemAdded -= OnItemAdded;
                itemManager.OnItemUsed -= OnItemUsed;
            }
            
            var specialItemManager = gameManager?.GetComponent<SpecialItemManager>();
            if (specialItemManager != null)
            {
                specialItemManager.OnSpecialItemObtained -= OnSpecialItemObtained;
                specialItemManager.OnInvisibilityActivated -= OnInvisibilityActivated;
            }
        }
    }
    
    /// <summary>
    /// フローティングテキストデータ
    /// </summary>
    public class FloatingTextData
    {
        public string text;
        public Vector3 worldPosition;
        public Vector3 screenPosition;
        public Color color;
        public float size;
        public FloatingTextType type;
        public bool useWorldPosition;
    }
    
    /// <summary>
    /// フローティングテキストタイプ
    /// </summary>
    public enum FloatingTextType
    {
        Normal,
        Damage,
        Healing, 
        Experience,
        Special
    }
    
    /// <summary>
    /// パーティクルエフェクトタイプ
    /// </summary>
    public enum ParticleEffectType
    {
        LevelUp,
        CombatHit,
        ItemGet,
        Special
    }
}