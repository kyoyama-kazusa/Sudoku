namespace Sudoku.Analytics.Construction.Components;

public partial union NodeOrNodeSet
{
	/// <summary>
	/// Provides an enumerator of the current type.
	/// </summary>
	/// <param name="nodes">The nodes.</param>
	public ref struct Enumerator(NodeOrNodeSet nodes) : IEnumerator<Node>
	{
		/// <summary>
		/// The backing enumerator.
		/// </summary>
		private AnonymousSpanEnumerator<Node> _enumerator =
			nodes switch { Node node => new((Node[])[node]), NodeSet nodes => nodes.GetEnumerator(), null => new([]) };


		/// <inheritdoc/>
		public readonly Node Current => _enumerator.Current;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => _enumerator.MoveNext();

		/// <inheritdoc/>
		readonly void IDisposable.Dispose()
		{
		}

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotSupportedException();
	}
}
