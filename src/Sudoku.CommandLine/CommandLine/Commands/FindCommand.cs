namespace Sudoku.CommandLine.Commands;

/// <summary>
/// Provides with find command.
/// </summary>
internal sealed class FindCommand : CommandBase
{
	/// <summary>
	/// Initializes a <see cref="FindCommand"/> instance.
	/// </summary>
	public FindCommand() : base("find", "Finds for the specified step of a puzzle")
	{
		OptionsCore = [new TechniqueOption(true), new PrintAllOption()];
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
		if (this is not ([TechniqueOption o1, PrintAllOption o2], [GridArgument a1]))
		{
			return;
		}

		var result = context.ParseResult;
		var technique = result.GetValueForOption(o1);
		var printAll = result.GetValueForOption(o2);
		var grid = result.GetValueForArgument(a1);
		CommonPreprocessors.PrintInvalidIfWorth(grid, new BitwiseSolver(), out var solution);
		if (solution.IsUndefined)
		{
			return;
		}

		var analyzer = new Analyzer();
		var r = analyzer.Analyze(grid);
		if (r is not { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps })
		{
			var reason = r.FailedReason;
			Console.WriteLine($"\e[31mThe grid cannot be solved. Reason flag: {reason}\e[0m");
			return;
		}

		foreach (ref readonly var pair in Step.Combine(grids, steps))
		{
			ref readonly var currentGrid = ref pair.KeyRef;
			var currentStep = pair.Value;
			if (currentStep.Code != technique)
			{
				continue;
			}

			Console.WriteLine(currentGrid.ToString("#"));
			Console.WriteLine(currentStep.ToString());
			if (!printAll)
			{
				return;
			}

			Console.WriteLine();
		}
	}
}
