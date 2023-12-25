# CHANGELOG

* Addressed issue which resulted in some parts of the library having older implementations when downloading the package using nuget.
* Added new `StringBuffer` collection, which rents a buffer of specified length, and allows efficient appending of elements without using any low level apis, indexing or slice management. And with zero costs to performance (tested in benchmarks)
* `StringBuffer` was internally integrated to replace almost all low level buffer manipulations
* `AesProvider.EncryptUrl` and `AesProvider.DecryptUrl` in **.NET8 or later** were optimized to minimize allocations using `StringBuffer` and using hardware intrinsics api's for character replacement.
* **BREAKING** Removed `String.Suffix` as the abstraction is the same as `String.Concat` which already uses a very good implementation
* Updated project properties for better end user support via nuget.
