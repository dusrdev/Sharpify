using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sharpify.CommandLineInterface.Tests;

public class CliBuilderTests {
	[Fact]
	public void Build_WhenEmpty_ReturnsEmpty() {
		var action = () => CliRunner.CreateBuilder().Build();

		Assert.Throws<InvalidOperationException>(action);
	}

	[Fact]
	public void Build_WhenNotEmpty_ReturnsCliRunner() {
		var action = () => CliRunner.CreateBuilder().AddCommand(new EchoCommand()).Build();

		action();
	}

	[Fact]
	public void Build_WhenNotEmpty_ReturnsCliRunnerWithCommands() {
		var echo = new EchoCommand();

		var cliRunner = CliRunner.CreateBuilder().AddCommand(echo).Build();

		Assert.Contains(echo, cliRunner.Commands);
	}

	[Fact]
	public async Task Runner_WithCustomWriter_OutputsCommandHelpToWriter() {
		var echo = new EchoCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .SetOutputWriter(writer)
                           .Build();
		await cliRunner.RunAsync("echo --help");

		Assert.Contains("echo <message>", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriter_SingleCommand_HelpCommand_OutputsAllInfo() {
		var single = new SingleCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(single)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => {
							options.Name = "Single";
							options.Description = "A single command";
							options.Version = "1.0.0";
							options.Author = "David";
							options.License = "MIT";
						   })
                           .Build();
		await cliRunner.RunAsync("help", false);

		var output = writer.ToString();
		Assert.Contains("Single", output);
		Assert.Contains("A single command", output);
		Assert.Contains("Version: 1.0.0", output);
		Assert.Contains("Author: David", output);
		Assert.Contains("License: MIT", output);
		Assert.Contains(single.Usage, output);
	}

	[Theory]
	[InlineData("help")]
	[InlineData("--help")]
	public async Task Runner_WithCustomWriter_SingleCommand_HelpCommand_OutputsCommandUsageToWriter(string input) {
		var single = new SingleCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(single)
						   .SetOutputWriter(writer)
                           .Build();
		await cliRunner.RunAsync(input, false);

		Assert.Contains(single.Usage, writer.ToString());
	}

	[Theory]
	[InlineData("version")]
	[InlineData("--version")]
	public async Task Runner_WithCustomWriter_SingleCommand_VersionCommand_OutputsVersionToWriter(string input) {
		var single = new SingleCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(single)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => options.Version = "1.0.0")
                           .Build();
		await cliRunner.RunAsync(input, false);

		Assert.Contains("Version: 1.0.0", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterMultipleCommands_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
                           .Build();
		await cliRunner.RunAsync("--help");

		Assert.Contains("Echo", writer.ToString());
		Assert.Contains("Add", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAddCommand_ReadOnlySpanInput() {
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(add)
						   .SetOutputWriter(writer)
                           .Build();
		await cliRunner.RunAsync(["add", "1", "2"]);

		Assert.Contains("3", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndMetadata_HelpCommand_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => options.Author = "Dave")
                           .Build();
		await cliRunner.RunAsync("help");

		Assert.Contains("Dave", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndMetadata_HelpFlag_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => options.Author = "Dave")
                           .Build();
		await cliRunner.RunAsync("--help");

		Assert.Contains("Dave", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndMetadata_VersionCommand_OutputsVersionToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => {
                               options.Author = "Dave";
							   options.Version = "1.0.0";
                           })
                           .Build();
		await cliRunner.RunAsync("version");

		Assert.Contains("Version: 1.0.0", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndMetadata_VersionFlag_OutputsVersionToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithMetadata(options => {
                               options.Author = "Dave";
							   options.Version = "1.0.0";
                           })
                           .Build();
		await cliRunner.RunAsync("--version");

		Assert.Contains("Version: 1.0.0", writer.ToString());
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndCustomHeader_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithCustomHeader("Dave")
						   .SetHelpTextSource(HelpTextSource.CustomHeader)
                           .Build();
		await cliRunner.RunAsync("--help");

		Assert.Contains("Dave", writer.ToString());
	}

	[Fact]
	public void Runner_WithOrderedCommands_IsOrdered() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var sAdd = new SynchronousAddCommand();
		var writer = new StringWriter(new StringBuilder(), CultureInfo.CurrentCulture);

		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(echo)
						   .AddCommand(add)
						   .AddCommand(sAdd)
						   .SortCommandsAlphabetically()
						   .SetOutputWriter(writer)
						   .Build();
		var copy = cliRunner.Commands;
		Assert.Equal(add, copy[0]);
		Assert.Equal(echo, copy[1]);
		Assert.Equal(sAdd, copy[2]);
	}

	[Fact]
	public async Task Runner_WithSingleCommand_NoParams() {
		StrongBox<bool> value = new(false);

		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(new SingleCommandNoParams(value))
						   .ConfigureEmptyInputBehavior(EmptyInputBehavior.AttemptToProceed)
						   .Build();
		var exitCode = await cliRunner.RunAsync("", false);

		Assert.Equal(0, exitCode);
		Assert.True(value.Value);
	}

	[Fact]
	public async Task Runner_WithMultipleCommands_CaseSensitive() {
		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(new AddCommand())
						   .AddCommand(new EchoCommand())
						   .ConfigureArgumentCaseHandling(ArgumentCaseHandling.CaseSensitive)
						   .Build();
		var exitCode = await cliRunner.RunAsync("aDD 1 2");

		Assert.NotEqual(0, exitCode);
	}
}