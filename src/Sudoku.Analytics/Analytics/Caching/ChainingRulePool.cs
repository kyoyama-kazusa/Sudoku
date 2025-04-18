namespace Sudoku.Analytics.Caching;

/// <summary>
/// Provides with an easy way to create <see cref="ChainingRule"/> instances.
/// </summary>
/// <seealso cref="ChainingRule"/>
internal static class ChainingRulePool
{
	/// <summary>
	/// Indicates the cached rules.
	/// </summary>
	private static readonly Dictionary<LinkType, ChainingRule> CachedRules = [];


	/// <summary>
	/// Clears the cached rules.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FlushCachedRules() => CachedRules.Clear();

	/// <summary>
	/// Creates a <see cref="ChainingRule"/> instance via the specified link type.
	/// </summary>
	/// <param name="linkType">The link type.</param>
	/// <returns>The created <see cref="ChainingRule"/> instance created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ChainingRule? TryCreate(LinkType linkType)
	{
		if (CachedRules.TryGetValue(linkType, out var rule))
		{
			return rule;
		}

		if (linkType.RuleInstance is { } createdRule)
		{
			CachedRules.Remove(linkType);
			CachedRules.Add(linkType, createdRule);
			return createdRule;
		}
		return null;
	}
}
