using Foundation;
using UnityEngine;

/// <summary>
/// ゲーム全体の制御をするクラス
/// NOTE: BGMのタイミングに合わせてプレイヤーと敵の行動を制御する
/// </summary>
public partial class GameManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("Components")]
    [Tooltip("サウンドクォンタイザーの参照")]
    [SerializeField] private SoundQuantizer soundQuantizer;

    [Tooltip("敵生成器の参照")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Tooltip("プレイヤーの参照")]
    [SerializeField] private Player player;

    [Header("Timing Settings")]
    [Tooltip("BPM")]
    [SerializeField, Inject] private float bpm = 120f;
    
    [Tooltip("BGM")]
    [SerializeField] private AudioSource bgmAudioSource = null;

    [Tooltip("プレイヤー移動に使用する拍数")]
    [SerializeField, Inject] private int playerMoveBeats = 4;

    [Tooltip("敵生成に使用する拍数")]
    [SerializeField, Inject] private int enemySpawnBeats = 8;

    [Header("Spawn Settings")]
    [Tooltip("1列のセル数")]
    [SerializeField, Inject] private int oneLineCellNum = 5;

    [Tooltip("敵生成間隔(m)")]
    [SerializeField, Inject] private float enemySpawnInterval = 2.0f;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化処理
    /// NOTE: SoundQuantizerを初期化してBGMを再生し、各イベントを登録する
    /// </summary>
    private void Start()
    {
        soundQuantizer.PlayAndSync(bgmAudioSource);

        // NOTE: 敵生成イベントを登録（enemySpawnBeats拍ごとに実行）
        soundQuantizer.Quantize(OnEnemySpawn, enemySpawnBeats);

        // NOTE: プレイヤー移動イベントを登録（playerMoveBeats拍ごとに実行）
        soundQuantizer.Quantize(OnPlayerMove, playerMoveBeats);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 敵の生成処理
    /// NOTE: SoundQuantizerから指定拍数ごとに呼び出される
    /// </summary>
    private void OnEnemySpawn()
    {
        // NOTE: EnemySpawnerのSpawn()を呼び出して敵を生成
        if (enemySpawner != null)
        {
            enemySpawner.Spawn();
        }

        soundQuantizer.Quantize(OnEnemySpawn, enemySpawnBeats);
    }

    /// <summary>
    /// プレイヤーの移動処理
    /// NOTE: SoundQuantizerから指定拍数ごとに呼び出される
    /// TODO: プレイヤー移動時に敵と当たっているか判定する（期待値より）
    /// </summary>
    private void OnPlayerMove()
    {
        // NOTE: PlayerのMove()を呼び出してプレイヤーを移動
        if (player != null)
        {
            player.Move();

            // TODO: プレイヤー移動後に敵との衝突判定を実装
            // CheckPlayerEnemyCollision();
        }

        soundQuantizer.Quantize(OnPlayerMove, playerMoveBeats);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 1列のセル数を取得
    /// NOTE: EnemySpawnerから呼び出される
    /// </summary>
    public int GetCellCount()
    {
        return oneLineCellNum;
    }

    /// <summary>
    /// 配置間隔を取得
    /// NOTE: EnemySpawnerから呼び出される
    /// </summary>
    public float GetSpawnInterval()
    {
        return enemySpawnInterval;
    }

    #endregion
}
