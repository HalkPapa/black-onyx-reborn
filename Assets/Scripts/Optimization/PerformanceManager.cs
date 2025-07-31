using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BlackOnyxReborn
{
    /// <summary>
    /// ブラックオニキス復刻版パフォーマンス最適化マネージャー
    /// ゲームの動作を監視し、最適なパフォーマンスを維持する
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableOptimization = true;
        [SerializeField] private bool enableProfiling = false;
        [SerializeField] private float monitoringInterval = 1f;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private float lowPerformanceThreshold = 30f;
        
        [Header("Memory Management")]
        [SerializeField] private bool enableMemoryOptimization = true;
        [SerializeField] private float memoryCleanupInterval = 30f;
        [SerializeField] private long memoryThresholdMB = 100;
        [SerializeField] private bool enableGarbageCollection = true;
        
        [Header("Quality Settings")]
        [SerializeField] private bool enableDynamicQuality = true;
        [SerializeField] private QualityLevel currentQualityLevel = QualityLevel.High;
        
        [Header("Debug Display")]
        [SerializeField] private bool showPerformanceStats = false;
        [SerializeField] private bool showMemoryStats = false;
        [SerializeField] private bool showProfilerStats = false;
        
        // Performance monitoring
        private float frameRate = 60f;
        private float averageFrameRate = 60f;
        private int frameCount = 0;
        private float frameTime = 0f;
        private float lastMonitorTime = 0f;
        private float lastMemoryCleanup = 0f;
        
        // Performance metrics
        private PerformanceMetrics currentMetrics = new PerformanceMetrics();
        private List<float> frameRateHistory = new List<float>();
        private const int FRAME_RATE_HISTORY_SIZE = 60;
        
        // Manager references
        private GameManager gameManager;
        private AudioManager audioManager;
        private UIEffectManager effectManager;
        
        // Optimization states
        private Dictionary<string, bool> optimizationFlags = new Dictionary<string, bool>();
        private Queue<System.Action> deferredActions = new Queue<System.Action>();
        
        void Start()
        {
            InitializePerformanceManager();
        }
        
        void Update()
        {
            MonitorPerformance();
            ProcessDeferredActions();
            
            if (enableOptimization)
            {
                ApplyPerformanceOptimizations();
            }
        }
        
        void OnGUI()
        {
            if (showPerformanceStats || showMemoryStats || showProfilerStats)
            {
                DrawPerformanceStats();
            }
        }
        
        /// <summary>
        /// パフォーマンスマネージャーの初期化
        /// </summary>
        private void InitializePerformanceManager()
        {
            // Get manager references
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                audioManager = gameManager.AudioManager;
                effectManager = gameManager.GetComponent<UIEffectManager>();
            }
            
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;
            
            // Initialize quality settings
            SetQualityLevel(currentQualityLevel);
            
            // Initialize optimization flags
            InitializeOptimizationFlags();
            
            // Enable profiler if needed
            if (enableProfiling)
            {
                Profiler.enabled = true;
            }
            
            Debug.Log("⚡ Performance Manager initialized");
        }
        
        /// <summary>
        /// 最適化フラグの初期化
        /// </summary>
        private void InitializeOptimizationFlags()
        {
            optimizationFlags["effects_enabled"] = true;
            optimizationFlags["particles_enabled"] = true;
            optimizationFlags["audio_enabled"] = true;
            optimizationFlags["shadows_enabled"] = true;
            optimizationFlags["vsync_enabled"] = true;
            optimizationFlags["antialiasing_enabled"] = true;
        }
        
        /// <summary>
        /// パフォーマンス監視
        /// </summary>
        private void MonitorPerformance()
        {
            // Calculate frame rate
            frameTime += Time.unscaledDeltaTime;
            frameCount++;
            
            if (Time.time - lastMonitorTime >= monitoringInterval)
            {
                // Calculate average frame rate
                frameRate = frameCount / frameTime;
                averageFrameRate = (averageFrameRate + frameRate) * 0.5f;
                
                // Update frame rate history
                UpdateFrameRateHistory(frameRate);
                
                // Update performance metrics
                UpdatePerformanceMetrics();
                
                // Check for low performance
                if (frameRate < lowPerformanceThreshold)
                {
                    OnLowPerformanceDetected();
                }
                
                // Reset counters
                frameCount = 0;
                frameTime = 0f;
                lastMonitorTime = Time.time;
            }
            
            // Memory cleanup
            if (enableMemoryOptimization && Time.time - lastMemoryCleanup >= memoryCleanupInterval)
            {
                PerformMemoryCleanup();
                lastMemoryCleanup = Time.time;
            }
        }
        
        /// <summary>
        /// フレームレート履歴の更新
        /// </summary>
        private void UpdateFrameRateHistory(float currentFrameRate)
        {
            frameRateHistory.Add(currentFrameRate);
            
            if (frameRateHistory.Count > FRAME_RATE_HISTORY_SIZE)
            {
                frameRateHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// パフォーマンスメトリクスの更新
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            currentMetrics.frameRate = frameRate;
            currentMetrics.averageFrameRate = averageFrameRate;
            currentMetrics.memoryUsage = GetMemoryUsageMB();
            currentMetrics.drawCalls = GetDrawCalls();
            currentMetrics.triangles = GetTriangleCount();
            currentMetrics.batches = GetBatchCount();
            
            if (enableProfiling)
            {
                currentMetrics.cpuTime = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
                currentMetrics.gpuTime = 0f; // GPU timing requires Unity Profiler API
            }
        }
        
        /// <summary>
        /// 低パフォーマンス検出時の処理
        /// </summary>
        private void OnLowPerformanceDetected()
        {
            if (!enableOptimization) return;
            
            Debug.LogWarning($"⚠️ Low performance detected: {frameRate:F1} FPS");
            
            // Apply emergency optimizations
            ApplyEmergencyOptimizations();
            
            // Consider reducing quality
            if (enableDynamicQuality && currentQualityLevel > QualityLevel.Low)
            {
                ReduceQualityLevel();
            }
        }
        
        /// <summary>
        /// 緊急最適化の適用
        /// </summary>
        private void ApplyEmergencyOptimizations()
        {
            // Disable non-essential effects
            if (effectManager != null)
            {
                // Reduce effect intensity
                optimizationFlags["effects_enabled"] = false;
            }
            
            // Reduce audio quality
            if (audioManager != null)
            {
                // No specific audio reduction for now
            }
            
            // Force garbage collection
            if (enableGarbageCollection)
            {
                System.GC.Collect();
            }
        }
        
        /// <summary>
        /// パフォーマンス最適化の適用
        /// </summary>
        private void ApplyPerformanceOptimizations()
        {
            // Object pooling optimization
            OptimizeObjectPooling();
            
            // Memory optimization
            OptimizeMemoryUsage();
            
            // Rendering optimization
            OptimizeRendering();
            
            // Audio optimization
            OptimizeAudio();
        }
        
        /// <summary>
        /// オブジェクトプーリング最適化
        /// </summary>
        private void OptimizeObjectPooling()
        {
            // Ensure UI effects are using object pooling
            // This is handled by UIEffectManager
        }
        
        /// <summary>
        /// メモリ使用最適化
        /// </summary>
        private void OptimizeMemoryUsage()
        {
            long currentMemory = GetMemoryUsageMB();
            
            if (currentMemory > memoryThresholdMB)
            {
                // Trigger memory cleanup
                DeferredExecute(() => PerformMemoryCleanup());
            }
        }
        
        /// <summary>
        /// レンダリング最適化
        /// </summary>
        private void OptimizeRendering()
        {
            // Optimize based on current performance
            if (frameRate < lowPerformanceThreshold * 1.2f)
            {
                // Reduce rendering quality
                QualitySettings.pixelLightCount = Mathf.Max(0, QualitySettings.pixelLightCount - 1);
            }
            else if (frameRate > targetFrameRate * 0.9f)
            {
                // Increase rendering quality if performance allows
                QualitySettings.pixelLightCount = Mathf.Min(4, QualitySettings.pixelLightCount + 1);
            }
        }
        
        /// <summary>
        /// オーディオ最適化
        /// </summary>
        private void OptimizeAudio()
        {
            if (audioManager == null) return;
            
            // Audio optimization based on performance
            if (frameRate < lowPerformanceThreshold)
            {
                // Could reduce audio quality here if needed
                optimizationFlags["audio_enabled"] = false;
            }
            else
            {
                optimizationFlags["audio_enabled"] = true;
            }
        }
        
        /// <summary>
        /// メモリクリーンアップの実行
        /// </summary>
        private void PerformMemoryCleanup()
        {
            if (!enableMemoryOptimization) return;
            
            // Unload unused assets
            Resources.UnloadUnusedAssets();
            
            // Force garbage collection
            if (enableGarbageCollection)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
            }
            
            Debug.Log($"🧹 Memory cleanup performed - Memory: {GetMemoryUsageMB()}MB");
        }
        
        /// <summary>
        /// 品質レベルの設定
        /// </summary>
        public void SetQualityLevel(QualityLevel level)
        {
            currentQualityLevel = level;
            
            switch (level)
            {
                case QualityLevel.Low:
                    QualitySettings.SetQualityLevel(0);
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    break;
                    
                case QualityLevel.Medium:
                    QualitySettings.SetQualityLevel(2);
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    break;
                    
                case QualityLevel.High:
                    QualitySettings.SetQualityLevel(4);
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.shadows = ShadowQuality.All;
                    break;
                    
                case QualityLevel.Ultra:
                    QualitySettings.SetQualityLevel(5);
                    QualitySettings.vSyncCount = 1;
                    QualitySettings.antiAliasing = 8;
                    QualitySettings.shadows = ShadowQuality.All;
                    break;
            }
            
            Debug.Log($"⚙️ Quality level set to: {level}");
        }
        
        /// <summary>
        /// 品質レベルの削減
        /// </summary>
        private void ReduceQualityLevel()
        {
            QualityLevel newLevel = currentQualityLevel;
            
            switch (currentQualityLevel)
            {
                case QualityLevel.Ultra:
                    newLevel = QualityLevel.High;
                    break;
                case QualityLevel.High:
                    newLevel = QualityLevel.Medium;
                    break;
                case QualityLevel.Medium:
                    newLevel = QualityLevel.Low;
                    break;
                case QualityLevel.Low:
                    // Already at lowest
                    return;
            }
            
            SetQualityLevel(newLevel);
        }
        
        /// <summary>
        /// 遅延実行
        /// </summary>
        private void DeferredExecute(System.Action action)
        {
            deferredActions.Enqueue(action);
        }
        
        /// <summary>
        /// 遅延アクションの処理
        /// </summary>
        private void ProcessDeferredActions()
        {
            int actionsToProcess = Mathf.Min(5, deferredActions.Count); // 1フレームに最大5アクション
            
            for (int i = 0; i < actionsToProcess; i++)
            {
                if (deferredActions.Count > 0)
                {
                    System.Action action = deferredActions.Dequeue();
                    action?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// メモリ使用量の取得（MB）
        /// </summary>
        private long GetMemoryUsageMB()
        {
            return Profiler.GetTotalAllocatedMemory(false) / (1024 * 1024);
        }
        
        /// <summary>
        /// ドローコール数の取得
        /// </summary>
        private int GetDrawCalls()
        {
            // Unity Statistics API would be used here in a real implementation
            return 0; // Placeholder
        }
        
        /// <summary>
        /// 三角形数の取得
        /// </summary>
        private int GetTriangleCount()
        {
            // Unity Statistics API would be used here in a real implementation
            return 0; // Placeholder
        }
        
        /// <summary>
        /// バッチ数の取得
        /// </summary>
        private int GetBatchCount()
        {
            // Unity Statistics API would be used here in a real implementation
            return 0; // Placeholder
        }
        
        /// <summary>
        /// パフォーマンス統計の描画
        /// </summary>
        private void DrawPerformanceStats()
        {
            int yOffset = 10;
            int lineHeight = 20;
            
            if (showPerformanceStats)
            {
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"FPS: {frameRate:F1} (Avg: {averageFrameRate:F1})");
                yOffset += lineHeight;
                
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Target FPS: {targetFrameRate}");
                yOffset += lineHeight;
                
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Quality: {currentQualityLevel}");
                yOffset += lineHeight;
            }
            
            if (showMemoryStats)
            {
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Memory: {currentMetrics.memoryUsage}MB");
                yOffset += lineHeight;
                
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Memory Threshold: {memoryThresholdMB}MB");
                yOffset += lineHeight;
            }
            
            if (showProfilerStats && enableProfiling)
            {
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Draw Calls: {currentMetrics.drawCalls}");
                yOffset += lineHeight;
                
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Triangles: {currentMetrics.triangles}");
                yOffset += lineHeight;
                
                GUI.Label(new Rect(10, yOffset, 300, lineHeight), $"Batches: {currentMetrics.batches}");
                yOffset += lineHeight;
            }
        }
        
        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        public PerformanceMetrics GetPerformanceMetrics()
        {
            return currentMetrics;
        }
        
        /// <summary>
        /// 最適化状態の取得
        /// </summary>
        public Dictionary<string, bool> GetOptimizationFlags()
        {
            return new Dictionary<string, bool>(optimizationFlags);
        }
        
        /// <summary>
        /// パフォーマンス最適化の有効/無効切り替え
        /// </summary>
        public void SetOptimizationEnabled(bool enabled)
        {
            enableOptimization = enabled;
            Debug.Log($"⚡ Performance optimization: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        /// <summary>
        /// パフォーマンス統計表示の切り替え
        /// </summary>
        public void SetPerformanceStatsVisible(bool visible)
        {
            showPerformanceStats = visible;
        }
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Performance Manager - Black Onyx Reborn:");
            sb.AppendLine($"Frame Rate: {frameRate:F1} FPS (Target: {targetFrameRate})");
            sb.AppendLine($"Average FPS: {averageFrameRate:F1}");
            sb.AppendLine($"Memory Usage: {currentMetrics.memoryUsage}MB / {memoryThresholdMB}MB");
            sb.AppendLine($"Quality Level: {currentQualityLevel}");
            sb.AppendLine($"Optimization: {(enableOptimization ? "ON" : "OFF")}");
            sb.AppendLine($"Dynamic Quality: {(enableDynamicQuality ? "ON" : "OFF")}");
            sb.AppendLine($"Memory Optimization: {(enableMemoryOptimization ? "ON" : "OFF")}");
            sb.AppendLine($"Profiling: {(enableProfiling ? "ON" : "OFF")}");
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// パフォーマンスメトリクス
    /// </summary>
    [System.Serializable]
    public class PerformanceMetrics
    {
        public float frameRate;
        public float averageFrameRate;
        public long memoryUsage;
        public int drawCalls;
        public int triangles;
        public int batches;
        public float cpuTime;
        public float gpuTime;
    }
    
    /// <summary>
    /// 品質レベル
    /// </summary>
    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Ultra
    }
}