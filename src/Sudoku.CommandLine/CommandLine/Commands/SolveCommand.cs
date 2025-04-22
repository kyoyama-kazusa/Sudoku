namespace Sudoku.CommandLine.Commands;

/// <summary>
/// Represents a solve command.
/// </summary>
internal sealed class SolveCommand : CommandBase
{
	/// <summary>
	/// Initializes a <see cref="SolveCommand"/> instance.
	/// </summary>
	public SolveCommand() : base("solve", "To solve a puzzle")
	{
		OptionsCore = [new SolvingMethodOption()];
		this.AddRange(OptionsCore);

		ArgumentsCore = [new GridArgument()];
		this.AddRange(ArgumentsCore);

		this.SetHandler(HandleCore);
	}


	/// <inheritdoc/>
	public override SymbolList<Option> OptionsCore { get; }

	/// <inheritdoc/>
	public override SymbolList<Argument> ArgumentsCore { get; }


	/// <inheritdoc/>
	protected override void HandleCore(InvocationContext context)
	{
		if (this is not ([SolvingMethodOption o1], [GridArgument a1]))
		{
			return;
		}

		var result = context.ParseResult;
		var type = result.GetValueForOption(o1);
		var grid = result.GetValueForArgument(a1);
		CommonPreprocessors.PrintInvalidIfWorth(grid, SolverType.CreateSolver(type), out var solution);
		if (!solution.IsUndefined)
		{
			Console.WriteLine(solution);
		}
	}
}
