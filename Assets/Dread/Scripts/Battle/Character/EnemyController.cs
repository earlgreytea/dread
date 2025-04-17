using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Dread.Battle.Util;

namespace Dread.Battle.Character
{
    /// <summary>
    /// ゲーム中のEnemyを管理するコントローラークラス
    /// </summary>
    public class EnemyController : SingletonMonoBehaviour<EnemyController>
    {
        // 敵キャラクターのリスト
        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true)]
        [FoldoutGroup("登録済みの敵"), LabelText("登録済みの敵一覧")]
        [InfoBox("このリストは自動的に管理されます。直接編集しないでください。")]
        private List<Enemy> enemies = new List<Enemy>();

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Awake()
        {
            // シングルトンの初期化を行う
            base.Awake();

            // シーン内の既存のEnemyを検索して登録
            RegisterExistingEnemies();
        }

        /// <summary>
        /// シーン内に既に配置されているEnemyを検索して登録する
        /// </summary>
        private void RegisterExistingEnemies()
        {
            Enemy[] sceneEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in sceneEnemies)
            {
                RegisterEnemy(enemy);
            }

            Debug.Log($"シーン内の敵を{enemies.Count}体登録しました。");
        }

        /// <summary>
        /// 敵キャラクターを登録するメソッド
        /// </summary>
        public void RegisterEnemy(Enemy enemy)
        {
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                Debug.Log($"敵を登録しました: {enemy.gameObject.name}");
            }
        }

        /// <summary>
        /// 敵キャラクターを登録解除するメソッド
        /// </summary>
        public void UnregisterEnemy(Enemy enemy)
        {
            if (enemies.Contains(enemy))
            {
                enemies.Remove(enemy);
                Debug.Log($"敵の登録を解除しました: {enemy.gameObject.name}");
            }
        }

        /// <summary>
        /// 登録されている敵の数を取得するプロパティ
        /// </summary>
        public int EnemyCount => enemies.Count;

        /// <summary>
        /// 登録されている敵のリストを取得するプロパティ（読み取り専用）
        /// </summary>
        public IReadOnlyList<Enemy> Enemies => enemies.AsReadOnly();

        /// <summary>
        /// 生存しているアクティブな敵のリストを取得するプロパティ
        /// </summary>
        public IEnumerable<Enemy> ActiveEnemies
        {
            get { return enemies.Where(enemy => enemy != null && enemy.IsAlive); }
        }

        /// <summary>
        /// 指定した中心点・半径内にいる敵リストを返す
        /// </summary>
        public List<Enemy> GetEnemiesInSphere(Vector3 center, float radius)
        {
            List<Enemy> result = new List<Enemy>();
            float sqrRadius = radius * radius;
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float sqrDist = (enemy.transform.position - center).sqrMagnitude;
                    if (sqrDist <= sqrRadius)
                    {
                        result.Add(enemy);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 指定した位置から最も近い敵を取得するメソッド
        /// </summary>
        public Enemy GetNearestEnemy(Vector3 position)
        {
            Enemy nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float distance = Vector3.Distance(position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// 指定した位置から指定した範囲内にある敵を取得するメソッド
        /// </summary>
        public List<Enemy> GetEnemiesInRange(Vector3 position, float range)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();

            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float distance = Vector3.Distance(position, enemy.transform.position);
                    if (distance <= range)
                    {
                        enemiesInRange.Add(enemy);
                    }
                }
            }

            return enemiesInRange;
        }

        /// <summary>
        /// すべての敵に対して指定したアクションを実行するメソッド
        /// </summary>
        public void ForEachEnemy(System.Action<Enemy> action)
        {
            foreach (Enemy enemy in enemies.ToArray())
            {
                if (enemy != null)
                {
                    action(enemy);
                }
            }
        }

        /// <summary>
        /// 無効な参照（nullになった敵）をリストから削除するメソッド
        /// </summary>
        public void CleanupEnemyList()
        {
            enemies.RemoveAll(enemy => enemy == null);
        }

        /// <summary>
        /// 更新処理（無効な参照を定期的に削除）
        /// </summary>
        private void FixedUpdate()
        {
            CleanupEnemyList();
        }

        /// <summary>
        /// ギズモ描画処理
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (enemies == null)
                return;

            Gizmos.color = Color.red;

            // 登録されている敵の位置に小さな球体を描画
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
                }
            }
        }
    }
}
