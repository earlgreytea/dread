using UnityEngine;
using Dread.Battle.Path;
using UnityEngine.Splines;

namespace Dread.Battle.Character
{
    /// <summary>
    /// UnityのSplineContainerを使用してスプライン曲線上を等速で移動するコンポーネント
    /// </summary>
    public class SplinePathFollower : MonoBehaviour
    {
        [Header("パス設定")]
        [SerializeField]
        private int pathIndex = -1; // -1の場合はランダム選択

        [SerializeField]
        private bool reverseDirection = false;

        [SerializeField]
        private bool loopPath = false;

        [Header("移動設定")]
        [SerializeField]
        private float moveSpeed = 3f;

        [SerializeField]
        private bool lookForward = true;

        [SerializeField]
        private float rotationSpeed = 10f;

        // 内部状態
        private SplineContainer currentSplineContainer;
        private Spline currentSpline;
        private float currentDistance = 0f;
        private float totalLength = 0f;
        private bool isMoving = false;
        private bool hasReachedEnd = false;

        /// <summary>
        /// 移動速度を取得または設定
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0, value);
        }

        /// <summary>
        /// 移動中かどうかを取得
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// パスの終点に到達したかどうかを取得
        /// </summary>
        public bool HasReachedEnd => hasReachedEnd;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start()
        {
            InitializePath();
        }

        /// <summary>
        /// パスの初期化
        /// </summary>
        public void InitializePath()
        {
            if (SplinePathManager.Instance == null)
            {
                Debug.LogError("SplinePathManagerが見つかりません。");
                return;
            }

            // パスの選択
            if (pathIndex < 0 || pathIndex >= SplinePathManager.Instance.PathCount)
            {
                currentSplineContainer = SplinePathManager.Instance.GetRandomPath();
            }
            else
            {
                currentSplineContainer = SplinePathManager.Instance.GetPath(pathIndex);
            }

            if (currentSplineContainer == null)
            {
                Debug.LogError("有効なパスが見つかりません。");
                return;
            }

            // Splineを取得
            currentSpline = currentSplineContainer.Spline;

            // パスの全長を計算
            totalLength = SplineUtility.CalculateLength(
                currentSpline,
                currentSplineContainer.transform.localToWorldMatrix
            );

            // 初期位置の設定
            currentDistance = reverseDirection ? 1f : 0f; // 正規化された距離（0～1）
            UpdatePosition();
            isMoving = true;
            hasReachedEnd = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update()
        {
            if (!isMoving || currentSpline == null)
                return;

            // パス上の距離を更新（正規化された距離を0～1の間で使用）
            float speedFactor = moveSpeed / totalLength;

            if (reverseDirection)
            {
                currentDistance -= speedFactor * Time.deltaTime;
                if (currentDistance <= 0f)
                {
                    currentDistance = 0f;
                    if (loopPath)
                    {
                        currentDistance = 1f;
                    }
                    else
                    {
                        hasReachedEnd = true;
                        isMoving = false;
                    }
                }
            }
            else
            {
                currentDistance += speedFactor * Time.deltaTime;
                if (currentDistance >= 1f)
                {
                    if (loopPath)
                    {
                        currentDistance = 0f;
                    }
                    else
                    {
                        currentDistance = 1f;
                        hasReachedEnd = true;
                        isMoving = false;
                    }
                }
            }

            // 位置と向きを更新
            UpdatePosition();
        }

        /// <summary>
        /// 位置と向きの更新
        /// </summary>
        private void UpdatePosition()
        {
            if (currentSpline == null || currentSplineContainer == null)
                return;

            // パス上の位置を取得
            float t = Mathf.Clamp01(currentDistance); // 0～1の間の値に制限
            Vector3 newPosition = currentSplineContainer.transform.TransformPoint(
                currentSpline.EvaluatePosition(t)
            );
            transform.position = newPosition;

            // 進行方向を向く
            if (lookForward)
            {
                float lookAheadT = 0.01f; // 少し先を見るための値
                float targetT = reverseDirection
                    ? Mathf.Max(0, t - lookAheadT)
                    : Mathf.Min(1, t + lookAheadT);

                Vector3 lookTarget = currentSplineContainer.transform.TransformPoint(
                    currentSpline.EvaluatePosition(targetT)
                );
                Vector3 direction = lookTarget - newPosition;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        /// <summary>
        /// 移動を開始
        /// </summary>
        public void StartMoving()
        {
            isMoving = true;
        }

        /// <summary>
        /// 移動を停止
        /// </summary>
        public void StopMoving()
        {
            isMoving = false;
        }

        /// <summary>
        /// 現在のパス上の進行度を取得（0.0～1.0）
        /// </summary>
        public float GetNormalizedProgress()
        {
            if (currentSpline == null)
                return 0f;
            return currentDistance; // 既に0～1の正規化された値
        }

        /// <summary>
        /// 新しいパスを設定
        /// </summary>
        public void SetPath(int newPathIndex, bool resetPosition = true)
        {
            pathIndex = newPathIndex;
            if (resetPosition)
            {
                InitializePath();
            }
        }

        /// <summary>
        /// 移動方向を反転
        /// </summary>
        public void ReverseDirection()
        {
            reverseDirection = !reverseDirection;
        }
    }
}
