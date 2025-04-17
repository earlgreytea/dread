using UnityEditor;
using UnityEngine;
using Dread.Battle.Bullet;
using Dread.Battle.Fx;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Dread.Battle.Turret
{
    // ターゲット選択方法の列挙型
    public enum TargetingMethod
    {
        FirstFound, // 最初に見つけた敵を追跡し続ける（従来の動作）
        NearestEnemy // 常に最も近い敵をターゲットする
    }

    public class TurretLogic : MonoBehaviour
    {
        [SerializeField, ReadOnly, LabelText("タレットモデル本体")]
        TurretView turretView;

        [SerializeField, ReadOnly, LabelText("タレットデータ")]
        TurretData turretData;

        // 狙いをつける対象
        [SerializeField]
        Transform target;

        // ターゲット選択方法
        [SerializeField, Tooltip("FirstFound: 最初に見つけた敵を追跡し続ける、NearestEnemy: 常に最も近い敵をターゲットする")]
        TargetingMethod targetingMethod = TargetingMethod.NearestEnemy;

        [SerializeField]
        float rotationSpeed = 5f; // 回転速度

        [SerializeField]
        float horizontalAngleLimit = 60f; // 水平方向の回転制限（度）

        bool fullRotation = true; // 水平方向に360度回転可能かどうか

        [SerializeField]
        float verticalAngleLimit = 60f; // 垂直方向の回転制限（度）

        // 弾の設定
        [Header("弾の設定")]
        [SerializeField]
        float bulletSpeed = 100f; // 弾の速度

        [SerializeField]
        float bulletDamage = 1f; // 弾のダメージ

        [SerializeField]
        float bulletLifetime = 3f; // 弾の生存時間

        [SerializeField]
        float bulletMaxDistance = 300f; // 弾の最大飛距離

        [SerializeField]
        float bulletSize = 0.3f; // 弾のサイズ

        [Header("有効射程設定")]
        [SerializeField, Tooltip("この距離内に敵が存在する場合のみ発砲します。実際の弾の最大飛距離とは異なります")]
        float effectiveRange = 150f; // 有効射程距離

        [SerializeField]
        BulletType bulletType = BulletType.Normal; // 弾の種類

        [SerializeField]
        float fireRate = 1f; // 1秒あたりの発射回数

        [SerializeField, Range(0f, 1f)]
        float accuracy = 0.5f; // 弾の精度（1.0が完全な精度、0.0が最大のブレ）

        float nextFireTime = 0f; // 次に発射可能になる時間

        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="turretData"></param>
        /// <param name="turretView"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void InitialSetup(TurretData turretData, TurretView turretView)
        {
            if (turretData == null)
                return;
            if (turretView == null)
                return;
            this.turretData = turretData;
            this.turretView = turretView;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            FindTarget();

            // 発射時間ちょっとブレを与える
            nextFireTime = Random.value;
        }

        // FixedUpdate is called every fixed framerate frame
        void FixedUpdate()
        {
            // ターゲット選択方法に応じて処理を分岐
            switch (targetingMethod)
            {
                case TargetingMethod.FirstFound:
                    // 従来の動作：ターゲットがnullの場合のみ新しいターゲットを探す
                    if (target == null)
                    {
                        FindTarget();
                    }
                    break;

                case TargetingMethod.NearestEnemy:
                    // 常に最も近い敵をターゲットする
                    FindNearestTarget();
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

        // 最初に見つけたターゲットを検索するメソッド（従来の動作）
        void FindTarget()
        {
            // Enemyタグを持つオブジェクトを検索
            GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");

            if (enemyObject != null)
            {
                target = enemyObject.transform;
            }
        }

        // 最も近いターゲットを検索するメソッド
        void FindNearestTarget()
        {
            // Enemyタグを持つすべてのオブジェクトを検索
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            if (enemies.Length == 0)
            {
                // 敵が見つからない場合はターゲットをクリア
                target = null;
                return;
            }

            // 最も近い敵を探す
            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }

            // 最も近い敵をターゲットに設定
            target = nearestEnemy;
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
            if (turretView == null)
                return;
            // XZ平面での方向ベクトルを計算（Y成分を0にする）
            Vector3 horizontalDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);

            float currentAngle = Vector3.SignedAngle(
                turretView.GetTurretBase().forward,
                horizontalDirection,
                Vector3.up
            );

            if (fullRotation || Mathf.Abs(currentAngle) <= horizontalAngleLimit)
            {
                Quaternion targetRotation = Quaternion.LookRotation(
                    horizontalDirection,
                    Vector3.up
                );
                turretView.RotateBase(targetRotation, rotationSpeed);
            }
        }

        // 垂直方向（X軸）の回転
        void RotateVertically(Vector3 targetDirection)
        {
            if (turretView == null)
                return;
            float heightDifference = target.position.y - turretView.GetBarrel().position.y;
            float horizontalDistance = new Vector3(
                targetDirection.x,
                0f,
                targetDirection.z
            ).magnitude;
            float elevationAngle =
                Mathf.Atan2(heightDifference, horizontalDistance) * Mathf.Rad2Deg;
            elevationAngle = -elevationAngle;
            elevationAngle = Mathf.Clamp(elevationAngle, -verticalAngleLimit, verticalAngleLimit);
            Quaternion targetRotation = Quaternion.Euler(elevationAngle, 0f, 0f);
            turretView.RotateBarrel(targetRotation, rotationSpeed);
        }

        // ターゲットが有効射程内にいるかチェック
        bool IsTargetInRange()
        {
            if (target == null)
                return false;

            // ターゲットとの距離を計算
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // 有効射程内にいるかどうかを返す
            return distanceToTarget <= effectiveRange;
        }

        // 弾を発射する処理
        void TryFireBullet()
        {
            // 発射レートに基づいて発射判定
            if (Time.time >= nextFireTime)
            {
                FireBullet();
                nextFireTime = Time.time + (1f / fireRate); // 次の発射可能時間を設定
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
                if (accuracy < 1.0f)
                {
                    float inaccuracy = 1.0f - accuracy;
                    float maxSpreadAngle = 10f * inaccuracy;
                    float randomAngleX = Random.Range(-maxSpreadAngle, maxSpreadAngle);
                    float randomAngleY = Random.Range(-maxSpreadAngle, maxSpreadAngle);
                    Quaternion spreadRotation = Quaternion.Euler(randomAngleX, randomAngleY, 0);
                    direction = spreadRotation * direction;
                }
                bulletController.FireBullet(
                    turretView.GetMuzzlePosition(),
                    direction,
                    bulletSpeed,
                    bulletDamage,
                    bulletLifetime,
                    bulletMaxDistance,
                    bulletSize,
                    Color.white,
                    bulletType,
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
            Gizmos.DrawWireSphere(transform.position, effectiveRange);
        }

        #endregion
    }
}
