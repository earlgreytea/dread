using UnityEditor;
using UnityEngine;
using Dread.Battle.Bullet;
using Dread.Battle.Fx;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using PlasticGui.WorkspaceWindow;

namespace Dread.Battle.Turret
{
    // ターゲット選択方法の列挙型
    public enum TargetingMethod
    {
        FirstFound, // 最初に見つけた敵を追跡し続ける
        NearestEnemy, // 常に最も近い敵をターゲットする
        FarthestEnemy // 常に最も遠い敵をターゲットする
    }

    public class TurretLogic : MonoBehaviour
    {
        [SerializeField, ReadOnly, LabelText("タレットモデル本体")]
        TurretView turretView;

        [SerializeField, ReadOnly, LabelText("タレットデータ")]
        TurretData turretData;

        // 親デッキ参照
        [SerializeField, ReadOnly, LabelText("親デッキの参照")]
        TurretDeck turretDeck;

        // 狙いをつける対象
        [SerializeField]
        Transform target;

        // ターゲット選択方法
        [SerializeField, Tooltip("FirstFound: 最初に見つけた敵を追跡し続ける、NearestEnemy: 常に最も近い敵をターゲットする")]
        TargetingMethod targetingMethod = TargetingMethod.NearestEnemy;

        float nextFireTime = 0f; // 次に発射可能になる時間

        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="turretData"></param>
        /// <param name="turretView"></param>
        /// <param name="turretDeck"></param>
        internal void InitialSetup(
            TurretData turretData,
            TurretView turretView,
            TurretDeck turretDeck
        )
        {
            this.turretData = turretData;
            this.turretView = turretView;
            this.turretDeck = turretDeck;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // 発射時間ちょっとブレを与える
            nextFireTime = Random.value;
        }

        // FixedUpdate is called every fixed framerate frame
        void FixedUpdate()
        {
            // ターゲット選択方法に応じて処理を分岐
            switch (targetingMethod)
            {
                // TODO:敵を追跡し続けるメソッドはちょっと待ってね
                case TargetingMethod.FirstFound:
                    break;

                case TargetingMethod.NearestEnemy:
                    // 常に最も近い敵をターゲットする
                    FindNearestTarget();
                    break;
                case TargetingMethod.FarthestEnemy:
                    // 常に最も遠い敵をターゲットする
                    FindFarthestTarget();
                    break;
            }

            if (target != null)
            {
                RotateTurret();

                // 敵が射程内にいる場合のみ発砲
                if (IsTargetInRange())
                {
                    TryFireBullet();
                }
            }
        }

        /// <summary>
        /// 汎用的な距離比較によるターゲット選択
        /// </summary>
        private void FindTargetByDistance(
            System.Func<float, float, bool> compare,
            float initialValue
        )
        {
            if (turretDeck == null)
            {
                target = null;
                return;
            }
            var enemies = turretDeck.GetEnemiesInDefenseRange();
            if (enemies == null || enemies.Count == 0)
            {
                target = null;
                return;
            }
            Transform selectedEnemy = null;
            float bestValue = initialValue;
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (compare(distance, bestValue))
                {
                    bestValue = distance;
                    selectedEnemy = enemy.transform;
                }
            }
            target = selectedEnemy;
        }

        /// <summary>
        /// 常に最も近い敵をターゲットする
        /// </summary>
        private void FindNearestTarget()
        {
            FindTargetByDistance((a, b) => a < b, float.MaxValue);
        }

        /// <summary>
        /// 常に最も遠い敵をターゲットする
        /// </summary>
        private void FindFarthestTarget()
        {
            FindTargetByDistance((a, b) => a > b, float.MinValue);
        }

        // ターゲットに向けて砲塔を回転させる
        void RotateTurret()
        {
            if (turretView == null || target == null)
                return;
            // ターゲットの方向ベクトルを計算
            Vector3 targetDirection = target.position - turretView.GetTurretBase().position;

            // 水平方向の回転処理
            RotateHorizontally(targetDirection);

            // 垂直方向の回転処理
            RotateVertically(targetDirection);
        }

