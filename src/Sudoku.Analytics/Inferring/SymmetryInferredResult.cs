namespace Sudoku.Inferring;

/// <summary>
/// Indicates the result value after <see cref="SymmetryInferrer.TryInfer(in Grid, out SymmetryInferredResult)"/> called.
/// </summary>
/// <param name="symmetricType"><inheritdoc cref="SymmetricType" path="/summary"/></param>
/// <param name="mappingDigits"><inheritdoc cref="MappingDigits" path="/summary"/></param>
/// <param name="selfPairedDigitsMask"><inheritdoc cref="SelfPairedDigitsMask" path="/summary"/></param>
/// <seealso cref="SymmetryInferrer.TryInfer(in Grid, out SymmetryInferredResult)"/>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct SymmetryInferredResult(SymmetricType symmetricType, ReadOnlySpan<Digit?> mappingDigits, Mask selfPairedDigitsMask)
{
	/// <summary>
	/// The symmetric type returned.
	/// </summary>
	public SymmetricType SymmetricType { get; } = symmetricType;

	/// <summary>
	/// The mapping digits returned.
	/// </summary>
	public ReadOnlySpan<Digit?> MappingDigits { get; } = mappingDigits;

	/// <summary>
	/// A mask that contains a list of digits self-paired.
	/// </summary>
	public Mask SelfPairedDigitsMask { get; } = selfPairedDigitsMask;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out SymmetricType symmetricType, out ReadOnlySpan<Digit?> mappingDigits, out Mask selfPairedDigitsMask)
	{
		symmetricType = SymmetricType;
		mappingDigits = MappingDigits;
		selfPairedDigitsMask = SelfPairedDigitsMask;
	}
}
