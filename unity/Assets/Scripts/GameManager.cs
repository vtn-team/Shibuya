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
    [Tooltip("プレイヤー移動に使用する拍数")]
    [SerializeField, Inject] private int playerMoveBeats = 4;

    [Tooltip("敵生成に使用する拍数")]
    [SerializeField, Inject] private int enemySpawnBeats = 8;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化処理
    /// NOTE: SoundQuantizerを初期化してBGMを再生し、各イベントを登録する
    /// </summary>
    private void Start()
    {
        // TODO: BGMのAudioSourceを取得して再生開始
        // soundQuantizer.PlayAndSync(bgmAudioSource);

        // NOTE: 敵生成イベントを登録（enemySpawnBeats拍ごとに実行）
        // TODO: SoundQuantizer.Quantizeメソッドの実装完了後に登録処理を実装
        // soundQuantizer.Quantize(enemySpawnBeats, 0, OnEnemySpawn);

        // NOTE: プレイヤー移動イベントを登録（playerMoveBeats拍ごとに実行）
        // TODO: SoundQuantizer.Quantizeメソッドの実装完了後に登録処理を実装
        // soundQuantizer.Quantize(playerMoveBeats, 0, OnPlayerMove);
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
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 1列のセル数を取得
    /// NOTE: EnemySpawnerから呼び出される
    /// TODO: 適切な値を設定または外部変数化
    /// </summary>
    public int GetCellCount()
    {
        // TODO: 実際のセル数を返す実装
        return 5; // 仮の値
    }

    /// <summary>
    /// 配置間隔を取得
    /// NOTE: EnemySpawnerから呼び出される
    /// TODO: 適切な値を設定または外部変数化
    /// </summary>
    public float GetSpawnInterval()
    {
        // TODO: 実際の配置間隔を返す実装
        return 2.0f; // 仮の値
    }

    /// <summary>
    /// 生成確率を取得
    /// NOTE: EnemySpawnerから呼び出される
    /// TODO: 適切な値を設定または外部変数化
    /// </summary>
    public float GetSpawnProbability()
    {
        // TODO: 実際の生成確率を返す実装
        return 0.5f; // 仮の値（50%）
    }

    #endregion
}
