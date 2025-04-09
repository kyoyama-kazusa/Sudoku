namespace Sudoku.Solving.Bitwise;

/// <summary>
/// Indicates the solver that is able to solve a sudoku puzzle, and then get the solution of that sudoku.
/// </summary>
/// <remarks>
/// <para>
/// The reason why the type name contains the word <i>bitwise</i> is that the solver uses the bitwise algorithm
/// to handle a sudoku grid, which is more efficient.
/// </para>
/// <para><b>
/// This type is thread-unsafe. If you want to use this type in multi-threading, please use <see langword="lock"/> statement.
/// </b></para>
/// </remarks>
public sealed unsafe partial class BitwiseSolver : ISolver, ISolutionEnumerableSolver
{
	/// <summary>
	/// Stack to store current and previous states.
	/// </summary>
	private readonly BitwiseSolverState[] _stack = new BitwiseSolverState[50];


	/// <summary>
	/// Nasty global flag telling if <see cref="ApplySingleOrEmptyCells"/> found anything.
	/// </summary>
	/// <seealso cref="ApplySingleOrEmptyCells"/>
	private bool _singleApplied;

	/// <summary>
	/// Pointer to where to store the first solution. This value can be <see langword="null"/>.
	/// </summary>
	private char* _solution;

	/// <summary>
	/// Indicates the number of solutions found so far.
	/// </summary>
	private long _numSolutions;

	/// <summary>
	/// Indicates the number of solutions you want to search.
	/// Assign <see cref="int.MaxValue"/> if you want to find all possible solutions,
	/// and assign 2 if you only want to check validity of the puzzle (if 2 solutions found, the puzzle will become invalid).
	/// </summary>
	private long _limitSolutions;

	/// <summary>
	/// Pointer to the currently active slot.
	/// </summary>
	private BitwiseSolverState* _g;


	/// <inheritdoc/>
	public string? UriLink => null;


	/// <inheritdoc/>
	public event EventHandler<SolverSolutionFoundEventArgs>? SolutionFound;


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool? Solve(in Grid grid, out Grid result)
	{
		ClearStack();

		var puzzleStr = grid.ToString();
		var solutionStr = stackalloc char[BufferLength];
		long solutions;
		fixed (char* pPuzzleStr = puzzleStr)
		{
			solutions = InternalSolve(pPuzzleStr, solutionStr, 2);
		}

		Unsafe.SkipInit(out result);
		var (_, @return) = solutions switch
		{
			0 => (Grid.Undefined, null),
			1 => (result = Grid.Parse(new ReadOnlySpan<char>(solutionStr, BufferLength)), true),
			_ => (Grid.Undefined, (bool?)false)
		};
		return @return;
	}

	/// <summary>
	/// Solves the puzzle represented as a string value.
	/// </summary>
	/// <param name="puzzle">The puzzle represented as a string.</param>
	/// <param name="solution">The solution.</param>
	/// <param name="limit">The limit of solutions to be checked.</param>
	/// <returns>A <see cref="long"/> value indicating the number of solutions.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long SolveString(char* puzzle, char* solution, int limit)
	{
		ArgumentNullException.ThrowIfNull(puzzle);

		ClearStack();

		var solutionStr = stackalloc char[BufferLength];
		var solutionsCount = InternalSolve(puzzle, solutionStr, limit);
		if (solution != null)
		{
			Unsafe.CopyBlock(solution, solutionStr, sizeof(char) * BufferLength);
		}
		return solutionsCount;
	}

	/// <inheritdoc cref="SolveString(char*, char*, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long SolveString(string puzzle, char* solution, int limit)
	{
		ClearStack();

		fixed (char* p = puzzle)
		{
			var solutionStr = stackalloc char[BufferLength];
			var result = InternalSolve(p, solutionStr, limit);
			if (solution != null)
			{
				Unsafe.CopyBlock(solution, solutionStr, sizeof(char) * BufferLength);
			}
			return result;
		}
	}

	/// <inheritdoc cref="SolveString(char*, char*, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long SolveString(string puzzle, out string solution, int limit)
	{
		ClearStack();

		fixed (char* p = puzzle)
		{
			var solutionStr = stackalloc char[BufferLength];
			var result = InternalSolve(p, solutionStr, limit);
			solution = new(solutionStr);
			return result;
		}
	}

