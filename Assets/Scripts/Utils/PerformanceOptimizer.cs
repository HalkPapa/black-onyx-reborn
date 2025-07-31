using UnityEngine;
using System.Collections;

namespace BlackOnyxReborn
{
    /// <summary>
    /// パフォーマンス最適化とモニタリング
    /// </summary>
    public class PerformanceOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableOptimizations = true;
        [SerializeField] private bool showFPSCounter = false;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool enableVSync = true;
        
        [Header("Memory Management")]
        [SerializeField] private bool autoGarbageCollection = true;
        [SerializeField] private float gcInterval = 30f; // seconds
        [SerializeField] private long memoryThreshold = 100 * 1024 * 1024; // 100MB
        
        [Header("Object Pooling")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private int maxPoolSize = 100;
        
        [Header("Rendering Optimizations")]
        [SerializeField] private bool optimizeRendering = true;
        [SerializeField] private bool enableOcclusion = false;
        [SerializeField] private bool enableBatching = true;
        
        // Performance monitoring
        private float deltaTime = 0f;
        private float averageFPS = 0f;
        private int frameCount = 0;
        private float fpsUpdateInterval = 1f;
        private float lastFPSUpdate = 0f;
        
        // Memory monitoring
        private long lastMemoryUsage = 0;
        private float lastGCTime = 0f;
        
        // Object pools
        private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Queue<GameObject>> objectPools 
            = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Queue<GameObject>>();
        
        // UI references
        private GUIStyle fpsStyle;
        private Rect fpsRect;
        
        void Start()
        {
            InitializeOptimizer();
        }
        
        void Update()
        {
            if (enableOptimizations)
            {
                UpdatePerformanceMonitoring();
                CheckMemoryUsage();
            }
        }
        
        void OnGUI()
        {
            if (showFPSCounter)
            {
                DrawFPSCounter();
            }
        }
        
        /// <summary>
        /// 最適化システムの初期化
        /// </summary>
        private void InitializeOptimizer()
        {
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;
            
            // Configure VSync
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
            
            // Optimize rendering settings
            if (optimizeRendering)
            {
                ApplyRenderingOptimizations();
            }
            
            // Initialize FPS counter style
            InitializeFPSCounter();
            
            Debug.Log($"🚀 Performance Optimizer initialized - Target FPS: {targetFrameRate}, VSync: {enableVSync}");
        }
        
        /// <summary>
        /// レンダリング最適化の適用
        /// </summary>
        private void ApplyRenderingOptimizations()
        {
            // Disable anti-aliasing for pixel art games
            QualitySettings.antiAliasing = 0;
            
            // Disable anisotropic filtering
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            
            // Enable batching
            if (enableBatching)
            {
                // These would be set in the graphics settings
                PlayerSettings.colorSpace = ColorSpace.Gamma; // Better for pixel art
            }
            
            // Set appropriate pixel light count
            QualitySettings.pixelLightCount = 1;
            
            // Optimize shadows
            QualitySettings.shadows = ShadowQuality.Disable; // Disable for 2D game
            
            Debug.Log("🎨 Rendering optimizations applied");
        }
        
        /// <summary>
        /// FPSカウンターの初期化
        /// </summary>
        private void InitializeFPSCounter()
        {
            fpsStyle = new GUIStyle();
            fpsStyle.fontSize = 16;
            fpsStyle.normal.textColor = Color.white;
            fpsStyle.alignment = TextAnchor.UpperLeft;
            
            fpsRect = new Rect(10, 10, 200, 30);
        }
        
        /// <summary>
        /// パフォーマンス監視の更新
        /// </summary>
        private void UpdatePerformanceMonitoring()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            
            if (Time.time - lastFPSUpdate >= fpsUpdateInterval)
            {
                averageFPS = frameCount / (Time.time - lastFPSUpdate);
                frameCount = 0;
                lastFPSUpdate = Time.time;
                
                // Check for performance issues
                if (averageFPS < targetFrameRate * 0.8f)
                {
                    OnPerformanceIssueDetected();
                }
            }
        }
        
        /// <summary>
        /// メモリ使用量のチェック
        /// </summary>
        private void CheckMemoryUsage()
        {
            if (!autoGarbageCollection) return;
            
            long currentMemory = System.GC.GetTotalMemory(false);
            
            // Check if memory usage increased significantly
            if (currentMemory > lastMemoryUsage + memoryThreshold)
            {
                ForceGarbageCollection();
            }
            
            // Periodic garbage collection
            if (Time.time - lastGCTime >= gcInterval)
            {
                ForceGarbageCollection();
            }
            
            lastMemoryUsage = currentMemory;
        }
        
