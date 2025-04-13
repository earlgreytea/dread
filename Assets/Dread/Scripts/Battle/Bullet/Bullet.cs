using UnityEngine;
using System.Runtime.InteropServices;

namespace Dread.Battle.Bullet
{
    /// <summary>
    /// 弾のデータを保持するクラス。MonoBehaviourではなく、単純なデータ構造として実装。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Bullet
    {
        // 弾の基本プロパティ
        public Vector3 position; // 現在位置
        public Vector3 initialPosition; // 初期位置
        public Vector3 direction; // 進行方向
        public Vector3 velocity; // 速度ベクトル
        public float speed; // 速さ
        public float damage; // 与えるダメージ量
        public float lifetime; // 残存時間
        public float maxLifetime; // 最大残存時間
        public float maxDistance; // 最大飛距離
        public float size; // 弾のサイズ
        public Color color; // 弾の色
        public bool isActive; // アクティブ状態

        // 弾の種類
        public BulletType type;

        // 所有者情報（プレイヤーの弾か敵の弾か）
        public BulletOwner owner;

        /// <summary>
        /// 弾を初期化するメソッド
        /// </summary>
        public void Initialize(
            Vector3 pos,
            Vector3 dir,
            float spd,
            float dmg,
            float life,
            float maxDist,
            float sz,
            Color col,
            BulletType tp,
            BulletOwner own
        )
        {
            position = pos;
            initialPosition = pos;
            direction = dir.normalized;
            speed = spd;
            velocity = direction * speed;
            damage = dmg;
            lifetime = life;
            maxLifetime = life;
            maxDistance = maxDist;
            size = sz;
            color = col;
            type = tp;
            owner = own;
            isActive = true;
        }

        /// <summary>
        /// 弾の位置を更新するメソッド
        /// </summary>
        /// <param name="deltaTime">フレーム間の経過時間</param>
        /// <returns>弾がまだアクティブかどうか</returns>
        public bool Update(float deltaTime)
        {
            if (!isActive)
                return false;

            // 位置の更新
            position += velocity * deltaTime;

            // 残存時間の更新
            lifetime -= deltaTime;
            if (lifetime <= 0)
            {
                // 寿命切れの場合は非アクティブにするが、DeactivateBulletを呼び出すためにフラグを立てない
                return false;
            }

            // 最大飛距離のチェック
            float distanceTraveled = Vector3.Distance(initialPosition, position);
            if (distanceTraveled >= maxDistance)
            {
                // 最大飛距離に達した場合も非アクティブにするが、DeactivateBulletを呼び出すためにフラグを立てない
                return false;
            }

            return true;
        }

        /// <summary>
        /// 弾が移動した距離を取得するメソッド
        /// </summary>
        public float GetDistanceTraveled()
        {
            return Vector3.Distance(initialPosition, position);
        }

        /// <summary>
        /// 弾の半径を取得するプロパティ
        /// </summary>
        public float radius => size * 0.5f;

        /// <summary>
        /// 弾を非アクティブにするメソッド
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
        }
    }

    /// <summary>
    /// 弾の種類を表す列挙型
    /// </summary>
    public enum BulletType
    {
        Normal, // 通常弾
        Piercing, // 貫通弾
        Explosive, // 爆発弾
        Homing, // 誘導弾
        Laser // レーザー
    }

    /// <summary>
    /// 弾の所有者を表す列挙型
    /// </summary>
    public enum BulletOwner
    {
        Player, // プレイヤーの弾
        Enemy, // 敵の弾
        Neutral // 中立の弾
    }
}