        // 水平方向（Y軸）の回転
        void RotateHorizontally(Vector3 targetDirection)
        {
            if (turretView == null || turretData == null)
                return;
            // XZ平面での方向ベクトルを計算（Y成分を0にする）
            Vector3 horizontalDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);

            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            turretView.RotateBase(targetRotation, turretData.rotationSpeed * Time.deltaTime);
        }

        // 垂直方向（X軸）の回転
        void RotateVertically(Vector3 targetDirection)
        {
            if (turretView == null || turretData == null)
                return;

            Vector3 flatTargetDir = new Vector3(targetDirection.x, 0f, targetDirection.z);
            float flatDist = flatTargetDir.magnitude;
            float verticalAngle = -Mathf.Atan2(targetDirection.y, flatDist) * Mathf.Rad2Deg;

            // 回転制限
            verticalAngle = Mathf.Clamp(
                verticalAngle,
                -turretData.verticalAngleLimit,
                turretData.verticalAngleLimit
            );

            // バレルのローカルX軸のみ回転させる
            Quaternion targetLocalRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
            Transform barrel = turretView.GetBarrel();
            barrel.localRotation = Quaternion.Slerp(
                barrel.localRotation,
                targetLocalRotation,
                turretData.rotationSpeed * Time.deltaTime
            );
        }

        // ターゲットが有効射程内にいるかチェック
        bool IsTargetInRange()
        {
            if (target == null || turretData == null)
                return false;

            // ターゲットとの距離を計算
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // 有効射程内にいるかどうかを返す
            return distanceToTarget <= turretData.effectiveRange;
        }

        // 弾を発射する処理
        void TryFireBullet()
        {
            // 発射レートに基づいて発射判定
            if (Time.time >= nextFireTime)
            {
                FireBullet();
                nextFireTime = Time.time + (1f / turretData.fireRate); // 次の発射可能時間を設定
            }
        }

        // 弾を発射するメソッド
        void FireBullet()
        {
            if (turretView == null)
                return;
            BulletController bulletController = BulletController.Instance;
            if (bulletController != null)
            {
                Vector3 direction = turretView.GetBarrelForward();
                // 偏差射撃有効時はリード計算を行う
                if (turretData.enablePredictiveFire && target != null)
                {
                    if (target.TryGetComponent<Character.Enemy>(out var enemy))
                    {
                        Vector3 toTarget =
                            enemy.transform.position - turretView.GetMuzzle().position;
                        Vector3 targetVelocity = enemy.DeltaPosition / Time.fixedDeltaTime;
                        float bulletSpeed =
                            turretData.bulletParams != null
                                ? turretData.bulletParams.BulletSpeed
                                : 100f;
                        float distance = toTarget.magnitude;
                        float timeToTarget = bulletSpeed > 0 ? distance / bulletSpeed : 0f;
                        Vector3 predictedPosition =
                            enemy.transform.position + targetVelocity * timeToTarget;
                        direction = (
                            predictedPosition - turretView.GetMuzzle().position
                        ).normalized;
                    }
                }
                // 精度による拡散は従来通り
                if (turretData.accuracy < 1.0f)
                {
                    float inaccuracy = 1.0f - turretData.accuracy;
                    float maxSpreadAngle = 10f * inaccuracy;
                    float randomAngleX = Random.Range(-maxSpreadAngle, maxSpreadAngle);
                    float randomAngleY = Random.Range(-maxSpreadAngle, maxSpreadAngle);
                    Quaternion spreadRotation = Quaternion.Euler(randomAngleX, randomAngleY, 0);
                    direction = spreadRotation * direction;
                }

                bulletController.FireBullet(
                    turretView.GetMuzzlePosition(),
                    direction,
                    turretData.bulletParams,
                    BulletOwner.Player
                );
                turretView.PlayMuzzleFlash(direction);
            }
            else
            {
                Debug.LogError("BulletControllerのインスタンスが見つかりません。");
            }
        }

        #region Gizmos
        void OnDrawGizmosSelected()
        {
            // 有効射程範囲のみ描画（見た目はView側で描画済み）
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f); // 青色半透明
            if (turretData != null)
                Gizmos.DrawWireSphere(transform.position, turretData.effectiveRange);
        }

        #endregion
    }
}
