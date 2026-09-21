using UnityEditor;
using UnityEngine;

namespace CanvasFlexbox.Editor
{
    [CustomEditor(typeof(FlexItem))]
    [CanEditMultipleObjects]
    public class FlexItemEditor : UnityEditor.Editor
    {
        private SerializedProperty _flexGrowProp;
        private SerializedProperty _flexShrinkProp;
        private SerializedProperty _flexBasisProp;
        private SerializedProperty _alignSelfProp;
        private SerializedProperty _marginProp;
        private SerializedProperty _minWidthProp;
        private SerializedProperty _minHeightProp;
        private SerializedProperty _maxWidthProp;
        private SerializedProperty _maxHeightProp;
        private SerializedProperty _ignoreLayoutProp;

        private void OnEnable()
        {
            _flexGrowProp = serializedObject.FindProperty("_flexGrow");
            _flexShrinkProp = serializedObject.FindProperty("_flexShrink");
            _flexBasisProp = serializedObject.FindProperty("_flexBasis");
            _alignSelfProp = serializedObject.FindProperty("_alignSelf");
            _marginProp = serializedObject.FindProperty("_margin");
            _minWidthProp = serializedObject.FindProperty("_minWidth");
            _minHeightProp = serializedObject.FindProperty("_minHeight");
            _maxWidthProp = serializedObject.FindProperty("_maxWidth");
            _maxHeightProp = serializedObject.FindProperty("_maxHeight");
            _ignoreLayoutProp = serializedObject.FindProperty("_ignoreLayout");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            DrawPresetBar();
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Flex Properties", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_flexGrowProp, new GUIContent("Flex Grow", "Ability to grow if free space is available."));
                EditorGUILayout.PropertyField(_flexShrinkProp, new GUIContent("Flex Shrink", "Ability to shrink if space is deficient."));
                DrawFlexBasisGUI();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Alignment Override", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_alignSelfProp, new GUIContent("Align Self", "Overrides container's AlignItems."));
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Margins", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawOffsetsGUI(_marginProp);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Size Constraints", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minWidthProp, new GUIContent("Min W"));
                EditorGUILayout.PropertyField(_maxWidthProp, new GUIContent("Max W"));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_minHeightProp, new GUIContent("Min H"));
                EditorGUILayout.PropertyField(_maxHeightProp, new GUIContent("Max H"));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_ignoreLayoutProp, new GUIContent("Ignore Layout", "Excludes this item from flex calculations."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPresetBar()
        {
            EditorGUILayout.LabelField("Item Presets", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Fixed (0)", EditorStyles.miniButtonLeft))
            {
                _flexGrowProp.floatValue = 0f;
                _flexShrinkProp.floatValue = 0f;
            }

            if (GUILayout.Button("Flexible (1)", EditorStyles.miniButtonMid))
            {
                _flexGrowProp.floatValue = 1f;
                _flexShrinkProp.floatValue = 1f;
            }

            if (GUILayout.Button("Auto Basis", EditorStyles.miniButtonMid))
            {
                var unitProp = _flexBasisProp.FindPropertyRelative("_unit");
                if (unitProp != null) unitProp.enumValueIndex = (int)FlexUnit.Auto;
            }

            if (GUILayout.Button("Stretch Cross", EditorStyles.miniButtonRight))
            {
                _alignSelfProp.enumValueIndex = (int)AlignSelf.Stretch;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFlexBasisGUI()
        {
            var unitProp = _flexBasisProp.FindPropertyRelative("_unit");
            var valProp = _flexBasisProp.FindPropertyRelative("_value");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Flex Basis");

            if (unitProp != null)
            {
                unitProp.enumValueIndex = (int)(FlexUnit)EditorGUILayout.EnumPopup((FlexUnit)unitProp.enumValueIndex, GUILayout.Width(70));
            }

            if (unitProp != null && unitProp.enumValueIndex != (int)FlexUnit.Auto && valProp != null)
            {
                valProp.floatValue = EditorGUILayout.FloatField(valProp.floatValue);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOffsetsGUI(SerializedProperty offsetsProp)
        {
            var left = offsetsProp.FindPropertyRelative("left");
            var right = offsetsProp.FindPropertyRelative("right");
            var top = offsetsProp.FindPropertyRelative("top");
            var bottom = offsetsProp.FindPropertyRelative("bottom");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(left, new GUIContent("Left"));
            EditorGUILayout.PropertyField(right, new GUIContent("Right"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(top, new GUIContent("Top"));
            EditorGUILayout.PropertyField(bottom, new GUIContent("Bottom"));
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            var item = (FlexItem)target;
            if (item == null) return;

            var rectTransform = item.RectTransform;
            if (rectTransform == null) return;

            // Draw Item Bounds
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Handles.color = new Color(0.3f, 1f, 0.4f, 0.9f);
            Handles.DrawPolyLine(corners[0], corners[1], corners[2], corners[3], corners[0]);

            // Draw Margin Bounds
            var margin = item.Margin;
            if (margin.Horizontal > 0 || margin.Vertical > 0)
            {
                Vector3 m0 = corners[0] + rectTransform.TransformDirection(new Vector3(-margin.Left, -margin.Bottom, 0));
                Vector3 m1 = corners[1] + rectTransform.TransformDirection(new Vector3(-margin.Left, margin.Top, 0));
                Vector3 m2 = corners[2] + rectTransform.TransformDirection(new Vector3(margin.Right, margin.Top, 0));
                Vector3 m3 = corners[3] + rectTransform.TransformDirection(new Vector3(margin.Right, -margin.Bottom, 0));

                Handles.color = new Color(1f, 0.6f, 0.2f, 0.6f);
                Handles.DrawDottedLine(m0, m1, 3f);
                Handles.DrawDottedLine(m1, m2, 3f);
                Handles.DrawDottedLine(m2, m3, 3f);
                Handles.DrawDottedLine(m3, m0, 3f);
            }
        }
    }
}
