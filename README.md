# Protocol Buffers for Unity

[![openupm](https://img.shields.io/npm/v/com.stalomeow.google-protobuf?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.cn/packages/com.stalomeow.google-protobuf/)

**None of the repo, the tool, nor the repo owner is affiliated with, or sponsored or authorized by, Google or its affiliates.**

## Overview

Protocol Buffers (a.k.a., protobuf) are Google's language-neutral, platform-neutral, extensible mechanism for serializing structured data. For more information, please check the original repository at [https://github.com/protocolbuffers/protobuf](https://github.com/protocolbuffers/protobuf).

This repo offers a package that simplifies the process of importing the latest version of Protocol Buffers into Unity. The source code of Protocol Buffers, by default v24.4 (**without any modifications**), can be found in the [Runtime/Core](Runtime/Core) folder.

## Dependencies

- Mono Cecil

## Message Pool

This package provides a thread-safe and lock-free object pool for Protobuf Messages. The codes can be found in [Runtime/Pool/MessagePool.cs](Runtime/Pool/MessagePool.cs).

### Customize C# Proto File Output

In order to take advantage of the Message Pool, we should modify `protoc`, the compiler of protobuf, to customize the C# File output.

I have done it for you but you should compile `protoc` manually. The codes can be found at [https://github.com/stalomeow/protobuf](https://github.com/stalomeow/protobuf). With this customized compiler, all messages implement `System.IDisposable` interface and will be recycled into Message Pool when their `Dispose` methods are called.

## Unity Editor Supports

![editor-utils](/Screenshots~/editor_utils.png)

## Best Practices in Unity

- Use **LESS** immutable reference types such as `string`, `bytes`, etc, because they can not be reused.
- **DO NOT** use Well-Known Types and Common Types **UNLESS** you recompile them manually otherwise they can not be reused and will cause C# compilation errors.
- **DO NOT** use `foreach` on a Repeated Field because it will cause extra GC allocation. Simply use `for` instead.
- Use **LESS** Map Field because it has low performance and will cause extra GC allocation when being enumerated.
- **DO** use `MessageParser<T>.ParseFrom(ReadOnlySpan<byte> data)` or `MessageParser<T>.ParseFrom(ReadOnlySequence<byte> data)` when deserializing Messages because they do not cause extra GC allocation.
- **DO** use `MessageExtensions.WriteTo(this IMessage message, Span<byte> output)` when serializing Messages because it does not cause extra GC allocation.
- Others can be found in [Protobuf's Official Document](https://protobuf.dev/programming-guides/dos-donts/).
- To be continued...

## License

The codes in the Runtime/Core folder are licensed under [Google's LICENSE](Runtime/Core/LICENSE), and the rest are licensed under [the MIT LICENSE](LICENSE).