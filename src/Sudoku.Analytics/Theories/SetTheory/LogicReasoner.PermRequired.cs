namespace Sudoku.Theories.SetTheory;

public partial class LogicReasoner
{
	/// <summary>
	/// Represents equivalent implementation of the parent type <see cref="LogicReasoner"/>,
	/// but an extra parameter <c>permutations</c> is required;
	/// sometimes <c>conclusions</c> is also required.
	/// </summary>
	/// <seealso cref="LogicReasoner"/>
	public static class PermRequired
	{
		/// <inheritdoc cref="LogicReasoner.GetRank(ref readonly Logic, out IReadOnlyDictionary{Conclusion, Logic})"/>
		public static Rank GetRank(ref readonly Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations, out IReadOnlyDictionary<Conclusion, Logic> sublogics)
		{
			var resultViews = new Dictionary<Conclusion, Logic>();
			if (permutations.Length == 0)
			{
				sublogics = resultViews;
				return Rank.Illegal;
			}

			// Optimization: If all candidates in this pattern is exact-covered, we can directly calculate rank.
			if (logic.IsExactCovered)
			{
				foreach (var conclusion in conclusions)
				{
					resultViews.Add(conclusion, logic);
				}

				sublogics = resultViews;
				return logic.Links.Count - logic.Truths.Count;
			}

			// Create a minimal logic lookup table as cache.
			var cachedMinimalLogics = new Dictionary<List<Space>, Logic>(ListOfSpaceEqualityComparer.Instance);
			var rankList = new SortedSet<int>();
			var onlyEliminations = conclusions.Eliminations;
			foreach (var elimination in onlyEliminations)
			{
				// Optimization: If the elimination hits minimal pattern cache, read cache instead of doing calculation.
				// Find for lightup links.
				var lightupLinks = new List<Space>(4);
				foreach (var link in elimination.Candidate.Spaces)
				{
					if (logic.Links.Contains(link))
					{
						lightupLinks.Add(link);
					}
				}

				// Read cache. If minimal pattern is found in cached field,
				// we can directly return the value and its corresponding rank.
				if (cachedMinimalLogics.TryGetValue(lightupLinks, out var minimalCached))
				{
					resultViews.Add(elimination, minimalCached);
					rankList.Add(minimalCached.Links.Count - minimalCached.Truths.Count);
					continue;
				}

				// Otherwise, a conclusion with different origin should be checked.
				var minimal = GetMinimalPattern(in logic, elimination.Candidate, conclusions, permutations);

				// Optimization: If the current minimal pattern is equal to the whole pattern,
				// we cannot find out any other eliminations corresponding to a smaller pattern.
				if (minimal == logic)
				{
					// Record values for the other conclusions.
					foreach (var conclusion in onlyEliminations)
					{
						resultViews.Add(conclusion, minimal);
					}
					rankList.Add(minimal.Links.Count - minimal.Truths.Count);
					break;
				}

				resultViews.Add(elimination, minimal);
				rankList.Add(minimal.Links.Count - minimal.Truths.Count);

				// Cache eliminations.
				cachedMinimalLogics.Add(lightupLinks, minimal);
			}

			sublogics = resultViews;
			return rankList is [var singleElement] ? singleElement : (int[])[.. rankList];
		}

		/// <inheritdoc cref="LogicReasoner.GetAssignedCount(ref readonly Logic)"/>
		public static AssignedCount GetAssignedCount(ref readonly Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			// Optimize: If there're only cell truths, just return the size of truths.
			if (logic.Truths == SpaceSet.AllCells)
			{
				return new(logic.Truths.Count, logic.Truths.Count);
			}

			var (min, max) = (int.MaxValue, int.MinValue);
			foreach (var permutation in permutations)
			{
				var count = permutation.Assignments.Length;
				if (count <= min)
				{
					min = count;
				}
				if (count >= max)
				{
					max = count;
				}
			}
			return (min, max) is ( >= 0, >= 0) ? min == max ? min : (min, max) : default;
		}

