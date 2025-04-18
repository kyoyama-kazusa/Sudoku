namespace Sudoku.CommandLine;

/// <summary>
/// Provides a list of commonly-used preprocessor methods.
/// </summary>
internal static class CommonPreprocessors
{
	/// <summary>
	/// Print invalid information if the puzzle is invalid;
	/// use <paramref name="solution"/> to determine whether the grid is valid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="solver">The solver.</param>
	/// <param name="solution">The solution.</param>
	public static void PrintInvalidIfWorth(in Grid grid, ISolver solver, out Grid solution)
	{
		var result = solver.Solve(grid, out solution);
		if (result is true)
		{
			return;
		}

		var text = result is false ? "The puzzle has multiple solutions." : "The puzzle has no valid solutions.";
		Console.WriteLine($"\e[31m{text}\e[0m");
		solution = Grid.Undefined;
	}

	/// <summary>
	/// Try to generate puzzles, and write them to the specified text writer (file or console).
	/// </summary>
	/// <typeparam name="TGenerator">The type of generator.</typeparam>
	/// <param name="generator">The generator object.</param>
	/// <param name="generatorMethod">The method to generate a <see cref="Grid"/> instance.</param>
	/// <param name="outputFilePath">
	/// The output file path. The value can be <see langword="null"/> if you don't want to write it to the specified file;
	/// in such case, the puzzle will be output onto console screen.
	/// </param>
	/// <param name="timeout">The timeout. The value can be -1 if you want to set infinity timeout.</param>
	/// <param name="count">
	/// The number of puzzles to be generated. The value can be -1 if you want to make an infinity loop.
	/// </param>
	/// <param name="filteredTechnique">
	/// The technique that the generated puzzle must use.
	/// The value can be <see cref="Technique.None"/> if you don't want to specify any techniques.
	/// </param>
	/// <param name="alsoOutputInfo">
	/// <para>Indicates whether the output text also contains filter information.</para>
	/// <para>
	/// For example, if <paramref name="filteredTechnique"/> is specified with a value not <see cref="Technique.None"/>,
	/// the output information will display the technique used for the target grid generated.
	/// </para>
	/// </param>
	/// <param name="outputTargetGridRatherThanOriginalGrid">
	/// Indicates whether the output text will replace original grid with the target grid that satisfies the filtered conditions.
	/// </param>
	/// <param name="separator">The separator that splits the output grid and detailed information (if available).</param>
	public static void GeneratePuzzles<TGenerator>(
		TGenerator generator,
		Func<TGenerator, CancellationToken, Grid> generatorMethod,
		string? outputFilePath,
		int timeout,
		int count,
		Technique filteredTechnique,
		bool alsoOutputInfo,
		bool outputTargetGridRatherThanOriginalGrid,
		string separator
	)
		where TGenerator : IGenerator<Grid>, allows ref struct
	{
		var analyzer = filteredTechnique == Technique.None ? null : new Analyzer();
		using var outputFileStream = outputFilePath is null ? null : new StreamWriter(outputFilePath);
		using var cts = CreateCancellationTokenSource(timeout);
		for (var i = 0; count == -1 || i < count;)
		{
			var tempGridGenerated = generatorMethod(generator, cts.Token);
			if (tempGridGenerated.IsUndefined)
			{
				// Cancelled.
				return;
			}

			var matchedKvp = default(KeyValuePair<Grid, Step>?);
			if (filteredTechnique != Technique.None)
			{
				if (analyzer!.Analyze(tempGridGenerated) is
					{
						IsSolved: true,
						StepsSpan: var steps,
						GridsSpan: var grids
					} tempAnalysisResult)
				{
					foreach (var kvp in Step.Combine(grids, steps))
					{
						if (kvp.Value.Code == filteredTechnique)
						{
							matchedKvp = kvp;
							break;
						}
					}
					if (matchedKvp is not null)
					{
						goto Output;
					}
				}
				continue;
			}

		Output:
			OutputTextTo(
				tempGridGenerated,
				outputFileStream ?? Console.Out,
				(in grid) => (alsoOutputInfo, outputTargetGridRatherThanOriginalGrid, matchedKvp) switch
				{
					(true, true, var (targetGrid, step)) => $"{targetGrid:#}{separator}{step.GetName(null)}",
					(true, _, (_, var step)) => $"{grid:.}{separator}{step.GetName(null)}",
					_ => $"{grid:.}"
				},
				true
			);
			i++;
		}
	}

	/// <summary>
	/// Output the object of type <typeparamref name="T"/> to a certain text writer.
	/// </summary>
	/// <typeparam name="T">The type of object.</typeparam>
	/// <param name="obj">The object.</param>
	/// <param name="writer">The stream.</param>
	/// <param name="outputTextCreator">
	/// The method that converts the object <paramref name="obj"/> to <see cref="string"/> representation.
	/// </param>
	/// <param name="appendNewLine">Indicates whether the new line characters will be appended after the output text.</param>
	public static void OutputTextTo<T>(in T obj, TextWriter writer, ObjectOutputHandler<T> outputTextCreator, bool appendNewLine)
		where T : allows ref struct
	{
		writer.Write(outputTextCreator(obj));
		if (appendNewLine)
		{
			writer.WriteLine();
		}
	}

	/// <summary>
	/// Creates a <see cref="CancellationTokenSource"/> instance with a timeout.
	/// Set <see cref="Timeout.Infinite"/> (i.e. -1) to set infinity timeout.
	/// </summary>
	/// <param name="timeout">The timeout in milliseconds.</param>
	/// <returns>A <see cref="CancellationTokenSource"/> instance.</returns>
	/// <seealso cref="Timeout.Infinite"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CancellationTokenSource CreateCancellationTokenSource(int timeout)
		=> timeout == Timeout.Infinite ? new() : new(timeout);
}
