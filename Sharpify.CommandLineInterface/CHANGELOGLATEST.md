# CHANGELOG

## Version 1.1.0

### Changes to `CliBuilder`

* `DoNotIncludeMetadataInHelpText` was removed, instead it will not be included by default. `ModifyMetadata` was renamed to `WithMetadata` and if used, will modify the default `CliMetadata` and include it in the help text.
* Added `WithCustomHeader(string)` as an alternative to using `CliRunnerMetadata`, there will be no exception when both are used, but in that case, `CliRunnerMetadata` has priority and will be the only one displayed.
* Added `SortCommandsAlphabetically`, which if specified will sort the commands alphabetically by name in the general help text, other than the help text, it has virtually no affect. Not specifying this, gives you control over the order, it will be exactly in the order that you added the commands and order of existing collection (if you added any commands via a collection).

### Changes to `Arguments`

* Overloads of `TryGetValue<TEnum>` were modified to add an option to `ignoreCase`, to make it more user friendly and still adhere to parameter placement guidelines, more overloads were added.
