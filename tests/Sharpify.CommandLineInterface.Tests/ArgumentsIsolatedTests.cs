namespace Sharpify.CommandLineInterface.Tests;

/// <summary>
/// This test class specifically tests that arguments are parsed correctly, and more importantly non-positional arguments are not lost during forwarding (Which happens with the cli builder naturally)
/// </summary>
public class ArgumentsIsolatedTests {
	private static readonly Arguments Args = Parser.ParseArguments(
		"command positional --named1 Harold --named2 Finch positional2 --flag --words \"word1|word2\" --numbers \"1|2\"")!;
    private static readonly int[] Expectation = [1, 2];

    [Fact]
	public void Positional_BeforeForwarding_ParsedCorrectly() {
		// Positional 0 [Command]
		Args.TryGetValue(0, out var pos0).Should().BeTrue();
		pos0.Should().Be("command");

		// Positional 0 [positional]
		Args.TryGetValue(1, out var pos1).Should().BeTrue();
		pos1.Should().Be("positional");

		// Positional 0 [positional2]
		Args.TryGetValue(2, out var pos2).Should().BeTrue();
		pos2.Should().Be("positional2");
	}

	[Fact]
	public void Positional_AfterForwarding_ParsedCorrectly() {
		var forwarded = Args.ForwardPositionalArguments();

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

		Args.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		Args.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Named_AfterForwarding_ParsedCorrectly() {
		var forwarded = Args.ForwardPositionalArguments();

		// named1 - Harold
		// named2 - Finch

		forwarded.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		forwarded.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Flag_BeforeForwarding_ParsedCorrectly() {
		Args.HasFlag("flag").Should().BeTrue();
	}

	[Fact]
	public void Flag_AfterForwarding_ParsedCorrectly() {
		var forwarded = Args.ForwardPositionalArguments();
		forwarded.HasFlag("flag").Should().BeTrue();
	}

	[Fact]
	public void Named_Array_String_ParsedCorrectly() {
		Args.TryGetValues("words", "|", out var words).Should().BeTrue();
		words.Should().BeEquivalentTo(["word1", "word2"]);
	}

	[Fact]
	public void Named_Array_Int_ParsedCorrectly() {
		Args.TryGetValues<int>("numbers", "|", out var numbers).Should().BeTrue();
		numbers.Should().BeEquivalentTo(Expectation);
	}
}