		/// <inheritdoc cref="LogicReasoner.GetConclusions(ref readonly Logic)"/>
		public static ConclusionSet GetConclusions(ref readonly Logic logic, ReadOnlySpan<Permutation> permutations, bool checkingLinks, bool checkingZone)
		{
			ref readonly var grid = ref logic.Grid;
			ref readonly var links = ref logic.Links;
			var candidatesMap = grid.CandidatesMap;
			var emptyCells = grid.EmptyCells;

			// Construct a link lookup. This lookup table records candidates covered in the grid.
			// We'll use this variable in checking light-up links for each permutation, in order to find strict conclusions
			// when option 'checkingLinks' is on.
			var result = ConclusionSet.Empty;
			var i = 0;
			foreach (var permutation in permutations)
			{
				var tempConclusions = ConclusionSet.Empty;

				// Links are ignored to be checked.
				foreach (var candidate in permutation)
				{
					var cell = candidate / 9;
					var digit = candidate % 9;
					if (checkingLinks)
					{
						// We should find for all light-up links, and find conclusions.
						// If the conclusion doesn't include in every links, the candidate may not be a valid one.
						foreach (var link in permutation.LightupLinks)
						{
							if (link.Contains(candidate))
							{
								goto CheckExistence;
							}
						}
						continue;
					}

				CheckExistence:
					if (checkingZone)
					{
						// If candidates exists, we can eliminate or set it without checking whether it is on a link or not.
						foreach (var c in Peer.PeersMap[cell] & emptyCells)
						{
							tempConclusions.Add(new(Elimination, c, digit));
						}
						foreach (var d in (Mask)(Grid.MaxCandidatesMask & ~(1 << digit)))
						{
							tempConclusions.Add(new(Elimination, cell, d));
						}
						tempConclusions.Add(new(Assignment, cell, digit));
					}
					else
					{
						// If candidates exists, we can eliminate or set it without checking whether it is on a link or not.
						foreach (var c in Peer.PeersMap[cell] & candidatesMap[digit])
						{
							tempConclusions.Add(new(Elimination, c, digit));
						}
						foreach (var d in (Mask)(grid.GetCandidates(cell) & ~(1 << digit)))
						{
							tempConclusions.Add(new(Elimination, cell, d));
						}
						tempConclusions.Add(new(Assignment, cell, digit));
					}
				}

				if (i++ == 0)
				{
					result |= tempConclusions;
				}
				else
				{
					result &= tempConclusions;
				}
			}

			if (checkingZone)
			{
				// Extra checking: Remove all conclusions that are from truths.
				ref readonly var truths = ref logic.Truths;
				ref readonly var emptyGrid = ref Grid.Empty;
				foreach (var conclusion in result[..])
				{
					foreach (var truth in truths)
					{
						if (truth.GetAvailableRange(emptyGrid).Contains(conclusion.Candidate))
						{
							result -= conclusion;
						}
					}
				}
			}

			// Return the result.
			return result;
		}

		/// <inheritdoc cref="LogicReasoner.GetRank0Links(ref readonly Logic)"/>
		public static SpaceSet GetRank0Links(ref readonly Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			if (permutations.Length == 0)
			{
				return SpaceSet.Empty;
			}

			var result = logic.Links;
			foreach (var permutation in permutations)
			{
				if (!(result &= [.. permutation.LightupLinks.Span]))
				{
					break;
				}
			}
			return result;
		}

		/// <inheritdoc cref="LogicReasoner.GetRank0Eliminations(ref readonly Logic)"/>
		public static CandidateMap GetRank0Eliminations(ref readonly Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			var result = CandidateMap.Empty;
			if (permutations.Length == 0)
			{
				return result;
			}

			foreach (var (type, candidate) in conclusions)
			{
				if (type != Elimination)
				{
					continue;
				}

				foreach (var link in GetRank0Links(in logic, permutations))
				{
					if (link.Contains(candidate))
					{
						result.Add(candidate);
					}
				}
			}
			return result;
		}

