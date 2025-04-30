namespace Sudoku.Analytics.Construction.Chaining;

/// <summary>
/// Represents a processed view nodes map.
/// </summary>
public sealed class ProcessedViewNodeMap : SortedDictionary<WellKnownColorIdentifierKind, (CellMap Cells, CandidateMap Candidates)>
{
	/// <summary>
	/// Indicates the maximal key in ALS set.
	/// </summary>
	public WellKnownColorIdentifierKind MaxKeyInAlmostLockedSet
	{
		get
		{
			var result = WellKnownColorIdentifierKind.Normal;
			foreach (var key in Keys)
			{
				if (key is >= WellKnownColorIdentifierKind.AlmostLockedSet1 and <= WellKnownColorIdentifierKind.AlmostLockedSet5
					&& key >= result)
				{
					result = key;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the maximal key in rectangle set.
	/// </summary>
	public WellKnownColorIdentifierKind MaxKeyInRectangle
	{
		get
		{
			var result = WellKnownColorIdentifierKind.Normal;
			foreach (var key in Keys)
			{
				if (key is >= WellKnownColorIdentifierKind.Rectangle1 and <= WellKnownColorIdentifierKind.Rectangle3
					&& key >= result)
				{
					result = key;
				}
			}
			return result;
		}
	}


	/// <summary>
	/// Determines whether the specified cell is stored in the table; if so, return the corresponding key.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="identifierKind">The cooresponding identifier kind.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool ContainsCell(Cell cell, out WellKnownColorIdentifierKind identifierKind)
	{
		foreach (var kvp in this)
		{
			ref readonly var cells = ref kvp.ValueRef.Cells;
			if (cells.Contains(cell))
			{
				identifierKind = kvp.Key;
				return true;
			}
		}

		identifierKind = default;
		return false;
	}

	/// <summary>
	/// Determines whether the specified candidate is stored in the table; if so, return the corresponding key.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <param name="identifierKind">The cooresponding identifier kind.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool ContainsCandidate(Candidate candidate, out WellKnownColorIdentifierKind identifierKind)
	{
		foreach (var kvp in this)
		{
			ref readonly var candidates = ref kvp.ValueRef.Candidates;
			if (candidates.Contains(candidate))
			{
				identifierKind = kvp.Key;
				return true;
			}
		}

		identifierKind = default;
		return false;
	}
}
