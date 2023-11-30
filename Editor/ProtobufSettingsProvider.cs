using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Google.Protobuf.Editor
{
    internal sealed class ProtobufSettingsProvider : SettingsProvider
    {
        private SerializedObject m_SerializedObject;
        private ReorderableList m_ProtoPathList;

        private SerializedProperty m_CompilerPath;
        private SerializedProperty m_CompileMillisecondTimeout;
        private SerializedProperty m_InputDirectory;
        private SerializedProperty m_OutputPath;
        private SerializedProperty m_FileExtension;
        private SerializedProperty m_BaseNamespace;
        private SerializedProperty m_EnableBaseNamespace;
        private SerializedProperty m_InternalAccess;
        private SerializedProperty m_Serializable;
        private SerializedProperty m_AdditionalArgs;

        public ProtobufSettingsProvider(IEnumerable<string> keywords = null)
            : base(ProtobufSettings.PathInProjectSettings, SettingsScope.Project, keywords) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            ProtobufSettings.instance.Save();
            m_SerializedObject = ProtobufSettings.instance.AsSerializedObject();

            SerializedProperty protoPaths = m_SerializedObject.FindProperty("ProtoPaths");
            m_ProtoPathList = new ReorderableList(m_SerializedObject, protoPaths,
                true, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, Labels.ProtoPaths),
                drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
                {
                    SerializedProperty element = protoPaths.GetArrayElementAtIndex(index);
                    element.stringValue = EditorGUI.TextField(rect, element.stringValue);
                }
            };

            // initialize properities
            FieldInfo[] fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(SerializedProperty))
                {
                    SerializedProperty property = m_SerializedObject.FindProperty(field.Name[2..]);
                    field.SetValue(this, property);
                }
            }
        }

        public override void OnGUI(string searchContext)
        {
            m_SerializedObject.Update();
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(15);

            EditorGUILayout.LabelField("Basic Compiler Options", EditorStyles.boldLabel);
            m_CompilerPath.stringValue = EditorGUILayout.TextField(Labels.CompilerPath, m_CompilerPath.stringValue);
            m_CompileMillisecondTimeout.intValue = EditorGUILayout.IntField(Labels.CompileMillisecondTimeout, m_CompileMillisecondTimeout.intValue);
            DrawFolderPathProperty(Labels.InputDirectory, m_InputDirectory);
            DrawFolderPathProperty(Labels.OutputPath, m_OutputPath);
            m_ProtoPathList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Advanced Compiler Options", EditorStyles.boldLabel);
            m_FileExtension.stringValue = EditorGUILayout.TextField(Labels.FileExtension, m_FileExtension.stringValue);

            m_EnableBaseNamespace.boolValue = EditorGUILayout.Toggle(Labels.EnableBaseNamespace, m_EnableBaseNamespace.boolValue);
            using (new EditorGUI.DisabledScope(!m_EnableBaseNamespace.boolValue))
            {
                EditorGUI.indentLevel++;
                m_BaseNamespace.stringValue = EditorGUILayout.TextField(Labels.BaseNamespace, m_BaseNamespace.stringValue);
                EditorGUI.indentLevel--;
            }

            m_InternalAccess.boolValue = EditorGUILayout.Toggle(Labels.InternalAccess, m_InternalAccess.boolValue);
            m_Serializable.boolValue = EditorGUILayout.Toggle(Labels.Serializable, m_Serializable.boolValue);
            m_AdditionalArgs.stringValue = EditorGUILayout.TextField(Labels.AdditionalArgs, m_AdditionalArgs.stringValue);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
                ProtobufSettings.instance.Save();
            }
        }

        public override void OnTitleBarGUI()
        {
            if (EditorGUILayout.DropdownButton(Labels.HelpMenuIcon, FocusType.Passive, EditorStyles.label))
            {
                Help.ShowHelpPage("https://protobuf.dev/");
            }
        }

        private static void DrawFolderPathProperty(GUIContent label, SerializedProperty prop)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                prop.stringValue = EditorGUILayout.TextField(label, prop.stringValue);

                if (GUILayout.Button("Browse...", GUILayout.MaxWidth(80)))
                {
                    string currentAbsFolder = Path.GetFullPath(prop.stringValue);
                    string newFolder = EditorUtility.OpenFolderPanel(label.text, currentAbsFolder, "");

                    if (!string.IsNullOrWhiteSpace(newFolder))
                    {
                        prop.stringValue = newFolder;
                        GUI.changed = true;
                    }
                }
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            return new ProtobufSettingsProvider();
        }

        private static class Labels
        {
            public static readonly GUIContent HelpMenuIcon = EditorGUIUtility.IconContent("_Help");

            public static readonly GUIContent CompilerPath = new("Compiler Path");

            public static readonly GUIContent CompileMillisecondTimeout = new("Compile Timeout (ms)");

            public static readonly GUIContent InputDirectory = new("Source Directory");

            public static readonly GUIContent OutputPath = new("C# Output Directory");

            public static readonly GUIContent ProtoPaths = new("Proto Import Paths",
                "Specify the directory in which to search for imports. Directories will be searched in order.");

            public static readonly GUIContent FileExtension = new("File Extension",
                "Sets the file extension for generated code. This defaults to .cs, but a common alternative is .g.cs to indicate that the file contains generated code.");

            public static readonly GUIContent BaseNamespace = new("Base Namespace",
                "When this option is specified, the generator creates a directory hierarchy for generated source code corresponding to the namespaces of the generated classes, using the value of the option to indicate which part of the namespace should be considered as the \"base\" for the output directory.");

            public static readonly GUIContent EnableBaseNamespace = new("Enable Base Namespace",
                "Enables Base Namespace.");

            public static readonly GUIContent InternalAccess = new("Internal Access",
                "When this option is specified, the generator creates types with the internal access modifier instead of public.");

            public static readonly GUIContent Serializable = new("Serializable",
                "When this option is specified, the generator adds the [Serializable] attribute to generated message classes.");

            public static readonly GUIContent AdditionalArgs = new("Additional Args");
        }
    }
}
