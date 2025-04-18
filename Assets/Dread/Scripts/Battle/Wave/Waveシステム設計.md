

# WaveController
- 敵ウェーブ管理システムの根幹クラス。
- SingletonMonoBehaviour継承。
- WavesScenario（ScriptableObject）を読み込み、EnemySpawnerを管理（生成・削除）する。
- EnemySpawnerを生成する際、WaveScenarioの一部（EnemyData,発生数、インターバル）を渡す。
- ウェーブ進行はコルーチン（WaveRoutine）で管理。
- コルーチン多重起動防止のため、`isWaveRunning`フラグと`Coroutine`参照を保持し、実行中はStartWave()を再度呼んでも無効。
- Awake時にWavesScenarioのバリデーション（ValidateScenario）を実行し、内容に不備があれば警告を出力。
- ウェーブ終了時にフラグ・コルーチン参照をリセット。
- Spawnerの生成・削除はリストで管理し、不要になったらDestroy。

# EnemySpawner
- 敵スポナークラス。
- WaveControllerによってインスタンス化される。
- WaveControllerから、WaveScenarioの一部（EnemyData,発生数、インターバル）を受け取り、それに従って敵を発生させる。
- （この機能は保留）WaveControllerが存在しなくても、EnemyDataが参照されていればヘッドレスで動作する。

# SplinePathManager
- 敵スポナー、および発生した敵が使用するスプラインパスを管理する。
- SingletonMonoBehaviour継承。
- 既に定義済みのパスを参照するためだけの存在。WaveControllerとはほぼ関わりが無い。


# WavesScenario
- ステージに発生する敵ウェーブ全体の情報をScriptableObjectとして管理。
- 順番にウェーブを開始する。
- 各ウェーブの開始タイミングを制御する。
- `ValidateScenario()`メソッドでデータのバリデーションが可能。
    - duration<=0、SpawnInfo.Count<=0、Interval<0、リストが空/null等を検出し、警告リストを返す。

# WaveContent
- 1ウェーブの内容を定義したクラス。
- 敵の種類（EnemyData）、発生数、インターバルを１セットとして管理し、複数セットを配置する。

# EnemyData
- 敵の種類を定義したクラス。


