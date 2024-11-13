namespace Sharpify.CommandLineInterface.Tests;

/// <summary>
/// This test class specifically tests that arguments are parsed correctly, and more importantly non-positional arguments are not lost during forwarding (Which happens with the cli builder naturally)
/// </summary>
public class ArgumentsIsolatedTests {
	private static readonly Arguments Args = Parser.ParseArguments(
		"command positional --named1 Harold --named2 Finch positional2 --flag --words \"word1|word2\" --numbers \"1|2\"")!;
    [Fact]
	public void Arguments_Positional_BeforeForwarding() {
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
	public void Arguments_Positional_AfterForwarding() {
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
	public void Arguments_Named_BeforeForwarding() {
		// named1 - Harold
		// named2 - Finch

		Args.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		Args.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Arguments_Named_AfterForwarding() {
		var forwarded = Args.ForwardPositionalArguments();

		// named1 - Harold
		// named2 - Finch

		forwarded.TryGetValue("named1", out var firstName).Should().BeTrue();
		firstName.Should().Be("Harold");

		forwarded.TryGetValue("named2", out var lastName).Should().BeTrue();
		lastName.Should().Be("Finch");
	}

	[Fact]
	public void Arguments_Flag_BeforeForwarding() {
		Args.HasFlag("flag").Should().BeTrue();
	}

	[Fact]
	public void Arguments_Flag_AfterForwarding() {
		var forwarded = Args.ForwardPositionalArguments();
		forwarded.HasFlag("flag").Should().BeTrue();
	}

	[Fact]
	public void Arguments_Contains_Key() {
		Args.Contains("named1").Should().BeTrue();
	}

	[Fact]
	public void Arguments_Contains_Position() {
		Args.Contains(0).Should().BeTrue();
	}

	[Fact]
	public void Arguments_Named_Array_String() {
		Args.TryGetValues("words", "|", out var words).Should().BeTrue();
		words.Should().BeEquivalentTo(["word1", "word2"]);
	}

	[Fact]
	public void Arguments_Named_MultipleKeys_Array_String() {
		var args = Parser.ParseArguments("command --words word1|word2")!;
		args.TryGetValues(["words", "w"], "|", out var words).Should().BeTrue();
		words.Should().BeEquivalentTo(["word1", "word2"]);
		args = Parser.ParseArguments("command --w word1|word2")!;
		args.TryGetValues(["words", "w"], "|", out var w).Should().BeTrue();
		w.Should().BeEquivalentTo(["word1", "word2"]);
	}

	[Fact]
	public void Arguments_Named_MultipleKeys_Array_Int() {
		var args = Parser.ParseArguments("command --numbers 1|2")!;
		args.TryGetValues<int>(["numbers", "n"], "|", out var numbers).Should().BeTrue();
		numbers.Should().BeEquivalentTo([1, 2]);
		args = Parser.ParseArguments("command -n 1|2")!;
		args.TryGetValues<int>(["numbers", "n"], "|", out var n).Should().BeTrue();
		n.Should().BeEquivalentTo([1, 2]);
	}

	[Fact]
	public void Arguments_Positional_Array_String() {
		var args = Parser.ParseArguments("command q1|q2")!;
		args.TryGetValues(1, "|", out var words).Should().BeTrue();
		words.Should().BeEquivalentTo(["q1", "q2"]);
	}

	[Fact]
	public void Arguments_Named_Array_Int() {
		Args.TryGetValues<int>("numbers", "|", out var numbers).Should().BeTrue();
		numbers.Should().BeEquivalentTo([1, 2]);
	}

	[Fact]
	public void Arguments_Positional_Array_Int() {
		var args = Parser.ParseArguments("command 1|2")!;
		args.TryGetValues<int>(1, "|", out var numbers).Should().BeTrue();
		numbers.Should().BeEquivalentTo([1, 2]);
	}

	[Fact]
	public void Arguments_TryGetValue_Named_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		args.TryGetValue("x", 0, out int x).Should().BeTrue();
		x.Should().Be(5);
		args.TryGetValue("y", 0, out int y).Should().BeFalse();
		y.Should().Be(0);
	}

	[Fact]
	public void Arguments_TryGetValue_Named_Double() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		args.TryGetValue("x", 0, out double x).Should().BeTrue();
		x.Should().Be(5);
		args.TryGetValue("y", 0, out double y).Should().BeFalse();
		y.Should().Be(0);
	}

	[Fact]
	public void Arguments_GetValue_Named_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		args.GetValue("x", 0).Should().Be(5);
		args.GetValue("y", 0).Should().Be(0);
	}

