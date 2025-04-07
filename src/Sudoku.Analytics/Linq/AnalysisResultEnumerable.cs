namespace Sudoku.Linq;

/// <summary>
/// Provides with some LINQ methods for type <see cref="AnalysisResult"/>.
/// </summary>
/// <seealso cref="AnalysisResult"/>
public static class AnalysisResultEnumerable
{
	/// <summary>
	/// Cast the object into a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TStep"/> instances.
	/// </summary>
	/// <typeparam name="TStep">The type of each element casted.</typeparam>
	/// <param name="this">The instance to be casted.</param>
	/// <returns>A list of <typeparamref name="TStep"/> instances.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<TStep> Cast<TStep>(this AnalysisResult @this) where TStep : Step
		=> from element in @this select (TStep)element;

	/// <summary>
	/// Filters the current collection, preserving <see cref="Step"/> instances that are satisfied the specified condition.
	/// </summary>
	/// <param name="this">The instance.</param>
	/// <param name="condition">The condition to be satisfied.</param>
	/// <returns>An array of <see cref="Step"/> instances.</returns>
	public static ReadOnlySpan<Step> Where(this AnalysisResult @this, Func<Step, bool> condition)
	{
		if (@this.StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var result = new List<Step>(stepsCount);
		foreach (var step in steps)
		{
			if (condition(step))
			{
				result.Add(step);
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Projects the collection, to an immutable result of target type.
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <param name="this">The instance.</param>
	/// <param name="selector">
	/// The selector to project the <see cref="Step"/> instance into type <typeparamref name="TResult"/>.
	/// </param>
	/// <returns>The projected collection of element type <typeparamref name="TResult"/>.</returns>
	public static ReadOnlySpan<TResult> Select<TResult>(this AnalysisResult @this, Func<Step, TResult> selector)
	{
		if (@this.StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var arr = new TResult[stepsCount];
		var i = 0;
		foreach (var step in steps)
		{
			arr[i++] = selector(step);
		}
		return arr;
	}

	/// <summary>
	/// Filters the current collection, preserving steps that are of type <typeparamref name="TStep"/>.
	/// </summary>
	/// <typeparam name="TStep">The type of the step you want to get.</typeparam>
	/// <param name="this">The instance.</param>
	/// <returns>An array of <typeparamref name="TStep"/> instances.</returns>
	public static ReadOnlySpan<TStep> OfType<TStep>(this AnalysisResult @this) where TStep : Step
	{
		if (@this.StepsSpan is not { Length: var stepsCount and not 0 } steps)
		{
			return [];
		}

		var list = new List<TStep>(stepsCount);
		foreach (var element in steps)
		{
			if (element is TStep current)
			{
				list.Add(current);
			}
		}
		return list.AsSpan();
	}
}
