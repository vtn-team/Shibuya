# GameManagerクラス設計

# 概要
- ゲーム全体の制御をするクラス

# 実装
- MonoBehaviourを継承する
- SoundQuantizerを所持する
- EnemySpawnerを所持する

# 外部変数
- bpm: BPM
- playerMoveBeats: プレイヤー移動に使用する拍数
- enemySpawnBeats: 敵生成に使用する拍数
- 

# 処理フロー
## 初期化
- SoundQuantizerを初期化してBGMを再生する
- それぞれ外部変数で指定された拍数感覚で、クォンタイザにイベントを登録する
	- 敵の生成
	- プレイヤーの移動

## 敵の生成
- EnemySpawnerのSpawn()を呼び出す

## プレイヤーの移動
- PlayerのMove()を呼び出す

# 期待値
- プレイヤー移動時に敵と当たっているか判定する

# エッジケース
- なし
