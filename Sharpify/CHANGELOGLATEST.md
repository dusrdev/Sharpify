# CHANGELOG

* Added new `StringBuffer` collection, which rents a buffer of specified length, and allows efficient appending of elements without using any low level apis, indexing or slice management. And with zero costs to performance (tested in benchmarks)
* `StringBuffer` was internally integrated to replace almost all low level buffer manipulations
* **BREAKING** Removed `String.Suffix` as the abstraction is the same as `String.Concat` which already uses a very good implementation
* Updated project properties for better end user support via nuget.
