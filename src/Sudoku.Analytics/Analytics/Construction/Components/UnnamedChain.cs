namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a forcing chain branch.
/// </summary>
/// <param name="lastNode"><inheritdoc cref="Chain(Node, bool, bool, bool)" path="/param[@name='lastNode']"/></param>
/// <param name="isDynamicChaining">
/// <inheritdoc cref="Chain(Node, bool, bool, bool)" path="/param[@name='isDynamicChaining']"/>
/// </param>
public closed class UnnamedChain(Node lastNode, bool isDynamicChaining = false) : Chain(lastNode, false, false, isDynamicChaining)
{
	/// <inheritdoc/>
	public sealed override bool IsNamed => false;

	/// <inheritdoc/>
	protected internal sealed override ReadOnlySpan<Node> ValidNodes => _nodes;

	/// <inheritdoc/>
	protected sealed override int LoopIdentity => 1;
}
