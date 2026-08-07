namespace Sudoku.Theories.SetTheory;

public partial struct Rank
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp15/feature[@name='union']/target[@name='union-interface']"/>
	public interface IUnionMembers : IUnion
	{
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
		bool TryGetValue(out int value);

		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
		bool TryGetValue([NotNullWhen(true)] out int[]? value);


		/// <summary>
		/// Creates a <see cref="Rank"/> instance via the specified rank value as absolute one.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The <see cref="Rank"/> value created.</returns>
		static Rank Create(int value) => new(value);

		/// <summary>
		/// Creates a <see cref="Rank"/> instance via the specified array of values as sequenced one.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <returns>The <see cref="Rank"/> value created.</returns>
		static Rank Create(int[] values) => new(values);
	}
}
