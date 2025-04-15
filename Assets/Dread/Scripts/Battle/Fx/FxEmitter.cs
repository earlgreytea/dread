using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Dread.Battle.Util;

namespace Dread.Battle.Fx
{
    /// <summary>
    /// エフェクトの種類を表す列挙型
    /// </summary>
    public enum FxType
    {
        None = -1,
        ExplosionFlash = 0,
        MuzzleFlash = 1,
        HitFlash = 2,
        Smoke = 3,
        Spark = 4,

        // 必要に応じて追加
        MaxCount = 5 // 常に最大値を設定する
    }

    /// <summary>
    /// エフェクトの発生を司るクラス
    /// 主にParticleSystemの発生などを管理
    /// 子階層に、ParticleSystemオブジェクトを多数持つ
    /// </summary>
    public class FxEmitter : SingletonMonoBehaviour<FxEmitter>
    {
        // エフェクトの種類を直接名前として使用する

        // 実際に使用するParticleSystemのリスト
        [ReadOnly]
        [ShowInInspector]
        [ListDrawerSettings(ShowPaging = true, ShowIndexLabels = true)]
        private List<ParticleSystem> _activeParticleSystems = new List<ParticleSystem>();

        // エフェクト名とParticleSystemのマッピング
        private Dictionary<string, ParticleSystem> _namedParticleSystems =
            new Dictionary<string, ParticleSystem>();

        // タイプ別の高速アクセス用配列
        // 高频度で呼ばれるメソッドのパフォーマンス向上のため
        private ParticleSystem[] _typedParticleSystems;

        // 初期化済みフラグ
        private bool _initialized = false;

        protected override void Awake()
        {
            // シングルトンの初期化を行う
            base.Awake();

            transform.position = Vector3.zero;

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            // ParticleSystemの取得
            CollectParticleSystems();

            // 名前ベースのマッピングを作成
            CreateNameMapping();

            // タイプ別の高速アクセス用配列を初期化
            InitializeTypedArray();

            // 自動発生をオフにする
            DisableAutoPlay();

            _initialized = true;
        }

        /// <summary>
        /// タイプ別の高速アクセス用配列を初期化
        /// </summary>
        private void InitializeTypedArray()
        {
            // FxTypeの最大値に基づいて配列を初期化
            int maxTypeCount = (int)FxType.MaxCount + 1;
            _typedParticleSystems = new ParticleSystem[maxTypeCount];

            // 各タイプに対応するParticleSystemを配列に設定
            foreach (FxType fxType in System.Enum.GetValues(typeof(FxType)))
            {
                if (fxType == FxType.None || fxType == FxType.MaxCount)
                    continue;

                int typeIndex = (int)fxType;
                string typeName = fxType.ToString();

                if (_namedParticleSystems.TryGetValue(typeName, out ParticleSystem ps))
                {
                    _typedParticleSystems[typeIndex] = ps;
                }
            }
        }

        /// <summary>
        /// ParticleSystemを取得する
        /// </summary>
        private void CollectParticleSystems()
        {
            _activeParticleSystems.Clear();

            // 子階層のParticleSystemをすべて取得
            GetComponentsInChildren(true, _activeParticleSystems);
        }

