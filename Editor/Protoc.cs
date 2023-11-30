using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Google.Protobuf.Editor
{
    public static class Protoc
    {
        [MenuItem("Protobuf/Open Settings...", priority = 0)]
        public static void OpenSettings()
        {
            ProtobufSettings.OpenInProjectSettings();
        }

        [MenuItem("Protobuf/Clean C# Output Directory", priority = 1)]
        public static void CleanCSharpOutputDirectory()
        {
            var settings = ProtobufSettings.instance;
            string directory = settings.OutputPath;

            if (!Directory.Exists(directory))
            {
                return;
            }

            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        [MenuItem("Protobuf/Generate C# Proto Files", priority = 2)]
        public static void GenerateCSharpProtoFiles()
        {
            var settings = ProtobufSettings.instance;
            var startInfo = new ProcessStartInfo
            {
                FileName = settings.CompilerPath,
                Arguments = $"--csharp_out=\"{settings.OutputPath}\" " +
                            $"--csharp_opt={GetCSharpOptionsArgValue(settings)} " +
                            $"{GetProtoPathArgs(settings)} " +
                            $"{GetProtoFileArgs(settings)} {settings.AdditionalArgs ?? string.Empty}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            // Debug.Log(startInfo.Arguments);

            if (!Directory.Exists(settings.OutputPath))
            {
                Directory.CreateDirectory(settings.OutputPath);
            }

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    Debug.LogError("Failed to start proto compiler.");
                }
                else if (process.WaitForExit(settings.CompileMillisecondTimeout))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                    {
                        Debug.Log(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                    }

                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError("Generate C# Proto Files Timeout.");
                }
            }
        }

        private static string GetProtoFileArgs(ProtobufSettings settings)
        {
            string[] files = Directory.GetFiles(settings.InputDirectory, "*.proto", SearchOption.AllDirectories);
            return string.Join(" ", files.Select(file => $"\"{file}\""));
        }

        private static string GetProtoPathArgs(ProtobufSettings settings)
        {
            var protoPaths = new List<string>(settings.ProtoPaths);
            protoPaths.Insert(0, settings.InputDirectory);
            return string.Join(" ", protoPaths.Select(path => $"--proto_path=\"{path}\""));
        }

        private static string GetCSharpOptionsArgValue(ProtobufSettings settings)
        {
            var csharpOptionArgs = new StringBuilder();

            csharpOptionArgs.AppendFormat("file_extension={0}", settings.FileExtension);

            if (settings.EnableBaseNamespace)
            {
                csharpOptionArgs.AppendFormat(",base_namespace={0}", settings.BaseNamespace ?? "");
            }

            if (settings.InternalAccess)
            {
                csharpOptionArgs.Append(",internal_access");
            }

            if (settings.Serializable)
            {
                csharpOptionArgs.Append(",serializable");
            }

            return csharpOptionArgs.ToString();
        }
    }
}
