namespace Sudoku.Analytics.Steps.Fishes;

/// <summary>
/// Provides with a step that is a <b>Fish</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="baseSetsMask"><inheritdoc cref="BaseSetsMask" path="/summary"/></param>
/// <param name="coverSetsMask"><inheritdoc cref="CoverSetsMask" path="/summary"/></param>
/// <param name="fins"><inheritdoc cref="Fins" path="/summary"/></param>
/// <param name="isSashimi"><inheritdoc cref="IsSashimi" path="/summary"/></param>
/// <param name="isSiamese"><inheritdoc cref="IsSiamese" path="/summary"/></param>
public abstract class FishStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	HouseMask baseSetsMask,
	HouseMask coverSetsMask,
	in CellMap fins,
	bool? isSashimi,
	bool isSiamese = false
) : FullPencilmarkingStep(conclusions, views, options), ISizeTrait
{
	/// <summary>
	/// Indicates whether the pattern is a Siamese Fish.
	/// </summary>
	public bool IsSiamese { get; } = isSiamese;

	/// <summary>
	/// <para>Indicates whether the fish is a Sashimi fish.</para>
	/// <para>
	/// All cases are as below:
	/// <list type="table">
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>The fish is a sashimi finned fish.</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>The fish is a normal finned fish.</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>The fish doesn't contain any fin.</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	public bool? IsSashimi { get; } = isSashimi;

	/// <inheritdoc/>
	/// <remarks>
	/// The name of the corresponding names are:
	/// <list type="table">
	/// <item><term>2</term><description>X-Wing</description></item>
	/// <item><term>3</term><description>Swordfish</description></item>
	/// <item><term>4</term><description>Jellyfish</description></item>
	/// <item><term>5</term><description>Squirmbag (or Starfish)</description></item>
	/// <item><term>6</term><description>Whale</description></item>
	/// <item><term>7</term><description>Leviathan</description></item>
	/// </list>
	/// Other fishes of sizes not appearing in above don't have well-known names.
	/// </remarks>
	public int Size => BitOperations.PopCount(BaseSetsMask);

	/// <inheritdoc/>
	public sealed override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the mask that describes the base sets.
	/// </summary>
	public HouseMask BaseSetsMask { get; } = baseSetsMask;

	/// <summary>
	/// Indicates the mask that describes the cover sets.
	/// </summary>
	public HouseMask CoverSetsMask { get; } = coverSetsMask;

	/// <summary>
	/// Indicates all the fins. For complex fishes, this property also includes endo-fins.
	/// </summary>
	public CellMap Fins { get; } = fins;

	/// <summary>
	/// Creates a <see cref="FishPattern"/> instance via the current data.
	/// </summary>
	internal FishPattern Pattern
		=> new(
			Digit,
			BaseSetsMask,
			CoverSetsMask,
			in this is NormalFishStep { Fins: var f }
				? ref f
				: ref this is ComplexFishStep { Exofins: var f2 } ? ref f2 : ref CellMap.Empty,
			in this is NormalFishStep
				? ref CellMap.Empty
				: ref this is ComplexFishStep { Endofins: var f3 } ? ref f3 : ref CellMap.Empty
		);

	/// <summary>
	/// The internal notation.
	/// </summary>
	private protected string InternalNotation => Pattern.ToString(Options.Converter);


	/// <inheritdoc cref="Step.ToString(IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new string ToString(IFormatProvider? formatProvider) => Pattern.ToString(formatProvider);
}
