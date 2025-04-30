namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents named chain patterns:
/// <list type="bullet">
/// <item>Alternating inference chain (corresponds to type <see cref="AlternatingInferenceChain"/>)</item>
/// <item>Continuous nice loop (corresponds to type <see cref="ContinuousNiceLoop"/>)</item>
/// </list>
/// </summary>
/// <param name="lastNode"><inheritdoc cref="Chain(Node, bool, bool, bool)" path="/param[@name='lastNode']"/></param>
/// <param name="isLoop"><inheritdoc cref="Chain(Node, bool, bool, bool)" path="/param[@name='isLoop']"/></param>
/// <seealso cref="AlternatingInferenceChain"/>
/// <seealso cref="ContinuousNiceLoop"/>
public abstract class NamedChain(Node lastNode, bool isLoop) : Chain(lastNode, isLoop, true)
{
	/// <inheritdoc/>
	public sealed override bool IsNamed => true;

	/// <summary>
	/// Indicates whether the chain is ALS-XZ, ALS-XY-Wing or ALS-XY-Chain.
	/// </summary>
	public bool IsAlmostLockedSetSequence
	{
		get
		{
			// A valid ALS chain-like pattern must satisfy 3 conditions:
			//   1) All strong links use ALS
			//   2) All weak links use RCC (a weak link to connect two same digit)
			//   3) All strong links are not bi-value cells (otherwise, XY-Chain)
			var isAllBivalueCells = true;
			foreach (var link in Links)
			{
				switch (link)
				{
					case (_, _, true) { IsBivalueCellLink: false }:
					{
						isAllBivalueCells = false;
						continue;
					}
					case (_, _, true) { IsBivalueCellLink: true }:
					{
						continue;
					}
					case (_, _, true, not AlmostLockedSetPattern):
					case (_, _, false, not null):
					case ({ Map.Digits: var d1 }, { Map.Digits: var d2 }, false) when d1 != d2 || !BitOperations.IsPow2(d1):
					{
						return false;
					}
				}
			}
			return !isAllBivalueCells;
		}
	}

	/// <summary>
	/// Indicates the number of grouped pattern used in chain.
	/// </summary>
	public FrozenDictionary<PatternType, int> GroupedPatternsCount
	{
		get
		{
			var result = new Dictionary<PatternType, int>();
			foreach (var link in Links)
			{
				if (link.GroupedLinkPattern is { } pattern && !result.TryAdd(pattern.Type, 1))
				{
					result[pattern.Type]++;
				}

				// Special case: If the link only uses a bi-value cell, and indeed is a strong link,
				// we should treat it as a special ALS.
				if (link is { IsStrong: true, IsBivalueCellLink: true } && !result.TryAdd(PatternType.AlmostLockedSet, 1))
				{
					result[PatternType.AlmostLockedSet]++;
				}
			}
			return result.ToFrozenDictionary();
		}
	}

	/// <summary>
	/// Indicates the number of ALSes used in chain.
	/// </summary>
	internal int AlmostLockedSetsCount => GroupedPatternsCount.TryGetValue(PatternType.AlmostLockedSet, out var r) ? r : 0;


	/// <summary>
	/// Try to get a <see cref="ConclusionSet"/> instance that contains all conclusions created by using the current chain.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <returns>A <see cref="ConclusionSet"/> instance. By default the method returns an empty conclusion set.</returns>
	public virtual ConclusionSet GetConclusions(in Grid grid) => ConclusionSet.Empty;
}
