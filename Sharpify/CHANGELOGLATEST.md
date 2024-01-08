# CHANGELOG

* Updated synchronization aspect of `SerializableObject{T}` and `MonitoredSerializableObject{T}`, they now both implement `IDisposable` and finalizers in case you forget to dispose of them, or their context makes it inconvenient.
