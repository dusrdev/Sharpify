# CHANGELOG

* Introduced new `MonitoredSerializableObject{T}` class that will serialize an object to a file, continuously monitor the file and synchronize changes, it also exposes an event that will fire on once the object has changed, whether by internal or external operations. The description says object for lack of a better word, it should work for structs as well.
* Added implicit converters to `ReadOnlySpan{Char}` for `StringBuffer` and `AllocatedStringBuffer`, which can enable usage of the buffer without any allocation in api's that accept `ReadOnlySpan{Char}`.
