namespace Sudoku.Descriptors;

public readonly partial struct ColorDescriptor
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
		bool TryGetValue(out (byte A, byte R, byte G, byte B) value);

		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="/g/csharp15/feature[@name='union']/target[@name='try-get-value-method']"/>
		bool TryGetValue(out ColorDescriptorAlias value);


		/// <summary>
		/// Creates a <see cref="ColorDescriptor"/> instance via the specified <see cref="int"/> value indicating ID.
		/// </summary>
		/// <param name="id">The ID value.</param>
		/// <returns>The <see cref="ColorDescriptor"/> instance created.</returns>
		static ColorDescriptor Create(int id) => new(id);

		/// <summary>
		/// Creates a <see cref="ColorDescriptor"/> instance via the specified quadruple of ARGB values.
		/// </summary>
		/// <param name="value">The quadruple of ARGB values.</param>
		/// <returns>The <see cref="ColorDescriptor"/> instance created.</returns>
		static ColorDescriptor Create((byte A, byte R, byte G, byte B) value) => new(value);

		/// <summary>
		/// Creates a <see cref="ColorDescriptor"/> instance via the specified <see cref="ColorDescriptorAlias"/> value
		/// indicating item.
		/// </summary>
		/// <param name="item">The aliased item.</param>
		/// <returns>The <see cref="ColorDescriptor"/> instance created.</returns>
		static ColorDescriptor Create(ColorDescriptorAlias item) => new(item);
	}
}
