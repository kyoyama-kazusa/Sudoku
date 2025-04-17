namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Represents the metadata implementation details for a <see cref="StepSearcher"/>.
/// </summary>
/// <param name="stepSearcher">The step searcher instance.</param>
/// <param name="backAttribute">The bound step searcher attribute.</param>
/// <seealso cref="StepSearcher"/>
public sealed partial class StepSearcherMetadataInfo([Field] StepSearcher stepSearcher, [Field] StepSearcherAttribute backAttribute)
{
	/// <inheritdoc cref="StepSearcherAttribute.IsCachingSafe"/>
	public bool IsCachingSafe => _backAttribute.IsCachingSafe;

	/// <inheritdoc cref="StepSearcherAttribute.IsOrderingFixed"/>
	public bool IsOrderingFixed => _backAttribute.IsOrderingFixed;

	/// <inheritdoc cref="StepSearcherAttribute.IsAvailabilityReadOnly"/>
	public bool IsReadOnly => _backAttribute.IsAvailabilityReadOnly;

	/// <summary>
	/// Determines whether the current step searcher supports sukaku solving.
	/// </summary>
	public bool SupportsSukaku => _backAttribute.SupportedSudokuTypes.HasFlag(SudokuType.Sukaku);

	/// <inheritdoc cref="StepSearcherAttribute.SupportAnalyzingMultipleSolutionsPuzzle"/>
	public bool SupportAnalyzingMultipleSolutionsPuzzle => _backAttribute.SupportAnalyzingMultipleSolutionsPuzzle;

	/// <summary>
	/// Determines whether the current step searcher is disabled
	/// by option <see cref="StepSearcherRuntimeFlags.TimeComplexity"/> being configured.
	/// </summary>
	/// <seealso cref="StepSearcherRuntimeFlags.TimeComplexity"/>
	public bool IsConfiguredSlow => _backAttribute.RuntimeFlags is { } cases && cases.HasFlag(StepSearcherRuntimeFlags.TimeComplexity);

	/// <summary>
	/// Determines whether the current step searcher is disabled
	/// by option <see cref="StepSearcherRuntimeFlags.SpaceComplexity"/> being configured.
	/// </summary>
	/// <seealso cref="StepSearcherRuntimeFlags.SpaceComplexity"/>
	public bool IsConfiguredHighAllocation
		=> _backAttribute.RuntimeFlags is { } cases && cases.HasFlag(StepSearcherRuntimeFlags.SpaceComplexity);

	/// <summary>
	/// Determines whether the current step searcher is only run for direct view.
	/// </summary>
	public bool IsOnlyRunForDirectViews
		=> _backAttribute.RuntimeFlags is { } cases && cases.HasFlag(StepSearcherRuntimeFlags.DirectTechniquesOnly);

	/// <summary>
	/// Indicates whether the searcher skips verification for conclusions.
	/// </summary>
	public bool SkipVerificationOnConclusions
		=> _backAttribute.RuntimeFlags is { } cases && cases.HasFlag(StepSearcherRuntimeFlags.SkipVerification);

	/// <summary>
	/// Determines whether the current step searcher is only run for indirect view.
	/// </summary>
	public bool IsOnlyRunForIndirectViews
		=> _backAttribute.RuntimeFlags is { } cases && cases.HasFlag(StepSearcherRuntimeFlags.IndirectTechniquesOnly);

	/// <summary>
	/// Indicates the <see cref="DifficultyLevel"/>s whose corresponding step can be produced by the current step searcher instance.
	/// </summary>
	public ReadOnlySpan<DifficultyLevel> DifficultyLevelRange => _backAttribute.DifficultyLevels.AllFlags;

	/// <inheritdoc cref="StepSearcherAttribute.SupportedTechniques"/>
	public TechniqueSet SupportedTechniques => _backAttribute.SupportedTechniques;


	/// <summary>
	/// Gets the name of the step searcher, using the specified culture.
	/// </summary>
	/// <param name="formatProvider">The culture information.</param>
	/// <returns>The name.</returns>
	public string GetName(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return _stepSearcher.GetType() switch
		{
			{ Name: var typeName } type => type.GetCustomAttribute<StepSearcherAttribute>() switch
			{
				{ NameKey: { } r } => SR.Get(r, culture),
				_ => SR.TryGet($"StepSearcherName_{typeName}", out var resource, culture ?? CultureInfo.CurrentUICulture)
					? resource
					: typeName
			}
		};
	}
}
