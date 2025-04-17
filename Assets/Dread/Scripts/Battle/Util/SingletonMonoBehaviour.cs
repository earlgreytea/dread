using UnityEngine;

namespace Dread.Battle.Util
{
    /// <summary>
    /// シングルトンパターンを実装したMonoBehaviourの基底クラス
    /// そのシーン内からのアクセスを容易にするためだけの機能であり、
    /// 自動生成やシーン跨ぎなどの機能は提供しません。
    /// </summary>
    /// <typeparam name="T">派生クラスの型</typeparam>
    [DefaultExecutionOrder(-100)] // スクリプト実行順を上げる
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T _instance;

        private static bool isGameQuitting = false;

        /// <summary>
        /// インスタンスを取得します。
        /// シーン内に存在しない場合はnullを返します。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (!isGameQuitting)
                    {
                        Debug.LogError(
                            $"{typeof(T).Name}のインスタンスが存在しません。シーン内に{typeof(T).Name}をアタッチしたゲームオブジェクトを配置してください。"
                        );
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// アプリケーション終了時に終了中状態を設定
        /// </summary>
        private void OnApplicationQuit()
        {
            isGameQuitting = true;
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