        /// <summary>
        /// ガベージコレクションの強制実行
        /// </summary>
        public void ForceGarbageCollection()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            lastGCTime = Time.time;
            
            if (Application.isEditor)
            {
                Debug.Log($"🗑️ Forced garbage collection - Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
            }
        }
        
        /// <summary>
        /// パフォーマンス問題検出時の処理
        /// </summary>
        private void OnPerformanceIssueDetected()
        {
            Debug.LogWarning($"⚠️ Performance issue detected - FPS: {averageFPS:F1} (Target: {targetFrameRate})");
            
            // Automatic optimization attempts
            if (enableOptimizations)
            {
                // Reduce quality settings temporarily
                if (QualitySettings.GetQualityLevel() > 0)
                {
                    QualitySettings.DecreaseLevel();
                    Debug.Log("📉 Quality level decreased to improve performance");
                }
                
                // Force garbage collection
                ForceGarbageCollection();
            }
        }
        
        /// <summary>
        /// FPSカウンターの描画
        /// </summary>
        private void DrawFPSCounter()
        {
            float fps = 1.0f / deltaTime;
            string fpsText = $"FPS: {fps:F1}\nAvg: {averageFPS:F1}\nMemory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB";
            
            // Background
            GUI.Box(fpsRect, "");
            
            // FPS text with color coding
            if (fps >= targetFrameRate * 0.9f)
                fpsStyle.normal.textColor = Color.green;
            else if (fps >= targetFrameRate * 0.7f)
                fpsStyle.normal.textColor = Color.yellow;
            else
                fpsStyle.normal.textColor = Color.red;
            
            GUI.Label(fpsRect, fpsText, fpsStyle);
        }
        
        /// <summary>
        /// オブジェクトプールからオブジェクトを取得
        /// </summary>
        public GameObject GetPooledObject(string poolName, GameObject prefab)
        {
            if (!enableObjectPooling) return Instantiate(prefab);
            
            if (!objectPools.ContainsKey(poolName))
            {
                objectPools[poolName] = new System.Collections.Generic.Queue<GameObject>();
            }
            
            var pool = objectPools[poolName];
            
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                return Instantiate(prefab);
            }
        }
        
        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        public void ReturnToPool(string poolName, GameObject obj)
        {
            if (!enableObjectPooling)
            {
                Destroy(obj);
                return;
            }
            
            if (!objectPools.ContainsKey(poolName))
            {
                objectPools[poolName] = new System.Collections.Generic.Queue<GameObject>();
            }
            
            var pool = objectPools[poolName];
            
            if (pool.Count < maxPoolSize)
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        
        /// <summary>
        /// 全プールのクリア
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in objectPools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            
            objectPools.Clear();
            Debug.Log("🧹 All object pools cleared");
        }
        
        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        public PerformanceStats GetPerformanceStats()
        {
            return new PerformanceStats
            {
                currentFPS = 1.0f / deltaTime,
                averageFPS = averageFPS,
                memoryUsage = System.GC.GetTotalMemory(false),
                targetFrameRate = targetFrameRate,
                qualityLevel = QualitySettings.GetQualityLevel(),
                vSyncEnabled = QualitySettings.vSyncCount > 0,
                pooledObjectsCount = GetTotalPooledObjects()
            };
        }
        
        /// <summary>
        /// プール内オブジェクト総数の取得
        /// </summary>
        private int GetTotalPooledObjects()
        {
            int total = 0;
            foreach (var pool in objectPools.Values)
            {
                total += pool.Count;
            }
            return total;
        }
        
        /// <summary>
        /// 設定の更新
        /// </summary>
        public void UpdateSettings(bool showFPS, int targetFPS, bool vSync, bool optimizations)
        {
            showFPSCounter = showFPS;
            targetFrameRate = targetFPS;
            enableVSync = vSync;
            enableOptimizations = optimizations;
            
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
            
            Debug.Log($"🚀 Performance settings updated - FPS: {targetFrameRate}, VSync: {enableVSync}");
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            var stats = GetPerformanceStats();
            return $"Performance Optimizer:\n" +
                   $"FPS: {stats.currentFPS:F1} (Avg: {stats.averageFPS:F1})\n" +
                   $"Memory: {stats.memoryUsage / 1024 / 1024}MB\n" +
                   $"Quality: {stats.qualityLevel}\n" +
                   $"Pooled Objects: {stats.pooledObjectsCount}\n" +
                   $"Optimizations: {enableOptimizations}";
        }
    }
    
    /// <summary>
    /// パフォーマンス統計
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        public float currentFPS;
        public float averageFPS;
        public long memoryUsage;
        public int targetFrameRate;
        public int qualityLevel;
        public bool vSyncEnabled;
        public int pooledObjectsCount;
    }
}