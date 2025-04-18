using UnityEngine;
using Sirenix.OdinInspector;
using Dread.Battle.Bullet;

namespace Dread.Battle.Turret
{
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
    /// タレットの基本性能・名称・見た目などを保持するScriptableObject
    /// </summary>
    [CreateAssetMenu(
        fileName = "TurretData",
        menuName = "Dread/Battle/Turret/TurretData",
        order = 0
    )]
    public class TurretData : ScriptableObject
    {
        [BoxGroup("基本情報"), LabelText("名称")]
        public string turretName;

        [BoxGroup("基本情報"), LabelText("説明"), TextArea]
        public string description;

        [BoxGroup("基本情報"), LabelText("アイコン")]
        public Sprite icon;

        [
            BoxGroup("基本情報"),
            LabelText("タレットPrefab"),
            PreviewField(75),
            AssetsOnly,
            ValidateInput("HasTurretViewValidation")
        ]
        public GameObject turretViewPrefab;

        [BoxGroup("旋回性能"), LabelText("回転速度")]
        public float rotationSpeed = 30f;

        // TODO:必要になったら復活させる性能
        //        [BoxGroup("旋回性能"), LabelText("水平方向角度制限")]
        //        public float horizontalAngleLimit = 60f;

        //        [BoxGroup("旋回性能"), LabelText("360度回転可能")]
        //        public bool fullRotation = true;

        [BoxGroup("旋回性能"), LabelText("垂直方向角度制限")]
        public float verticalAngleLimit = 70f;

        [BoxGroup("発射性能"), LabelText("発射レート（毎秒）")]
        public float fireRate = 1f;

        [BoxGroup("発射性能"), LabelText("偏差射撃有効"), Tooltip("ONで偏差射撃を行う")]
        public bool enablePredictiveFire = false;

        [BoxGroup("発射性能"), LabelText("精度（1.0=高精度）")]
        public float accuracy = 0.5f;

        [BoxGroup("発射性能"), LabelText("有効射程距離")]
        public float effectiveRange = 150f;

        [BoxGroup("弾性能"), LabelText("弾パラメータ")]
        public BulletParams bulletParams = new BulletParams();

        /// <summary>
        /// 指定されたGameObjectがTurretViewコンポーネントを持っているかを検証する
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool HasTurretViewValidation(GameObject gameObject, ref string errorMessage)
        {
            if (gameObject == null)
            {
                errorMessage = "GameObject is null";
                return false;
            }

            if (gameObject.GetComponentInChildren<TurretView>() == null)
            {
                // If errorMessage is left as null, the default error message from the attribute will be used
                errorMessage = "\"" + gameObject.name + "\" must have a TurretView component";

                return false;
            }

            return true;
        }
    }
}
