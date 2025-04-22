namespace Sudoku.Behaviors.Ittoryu;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/>.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridIttoryuExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Try to reproduce ittoryu ordering for the specified grid whose path can be found by <see cref="DisorderedIttoryuFinder"/>.
		/// </summary>
		/// <param name="ittoryuPath">
		/// The path to be used. The argument isn't required to be a complete path. It's considered to be OK if a path contains at least 2 digits.
		/// </param>
		/// <exception cref="ArgumentException">Throws when the ittoryu path contains a digit series of length 0 or 1.</exception>
		/// <seealso cref="DisorderedIttoryuFinder"/>
		public void MakeIttoryu(DisorderedIttoryuDigitPath ittoryuPath)
		{
			if (ittoryuPath.Digits is not { Length: >= 2 })
			{
				throw new ArgumentException($"The argument '{nameof(ittoryuPath)}' requires a digit series of length greater than 1.", nameof(ittoryuPath));
			}

			if (ittoryuPath == Digits)
			{
				// The puzzle won't be changed.
				return;
			}

			// Try to replace digits.
			var result = Grid.Empty;
			var valuesMap = @this.ValuesMap;
			for (var digit = 0; digit < ittoryuPath.Digits.Length; digit++)
			{
				ref readonly var valueMap = ref valuesMap[ittoryuPath.Digits[digit]];
				foreach (var cell in valueMap)
				{
					result.SetDigit(cell, digit);
				}
			}

			// Fix the grid for the initial state.
			for (var cell = 0; cell < 81; cell++)
			{
				if (@this.GetState(cell) == CellState.Given)
				{
					result.SetState(cell, CellState.Given);
				}
			}

			@this = result.ResetGrid;
		}
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Determine whether the specified grid is an ittoryu puzzle, meaning the puzzle can be finished via digits one by one.
		/// </summary>
		/// <param name="path">The first found ittoryu path.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsIttoryu([NotNullWhen(true)] out DisorderedIttoryuDigitPath? path)
			=> @this.IsIttoryu(TechniqueIttoryuSets.IttoryuTechniques, out path);

		/// <summary>
		/// Determine whether the specified grid is an ittoryu puzzle, meaning the puzzle can be finished via digits one by one,
		/// with checking using the specified techniques.
		/// </summary>
		/// <param name="techniques">
		/// <para>The techniques supported.</para>
		/// <para>
		/// The value is a <see cref="TechniqueSet"/> instance. You can only chose one or more fields in the following list:
		/// <list type="bullet">
		/// <item><see cref="Technique.FullHouse"/> (Full House)</item>
		/// <item><see cref="Technique.HiddenSingleBlock"/> (Hidden Single in Block)</item>
		/// <item><see cref="Technique.HiddenSingleRow"/> (Hidden Single in Row)</item>
		/// <item><see cref="Technique.HiddenSingleColumn"/> (Hidden Single in Column)</item>
		/// <item><see cref="Technique.NakedSingle"/> (Naked Single)</item>
		/// </list>
		/// </para>
		/// <para>For example, <c>[Technique.FullHouse, Technique.NakedSingle]</c> is valid argument, but <c>[Technique.NakedPair]</c> is not.</para>
		/// </param>
		/// <param name="path">The first found ittoryu path.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsIttoryu(TechniqueSet techniques, [NotNullWhen(true)] out DisorderedIttoryuDigitPath? path)
		{
			var pathFinder = new DisorderedIttoryuFinder(techniques);
			var foundPath = pathFinder.FindPath(in @this);
			if (foundPath.IsComplete)
			{
				path = foundPath;
				return true;
			}
			else
			{
				path = null;
				return false;
			}
		}
	}
}
