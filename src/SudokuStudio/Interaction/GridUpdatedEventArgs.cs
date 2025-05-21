namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="GridUpdatedEventHandler"/>.
/// </summary>
/// <param name="behavior"><inheritdoc cref="Behavior" path="/summary"/></param>
/// <param name="newValue"><inheritdoc cref="NewValue" path="/summary"/></param>
/// <seealso cref="GridUpdatedEventHandler"/>
public sealed class GridUpdatedEventArgs(GridUpdatedBehavior behavior, object newValue) : EventArgs
{
	/// <summary>
	/// <para>Indicates the new value to be replaced.</para>
	/// <para>
	/// The type of the value is <see cref="object"/> because the internal type will be varied
	/// from different value of property <see cref="Behavior"/>. The target type is:
	/// <list type="table">
	/// <listheader>
	/// <term>Value of property <see cref="Behavior"/></term>
	/// <description>Target type of this property</description>
	/// </listheader>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Elimination"/></term>
	/// <description>An <see cref="Candidate"/> value indicating the candidate eliminated.</description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.EliminationMultiple"/></term>
	/// <description>
	/// An <see cref="short"/> value indicating the candidates eliminated,
	/// by <c><![CDATA[>> 9]]></c> to get the target cell, and <c><![CDATA[& 511]]></c> to get digits eliminated.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Assignment"/></term>
	/// <description>A <see cref="Candidate"/> value indicating the candidate set.</description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Clear"/></term>
	/// <description>A <see cref="Cell"/> value indicating the cell to be set empty.</description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Undoing"/></term>
	/// <description>A <see cref="Grid"/> value indicating the newer grid to be replaced with the original one.</description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Redoing"/></term>
	/// <description>A <see cref="Grid"/> value indicating the newer grid to be replaced with the original one.</description>
	/// </item>
	/// <item>
	/// <term><see cref="GridUpdatedBehavior.Replacing"/></term>
	/// <description>A <see cref="Grid"/> value indicating the newer grid to be replaced with the original one.</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// You can use pattern matching to check the internal type such as <c>NewValue is T targetValue</c>,
	/// or just use type casting: <c>(T)NewValue</c> if you know what the behavior is currently.
	/// </para>
	/// </summary>
	public object NewValue { get; } = newValue;

	/// <summary>
	/// Indicates the behavior.
	/// </summary>
	public GridUpdatedBehavior Behavior { get; } = behavior;
}
