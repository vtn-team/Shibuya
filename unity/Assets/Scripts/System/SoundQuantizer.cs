using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3;

namespace Foundation
{
    /// <summary>
    /// BGMに合わせてゲーム中の行動をクォンタイズするクラス
    /// SEやエフェクト再生のタイミングを調整する
    /// </summary>
    public class SoundQuantizer : MonoBehaviour
    {
        [Header("音楽設定")]
        [SerializeField] private float bpm = 120f;

        private static SoundQuantizer _instance;
        public static SoundQuantizer Instance => _instance;

        private AudioSource _bgmPlayer;
        private float _startTime;
        private bool _isPlaying;

        private readonly List<QuantizeEvent> _events = new List<QuantizeEvent>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private CancellationTokenSource _quantizeCts;

        /// <summary>
        /// クォンタイズイベントの定義
        /// </summary>
        private class QuantizeEvent
        {
            public int beatDivision;
            public float delayTime;
            public Action callback;
            public float scheduledTime;
            public bool isExecuted;

            public QuantizeEvent(int beatDivision, float delayTime, Action callback)
            {
                this.beatDivision = beatDivision;
                this.delayTime = delayTime;
                this.callback = callback;
                this.isExecuted = false;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _quantizeCts?.Cancel();
            _quantizeCts?.Dispose();
        }

        /// <summary>
        /// BGM再生プレイヤーを設定し、クォンタイズ処理を開始する
        /// </summary>
        /// <param name="bgmPlayer">BGM再生用のAudioSource</param>
        public void PlayAndSync(AudioSource bgmPlayer)
        {
            if (bgmPlayer == null)
            {
                Debug.LogError("BGM Player is null");
                return;
            }

            _bgmPlayer = bgmPlayer;
            _startTime = Time.time;
            _isPlaying = true;

            // 既存のクォンタイズ処理をキャンセル
            _quantizeCts?.Cancel();
            _quantizeCts?.Dispose();
            _quantizeCts = new CancellationTokenSource();

            // クォンタイズ処理を開始
            StartQuantizeLoop(_quantizeCts.Token).Forget();

            Debug.Log($"SoundQuantizer started with BPM: {bpm}");
        }

        /// <summary>
        /// イベントをクォンタイズして登録する
        /// </summary>
        /// <param name="callback">実行するコールバック関数</param>
        /// <param name="beatDivision">調整拍数（デフォルト: 16）</param>
        /// <param name="delayTime">ジャストタイミングまでの時間（デフォルト: 0）</param>
        public void Quantize(Action callback, int beatDivision = 16, float delayTime = 0f)
        {
            if (callback == null)
            {
                Debug.LogWarning("Callback is null");
                return;
            }

            if (!_isPlaying)
            {
                Debug.LogWarning("SoundQuantizer is not playing. Call PlayAndSync first.");
                callback?.Invoke();
                return;
            }

            var quantizeEvent = new QuantizeEvent(beatDivision, delayTime, callback);

            // 次のクォンタイズタイミングを計算
            float currentTime = GetCurrentMusicTime();
            float nextQuantizeTime = CalculateNextQuantizeTime(currentTime, beatDivision);
            quantizeEvent.scheduledTime = nextQuantizeTime + delayTime;

            lock (_events)
            {
                _events.Add(quantizeEvent);
            }

            Debug.Log($"Event scheduled for time: {quantizeEvent.scheduledTime} (beat division: {beatDivision})");
        }

        /// <summary>
        /// 現在の音楽時間を取得
        /// </summary>
        private float GetCurrentMusicTime()
        {
            if (_bgmPlayer != null && _bgmPlayer.isPlaying)
            {
                return _bgmPlayer.time;
            }
            return Time.time - _startTime;
        }

        /// <summary>
        /// 次のクォンタイズタイミングを計算
        /// </summary>
        private float CalculateNextQuantizeTime(float currentTime, int beatDivision)
        {
            float beatDuration = 60f / bpm;
            float quantizeDuration = beatDuration * 4f / beatDivision; // 4拍子基準

            float nextQuantizeTime = Mathf.Ceil(currentTime / quantizeDuration) * quantizeDuration;
            return nextQuantizeTime;
        }

        /// <summary>
        /// クォンタイズループ処理
        /// </summary>
        private async UniTask StartQuantizeLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isPlaying)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                float currentTime = GetCurrentMusicTime();

                lock (_events)
                {
                    for (int i = _events.Count - 1; i >= 0; i--)
                    {
                        var evt = _events[i];
                        if (!evt.isExecuted && currentTime >= evt.scheduledTime)
                        {
                            evt.isExecuted = true;
                            evt.callback?.Invoke();
                            _events.RemoveAt(i);
                        }
                    }
                }

                await UniTask.Delay(1, true, PlayerLoopTiming.Update, cancellationToken);
            }
        }

        /// <summary>
        /// 再生を停止
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _quantizeCts?.Cancel();

            lock (_events)
            {
                _events.Clear();
            }

            Debug.Log("SoundQuantizer stopped");
        }

        /// <summary>
        /// 現在のBPMを設定
        /// </summary>
        public void SetBPM(float newBpm)
        {
            if (newBpm <= 0)
            {
                Debug.LogWarning("BPM must be greater than 0");
                return;
            }

            bpm = newBpm;
            Debug.Log($"BPM changed to: {bpm}");
        }

        /// <summary>
        /// 現在登録されているイベント数を取得
        /// </summary>
        public int GetEventCount()
        {
            lock (_events)
            {
                return _events.Count;
            }
        }

        /// <summary>
        /// 再生状態を取得
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 現在のBPMを取得
        /// </summary>
        public float CurrentBPM => bpm;
    }
}