	/// <summary>
	/// Same as <see cref="CheckValidity(string, out string?)"/>, but doesn't contain
	/// any <see langword="out"/> parameters.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>The <see cref="bool"/> result. <see langword="true"/> for unique solution.</returns>
	/// <exception cref="ArgumentNullException">
	/// Throws when the argument <paramref name="grid"/> is <see langword="null"/>.
	/// </exception>
	/// <seealso cref="CheckValidity(string, out string?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckValidity(char* grid)
	{
		ArgumentNullException.ThrowIfNull(grid);
		ClearStack();
		return InternalSolve(grid, null, 2) == 1;
	}

	/// <inheritdoc cref="CheckValidity(char*)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckValidity(ref readonly char grid) => CheckValidity((char*)Unsafe.AsPointer(ref Unsafe.AsRef(in grid)));

	/// <inheritdoc cref="CheckValidity(char*)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckValidity(string grid) => CheckValidity(in grid.AsSpan()[0]);

	/// <summary>
	/// Check the validity of the puzzle.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="solutionIfUnique">The solution if the puzzle is unique.</param>
	/// <returns>The <see cref="bool"/> result. <see langword="true"/> for unique solution.</returns>
	public bool CheckValidity(string grid, [NotNullWhen(true)] out string? solutionIfUnique)
	{
		ClearStack();

		fixed (char* puzzle = grid)
		{
			var result = stackalloc char[BufferLength];
			if (InternalSolve(puzzle, result, 2) == 1)
			{
				solutionIfUnique = @ref.AsSpan(ref *result, BufferLength).ToString();
				return true;
			}

			solutionIfUnique = null;
			return false;
		}
	}

	/// <summary>
	/// To solve the puzzle, and get the solution.
	/// </summary>
	/// <param name="puzzle">The puzzle to solve.</param>
	/// <returns>The solution. If failed to solve, <see cref="Grid.Undefined"/>.</returns>
	/// <seealso cref="Grid.Undefined"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Grid Solve(in Grid puzzle) => Solve(puzzle, out var result) is true ? result : Grid.Undefined;

