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
		Assert.True(Args.TryGetValue(0, out var pos0));
		Assert.Equal("command", pos0);

		// Positional 0 [positional]
		Assert.True(Args.TryGetValue(1, out var pos1));
		Assert.Equal("positional", pos1);

		// Positional 0 [positional2]
		Assert.True(Args.TryGetValue(2, out var pos2));
		Assert.Equal("positional2", pos2);
	}

	[Fact]
	public void Arguments_Positional_AfterForwarding() {
		var forwarded = Args.ForwardPositionalArguments();

		// "command" no longer exists
		// positional should be at 0, and positional2 at 1.

		// Positional 0 [positional]
		Assert.True(forwarded.TryGetValue(0, out var pos0));
		Assert.Equal("positional", pos0);

		// Positional 1 [positional2]
		Assert.True(forwarded.TryGetValue(1, out var pos1));
		Assert.Equal("positional2", pos1);
	}

	[Fact]
	public void Arguments_Named_BeforeForwarding() {
		// named1 - Harold
		// named2 - Finch

		Assert.True(Args.TryGetValue("named1", out var firstName));
		Assert.Equal("Harold", firstName);

		Assert.True(Args.TryGetValue("named2", out var lastName));
		Assert.Equal("Finch", lastName);
	}

	[Fact]
	public void Arguments_Named_AfterForwarding() {
		var forwarded = Args.ForwardPositionalArguments();

		// named1 - Harold
		// named2 - Finch

		Assert.True(forwarded.TryGetValue("named1", out var firstName));
		Assert.Equal("Harold", firstName);

		Assert.True(forwarded.TryGetValue("named2", out var lastName));
		Assert.Equal("Finch", lastName);
	}

	[Fact]
	public void Arguments_Flag_BeforeForwarding() {
		Assert.True(Args.HasFlag("flag"));
	}

	[Fact]
	public void Arguments_Flag_AfterForwarding() {
		var forwarded = Args.ForwardPositionalArguments();
		Assert.True(forwarded.HasFlag("flag"));
	}

	[Fact]
	public void Arguments_Contains_Key() {
		Assert.True(Args.Contains("named1"));
	}

	[Fact]
	public void Arguments_Contains_Position() {
		Assert.True(Args.Contains(0));
	}

	[Fact]
	public void Arguments_Named_Array_String() {
		Assert.True(Args.TryGetValues("words", "|", out var words));
		Assert.Equal(["word1", "word2"], words); 
	}

	[Fact]
	public void Arguments_Named_MultipleKeys_Array_String() {
		var args = Parser.ParseArguments("command --words word1|word2")!;
		Assert.True(args.TryGetValues(["words", "w"], "|", out var words));
		Assert.Equal(["word1", "word2"], words);
		args = Parser.ParseArguments("command --w word1|word2")!;
		Assert.True(args.TryGetValues(["words", "w"], "|", out var w));
		Assert.Equal(["word1", "word2"], w);
	}

	[Fact]
	public void Arguments_Named_MultipleKeys_Array_Int() {
		var args = Parser.ParseArguments("command --numbers 1|2")!;
		Assert.True(args.TryGetValues<int>(["numbers", "n"], "|", out var numbers));
		Assert.Equal([1, 2], numbers);
		args = Parser.ParseArguments("command -n 1|2")!;
		Assert.True(args.TryGetValues<int>(["numbers", "n"], "|", out var n));
		Assert.Equal([1, 2], n);
	}

	[Fact]
	public void Arguments_Positional_Array_String() {
		var args = Parser.ParseArguments("command q1|q2")!;
		Assert.True(args.TryGetValues(1, "|", out var words));
		Assert.Equal(["q1", "q2"], words);
	}

	[Fact]
	public void Arguments_Named_Array_Int() {
		Assert.True(Args.TryGetValues<int>("numbers", "|", out var numbers));
		Assert.Equal([1, 2], numbers);
	}

	[Fact]
	public void Arguments_Positional_Array_Int() {
		var args = Parser.ParseArguments("command 1|2")!;
		Assert.True(args.TryGetValues<int>(1, "|", out var numbers));
		Assert.Equal([1, 2], numbers);
	}

	[Fact]
	public void Arguments_TryGetValue_Named_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		Assert.True(args.TryGetValue("x", 0, out int x));
		Assert.Equal(5, x);
		Assert.False(args.TryGetValue("y", 0, out int y));
		Assert.Equal(0, y);
	}

	[Fact]
	public void Arguments_TryGetValue_Named_Double() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		Assert.True(args.TryGetValue("x", 0, out double x));
		Assert.Equal(5, x);
		Assert.False(args.TryGetValue("y", 0, out double y));
		Assert.Equal(0, y);
	}

	[Fact]
	public void Arguments_GetValue_Named_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		Assert.Equal(5, args.GetValue("x", 0));
		Assert.Equal(0, args.GetValue("y", 0));
	}

	[Fact]
	public void Arguments_GetValue_Named_MultipleKeys_Int() {
		var args = Parser.ParseArguments("command -x 5 -y Hello")!;
		Assert.Equal(5, args.GetValue(["x", "one"], 0));
		args = Parser.ParseArguments("command --one 5 -y Hello")!;
		Assert.Equal(5, args.GetValue(["x", "one"], 0));
	}

	[Fact]
	public void Arguments_TryGetValue_Positional_Int() {
		var args = Parser.ParseArguments("command 5 Hello")!;
		Assert.True(args.TryGetValue(1, 0, out double x));
		Assert.Equal(5, x);
		Assert.False(args.TryGetValue(2, 0, out double y));
		Assert.Equal(0, y);
	}

	[Fact]
	public void Arguments_GetValue_Positional_Int() {
		var args = Parser.ParseArguments("command 5 Hello")!;
		Assert.Equal(5, args.GetValue(1, 0));
		Assert.Equal(0, args.GetValue(2, 0));
	}

	[Fact]
	public void Arguments_TryGetEnum_Positional() {
		var args = Parser.ParseArguments("command Blue")!;
		Assert.True(args.TryGetEnum(1, out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
	}

	[Fact]
	public void Arguments_TryGetEnum_Positional_IgnoreCase() {
		var args = Parser.ParseArguments("command bLue")!;
		Assert.True(args.TryGetEnum(1, true, out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named() {
		var args = Parser.ParseArguments("command --color Blue")!;
		Assert.True(args.TryGetEnum("color", out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		Assert.True(args.TryGetEnum("color", true, out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_MultipleKeys() {
		var args = Parser.ParseArguments("command --color Blue")!;
		Assert.True(args.TryGetEnum(["color", "c"], out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
		args = Parser.ParseArguments("command -c Blue")!;
		Assert.True(args.TryGetEnum(["color", "c"], out ConsoleColor c));
		Assert.Equal(ConsoleColor.Blue, c);
	}

	[Fact]
	public void Arguments_TryGetEnum_Named_MultipleKeys_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		Assert.True(args.TryGetEnum(["color", "c"], true, out ConsoleColor color));
		Assert.Equal(ConsoleColor.Blue, color);
		args = Parser.ParseArguments("command -c bLue")!;
		Assert.True(args.TryGetEnum(["color", "c"], true, out ConsoleColor c));
		Assert.Equal(ConsoleColor.Blue, c);
	}

	[Fact]
	public void Arguments_GetEnum_Positional() {
		var args = Parser.ParseArguments("command Blue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(1, ConsoleColor.Black));
	}

	[Fact]
	public void Arguments_GetEnum_Positional_IgnoreCase() {
		var args = Parser.ParseArguments("command bLue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(1, ConsoleColor.Black, true));
	}

	[Fact]
	public void Arguments_GetEnum_Named() {
		var args = Parser.ParseArguments("command --color Blue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum("color", ConsoleColor.Black));
	}

	[Fact]
	public void Arguments_GetEnum_Named_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum("color", ConsoleColor.Black, true));
	}

	[Fact]
	public void Arguments_GetEnum_Named_MultipleKeys() {
		var args = Parser.ParseArguments("command --color Blue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(["color", "c"], ConsoleColor.Black));
		args = Parser.ParseArguments("command -c Blue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(["color", "c"], ConsoleColor.Black));
	}

	[Fact]
	public void Arguments_GetEnum_Named_MultipleKeys_IgnoreCase() {
		var args = Parser.ParseArguments("command --color bLue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(["color", "c"], ConsoleColor.Black, true));
		args = Parser.ParseArguments("command -c bLue")!;
		Assert.Equal(ConsoleColor.Blue, args.GetEnum(["color", "c"], ConsoleColor.Black, true));
	}
}