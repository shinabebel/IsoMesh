using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IsoMesh.Editor
{
    [CustomEditor(typeof(SDFPrimitive))]
    public class SDFPrimitiveEditor : UnityEditor.Editor
    {
        private static class Labels
        {
            public static GUIContent Type = new GUIContent("Type", "Which primitive this object represents.");
            public static GUIContent Operation = new GUIContent("Operation", "How this primitive is combined with the previous SDF objects in the hierarchy.");
            public static GUIContent Flip = new GUIContent("Flip", "Turn this object inside out.");
            public static GUIContent Bounds = new GUIContent("Bounds", "The xyz bounds of the cuboid.");
            public static GUIContent Roundedness = new GUIContent("Roundedness", "How curved are the cuboids corners and edges.");
            public static GUIContent Radius = new GUIContent("Radius", "The radius of the sphere.");
            public static GUIContent MajorRadius = new GUIContent("Major Radius", "The radius of the whole torus.");
            public static GUIContent MinorRadius = new GUIContent("Minor Radius", "The radius of the tube of the torus.");
            public static GUIContent Thickness = new GUIContent("Thickness", "The thickness of the frame.");
        }

        private class SerializedProperties
        {
            public SerializedProperty Type { get; }
            public SerializedProperty Data { get; }
            public SerializedProperty Operation { get; }
            public SerializedProperty Flip { get; }

            public SerializedProperties(SerializedObject serializedObject)
            {
                Type = serializedObject.FindProperty("m_type");
                Data = serializedObject.FindProperty("m_data");
                Operation = serializedObject.FindProperty("m_operation");
                Flip = serializedObject.FindProperty("m_flip");
            }
        }


        private SerializedProperties m_serializedProperties;
        private SDFPrimitive m_sdfPrimitive;
        private SerializedPropertySetter m_setter;

        private void OnEnable()
        {
            m_serializedProperties = new SerializedProperties(serializedObject);
            m_sdfPrimitive = target as SDFPrimitive;
            m_setter = new SerializedPropertySetter(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.DrawScript();
            m_setter.Clear();

            m_setter.DrawProperty(Labels.Type, m_serializedProperties.Type);
            m_setter.DrawProperty(Labels.Operation, m_serializedProperties.Operation);
            m_setter.DrawProperty(Labels.Flip, m_serializedProperties.Flip);

            using (EditorGUILayout.VerticalScope box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                switch (m_sdfPrimitive.Type)
                {
                    case SDFPrimitiveType.Sphere:
                        m_setter.DrawVectorSettingX(Labels.Radius, m_serializedProperties.Data, min: 0f);
                        break;
                    case SDFPrimitiveType.Torus:
                        m_setter.DrawVectorSettingX(Labels.MajorRadius, m_serializedProperties.Data, min: 0f);
                        m_setter.DrawVectorSettingY(Labels.MinorRadius, m_serializedProperties.Data, min: 0f);
                        break;
                    case SDFPrimitiveType.Cuboid:
                        m_setter.DrawVector3Setting(Labels.Bounds, m_serializedProperties.Data, min: 0f);
                        m_setter.DrawVectorSettingW(Labels.Roundedness, m_serializedProperties.Data, min: 0f);
                        break;
                    case SDFPrimitiveType.BoxFrame:
                        m_setter.DrawVector3Setting(Labels.Bounds, m_serializedProperties.Data, min: 0f);
                        m_setter.DrawVectorSettingW(Labels.Thickness, m_serializedProperties.Data, min: 0f);
                        break;
                }
            }

            m_setter.Update();
        }

        private void OnSceneGUI()
        {
            Color col = m_sdfPrimitive.Operation == SDFCombineType.SmoothSubtract ? Color.red : Color.blue;
            Vector4 data = m_serializedProperties.Data.vector4Value;
            Handles.color = col;
            Handles.matrix = m_sdfPrimitive.transform.localToWorldMatrix;

            switch (m_sdfPrimitive.Type)
            {
                case SDFPrimitiveType.BoxFrame:
                case SDFPrimitiveType.Cuboid:
                    Handles.DrawWireCube(Vector3.zero, data.XYZ() * 2f);
                    break;
                //case SDFPrimitiveType.BoxFrame:
                //    Handles.DrawWireCube(Vector3.zero, data.XYZ() * 2f);
                //    break;
                default:
                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, data.x);
                    break;
            }

        }
    }
}