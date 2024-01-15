using System.Text;

namespace Sharpify.CommandLineInterface.Tests;

public sealed class TestTextWriter : TextWriter {
	public readonly StringBuilder Builder = new();

	public override Encoding Encoding => Encoding.UTF8;

	public override void Write(char value) {
		Builder.Append(value);
	}

	public override void Write(string? value) {
		Builder.Append(value);
	}

	public override void WriteLine(string? value) {
		Builder.AppendLine(value);
	}

    public override string ToString() {
        return Builder.ToString();
    }
}