		/// <inheritdoc cref="LogicReasoner.GetMinimalTruths(ref readonly Logic, Candidate)"/>
		public static SpaceSet GetMinimalTruths(ref readonly Logic logic, Candidate elimination, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var truths = ref logic.Truths;
			if (truths.Count <= 2)
			{
				// There's nothing to do - A minimum pattern is an X-Wing which cannot reduce.
				return truths;
			}

			var allEliminations = CandidateMap.Empty;
			foreach (var (type, candidate) in conclusions)
			{
				if (type == Elimination)
				{
					allEliminations += candidate;
				}
			}
			if (GetRank0Eliminations(in logic, conclusions, permutations) == allEliminations)
			{
				// All candidates are rank-0 eliminations.
				return truths;
			}
			if (!allEliminations.Contains(elimination))
			{
				// Invalid case - the original pattern cannot remove such candidate.
				var candidateString = Candidate.ToCandidateString(elimination, new RxCyConverter());
				throw new ArgumentException(
					$"The original pattern cannot remove the specified candidate '{candidateString}'.",
					nameof(elimination)
				);
			}

			var truthsArray = truths.ToArray();
			var tempFoundTruthCombinations = default(Space[]);
			bool found;
			do
			{
				found = false;

				// Iterate on each combination of truths.
				var targetTruths = tempFoundTruthCombinations ?? truthsArray;
				foreach (var truthCombination in targetTruths & targetTruths.Length - 1)
				{
					var sublogic = new Logic([.. truthCombination], logic.Links, logic.Grid);
					var sublogicConclusions = GetConclusions(in sublogic, GetPermutations(in sublogic), true, false);
					if (sublogicConclusions.Contains(new(Elimination, elimination)))
					{
						// If the pattern (with lower-sized truths) can delete such candidate,
						// we should continue to check, until the candidate cannot be eliminiated.
						tempFoundTruthCombinations = truthCombination;
						found = true;
						break;
					}
				}
			} while (found);

			// Otherwise, we cannot find any subpatterns that can remove that candidate - all truths are necessary.
			return tempFoundTruthCombinations?.AsSpaceSet() ?? truths;
		}

		/// <inheritdoc cref="LogicReasoner.GetMinimalPattern(ref readonly Logic, Candidate)"/>
		public static Logic GetMinimalPattern(ref readonly Logic logic, Candidate elimination, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			var sublogic = new Logic(GetMinimalTruths(in logic, elimination, conclusions, permutations), logic.Links, logic.Grid);
			return TrimExcessLinks(
				in sublogic,
				[new(Elimination, elimination)],
				sublogic == logic ? permutations : GetPermutations(in sublogic)
			);
		}

		/// <inheritdoc cref="LogicReasoner.TrimExcessLinks(ref readonly Logic)"/>
		public static Logic TrimExcessLinks(ref readonly Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			//if (!conclusions)
			//{
			//	// This logic doesn't produce any possible conclusions available.
			//	return logic;
			//}

			// Optimization: If n(truths) == n(links), we cannot remove any possible links because it is already a minimal case.
			if (logic.Truths.Count == logic.Links.Count)
			{
				return logic;
			}

			// Just remove the link and find conclusions.
			// If we can find all possible conclusions of the pattern if removed,
			// we can know that the link having been removed is redundant.
			var result = logic;
			var tempLogic = logic; // Defensive copy.
			var map = conclusions.Map;
			ref readonly var grid = ref tempLogic.Grid;
			bool isChanged;
			do
			{
				isChanged = false;
				foreach (var link in tempLogic.Links)
				{
					if (link.GetAvailableRange(grid) is var linkMap
						&& (tempLogic.Map & linkMap).Count == 1 && !(linkMap & map)
						|| new Logic(tempLogic.Truths, tempLogic.Links - link, tempLogic.Grid) is var sublogic
						&& GetConclusions(in sublogic, GetPermutations(in sublogic), true, false) == conclusions)
					{
						// We can cut this link out of the pattern if either of conditions mentioned below are satisfied:
						//   1) It only hold one candidate in the pattern (which is not enclosing pattern),
						//      and the link cannot intersect with any conclusions.
						//   2) The subpattern with this link having been removed can also make all conclusions.
						result.RemoveLink(link);
						tempLogic = result;
						isChanged = true;
					}
				}
			} while (isChanged);
			return result;
		}

		/// <inheritdoc cref="LogicReasoner.ConvertTruthsToLinks(ref readonly Logic)"/>
		public static Logic ConvertTruthsToLinks(ref readonly Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			//if (!conclusions)
			//{
			//	// This logic doesn't produce any possible conclusions available.
			//	return logic;
			//}

			// Iterate truths and convert them into links, and check whether the pattern can also eliminate such candidates or not.
			var result = logic;
			var tempLogic = logic; // Defensive copy.
			bool isChanged;
			do
			{
				isChanged = false;
				foreach (var truth in tempLogic.Truths)
				{
					if (new Logic(tempLogic.Truths - truth, tempLogic.Links + truth, tempLogic.Grid) is var sublogic
						&& GetConclusions(in sublogic, GetPermutations(in sublogic), true, false) == conclusions)
					{
						result.RemoveTruth(truth);
						result.AddLink(truth);
						tempLogic = result;
						isChanged = true;
					}
				}
			} while (isChanged);
			return result;
		}
	}
}
