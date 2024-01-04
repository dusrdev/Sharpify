# CHANGELOG

* Introduced new `SerializableObject{T}` class that will serialize an object to a file and expose an event that will fire on once the object has changed, also a variant `MonitoredSerializableObject` that has the same functionality and in addition it will monitor for external changes within the file system and synchronize them as well.
* Added implicit converters to `ReadOnlySpan{Char}` for `StringBuffer` and `AllocatedStringBuffer`, which can enable usage of the buffer without any allocation in api's that accept `ReadOnlySpan{Char}`.
* Added `[Flags]` attribute to `RoutineOptions` to calm down some IDEs.
* Updated `AesProvider.EncryptBytes` and `AesProvider.DecryptBytes` to use `ReadOnlySpan{byte}` parameters
* Added `AesProvider.EncryptBytes` and `AesProvider.DecryptBytes` overloads that encrypt into a destination span, with guides to length requirements in the summary.
* Added implicit converter to `ReadOnlyMemory{char}` for `StringBuffer` that might help usage in some cases.
