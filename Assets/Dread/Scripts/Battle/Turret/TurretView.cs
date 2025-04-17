using UnityEngine;

namespace Dread.Battle.Turret
{
    /// <summary>
    /// タレットの見た目・アニメーション・Transform制御を担当するクラス
    /// </summary>
    public class TurretView : MonoBehaviour
    {
        [Header("タレットパーツのTransform")]
        [SerializeField]
        Transform turretBase; // 水平旋回部分

        [SerializeField]
        Transform barrel; // 垂直旋回部分

        [SerializeField]
        Transform muzzle; // 発射位置

        // 水平旋回
        public void RotateBase(Quaternion targetRotation, float speed)
        {
            if (turretBase != null)
                turretBase.rotation = Quaternion.Slerp(
                    turretBase.rotation,
                    targetRotation,
                    speed * Time.deltaTime
                );
        }

        // 垂直旋回
        public void RotateBarrel(Quaternion targetRotation, float speed)
        {
            if (barrel != null)
                barrel.localRotation = Quaternion.Slerp(
                    barrel.localRotation,
                    targetRotation,
                    speed * Time.deltaTime
                );
        }

        // 発射エフェクト
        public void PlayMuzzleFlash(Vector3 direction)
        {
            if (muzzle != null)
                Fx.FxEmitter.Instance.EmitByType(Fx.FxType.MuzzleFlash, muzzle.position, direction);
        }

        public Vector3 GetMuzzlePosition() => muzzle != null ? muzzle.position : transform.position;

        public Vector3 GetBarrelForward() => barrel != null ? barrel.forward : transform.forward;

        public Transform GetBarrel() => barrel;

        public Transform GetMuzzle() => muzzle;

        public Transform GetTurretBase() => turretBase;

        // Gizmos描画
        void OnDrawGizmosSelected()
        {
            if (barrel != null && muzzle != null)
            {
                Gizmos.color = Color.red;
                Vector3 lineStart = muzzle.position;
                Vector3 lineEnd = lineStart + barrel.forward * 10f;
                Gizmos.DrawLine(lineStart, lineEnd);
                Gizmos.DrawSphere(lineEnd, 0.2f);
                // 有効射程範囲はLogic側で描画する想定
            }
        }
    }
}