	[Fact]
	public void Arguments_GetValue_Named_MultipleKeys_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		args.GetValue(["x", "one"], 0).Should().Be(5);
		args = Parser.ParseArguments("command --one 5 -y Hello")!;
		args.GetValue(["x", "one"], 0).Should().Be(5);
	}

	[Fact]
	public void Arguments_TryGetValue_Positional_Int() {
		var args = Parser.ParseArguments("command 5 Hello")!;
		args.TryGetValue(1, 0, out double x).Should().BeTrue();
		x.Should().Be(5);
		args.TryGetValue(2, 0, out double y).Should().BeFalse();
		y.Should().Be(0);
	}

	[Fact]
	public void Arguments_GetValue_Positional_Int() {
		var args = Parser.ParseArguments("command 5 Hello")!;
		args.GetValue(1, 0).Should().Be(5);
		args.GetValue(2, 0).Should().Be(0);
	}

	[Fact]
	public void Arguments_TryGetEnum_Positional() {
		var args = Parser.ParseArguments("command Blue")!;
		args.TryGetEnum(1, out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_TryGetEnum_Positional_IgnoreCase() {
		var args = Parser.ParseArguments("command bLue")!;
		args.TryGetEnum(1, true, out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named() {
		var args = Parser.ParseArguments("command --color Blue")!;
		args.TryGetEnum("color", out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		args.TryGetEnum("color", true, out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_MultipleKeys() {
		var args = Parser.ParseArguments("command --color Blue")!;
		args.TryGetEnum(["color", "c"], out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
		args = Parser.ParseArguments("command -c Blue")!;
		args.TryGetEnum(["color", "c"], out ConsoleColor c).Should().BeTrue();
		c.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_MultipleKeys_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		args.TryGetEnum(["color", "c"], true, out ConsoleColor color).Should().BeTrue();
		color.Should().Be(ConsoleColor.Blue);
		args = Parser.ParseArguments("command -c bLue")!;
		args.TryGetEnum(["color", "c"], true, out ConsoleColor c).Should().BeTrue();
		c.Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Positional() {
		var args = Parser.ParseArguments("command Blue")!;
		args.GetEnum(1, ConsoleColor.Black).Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Positional_IgnoreCase() {
		var args = Parser.ParseArguments("command bLue")!;
		args.GetEnum(1, ConsoleColor.Black, true).Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Named() {
		var args = Parser.ParseArguments("command --color Blue")!;
		args.GetEnum("color", ConsoleColor.Black).Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Named_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		args.GetEnum("color", ConsoleColor.Black, true).Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Named_MultipleKeys() {
		var args = Parser.ParseArguments("command --color Blue")!;
		args.GetEnum(["color", "c"], ConsoleColor.Black).Should().Be(ConsoleColor.Blue);
		args = Parser.ParseArguments("command -c Blue")!;
		args.GetEnum(["color", "c"], ConsoleColor.Black).Should().Be(ConsoleColor.Blue);
	}

	[Fact]
	public void Arguments_GetEnum_Named_MultipleKeys_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		args.GetEnum(["color", "c"], ConsoleColor.Black, true).Should().Be(ConsoleColor.Blue);
		args = Parser.ParseArguments("command -c bLue")!;
		args.GetEnum(["color", "c"], ConsoleColor.Black, true).Should().Be(ConsoleColor.Blue);
	}
}