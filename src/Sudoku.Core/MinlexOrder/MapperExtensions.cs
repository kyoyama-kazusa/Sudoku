namespace Sudoku.MinlexOrder;

/// <summary>
/// Provides with extension methods on <see cref="Mapper"/> instances.
/// </summary>
/// <seealso cref="Mapper"/>
internal static class MapperExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="mapper">The current mapper instance.</param>
	extension(in Mapper mapper)
	{
		/// <summary>
		/// Converts <see cref="Mapper"/> and <see cref="MinlexCandidate"/> into a readable <see cref="Transform"/> instance.
		/// </summary>
		/// <seealso cref="MinlexCandidate"/>
		public Transform FromMapper(in MinlexCandidate cand)
		{
			// Use MapRowsBackward (canonicalRow -> sourceRow).
			var rows = new RowIndex[9];
			Array.Fill(rows, -1);
			for (var r = 0; r < 9; r++)
			{
				rows[r] = unsafe (cand.MapRowsBackward[r]) is var sr and >= 0 ? sr : -1;
			}

			// Scan map.Cell to infer columns (and cross-check rows if candidate inconsistent).
			var columns = new ColumnIndex[9];
			Array.Fill(columns, -1);
			var transpose = cand.IsTransposed != 0;
			for (var s = 0; s < 81; s++)
			{
				var canon = unsafe (mapper.Cell[s]);
				if (canon >= MinlexFinder.EmptyCellPlaceholder)
				{
					continue; // Masked / Non-given.
				}
				var canonicalRow = canon / 9;
				var canonicalCol = canon % 9;
				var sourceRow = s / 9;
				var sourceColumn = s % 9;

				// If not transposed: source column -> canonical column.
				if (!transpose)
				{
					// Fill column mapping if not already set.
					if (columns[canonicalCol] == -1)
					{
						columns[canonicalCol] = sourceColumn;
					}
				}
				else
				{
					// Transposed: source row acts as column.
					if (columns[canonicalCol] == -1)
					{
						columns[canonicalCol] = sourceRow;
					}
				}

				// (Optional) sanity: if rows[] not set from candidate, infer from cell.
				if (rows[canonicalRow] == -1)
				{
					rows[canonicalRow] = transpose ? sourceColumn : sourceRow;
				}
			}

			// map.Label[canonicalLabel] = original digit.
			var digits = (stackalloc Digit[10]);
			for (var i = 0; i < 10; i++)
			{
				digits[i] = unsafe (mapper.Label[i]) - 1;
			}

			return new(
				transpose ? 1 : 0,
				CantorExpansion.RankRelabeledLines(rows),
				CantorExpansion.RankRelabeledLines(columns),
				CantorExpansion.RankRelabeledDigits(digits[1..])
			);
		}

		/// <summary>
		/// Converts <see cref="Mapper"/> into <see cref="Transform"/>, without using <see cref="MinlexCandidate"/>.
		/// This method will do a heuristics search, trying to infer target grid with ordering of "fitness" of transformations.
		/// </summary>
		public Transform FromMapperInfer()
		{
			// Quick heuristics: count how many cell mappings are consistent with "no-transpose".
			var score_NotTransposed = 0;
			var score_Transposed = 0;
			var rows_NotTransposed = new RowIndex[9];
			var columns_NotTransposed = new ColumnIndex[9];
			var rows_Transposed = new RowIndex[9];
			var columns_Transposed = new ColumnIndex[9];
			Array.Fill(rows_NotTransposed, -1);
			Array.Fill(columns_NotTransposed, -1);
			Array.Fill(rows_Transposed, -1);
			Array.Fill(columns_Transposed, -1);

			for (var cell = 0; cell < 81; cell++)
			{
				var canonicalCell = unsafe (mapper.Cell[cell]);
				if (canonicalCell >= MinlexFinder.EmptyCellPlaceholder)
				{
					continue;
				}

				var canonicalRow = canonicalCell / 9;
				var canonicalColumn = canonicalCell % 9;
				var sourceRow = cell / 9;
				var sourceColumn = cell % 9;

				// Non-transpose interpretation.
				if (rows_NotTransposed[canonicalRow] == -1 || rows_NotTransposed[canonicalRow] == sourceRow)
				{
					rows_NotTransposed[canonicalRow] = sourceRow;
					score_NotTransposed++;
				}
				if (columns_NotTransposed[canonicalColumn] == -1 || columns_NotTransposed[canonicalColumn] == sourceColumn)
				{
					columns_NotTransposed[canonicalColumn] = sourceColumn;
					score_NotTransposed++;
				}

				// Transpose interpretation.
				if (rows_Transposed[canonicalRow] == -1 || rows_Transposed[canonicalRow] == sourceColumn)
				{
					rows_Transposed[canonicalRow] = sourceColumn;
					score_Transposed++;
				}
				if (columns_Transposed[canonicalColumn] == -1 || columns_Transposed[canonicalColumn] == sourceRow)
				{
					columns_Transposed[canonicalColumn] = sourceRow;
					score_Transposed++;
				}
			}

			var digits = (stackalloc Digit[10]);
			for (var i = 0; i < 10; i++)
			{
				digits[i] = unsafe (mapper.Label[i]) - 1;
			}
			return score_NotTransposed >= score_Transposed
				? new(
					0,
					CantorExpansion.RankRelabeledLines(rows_NotTransposed),
					CantorExpansion.RankRelabeledLines(columns_NotTransposed),
					CantorExpansion.RankRelabeledDigits(digits[1..])
				)
				: new(
					1,
					CantorExpansion.RankRelabeledLines(rows_Transposed),
					CantorExpansion.RankRelabeledLines(columns_Transposed),
					CantorExpansion.RankRelabeledDigits(digits[1..])
				);
		}
	}
}
