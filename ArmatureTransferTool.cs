using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ArmatureTransferTool : EditorWindow
{
    // ボーンのTransform情報を一時保存する辞書
    private Dictionary<string, TransformData> recordedBones = new Dictionary<string, TransformData>();

    // ユーザーが設定するオブジェクト
    private GameObject sourceObject;
    private GameObject targetObject;

    [MenuItem("Tools/Armature Transfer Tool")]
    public static void ShowWindow()
    {
        GetWindow<ArmatureTransferTool>("Armature Transfer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Armature Transfer Tool", EditorStyles.boldLabel);

        // Source と Target のオブジェクトを設定
        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        // ArmatureのTransformを記録
        if (GUILayout.Button("Record Armature Transforms"))
        {
            RecordArmatureTransforms();
        }

        // 記録したTransformをTargetに適用
        if (GUILayout.Button("Transfer Transforms"))
        {
            TransferTransforms();
        }
    }

    /// <summary>
    /// Sourceオブジェクト内のArmatureを検索し、その配下のボーンのTransformを記録する
    /// ただし、親と同じ名前のボーンは記録対象外
    /// </summary>
    private void RecordArmatureTransforms()
    {
        if (sourceObject == null)
        {
            Debug.LogError("Source object is not assigned.");
            return;
        }

        recordedBones.Clear(); // 記録をリセット

        // Source の Armature を探す
        Transform sourceArmature = FindArmature(sourceObject);
        if (sourceArmature == null)
        {
            Debug.LogError("No Armature found in Source Object.");
            return;
        }

        // Armatureの直下にあるボーンを記録（親と同名のものは除外）
        foreach (Transform bone in sourceArmature.GetComponentsInChildren<Transform>())
        {
            if (!IsInvalidBone(bone)) // 除外条件を満たさない場合のみ記録
            {
                recordedBones[bone.name] = new TransformData(bone);
            }
        }

        Debug.Log($"Recorded {recordedBones.Count} bones.");
    }

    /// <summary>
    /// 記録したTransformをTargetオブジェクト内の同名ボーンに適用する
    /// </summary>
    private void TransferTransforms()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
            return;
        }

        if (recordedBones.Count == 0)
        {
            Debug.LogError("No bone transforms recorded. Please record first.");
            return;
        }

        // Target の Armature を探す
        Transform targetArmature = FindArmature(targetObject);
        if (targetArmature == null)
        {
            Debug.LogError("No Armature found in Target Object.");
            return;
        }

        // 変更をUndoできるように登録
        Undo.RegisterCompleteObjectUndo(targetObject, "Transfer Armature Transforms");

        // Target内のボーンを検索し、記録データがある場合のみ適用
        foreach (Transform bone in targetArmature.GetComponentsInChildren<Transform>())
        {
            if (recordedBones.ContainsKey(bone.name) && !IsInvalidBone(bone)) // 記録済みで、除外条件を満たさない場合
            {
                Undo.RecordObject(bone, "Transform Change"); // 個別のUndoを記録
                TransformData data = recordedBones[bone.name];
                bone.localPosition = data.localPosition;
                bone.localRotation = data.localRotation;
                bone.localScale = data.localScale;
            }
        }

        Debug.Log("Transform values transferred. You can undo with Ctrl+Z.");
    }

    /// <summary>
    /// 指定されたオブジェクト内の"Armature"を探して返す
    /// </summary>
    private Transform FindArmature(GameObject obj)
    {
        foreach (Transform child in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower() == "armature") // 名前が "Armature" に一致する場合
            {
                return child;
            }
        }
        return null; // 見つからなかった場合
    }

    /// <summary>
    /// ボーンが親と同じ名前を持つ場合、処理対象外とする
    /// 例: "Hips"の下にある"Hips" など
    /// </summary>
    private bool IsInvalidBone(Transform bone)
    {
        if (bone.parent == null) return false; // ルートオブジェクトならOK

        Transform parent = bone.parent;
        while (parent != null)
        {
            if (bone.name == parent.name) return true; // 親と同じ名前なら除外
            parent = parent.parent;
        }
        return false; // 問題ない場合は対象に含める
    }

    /// <summary>
    /// ボーンのTransformデータを格納するクラス
    /// </summary>
    private class TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public TransformData(Transform transform)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }
    }
}
