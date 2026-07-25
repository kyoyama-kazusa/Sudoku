namespace System;

/// <summary>
/// Provides with extension members on <see cref="Exception"/> and its derived types.
/// </summary>
public static class ExceptionExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TException">The type of exception.</typeparam>
	extension<TException>(TException) where TException : SystemException
	{
		/// <summary>
		/// Throws an instance of type <typeparamref name="TException"/> if the specified assertion is failed.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="failedExpressionString">The string to the argument <paramref name="expression"/>.</param>
		/// <exception cref="InvalidOperationException">Throws when assertion is failed.</exception>
		public static void Assert(
			[DoesNotReturnIf(false)] bool expression,
			[CallerArgumentExpression(nameof(expression))] string failedExpressionString = null!
		)
		{
			if (!expression)
			{
				throw ExceptionConstructorAccessor<TException>.CreateInstance(
					$"The specified expression is failed to be checked: '{failedExpressionString}'."
				)!;
			}
		}
	}
}

/// <summary>
/// Provides an unsafe accessor type of type <typeparamref name="TException"/>.
/// </summary>
/// <typeparam name="TException">The type of exception.</typeparam>
file static class ExceptionConstructorAccessor<TException> where TException : SystemException
{
	/// <summary>
	/// Calls <typeparamref name="TException"/>.<see langword="new"/>(<see cref="string"/>) to create an instance
	/// via the specified <see cref="string"/> value.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>The instance created.</returns>
	[UnsafeAccessor(UnsafeAccessorKind.Constructor)]
	public static extern safe TException CreateInstance(string? message);
}
