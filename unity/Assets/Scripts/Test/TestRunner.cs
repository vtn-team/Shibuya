using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// テスト実行ランナー
/// NOTE: コマンドラインからUnityのテストを実行するためのクラス
/// </summary>
public class TestRunner
{
    /// <summary>
    /// 100m走破テストを実行
    /// NOTE: コマンドラインから-executeMethod TestRunner.RunGameClearTestで呼び出される
    /// </summary>
    public static void RunGameClearTest()
    {
        Debug.Log("[TestRunner] 100m走破テストを開始します...");

        // NOTE: InGameシーンをロード
        string scenePath = "Assets/Scenes/InGame.unity";

        try
        {
            // NOTE: シーンを開く
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[TestRunner] シーンをロードしました: {scenePath}");

            // NOTE: GameClearTestコンポーネントを探す、なければ作成
            GameClearTest testScript = Object.FindObjectOfType<GameClearTest>();

            if (testScript == null)
            {
                // NOTE: テストオブジェクトを作成
                GameObject testObject = new GameObject("GameClearTest");
                testScript = testObject.AddComponent<GameClearTest>();
                Debug.Log("[TestRunner] GameClearTestコンポーネントを作成しました。");
            }

            // NOTE: テストを実行
            Debug.Log("[TestRunner] テスト実行中...");

            // NOTE: Playモードに入る
            EditorApplication.isPlaying = true;

            // NOTE: テスト完了を待つハンドラを登録
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TestRunner] テスト実行中にエラーが発生しました: {e.Message}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Playモード状態変更時のハンドラ
    /// </summary>
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // NOTE: Playモードを抜けたらテスト終了
            Debug.Log("[TestRunner] テストが完了しました。");

            // NOTE: ハンドラを解除
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            // NOTE: 成功として終了
            EditorApplication.Exit(0);
        }
    }
}
