using UnityEngine;
using Foundation;

/// <summary>
/// SoundQuantizerクラスのテスト用クラス
/// クリックイベントをクォンタイズして実行の確認を行う
/// </summary>
public class QuantizeTest : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField, Tooltip("テスト用のBGM AudioSource")]
    private AudioSource bgmAudioSource;

    [SerializeField, Tooltip("テスト用のBPM設定")]
    private float testBpm = 120f;

    [SerializeField, Tooltip("調整拍数（デフォルト: 16）")]
    private int beatDivision = 16;

    [SerializeField, Tooltip("ジャストタイミングまでの遅延時間（デフォルト: 0）")]
    private float delayTime = 0f;


    [Header("テスト状態")]
    [SerializeField, Tooltip("クリック回数カウンター")]
    private int clickCount = 0;

    [SerializeField, Tooltip("クォンタイズされたイベント実行回数")]
    private int quantizedEventCount = 0;

    // NOTE: SoundQuantizerのインスタンス参照
    private SoundQuantizer soundQuantizer;

    // NOTE: 初期化フラグ
    private bool isInitialized = false;

    /// <summary>
    /// 初期化処理
    /// SoundQuantizerのコールバックを登録
    /// </summary>
    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// SoundQuantizerの初期化とセットアップ
    /// </summary>
    private void Initialize()
    {
        // NOTE: SoundQuantizerのインスタンスを取得または作成
        soundQuantizer = SoundQuantizer.Instance;

        if (soundQuantizer == null)
        {
            // NOTE: SoundQuantizerが存在しない場合は新規作成
            GameObject quantizerObject = new GameObject("SoundQuantizer");
            soundQuantizer = quantizerObject.AddComponent<SoundQuantizer>();
            Debug.Log("QuantizeTest: SoundQuantizerを新規作成しました");
        }

        // NOTE: BGM AudioSourceが設定されていない場合は作成
        if (bgmAudioSource == null)
        {
            GameObject audioObject = new GameObject("TestBGM");
            audioObject.transform.SetParent(transform);
            bgmAudioSource = audioObject.AddComponent<AudioSource>();

            // NOTE: テスト用のAudioSource設定
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = 0.1f; // NOTE: テスト用に音量を下げる

            Debug.Log("QuantizeTest: テスト用BGM AudioSourceを作成しました");
        }

        // NOTE: BPMを設定
        soundQuantizer.SetBPM(testBpm);

        // NOTE: 初期状態の表示
        Debug.Log($"QuantizeTest: 初期化完了 - BPM: {testBpm}, BeatDivision: {beatDivision}, DelayTime: {delayTime}");

        isInitialized = true;

        ToggleBGM();
    }

    /// <summary>
    /// Update処理でクリック入力を監視
    /// </summary>
    void Update()
    {
        if (!isInitialized) return;

        // NOTE: マウスクリックまたはスペースキーでClickEventを実行
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            ClickEvent();
        }

        // NOTE: BGMの再生/停止をKキーで制御
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleBGM();
        }

        // NOTE: 設定変更のためのキー入力
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetBeatDivision(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetBeatDivision(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetBeatDivision(16);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetBeatDivision(32);
        }
    }

    /// <summary>
    /// クリックイベント
    /// クォンタイズされてイベントが発火する
    /// </summary>
    [ContextMenu("ClickEvent - クリックイベント実行")]
    public void ClickEvent()
    {
        if (!isInitialized)
        {
            Debug.LogError("QuantizeTest: 初期化されていません");
            return;
        }

        if (soundQuantizer == null)
        {
            Debug.LogError("QuantizeTest: SoundQuantizerが無効です");
            return;
        }

        clickCount++;

        // NOTE: クォンタイズされたコールバックを登録
        soundQuantizer.Quantize(() => {
            OnQuantizedEvent();
        }, beatDivision, delayTime);

        Debug.Log($"QuantizeTest [ClickEvent]: クリック#{clickCount} - クォンタイズイベントを登録しました");
        Debug.Log($"QuantizeTest [ClickEvent]: 現在の登録イベント数: {soundQuantizer.GetEventCount()}");
    }

    /// <summary>
    /// クォンタイズされたイベントのコールバック
    /// </summary>
    private void OnQuantizedEvent()
    {
        quantizedEventCount++;

        Debug.Log($"★ QuantizeTest [OnQuantizedEvent]: クォンタイズイベント実行! #{quantizedEventCount}");
        Debug.Log($"★ QuantizeTest [OnQuantizedEvent]: 実行時刻: {Time.time:F3}秒");

    }

    /// <summary>
    /// BGMの再生/停止を切り替え
    /// </summary>
    [ContextMenu("ToggleBGM - BGM再生切替")]
    public void ToggleBGM()
    {
        if (!isInitialized)
        {
            Debug.LogError("QuantizeTest: 初期化されていません");
            return;
        }

        if (bgmAudioSource == null)
        {
            Debug.LogError("QuantizeTest: BGM AudioSourceが無効です");
            return;
        }

        if (soundQuantizer.IsPlaying)
        {
            // NOTE: 停止処理
            soundQuantizer.Stop();
            bgmAudioSource.Stop();
            Debug.Log("QuantizeTest [ToggleBGM]: BGMを停止しました");
        }
        else
        {
            // NOTE: 再生開始処理
            bgmAudioSource.Play();
            soundQuantizer.PlayAndSync(bgmAudioSource);
            Debug.Log("QuantizeTest [ToggleBGM]: BGMを再生開始しました");
        }
    }

    /// <summary>
    /// 拍数分割設定を変更
    /// </summary>
    /// <param name="newBeatDivision">新しい拍数分割値</param>
    public void SetBeatDivision(int newBeatDivision)
    {
        if (newBeatDivision <= 0)
        {
            Debug.LogWarning("QuantizeTest: BeatDivisionは1以上である必要があります");
            return;
        }

        beatDivision = newBeatDivision;
        Debug.Log($"QuantizeTest [SetBeatDivision]: 拍数分割を{beatDivision}に変更しました");
    }

    /// <summary>
    /// 遅延時間設定を変更
    /// </summary>
    /// <param name="newDelayTime">新しい遅延時間</param>
    public void SetDelayTime(float newDelayTime)
    {
        delayTime = Mathf.Max(0f, newDelayTime);
        Debug.Log($"QuantizeTest [SetDelayTime]: 遅延時間を{delayTime:F3}秒に変更しました");
    }

    /// <summary>
    /// BPM設定を変更
    /// </summary>
    /// <param name="newBpm">新しいBPM値</param>
    public void SetBPM(float newBpm)
    {
        if (newBpm <= 0)
        {
            Debug.LogWarning("QuantizeTest: BPMは0より大きい値である必要があります");
            return;
        }

        testBpm = newBpm;

        if (soundQuantizer != null)
        {
            soundQuantizer.SetBPM(testBpm);
        }

        Debug.Log($"QuantizeTest [SetBPM]: BPMを{testBpm}に変更しました");
    }

    /// <summary>
    /// テスト統計情報をリセット
    /// </summary>
    [ContextMenu("ResetStats - 統計リセット")]
    public void ResetStats()
    {
        clickCount = 0;
        quantizedEventCount = 0;
        Debug.Log("QuantizeTest [ResetStats]: 統計情報をリセットしました");
    }

    /// <summary>
    /// 現在のテスト状態を表示
    /// </summary>
    [ContextMenu("ShowTestInfo - テスト情報表示")]
    public void ShowTestInfo()
    {
        Debug.Log("=== QuantizeTest 状態情報 ===");
        Debug.Log($"初期化状態: {(isInitialized ? "完了" : "未完了")}");
        Debug.Log($"BPM設定: {testBpm}");
        Debug.Log($"拍数分割: {beatDivision}");
        Debug.Log($"遅延時間: {delayTime:F3}秒");
        Debug.Log($"クリック回数: {clickCount}");
        Debug.Log($"クォンタイズイベント実行回数: {quantizedEventCount}");

        if (soundQuantizer != null)
        {
            Debug.Log($"SoundQuantizer再生状態: {(soundQuantizer.IsPlaying ? "再生中" : "停止中")}");
            Debug.Log($"登録イベント数: {soundQuantizer.GetEventCount()}");
        }
        else
        {
            Debug.Log("SoundQuantizer: 未初期化");
        }

        if (bgmAudioSource != null)
        {
            Debug.Log($"BGM AudioSource: {(bgmAudioSource.isPlaying ? "再生中" : "停止中")}");
        }
        else
        {
            Debug.Log("BGM AudioSource: 未設定");
        }
    }

    /// <summary>
    /// 自動テストモード（開発用）
    /// </summary>
    [ContextMenu("AutoTest - 自動テスト実行")]
    public void AutoTest()
    {
        if (!isInitialized)
        {
            Debug.LogError("QuantizeTest: 初期化されていません");
            return;
        }

        Debug.Log("QuantizeTest [AutoTest]: 自動テストを開始します...");

        // NOTE: BGMが停止している場合は開始
        if (!soundQuantizer.IsPlaying)
        {
            ToggleBGM();
        }

        // NOTE: 異なる拍数で複数のイベントを登録
        SetBeatDivision(4);
        ClickEvent();

        SetBeatDivision(8);
        ClickEvent();

        SetBeatDivision(16);
        ClickEvent();

        Debug.Log($"QuantizeTest [AutoTest]: 3つのイベントを異なる拍数で登録しました（4拍、8拍、16拍）");
    }

    /// <summary>
    /// UI表示用のGUI
    /// </summary>
    private void OnGUI()
    {
        // NOTE: 簡易的な操作説明を画面に表示
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== QuantizeTest 操作方法 ===");
        GUILayout.Label("マウスクリック/Space: ClickEvent実行");
        GUILayout.Label("Kキー: BGM再生/停止切替");
        GUILayout.Label("1-4キー: 拍数分割変更 (4/8/16/32)");
        GUILayout.Label($"現在の設定: BPM={testBpm}, Beat={beatDivision}, Delay={delayTime:F1}s");
        GUILayout.Label($"統計: Click={clickCount}, Quantized={quantizedEventCount}");

        if (soundQuantizer != null)
        {
            GUILayout.Label($"状態: {(soundQuantizer.IsPlaying ? "再生中" : "停止中")}, Events={soundQuantizer.GetEventCount()}");
        }

        GUILayout.EndArea();
    }
}