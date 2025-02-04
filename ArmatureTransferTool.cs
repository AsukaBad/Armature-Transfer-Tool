using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ArmatureTransferTool : EditorWindow
{
    // �{�[����Transform�����ꎞ�ۑ����鎫��
    private Dictionary<string, TransformData> recordedBones = new Dictionary<string, TransformData>();

    // ���[�U�[���ݒ肷��I�u�W�F�N�g
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

        // Source �� Target �̃I�u�W�F�N�g��ݒ�
        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        // Armature��Transform���L�^
        if (GUILayout.Button("Record Armature Transforms"))
        {
            RecordArmatureTransforms();
        }

        // �L�^����Transform��Target�ɓK�p
        if (GUILayout.Button("Transfer Transforms"))
        {
            TransferTransforms();
        }
    }

    /// <summary>
    /// Source�I�u�W�F�N�g����Armature���������A���̔z���̃{�[����Transform���L�^����
    /// �������A�e�Ɠ������O�̃{�[���͋L�^�ΏۊO
    /// </summary>
    private void RecordArmatureTransforms()
    {
        if (sourceObject == null)
        {
            Debug.LogError("Source object is not assigned.");
            return;
        }

        recordedBones.Clear(); // �L�^�����Z�b�g

        // Source �� Armature ��T��
        Transform sourceArmature = FindArmature(sourceObject);
        if (sourceArmature == null)
        {
            Debug.LogError("No Armature found in Source Object.");
            return;
        }

        // Armature�̒����ɂ���{�[�����L�^�i�e�Ɠ����̂��̂͏��O�j
        foreach (Transform bone in sourceArmature.GetComponentsInChildren<Transform>())
        {
            if (!IsInvalidBone(bone)) // ���O�����𖞂����Ȃ��ꍇ�̂݋L�^
            {
                recordedBones[bone.name] = new TransformData(bone);
            }
        }

        Debug.Log($"Recorded {recordedBones.Count} bones.");
    }

    /// <summary>
    /// �L�^����Transform��Target�I�u�W�F�N�g���̓����{�[���ɓK�p����
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

        // Target �� Armature ��T��
        Transform targetArmature = FindArmature(targetObject);
        if (targetArmature == null)
        {
            Debug.LogError("No Armature found in Target Object.");
            return;
        }

        // �ύX��Undo�ł���悤�ɓo�^
        Undo.RegisterCompleteObjectUndo(targetObject, "Transfer Armature Transforms");

        // Target���̃{�[�����������A�L�^�f�[�^������ꍇ�̂ݓK�p
        foreach (Transform bone in targetArmature.GetComponentsInChildren<Transform>())
        {
            if (recordedBones.ContainsKey(bone.name) && !IsInvalidBone(bone)) // �L�^�ς݂ŁA���O�����𖞂����Ȃ��ꍇ
            {
                Undo.RecordObject(bone, "Transform Change"); // �ʂ�Undo���L�^
                TransformData data = recordedBones[bone.name];
                bone.localPosition = data.localPosition;
                bone.localRotation = data.localRotation;
                bone.localScale = data.localScale;
            }
        }

        Debug.Log("Transform values transferred. You can undo with Ctrl+Z.");
    }

    /// <summary>
    /// �w�肳�ꂽ�I�u�W�F�N�g����"Armature"��T���ĕԂ�
    /// </summary>
    private Transform FindArmature(GameObject obj)
    {
        foreach (Transform child in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower() == "armature") // ���O�� "Armature" �Ɉ�v����ꍇ
            {
                return child;
            }
        }
        return null; // ������Ȃ������ꍇ
    }

    /// <summary>
    /// �{�[�����e�Ɠ������O�����ꍇ�A�����ΏۊO�Ƃ���
    /// ��: "Hips"�̉��ɂ���"Hips" �Ȃ�
    /// </summary>
    private bool IsInvalidBone(Transform bone)
    {
        if (bone.parent == null) return false; // ���[�g�I�u�W�F�N�g�Ȃ�OK

        Transform parent = bone.parent;
        while (parent != null)
        {
            if (bone.name == parent.name) return true; // �e�Ɠ������O�Ȃ珜�O
            parent = parent.parent;
        }
        return false; // ���Ȃ��ꍇ�͑ΏۂɊ܂߂�
    }

    /// <summary>
    /// �{�[����Transform�f�[�^���i�[����N���X
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
