using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BlackOnyxReborn
{
    /// <summary>
    /// オーディオ管理マネージャー
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        
        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float bgmVolume = 0.7f;
        [SerializeField] private float seVolume = 0.8f;
        [SerializeField] private float fadeTime = 1f;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip titleBGM;
        [SerializeField] private AudioClip dungeonBGM;
        [SerializeField] private AudioClip battleBGM;
        [SerializeField] private AudioClip[] seClips;
        
        // Audio clip dictionary for quick access
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        
        // Current BGM info
        private string currentBGM = "";
        private Coroutine fadeCoroutine;
        
        void Awake()
        {
            InitializeAudioSources();
            LoadAudioClips();
        }
        
        void Start()
        {
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// オーディオソースの初期化
        /// </summary>
        private void InitializeAudioSources()
        {
            // BGM AudioSource setup
            if (bgmSource == null)
            {
                GameObject bgmObj = new GameObject("BGM Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
            }
            
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
            
            // SE AudioSource setup
            if (seSource == null)
            {
                GameObject seObj = new GameObject("SE Source");
                seObj.transform.SetParent(transform);
                seSource = seObj.AddComponent<AudioSource>();
            }
            
            seSource.loop = false;
            seSource.playOnAwake = false;
            seSource.volume = seVolume;
            
            Debug.Log("🔊 Audio sources initialized");
        }
        
        /// <summary>
        /// オーディオクリップの読み込み
        /// </summary>
        private void LoadAudioClips()
        {
            // Register BGM clips
            RegisterClip("title", titleBGM);
            RegisterClip("dungeon", dungeonBGM);
            RegisterClip("battle", battleBGM);
            
            // Register SE clips (check for null array first)
            if (seClips != null)
            {
                for (int i = 0; i < seClips.Length; i++)
                {
                    if (seClips[i] != null)
                    {
                        RegisterClip($"se_{i}", seClips[i]);
                    }
                }
            }
            
            if (audioClips.Count > 0)
            {
                Debug.Log($"🎵 Loaded {audioClips.Count} audio clips");
            }
            else
            {
                Debug.Log($"🎵 No audio clips loaded - See AUDIO_SETUP_GUIDE.md for setup instructions");
            }
        }
        
        /// <summary>
        /// オーディオクリップの登録
        /// </summary>
        private void RegisterClip(string name, AudioClip clip)
        {
            if (clip != null && !audioClips.ContainsKey(name))
            {
                audioClips[name] = clip;
            }
        }
        
        /// <summary>
        /// BGM再生
        /// </summary>
        public void PlayBGM(string bgmName, bool fade = true)
        {
            if (string.IsNullOrEmpty(bgmName) || bgmName == currentBGM)
                return;
            
            if (audioClips.TryGetValue(bgmName, out AudioClip clip))
            {
                if (fade && bgmSource.isPlaying)
                {
                    StartCoroutine(FadeBGM(clip, bgmName));
                }
                else
                {
                    bgmSource.clip = clip;
                    bgmSource.Play();
                    currentBGM = bgmName;
                }
                
                Debug.Log($"🎵 Playing BGM: {bgmName}");
            }
            else
            {
                Debug.Log($"🎵 BGM '{bgmName}' not available - Add audio files to Assets/Audio/BGM/ and assign in AudioManager Inspector");
            }
        }
        
        /// <summary>
        /// BGM停止
        /// </summary>
        public void StopBGM(bool fade = true)
        {
            if (fade && bgmSource.isPlaying)
            {
                StartCoroutine(FadeOutBGM());
            }
            else
            {
                bgmSource.Stop();
                currentBGM = "";
            }
        }
        
        /// <summary>
        /// BGMフェード処理
        /// </summary>
        private IEnumerator FadeBGM(AudioClip newClip, string newBGMName)
        {
            // Fade out current BGM
            yield return StartCoroutine(FadeVolume(bgmSource, 0f, fadeTime * 0.5f));
            
            // Change clip
            bgmSource.clip = newClip;
            bgmSource.Play();
            currentBGM = newBGMName;
            
            // Fade in new BGM
            yield return StartCoroutine(FadeVolume(bgmSource, bgmVolume * masterVolume, fadeTime * 0.5f));
        }
        
        /// <summary>
        /// BGMフェードアウト
        /// </summary>
        private IEnumerator FadeOutBGM()
        {
            yield return StartCoroutine(FadeVolume(bgmSource, 0f, fadeTime));
            bgmSource.Stop();
            currentBGM = "";
        }
        
        /// <summary>
        /// 音量フェード
        /// </summary>
        private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
        {
            float startVolume = source.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }
        
        /// <summary>
        /// 効果音再生
        /// </summary>
        public void PlaySE(string seName)
        {
            if (audioClips.TryGetValue(seName, out AudioClip clip))
            {
                seSource.PlayOneShot(clip, seVolume * masterVolume);
                Debug.Log($"🔊 Playing SE: {seName}");
            }
            else
            {
                Debug.Log($"🔊 SE '{seName}' not available - Add audio files to Assets/Audio/SE/ and assign in AudioManager Inspector");
            }
        }
        
        /// <summary>
        /// 効果音再生（番号指定）
        /// </summary>
        public void PlaySE(int seIndex)
        {
            PlaySE($"se_{seIndex}");
        }
        
        /// <summary>
        /// マスター音量設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// BGM音量設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// SE音量設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 音量設定の適用
        /// </summary>
        private void ApplyVolumeSettings()
        {
            if (bgmSource != null)
                bgmSource.volume = bgmVolume * masterVolume;
            
            if (seSource != null)
                seSource.volume = seVolume * masterVolume;
        }
        
        /// <summary>
        /// 音量設定の保存
        /// </summary>
        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SEVolume", seVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 音量設定の読み込み
        /// </summary>
        public void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
            seVolume = PlayerPrefs.GetFloat("SEVolume", 0.8f);
            ApplyVolumeSettings();
        }
        
        /// <summary>
        /// 全オーディオミュート
        /// </summary>
        public void MuteAll(bool mute)
        {
            if (bgmSource != null)
                bgmSource.mute = mute;
            
            if (seSource != null)
                seSource.mute = mute;
        }
        
        /// <summary>
        /// オーディオの一時停止
        /// </summary>
        public void PauseAll()
        {
            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Pause();
        }
        
        /// <summary>
        /// オーディオの再開
        /// </summary>
        public void ResumeAll()
        {
            if (bgmSource != null && !bgmSource.isPlaying && bgmSource.clip != null)
                bgmSource.UnPause();
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                PauseAll();
            else
                ResumeAll();
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                PauseAll();
            else
                ResumeAll();
        }
        
        /// <summary>
        /// デフォルトオーディオクリップの作成（プロシージャル音声）
        /// </summary>
        private void CreateDefaultAudioClips()
        {
            // Create procedural audio clips for missing sounds
            CreateProceduralTones();
        }
        
        /// <summary>
        /// プロシージャルBGMの再生
        /// </summary>
        private void PlayProceduralBGM(string bgmName)
        {
            // Simple procedural BGM based on name
            float frequency = GetBGMFrequency(bgmName);
            StartCoroutine(GenerateProceduralBGM(frequency));
            currentBGM = bgmName;
        }
        
        /// <summary>
        /// プロシージャルSEの再生
        /// </summary>
        private void PlayProceduralSE(string seName)
        {
            // Simple procedural SE based on name
            float frequency = GetSEFrequency(seName);
            float duration = GetSEDuration(seName);
            StartCoroutine(GenerateProceduralSE(frequency, duration));
        }
        
        /// <summary>
        /// BGM周波数の取得
        /// </summary>
        private float GetBGMFrequency(string bgmName)
        {
            switch (bgmName)
            {
                case "title": return 440f; // A4
                case "dungeon": return 330f; // E4
                case "battle": return 550f; // C#5
                case "colormaze": return 370f; // F#4
                case "heaven": return 660f; // E5
                case "ending": return 880f; // A5
                case "ultimate_ending": return 1100f; // C#6
                default: return 440f;
            }
        }
        
        /// <summary>
        /// SE周波数と持続時間の取得
        /// </summary>
        private float GetSEFrequency(string seName)
        {
            switch (seName)
            {
                case "attack": return 800f;
                case "hit": return 600f;
                case "levelup": return 1200f;
                case "item_get": return 900f;
                case "special_item": return 1500f;
                case "door_open": return 400f;
                case "stairs": return 350f;
                case "trap": return 200f;
                case "heal": return 1000f;
                case "button": return 500f;
                case "menu": return 700f;
                case "error": return 150f;
                case "invisibility": return 1300f;
                case "teleport": return 1800f;
                case "victory": return 2000f;
                case "gameover": return 100f;
                default: return 440f;
            }
        }
        
        /// <summary>
        /// SE持続時間の取得
        /// </summary>
        private float GetSEDuration(string seName)
        {
            switch (seName)
            {
                case "attack": return 0.1f;
                case "hit": return 0.15f;
                case "levelup": return 0.8f;
                case "item_get": return 0.3f;
                case "special_item": return 1.0f;
                case "door_open": return 0.5f;
                case "stairs": return 0.4f;
                case "trap": return 0.2f;
                case "heal": return 0.4f;
                case "button": return 0.05f;
                case "menu": return 0.1f;
                case "error": return 0.3f;
                case "invisibility": return 0.6f;
                case "teleport": return 0.5f;
                case "victory": return 2.0f;
                case "gameover": return 1.5f;
                default: return 0.2f;
            }
        }
        
        /// <summary>
        /// プロシージャルトーンの作成
        /// </summary>
        private void CreateProceduralTones()
        {
            // Create basic procedural audio clips for essential sounds
            // This provides fallback audio when no clips are assigned
        }
        
        /// <summary>
        /// プロシージャルBGMジェネレータ
        /// </summary>
        private IEnumerator GenerateProceduralBGM(float frequency)
        {
            // Simple sine wave BGM generation
            // This is a placeholder - in a real implementation,
            // you would use AudioClip.Create() to generate procedural audio
            yield return null;
        }
        
        /// <summary>
        /// プロシージャルSEジェネレータ
        /// </summary>
        private IEnumerator GenerateProceduralSE(float frequency, float duration)
        {
            // Simple SE generation
            // This is a placeholder - in a real implementation,
            // you would use AudioClip.Create() to generate procedural audio
            yield return new WaitForSeconds(duration);
        }
        
        /// <summary>
        /// フロア別BGMの自動選択（ブラックオニキス準拠）
        /// </summary>
        public void PlayBGMForFloor(int floor)
        {
            string bgmName = "dungeon"; // デフォルト
            
            switch (floor)
            {
                case 2: // 天界
                    bgmName = "heaven";
                    break;
                case 1: // ブラックタワー
                    bgmName = "title";
                    break;
                case -6: // カラー迷路
                    bgmName = "colormaze";
                    break;
                default: // 地下階層
                    bgmName = "dungeon";
                    break;
            }
            
            PlayBGM(bgmName);
        }
        
        /// <summary>
        /// コンバット関連SEの再生
        /// </summary>
        public void PlayCombatSE(string combatAction, bool isPlayer = true)
        {
            switch (combatAction)
            {
                case "attack":
                    PlaySE("attack");
                    break;
                case "hit":
                    PlaySE("hit");
                    break;
                case "victory":
                    PlaySE("victory");
                    break;
                case "defeat":
                    PlaySE("gameover");
                    break;
                case "levelup":
                    PlaySE("levelup");
                    break;
            }
        }
        
        /// <summary>
        /// アイテム関連SEの再生
        /// </summary>
        public void PlayItemSE(string itemAction, bool isSpecialItem = false)
        {
            switch (itemAction)
            {
                case "get":
                    PlaySE(isSpecialItem ? "special_item" : "item_get");
                    break;
                case "use":
                    PlaySE("heal"); // 使用SE
                    break;
                case "equip":
                    PlaySE("button");
                    break;
            }
        }
        
        /// <summary>
        /// ダンジョン関連SEの再生
        /// </summary>
        public void PlayDungeonSE(string dungeonAction)
        {
            switch (dungeonAction)
            {
                case "stairs":
                    PlaySE("stairs");
                    break;
                case "door":
                    PlaySE("door_open");
                    break;
                case "trap":
                    PlaySE("trap");
                    break;
                case "teleport":
                    PlaySE("teleport");
                    break;
                case "invisibility":
                    PlaySE("invisibility");
                    break;
            }
        }
        
        /// <summary>
        /// オーディオデバッグ情報の取得
        /// </summary>
        public string GetAudioDebugInfo()
        {
            return $"Audio Manager - Black Onyx Reborn:\n" +
                   $"Current BGM: {currentBGM}\n" +
                   $"Master Volume: {masterVolume:P0}\n" +
                   $"BGM Volume: {bgmVolume:P0}\n" +
                   $"SE Volume: {seVolume:P0}\n" +
                   $"Loaded Clips: {audioClips.Count}\n" +
                   $"BGM Playing: {(bgmSource?.isPlaying ?? false)}\n" +
                   $"Audio Muted: {(bgmSource?.mute ?? false)}";
        }
    }
    
    /// <summary>
    /// オーディオイベントタイプ（ブラックオニキス用）
    /// </summary>
    public enum AudioEventType
    {
        Combat,
        Item,
        Dungeon,
        UI,
        Special
    }
}