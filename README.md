# Protocol Buffers for Unity

[![openupm](https://img.shields.io/npm/v/com.stalomeow.google-protobuf?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.cn/packages/com.stalomeow.google-protobuf/)

**None of the repo, the tool, nor the repo owner is affiliated with, or sponsored or authorized by, Google or its affiliates.**

## Overview

Protocol Buffers (a.k.a., protobuf) are Google's language-neutral, platform-neutral, extensible mechanism for serializing structured data. For more information, please check the original repository at [https://github.com/protocolbuffers/protobuf](https://github.com/protocolbuffers/protobuf).

This repository provides a Unity package that utilizes [my forked version of Protocol Buffers](https://github.com/stalomeow/protobuf/tree/unity), incorporating several optimizations.

## Requirements

- Unity >= 2021.3.
- Mono Cecil >= 1.10.1.

## Highlights

### Fast String

Faster string parsing by defining `GOOGLE_PROTOBUF_SUPPORT_FAST_STRING`.

### Message Pool & Protoc

This package provides a thread-safe object pool for Protobuf Messages. 

- Usage 1

    ``` csharp
    var msg = ExampleMessage.NewFromPool();
    // ...
    msg.Dispose(); // recycle to pool
    ```

- Usage 2

    ``` csharp
    var msg = ExampleMessage.Parser.ParseFrom(data);
    // ...
    msg.Dispose(); // recycle to pool
    ```

The protoc that supports this feature is available here: [https://github.com/stalomeow/protobuf/tree/unity](https://github.com/stalomeow/protobuf/tree/unity); simply compile it according to the documentation, and you'll be able to use it.

## Best Practices in Unity

- **DO NOT** use `foreach` on a Repeated/Map Field because it will cause extra GC allocation. Simply use `for` instead (if possible).
- **DO** use

    - `MessageParser<T>.ParseFrom(ReadOnlySpan<byte> data)`
    - `MessageParser<T>.ParseFrom(ReadOnlySequence<byte> data)`
    - `MessageExtensions.WriteTo(this IMessage message, Span<byte> output)`
    
    when deserializing/serializing Messages because they do not cause extra GC allocation.
- Others can be found in [Protobuf's Official Document](https://protobuf.dev/programming-guides/dos-donts/).
- To be continued...

## License

The codes in the Runtime/Core folder are licensed under [Google's LICENSE](Runtime/Core/LICENSE), and the rest are licensed under [the MIT LICENSE](LICENSE).