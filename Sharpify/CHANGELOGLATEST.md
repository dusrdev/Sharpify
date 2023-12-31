# CHANGELOG

* Addressed issue which resulted in some parts of the library having older implementations when downloading the package using nuget.
* Added new `StringBuffer` collection, which rents a buffer of specified length, and allows efficient appending of elements without using any low level apis, indexing or slice management. And with zero costs to performance (tested in benchmarks), for smaller lengths it is more recommended to use `AllocatedStringBuffer` with `stackalloc`, for larger than about 1024 characters it would be better to use `StringBuffer` as the it would create less pressure on the system, at those scales `stackalloc` can become slow and sometimes may even fail.
* Added new `AllocateStringBuffer` which is similar to `StringBuffer` but requires a preallocated buffer - allowing usage of `stackalloc`.
* `StringBuffer` and `AllocatedStringBuffer` were internally integrated to replace almost all low level buffer manipulations
* `AesProvider.EncryptUrl` and `AesProvider.DecryptUrl` in **.NET8 or later** were optimized to minimize allocations and using hardware intrinsics api's for character replacement.
* **BREAKING** Removed `String.Suffix` as the abstraction is the same as `String.Concat` which already uses a very good implementation
* Updated project properties for better end user support via nuget.
