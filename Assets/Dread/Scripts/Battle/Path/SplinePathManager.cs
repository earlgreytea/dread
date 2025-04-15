using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Sirenix.OdinInspector;
using Dread.Battle.Util;

namespace Dread.Battle.Path
{
    /// <summary>
    /// UnityのSplineContainerを使用してスプラインパスを管理するマネージャークラス
    /// </summary>
    public class SplinePathManager : SingletonMonoBehaviour<SplinePathManager>
    {
        [SerializeField, ListDrawerSettings(ShowFoldout = true)]
        private List<SplineContainer> splinePaths = new List<SplineContainer>();

        [SerializeField]
        private bool showGizmos = true;

        [SerializeField]
        private Color pathColor = Color.red;

        /// <summary>
        /// 利用可能なパスの数を取得
        /// </summary>
        public int PathCount => splinePaths.Count;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Awake()
        {
            // シングルトンの初期化を行う
            base.Awake();
        }

        /// <summary>
        /// 新しいスプラインを作成してリストに追加
        /// </summary>
        [Button("\u65b0規スプライン追加", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void AddNewSpline()
        {
            // 新しいスプラインコンテナを作成
            GameObject splineObj = new GameObject($"Spline_{splinePaths.Count}");
            splineObj.transform.SetParent(transform);

            // SplineContainerコンポーネントを追加
            SplineContainer splineContainer = splineObj.AddComponent<SplineContainer>();

            // 初期ポイントを設定
            Spline spline = splineContainer.Spline;
            spline.Clear();

            // デフォルトで直線のスプラインを作成
            spline.Add(new BezierKnot(new Vector3(-5, 0, 0)));
            spline.Add(new BezierKnot(new Vector3(5, 0, 0)));

            // リストに追加
            splinePaths.Add(splineContainer);

            // 作成したスプラインを選択
#if UNITY_EDITOR
            UnityEditor.Selection.activeGameObject = splineObj;
#endif
            Debug.Log($"新しいスプラインが作成されました: {splineObj.name}");
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start()
        {
            // シングルトンの初期化はAwakeで行われるため、ここでは特に何もしない
        }

        /// <summary>
        /// 指定したインデックスのSplineContainerを取得
        /// </summary>
        public SplineContainer GetPath(int index)
        {
            if (index < 0 || index >= splinePaths.Count)
            {
                Debug.LogWarning($"指定されたインデックス {index} のパスは存在しません。");
                return null;
            }
            return splinePaths[index];
        }

        /// <summary>
        /// ランダムなSplineContainerを取得
        /// </summary>
        public SplineContainer GetRandomPath()
        {
            if (splinePaths.Count == 0)
            {
                Debug.LogWarning("利用可能なパスがありません。");
                return null;
            }
            return splinePaths[Random.Range(0, splinePaths.Count)];
        }

        /// <summary>
        /// Gizmoの描画
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGizmos)
                return;

            // 各パスを描画
            foreach (SplineContainer splineContainer in splinePaths)
            {
                if (splineContainer == null)
                    continue;

                // パスを描画（UnityのSplineはデフォルトでシーンビューに表示されるが、ゲームビューでも表示するためにGizmosで描画）
                Gizmos.color = pathColor;

                // スプラインに沿って点を取得して線を描画
                Spline spline = splineContainer.Spline;
                int segmentCount = 50; // 描画精度

                for (int i = 0; i < segmentCount; i++)
                {
                    float t1 = i / (float)segmentCount;
                    float t2 = (i + 1) / (float)segmentCount;

                    Vector3 p1 = splineContainer.transform.TransformPoint(
                        spline.EvaluatePosition(t1)
                    );
                    Vector3 p2 = splineContainer.transform.TransformPoint(
                        spline.EvaluatePosition(t2)
                    );

                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }
}
