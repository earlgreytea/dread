# スプラインパスシステム

このシステムは、UnityのSplineContainerを使用して敵キャラクターがスプライン曲線上を等速で移動するための機能を提供します。

## 主要コンポーネント

### SplinePathManager

- 複数のSplineContainerを管理するマネージャークラス
- シングルトンパターンで実装されており、ゲーム中のどこからでもアクセス可能
- パスの取得や可視化機能を提供

### SplinePathFollower

- スプライン曲線上を等速で移動するためのコンポーネント
- 移動速度、方向、ループ設定などのカスタマイズが可能
- 進行方向に自動的に向きを変える機能

## 使用方法

1. シーンにSplinePathManagerコンポーネントを追加
2. Unityのスプラインエディタを使用してSplineContainerを作成
3. 作成したSplineContainerをSplinePathManagerのsplinePathsリストに追加
4. 敵オブジェクトにSimpleEnemyコンポーネントを追加し、以下を設定：
   - `useSplinePath`：スプラインパスを使用するかどうか
   - `pathIndex`：使用するパスのインデックス（-1でランダム選択）
   - `reverseDirection`：逆方向に移動するかどうか
   - `loopPath`：パスをループするかどうか
   - `lookForward`：進行方向に向きを変えるかどうか
   - `rotationSpeed`：回転速度

## 注意事項

- SplineContainerを作成するには、Unity 2022.2以降が必要です
- パフォーマンスを考慮して、複雑すぎるパスは避けることをお勧めします
