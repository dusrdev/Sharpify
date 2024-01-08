# CHANGELOG

## v1.0.3

* Upgraded concurrency synchronization model of `Database` and `Database{T}` to get more accurate reads when other threads are writing.
* Both now implement `IDisposable` to release the resources of the synchronization and should be disposed of properly, but since they are designed to be used throughout the lifetime of the application, it isn't absolutely crucial to do this, and the implement finalizer should take care of this, if disposing of it from your end is inconvenient

## v1.0.2

* Fixed issue where an exception would be thrown if `Upsert` overrides a key. it should by design override.

## v1.0.1

* Further optimized both database, heavily utilizing array pooling for encryption

## v1.0.0

* Initial release - check github for information
