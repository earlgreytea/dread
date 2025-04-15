using UnityEditor;
using UnityEngine;
using Dread.Battle.Bullet;
using Dread.Battle.Fx;
using System.Collections.Generic;

namespace Dread.Battle.Turret
{
    // ターゲット選択方法の列挙型
    public enum TargetingMethod
    {
        FirstFound, // 最初に見つけた敵を追跡し続ける（従来の動作）
        NearestEnemy // 常に最も近い敵をターゲットする
    }

    public class Turret : MonoBehaviour
    {
        [SerializeField]
        Transform turretBase; // 水平旋回部分

        [SerializeField]
        Transform barrel; // 垂直旋回部分

        [SerializeField]
        Transform muzzle; // 発射位置

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            FindTarget();

            // 発射時間ちょっとブレを与える
            nextFireTime = Random.value;
        }

        // Update is called once per frame
        void Update()
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
            // ターゲットの方向ベクトルを計算
            Vector3 targetDirection = target.position - turretBase.position;

            // 水平方向の回転処理
            RotateHorizontally(targetDirection);

            // 垂直方向の回転処理
            RotateVertically(targetDirection);
        }

        // 水平方向（Y軸）の回転
        void RotateHorizontally(Vector3 targetDirection)
        {
            // XZ平面での方向ベクトルを計算（Y成分を0にする）
            Vector3 horizontalDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);

            // 回転角度を計算
            float currentAngle = Vector3.SignedAngle(
                turretBase.forward,
                horizontalDirection,
                Vector3.up
            );

            // 360度回転モードか角度制限モードかで処理を分ける
            if (fullRotation || Mathf.Abs(currentAngle) <= horizontalAngleLimit)
            {
                // ターゲット方向への回転を計算
                Quaternion targetRotation = Quaternion.LookRotation(
                    horizontalDirection,
                    Vector3.up
                );

                // 徐々に回転させる
                turretBase.rotation = Quaternion.Slerp(
                    turretBase.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // 垂直方向（X軸）の回転
        void RotateVertically(Vector3 targetDirection)
        {
            // ターゲットの高さ差を計算
            float heightDifference = target.position.y - barrel.position.y;

            // ターゲットまでの水平距離を計算
            float horizontalDistance = new Vector3(
                targetDirection.x,
                0f,
                targetDirection.z
            ).magnitude;

            // 仰角を計算（ラジアン）
            float elevationAngle =
                Mathf.Atan2(heightDifference, horizontalDistance) * Mathf.Rad2Deg;

            // 回転方向を反転（マイナスをかける）
            elevationAngle = -elevationAngle;

            // 角度制限を適用
            elevationAngle = Mathf.Clamp(elevationAngle, -verticalAngleLimit, verticalAngleLimit);

            // 現在の回転から目標の回転へ徐々に変化させる
            Quaternion currentRotation = barrel.localRotation;
            Quaternion targetRotation = Quaternion.Euler(elevationAngle, 0f, 0f);

            // 徐々に回転させる
            barrel.localRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
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
            // BulletControllerのシングルトンインスタンスを取得
            BulletController bulletController = BulletController.Instance;

            if (bulletController != null)
            {
                // 弾の発射方向（バレルの前方向）
                Vector3 direction = barrel.forward;

                // 精度に基づいて射撃方向にランダムなブレを適用
                if (accuracy < 1.0f)
                {
                    // 精度が低いほど大きなブレを発生させる（1.0-accuracy）が大きいほどブレが大きい
                    float inaccuracy = 1.0f - accuracy;
                    // 最大10度までのブレを許容（精度0で10度、精度1で0度）
                    float maxSpreadAngle = 10f * inaccuracy;

                    // ランダムな角度を生成
                    float randomAngleX = Random.Range(-maxSpreadAngle, maxSpreadAngle);
                    float randomAngleY = Random.Range(-maxSpreadAngle, maxSpreadAngle);

                    // 方向ベクトルに回転を適用
                    Quaternion spreadRotation = Quaternion.Euler(randomAngleX, randomAngleY, 0);
                    direction = spreadRotation * direction;
                }

                // 弾を発射
                bulletController.FireBullet(
                    muzzle.position, // 発射位置
                    direction, // 発射方向（ブレが適用された方向）
                    bulletSpeed, // 速度
                    bulletDamage, // ダメージ
                    bulletLifetime, // 生存時間
                    bulletMaxDistance, // 最大飛距離
                    bulletSize, // サイズ
                    Color.white, // 色
                    bulletType, // 種類
                    BulletOwner.Player // 所有者（プレイヤー）
                );

                // マズルフラッシュエフェクトを焚く
                FxEmitter.Instance.EmitByType(FxType.MuzzleFlash, muzzle.position, direction);

                // 発射ログ
                Debug.Log($"砲塔が弾を発射: 位置={muzzle.position}, 方向={direction}");
            }
            else
            {
                Debug.LogError("BulletControllerのインスタンスが見つかりません。");
            }
        }

        #region Gizmos
        void OnDrawGizmos()
        {
            if (barrel != null && muzzle != null)
            {
                // バレルの正面方向に伸びるラインを描画
                Gizmos.color = Color.red;

                // マズルから前方に伸びるライン
                Vector3 lineStart = muzzle.position;
                Vector3 lineEnd = lineStart + barrel.forward * 10f; // 10メートルのライン

                Gizmos.DrawLine(lineStart, lineEnd);

                // ラインの終点に小さな球を描画
                Gizmos.DrawSphere(lineEnd, 0.2f);

                // 有効射程範囲を表すワイヤー球を描画
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f); // 青色半透明
                Gizmos.DrawWireSphere(transform.position, effectiveRange);
            }
        }
        #endregion
    }
}
