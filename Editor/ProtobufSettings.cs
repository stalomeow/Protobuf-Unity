using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Google.Protobuf.Editor
{
    [FilePath("ProjectSettings/ProtobufSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class ProtobufSettings : ScriptableSingleton<ProtobufSettings>
    {
        [SerializeField] public string CompilerPath = "protoc";
        [SerializeField] public int CompileMillisecondTimeout = 600_000;
        [SerializeField] public string InputDirectory = "Assets/Protos";
        [SerializeField] public string OutputPath = "Assets/Scripts/Protos";
        [SerializeField] public List<string> ProtoPaths = new();
        [SerializeField] public string FileExtension = ".cs";
        [SerializeField] public string BaseNamespace = "";
        [SerializeField] public bool InternalAccess = false;
        [SerializeField] public bool Serializable = false;
        [SerializeField] public string AdditionalArgs = "";

        private void OnEnable() => hideFlags &= ~HideFlags.NotEditable;

        private void OnDisable() => Save();

        public void Save() => Save(true);

        public SerializedObject AsSerializedObject() => new(this);

        public const string PathInProjectSettings = "Project/Protocol Buffers";

        public static void OpenInProjectSettings() => SettingsService.OpenProjectSettings(PathInProjectSettings);
    }
}
