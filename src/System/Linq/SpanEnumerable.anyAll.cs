namespace System.Linq;

public partial class SpanEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <param name="source">The source collection.</param>
	extension<TSource>(ReadOnlySpan<TSource> source)
	{
		/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.Any(Func{TSource, bool})"/>
		public bool Any(Func<TSource, bool> match)
		{
			foreach (var element in source)
			{
				if (match(element))
				{
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc cref="Any{TSource}(ReadOnlySpan{TSource}, Func{TSource, bool})"/>
		public bool AnyUnsafe(delegate*<TSource, bool> match)
		{
			foreach (var element in source)
			{
				if (unsafe (match(element)))
				{
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc cref="IAnyAllMethod{TSelf, TSource}.All(Func{TSource, bool})"/>
		public bool All(Func<TSource, bool> match)
		{
			foreach (var element in source)
			{
				if (!match(element))
				{
					return false;
				}
			}
			return true;
		}

		/// <inheritdoc cref="All{TSource}(ReadOnlySpan{TSource}, Func{TSource, bool})"/>
		public bool AllUnsafe(delegate*<TSource, bool> match)
		{
			foreach (var element in source)
			{
				if (!unsafe (match(element)))
				{
					return false;
				}
			}
			return true;
		}
	}
}
