namespace Sharpify.CommandLineInterface.Tests;

/// <summary>
/// This test class specifically tests that arguments are parsed correctly, and more importantly non-positional arguments are not lost during forwarding (Which happens with the cli builder naturally)
/// </summary>
public class ArgumentsIsolatedTests {
	private static readonly Arguments _args = Parser.ParseArguments(
		"command positional --named1 Harold --named2 Finch positional2 --flag")!;

	[Fact]
	public void Positional_BeforeForwarding_ParsedCorrectly() {
		// Positional 0 [Command]
		_args.TryGetValue(0, out var pos0).Should().BeTrue();
		pos0.Should().Be("command");

		// Positional 0 [positional]
		_args.TryGetValue(1, out var pos1).Should().BeTrue();
		pos1.Should().Be("positional");

		// Positional 0 [positional2]
		_args.TryGetValue(2, out var pos2).Should().BeTrue();
		pos2.Should().Be("positional2");
	}

	[Fact]
	public void Positional_AfterForwarding_ParsedCorrectly() {
		var forwarded = _args.ForwardPositionalArguments();

		// "command" no longer exists
		// positional should be at 0, and positional2 at 1.

		// Positional 0 [positional]
		forwarded.TryGetValue(0, out var pos0).Should().BeTrue();
		pos0.Should().Be("positional");

		// Positional 1 [positional2]
		forwarded.TryGetValue(1, out var pos1).Should().BeTrue();
		pos1.Should().Be("positional2");
	}

	[Fact]
	public void Named_BeforeForwarding_ParsedCorrectly() {
		// named1 - Harold
		// named2 - Finch

		_args.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		_args.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Named_AfterForwarding_ParsedCorrectly() {
		var forwarded = _args.ForwardPositionalArguments();

		// named1 - Harold
		// named2 - Finch

		forwarded.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		forwarded.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Flag_BeforeForwarding_ParsedCorrectly() {
		_args.Contains("flag").Should().BeTrue();
		_args.HasFlag("flag").Should().BeTrue();
	}

	[Fact]
	public void Flag_AfterForwarding_ParsedCorrectly() {
		var forwarded = _args.ForwardPositionalArguments();

		forwarded.Contains("flag").Should().BeTrue();
		forwarded.HasFlag("flag").Should().BeTrue();
	}
}