namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods on <see cref="LinkType"/>.
/// </summary>
/// <seealso cref="LinkType"/>
public static class LinkTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="LinkType"/>.
	/// </summary>
	extension(LinkType @this)
	{
		/// <summary>
		/// Indicates a <see cref="ChainingRule"/> instance from the specified link type.
		/// </summary>
		/// <returns>The target <see cref="ChainingRule"/> instance.</returns>
		public ChainingRule? RuleInstance
		{
			get
			{
				var types = LinkType.FieldInfoOf(@this)?.GetGenericAttributeTypeArguments(typeof(ChainingRuleAttribute<>));
				return types is [var type] ? (ChainingRule?)Activator.CreateInstance(type) : null;
			}
		}
	}
}
