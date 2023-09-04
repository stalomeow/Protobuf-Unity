using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Google.Protobuf.CodeGen
{
    [PublicAPI]
    internal static class UnsafeMethodImpls
    {
        public static void As(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Clear();

            // Impl
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
        }

        public static void Add(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Clear();

            // Impl
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Sizeof, method.GenericParameters[0]);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ret);
        }

        public static void AddByteOffset(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Clear();

            // Impl
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ret);
        }

        public static void ReadUnaligned(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Clear();

            // Impl
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unaligned, (byte)1);
            il.Emit(OpCodes.Ldobj, method.GenericParameters[0]);
            il.Emit(OpCodes.Ret);
        }

        public static void WriteUnaligned(MethodDefinition method)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Clear();

            // Impl
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unaligned, (byte)1);
            il.Emit(OpCodes.Stobj, method.GenericParameters[0]);
            il.Emit(OpCodes.Ret);
        }
    }
}
