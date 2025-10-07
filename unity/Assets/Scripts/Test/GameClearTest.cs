using UnityEngine;
using System.Collections;

/// <summary>
/// 100m Clear Test Class
/// NOTE: Tests if player can reach 100m while avoiding enemies
/// NOTE: Automatically inputs movement every frame to avoid enemies
/// </summary>
public class GameClearTest : MonoBehaviour
{
    #region Inspector Variables

    [Header("Test Settings")]
    [Tooltip("Player to test")]
    [SerializeField] private Player player;

    [Tooltip("Goal distance in meters")]
    [SerializeField] private float goalDistance = 100f;

    [Tooltip("Enemy detection range in meters")]
    [SerializeField] private float enemyDetectionRange = 5f;

    [Tooltip("Run test flag")]
    [SerializeField] private bool runTest = true;

    [Tooltip("Enable auto input")]
    [SerializeField] private bool enableAutoInput = true;

    #endregion

    #region Private Fields

    /// <summary>
    /// Player start position
    /// </summary>
    private Vector3 _startPosition;

    /// <summary>
    /// Test started flag
    /// </summary>
    private bool _testStarted = false;

    /// <summary>
    /// Test completed flag
    /// </summary>
    private bool _testCompleted = false;

    /// <summary>
    /// Test failed flag
    /// </summary>
    private bool _testFailed = false;

    /// <summary>
    /// Current auto input direction
    /// NOTE: Movement direction to avoid enemies
    /// </summary>
    private float _currentAutoInputX = 0f;

    /// <summary>
    /// Last move check time
    /// </summary>
    private float _lastMoveCheckTime = 0f;

