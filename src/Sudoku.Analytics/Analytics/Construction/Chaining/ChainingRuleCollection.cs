namespace Sudoku.Analytics.Construction.Chaining;

/// <summary>
/// Represents a list of <see cref="ChainingRule"/> instances.
/// </summary>
/// <param name="rules"><inheritdoc cref="Rules" path="/summary"/></param>
[CollectionBuilder(typeof(ChainingRuleCollection), nameof(Create))]
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public readonly ref partial struct ChainingRuleCollection(ReadOnlySpan<ChainingRule> rules) :
	IEnumerable<ChainingRule>,
	IToArrayMethod<ChainingRuleCollection, ChainingRule>
{
	/// <summary>
	/// Indicates the length of rules.
	/// </summary>
	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Rules.Length;
	}

	/// <summary>
	/// Indicates the rules.
	/// </summary>
	public ReadOnlySpan<ChainingRule> Rules { get; } = rules;


	/// <summary>
	/// Gets a <see cref="ChainingRule"/> instance at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>A <see cref="ChainingRule"/> instance returned.</returns>
	public ChainingRule this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Rules[index];
	}


	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnonymousSpanEnumerator<ChainingRule> GetEnumerator() => new(Rules);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ChainingRule[] ToArray() => Rules.ToArray();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Rules.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ChainingRule> IEnumerable<ChainingRule>.GetEnumerator() => Rules.ToArray().AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates a <see cref="ChainingRuleCollection"/> instance via a list of <see cref="ChainingRule"/> instances.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="ChainingRuleCollection"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChainingRuleCollection Create(ReadOnlySpan<ChainingRule> value) => new(value);


	/// <summary>
	/// Initializes a <see cref="ChainingRuleCollection"/> instance.
	/// </summary>
	/// <param name="rules">The rules.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator ChainingRuleCollection(ReadOnlySpan<ChainingRule> rules) => new(rules);
}
