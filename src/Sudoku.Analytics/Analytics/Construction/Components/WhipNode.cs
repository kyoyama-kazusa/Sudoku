namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a whip node.
/// </summary>
/// <param name="assignment"><inheritdoc cref="Assignment" path="/summary"/></param>
/// <param name="availableAssignments"><inheritdoc cref="AvailableAssignments" path="/summary"/></param>
/// <param name="parent"><inheritdoc cref="Parent" path="/summary"/></param>
[StructLayout(LayoutKind.Auto)]
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class WhipNode(
	WhipAssignment assignment,
	ReadOnlyMemory<WhipAssignment> availableAssignments,
	WhipNode? parent = null
) : IParentLinkedNode<WhipNode>
{
	/// <summary>
	/// Initializes a <see cref="WhipNode"/> with no next assignments.
	/// </summary>
	/// <param name="assignment">Indicates assignments.</param>
	public WhipNode(WhipAssignment assignment) : this(assignment, ReadOnlyMemory<WhipAssignment>.Empty)
	{
	}


	/// <summary>
	/// Indicates the available assignments.
	/// </summary>
	public ReadOnlyMemory<WhipAssignment> AvailableAssignments { get; } = availableAssignments;

	/// <summary>
	/// Indicates the assignment conclusion.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	public WhipAssignment Assignment { get; } = assignment;

	/// <inheritdoc/>
	public WhipNode Root
	{
		get
		{
			var (result, p) = (this, Parent);
			while (p is not null)
			{
				_ = (result = p, p = p.Parent);
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the parent node.
	/// </summary>
	public WhipNode? Parent { get; } = parent;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.WhipNode;


	/// <inheritdoc/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var parentString = Parent is { Assignment: var assignment } ? converter.CandidateConverter(assignment.Map) : "<null>";
		return $$"""{{nameof(WhipNode)}} { {{nameof(Assignment)}} = {{Assignment}}, {{nameof(Parent)}} = {{parentString}} }""";
	}


	/// <summary>
	/// Creates a <see cref="WhipNode"/> instance with parent node.
	/// </summary>
	/// <param name="current">The current node.</param>
	/// <param name="parent">The parent node.</param>
	/// <returns>The new node created.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WhipNode operator >>(WhipNode current, WhipNode? parent)
		=> new(current.Assignment, current.AvailableAssignments, parent);
}
