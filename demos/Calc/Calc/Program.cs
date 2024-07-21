using Calc.Commands;

namespace Calc;

public class Program {
    private static ReadOnlySpan<Command> Commands => new Command[] {
        new AddCommand(),
        new SubtractCommand(),
        new MultiplyCommand(),
        new DivideCommand()
    };

    static async Task<int> Main(string[] args) {
        var runner = CliRunner.CreateBuilder()
        .AddCommands(Commands)
        .SortCommandsAlphabetically()
        .UseConsoleAsOutputWriter()
        .WithMetadata(metadata => {
            metadata.Name = "Calc";
            metadata.Description = "A simple calculator";
            metadata.Version = "1.0.0";
            metadata.Author = "Dave";
            metadata.License = "MIT";
        })
        .Build();

        return await runner.RunAsync(args);
    }
}