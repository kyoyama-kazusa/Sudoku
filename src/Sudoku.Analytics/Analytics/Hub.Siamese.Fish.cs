namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class Siamese
	{
		/// <summary>
		/// Provides a way to construct Siamese fishes.
		/// </summary>
		public static class Fish
		{
			/// <summary>
			/// Get all possible Siamese patterns of type <see cref="FishStep"/> for the specified accumulator.
			/// </summary>
			/// <param name="accumulator">The accumulator.</param>
			/// <param name="grid">The grid.</param>
			/// <returns>All Siamese steps of type <see cref="FishStep"/>.</returns>
			public static ReadOnlySpan<FishStep> GetSiamese(List<FishStep> accumulator, in Grid grid)
			{
				var result = new List<FishStep>();
				var stepsSpan = accumulator.AsSpan();
				for (var index1 = 0; index1 < accumulator.Count - 1; index1++)
				{
					var fish1 = stepsSpan[index1];
					for (var index2 = index1 + 1; index2 < accumulator.Count; index2++)
					{
						var fish2 = stepsSpan[index2];
						if (check(grid, fish1, fish2, out var siameseStep))
						{
							// Siamese fish contain more eliminations, we should insert them into the first place.
							result.Add(siameseStep);
						}
					}
				}
				return result.AsSpan();


				static bool check(in Grid puzzle, FishStep fish1, FishStep fish2, [NotNullWhen(true)] out FishStep? siameseStep)
				{
					if (fish1.BaseSetsMask != fish2.BaseSetsMask || fish1.Digit != fish2.Digit)
					{
						// A Siamese fish must hold a pair of fish containing same base sets, with a same digit.
						goto ReturnFalse;
					}

					if (fish1.Fins == fish2.Fins || (fish1.Fins & fish2.Fins) == fish2.Fins || (fish2.Fins & fish1.Fins) == fish1.Fins)
					{
						// They shouldn't be a same fish, or all fins from one fish belongs to the other fish. 
						goto ReturnFalse;
					}

					if (!fish1.Fins)
					{
						// A Siamese fish must contain at least one fin.
						goto ReturnFalse;
					}

					if ((fish1, fish2) is (ComplexFishStep { Pattern.ShapeKind: var a }, ComplexFishStep { Pattern.ShapeKind: var b }) && a != b)
					{
						// They cannot hold different kind of shapes.
						goto ReturnFalse;
					}

					var set1Conclusions = fish1.Conclusions.AsSet();
					var set2Conclusions = fish2.Conclusions.AsSet();
					if (set1Conclusions == set2Conclusions)
					{
						// Two fish cannot contain total same conclusions.
						goto ReturnFalse;
					}

					// They can form a Siamese fish.
					// Check for merged data.
					var mergedFins = fish1.Fins | fish2.Fins;
					var coveredSetsMask = fish1.CoverSetsMask | fish2.CoverSetsMask;
					var siameseCoverSetsMask = fish1.CoverSetsMask ^ fish2.CoverSetsMask;
					var conclusions = (set1Conclusions | set2Conclusions).ToArray();
					var isSashimi = (fish1.IsSashimi, fish2.IsSashimi) switch
					{
						(true, not null) or (not null, true) => true,
						(false, false) => false,
						(null, null) => default(bool?),
						_ => throw new InvalidOperationException(SR.ExceptionMessage("SashimiPropertyInvalid"))
					};

					// Check for cannibalism.
					var isCannibalism = false;
					foreach (var house in fish1.BaseSetsMask)
					{
						var cells = HousesMap[house] & CandidatesMap[fish1.Digit];
						if (Array.Exists(conclusions, conclusion => cells.Contains(conclusion.Cell)))
						{
							isCannibalism = true;
							break;
						}
					}

					// Normal fish contains a direct view, which will not be useful here.
					var fish1ViewNodes = fish1.Views![0];
					var fish2ViewNodes = fish2.Views![0];
					View view = [
						.. collectViewNodes(fish1ViewNodes, fish2ViewNodes),
						.. collectViewNodes(fish2ViewNodes, fish1ViewNodes),
						.. from house in fish1.BaseSetsMask select new HouseViewNode(ColorDescriptorAlias.Normal, house),
						.. from house in siameseCoverSetsMask select new HouseViewNode(ColorDescriptorAlias.Auxiliary3, house),
						..
						from house in coveredSetsMask & ~siameseCoverSetsMask
						select new HouseViewNode(ColorDescriptorAlias.Auxiliary2, house)
					];

					siameseStep = fish1 switch
					{
						NormalFishStep => new NormalFishStep(
							conclusions,
							[view, fish1ViewNodes, fish2ViewNodes],
							fish1.Options,
							fish1.Digit,
							fish1.BaseSetsMask,
							coveredSetsMask,
							mergedFins,
							isSashimi,
							true
						),
						ComplexFishStep p => new ComplexFishStep(
							conclusions,
							[view, fish1ViewNodes, fish2ViewNodes],
							fish1.Options,
							fish1.Digit,
							fish1.BaseSetsMask,
							coveredSetsMask,
							mergedFins & (p.Exofins | ((ComplexFishStep)fish2).Exofins),
							mergedFins & (p.Endofins | ((ComplexFishStep)fish2).Endofins),
							p.Pattern.ShapeKind switch
							{
								FishShapeKind.Franken => true,
								FishShapeKind.Mutant => false,
								_ => throw new InvalidOperationException(SR.ExceptionMessage("ComplexFishCannotBeNormal"))
							},
							isSashimi,
							isCannibalism,
							true
						),
						_ => throw new NotSupportedException(SR.ExceptionMessage("MemberNotSupported"))
					};
					return true;

				ReturnFalse:
					siameseStep = null;
					return false;


					static ReadOnlySpan<CandidateViewNode> collectViewNodes(View fish1ViewNodes, View fish2ViewNodes)
					{
						var result = new List<CandidateViewNode>();
						foreach (var node1 in fish1ViewNodes)
						{
							if (node1 is not CandidateViewNode { Identifier: ColorDescriptorAlias id1, Candidate: var candidate1 })
							{
								continue;
							}

							var n = fish2ViewNodes.FirstOrDefault(node => node is CandidateViewNode { Candidate: var candidate2 } && candidate1 == candidate2);
							if (n is not { Identifier: ColorDescriptorAlias id2 })
							{
								throw new InvalidOperationException(SR.ExceptionMessage("NormalFishViewInvalid"));
							}

							result.Add(
								new CandidateViewNode(
									(id1, id2) switch
									{
										(ColorDescriptorAlias.Endofin, _) => ColorDescriptorAlias.Endofin,
										(_, ColorDescriptorAlias.Endofin) => ColorDescriptorAlias.Endofin,
										(ColorDescriptorAlias.Exofin, _) => ColorDescriptorAlias.Exofin,
										(_, ColorDescriptorAlias.Exofin) => ColorDescriptorAlias.Exofin,
										_ => ColorDescriptorAlias.Normal
									},
									candidate1
								)
							);
						}

						return result.AsSpan();
					}
				}
			}
		}
	}
}
