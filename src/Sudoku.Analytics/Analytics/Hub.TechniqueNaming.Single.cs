namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class TechniqueNaming
	{
		/// <summary>
		/// Represents naming rules for single techniques.
		/// </summary>
		public static class Single
		{
			/// <summary>
			/// Get subtype of the hidden single.
			/// </summary>
			/// <param name="grid">The grid.</param>
			/// <param name="cell">The cell.</param>
			/// <param name="house">Indicates the house.</param>
			/// <param name="chosenCells">The chosen cells.</param>
			/// <returns>The subtype of the hidden single.</returns>
			public static SingleSubtype GetHiddenSingleSubtype(in Grid grid, Cell cell, House house, in CellMap chosenCells)
			{
				ref readonly var houseCells = ref HousesMap[house];
				var (b, r, c) = (0, 0, 0);
				foreach (var chosenCell in chosenCells)
				{
					foreach (var houseType in HouseTypes)
					{
						if (HousesMap[chosenCell.ToHouse(houseType)] & houseCells)
						{
							(houseType == HouseType.Block ? ref b : ref houseType == HouseType.Row ? ref r : ref c)++;
							break;
						}
					}
				}

				return SingleSubtype.Parse(
					house switch
					{
						>= 0 and < 9 => $"{HouseType.Block}HiddenSingle0{r}{c}",
						>= 9 and < 18 => $"{HouseType.Row}HiddenSingle{b}0{c}",
						>= 18 and < 27 => $"{HouseType.Column}HiddenSingle{b}{r}0"
					}
				);
			}

			/// <summary>
			/// Get subtype of the naked single.
			/// </summary>
			/// <param name="grid">The grid.</param>
			/// <param name="cell">The cell.</param>
			/// <returns>The subtype of the naked single.</returns>
			public static SingleSubtype GetNakedSingleSubtype(in Grid grid, Cell cell)
			{
				var (valuesCountInBlock, valuesCountInRow, valuesCountInColumn) = (0, 0, 0);
				foreach (var houseType in HouseTypes)
				{
					foreach (var c in HousesMap[cell.ToHouse(houseType)])
					{
						if (grid.GetState(c) != CellState.Empty)
						{
							(
								houseType == HouseType.Block
									? ref valuesCountInBlock
									: ref houseType == HouseType.Row ? ref valuesCountInRow : ref valuesCountInColumn
							)++;
						}
					}
				}
				var maxValue = Math.Max(valuesCountInBlock, valuesCountInRow, valuesCountInColumn);
				return SingleSubtype.Parse(
					maxValue == valuesCountInBlock
						? $"NakedSingleBlock{maxValue}"
						: maxValue == valuesCountInRow ? $"NakedSingleRow{maxValue}" : $"NakedSingleColumn{maxValue}"
				);
			}
		}
	}
}
