using System;
using System.IO;
using System.Collections;
using System.Reflection;

using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using Dread.Battle.Util;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
#endif

namespace Dread.Util
{
    /// <summary>
    /// スクリーンショット機能を提供するマネージャークラス
    /// </summary>
    [HideMonoScript]
    [DefaultExecutionOrder(-90)] // SingletonMonoBehaviourよりも後に実行されるようにする
    public class ScreenshotManager : SingletonMonoBehaviour<ScreenshotManager>
    {
        [SerializeField, LabelText("シーンカメラ(MainCamera)")]
        private Camera sceneCamera;

        [SerializeField, LabelText("撮影用カメラ")]
        private Camera snapCamera; // 撮影用カメラ

        [FoldoutGroup("スクリーンショット設定")]
        [SerializeField, Tooltip("スクリーンショットの保存先フォルダ")]
        [FolderPath, LabelText("保存先フォルダ")]
        private string saveDirectory = "Screenshots";

        [FoldoutGroup("スクリーンショット設定")]
        [SerializeField, Tooltip("スクリーンショットのファイル名のプレフィックス")]
        [LabelText("ファイル名プレフィックス")]
        private string fileNamePrefix = "DreadnoughtScreenshot";

        [FoldoutGroup("スクリーンショット設定")]
        [SerializeField, Tooltip("スクリーンショットの解像度倍率")]
        [Range(1, 4)]
        [LabelText("解像度倍率")]
        private int resolutionMultiplier = 1;

        [FoldoutGroup("スクリーンショット設定")]
        [SerializeField, Tooltip("Sceneビュー視点からのスクリーンショットの解像度倍率")]
        [Range(1, 4)]
        [LabelText("Sceneビュー視点解像度倍率")]
        private int sceneViewResolutionMultiplier = 2;

        [FoldoutGroup("スクリーンショット設定")]
        [SerializeField, Tooltip("スクリーンショット時にUIを非表示にするかどうか")]
        [LabelText("UIを非表示にする")]
        [ToggleLeft]
        private bool hideUI = true;

        private Canvas[] uiCanvases;

        protected override void Awake()
        {
            base.Awake(); // SingletonMonoBehaviourのAwakeを呼び出してシングルトンの初期化を行う

            // UIキャンバスの参照を取得
            uiCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            // カメラが設定されていない場合はメインカメラを使用
            if (sceneCamera == null)
            {
                sceneCamera = Camera.main;
            }

            // 撮影用カメラが設定されていない場合はメインカメラを使用
            if (snapCamera == null)
            {
                Debug.LogWarning("撮影用カメラが設定されていません。");
            }

            // 保存先ディレクトリが存在しない場合は作成
            string fullPath = System.IO.Path.Combine(Application.persistentDataPath, saveDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            // シーン遷移で破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 現在のGameViewからスクリーンショットを撮影して保存する
        /// </summary>
        [Button(ButtonSizes.Medium, Name = "ゲーム画面を撮影")]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("スクリーンショット", centerLabel: true)]
        public void CaptureGameViewScreenshot()
        {
            StartCoroutine(CaptureScreenshotCoroutine());
        }

        private System.Collections.IEnumerator CaptureScreenshotCoroutine()
        {
            // UIを非表示にする（設定されている場合）
            if (hideUI)
            {
                SetUIVisibility(false);
            }

            // 1フレーム待機して画面の更新を確実にする
            yield return new WaitForEndOfFrame();

            // スクリーンショットを撮影
            string fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = GetScreenshotPath(fileName);

            // 解像度倍率を適用してスクリーンショットを撮影
            ScreenCapture.CaptureScreenshot(fullPath, resolutionMultiplier);

            // UIを元に戻す
            if (hideUI)
            {
                SetUIVisibility(true);
            }

            Debug.Log($"スクリーンショットを保存しました: {fullPath}");

#if UNITY_EDITOR
            // エディタの場合はプロジェクトウィンドウを更新
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// UIの表示/非表示を切り替える
        /// </summary>
        private void SetUIVisibility(bool visible)
        {
            if (uiCanvases != null)
            {
                foreach (var canvas in uiCanvases)
                {
                    if (canvas != null)
                    {
                        canvas.enabled = visible;
                    }
                }
            }
        }

        /// <summary>
        /// スクリーンショットの保存パスを取得する
        /// </summary>
        private string GetScreenshotPath(string fileName)
        {
            string directoryPath = System.IO.Path.Combine(
                Application.persistentDataPath,
                saveDirectory
            );

            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return System.IO.Path.Combine(directoryPath, fileName);
        }

#if UNITY_EDITOR


        /// <summary>
        /// GameViewをSceneViewのカメラ視点から撮影する（エディタのみ）
        /// </summary>
        [Button(ButtonSizes.Medium, Name = "Sceneビュー視点からゲームを撮影")]
        [GUIColor(0.4f, 0.8f, 1.0f)]
        [BoxGroup("スクリーンショット/エディタ用")]
        [InfoBox("現在のSceneViewカメラの視点からゲームを撮影します。ポストエフェクトを含む完全なレンダリングを行います。")]
        public void CaptureGameViewFromSceneViewCamera()
        {
            StartCoroutine(CaptureGameViewFromSceneViewCameraCoroutine());
        }

        /// <summary>
        /// SceneViewカメラの視点からゲームを撮影するコルーチン
        /// </summary>
        private System.Collections.IEnumerator CaptureGameViewFromSceneViewCameraCoroutine()
        {
            // SceneViewの参照を取得
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogWarning("アクティブなSceneViewが見つかりません。");
                yield break;
            }

            // 撮影用カメラの状態を保存
            bool originalCameraState = snapCamera.enabled;

            // カメラの位置と回転を保存
            snapCamera.transform.GetPositionAndRotation(
                out Vector3 originalPosition,
                out Quaternion originalRotation
            );
            float originalFieldOfView = snapCamera.fieldOfView;

            // ViewportRectの変更は不要になったため、処理を削除

            // UIを非表示にする（設定されている場合）
            if (hideUI)
            {
                SetUIVisibility(false);
            }

            try
            {
                // SceneViewカメラの設定を取得
                Camera sceneViewCamera = sceneView.camera;

                // 撮影用カメラをSceneViewカメラの設定に合わせる
                snapCamera.transform.SetPositionAndRotation(
                    sceneViewCamera.transform.position,
                    sceneViewCamera.transform.rotation
                );
                snapCamera.fieldOfView = sceneViewCamera.fieldOfView;

                // 撮影用カメラを有効化
                snapCamera.enabled = true;

                // プレイヤーループを更新してシーンを最新状態にする
                EditorApplication.QueuePlayerLoopUpdate();

                // 通常のスクリーンショット撮影と同じ処理を利用
                // ファイル名だけ変更
                string originalPrefix = fileNamePrefix;
                fileNamePrefix = $"{fileNamePrefix}_SV";

                // 通常のスクリーンショット撮影コルーチンを利用
                yield return CaptureScreenshotCoroutine();

                // ファイル名プレフィックスを元に戻す
                fileNamePrefix = originalPrefix;
            }
            finally
            {
                // 撮影用カメラを元の状態に戻す
                snapCamera.enabled = originalCameraState;

                // カメラの位置と回転を元に戻す
                snapCamera.transform.SetPositionAndRotation(originalPosition, originalRotation);
                snapCamera.fieldOfView = originalFieldOfView;

                // UIを元に戻す
                if (hideUI)
                {
                    SetUIVisibility(true);
                }

                // エディタのプロジェクトウィンドウを更新
                AssetDatabase.Refresh();
            }
        }
#endif
    }
}
