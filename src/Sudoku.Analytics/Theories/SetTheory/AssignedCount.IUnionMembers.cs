namespace Sudoku.Theories.SetTheory;

public partial record struct AssignedCount
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
		bool TryGetValue(out (int Min, int Max) value);


		/// <summary>
		/// Creates an <see cref="AssignedCount"/> instance via the specified value of type <see cref="int"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>An <see cref="AssignedCount"/> instance created.</returns>
		static AssignedCount Create(int value) => new(value, value);

		/// <summary>
		/// Creates an <see cref="AssignedCount"/> instance via the specified value of type
		/// <see cref="ValueTuple{T1, T2}"/> of types <see cref="int"/> (min value) and <see cref="int"/> (max value).
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>An <see cref="AssignedCount"/> instance created.</returns>
		static AssignedCount Create((int Min, int Max) value) => new(value.Min, value.Max);
	}
}
