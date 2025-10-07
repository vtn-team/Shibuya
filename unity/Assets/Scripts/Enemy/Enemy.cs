using UnityEngine;

/// <summary>
/// 敵の行動制御をするベースクラス
/// NOTE: このクラスを派生させて具体的な敵の種類を実装する
/// </summary>
public abstract class Enemy : MonoBehaviour
{
    /// <summary>
    /// 敵の行動を開始する
    /// NOTE: EnemySpawnerから呼び出される外部インタフェース
    /// NOTE: 派生クラスで具体的な行動を実装すること
    /// </summary>
    public abstract void StartAction();
}
