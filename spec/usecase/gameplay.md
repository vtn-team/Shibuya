# インゲーム中に期待される振る舞い
インゲームはInGameシーンを使用する  
InGameシーンの構成については```/spec/scene/InGame.md```を参照すること  


## Feature: プレイヤー入力

#### Scenario: 移動入力の処理
  Given いつでも(1秒に1回きり)
  When 上下左右に対応したアクションキーが入力される  
  Then Playerがアクションをする

#### Scenario: 入力の無効化
  Given ゲームが[ポーズ状態]である  
  When なんらかのキー入力があった  
  Then 入力を処理しない  



## Feature: ゲーム中の接触判定

#### Scenario: 毎フレームの接触判定の起動
  Given 常に  
  When PlayerのUpdateで  
  Then HitDetectorがStageBehaviourから現在の[セル]の情報を取得  
  And 前回のセルと差分があれば判定処理を行う  


## Feature: ゲームの勝利条件と敗北条件
  Given 常に  
  When PlayerのUpdateで  
  Then 横断歩道を走破したらゲームクリア  

  Given 接触判定時  
  When ダメな組み合わせのイベントが発生した 
  Then ゲームオーバー  


