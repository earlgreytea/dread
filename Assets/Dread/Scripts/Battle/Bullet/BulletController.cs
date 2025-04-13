using UnityEngine;
using System.Collections.Generic;
using Dread.Battle.Character;
using System.Linq;
using Dread.Battle.Fx;
using Dread.Battle.Collision;

namespace Dread.Battle.Bullet
{
    /// <summary>
    /// ゲーム中のアクティブな弾配列を管理するクラス
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        // シングルトンインスタンス
        private static BulletController _instance;

        /// <summary>
        /// シングルトンインスタンスへのアクセスプロパティ
        /// </summary>
        public static BulletController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<BulletController>();
                    if (_instance == null)
                    {
                        Debug.LogError("シーン内にBulletControllerが見つかりません。");
                    }
                }
                return _instance;
            }
        }

        // 弾の最大数
        [SerializeField]
        private int maxBullets = 1000;

        // 弾の描画を担当するレンダラー
        [SerializeField]
        private BulletRenderer bulletRenderer;

        // 弾の配列
        private Bullet[] bullets;

        // アクティブな弾の数
        private int activeBulletCount = 0;

        // 使用可能な弾のインデックスを管理するキュー
        private Queue<int> availableBulletIndices;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            // シングルトンの設定
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            // 弾の配列を初期化
            bullets = new Bullet[maxBullets];

            // 使用可能な弾のインデックスを初期化
            availableBulletIndices = new Queue<int>(maxBullets);
            for (int i = 0; i < maxBullets; i++)
            {
                availableBulletIndices.Enqueue(i);
            }

            // レンダラーがアタッチされていない場合は追加
            if (bulletRenderer == null)
            {
                bulletRenderer = GetComponent<BulletRenderer>();
                if (bulletRenderer == null)
                {
                    bulletRenderer = gameObject.AddComponent<BulletRenderer>();
                }
            }

            // レンダラーを初期化
            bulletRenderer.Initialize();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update()
        {
            UpdateBullets();
            CheckCollisions();
            bulletRenderer.UpdateBulletData(bullets, maxBullets);
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        private void LateUpdate()
        {
            bulletRenderer.Render();
        }

        /// <summary>
        /// 弾の位置を更新するメソッド
        /// </summary>
        private void UpdateBullets()
        {
            for (int i = 0; i < maxBullets; i++)
            {
                if (bullets[i].isActive)
                {
                    bool stillActive = bullets[i].Update(Time.deltaTime);

                    // 弾が非アクティブになった場合（寿命切れまたは最大飛距離に達した場合）
                    if (!stillActive)
                    {
                        DeactivateBullet(i);
                    }
                }
            }
        }

        /// <summary>
        /// 弾の衝突判定を行うメソッド
        /// </summary>
        private void CheckCollisions()
        {
            for (int i = 0; i < maxBullets; i++)
            {
                if (bullets[i].isActive)
                {
                    // EnemyControllerを使用した衝突判定
                    if (EnemyController.Instance != null && bullets[i].owner != BulletOwner.Enemy)
                    {
                        Enemy hitEnemy = CheckBulletEnemyCollision(i);
                        if (hitEnemy != null)
                        {
                            // Enemyとの衝突処理
                            HandleBulletEnemyCollision(i, hitEnemy);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 弾とEnemyの衝突判定を行うメソッド
        /// </summary>
        /// <param name="bulletIndex">弾のインデックス</param>
        /// <returns>衝突したEnemy、衝突しなかった場合はnull</returns>
        private Enemy CheckBulletEnemyCollision(int bulletIndex)
        {
            // 弾の情報を取得
            Vector3 bulletPosition = bullets[bulletIndex].position;
            float bulletRadius = bullets[bulletIndex].radius;

            // 敵との衝突判定

            foreach (var enemy in EnemyController.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                    continue;

                // 敵のコリジョン情報を取得
                Vector3 enemyPosition = enemy.CollisionCenter;
                float enemyRadius = enemy.CollisionRadius;

                // CollisionUtilityを使用して衝突判定
                if (
                    CollisionUtility.CheckSphereCollision(
                        bulletPosition,
                        bulletRadius,
                        enemyPosition,
                        enemyRadius
                    )
                )
                {
                    return enemy;
                }
            }

            return null;
        }

        /// <summary>
        /// 弾とEnemyの衝突処理を行うメソッド
        /// </summary>
        private void HandleBulletEnemyCollision(int bulletIndex, Enemy enemy)
        {
            // ダメージを与える処理
            enemy.TakeDamage(bullets[bulletIndex].damage);

            // CollisionUtilityを使用してHitFlashエフェクトを発生させる
            CollisionUtility.EmitHitEffect(
                bullets[bulletIndex].position,
                bullets[bulletIndex].radius,
                bullets[bulletIndex].direction,
                enemy
            );

            // 衝突ログの追加
            Debug.Log($"弾[ID:{bulletIndex}]が{enemy.name}に命中! ダメージ:{bullets[bulletIndex].damage}");

            // 弾の種類に応じた処理
            switch (bullets[bulletIndex].type)
            {
                case BulletType.Normal:
                    // 通常弾は衝突したら消える
                    Debug.Log($"通常弾[ID:{bulletIndex}]が消滅");
                    DeactivateBullet(bulletIndex);
                    break;

                case BulletType.Piercing:
                    // 貫通弾は衝突しても消えない
                    Debug.Log($"貫通弾[ID:{bulletIndex}]が{enemy.name}を貫通");
                    break;

                case BulletType.Explosive:
                    // 爆発弾は爆発効果を発生させて消える
                    Debug.Log($"爆発弾[ID:{bulletIndex}]が爆発! 位置:{bullets[bulletIndex].position}");
                    CreateExplosion(bullets[bulletIndex].position, bullets[bulletIndex].damage);
                    DeactivateBullet(bulletIndex);
                    break;

                default:
                    DeactivateBullet(bulletIndex);
                    break;
            }
        }

        /// <summary>
        /// 爆発効果を生成するメソッド
        /// </summary>
        private void CreateExplosion(Vector3 position, float damage)
        {
            float explosionRadius = 3f;
            if (EnemyController.Instance != null)
            {
                // 爆発範囲内の敵を取得
                List<Enemy> enemiesInRange = EnemyController.Instance.GetEnemiesInRange(
                    position,
                    explosionRadius
                );

                foreach (Enemy enemy in enemiesInRange)
                {
                    if (enemy != null && enemy.IsAlive)
                    {
                        // 距離に応じてダメージを減衰させる
                        float distance = Vector3.Distance(position, enemy.CollisionCenter);
                        float damageMultiplier = 1f - (distance / explosionRadius);
                        enemy.TakeDamage(damage * damageMultiplier);

                        // HitFlashエフェクトを発生させる
                        if (FxEmitter.Instance != null)
                        {
                            // 爆発中心から敵への方向を計算
                            Vector3 hitDirection = (enemy.CollisionCenter - position).normalized;
                            FxEmitter.Instance.EmitByType(
                                FxType.HitFlash,
                                enemy.CollisionCenter,
                                hitDirection
                            );
                        }

                        Debug.Log($"爆発が{enemy.name}にダメージ: {damage * damageMultiplier}");
                    }
                }
            }

            // 爆発エフェクトの生成
            if (FxEmitter.Instance != null)
            {
                // 爆発エフェクトを発生させる
                FxEmitter.Instance.EmitByType(FxType.ExplosionFlash, position, Vector3.up, 5);
            }

            Debug.Log($"爆発発生: {position}");
        }

        // IsInBounds処理は削除し、代わりに弾の最大飛距離に基づいて消滅する処理を実装

        /// <summary>
        /// 新しい弾を生成するメソッド
        /// </summary>
        public void FireBullet(
            Vector3 position,
            Vector3 direction,
            float speed,
            float damage,
            float lifetime,
            float maxDistance,
            float size,
            Color color,
            BulletType type,
            BulletOwner owner
        )
        {
            // 使用可能な弾がない場合は何もしない
            if (availableBulletIndices.Count == 0)
            {
                Debug.LogWarning("使用可能な弾がありません。maxBulletsの値を増やしてください。");
                return;
            }

            // 使用可能な弾のインデックスを取得
            int index = availableBulletIndices.Dequeue();

            // 弾を初期化
            bullets[index].Initialize(
                position,
                direction,
                speed,
                damage,
                lifetime,
                maxDistance,
                size,
                color,
                type,
                owner
            );

            // アクティブな弾の数を増やす
            activeBulletCount++;
        }

        /// <summary>
        /// 弾を非アクティブにするメソッド
        /// </summary>
        private void DeactivateBullet(int index)
        {
            if (bullets[index].isActive)
            {
                bullets[index].Deactivate();
                availableBulletIndices.Enqueue(index);
                activeBulletCount--;
            }
        }

        /// <summary>
        /// すべての弾をクリアするメソッド
        /// </summary>
        public void ClearAllBullets()
        {
            for (int i = 0; i < maxBullets; i++)
            {
                DeactivateBullet(i);
            }

            // 使用可能な弾のインデックスを再初期化
            availableBulletIndices.Clear();
            for (int i = 0; i < maxBullets; i++)
            {
                availableBulletIndices.Enqueue(i);
            }

            activeBulletCount = 0;
        }

        /// <summary>
        /// アクティブな弾の数を取得するプロパティ
        /// </summary>
        public int ActiveBulletCount => activeBulletCount;

        /// <summary>
        /// ギズモ表示用の設定
        /// </summary>
        [Header("ギズモ設定")]
        [SerializeField]
        private bool showBulletGizmos = true;

        [SerializeField]
        private Color normalBulletGizmoColor = Color.yellow;

        [SerializeField]
        private Color piercingBulletGizmoColor = Color.cyan;

        [SerializeField]
        private Color explosiveBulletGizmoColor = Color.red;

        /// <summary>
        /// ギズモ描画処理
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!showBulletGizmos || bullets == null)
                return;

            // アクティブな弾のみを表示
            for (int i = 0; i < maxBullets; i++)
            {
                if (bullets[i].isActive)
                {
                    // 弾の種類によって色を変える
                    Color gizmoColor;
                    switch (bullets[i].type)
                    {
                        case BulletType.Piercing:
                            gizmoColor = piercingBulletGizmoColor;
                            break;
                        case BulletType.Explosive:
                            gizmoColor = explosiveBulletGizmoColor;
                            break;
                        default:
                            gizmoColor = normalBulletGizmoColor;
                            break;
                    }

                    Gizmos.color = gizmoColor;

                    // 弾の位置に球を描画
                    Gizmos.DrawSphere(bullets[i].position, bullets[i].size * 0.5f);

                    // 弾の進行方向を矢印で表示
                    Gizmos.DrawRay(
                        bullets[i].position,
                        bullets[i].direction * bullets[i].speed * 0.2f
                    );

                    // 最大飛距離を点線で表示
                    if (bullets[i].type == BulletType.Explosive)
                    {
                        // 爆発弾の場合は爆発範囲も表示
                        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
                        Gizmos.DrawWireSphere(bullets[i].position, 3f); // 爆発半径
                    }
                }
            }
        }
    }
}
