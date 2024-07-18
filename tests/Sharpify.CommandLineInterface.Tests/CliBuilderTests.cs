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
		var writer = new StringWriter();

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .SetOutputWriter(writer)
                           .Build();
		await cliRunner.RunAsync("echo --help");

		writer.ToString().Should().Contain("echo <message>");
	}

	[Fact]
	public async Task Runner_WithCustomWriterMultipleCommands_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter();

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
	public async Task Runner_WithCustomWriterAndMetadata_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter();

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithMetadata(data => data.Author = "Dave")
                           .Build();
		await cliRunner.RunAsync("--help");

		writer.ToString().Should().Contain("Dave");
	}

	[Fact]
	public async Task Runner_WithCustomWriterAndCustomHeader_OutputsGeneralHelpToWriter() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var writer = new StringWriter();

		var cliRunner = CliRunner.CreateBuilder()
                           .AddCommand(echo)
						   .AddCommand(add)
						   .SetOutputWriter(writer)
						   .WithCustomHeader("Dave")
                           .Build();
		await cliRunner.RunAsync("--help");

		writer.ToString().Should().Contain("Dave");
	}

	[Fact]
	public void Runner_WithOrderedCommands_IsOrdered() {
		var echo = new EchoCommand();
		var add = new AddCommand();
		var sAdd = new SynchronousAddCommand();
		var writer = new StringWriter();

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
}