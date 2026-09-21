using UnityEditor;
using UnityEngine;

namespace CanvasFlexbox.Editor
{
    [CustomEditor(typeof(FlexContainer))]
    [CanEditMultipleObjects]
    public class FlexContainerEditor : UnityEditor.Editor
    {
        private SerializedProperty _directionProp;
        private SerializedProperty _wrapProp;
        private SerializedProperty _justifyContentProp;
        private SerializedProperty _alignItemsProp;
        private SerializedProperty _alignContentProp;
        private SerializedProperty _paddingProp;
        private SerializedProperty _rowGapProp;
        private SerializedProperty _columnGapProp;
        private SerializedProperty _fitWidthProp;
        private SerializedProperty _fitHeightProp;

        private void OnEnable()
        {
            _directionProp = serializedObject.FindProperty("_direction");
            _wrapProp = serializedObject.FindProperty("_wrap");
            _justifyContentProp = serializedObject.FindProperty("_justifyContent");
            _alignItemsProp = serializedObject.FindProperty("_alignItems");
            _alignContentProp = serializedObject.FindProperty("_alignContent");
            _paddingProp = serializedObject.FindProperty("_padding");
            _rowGapProp = serializedObject.FindProperty("_rowGap");
            _columnGapProp = serializedObject.FindProperty("_columnGap");
            _fitWidthProp = serializedObject.FindProperty("_fitToContentWidth");
            _fitHeightProp = serializedObject.FindProperty("_fitToContentHeight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            DrawPresetBar();
            EditorGUILayout.Space(8);

            // Flex Axis Section
            EditorGUILayout.LabelField("Flex Axis & Direction", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_directionProp, new GUIContent("Direction", "Main axis direction: Row or Column."));
                EditorGUILayout.PropertyField(_wrapProp, new GUIContent("Wrap", "Whether items wrap onto multiple lines."));
                EditorGUILayout.PropertyField(_justifyContentProp, new GUIContent("Justify Content", "Main axis distribution of items."));
            }

            EditorGUILayout.Space(4);

            // Cross Axis Section
            EditorGUILayout.LabelField("Cross Axis Alignment", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_alignItemsProp, new GUIContent("Align Items", "Cross axis alignment of items within their line."));
                if (_wrapProp.enumValueIndex != (int)FlexWrap.NoWrap)
                {
                    EditorGUILayout.PropertyField(_alignContentProp, new GUIContent("Align Content", "Distribution of multiple lines along cross axis."));
                }
            }

            EditorGUILayout.Space(4);

            // Spacing & Padding
            EditorGUILayout.LabelField("Spacing & Padding", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_rowGapProp, new GUIContent("Row Gap (px)", "Spacing between rows."));
                EditorGUILayout.PropertyField(_columnGapProp, new GUIContent("Column Gap (px)", "Spacing between columns."));

                EditorGUILayout.Space(2);
                DrawOffsetsGUI("Padding", _paddingProp);
            }

            EditorGUILayout.Space(4);

            // Self Sizing Section
            EditorGUILayout.LabelField("Container Sizing", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_fitWidthProp, new GUIContent("Fit To Content Width", "Resizes container RectTransform width to fit content."));
                EditorGUILayout.PropertyField(_fitHeightProp, new GUIContent("Fit To Content Height", "Resizes container RectTransform height to fit content."));

