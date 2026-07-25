namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator type that can generate puzzles with the specified technique used.
/// </summary>
public closed class TechniqueGenerator : ITechniqueGenerator
{
	/// <summary>
	/// Indicates the random number generator.
	/// </summary>
	protected static readonly Random Rng = Random.Shared;


	/// <inheritdoc/>
	public abstract TechniqueSet SupportedTechniques { get; }


	/// <inheritdoc/>
	static ref readonly Random ITechniqueGenerator.RandomNumberGenerator => ref Rng;


	/// <inheritdoc/>
	public abstract bool TryGenerateUnique(out Grid result, CancellationToken cancellationToken = default);
}
