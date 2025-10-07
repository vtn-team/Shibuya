using UnityEngine;

namespace Shibuya
{
    /// <summary>
    /// 敵の配置をするクラス
    /// NOTE: GameManagerから呼び出されて敵を一列に生成する
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        #region Inspector Variables

        [Header("References")]
        [Tooltip("GameManagerの参照")]
        [SerializeField] private GameManager gameManager;

        [Header("Enemy Settings")]
        [Tooltip("生成する敵のプレハブ")]
        [SerializeField] private Enemy enemyPrefab;

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
                Debug.LogWarning("GameManager reference is not set in EnemySpawner.");
                return;
            }

            if (enemyPrefab == null)
            {
                Debug.LogWarning("Enemy prefab is not set in EnemySpawner.");
                return;
            }

            // NOTE: GameManagerから生成パラメータを取得
            int cellCount = gameManager.GetCellCount();
            float spawnInterval = gameManager.GetSpawnInterval();
            float spawnProbability = gameManager.GetSpawnProbability();

            // NOTE: このGameObjectが向いている向きを取得
            Vector3 spawnDirection = transform.forward;

            // NOTE: 1列ぶん敵を生成
            for (int i = 0; i < cellCount; i++)
            {
                // NOTE: 生成確率に基づいてランダムに敵を生成するか判定
                if (Random.value <= spawnProbability)
                {
                    // NOTE: 配置間隔分開けて位置を計算
                    Vector3 spawnPosition = transform.position + spawnDirection * (spawnInterval * i);

                    // NOTE: 敵を生成
                    Enemy spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);

                    // NOTE: 生成した敵に行動開始命令を投げる
                    spawnedEnemy.StartAction();
                }
            }
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
                Debug.LogError("GameManager is not assigned to EnemySpawner. Please assign it in the Inspector.");
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned to EnemySpawner. Please assign it in the Inspector.");
            }
        }

        #endregion
    }
}
