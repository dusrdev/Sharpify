using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sharpify.CommandLineInterface.Tests;

public class CliBuilderTests {
	[Fact]
	public void Build_WhenEmpty_ReturnsEmpty() {
		var action = () => CliRunner.CreateBuilder().Build();

		action.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Build_WhenNotEmpty_ReturnsCliRunner() {
		var action = () => CliRunner.CreateBuilder().AddCommand(new EchoCommand()).Build();

		action.Should().NotThrow<InvalidOperationException>();
	}

	[Fact]
	public void Build_WhenNotEmpty_ReturnsCliRunnerWithCommands() {
		var echo = new EchoCommand();

		var cliRunner = CliRunner.CreateBuilder().AddCommand(echo).Build();

		cliRunner.Commands.Should().Contain(echo);
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

		writer.ToString().Should().Contain("echo <message>");
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
		output.Should().Contain("Single");
		output.Should().Contain("A single command");
		output.Should().Contain("Version: 1.0.0");
		output.Should().Contain("Author: David");
		output.Should().Contain("License: MIT");
		output.Should().Contain(single.Usage);
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

		writer.ToString().Should().Contain(single.Usage);
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

		writer.ToString().Should().Contain("Version: 1.0.0");
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

		writer.ToString().Should().Contain("Echo");
		writer.ToString().Should().Contain("Add");
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

		writer.ToString().Should().Contain("3");
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

		writer.ToString().Should().Contain("Dave");
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

		writer.ToString().Should().Contain("Dave");
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

		writer.ToString().Should().Contain("Version: 1.0.0");
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

		writer.ToString().Should().Contain("Version: 1.0.0");
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

		writer.ToString().Should().Contain("Dave");
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
		copy[0].Should().Be(add);
		copy[1].Should().Be(echo);
		copy[2].Should().Be(sAdd);
	}

	[Fact]
	public async Task Runner_WithSingleCommand_NoParams() {
		StrongBox<bool> value = new(false);

		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(new SingleCommandNoParams(value))
						   .ConfigureEmptyInputBehavior(EmptyInputBehavior.AttemptToProceed)
						   .Build();
		var exitCode = await cliRunner.RunAsync("", false);

		exitCode.Should().Be(0);
		value.Value.Should().BeTrue();
	}

	[Fact]
	public async Task Runner_WithMultipleCommands_CaseSensitive() {
		var cliRunner = CliRunner.CreateBuilder()
						   .AddCommand(new AddCommand())
						   .AddCommand(new EchoCommand())
						   .ConfigureArgumentCaseHandling(ArgumentCaseHandling.CaseSensitive)
						   .Build();
		var exitCode = await cliRunner.RunAsync("aDD 1 2");

		exitCode.Should().NotBe(0);
	}
}