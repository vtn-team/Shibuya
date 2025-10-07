using UnityEngine;
using UnityEngine.InputSystem;
using R3;

/// <summary>
/// プレイヤーの挙動を統括するクラス
/// NOTE: InputSystemを使用してプレイヤーの入力を管理し、外部から呼び出される移動処理を実行する
/// NOTE: 入力はポーズ状態では無効化される
/// </summary>
public partial class Player : MonoBehaviour
{
    #region Inspector Variables

    [Header("Input Settings")]
    [Tooltip("プレイヤー入力アクション")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Movement Settings")]
    [Tooltip("左右の移動距離（メートル）")]
    [SerializeField, Inject] private float horizontalMoveDistance = 1f;

    [Tooltip("前進の移動距離（メートル）")]
    [SerializeField, Inject] private float forwardMoveDistance = 1f;

    #endregion

    #region Private Fields

    /// <summary>
    /// 現在の移動入力値
    /// NOTE: InputSystemからの入力を保存する
    /// </summary>
    private Vector2 _currentMoveInput = Vector2.zero;

    /// <summary>
    /// ポーズ状態フラグ
    /// NOTE: trueの場合は全ての入力イベントが無効化される
    /// </summary>
    private bool _isPaused = false;

    /// <summary>
    /// 移動アクション
    /// NOTE: InputSystemのMoveアクションへの参照
    /// </summary>
    private InputAction _moveAction;

    /// <summary>
    /// ポーズアクション
    /// NOTE: InputSystemのPauseアクションへの参照
    /// </summary>
    private InputAction _pauseAction;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化処理
    /// NOTE: コンポーネントの初期化とInputSystemのコールバック登録を行う
    /// </summary>
    private void Awake()
    {
        // NOTE: PlayerInputコンポーネントが設定されていない場合は取得を試みる
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();

            if (playerInput == null)
            {
                Debug.LogError("[Player] PlayerInputコンポーネントが見つかりません。インスペクタで設定してください。");
                return;
            }
        }

        // NOTE: InputSystemのアクションを取得
        _moveAction = playerInput.actions["Move"];
        _pauseAction = playerInput.actions["Pause"];

        // NOTE: 移動入力のコールバック登録
        if (_moveAction != null)
        {
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
        }
        else
        {
            Debug.LogError("[Player] Moveアクションが見つかりません。InputActionsに'Move'アクションを設定してください。");
        }

        // NOTE: ポーズ入力のコールバック登録
        if (_pauseAction != null)
        {
            _pauseAction.performed += OnPausePerformed;
        }
        else
        {
            Debug.LogError("[Player] Pauseアクションが見つかりません。InputActionsに'Pause'アクションを設定してください。");
        }
    }

    /// <summary>
    /// 破棄処理
    /// NOTE: InputSystemのコールバックを解除
    /// </summary>
    private void OnDestroy()
    {
        // NOTE: メモリリークを防ぐためにコールバックを解除
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMovePerformed;
            _moveAction.canceled -= OnMoveCanceled;
        }

        if (_pauseAction != null)
        {
            _pauseAction.performed -= OnPausePerformed;
        }
    }

    #endregion

    #region Input Callbacks

    /// <summary>
    /// 移動入力が行われた時の処理
    /// NOTE: ポーズ状態では入力を無視する
    /// </summary>
    /// <param name="context">InputSystemのコンテキスト</param>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // NOTE: ポーズ状態では全ての入力イベントは無効
        if (_isPaused)
        {
            return;
        }

        // NOTE: 入力値を保存（Vector2として取得）
        _currentMoveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// 移動入力がキャンセルされた時の処理
    /// </summary>
    /// <param name="context">InputSystemのコンテキスト</param>
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // NOTE: 入力がない状態に戻す
        _currentMoveInput = Vector2.zero;
    }

    /// <summary>
    /// ポーズ入力が行われた時の処理
    /// NOTE: ポーズ状態を切り替える
    /// </summary>
    /// <param name="context">InputSystemのコンテキスト</param>
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // NOTE: ポーズ状態を切り替え
        _isPaused = !_isPaused;

        Debug.Log($"[Player] ポーズ状態: {(_isPaused ? "ON" : "OFF")}");

        // TODO: ポーズUIの表示/非表示処理を実装
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// プレイヤーの移動処理
    /// NOTE: GameManagerから呼び出される（1秒に1回）
    /// NOTE: 入力があった場合、左右の入力があればその方向に1m移動し、それとあわせて1m前に進む（斜めに進む）
    /// </summary>
    public void Move()
    {
        // NOTE: ポーズ状態では移動しない
        if (_isPaused)
        {
            return;
        }

        // NOTE: 移動ベクトルを計算
        Vector3 moveDirection = Vector3.zero;

        // NOTE: 左右の入力がある場合、その方向に移動距離を加算
        // Input Systemの横軸入力（A/D、左右矢印）
        if (Mathf.Abs(_currentMoveInput.x) > 0.1f)
        {
            float horizontalInput = Mathf.Sign(_currentMoveInput.x);
            moveDirection += Vector3.right * horizontalInput * horizontalMoveDistance;
        }

        // NOTE: 常に前方に移動距離を加算
        // Input Systemの縦軸入力（W、上矢印）があるかどうかに関わらず前進
        moveDirection += Vector3.forward * forwardMoveDistance;

        // NOTE: 実際に移動を実行
        transform.position += moveDirection;

        Debug.Log($"[Player] 移動: {moveDirection}, 新しい位置: {transform.position}");
    }

    /// <summary>
    /// ポーズ状態を設定
    /// NOTE: 外部からポーズ状態を制御する場合に使用
    /// </summary>
    /// <param name="paused">ポーズ状態</param>
    public void SetPaused(bool paused)
    {
        _isPaused = paused;
        Debug.Log($"[Player] ポーズ状態を外部から設定: {(_isPaused ? "ON" : "OFF")}");
    }

    /// <summary>
    /// 現在のポーズ状態を取得
    /// </summary>
    /// <returns>ポーズ状態</returns>
    public bool IsPaused()
    {
        return _isPaused;
    }

    /// <summary>
    /// 現在の移動入力を取得
    /// NOTE: デバッグやテスト用
    /// </summary>
    /// <returns>現在の入力値</returns>
    public Vector2 GetCurrentInput()
    {
        return _currentMoveInput;
    }

    #endregion
}
