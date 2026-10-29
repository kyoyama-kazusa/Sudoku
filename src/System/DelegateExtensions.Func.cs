namespace System;

public partial class DelegateExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of result value.</typeparam>
	extension<T>(Func<T>) where T : allows ref struct
	{
		/// <summary>
		/// Indicates the pointer to method <see cref="SelfMethod{T}(T)"/>.
		/// </summary>
		/// <seealso cref="SelfMethod{T}(T)"/>
		public static delegate*<T, T> SelfMethodPtr => &SelfMethod;

		/// <summary>
		/// Creates a <see cref="Func{T, TResult}"/> instance that directly returns parameter.
		/// </summary>
		public static Func<T, T> Self => SelfMethod;


		/// <summary>
		/// Represents a method that directly returns <paramref name="instance"/>.
		/// </summary>
		public static T SelfMethod(T instance) => instance;
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of input value.</typeparam>
	/// <typeparam name="TResult">The type of output value (result).</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T, TResult>(Func<Func<T, TResult>, Func<T, TResult>> @this)
		where T : allows ref struct
		where TResult : allows ref struct
	{
		/// <summary>
		/// (Easter egg) Represents Y-Combinator. This method can allow you create recursive lambdas.
		/// For example, this code snippet will calculate for factorial of the specified digit:
		/// <code><![CDATA[
		/// // Define a nested lambda that performs a recursion.
		/// var nested = (Func<int, int> lambda) => (int value) => value switch
		/// {
		///     < 0 => throw new ArgumentException("Invalid argument", nameof(value)),
		///     0 or 1 => 1,
		///     _ => value * lambda(value - 1)
		/// };
		/// var factorial = nested.YCombinator; // To get Y-combinator of this type.
		/// Console.WriteLine(factorial(5)); // 120
		/// ]]></code>
		/// </summary>
		/// <returns>A function that creates a nesting lambda that is a recursive lambda.</returns>
		public Func<T, TResult> YCombinator => value => @this(@this.YCombinator)(value);
	}
}
