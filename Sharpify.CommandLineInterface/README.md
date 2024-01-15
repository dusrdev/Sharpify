# Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is a high performance, reflection free and AOT-ready framework for creating command line interfaces, with a configurable output writer and no direct dependency to `System.Console` enabling it to be embedded and used with inputs from any source.

Most other command line frameworks in c# use `reflection` to provide their "magic" such as generating help text, and providing input validation, `Sharpify.CommandLineInterface` instead uses compile time implemented metadata and static resolve of said metadata for this. each command, must implement the `Command` abstract class, part of which will be to set the command metadata, the main entry `CliRunner` also has an application level metadata object that can be customized in the `CliBuilder` process, using those, `Sharpify.CommandLineInterface` can resolve and format that metadata to generate an output similar to the other frameworks, and each generate it once and cache it to maximize performance.

## Usage

