namespace Sudoku.Categorization;

/// <summary>
/// Provides with extension methods on <see cref="TechniqueSet"/>.
/// </summary>
/// <seealso cref="TechniqueSet"/>
public static class TechniqueSetExtensions
{
	/// <summary>
	/// Try to get common suitable <see cref="Type"/> referring to a <see cref="Step"/> type,
	/// whose containing property <see cref="Step.Code"/> can create this technique.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <returns>
	/// A valid <see cref="Type"/> instance, or <see langword="null"/> if it may not be referred by all <see cref="Step"/>
	/// derived types.
	/// </returns>
	/// <seealso href="https://en.wikipedia.org/wiki/Lowest_common_ancestor">Wikipedia: Lowest Common Ancestor</seealso>
	/// <seealso href="https://www.zhihu.com/question/369088717/answer/2613233529">
	/// Zhihu: Which way to calculate LCA in a multiway tree is fastest?
	/// </seealso>
	public static Type? GetCommonSuitableStepType(this TechniqueSet @this)
	{
		switch (@this)
		{
			case []:
			{
				return null;
			}
			case [{ SuitableStepType: { } stepType } technique]:
			{
				return stepType;
			}
			case var _ when @this.Exists(static technique => technique.SuitableStepType is null):
			{
				// Return null if at least one technique doesn't refer to a valid step type.
				return null;
			}
			default:
			{
				// Firstly we should check for the depth of step type deriving for each technique.
				var depthDictionary = new Dictionary<Technique, int>(@this.Count);
				foreach (var technique in @this)
				{
					depthDictionary.Add(technique, getDepth(technique));
				}

				// Then adjust the nodes to the same level.
				var min = depthDictionary.MinByValue();
				var typesSet = new List<Type>();
				foreach (var technique in @this)
				{
					var type = technique.SuitableStepType!;
					for (var i = 0; i < depthDictionary[technique] - min; i++)
					{
						type = type.BaseType!;
					}

					typesSet.Add(type);
				}

				// Now we have all nodes at the same depth. Advance each step to its parent node, and check whether they are same.
				// If same, the node should be the lowest common ancestor (LCA) for them.
				var commonAncestors = new HashSet<Type>();
				for (var i = min; i >= 0; i--)
				{
					commonAncestors.Clear();

					foreach (var type in typesSet)
					{
						commonAncestors.Add(type.BaseType!);
					}

					if (commonAncestors.Count == 1 && commonAncestors.First() is var result
						&& result != typeof(Step) && result != typeof(object))
					{
						return result;
					}

					// Replace and do next loop.
					typesSet.Clear();
					typesSet.AddRange(commonAncestors);
				}

				// None found.
				return null;
			}
		}


		static int getDepth(Technique technique)
		{
			var result = 0;
			for (var temp = technique.SuitableStepType!; temp != typeof(Step); temp = temp!.BaseType)
			{
				result++;
			}
			return result;
		}
	}
}
