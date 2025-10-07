using Foundation;
using UnityEngine;

/// <summary>
/// 通常の敵クラス
/// NOTE: Enemyベースクラスを継承し、基本的な移動挙動を実装
/// NOTE: SoundQuantizerに登録して拍に合わせて移動する
/// </summary>
public class NormalEnemy : Enemy
{
    #region Inspector Variables

    [Header("Movement Settings")]
    [Tooltip("移動間隔（拍数）")]
    [SerializeField] private int moveBeats = 4;

    [Tooltip("1回の移動距離（メートル）")]
    [SerializeField] private float moveDistance = 1f;

    private SoundQuantizer soundQuantizer;

    #endregion

    #region Private Fields

    /// <summary>
    /// 行動開始済みフラグ
    /// NOTE: StartAction()が複数回呼ばれるのを防ぐ
    /// </summary>
    private bool _actionStarted = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化処理
    /// NOTE: SoundQuantizerの参照を検証
    /// </summary>
    private void Awake()
    {
        // NOTE: SoundQuantizerが設定されていない場合は検索を試みる
        if (soundQuantizer == null)
        {
            soundQuantizer = SoundQuantizer.Instance;

            if (soundQuantizer == null)
            {
                Debug.LogWarning("[NormalEnemy] SoundQuantizerが見つかりません。移動処理が実行されない可能性があります。");
            }
        }
    }

    #endregion

    #region Enemy Override

    /// <summary>
    /// 行動を開始する
    /// NOTE: EnemySpawnerから呼び出される外部インタフェース
    /// NOTE: SoundQuantizerにmoveBeatsの間隔で移動関数を登録
    /// </summary>
    public override void StartAction()
    {
        // NOTE: 既に行動開始済みの場合は何もしない
        if (_actionStarted)
        {
            Debug.LogWarning("[NormalEnemy] StartAction()が既に呼ばれています。");
            return;
        }

        if (soundQuantizer == null)
        {
            Debug.LogError("[NormalEnemy] SoundQuantizerが設定されていないため、行動を開始できません。");
            return;
        }

        // NOTE: SoundQuantizerにmoveBeatsの間隔で移動関数を登録
        soundQuantizer.Quantize(OnMove, moveBeats);

        _actionStarted = true;

        Debug.Log($"[NormalEnemy] 行動を開始しました。移動間隔: {moveBeats}拍");
    }

    #endregion

    #region Movement

    /// <summary>
    /// 移動処理
    /// NOTE: SoundQuantizerから指定拍数ごとに呼び出される
    /// NOTE: Z方向に-1m移動する（プレイヤーに向かって前進）
    /// </summary>
    private void OnMove()
    {
        // NOTE: Z方向（forward方向の逆）に移動距離分移動
        Vector3 moveDirection = new Vector3(0, 0, -1);//-transform.forward;
        transform.position += moveDirection * moveDistance;

        Debug.Log($"[NormalEnemy] 移動しました。新しい位置: {transform.position}");

        // TODO: 画面外やゴールラインを超えたら自動的に破棄する処理を実装

        soundQuantizer.Quantize(OnMove, moveBeats);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 行動を停止する
    /// NOTE: ゲームオーバーやポーズ時に呼び出す
    /// </summary>
    public void StopAction()
    {
        // TODO: SoundQuantizerから登録解除
        // soundQuantizer.Unregister(OnMove);

        _actionStarted = false;

        Debug.Log("[NormalEnemy] 行動を停止しました。");
    }

    /// <summary>
    /// 移動間隔を取得
    /// NOTE: デバッグやテスト用
    /// </summary>
    /// <returns>移動間隔（拍数）</returns>
    public int GetMoveBeats()
    {
        return moveBeats;
    }

    #endregion
}
