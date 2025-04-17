namespace Sudoku.Analytics;

/// <summary>
/// Represents a provider type that can create built-in <see cref="StepSearcher"/> instances.
/// </summary>
/// <seealso cref="StepSearcher"/>
public static class StepSearcherFactory
{
	/// <summary>
	/// Indicates the current assembly.
	/// </summary>
	private static readonly Assembly ThisAssembly = typeof(StepSearcherFactory).Assembly;


	/// <summary>
	/// Indicates built-in step searchers defined.
	/// </summary>
	public static ReadOnlyMemory<StepSearcher> StepSearchers
	{
		get
		{
			var result = new SortedSet<StepSearcher>();
			foreach (var type in ThisAssembly.GetDerivedTypes<StepSearcher>())
			{
				if (type.IsDefined<StepSearcherAttribute>() && type.HasParameterlessConstructor)
				{
					result.Add(GetStepSearcher(type));
				}
			}
			return result.ToArray();
		}
	}


	/// <summary>
	/// <inheritdoc cref="GetStepSearcher(Type)" path="/summary"/>
	/// </summary>
	/// <param name="typeName">The raw type name. Please note that the string text shouldn't contain its containing namespace.</param>
	/// <returns><inheritdoc cref="GetStepSearcher(Type)" path="/returns"/></returns>
	/// <exception cref="InvalidOperationException">
	/// Throws when the corresponding <see cref="Type"/> reflection result is not found.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static StepSearcher GetStepSearcher(string typeName)
		=> GetStepSearcher(ThisAssembly.GetDerivedTypes<StepSearcher>().FirstOrDefault(t => t.Name == typeName)!);

	/// <summary>
	/// The internal method to get all <see cref="StepSearcher"/> instances derived from <paramref name="type"/> defined in this assembly.
	/// </summary>
	/// <param name="type">The type of the step searcher.</param>
	/// <returns>An array of <see cref="StepSearcher"/> instances found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static StepSearcher GetStepSearcher(Type type) => (StepSearcher)Activator.CreateInstance(type)!;
}
