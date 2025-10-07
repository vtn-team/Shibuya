using UnityEngine;

/// <summary>
/// 敵の配置をするクラス
/// NOTE: GameManagerから呼び出されて敵を一列に生成する
/// </summary>
public partial class EnemySpawner : MonoBehaviour
{
    #region Inspector Variables

    [Header("References")]
    [Tooltip("GameManagerの参照")]
    [SerializeField] private GameManager gameManager;

    [Header("Enemy Settings")]
    [Tooltip("生成する敵のプレハブ")]
    [SerializeField] private Enemy enemyPrefab;

    [Tooltip("生成タイミング数")]
    [SerializeField, Inject] private int spawnCount = 10;

    [Tooltip("最大生成数")]
    [SerializeField, Inject] private int maxEnemyNum = 10;

    [Tooltip("生成確率（0.0〜1.0）")]
    [SerializeField, Inject] private float spawnProp = 0.5f;

    private int spawnBeatCount = 0;

    #endregion

    #region Public Interface

    /// <summary>
    /// 敵を生成する
    /// NOTE: GameManagerから呼び出される外部インタフェース
    /// NOTE: このGameObjectが向いている向きに、配置間隔分開けて、ランダムに1列ぶん敵を生成する
    /// </summary>
    public void Spawn()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("[EnemySpawner] GameManager reference is not set.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Enemy prefab is not set.");
            return;
        }

        spawnBeatCount++;
        if(spawnBeatCount < spawnCount)
        {
            return;
        }

        // 生成した敵の総数カウント
        int totalSpawnedCount = 0;

        // NOTE: GameManagerから生成パラメータを取得（1列のセル数と配置間隔）
        int cellCount = gameManager.GetCellCount();
        float spawnInterval = gameManager.GetSpawnInterval();

        // NOTE: このGameObjectが向いている向きを取得
        Vector3 spawnDirection = transform.forward;

        // NOTE: 1列ぶん敵をランダムに生成
        int spawnedInThisRow = 0;
        for (int i = 0; i < cellCount; i++)
        {
            // NOTE: 最大生成数チェック
            if (totalSpawnedCount >= maxEnemyNum)
            {
                Debug.Log($"[EnemySpawner] 生成中に最大生成数に達しました。");
                break;
            }

            // NOTE: 生成確率(spawnProp)に基づいてランダムに敵を生成するか判定
            if (Random.value <= spawnProp)
            {
                // NOTE: 配置間隔分開けて位置を計算
                Vector3 spawnPosition = transform.position + spawnDirection * (spawnInterval * i);

                // NOTE: 敵を生成
                // TODO: あとあと生成する敵の種類を変更できるようにする
                Enemy spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);

                // NOTE: 生成した敵に行動開始命令を投げる
                spawnedEnemy.StartAction();

                // NOTE: 生成カウントをインクリメント
                totalSpawnedCount++;
                spawnedInThisRow++;
            }
        }

        Debug.Log($"[EnemySpawner] {spawnedInThisRow}体の敵を生成しました。総生成数: {totalSpawnedCount}/{maxEnemyNum}");
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化時の検証
    /// NOTE: 必要な参照が設定されているかチェック
    /// </summary>
    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[EnemySpawner] GameManager is not assigned to EnemySpawner. Please assign it in the Inspector.");
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy prefab is not assigned to EnemySpawner. Please assign it in the Inspector.");
        }

        // NOTE: 生成確率の値を検証
        if (spawnProp < 0f || spawnProp > 1f)
        {
            Debug.LogWarning($"[EnemySpawner] 生成確率が範囲外です({spawnProp})。0.0〜1.0の範囲に収めてください。");
            spawnProp = Mathf.Clamp01(spawnProp);
        }
    }

    #endregion
}
