using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Google.Protobuf.CodeGen
{
    [PublicAPI]
    internal class ProtobufILPostProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly) => compiledAssembly.Name == "Google.Protobuf.Unsafe";

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
            {
                return new ILPostProcessResult(null);
            }

            AssemblyDefinition assembly = null;
            MemoryStream peStream = null;
            MemoryStream pdbStream = null;

            try
            {
                var inMemoryAssembly = compiledAssembly.InMemoryAssembly;
                peStream = new MemoryStream(inMemoryAssembly.PeData);
                pdbStream = new MemoryStream(inMemoryAssembly.PdbData);

                // For IL Post Processing, use the builtin symbol reader provider
                assembly = LoadAssembly(peStream, pdbStream, new PortablePdbReaderProvider());
                TypeDefinition type = assembly.MainModule.GetType("System.Runtime.CompilerServices.Unsafe");
                Dictionary<string, Action<MethodDefinition>> methodImpls = GetUnsafeMethodImpls();

                foreach (MethodDefinition method in type.Methods)
                {
                    if (methodImpls.TryGetValue(method.Name, out Action<MethodDefinition> impl))
                    {
                        impl(method);
                    }
                }

                return new ILPostProcessResult(WriteAssemblyToMemory(assembly));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Internal compiler error for Protobuf ILPostProcessor on {compiledAssembly.Name}. Exception: {ex}");
            }
            finally
            {
                assembly?.Dispose();
                peStream?.Dispose();
                pdbStream?.Dispose();
            }
        }

        private static Dictionary<string, Action<MethodDefinition>> GetUnsafeMethodImpls()
        {
            MethodInfo[] methods = typeof(UnsafeMethodImpls).GetMethods(BindingFlags.Public | BindingFlags.Static);

            return methods.ToDictionary(
                method => method.Name,
                method => (Action<MethodDefinition>)method.CreateDelegate(typeof(Action<MethodDefinition>), null)
            );
        }

        private static AssemblyDefinition LoadAssembly(Stream peStream, Stream pdbStream, ISymbolReaderProvider symbolReader = null)
        {
            peStream.Seek(0, SeekOrigin.Begin);
            pdbStream.Seek(0, SeekOrigin.Begin);

            var readerParameters = new ReaderParameters
            {
                InMemory = true,
                ReadingMode = ReadingMode.Immediate
            };

            if (symbolReader != null)
            {
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolReaderProvider = symbolReader;
            }

            try
            {
                readerParameters.SymbolStream = pdbStream;
                return AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            }
            catch
            {
                readerParameters.ReadSymbols = false;
                readerParameters.SymbolStream = null;
                peStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);
                return AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            }
        }

        private static InMemoryAssembly WriteAssemblyToMemory(AssemblyDefinition assembly)
        {
            using var peStream = new MemoryStream();
            using var pdbStream = new MemoryStream();
            var writeParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                WriteSymbols = true,
                SymbolStream = pdbStream
            };

            assembly.Write(peStream, writeParameters);
            return new InMemoryAssembly(peStream.ToArray(), pdbStream.ToArray());
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{nameof(ProtobufILPostProcessor)}: {message}");
        }
    }
}
