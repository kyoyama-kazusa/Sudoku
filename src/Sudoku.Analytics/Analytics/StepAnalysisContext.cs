namespace Sudoku.Analytics;

/// <summary>
/// Defines a context that will be used for finding a single <see cref="Step"/> from a <see cref="Concepts.Grid"/> state.
/// </summary>
/// <param name="grid">Indicates the puzzle to be solved and analyzed.</param>
/// <param name="initialGrid">Indicates the initial grid.</param>
/// <seealso cref="Step"/>
/// <seealso cref="Concepts.Grid"/>
[StructLayout(LayoutKind.Auto)]
[TypeImpl(TypeImplFlags.AllObjectMethods, IsLargeStructure = true)]
public ref partial struct StepAnalysisContext(
	[Field(Accessibility = "public", NamingRule = NamingRules.Property)] in Grid grid,
	[Field(Accessibility = "public", NamingRule = NamingRules.Property)] ref readonly Grid initialGrid
)
{
	/// <summary>
	/// Initializes an <see cref="StepAnalysisContext"/> instance via the specified grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	public StepAnalysisContext(in Grid grid) : this(grid, in Grid.nullref)
	{
	}


	/// <summary>
	/// Indicates whether the solver only find one possible step and exit the searcher.
	/// </summary>
	/// <remarks>
	/// If this property returns <see langword="true"/>, property <see cref="Accumulator"/>
	/// will become useless because we only finding one step is okay,
	/// so we may not use the accumulator to store all possible steps, in order to optimize the performance.
	/// Therefore, property <see cref="Accumulator"/> can be <see langword="null"/> in this case.
	/// </remarks>
	/// <seealso cref="Accumulator"/>
	[MemberNotNullWhen(false, nameof(Accumulator))]
	public required bool OnlyFindOne { get; init; }

	/// <summary>
	/// Indicates whether a puzzle satisfies a Gurth's Symmetrical Placement (GSP) pattern.
	/// If satisfying, what kind of symmetry the pattern will be.
	/// </summary>
	/// <remarks>
	/// This value will only be set in <see cref="Analyzer"/> with a not-<see langword="null"/> value.
	/// </remarks>
	/// <seealso cref="Analyzer"/>
	[DisallowNull]
	public SymmetricType? GspPatternInferred { get; internal set; }

	/// <summary>
	/// Indicates the previously set digit.
	/// </summary>
	/// <remarks>
	/// This value will only be set in <see cref="SingleStepSearcher"/>.
	/// </remarks>
	/// <seealso cref="SingleStepSearcher"/>
	public Digit PreviousSetDigit { get; internal set; }

	/// <summary>
	/// Indicates the mapping relations. The index of the array means the base digit,
	/// and the target value at the specified index means the other mapping digit. If a value does not contain a mapping digit,
	/// the indexed value (target value) will be an empty array.
	/// </summary>
	/// <remarks><b>
	/// <para>
	/// By default, the value is an empty array or trash memory value (stack-only structures can be used as locals, uninitialized).
	/// Due to being uninitialized, we should use this property very carefully.
	/// </para>
	/// <para>
	/// Please firstly check for property <see cref="GspPatternInferred"/>.
	/// If that property returns an array not empty, you can use this property with safety.
	/// </para>
	/// </b></remarks>
	/// <seealso cref="GspPatternInferred"/>
	public ReadOnlySpan<Digit?> MappingRelations { get; internal set; }

	/// <summary>
	/// Indicates the cancellation token that can cancel the current operation.
	/// </summary>
	public CancellationToken CancellationToken { get; init; }

	/// <summary>
	/// Indicates the pre-defined options set by user in type <see cref="Analyzer"/>.
	/// The value can be <see langword="null"/> if the target step searcher doesn't use this property.
	/// </summary>
	public required StepGathererOptions Options { get; init; }

	/// <summary>
	/// Indicates the accumulator to store each step while searching.
	/// </summary>
	public List<Step>? Accumulator { get; internal set; }
}
