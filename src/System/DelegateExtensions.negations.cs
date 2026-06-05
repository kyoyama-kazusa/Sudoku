namespace System;

public partial class DelegateExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(Func<bool>)
	{
		/// <summary>
		/// Negates the logic of the specified method.
		/// </summary>
		/// <param name="value">The original method.</param>
		/// <returns>A new method that returns the negated value of <paramref name="value"/>.</returns>
		public static Func<bool> operator ~(Func<bool> value) => () => !value();
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T>(Func<T, bool>)
	{
		/// <inheritdoc cref="extension(Func{bool}).op_OnesComplement(Func{bool})"/>
		public static Func<T, bool> operator ~(Func<T, bool> value) => arg => !value(arg);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T1, T2>(Func<T1, T2, bool>)
	{
		/// <inheritdoc cref="extension(Func{bool}).op_OnesComplement(Func{bool})"/>
		public static Func<T1, T2, bool> operator ~(Func<T1, T2, bool> value) => (arg1, arg2) => !value(arg1, arg2);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T1, T2, T3>(Func<T1, T2, T3, bool>)
	{
		/// <inheritdoc cref="extension(Func{bool}).op_OnesComplement(Func{bool})"/>
		public static Func<T1, T2, T3, bool> operator ~(Func<T1, T2, T3, bool> value)
			=> (arg1, arg2, arg3) => !value(arg1, arg2, arg3);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool>)
	{
		/// <inheritdoc cref="extension(Func{bool}).op_OnesComplement(Func{bool})"/>
		public static Func<T1, T2, T3, T4, bool> operator ~(Func<T1, T2, T3, T4, bool> value)
			=> (arg1, arg2, arg3, arg4) => !value(arg1, arg2, arg3, arg4);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T>(Predicate<T>)
	{
		/// <inheritdoc cref="extension(Func{bool}).op_OnesComplement(Func{bool})"/>
		public static Predicate<T> operator ~(Predicate<T> value) => arg => !value(arg);
	}
}