                EditorGUILayout.Space(2);
                if (GUILayout.Button("Stretch to Parent (Full Width/Height)", EditorStyles.miniButton))
                {
                    var container = (FlexContainer)target;
                    var rect = container.RectTransform;
                    Undo.RecordObject(rect, "Stretch Container to Parent");
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                }
            }

            EditorGUILayout.Space(8);

            // Quick Child Creation Button
            if (GUILayout.Button("➕ Add Flex Child", GUILayout.Height(28)))
            {
                AddChildItem();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPresetBar()
        {
            EditorGUILayout.LabelField("Quick Presets", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Top-Left", EditorStyles.miniButtonLeft))
            {
                _justifyContentProp.enumValueIndex = (int)JustifyContent.FlexStart;
                _alignItemsProp.enumValueIndex = (int)AlignItems.FlexStart;
            }

            if (GUILayout.Button("Row Between", EditorStyles.miniButtonMid))
            {
                _directionProp.enumValueIndex = (int)FlexDirection.Row;
                _justifyContentProp.enumValueIndex = (int)JustifyContent.SpaceBetween;
                _alignItemsProp.enumValueIndex = (int)AlignItems.Center;
            }

            if (GUILayout.Button("Center Both", EditorStyles.miniButtonMid))
            {
                _justifyContentProp.enumValueIndex = (int)JustifyContent.Center;
                _alignItemsProp.enumValueIndex = (int)AlignItems.Center;
            }

            if (GUILayout.Button("Grid Wrap", EditorStyles.miniButtonRight))
            {
                _directionProp.enumValueIndex = (int)FlexDirection.Row;
                _wrapProp.enumValueIndex = (int)FlexWrap.Wrap;
                _justifyContentProp.enumValueIndex = (int)JustifyContent.FlexStart;
                _alignItemsProp.enumValueIndex = (int)AlignItems.FlexStart;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOffsetsGUI(string label, SerializedProperty offsetsProp)
        {
            var left = offsetsProp.FindPropertyRelative("left");
            var right = offsetsProp.FindPropertyRelative("right");
            var top = offsetsProp.FindPropertyRelative("top");
            var bottom = offsetsProp.FindPropertyRelative("bottom");

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 46f;
            EditorGUILayout.PropertyField(left, new GUIContent("Left"));
            EditorGUILayout.PropertyField(right, new GUIContent("Right"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 46f;
            EditorGUILayout.PropertyField(top, new GUIContent("Top"));
            EditorGUILayout.PropertyField(bottom, new GUIContent("Bottom"));
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        private void AddChildItem()
        {
            var container = (FlexContainer)target;
            var newChild = new GameObject("FlexItem_" + (container.transform.childCount + 1), typeof(RectTransform), typeof(FlexItem));
            Undo.RegisterCreatedObjectUndo(newChild, "Create Flex Child");
            GameObjectUtility.SetParentAndAlign(newChild, container.gameObject);
            var rect = newChild.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 50f);
            Selection.activeGameObject = newChild;
        }

        private void OnSceneGUI()
        {
            var container = (FlexContainer)target;
            if (container == null) return;

            var rectTransform = container.RectTransform;
            if (rectTransform == null) return;

            // Draw Container Bounds in Scene
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Handles.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            Handles.DrawPolyLine(corners[0], corners[1], corners[2], corners[3], corners[0]);

            // Draw Padding Rect
            var padding = container.Padding;
            if (padding.Horizontal > 0 || padding.Vertical > 0)
            {
                Vector3 p0 = corners[0] + rectTransform.TransformDirection(new Vector3(padding.Left, padding.Bottom, 0));
                Vector3 p1 = corners[1] + rectTransform.TransformDirection(new Vector3(padding.Left, -padding.Top, 0));
                Vector3 p2 = corners[2] + rectTransform.TransformDirection(new Vector3(-padding.Right, -padding.Top, 0));
                Vector3 p3 = corners[3] + rectTransform.TransformDirection(new Vector3(-padding.Right, padding.Bottom, 0));

                Handles.color = new Color(0.2f, 0.8f, 1f, 0.35f);
                Handles.DrawDottedLine(p0, p1, 4f);
                Handles.DrawDottedLine(p1, p2, 4f);
                Handles.DrawDottedLine(p2, p3, 4f);
                Handles.DrawDottedLine(p3, p0, 4f);
            }

            // Draw Direction Indicator Arrow
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            Vector3 dirVector = container.Direction switch
            {
                FlexDirection.Row => rectTransform.TransformDirection(Vector3.right),
                FlexDirection.RowReverse => rectTransform.TransformDirection(Vector3.left),
                FlexDirection.Column => rectTransform.TransformDirection(Vector3.down),
                FlexDirection.ColumnReverse => rectTransform.TransformDirection(Vector3.up),
                _ => Vector3.zero
            };

            Handles.color = new Color(1f, 0.7f, 0.1f, 0.9f);
            Handles.ArrowHandleCap(0, center, Quaternion.LookRotation(dirVector, Vector3.back), 40f, EventType.Repaint);
        }
    }
}
