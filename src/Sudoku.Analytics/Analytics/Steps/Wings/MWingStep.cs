namespace Sudoku.Analytics.Steps.Wings;

/// <summary>
/// Provides with a step that is a <b>(Grouped) M-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="node1"><inheritdoc cref="Node1" path="/summary"/></param>
/// <param name="node2"><inheritdoc cref="Node2" path="/summary"/></param>
/// <param name="strongXyCell"><inheritdoc cref="StrongXyCell" path="/summary"/></param>
/// <param name="weakXyCell"><inheritdoc cref="WeakXyCell" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class MWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap node1,
	in CellMap node2,
	Cell strongXyCell,
	Cell weakXyCell,
	Mask digitsMask
) : IrregularWingStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override bool IsSymmetricPattern => false;

	/// <inheritdoc/>
	public override bool IsGrouped => Node1.Count != 1 || Node2.Count != 1;

	/// <inheritdoc/>
	public override int BaseDifficulty => 45;

	/// <inheritdoc/>
	public override Technique Code => IsGrouped ? Technique.GroupedMWing : Technique.MWing;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [Node1Str, Node2Str, Cell1Str, Cell2Str]),
			new(SR.ChineseLanguage, [Node1Str, Node2Str, Cell1Str, Cell2Str])
		];

	/// <summary>
	/// Indicates the node 1.
	/// </summary>
	public CellMap Node1 { get; } = node1;

	/// <summary>
	/// Indicates the node 2.
	/// </summary>
	public CellMap Node2 { get; } = node2;

	/// <summary>
	/// Indicates the strong XY cell.
	/// </summary>
	public Cell StrongXyCell { get; } = strongXyCell;

	/// <summary>
	/// Indicates the weak XY cell.
	/// </summary>
	public Cell WeakXyCell { get; } = weakXyCell;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	private string Node1Str => Options.Converter.CellConverter(Node1);

	private string Node2Str => Options.Converter.CellConverter(Node2);

	private string Cell1Str => Options.Converter.CellConverter(in WeakXyCell.AsCellMap());

	private string Cell2Str => Options.Converter.CellConverter(in StrongXyCell.AsCellMap());


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is MWingStep comparer && Node1 == comparer.Node1 && Node2 == comparer.Node2
		&& StrongXyCell == comparer.StrongXyCell && WeakXyCell == comparer.WeakXyCell;
}