        /// <summary>
        /// 名前ベースのマッピングを作成
        /// </summary>
        private void CreateNameMapping()
        {
            _namedParticleSystems.Clear();

            // 各ParticleSystemを名前で登録
            foreach (var ps in _activeParticleSystems)
            {
                if (ps != null)
                {
                    _namedParticleSystems[ps.gameObject.name] = ps;
                }
            }

            // FxTypeの値を文字列化して検索
            foreach (FxType fxType in System.Enum.GetValues(typeof(FxType)))
            {
                if (fxType == FxType.None || fxType == FxType.MaxCount)
                    continue;

                string typeName = fxType.ToString();

                // 同名のParticleSystemがあればそれを使用
                if (_namedParticleSystems.ContainsKey(typeName))
                {
                    continue;
                }

                // 名前を含むParticleSystemを探す
                foreach (var ps in _activeParticleSystems)
                {
                    if (ps != null && ps.gameObject.name.Contains(typeName))
                    {
                        _namedParticleSystems[typeName] = ps;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 自動発生をオフにする
        /// </summary>
        private void DisableAutoPlay()
        {
            foreach (var ps in _activeParticleSystems)
            {
                if (ps != null)
                {
                    // 姿勢や座標をリセット
                    ps.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                    var main = ps.main;
                    main.playOnAwake = false;
                    ps.Stop(true);

                    // Emission を取得
                    var emission = ps.emission;

                    // Emission をオフにする
                    emission.enabled = false;
                }
            }
        }

        /// <summary>
        /// 指定したインデックスのParticleSystemを発生させる
        /// </summary>
        /// <param name="index">ParticleSystemのインデックス</param>
        /// <param name="count">発生させるパーティクル数</param>
        /// <returns>成功したかどうか</returns>
        private bool EmitAt(int index, int count = 1)
        {
            if (!_initialized)
                Initialize();

            if (index >= 0 && index < _activeParticleSystems.Count)
            {
                var ps = _activeParticleSystems[index];
                if (ps != null && ps.gameObject.activeSelf)
                {
                    ps.Emit(count);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定したタイプのParticleSystemを発生させる
        /// </summary>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool EmitByType(FxType type, Vector3 position, Vector3 velocity, int count = 1)
        {
            if (!_initialized)
                Initialize();

            int typeIndex = (int)type;
            if (typeIndex >= 0 && typeIndex < _typedParticleSystems.Length)
            {
                var ps = _typedParticleSystems[typeIndex];
                if (ps != null && ps.gameObject.activeSelf)
                {
                    // EmitParams を作成
                    var emitParams = new ParticleSystem.EmitParams
                    {
                        position = position,
                        applyShapeToPosition = true,
                        // 方向を指定します
                        velocity = velocity
                    };

                    ps.Emit(emitParams, count);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定した名前のParticleSystemを発生させる
        /// </summary>
        /// <param name="name">ParticleSystemの名前</param>
        /// <param name="count">発生させるパーティクル数</param>
        /// <returns>成功したかどうか</returns>
        private bool EmitByName(string name, int count = 1)
        {
            if (!_initialized)
                Initialize();

            if (_namedParticleSystems.TryGetValue(name, out ParticleSystem ps))
            {
                if (ps != null && ps.gameObject.activeSelf)
                {
                    ps.Emit(count);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定したインデックスのParticleSystemを取得
        /// </summary>
        /// <param name="index">インデックス</param>
        /// <returns>ParticleSystem（存在しない場合はnull）</returns>
        private ParticleSystem GetParticleSystemAt(int index)
        {
            if (!_initialized)
                Initialize();

            if (index >= 0 && index < _activeParticleSystems.Count)
            {
                return _activeParticleSystems[index];
            }
            return null;
        }

        /// <summary>
        /// 指定したエフェクトタイプのParticleSystemを取得
        /// </summary>
        /// <param name="type">エフェクトタイプ</param>
        /// <returns>ParticleSystem（存在しない場合はnull）</returns>
        private ParticleSystem GetParticleSystemByType(FxType type)
        {
            if (!_initialized)
                Initialize();

            int typeIndex = (int)type;
            if (typeIndex >= 0 && typeIndex < _typedParticleSystems.Length)
            {
                return _typedParticleSystems[typeIndex];
            }
            return null;
        }

        /// <summary>
        /// 指定した名前のParticleSystemを取得
        /// </summary>
        /// <param name="name">ParticleSystemの名前</param>
        /// <returns>ParticleSystem（存在しない場合はnull）</returns>
        private ParticleSystem GetParticleSystemByName(string name)
        {
            if (!_initialized)
                Initialize();

            if (_namedParticleSystems.TryGetValue(name, out ParticleSystem ps))
            {
                return ps;
            }
            return null;
        }

        /// <summary>
        /// 管理しているParticleSystemの数を取得
        /// </summary>
        /// <returns>ParticleSystemの数</returns>
        private int GetParticleSystemCount()
        {
            if (!_initialized)
                Initialize();
            return _activeParticleSystems.Count;
        }
    }
}
