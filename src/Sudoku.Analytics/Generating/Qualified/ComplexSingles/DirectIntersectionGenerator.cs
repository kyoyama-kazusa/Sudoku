namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator that supports generating for puzzles that can be solved by only using Direct Intersections.
/// </summary>
public sealed class DirectIntersectionGenerator : ComplexSingleGenerator
{
	/// <inheritdoc/>
	public override TechniqueSet SupportedTechniques
		=> [
			Technique.ComplexFullHouse, Technique.ComplexCrosshatchingBlock, Technique.ComplexCrosshatchingRow,
			Technique.ComplexCrosshatchingColumn, Technique.ComplexNakedSingle
		];

	/// <inheritdoc/>
	protected override InterimCellsCreator InterimCellsCreator
		=> static (in g, s) =>
		{
			var step = (DirectIntersectionStep)s;
			var result = CellMap.Empty;
			switch (step.BasedOn)
			{
				case Technique.FullHouse or Technique.NakedSingle:
				{
					var targetCell = step.Cell;
					var targetDigit = step.Digit;
					var interimDigit = step.InterimDigit;
					for (var digit = 0; digit < 9; digit++)
					{
						if (digit != targetDigit && digit != interimDigit)
						{
							foreach (var cell in PeersMap[targetCell])
							{
								if (g.GetState(cell) != CellState.Empty && g.GetDigit(cell) == digit)
								{
									result.Add(cell);
									break;
								}
							}
						}
					}
					goto default;
				}
				case var basedOn and (Technique.CrosshatchingBlock or Technique.CrosshatchingRow or Technique.CrosshatchingColumn):
				{
					var targetHouse = step.Cell.ToHouse(
						basedOn switch
						{
							Technique.CrosshatchingBlock => HouseType.Block,
							Technique.CrosshatchingRow => HouseType.Row,
							_ => HouseType.Column
						}
					);
					foreach (var cell in HousesMap[targetHouse])
					{
						if (g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					goto default;
				}
				default:
				{
					foreach (var cell in HousesMap[step.IntersectionHouse])
					{
						if (g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					foreach (var node in s.Views![0])
					{
						if (node is CircleViewNode { Cell: var cell } && g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					foreach (var cell in step.IntersectionCells)
					{
						result.Remove(cell);
					}
					break;
				}
			}
			return result;
		};

	/// <inheritdoc/>
	protected override StepFilter StepFilter => static step => step is DirectIntersectionStep;
}
