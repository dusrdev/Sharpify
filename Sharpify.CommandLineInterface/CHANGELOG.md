# CHANGELOG

## Version 1.0.4

* Added missing line break in global help text
* If the single word help is entered, it will now be recognized in place of command name to return the global help text, instead of trying to be parsed as a command.

## Version 1.0.3

* Updated `Sharpify` dependency and implemented usage of new APIs to aid in maintainability.
* Add `DoNotIncludeMetadataInHelpText()` in `CliBuilder` which removes the metadata inclusion in the general help text.

## Version 1.0.2

* Removed thread-local `StringBuilder` from `CliRunner`, replaced all usages with `StringBuffer` from `Sharpify`

## Version 1.0.1

* Updated `Sharpify` dependency
* Slightly improved performance of general help text generator

## Version 1.0.0

Initial version - no changes
