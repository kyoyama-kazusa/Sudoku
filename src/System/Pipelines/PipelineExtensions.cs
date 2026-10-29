namespace System.Pipelines;

/// <summary>
/// Provides a way to use pipeline syntax.
/// </summary>
public static class Pipeline
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of instance.</typeparam>
	extension<T>(T) where T : notnull, IParsable<T>
	{
		/// <summary>
		/// Redirects the standard output (STDOUT) of a command to a specified file.
		/// If the file already exists, its contents are overwritten.
		/// </summary>
		/// <param name="input">The desired input data.</param>
		/// <param name="filePath">The file path.</param>
		/// <returns>The file stream.</returns>
		public static Stream operator >(in T input, string filePath) => input > new FileInfo(filePath);

		/// <inheritdoc cref="extension{T}(T).op_GreaterThan(in T, string)"/>
		public static Stream operator >(in T input, FileInfo file)
		{
			var resultStream = file.OpenWrite();
			using var textStream = new StreamWriter(resultStream, Encoding.UTF8, leaveOpen: true);
			textStream.WriteLine(input.ToString());
			textStream.Flush();

			resultStream.Position = 0;
			return resultStream;
		}

		/// <summary>
		/// Redirects the standard input (STDIN) of a command to come from a specified file instead of the keyboard.
		/// </summary>
		/// <param name="input">The desired input data.</param>
		/// <param name="filePath">The file path.</param>
		/// <returns>The file stream.</returns>
		public static Stream operator <(in T input, string filePath) => input < new FileInfo(filePath);

		/// <inheritdoc cref="extension{T}(T).op_LessThan(in T, string)"/>
		public static Stream operator <(in T input, FileInfo file)
		{
			ref var inputRef = ref Unsafe.AsRef(in input);
			var resultStream = file.OpenRead();
			using var textStream = new StreamReader(resultStream, Encoding.UTF8, leaveOpen: true);
			var text = textStream.ReadToEnd();
			inputRef = T.Parse(text, null);

			return resultStream;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of instance.</typeparam>
	/// <typeparam name="TResult">The type of result.</typeparam>
	extension<T, TResult>(T)
		where T : allows ref struct
		where TResult : allows ref struct
	{
		/// <summary>
		/// Performs pipeline operation. Syntax <c>arg | operation</c> means <c>operation(arg)</c>.
		/// </summary>
		/// <param name="left">The instance.</param>
		/// <param name="right">The function that receives <paramref name="left"/> as the only parameter.</param>
		/// <returns>The result.</returns>
		public static TResult operator |(T left, Func<T, TResult> right) => right(left);

		/// <inheritdoc cref="extension{T, TResult}(T).op_BitwiseOr(T, Func{T, TResult})"/>
		public static TResult operator |(T left, delegate*<T, TResult> right) => unsafe (right(left));
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T1">The type of parameter 1.</typeparam>
	/// <typeparam name="T2">The type of parameter 2.</typeparam>
	/// <typeparam name="TResult">The type of result.</typeparam>
	extension<T1, T2, TResult>(ValueTuple<T1, T2>) where TResult : allows ref struct
	{
		/// <summary>
		/// Performs pipeline operation. Syntax <c>(arg1, arg2) | operation</c> means <c>operation(arg1, arg2)</c>.
		/// </summary>
		/// <param name="left">The pair of arguments.</param>
		/// <param name="right">The function that receives <paramref name="left"/> as pair of parameters.</param>
		/// <returns>The result.</returns>
		public static TResult operator |(ValueTuple<T1, T2> left, Func<T1, T2, TResult> right) => right(left.Item1, left.Item2);

		/// <inheritdoc cref="extension{T1, T2, TResult}(ValueTuple{T1, T2}).op_BitwiseOr(ValueTuple{T1, T2}, Func{T1, T2, TResult})"/>
		public static TResult operator |(ValueTuple<T1, T2> left, delegate*<T1, T2, TResult> right)
			=> unsafe (right(left.Item1, left.Item2));
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T1">The type of parameter 1.</typeparam>
	/// <typeparam name="T2">The type of parameter 2.</typeparam>
	/// <typeparam name="T3">The type of parameter 3.</typeparam>
	/// <typeparam name="TResult">The type of result.</typeparam>
	extension<T1, T2, T3, TResult>(ValueTuple<T1, T2, T3>) where TResult : allows ref struct
	{
		/// <summary>
		/// Performs pipeline operation. Syntax <c>(arg1, arg2, arg3) | operation</c> means <c>operation(arg1, arg2, arg3)</c>.
		/// </summary>
		/// <param name="left">The triplet of arguments.</param>
		/// <param name="right">The function that receives <paramref name="left"/> as pair of parameters.</param>
		/// <returns>The result.</returns>
		public static TResult operator |(ValueTuple<T1, T2, T3> left, Func<T1, T2, T3, TResult> right)
			=> right(left.Item1, left.Item2, left.Item3);

		/// <inheritdoc cref="extension{T1, T2, T3, TResult}(ValueTuple{T1, T2, T3}).op_BitwiseOr(ValueTuple{T1, T2, T3}, Func{T1, T2, T3, TResult})"/>
		public static TResult operator |(ValueTuple<T1, T2, T3> left, delegate*<T1, T2, T3, TResult> right)
			=> unsafe (right(left.Item1, left.Item2, left.Item3));
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of argument.</typeparam>
	extension<T>(Action<T>) where T : notnull, IParsable<T>
	{
		/// <inheritdoc cref="extension{T}(T).op_LessThan(in T, string)"/>
		public static Stream operator <(Action<T> action, string filePath) => action < new FileInfo(filePath);

		/// <inheritdoc cref="extension{T}(T).op_LessThan(in T, string)"/>
		public static Stream operator <(Action<T> action, FileInfo file)
		{
			Unsafe.SkipInit(out T value);
			var resultStream = value < file;
			action(value);
			return resultStream;
		}

		/// <inheritdoc cref="extension{T}(T).op_GreaterThan(in T, string)"/>
		[DoesNotReturn]
		[Obsolete("This method has no meaning.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Stream operator >(Action<T> action, string filePath) => throw new NotSupportedException();

		/// <inheritdoc cref="extension{T}(T).op_GreaterThan(in T, string)"/>
		[DoesNotReturn]
		[Obsolete("This method has no meaning.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Stream operator >(Action<T> action, FileInfo file) => throw new NotSupportedException();
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension<T>(Func<T>) where T : notnull, IParsable<T>
	{
		/// <inheritdoc cref="extension{T}(T).op_LessThan(in T, string)"/>
		[DoesNotReturn]
		[Obsolete("This method has no meaning.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Stream operator <(Func<T> func, string filePath) => throw new NotSupportedException();

		/// <inheritdoc cref="extension{T}(T).op_LessThan(in T, string)"/>
		[DoesNotReturn]
		[Obsolete("This method has no meaning.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Stream operator <(Func<T> func, FileInfo file) => throw new NotSupportedException();

		/// <inheritdoc cref="extension{T}(T).op_GreaterThan(in T, string)"/>
		public static Stream operator >(Func<T> func, string filePath) => func() > new FileInfo(filePath);

		/// <inheritdoc cref="extension{T}(T).op_GreaterThan(in T, string)"/>
		public static Stream operator >(Func<T> func, FileInfo file) => func() > file;
	}
}
