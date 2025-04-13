using UnityEngine;
using Dread.Battle.Character;
using Dread.Battle.Fx;

namespace Dread.Battle.Collision
{
    /// <summary>
    /// コリジョンパラメータを格納する構造体
    /// </summary>
    [System.Serializable]
    public struct CollisionParameters
    {
        [Tooltip("コリジョンの半径")]
        public float radius;

        [Tooltip("コリジョンの中心位置のオフセット")]
        public Vector3 offset;
    }

    /// <summary>
    /// 衝突処理に関するユーティリティクラス
    /// </summary>
    public static class CollisionUtility
    {
        /// <summary>
        /// 2つの球体コリジョン間の衝突判定を行う
        /// </summary>
        /// <param name="position1">1つ目のオブジェクトの位置</param>
        /// <param name="radius1">1つ目のオブジェクトの半径</param>
        /// <param name="position2">2つ目のオブジェクトの位置</param>
        /// <param name="radius2">2つ目のオブジェクトの半径</param>
        /// <returns>衝突しているかどうか</returns>
        public static bool CheckSphereCollision(
            Vector3 position1,
            float radius1,
            Vector3 position2,
            float radius2
        )
        {
            float distance = Vector3.Distance(position1, position2);
            return distance < (radius1 + radius2);
        }

        /// <summary>
        /// 衝突点の位置を計算する
        /// </summary>
        /// <param name="position1">1つ目のオブジェクトの位置</param>
        /// <param name="radius1">1つ目のオブジェクトの半径</param>
        /// <param name="position2">2つ目のオブジェクトの位置</param>
        /// <param name="radius2">2つ目のオブジェクトの半径</param>
        /// <returns>衝突点の位置</returns>
        public static Vector3 CalculateCollisionPoint(
            Vector3 position1,
            float radius1,
            Vector3 position2,
            float radius2
        )
        {
            // 半径の合算
            float radiusSum = radius1 + radius2;
            // 移動単位
            Vector3 moveUnit = (position2 - position1) / radiusSum;
            // いずれかの半径倍数分の増加
            Vector3 move = moveUnit * radius1;
            // 半径の割合分移動した位置
            return position1 + move;
        }

        /// <summary>
        /// 弾と敵の衝突時にエフェクトを発生させる
        /// </summary>
        /// <param name="bulletPosition">弾の位置</param>
        /// <param name="bulletRadius">弾の半径</param>
        /// <param name="bulletDirection">弾の進行方向</param>
        /// <param name="enemy">敵オブジェクト</param>
        public static void EmitHitEffect(
            Vector3 bulletPosition,
            float bulletRadius,
            Vector3 bulletDirection,
            Enemy enemy
        )
        {
            if (FxEmitter.Instance == null)
                return;

            // 弾の進行方向を反転させてエフェクトの向きを決定
            Vector3 hitDirection = -bulletDirection;

            // 衝突点の計算
            Vector3 hitPosition = CalculateCollisionPoint(
                bulletPosition,
                bulletRadius,
                enemy.CollisionCenter,
                enemy.CollisionRadius
            );

            // エフェクト発生
            FxEmitter.Instance.EmitByType(FxType.HitFlash, hitPosition, hitDirection);
        }
    }
}
