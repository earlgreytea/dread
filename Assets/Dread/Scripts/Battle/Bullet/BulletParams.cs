using Dread.Battle.Turret;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Bullet
{
    /// <summary>
    /// 弾のパラメータをまとめて管理するクラス
    /// </summary>
    [System.Serializable]
    public class BulletParams
    {
        [LabelText("弾速")]
        public float BulletSpeed = 200f;

        [LabelText("最大飛距離")]
        public float BulletMaxDistance = 300f;

        [LabelText("ダメージ")]
        public float BulletDamage = 10f;

        [LabelText("弾サイズ")]
        public float BulletSize = 0.5f;

        [LabelText("弾種")]
        public BulletType BulletType = BulletType.Normal;
    }
}
