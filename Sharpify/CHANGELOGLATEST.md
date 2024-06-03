# CHANGELOG

## v2.0.0

* Performance improvements to parallel extensions that use `AsyncLocal`
  * The changes are *BREAKING* as now the call sites should use newer convention, behavior will be the same.
  * Instead of the previous `.AsAsyncLocal`, there are now 2 overloads, both use nested generics which is the reason for the update, instead of `IList<T>` they use `<TList, TItem>` where `TList : IList<TItem>`. The first overload with no additional parameters can be used but it will require to specify both generic types as the compiler cannot infer `TItem` for some reason. To partially compensate for the verbosity it creates when using complex types, a second overload accepts a `TItem? @default` argument. Specifying `default(TItem)` there will enable the compiler to infer the generic.

  ```csharp
  // EXAMPLE
  var items = List<int>() { 1, 2, 3 };
  _ = items.AsAsyncLocal<List<int>, int>(); // Overload 1 with no arguments
  _ = items.AsAsyncLocal(default(int)); // Overload 2 with TItem? @default
  ```

  * This might seem like a step backwards but using a nested generic here, can improve performance by not virtualizing interface calls to `IList<T>`, and also enable the compiler to more accurately trim the code when compiling to `NativeAOT`.
* Both overloads of `DecryptBytes` in `AesProvider` now have an optional parameter `throwOnError` which is set to `false` by default, retaining the previous behavior. Setting it to `true` will make sure the exception is thrown in case of a `CryptographicException` instead of just returning an empty array or 0 (respective of the overloads). This might help catch certain issues.
  * Note: Previously and still now, exceptions other than `CryptographicException` would and will be thrown. They are not caught in the methods.
* Implement a trick in `RemoveDuplicatesFromSorted`, `RemoveDuplicates`, `RemoveDuplicatesSorted` and `TryConvertToInt32` to ensure the compiler knows to remove the bounds checking from the loops.
* `IComparer<T>` parameter type in `SortedList<T>` was changed to `Comparer<T>`, the interface virtualization seemed to make custom comparers produce unexpected results.
* `StringBuffer` and `AllocatedStringBuffer` can now use the builder pattern to chain its construction, same as `StringBuilder`.

  ```csharp
  var buffer = StringBuffer.Create(stackalloc char[20]).Append("Hello").AppendLine().Append("World");
  ```

  * This change required each method to return the actual reference of the struct, Use this with caution as this means you can change the actual reference instead of a copy if you manually assign it a new value.
  * From my testing, with correct usage, without manually assigning values to the original variable, previous and builder pattern usage passes my tests. If you encounter any issue with this, make sure to submit it in the GitHub repo.
* Fixed edge case where pausing an `AsyncRoutine` would stop it permanently, regardless of calls to `Resume`.
