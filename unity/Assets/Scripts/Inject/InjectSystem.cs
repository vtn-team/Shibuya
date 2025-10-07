using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ========== アトリビュート定義 ==========

/// <summary>
/// フィールドに値を注入するためのアトリビュート
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public string Key { get; }

    public InjectAttribute(string key = null)
    {
        Key = key;
    }
}

/// <summary>
/// InjectMapperでシリアライズ可能なパラメータ用のアトリビュート
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectParamAttribute : PropertyAttribute
{
}

// ========== 依存注入インターフェース ==========

public interface IInjectable
{
    void InjectDependencies();
}

// ========== 基底クラス ==========

public abstract class InjectableMonoBehaviour : MonoBehaviour, IInjectable
{
    protected virtual void Awake()
    {
        InjectDependencies();
    }

    public abstract void InjectDependencies();
}

public static class DIInjector
{
    // ジェネリックメソッドでタイプセーフな注入
    public static void InjectInto<T>(T target) where T : IInjectable
    {
        target.InjectDependencies();
    }
}

// ========== InjectSystem Singleton ==========

/// <summary>
/// 注入用パラメータを保持するシングルトン
/// </summary>
public class InjectSystem : MonoBehaviour
{
    private static InjectSystem _instance;
    private ParamInjectSettings _paramInjectSettings;
    private bool _isInitialized = false;

    public static InjectSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                // シーン内から既存のインスタンスを検索
                _instance = FindFirstObjectByType<InjectSystem>();
                
                if (_instance == null)
                {
                    // 新しいGameObjectを作成してInjectSystemをアタッチ
                    GameObject go = new GameObject("InjectSystem");
                    _instance = go.AddComponent<InjectSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// ParamInjectSettingsを取得
    /// ファイルが存在しない場合はnullを返し、Inject処理をスキップする
    /// </summary>
    public ParamInjectSettings ParamInjectSettings
    {
        get
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return _paramInjectSettings;
        }
    }

    /// <summary>
    /// ParamInjectSettingsが利用可能かどうかを確認
    /// </summary>
    public bool IsParamInjectSettingsAvailable => ParamInjectSettings != null;

    private void Awake()
    {
        // 既に別のインスタンスが存在する場合は自身を破棄
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    /// <summary>
    /// シーン読み込み時またはゲーム起動時にParamInjectSettingsを読み込む
    /// </summary>
    private void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            // まず直接的なパスから読み込みを試す
            _paramInjectSettings = Addressables.LoadAssetAsync<ParamInjectSettings>("Assets/DataAsset/Params/ParamInjectSettings.asset").WaitForCompletion();
            if (_paramInjectSettings == null)
            {
#if UNITY_EDITOR
                // ParamInjectSettingsが見つからない場合、自動生成を試行
                Debug.Log("InjectSystem: ParamInjectSettings not found. Attempting to auto-generate...");
                _paramInjectSettings = CreateDefaultParamInjectSettings();

                if (_paramInjectSettings != null)
                {
                    Debug.Log("InjectSystem: Successfully auto-generated ParamInjectSettings");
                }
                else
                {
                    Debug.LogWarning("InjectSystem: Failed to auto-generate ParamInjectSettings");
                }
#else
                Debug.LogWarning("InjectSystem: ParamInjectSettings not found. Auto-generation is only available in Editor mode.");
#endif
            }
            else
            {
                Debug.Log($"InjectSystem: Loaded ParamInjectSettings successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InjectSystem: Failed to load ParamInjectSettings: {e.Message}");
        }

        _isInitialized = true;
    }

    /// <summary>
    /// パラメータ設定を手動で再読み込み
    /// </summary>
    [ContextMenu("Reload Parameters")]
    public void ReloadParameters()
    {
        _isInitialized = false;
        Initialize();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// デフォルトのParamInjectSettingsを自動生成
    /// </summary>
    private ParamInjectSettings CreateDefaultParamInjectSettings()
    {
        try
        {
            // ディレクトリが存在しない場合は作成
            string directoryPath = "Assets/DataAsset/Params";
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            // ParamInjectSettingsを作成
            ParamInjectSettings settings = ScriptableObject.CreateInstance<ParamInjectSettings>();

            // デフォルト値を設定（必要に応じて調整可能）
            settings.SetAutoGenerate(true);
            settings.SetGeneratedCodePath("Assets/Scripts/Inject/Generated/");

            // アセットとして保存
            string assetPath = "Assets/DataAsset/Params/ParamInjectSettings.asset";
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"InjectSystem: Created default ParamInjectSettings at {assetPath}");

            return settings;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InjectSystem: Failed to create default ParamInjectSettings: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// ParamInjectSettingsを手動で生成（コンテキストメニュー用）
    /// </summary>
    [ContextMenu("Create Default ParamInjectSettings")]
    public void CreateDefaultParamInjectSettingsManual()
    {
        if (_paramInjectSettings != null)
        {
            Debug.LogWarning("InjectSystem: ParamInjectSettings already exists. Delete the existing one first if you want to recreate it.");
            return;
        }

        _paramInjectSettings = CreateDefaultParamInjectSettings();
        if (_paramInjectSettings != null)
        {
            Debug.Log("InjectSystem: Successfully created default ParamInjectSettings manually");
        }
    }
#endif
}