namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class TechniqueNaming
	{
		/// <summary>
		/// Represents chain naming rules.
		/// </summary>
		public static class Chain
		{
			/// <summary>
			/// Indicates the sort key dictionary.
			/// </summary>
			private static readonly Dictionary<Technique, int> SortKeyDictionary = new()
			{
				// 3-chain
				{ Technique.Skyscraper, 1 },
				{ Technique.TwoStringKite, 2 },
				{ Technique.TurbotFish, 3 },
				{ Technique.GroupedSkyscraper, 4 },
				{ Technique.GroupedTwoStringKite, 5 },
				{ Technique.GroupedTurbotFish, 6 },

				// 5-chain
				{ Technique.WWing, 101 },
				{ Technique.MWing, 102 },
				{ Technique.SWing, 103 },
				{ Technique.LWing, 104 },
				{ Technique.HWing, 105 },
				{ Technique.PurpleCow, 106 },
				{ Technique.GroupedWWing, 107 },
				{ Technique.GroupedMWing, 108 },
				{ Technique.GroupedSWing, 109 },
				{ Technique.GroupedLWing, 110 },
				{ Technique.GroupedHWing, 111 },
				{ Technique.GroupedPurpleCow, 112 },

				// Others
				{ Technique.FishyCycle, 201 },
				{ Technique.XyCycle, 202 },
				{ Technique.ContinuousNiceLoop, 203 },
				{ Technique.XChain, 204 },
				{ Technique.XyChain, 205 },
				{ Technique.XyXChain, 206 },
				{ Technique.SelfConstraint, 207 },
				{ Technique.AlternatingInferenceChain, 208 },
				{ Technique.DiscontinuousNiceLoop, 209 },
				{ Technique.GroupedFishyCycle, 211 },
				{ Technique.GroupedXyCycle, 212 },
				{ Technique.GroupedContinuousNiceLoop, 213 },
				{ Technique.GroupedXChain, 214 },
				{ Technique.GroupedXyChain, 215 },
				{ Technique.GroupedXyXChain, 216 },
				{ Technique.GroupedSelfConstraint, 217 },
				{ Technique.NodeCollision, 218 },
				{ Technique.GroupedAlternatingInferenceChain, 219 },
				{ Technique.GroupedDiscontinuousNiceLoop, 220 },
				{ Technique.SinglyLinkedAlmostLockedSetsXzRule, 221 },
				{ Technique.DoublyLinkedAlmostLockedSetsXzRule, 222 },
				{ Technique.AlmostLockedSetsXyWing, 223 },
				{ Technique.AlmostLockedSetsWWing, 224 },
				{ Technique.GroupedAlmostLockedSetsWWing, 225 },
				{ Technique.AlmostLockedSetsChain, 226 }
			};


			/// <summary>
			/// Try to get sort key of the pattern.
			/// </summary>
			/// <param name="chain">The chain to be checked.</param>
			/// <param name="conclusions">Indicates conclusions to be used.</param>
			/// <returns>The pattern sort key.</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static int GetSortKey(NamedChain chain, ConclusionSet conclusions)
				=> SortKeyDictionary[GetTechnique(chain, conclusions)];

			/// <summary>
			/// Try to categorize the pattern and return an equivalent <see cref="Technique"/> field representing such patterns.
			/// </summary>
			/// <param name="chain">The chain to be checked.</param>
			/// <param name="grid">The grid to calculate on conclusions for the pattern.</param>
			/// <returns>The <see cref="Technique"/> field categorized.</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Technique GetTechnique(NamedChain chain, in Grid grid)
				=> GetTechnique(chain, chain.GetConclusions(grid));

			/// <summary>
			/// Try to categorize the pattern and return an equivalent <see cref="Technique"/> field representing such patterns.
			/// </summary>
			/// <param name="chain">The chain to be checked.</param>
			/// <param name="conclusions">The conclusions.</param>
			/// <returns>The <see cref="Technique"/> field categorized.</returns>
			public static Technique GetTechnique(NamedChain chain, ConclusionSet conclusions)
				=> chain switch
				{
					AlternatingInferenceChain { IsX: var isX, IsStrictlyGrouped: var isGrouped, Links: var links } instance => instance switch
					{
						{ IsOverlapped: true } and [{ Map: var map1 }, .., { Map: var map2 }] when map1 == map2
							=> isGrouped ? Technique.GroupedSelfConstraint : Technique.SelfConstraint,
						{ IsAlmostLockedSetWWing: true, IsStrongLinksStrictlyGrouped: var isStrictlyGrouped }
							=> isStrictlyGrouped ? Technique.GroupedAlmostLockedSetsWWing : Technique.AlmostLockedSetsWWing,
						{ IsAlmostLockedSetSequence: true, AlmostLockedSetsCount: var count and >= 2 } => count switch
						{
							2 => Technique.SinglyLinkedAlmostLockedSetsXzRule,
							3 => Technique.AlmostLockedSetsXyWing,
							_ => Technique.AlmostLockedSetsChain
						},
						{ IsX: true } => isGrouped ? Technique.GroupedXChain : Technique.XChain,
						{ IsY: true } => isGrouped ? Technique.GroupedXyChain : Technique.XyChain,
						{ IsOverlapped: true } => Technique.NodeCollision,
#if false
						{ IsWoodsWing: true } => isGrouped ? Technique.GroupedWWing : Technique.WWing,
						{ IsMedusaWing: true } => isGrouped ? Technique.GroupedMWing : Technique.MWing,
#endif
						{ IsSplitWing: true } => isGrouped ? Technique.GroupedSWing : Technique.SWing,
						{ IsLocalWing: true } => isGrouped ? Technique.GroupedLWing : Technique.LWing,
						{ IsHybridWing: true } => isGrouped ? Technique.GroupedHWing : Technique.HWing,
						{ Links.Length: 5, IsWoodsWing: false, IsMedusaWing: false }
							=> isGrouped ? Technique.GroupedPurpleCow : Technique.PurpleCow,
						{ Links.Length: 3 } when isX => links switch
						{
#pragma warning disable format
							[
								{ FirstNode.Map.Cells: var cells11, SecondNode.Map.Cells: var cells12 },
								_,
								{ FirstNode.Map.Cells: var cells21, SecondNode.Map.Cells: var cells22 }
							] => (
								HouseMask.TrailingZeroCount((cells11 | cells12).SharedHouses).ToHouseType(),
								HouseMask.TrailingZeroCount((cells21 | cells22).SharedHouses).ToHouseType()
							) switch
							{
								(HouseType.Block, _) or (_, HouseType.Block)
									=> isGrouped ? Technique.GroupedTurbotFish : Technique.TurbotFish,
								(HouseType.Row, HouseType.Column) or (HouseType.Column, HouseType.Row)
									=> isGrouped ? Technique.GroupedTwoStringKite : Technique.TwoStringKite,
								_ => isGrouped ? Technique.GroupedSkyscraper : Technique.Skyscraper
							}
#pragma warning restore format
						},
						[{ Map.Digits: var digits1 }, .., { Map.Digits: var digits2 }]
							=> digits1 == digits2
								? isGrouped ? Technique.GroupedAlternatingInferenceChain : Technique.AlternatingInferenceChain
								: conclusions.Count switch
								{
									1 => isGrouped ? Technique.GroupedDiscontinuousNiceLoop : Technique.DiscontinuousNiceLoop,
									_ => isGrouped ? Technique.GroupedXyXChain : Technique.XyXChain
								}
					},
					ContinuousNiceLoop { IsX: var isX, IsY: var isY, IsStrictlyGrouped: var isGrouped } instance => instance switch
					{
						{ IsAlmostLockedSetSequence: true, AlmostLockedSetsCount: 2 } => Technique.DoublyLinkedAlmostLockedSetsXzRule,
						_ => (isX, isY) switch
						{
							(true, false) => isGrouped ? Technique.GroupedFishyCycle : Technique.FishyCycle,
							(false, true) => isGrouped ? Technique.GroupedXyCycle : Technique.XyCycle,
							_ => isGrouped ? Technique.GroupedContinuousNiceLoop : Technique.ContinuousNiceLoop
						}
					}
				};
		}
	}
}
