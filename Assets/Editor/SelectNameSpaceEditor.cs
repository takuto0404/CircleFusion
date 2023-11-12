using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Plugins.Editor.MermaidMaker
{
    public class SelectNameSpaceEditor : EditorWindow
    {
        private bool _selectAssembly;
        private AssemblyDefinitionAsset _ada;
        private Assembly _assembly;

        [MenuItem("ohagi/MermaidMaker")]
        public static void ShowWindow()
        {
            GetWindow<SelectNameSpaceEditor>();
        }

        private void OnGUI()
        {
            _selectAssembly = EditorGUILayout.Toggle("Select Assembly", _selectAssembly);
            if (_selectAssembly)
            {
                GUILayout.Label("Assembly", EditorStyles.boldLabel);
                _ada = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", _ada,
                    typeof(AssemblyDefinitionAsset), false);
            }

            if (GUI.Button(new Rect(0.0f, 200.0f, 120.0f, 20.0f), "Select NameSpaces"))
            {
                var text = "Assembly-CSharp";
                if (_ada != null)
                {
                    text = _ada.name;
                }

                _assembly = Assembly.Load(text);
                var root = MermaidMakerUtility.GetNameSpaces(_assembly);
                ShowNameSpacesEditor.ShowWindow(root);
            }
        }
    }

    public class ShowNameSpacesEditor : EditorWindow
    {
        private NameSpacesTreeView _treeView;
        private TreeViewState _treeViewState;
        private static NameSpaceNode _rootNode;

        public static void ShowWindow(NameSpaceNode rootNode)
        {
            _rootNode = rootNode;
            GetWindow<ShowNameSpacesEditor>();
        }

        private void OnEnable()
        {
            if (_treeViewState == null)
            {
                _treeViewState = new TreeViewState();
            }

            var selectBoxColumn = new MultiColumnHeaderState.Column
            {
                canSort = false,
                width = 15, 
                minWidth = 15,
                autoResize = true,
                allowToggleVisibility = false
            };
            var nameColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("NameSpaceName"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 150, 
                minWidth = 50,
                autoResize = true,
                allowToggleVisibility = false
            };
            var headerState = new MultiColumnHeaderState(new []{ selectBoxColumn,nameColumn });
            var multiColumnHeader = new MultiColumnHeader(headerState);
            _treeView = new NameSpacesTreeView(_treeViewState, multiColumnHeader);
        }

        private void OnGUI()
        {
            GUILayout.Label("NameSpaces", EditorStyles.boldLabel);
            _treeView.Setup(_rootNode);
            var rect = EditorGUILayout.GetControlRect(false, 200);
            _treeView.OnGUI(rect);
        }
    }

    public class NameSpacesTreeView : TreeView
    {
        private NameSpaceNode _rootNode;

        public NameSpacesTreeView(TreeViewState treeViewState,MultiColumnHeader multiColumnHeader) : base(treeViewState,multiColumnHeader)
        {
            
        }

        public void Setup(NameSpaceNode rootNode)
        {
            _rootNode = rootNode;
            Reload();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();
            
            if (_rootNode != null)
            {
                StringToTreeView(_rootNode.Children, root, rows);
            }

            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        private void StringToTreeView(List<NameSpaceNode> nameSpaceNodes, TreeViewItem parent, IList<TreeViewItem> rows)
        {
            foreach (var nameSpaceNode in nameSpaceNodes)
            {
                var childItem = new NameSpaceNodeTreeViewItem
                {
                    id = nameSpaceNode.Id,
                    displayName = nameSpaceNode.NameSpaceName,
                    Data = nameSpaceNode
                };
                parent.AddChild(childItem);
                rows.Add(childItem);
                if (nameSpaceNode.Children.Count > 0)
                {
                    if (IsExpanded(nameSpaceNode.Id))
                    {
                        StringToTreeView(nameSpaceNode.Children, childItem, rows);
                    }
                    else
                    {
                        childItem.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1 };
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (NameSpaceNodeTreeViewItem)args.item;
            
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);

                if (columnIndex == 0)
                {
                    item.Data.WasSelected = GUI.Toggle(cellRect, false, GUIContent.none);
                }
                else if (columnIndex == 1)
                {
                    base.RowGUI(args);
                }
            }
        }

        private class NameSpaceNodeTreeViewItem : TreeViewItem
        {
            public NameSpaceNode Data;
        }
    }
}