	/// <inheritdoc/>
	void ISolutionEnumerableSolver.EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken)
		=> SolveString(grid.ToString("0"), null, int.MaxValue);

	/// <summary>
	/// To clear the field <see cref="_stack"/>.
	/// </summary>
	/// <seealso cref="_stack"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ClearStack() => Array.Clear(_stack);

	/// <summary>
	/// Set a cell as solved - used in <see cref="InitSudoku"/>.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SetSolvedDigit(Cell cell, Digit digit)
	{
		var subBand = (int)Cell2Floor[cell];
		var band = Digit2BaseBand[digit] + subBand;
		var mask = Cell2Mask[cell];
		if ((_g->Bands[band] & mask) == 0)
		{
			return false;
		}

		_g->Bands[band] &= SelfMaskTable[cell];
		var tblMask = OtherMaskTable[cell];
		_g->Bands[Another1Table[band]] &= (uint)tblMask;
		_g->Bands[Another2Table[band]] &= (uint)tblMask;
		mask = ~mask;
		_g->UnsolvedCells[subBand] &= (uint)mask;
		var rowBit = digit * 9 + CellToRow[cell];
		_g->UnsolvedRows[rowBit / 27] &= (uint)~(1 << (Mod27[rowBit]));
		_g->Bands[subBand] &= (uint)mask;
		_g->Bands[subBand + 3] &= (uint)mask;
		_g->Bands[subBand + 6] &= (uint)mask;
		_g->Bands[subBand + 9] &= (uint)mask;
		_g->Bands[subBand + 12] &= (uint)mask;
		_g->Bands[subBand + 15] &= (uint)mask;
		_g->Bands[subBand + 18] &= (uint)mask;
		_g->Bands[subBand + 21] &= (uint)mask;
		_g->Bands[subBand + 24] &= (uint)mask;
		_g->Bands[band] |= (uint)~mask;
		return true;
	}

	/// <summary>
	/// Eliminate a digit - used in <see cref="InitSudoku"/>.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool EliminateDigit(Cell cell, Digit digit)
	{
		var subBand = Cell2Floor[cell];
		var band = Digit2BaseBand[digit] + subBand;
		var mask = Cell2Mask[cell];
		if ((_g->Bands[band] & mask) == 0)
		{
			// This candidate has been removed yet.
			return true;
		}

		_g->Bands[band] &= (uint)~mask;
		return true;
	}

	/// <summary>
	/// Set a cell as solved - used in various guess routines.
	/// </summary>
	/// <param name="band">The band.</param>
	/// <param name="mask">The mask.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SetSolvedMask(int band, uint mask)
	{
		if ((_g->Bands[band] & mask) == 0)
		{
			return false;
		}

		var subBand = Mod3[band];
		var cell = subBand * 27 + BitPos(mask);
		_g->Bands[band] &= SelfMaskTable[cell];
		return true;
	}

	/// <summary>
	/// Setup everything and load the puzzle.
	/// </summary>
	/// <param name="puzzle">The pointer that points to a puzzle buffer.</param>
	/// <returns>The <see cref="bool"/> result.</returns>
	private bool InitSudoku(char* puzzle)
	{
		fixed (BitwiseSolverState* g = _stack)
		{
			_numSolutions = 0;
			for (var band = 0; band < 27; band++)
			{
				g->Bands[band] = BitSet27;
			}

			Unsafe.InitBlock(g->PrevBands, 0, 27 * sizeof(uint));
			g->UnsolvedCells[0] = g->UnsolvedCells[1] = g->UnsolvedCells[2] = BitSet27;
			g->UnsolvedRows[0] = g->UnsolvedRows[1] = g->UnsolvedRows[2] = BitSet27;
			g->Pairs[0] = g->Pairs[1] = g->Pairs[2] = 0;
			_g = g;
		}

		switch (StringLengthOf(puzzle))
		{
			case 81:
			{
				for (var cell = 0; cell < 81; cell++, puzzle++)
				{
					if (*puzzle is > '0' and <= '9')
					{
						if (!SetSolvedDigit(cell, *puzzle - '1'))
						{
							return false;
						}
					}
					else if (*puzzle == 0)
					{
						// End of string before end of puzzle!
						return false;
					}
				}
				return true;
			}
			case 729:
			{
				for (var cell = 0; cell < 81; cell++)
				{
					var mask = (Mask)0;
					for (var digit = 0; digit < 9; digit++, puzzle++)
					{
						if (*puzzle == '0')
						{
							mask |= (Mask)(1 << digit);
						}
					}

					for (var (digit, temp) = (0, mask); digit < 9; digit++, temp >>= 1)
					{
						if ((temp & 1) != 0 && !EliminateDigit(cell, digit))
						{
							return false;
						}
					}
				}
				return true;
			}
			default:
			{
				return false;
			}
		}
	}

	/// <summary>
	/// Core of fast processing.
	/// </summary>
	/// <returns>The <see cref="bool"/> value.</returns>
	private bool Update()
	{
		uint shrink = 1, a, b, c, cl;
		while (shrink != 0)
		{
			uint s;
			shrink = 0;
			if (_g->UnsolvedRows[0] == 0) goto Digit3;
			{
				var ar = _g->UnsolvedRows[0];  // valid for Digits 0,1,2
				if ((ar & 0x1FF) == 0) goto Digit1;
				if (_g->Bands[0 * 3 + 0] == _g->PrevBands[0 * 3 + 0]) goto Digit0b;
				if (!updn(&s, 0, 0, 1, 2)) return false;
				if ((ar & 7) != *&s)
				{
					ar &= 0x7FFFFF8 | s;
					upwcl(s, 0, 3, 6, 9, 12, 15, 18, 21, 24, a);
				}
			Digit0b:
				if (_g->Bands[0 * 3 + 1] == _g->PrevBands[0 * 3 + 1]) goto Digit0c;
				if (!updn(&s, 0, 1, 0, 2)) return false;
				if (((ar >> 3) & 7) != *&s)
				{
					ar &= 0x7FFFFC7 | (s << 3);
					upwcl(s, 1, 4, 7, 10, 13, 16, 19, 22, 25, a);
				}
			Digit0c:
				if (_g->Bands[0 * 3 + 2] == _g->PrevBands[0 * 3 + 2]) goto Digit1;
				if (!updn(&s, 0, 2, 0, 1)) return false;
				if (((ar >> 6) & 7) != *&s)
				{
					ar &= 0x7FFFE3F | (s << 6);
					upwcl(s, 2, 5, 8, 11, 14, 17, 20, 23, 26, a);
				}
			Digit1:
				if (((ar >> 9) & 0x1FF) == 0) goto Digit2;
				if (_g->Bands[1 * 3 + 0] == _g->PrevBands[1 * 3 + 0]) goto Digit1b;
				if (!updn(&s, 1, 0, 1, 2)) return false;
				if (((ar >> 9) & 7) != *&s)
				{
					ar &= 0x7FFF1FF | (s << 9);
					upwcl(s, 0, 0, 6, 9, 12, 15, 18, 21, 24, a);
				}
			Digit1b:
				if (_g->Bands[1 * 3 + 1] == _g->PrevBands[1 * 3 + 1]) goto Digit1c;
				if (!updn(&s, 1, 1, 0, 2)) return false;
				if (((ar >> 12) & 7) != *&s)
				{
					ar &= 0x7FF8FFF | (s << 12);
					upwcl(s, 1, 1, 7, 10, 13, 16, 19, 22, 25, a);
				}
			Digit1c:
				if (_g->Bands[1 * 3 + 2] == _g->PrevBands[1 * 3 + 2]) goto Digit2;
				if (!updn(&s, 1, 2, 0, 1)) return false;
				if (((ar >> 15) & 7) != *&s)
				{
					ar &= 0x7FC7FFF | (s << 15);
					upwcl(s, 2, 2, 8, 11, 14, 17, 20, 23, 26, a);
				}
			Digit2:
				if (((ar >> 18) & 0x1FF) == 0) goto End012;
				if (_g->Bands[2 * 3 + 0] == _g->PrevBands[2 * 3 + 0]) goto Digit2b;
				if (!updn(&s, 2, 0, 1, 2)) return false;
				if (((ar >> 18) & 7) != *&s)
				{
					ar &= 0x7E3FFFF | (s << 18);
					upwcl(s, 0, 0, 3, 9, 12, 15, 18, 21, 24, a);
				}
			Digit2b:
				if (_g->Bands[2 * 3 + 1] == _g->PrevBands[2 * 3 + 1]) goto Digit2c;
				if (!updn(&s, 2, 1, 0, 2)) return false;
				if (((ar >> 21) & 7) != *&s)
				{
					ar &= 0x71FFFFF | (s << 21);
					upwcl(s, 1, 1, 4, 10, 13, 16, 19, 22, 25, a);
				}
			Digit2c:
				if (_g->Bands[2 * 3 + 2] == _g->PrevBands[2 * 3 + 2]) goto End012;
				if (!updn(&s, 2, 2, 0, 1)) return false;
				if (((ar >> 24) & 7) != *&s)
				{
					ar &= 0xFFFFFF | (s << 24);
					upwcl(s, 2, 2, 5, 11, 14, 17, 20, 23, 26, a);
				}
			End012:
				_g->UnsolvedRows[0] = ar;
			}
		Digit3:
			if (_g->UnsolvedRows[1] == 0) goto Digit6;
			{
				var ar = _g->UnsolvedRows[1];  // valid for Digits 3,4,5
				if ((ar & 0x1FF) == 0) goto Digit4;
				if (_g->Bands[3 * 3 + 0] == _g->PrevBands[3 * 3 + 0]) goto Digit3b;
				if (!updn(&s, 3, 0, 1, 2)) return false;
				if ((ar & 7) != *&s)
				{
					ar &= 0x7FFFFF8 | s;
					upwcl(s, 0, 0, 3, 6, 12, 15, 18, 21, 24, a);
				}
			Digit3b:
				if (_g->Bands[3 * 3 + 1] == _g->PrevBands[3 * 3 + 1]) goto Digit3c;
				if (!updn(&s, 3, 1, 0, 2)) return false;
				if (((ar >> 3) & 7) != *&s)
				{
					ar &= 0x7FFFFC7 | (s << 3);
					upwcl(s, 1, 1, 4, 7, 13, 16, 19, 22, 25, a);
				}
			Digit3c:
				if (_g->Bands[3 * 3 + 2] == _g->PrevBands[3 * 3 + 2]) goto Digit4;
				if (!updn(&s, 3, 2, 0, 1)) return false;
				if (((ar >> 6) & 7) != *&s)
				{
					ar &= 0x7FFFE3F | (s << 6);
					upwcl(s, 2, 2, 5, 8, 14, 17, 20, 23, 26, a);
				}
			Digit4:
				if (((ar >> 9) & 0x1FF) == 0) goto Digit5;
				if (_g->Bands[4 * 3 + 0] == _g->PrevBands[4 * 3 + 0]) goto Digit4b;
				if (!updn(&s, 4, 0, 1, 2)) return false;
				if (((ar >> 9) & 7) != *&s)
				{
					ar &= 0x7FFF1FF | (s << 9);
					upwcl(s, 0, 0, 3, 6, 9, 15, 18, 21, 24, a);
				}
			Digit4b:
				if (_g->Bands[4 * 3 + 1] == _g->PrevBands[4 * 3 + 1]) goto Digit4c;
				if (!updn(&s, 4, 1, 0, 2)) return false;
				if (((ar >> 12) & 7) != *&s)
				{
					ar &= 0x7FF8FFF | (s << 12);
					upwcl(s, 1, 1, 4, 7, 10, 16, 19, 22, 25, a);
				}
			Digit4c:
				if (_g->Bands[4 * 3 + 2] == _g->PrevBands[4 * 3 + 2]) goto Digit5;
				if (!updn(&s, 4, 2, 0, 1)) return false;
				if (((ar >> 15) & 7) != *&s)
				{
					ar &= 0x7FC7FFF | (s << 15);
					upwcl(s, 2, 2, 5, 8, 11, 17, 20, 23, 26, a);
				}
			Digit5:
				if (((ar >> 18) & 0x1FF) == 0) goto End345;
				if (_g->Bands[5 * 3 + 0] == _g->PrevBands[5 * 3 + 0]) goto Digit5b;
				if (!updn(&s, 5, 0, 1, 2)) return false;
				if (((ar >> 18) & 7) != *&s)
				{
					ar &= 0x7E3FFFF | (s << 18);
					upwcl(s, 0, 0, 3, 6, 9, 12, 18, 21, 24, a);
				}
			Digit5b:
				if (_g->Bands[5 * 3 + 1] == _g->PrevBands[5 * 3 + 1]) goto Digit5c;
				if (!updn(&s, 5, 1, 0, 2)) return false;
				if (((ar >> 21) & 7) != *&s)
				{
					ar &= 0x71FFFFF | (s << 21);
					upwcl(s, 1, 1, 4, 7, 10, 13, 19, 22, 25, a);
				}
			Digit5c:
				if (_g->Bands[5 * 3 + 2] == _g->PrevBands[5 * 3 + 2]) goto End345;
				if (!updn(&s, 5, 2, 0, 1)) return false;
				if (((ar >> 24) & 7) != *&s)
				{
					ar &= 0xFFFFFF | (s << 24);
					upwcl(s, 2, 2, 5, 8, 11, 14, 20, 23, 26, a);
				}
			End345:
				_g->UnsolvedRows[1] = ar;
			}
		Digit6:
			if (_g->UnsolvedRows[2] == 0) continue;
			{
				var ar = _g->UnsolvedRows[2];  // valid for Digits 6,7,8
				if ((ar & 0x1FF) == 0) goto Digit7;
				if (_g->Bands[6 * 3 + 0] == _g->PrevBands[6 * 3 + 0]) goto Digit6b;
				if (!updn(&s, 6, 0, 1, 2)) return false;
				if ((ar & 7) != *&s)
				{
					ar &= 0x7FFFFF8 | s;
					upwcl(s, 0, 0, 3, 6, 9, 12, 15, 21, 24, a);
				}
			Digit6b:
				if (_g->Bands[6 * 3 + 1] == _g->PrevBands[6 * 3 + 1]) goto Digit6c;
				if (!updn(&s, 6, 1, 0, 2)) return false;
				if (((ar >> 3) & 7) != *&s)
				{
					ar &= 0x7FFFFC7 | (s << 3);
					upwcl(s, 1, 1, 4, 7, 10, 13, 16, 22, 25, a);
				}
			Digit6c:
				if (_g->Bands[6 * 3 + 2] == _g->PrevBands[6 * 3 + 2]) goto Digit7;
				if (!updn(&s, 6, 2, 0, 1)) return false;
				if (((ar >> 6) & 7) != *&s)
				{
					ar &= 0x7FFFE3F | (s << 6);
					upwcl(s, 2, 2, 5, 8, 11, 14, 17, 23, 26, a);
				}
			Digit7:
				if (((ar >> 9) & 0x1FF) == 0) goto Digit8;
				if (_g->Bands[7 * 3 + 0] == _g->PrevBands[7 * 3 + 0]) goto Digit7b;
				if (!updn(&s, 7, 0, 1, 2)) return false;
				if (((ar >> 9) & 7) != *&s)
				{
					ar &= 0x7FFF1FF | (s << 9);
					upwcl(s, 0, 0, 3, 6, 9, 12, 15, 18, 24, a);
				}
			Digit7b:
				if (_g->Bands[7 * 3 + 1] == _g->PrevBands[7 * 3 + 1]) goto Digit7c;
				if (!updn(&s, 7, 1, 0, 2)) return false;
				if (((ar >> 12) & 7) != *&s)
				{
					ar &= 0x7FF8FFF | (s << 12);
					upwcl(s, 1, 1, 4, 7, 10, 13, 16, 19, 25, a);
				}
			Digit7c:
				if (_g->Bands[7 * 3 + 2] == _g->PrevBands[7 * 3 + 2]) goto Digit8;
				if (!updn(&s, 7, 2, 0, 1)) return false;
				if (((ar >> 15) & 7) != *&s)
				{
					ar &= 0x7FC7FFF | (s << 15);
					upwcl(s, 2, 2, 5, 8, 11, 14, 17, 20, 26, a);
				}
			Digit8:
				if (((ar >> 18) & 0x1FF) == 0) goto End678;
				if (_g->Bands[8 * 3 + 0] == _g->PrevBands[8 * 3 + 0]) goto Digit8b;
				if (!updn(&s, 8, 0, 1, 2)) return false;
				if (((ar >> 18) & 7) != *&s)
				{
					ar &= 0x7E3FFFF | (s << 18);
					upwcl(s, 0, 0, 3, 6, 9, 12, 15, 18, 21, a);
				}
			Digit8b:
				if (_g->Bands[8 * 3 + 1] == _g->PrevBands[8 * 3 + 1]) goto Digit8c;
				if (!updn(&s, 8, 1, 0, 2)) return false;
				if (((ar >> 21) & 7) != *&s)
				{
					ar &= 0x71FFFFF | (s << 21);
					upwcl(s, 1, 1, 4, 7, 10, 13, 16, 19, 22, a);
				}
			Digit8c:
				if (_g->Bands[8 * 3 + 2] == _g->PrevBands[8 * 3 + 2]) goto End678;
				if (!updn(&s, 8, 2, 0, 1)) return false;
				if (((ar >> 24) & 7) != *&s)
				{
					ar &= 0xFFFFFF | (s << 24);
					upwcl(s, 2, 2, 5, 8, 11, 14, 17, 20, 23, a);
				}
			End678:
				_g->UnsolvedRows[2] = ar;
			}
		}
		return true;


		// The core Update routine from Zhou Yundong.
		// This copy has been optimized by champagne and JasonLion in minor ways.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool updn(uint* s, uint i, uint j, uint k, uint l)
		{
			a = _g->Bands[i * 3 + j];
			shrink = (uint)(ShrinkMaskTable[a & 0x1FF] | ShrinkMaskTable[(a >> 9) & 0x1FF] << 3 | ShrinkMaskTable[a >> 18] << 6);
			if ((a &= (uint)ComplexMaskTable[shrink]) == 0)
			{
				return false;
			}

			b = _g->Bands[i * 3 + k];
			c = _g->Bands[i * 3 + l];
			*s = (a | a >> 9 | a >> 18) & 0x1FF;
			_g->Bands[i * 3 + l] &= (uint)MaskSingleTable[*s];
			_g->Bands[i * 3 + k] &= (uint)MaskSingleTable[*s];
			*s = RowUniqueTable[ShrinkSingleTable[shrink] & ColumnSingleTable[*s]];
			_g->PrevBands[i * 3 + j] = _g->Bands[i * 3 + j] = a;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void upwcl(uint s, uint i, uint p, uint q, uint r, uint t, uint u, uint v, uint w, uint x, uint a)
		{
			cl = ~(a & RowMaskTable[s]);
			_g->UnsolvedCells[i] &= cl;
			_g->Bands[p] &= cl;
			_g->Bands[q] &= cl;
			_g->Bands[r] &= cl;
			_g->Bands[t] &= cl;
			_g->Bands[u] &= cl;
			_g->Bands[v] &= cl;
			_g->Bands[w] &= cl;
			_g->Bands[x] &= cl;
		}
	}

	/// <summary>
	/// Find singles, bi-value cells, and impossible cells.
	/// </summary>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool ApplySingleOrEmptyCells()
	{
		_singleApplied = false;
		for (var subBand = 0; subBand < 3; subBand++)
		{
			// Loop unrolling really helps.
			var r1 = _g->Bands[subBand];           // r1 - Cells in band with pencil one or more times.
			var bandData = _g->Bands[subBand + 3]; // bandData - Hint to save value in register.
			var r2 = r1 & bandData;                // r2 - Pencil mark in cell two or more times.
			r1 |= bandData;
			bandData = _g->Bands[subBand + 6];
			var r3 = r2 & bandData;                // r3 - Pencil mark in cell three or more times.
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 9];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 12];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 15];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 18];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 21];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;
			bandData = _g->Bands[subBand + 24];
			r3 |= r2 & bandData;
			r2 |= r1 & bandData;
			r1 |= bandData;

			if (r1 != BitSet27)
			{
				// Something is locked, can't be solved.
				return true;
			}

			_g->Pairs[subBand] = r2 & ~r3;          // Exactly two pencil marks in cell.
			r1 &= ~r2;                              // Exactly one pencil mark in cell.
			r1 &= _g->UnsolvedCells[subBand];       // Ignore already solved cells.
			while (r1 != 0)
			{
				// Set all the single pencil mark cells
				_singleApplied = true;
				var bit = r1 & (uint)-(int)r1;     // Process once cell at a time.
				r1 ^= bit;
				Digit digit;
				for (digit = 0; digit < 9; digit++)
				{
					// Requires finding for which digit they are.
					if ((_g->Bands[digit * 3 + subBand] & bit) != 0)
					{
						SetSolvedMask(digit * 3 + subBand, bit);
						break;
					}
				}
				if (digit == 9)
				{
					// Previous singles locked the cell.
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// The internal solving method.
	/// </summary>
	/// <param name="puzzle">The pointer to the puzzle string.</param>
	/// <param name="solutionPtr">The pointer to the solution string.</param>
	/// <param name="limit">The limitation for the number of all final solutions.</param>
	/// <returns>The number of solutions found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private long InternalSolve(char* puzzle, char* solutionPtr, int limit)
	{
		_numSolutions = 0;
		_limitSolutions = limit;
		_solution = solutionPtr;

		if (!InitSudoku(puzzle))
		{
			return 0;
		}

		if (ApplySingleOrEmptyCells())
		{
			// Locked empty cell or conflict singles in cells.
			return 0;
		}

		if (FullUpdate() == 0)
		{
			return 0;
		}

		Guess();

		return _numSolutions;
	}

	/// <summary>
	/// Extract solution as a string.
	/// </summary>
	/// <param name="solution">
	/// The solution pointer. <b>The buffer should be at least <see cref="BufferLength"/>
	/// of value of length.</b>
	/// </param>
	private void ExtractSolution(char* solution)
	{
		if (_limitSolutions == int.MaxValue)
		{
			var solutionSpan = (stackalloc char[BufferLength]);
			f(solutionSpan);
			SolutionFound?.Invoke(this, new(Grid.Parse(solutionSpan)));
			return;
		}

		if (_solution != null && _numSolutions == 0)
		{
			f(new(solution, BufferLength));
		}


		void f(Span<char> span)
		{
			for (var cell = 0; cell < 81; cell++)
			{
				var mask = Cell2Mask[cell];
				var offset = (int)Cell2Floor[cell];
				for (var digit = 0; digit < 9; digit++)
				{
					if ((_g->Bands[offset] & mask) != 0)
					{
						span[cell] = (char)('1' + digit);
						break;
					}
					offset += 3;
				}
			}
			span[81] = '\0';
		}
	}

	/// <summary>
	/// Try both options for cells with exactly two pencil marks.
	/// </summary>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool GuessBiValueInCell()
	{
		// Uses pairs map, set in ApplySingleOrEmptyCells
		for (var subBand = 0; subBand < 3; subBand++)
		{
			var map = _g->Pairs[subBand];
			if (map != 0)
			{
				map &= (uint)-(int)map;
				var tries = 2;
				var band = subBand;
				for (var digit = 0; digit < 9; digit++, band += 3)
				{
					if ((_g->Bands[band] & map) != 0)
					{
						if (--tries != 0)
						{
							// First of pair.
							Unsafe.CopyBlock(_g + 1, _g, (uint)sizeof(BitwiseSolverState));
							_g->Bands[band] ^= map;
							_g++;
							SetSolvedMask(band, map);
							if (FullUpdate() != 0)
							{
								Guess();
							}

							_g--;
						}
						else
						{
							// Second of pair.
							SetSolvedMask(band, map);
							if (FullUpdate() != 0)
							{
								Guess();
							}

							return true;
						}
					}
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Guess all possibilities in first unsolved cell.
	/// </summary>
	private void GuessFirstCell()
	{
		// Kind of dumb, but _way_ fast code.
		for (var subBand = 0; subBand < 3; subBand++)
		{
			if (_g->UnsolvedCells[subBand] == 0)
			{
				continue;
			}

			var cellMask = _g->UnsolvedCells[subBand];
			cellMask &= (uint)-(int)cellMask;
			var band = subBand;
			for (var digit = 0; digit < 9; digit++, band += 3)
			{
				if ((_g->Bands[band] & cellMask) != 0)
				{
					// Eliminate option in the current stack entry.
					Unsafe.CopyBlock(_g + 1, _g, (uint)sizeof(BitwiseSolverState));
					_g->Bands[band] ^= cellMask;
					_g++;
					SetSolvedMask(band, cellMask); // And try it out in a nested stack entry.
					if (FullUpdate() != 0)
					{
						Guess();
					}

					_g--;
				}
			}
			return;
		}
	}

	/// <summary>
	/// Either already solved, or guess and recurse.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Guess()
	{
		if ((_g->UnsolvedRows[0] | _g->UnsolvedRows[1] | _g->UnsolvedRows[2]) == 0)
		{
			// Already solved.
			ExtractSolution(_solution);

			_numSolutions++;
			return;
		}

		if (!GuessBiValueInCell())
		{
			// Both of these recursions.
			GuessFirstCell();
		}
	}

	/// <summary>
	/// Get as far as possible without guessing.
	/// </summary>
	/// <returns>A <see cref="byte"/> result.</returns>
	private byte FullUpdate()
	{
		if (_numSolutions >= _limitSolutions)
		{
			return 0;
		}

		while (true)
		{
			if (!Update())
			{
				// Game locked in update.
				return 0;
			}

			if ((_g->UnsolvedCells[0] | _g->UnsolvedCells[1] | _g->UnsolvedCells[2]) == 0)
			{
				return 2;
			}

			// locked empty cell or conflict singles in cells.
			if (ApplySingleOrEmptyCells())
			{
				return 0;
			}

			// Found a single, run Update again.
			if (_singleApplied)
			{
				continue;
			}

			break;
		}

		return 1;
	}


	/// <summary>
	/// Get the bit position.
	/// </summary>
	/// <param name="map">The map.</param>
	/// <returns>The position.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte BitPos(uint map) => MultiplyDeBruijnBitPosition32[map * 0x077CB531U >> 27];

	/// <summary>
	/// Get the length of the specified string which is represented by a <see cref="char"/>*.
	/// </summary>
	/// <param name="ptr">The pointer.</param>
	/// <returns>The total length.</returns>
	/// <exception cref="ArgumentNullException">
	/// Throws when the argument <paramref name="ptr"/> is <see langword="null"/>.
	/// </exception>
	/// <remarks>
	/// In C#, this function is unsafe because the implementation of
	/// <see cref="string"/> types between C and C# is totally different.
	/// In C, <see cref="string"/> is like a <see cref="char"/>* or a
	/// <see cref="char"/>[], they ends with the terminator symbol <c>'\0'</c>.
	/// However, C# not.
	/// </remarks>
	private static int StringLengthOf(char* ptr)
	{
		ArgumentNullException.ThrowIfNull(ptr);

		var result = 0;
		for (var p = ptr; *p != '\0'; p++)
		{
			result++;
		}
		return result;
	}
}
