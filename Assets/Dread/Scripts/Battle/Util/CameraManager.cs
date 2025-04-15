using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Util
{
    /// <summary>
    /// Cinemachineカメラを管理し、カメラ間の切り替えを行うマネージャークラス
    /// </summary>
    ///     [DefaultExecutionOrder(-100)] // 基底クラス側で定義されているが、上書きしたい場合はここで変更すること
    [HideMonoScript]
    public class CameraManager : SingletonMonoBehaviour<CameraManager>
    {
        [BoxGroup("カメラ設定")]
        [SerializeField, Tooltip("シーンカメラの参照")]
        private Camera sceneCamera;

        [FoldoutGroup("ブレンド設定", expanded: true)]
        [SerializeField, Tooltip("カメラ切り替え時のブレンド時間(秒)")]
        [Range(0f, 5f)]
        private float blendTime = 1.0f;

        [FoldoutGroup("ブレンド設定")]
        [SerializeField, Tooltip("カメラ切り替え時のブレンドスタイル")]
        private CinemachineBlendDefinition.Styles style = CinemachineBlendDefinition.Styles.HardOut;

        [FoldoutGroup("ブレンド設定")]
        [SerializeField, Tooltip("カメラ切り替え時のブレンドカーブ")]
        private AnimationCurve blendCurve;

        [BoxGroup("カメラリスト", centerLabel: true)]
        [SerializeField, Tooltip("シーン内のすべてのCinemachineカメラ")]
        [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        private List<CinemachineCamera> _cameras = new List<CinemachineCamera>();

        [BoxGroup("カメラリスト")]
        [SerializeField, Tooltip("現在アクティブなカメラのインデックス")]
        [OnValueChanged("OnCameraIndexChanged")]
        [PropertyRange(0, "MaxCameraIndex")]
        [LabelText("現在のカメラ")]
        private int _currentCameraIndex = 0;

        private CinemachineCamera _currentCamera;

        // カメラインデックスの最大値を取得するプロパティ
        private int MaxCameraIndex => _cameras.Count > 0 ? _cameras.Count - 1 : 0;
        private CinemachineBrain cinemachineBrain;

        protected override void Awake()
        {
            base.Awake();
            // 使うカメラから、CinemachineBrainを取得
            cinemachineBrain = sceneCamera.GetComponent<CinemachineBrain>();

            // 初期カメラをアクティブにする
            if (_cameras.Count > 0)
            {
                ActivateCamera(_currentCameraIndex);
            }
            else
            {
                Debug.LogWarning("カメラが登録されていません。インスペクタからカメラを追加してください。");
            }
        }

        // インスペクタでカメラインデックスが変更されたときに呼ばれる
        private void OnCameraIndexChanged()
        {
            if (Application.isPlaying && _cameras.Count > 0)
            {
                ActivateCamera(_currentCameraIndex);
            }
        }

        /// <summary>
        /// 指定されたインデックスのカメラをアクティブにする
        /// </summary>
        /// <param name="index">アクティブにするカメラのインデックス</param>
        public void ActivateCamera(int index)
        {
            if (_cameras.Count == 0)
                return;

            // インデックスの範囲チェック
            index = Mathf.Clamp(index, 0, _cameras.Count - 1);
            _currentCameraIndex = index;

            // すべてのカメラを非アクティブにする
            foreach (var camera in _cameras)
            {
                camera.Priority = 0;
            }

            // 指定されたカメラをアクティブにする
            _currentCamera = _cameras[_currentCameraIndex];
            _currentCamera.Priority = 10;

            // 新しいブレンド定義を作成して設定
            var newBlend = new CinemachineBlendDefinition(
                style, // スタイル反映
                blendTime // ブレンド時間を設定
            );
            newBlend.CustomCurve = blendCurve; // 自前のカーブ
            cinemachineBrain.DefaultBlend = newBlend; // ブレンド定義を反映
            Debug.Log(
                $"カメラを切り替えました: {_currentCamera.name} style: {newBlend.Style} (ブレンド時間: {blendTime}秒)"
            );
        }

        /// <summary>
        /// 名前で指定されたカメラをアクティブにする
        /// </summary>
        /// <param name="cameraName">アクティブにするカメラの名前</param>
        public void ActivateCamera(string cameraName)
        {
            for (int i = 0; i < _cameras.Count; i++)
            {
                if (_cameras[i].name == cameraName)
                {
                    ActivateCamera(i);
                    return;
                }
            }

            Debug.LogWarning($"指定された名前のカメラが見つかりません: {cameraName}");
        }

        /// <summary>
        /// 次のカメラに切り替える
        /// </summary>
        [Button(ButtonSizes.Medium, Name = "次のカメラへ")]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("カメラ操作", centerLabel: true)]
        public void SwitchToNextCamera()
        {
            if (_cameras.Count == 0)
                return;

            int nextIndex = (_currentCameraIndex + 1) % _cameras.Count;
            ActivateCamera(nextIndex);
        }

        /// <summary>
        /// 前のカメラに切り替える
        /// </summary>
        [Button(ButtonSizes.Medium, Name = "前のカメラへ")]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("カメラ操作")]
        public void SwitchToPreviousCamera()
        {
            if (_cameras.Count == 0)
                return;

            int prevIndex = (_currentCameraIndex - 1 + _cameras.Count) % _cameras.Count;
            ActivateCamera(prevIndex);
        }

        /// <summary>
        /// 現在アクティブなカメラを取得する
        /// </summary>
        /// <returns>現在アクティブなCinemachineCamera</returns>
        public CinemachineCamera GetCurrentCamera()
        {
            return _currentCamera;
        }

        /// <summary>
        /// 管理対象のカメラリストに新しいカメラを追加する
        /// </summary>
        /// <param name="camera">追加するCinemachineCamera</param>
        public void AddCamera(CinemachineCamera camera)
        {
            if (!_cameras.Contains(camera))
            {
                _cameras.Add(camera);
                Debug.Log($"カメラを追加しました: {camera.name}");
            }
        }

        /// <summary>
        /// 管理対象のカメラリストからカメラを削除する
        /// </summary>
        /// <param name="camera">削除するCinemachineCamera</param>
        public void RemoveCamera(CinemachineCamera camera)
        {
            if (_cameras.Contains(camera))
            {
                _cameras.Remove(camera);
                Debug.Log($"カメラを削除しました: {camera.name}");

                // 現在のカメラが削除された場合、別のカメラをアクティブにする
                if (_currentCamera == camera && _cameras.Count > 0)
                {
                    ActivateCamera(0);
                }
            }
        }
    }
}
