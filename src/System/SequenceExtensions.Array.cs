namespace System;

public partial class SequenceExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension<T>(T[]? @this)
	{
		/// <inheritdoc cref="MemoryExtensions.AsSpan{T}(T[])"/>
		public ReadOnlySpan<T> AsReadOnlySpan() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(Array)
	{
		/// <summary>
		/// Initializes an array, using the specified method to initialize each element.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="array">The array.</param>
		/// <param name="initializer">The initializer callback method.</param>
		public static void InitializeArray<T>(T?[] array, ArrayInitializer<T> initializer)
		{
			foreach (ref var element in array.AsSpan())
			{
				initializer(ref element);
			}
		}

		/// <summary>
		/// Creates an array that only contains one element.
		/// </summary>
		/// <typeparam name="T">The type of element.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>The result.</returns>
		public static T[] Single<T>(T value) => [value];

		/// <summary>
		/// Flats the specified 2D array into an 1D array.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="array">An array of elements of type <typeparamref name="T"/>.</param>
		/// <returns>An 1D array.</returns>
		public static T[] Flat<T>(T[,] array)
		{
			var result = new T[array.Length];
			for (var i = 0; i < array.GetLength(0); i++)
			{
				for (var (j, l2) = (0, array.GetLength(1)); j < l2; j++)
				{
					result[i * l2 + j] = array[i, j];
				}
			}
			return result;
		}

		/// <summary>
		/// Converts an array into a <see cref="string"/>.
		/// </summary>
		/// <typeparam name="T">The type of each element inside array.</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>The string representation.</returns>
		public static string ToArrayString<T>(T[] array) => Array.ToArrayString(array, null);

		/// <summary>
		/// Converts an array into a <see cref="string"/>, using the specified formatter method
		/// that can convert an instance of type <typeparamref name="T"/> into a <see cref="string"/> representation.
		/// </summary>
		/// <typeparam name="T">The type of each element inside array.</typeparam>
		/// <param name="array">The array.</param>
		/// <param name="valueConverter">The value converter method.</param>
		/// <returns>The string representation.</returns>
		public static string ToArrayString<T>(T[] array, Func<T, string?>? valueConverter)
		{
			valueConverter ??= static value => value?.ToString();
			return $"[{string.Join(", ", from element in array select valueConverter(element))}]";
		}

		/// <inheritdoc cref="ToArrayString{T}(T[])"/>
		[OverloadResolutionPriority(1)]
		public static string ToArrayString<T>(T[][] array) => Array.ToArrayString(array, null);

		/// <inheritdoc cref="ToArrayString{T}(T[], Func{T, string?})"/>
		[OverloadResolutionPriority(1)]
		public static string ToArrayString<T>(T[][] array, Func<T, string?>? valueConverter)
		{
			var sb = new StringBuilder();
			sb.Append('[').AppendLine();
			for (var i = 0; i < array.Length; i++)
			{
				var element = array[i];
				sb.Append("  ").Append(Array.ToArrayString(element, valueConverter));
				if (i != array.Length - 1)
				{
					sb.Append(',');
				}
				sb.AppendLine();
			}
			sb.Append(']');
			return sb.ToString();
		}

		/// <inheritdoc cref="ToArrayString{T}(T[])"/>
		public static string ToArrayString<T>(T[,] array) => Array.ToArrayString(array, null);

		/// <inheritdoc cref="ToArrayString{T}(T[], Func{T, string?})"/>
		public static string ToArrayString<T>(T[,] array, Func<T, string?>? valueConverter)
		{
			valueConverter ??= static value => value?.ToString();

			var (m, n) = (array.GetLength(0), array.GetLength(1));
			var sb = new StringBuilder();
			sb.Append('[').AppendLine();
			for (var i = 0; i < m; i++)
			{
				sb.Append("  ");
				for (var j = 0; j < n; j++)
				{
					var element = array[i, j];
					sb.Append(valueConverter(element));
					if (j != n - 1)
					{
						sb.Append(", ");
					}
				}
				if (i != m - 1)
				{
					sb.Append(',');
				}
				sb.AppendLine();
			}
			sb.Append(']');
			return sb.ToString();
		}

		/// <summary>
		/// Returns the one-dimensional array representation from the two-dimensional array.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>The <see cref="Span{T}"/> casted.</returns>
		public static Span<T> AsSpanUnsafe<T>(T[,] array)
		{
			fixed (T* ptr = &array[0, 0])
			{
				return new(ptr, array.Length);
			}
		}

		/// <summary>
		/// Returns the one-dimensional array representation from the three-dimensional array.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>The <see cref="Span{T}"/> casted.</returns>
		public static Span<T> AsSpanUnsafe<T>(T[,,] array)
		{
			fixed (T* ptr = &array[0, 0, 0])
			{
				return new(ptr, array.Length);
			}
		}

		/// <summary>
		/// Reinterpret the specified multiple dimensional array into a <see cref="Span{T}"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="array">The multiple dimensional array to be reinterpreted.</param>
		/// <returns>A <see cref="Span{T}"/> instance.</returns>
		public static Span<T> AsSpanUnsafe<T>(Array array) where T : unmanaged
		{
			// Calculate total number of elements.
			// In multiple dimensional array, the result will be the production of lengths from all dimensions.
			var length = array.Length;

			// Pin the array so that it doesn't move during GC.
			var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				// Get a pointer to the beginning of the array.
				return new((void*)handle.AddrOfPinnedObject(), length);
			}
			finally
			{
				// Always free the handle to avoid memory leaks.
				handle.Free();
			}
		}
	}
}