    /// <summary>
    /// Move check interval in seconds
    /// </summary>
    private float _moveCheckInterval = 0.1f;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("[GameClearTest] Player not found.");
                return;
            }
        }

        if (runTest)
        {
            StartTest();
        }
    }

    /// <summary>
    /// Update every frame
    /// NOTE: Monitor player and enemy positions, perform auto input
    /// </summary>
    private void Update()
    {
        if (!_testStarted || _testCompleted || _testFailed)
        {
            return;
        }

        // NOTE: Get current player position
        float currentDistance = player.transform.position.z - _startPosition.z;

        // NOTE: Check goal
        if (currentDistance >= goalDistance)
        {
            CompleteTest(true);
            return;
        }

        // NOTE: Perform auto input if enabled
        if (enableAutoInput)
        {
            PerformAutoInput();
        }

        // NOTE: Check collision (simple version)
        CheckCollision();
    }

    #endregion

    #region Test Control

    /// <summary>
    /// Start test
    /// </summary>
    public void StartTest()
    {
        if (_testStarted)
        {
            Debug.LogWarning("[GameClearTest] Test already started.");
            return;
        }

        _startPosition = player.transform.position;
        _testStarted = true;
        _testCompleted = false;
        _testFailed = false;
        _lastMoveCheckTime = Time.time;

        Debug.Log($"[GameClearTest] Test started: Start position={_startPosition}, Goal distance={goalDistance}m");
    }

    /// <summary>
    /// Complete test
    /// </summary>
    /// <param name="success">Success flag</param>
    private void CompleteTest(bool success)
    {
        if (_testCompleted || _testFailed)
        {
            return;
        }

        float finalDistance = player.transform.position.z - _startPosition.z;

        if (success)
        {
            _testCompleted = true;
            Debug.Log($"[GameClearTest] *** TEST SUCCESS *** - 100m cleared! Final distance: {finalDistance:F2}m");

            // NOTE: Stop playback in Unity Editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // NOTE: Quit application with success code 0 in build
            Application.Quit(0);
            #endif
        }
        else
        {
            _testFailed = true;
            Debug.LogError($"[GameClearTest] XXX TEST FAILED XXX - Collision with enemy. Distance: {finalDistance:F2}m");

            // NOTE: Stop playback in Unity Editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // NOTE: Quit application with failure code 1 in build
            Application.Quit(1);
            #endif
        }
    }

    #endregion

    #region Auto Input

    /// <summary>
    /// Perform auto input
    /// NOTE: Detect enemy position and input movement to avoid
    /// </summary>
    private void PerformAutoInput()
    {
        // NOTE: Perform move check at intervals
        if (Time.time - _lastMoveCheckTime < _moveCheckInterval)
        {
            return;
        }

        _lastMoveCheckTime = Time.time;

        // NOTE: Detect enemies ahead
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float nearestEnemyDistance = float.MaxValue;
        float nearestEnemyX = 0f;

        foreach (Enemy enemy in enemies)
        {
            // NOTE: Only check enemies ahead of player
            Vector3 toEnemy = enemy.transform.position - player.transform.position;

            // NOTE: Check only enemies in forward direction (Z)
            if (toEnemy.z > 0 && toEnemy.z < enemyDetectionRange)
            {
                float distance = toEnemy.magnitude;
                if (distance < nearestEnemyDistance)
                {
                    nearestEnemyDistance = distance;
                    nearestEnemyX = enemy.transform.position.x;
                }
            }
        }

        // NOTE: Determine avoidance direction when enemy is close
        if (nearestEnemyDistance < enemyDetectionRange)
        {
            float playerX = player.transform.position.x;

            // NOTE: Move right if enemy is on left, move left if enemy is on right
            if (nearestEnemyX < playerX)
            {
                _currentAutoInputX = 1f; // Move right
                Debug.Log($"[GameClearTest] Enemy detected (left). Avoiding right. Distance: {nearestEnemyDistance:F2}m");
            }
            else if (nearestEnemyX > playerX)
            {
                _currentAutoInputX = -1f; // Move left
                Debug.Log($"[GameClearTest] Enemy detected (right). Avoiding left. Distance: {nearestEnemyDistance:F2}m");
            }
            else
            {
                // NOTE: Random avoidance if enemy is straight ahead
                _currentAutoInputX = Random.value > 0.5f ? 1f : -1f;
                Debug.Log($"[GameClearTest] Enemy detected (front). Random avoidance. Distance: {nearestEnemyDistance:F2}m");
            }
        }
        else
        {
            // NOTE: Go straight if no enemy nearby
            _currentAutoInputX = 0f;
        }

        // NOTE: Simulate input (currently Player class reads from InputSystem directly)
        // TODO: Use direct input injection method if available in Player.cs
        // Currently only recording strategy as Player gets input from InputSystem
    }

    /// <summary>
    /// Check collision
    /// NOTE: Check distance between player and enemies
    /// </summary>
    private void CheckCollision()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            // NOTE: Consider collision if within 1m
            if (distance < 1f)
            {
                Debug.LogError($"[GameClearTest] Collision with enemy! Position: Player={player.transform.position}, Enemy={enemy.transform.position}");
                CompleteTest(false);
                return;
            }
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Get test progress
    /// </summary>
    /// <returns>Progress ratio (0.0-1.0)</returns>
    public float GetProgress()
    {
        if (!_testStarted)
        {
            return 0f;
        }

        float currentDistance = player.transform.position.z - _startPosition.z;
        return Mathf.Clamp01(currentDistance / goalDistance);
    }

    /// <summary>
    /// Check if test is completed
    /// </summary>
    /// <returns>true: completed, false: not completed</returns>
    public bool IsTestCompleted()
    {
        return _testCompleted;
    }

    /// <summary>
    /// Check if test failed
    /// </summary>
    /// <returns>true: failed, false: success or running</returns>
    public bool IsTestFailed()
    {
        return _testFailed;
    }

    #endregion

    #region Debug Display

    /// <summary>
    /// Display debug information
    /// </summary>
    private void OnGUI()
    {
        if (!_testStarted)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        float currentDistance = player.transform.position.z - _startPosition.z;
        float progress = GetProgress() * 100f;

        GUI.Label(new Rect(10, 10, 500, 30), $"Progress: {currentDistance:F2}m / {goalDistance}m ({progress:F1}%)", style);

        if (_testCompleted)
        {
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(10, 40, 500, 30), "*** TEST SUCCESS ***", style);
        }
        else if (_testFailed)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(10, 40, 500, 30), "XXX TEST FAILED XXX", style);
        }
    }

    #endregion
}
