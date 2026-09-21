using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasFlexbox.Editor
{
    /// <summary>
    /// Adds GameObject menu items for creating Flexbox containers directly in Unity Hierarchy.
    /// </summary>
    public static class FlexMenuCommands
    {
        private const int MenuPriority = 2050;

        [MenuItem("GameObject/UI/Flexbox/Flex Container (Row)", false, MenuPriority)]
        public static void CreateRowContainer(MenuCommand menuCommand)
        {
            CreateContainer(menuCommand, "FlexRow", FlexDirection.Row, FlexWrap.NoWrap, JustifyContent.FlexStart, AlignItems.Center);
        }

        [MenuItem("GameObject/UI/Flexbox/Flex Container (Column)", false, MenuPriority + 1)]
        public static void CreateColumnContainer(MenuCommand menuCommand)
        {
            CreateContainer(menuCommand, "FlexColumn", FlexDirection.Column, FlexWrap.NoWrap, JustifyContent.FlexStart, AlignItems.Stretch);
        }

        [MenuItem("GameObject/UI/Flexbox/Flex Container (Wrap Grid)", false, MenuPriority + 2)]
        public static void CreateWrapContainer(MenuCommand menuCommand)
        {
            CreateContainer(menuCommand, "FlexWrapGrid", FlexDirection.Row, FlexWrap.Wrap, JustifyContent.FlexStart, AlignItems.FlexStart);
        }

        [MenuItem("GameObject/UI/Flexbox/Flex Child Item", false, MenuPriority + 3)]
        public static void CreateChildItem(MenuCommand menuCommand)
        {
            GameObject context = menuCommand.context as GameObject;
            GameObject parent = context;

            if (parent == null)
            {
                var container = Object.FindFirstObjectByType<FlexContainer>();
                if (container != null)
                {
                    parent = container.gameObject;
                }
            }

            if (parent == null)
            {
                parent = EnsureCanvas().gameObject;
            }

            GameObject child = new GameObject("FlexItem", typeof(RectTransform), typeof(FlexItem));
            Undo.RegisterCreatedObjectUndo(child, "Create Flex Item");
            GameObjectUtility.SetParentAndAlign(child, parent);

            var rect = child.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 50f);

            Selection.activeGameObject = child;
        }

        private static GameObject CreateContainer(
            MenuCommand menuCommand,
            string defaultName,
            FlexDirection direction,
            FlexWrap wrap,
            JustifyContent justify,
            AlignItems align)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = EnsureCanvas();
                parent = canvas.gameObject;
            }

            GameObject containerObj = new GameObject(defaultName, typeof(RectTransform), typeof(FlexContainer));
            Undo.RegisterCreatedObjectUndo(containerObj, "Create " + defaultName);
            GameObjectUtility.SetParentAndAlign(containerObj, parent);

            var rect = containerObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 200f);

            var container = containerObj.GetComponent<FlexContainer>();
            container.Direction = direction;
            container.Wrap = wrap;
            container.JustifyContent = justify;
            container.AlignItems = align;
            container.ColumnGap = 10f;
            container.RowGap = 10f;
            container.Padding = new FlexOffsets(10f);

            Selection.activeGameObject = containerObj;
            return containerObj;
        }

        private static Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) return canvas;

            GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");

            // Add EventSystem if missing
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventObj, "Create EventSystem");
            }

            return canvas;
        }
    }
}
