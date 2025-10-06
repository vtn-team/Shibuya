# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

Shibuyaは仕様駆動開発で開発されているゲームプロジェクトです。リポジトリ構成:
- `spec/` - 仕様書と設計ドキュメント
- `unity/` - Unityゲームプロジェクト

## ビルドと開発コマンド

### コンパイルチェック
プロジェクトはself-hosted runnerを使用したGitHub ActionsでUnityのコンパイルチェックを実行します:
```bash
# Workflowファイル: .github/workflows/pr-compile-check.yml
# プルリクエスト時と手動実行でトリガー
```

### Unityビルド
コマンドラインからUnityプロジェクトをビルドする方法:
```bash
# Unity Editorから: VTNTools > Build Application
# コマンドラインから（Windows例）:
"C:\Program Files\Unity\Hub\Editor\2022.3.49f1\Editor\Unity.exe" -quit -batchmode -projectPath unity -executeMethod BuildCommand.Build
```

ビルドコマンドのパラメータ:
- `-team <teamID>` - チーム識別子（デフォルト: "Foundation"）
- `-platform <Windows|Android|Mac|Switch>` - ビルドプラットフォーム
- `-devmode <true|false>` - 開発ビルドフラグ
- `-projectPath <path>` - プロジェクトパス

ビルドスクリプトの場所: `unity/Assets/Foundation/BaseSystemEditor/Editor/JenkinsBuild.cs:12`

## アーキテクチャ

### Foundationシステム
プロジェクトは`unity/Assets/Foundation/`にカスタムFoundation層を使用しています:

**BaseSystem** - コアゲームシステム:
- `DataManagement/` - Google Sheets API連携を使用したマスターデータ管理
  - `MasterData.cs` - バージョン管理とキャッシュ機能付きシングルトンマスターデータローダー
  - `MasterData.Implements.cs` - 実際のマスターデータ実装（新しいマスターはここに追加）
  - マスターデータはGoogle Sheets APIから取得（`GameSettings.MasterDataAPIURI`参照）
  - ネットワークリクエストを最小化するためにバージョンチェック付きでローカルキャッシュ

- `Foundation/` - コアユーティリティ:
  - `IO/LocalData.cs` - AES暗号化サポート付きローカルファイルI/O（RELEASEビルドで有効）
  - `Network/` - リトライロジック付きHTTPリクエスト処理（GET/POST）
    - `HTTPRequest.cs` - 5回リトライするMonoBehaviourベースのリクエストワーカー
    - `WebRequest.cs` - リクエスト調整レイヤー

- `Dynamic/BuildState.cs` - ビルド設定状態
- `GameSettings.cs` - シーン管理を含む中央ゲーム設定

**シーン設定**:
シーン設定は`GameSettings.cs:21`のディクショナリで定義されています:
- `"Ingame"` - 通常のゲームプレイ
- `"Ingame_Debug"` - デバッグオーバーレイ付きゲームプレイ
- `"ProgramTest"` - テスト環境
- エディタで現在アクティブなシーンのプレースホルダーとして`@PlayScene`を使用

**サードパーティライブラリ**:
- UniTask - Unity向けのasync/awaitサポート（`Assets/Foundation/UniTask/`に配置）
- SerializableDictionary - Dictionaryシリアライゼーションサポート

### Unityパッケージ
主要な依存関係（`unity/Packages/manifest.json`参照）:
- Unity Addressables (2.4.6) - アセット管理
- AI Navigation (2.0.7) - NavMeshシステム
- Universal Render Pipeline (17.0.4)
- Input System (1.14.0)
- NuGetForUnity - NuGetパッケージ管理

### プロジェクト構造
```
unity/Assets/
├── Foundation/           # コアフレームワーク（チーム内相談なしで変更禁止）
│   ├── BaseSystem/       # ゲームシステム基盤
│   └── BaseSystemEditor/ # エディタユーティリティとビルドツール
├── Scripts/              # ゲーム固有スクリプト
├── Scenes/               # Unityシーン
├── Settings/             # プロジェクト設定と設定ファイル
└── AddressableAssetsData/ # Addressables設定
```

## 開発ノート

### マスターデータワークフロー
1. マスターデータはGoogle Sheetsで定義
2. データは`GameSettings.MasterDataAPIURI`のAPIエンドポイント経由でアクセス
3. 初回アクセスはネットワークから取得、以降の読み込みはローカルキャッシュを使用
4. キャッシュは24時間後、またはサーバーバージョンが新しい場合に無効化
5. 新しいマスターデータを追加する方法:
   - `MasterData.Implements.cs:23`にプロパティを追加
   - `MasterData.Implements.cs:28`の`MasterDataLoad()`メソッドに読み込みタスクを追加

### ビルド定義
- `RELEASE` - ローカルデータストレージのAES暗号化を有効化
- 開発ビルドには`BuildOptions.Development | BuildOptions.AllowDebugging`を含む

### カスタム属性
- `SubclassSelectorAttribute` - サブクラス選択用のインスペクタードロップダウン
- `InspectorVariantNameAttribute` - インスペクタでのカスタム命名

### データ永続化
- 開発環境: データは`Application.dataPath/StreamingAssets`に保存（デバッグ用に可視化）
- 本番環境: セキュアストレージには`Application.persistentDataPath`の使用を検討
- AES暗号化キーとIVは`LocalData.cs:206-207`にハードコード（本番環境では外部化を検討）



# 共通命令
ここに記載された内容について{}で囲われた関連するキーワードを受け取った際に適宜実行すること。

## プロンプト実行時(非コマンド命令時)
1. プロンプトと実行時のサマリを`/log/`に保存する
	- ログ名は日付形式 (yyyy-mm-dd.md)
	- ファイルは必ず追記すること

## ログ出力
1. 以下の内容を`/log/`に追記保存する
	1. 最終更新日時：[実行日時]
	2. 仕様書の更新内容
	3. 更新したファイルの情報
	- ログ名は日付形式 (yyyy-mm-dd.md)
	- ファイルは必ず追記すること

