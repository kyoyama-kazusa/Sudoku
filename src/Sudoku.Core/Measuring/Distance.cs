namespace Sudoku.Measuring;

/// <summary>
/// Represents the methods that calculates for distance.
/// </summary>
/// <param name="p">Indicates the integer part.</param>
/// <param name="q">Indicates the root part.</param>
/// <exception cref="ArgumentOutOfRangeException">Throws when either <paramref name="p"/> or <paramref name="q"/> are less than 1.</exception>
/// <remarks>
/// This type is implemented via irrational numbers logic that only takes a square root.
/// </remarks>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.AllEqualityComparisonOperators)]
public readonly ref partial struct Distance(int p, int q)
{
	/// <summary>
	/// Indicates the default root part specifier.
	/// </summary>
	private const string RootPartSpecifier = "s";


	/// <summary>
	/// The table that displays the minimal and maximal index of cells that makes the distances least or greatest with the specified cell.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The table is grouped with houses. If you want to fetch the smallest or largest distance value from a cell to a house, you can use
	/// <c><see cref="DistanceTable"/>[Cell][House]</c>, where <c>Cell</c> is between 0 and 81, and <c>House</c> is between 0 and 27.
	/// If the cell is inside the house, the value will be <see langword="null"/>.
	/// </para>
	/// <para>
	/// In further, if you want to get the distance from two cells using this table, just call method <see cref="GetDistance(Cell, Cell)"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="GetDistance(Cell, Cell)"/>
	private static readonly (Cell Min, Cell Max)?[][] DistanceTable = [
		[null, (3, 5), (6, 8), (27, 45), null, null, (54, 72), null, null, null, (9, 11), (18, 20), (27, 27), (36, 36), (45, 45), (54, 54), (63, 63), (72, 72), null, (1, 19), (2, 20), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8)],
		[null, (3, 5), (6, 8), (28, 46), null, null, (55, 73), null, null, null, (10, 11), (19, 20), (28, 28), (37, 37), (46, 46), (55, 55), (64, 64), (73, 73), (0, 18), null, (2, 20), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8)],
		[null, (3, 5), (6, 8), (29, 47), null, null, (56, 74), null, null, null, (11, 9), (20, 18), (29, 29), (38, 38), (47, 47), (56, 56), (65, 65), (74, 74), (0, 18), (1, 19), null, (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8)],
		[(2, 0), null, (6, 8), null, (30, 48), null, null, (57, 75), null, null, (12, 14), (21, 23), (30, 30), (39, 39), (48, 48), (57, 57), (66, 66), (75, 75), (0, 0), (1, 1), (2, 2), null, (4, 22), (5, 23), (6, 6), (7, 7), (8, 8)],
		[(2, 0), null, (6, 8), null, (31, 49), null, null, (58, 76), null, null, (13, 14), (22, 23), (31, 31), (40, 40), (49, 49), (58, 58), (67, 67), (76, 76), (0, 0), (1, 1), (2, 2), (3, 21), null, (5, 23), (6, 6), (7, 7), (8, 8)],
		[(2, 0), null, (6, 8), null, (32, 50), null, null, (59, 77), null, null, (14, 12), (23, 21), (32, 32), (41, 41), (50, 50), (59, 59), (68, 68), (77, 77), (0, 0), (1, 1), (2, 2), (3, 21), (4, 22), null, (6, 6), (7, 7), (8, 8)],
		[(2, 0), (5, 3), null, null, null, (33, 51), null, null, (60, 78), null, (15, 17), (24, 26), (33, 33), (42, 42), (51, 51), (60, 60), (69, 69), (78, 78), (0, 0), (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), null, (7, 25), (8, 26)],
		[(2, 0), (5, 3), null, null, null, (34, 52), null, null, (61, 79), null, (16, 17), (25, 26), (34, 34), (43, 43), (52, 52), (61, 61), (70, 70), (79, 79), (0, 0), (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 24), null, (8, 26)],
		[(2, 0), (5, 3), null, null, null, (35, 53), null, null, (62, 80), null, (17, 15), (26, 24), (35, 35), (44, 44), (53, 53), (62, 62), (71, 71), (80, 80), (0, 0), (1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 24), (7, 25), null],
		[null, (12, 14), (15, 17), (27, 45), null, null, (54, 72), null, null, (0, 2), null, (18, 20), (27, 27), (36, 36), (45, 45), (54, 54), (63, 63), (72, 72), null, (10, 19), (11, 20), (12, 12), (13, 13), (14, 14), (15, 15), (16, 16), (17, 17)],
		[null, (12, 14), (15, 17), (28, 46), null, null, (55, 73), null, null, (1, 2), null, (19, 20), (28, 28), (37, 37), (46, 46), (55, 55), (64, 64), (73, 73), (9, 18), null, (11, 20), (12, 12), (13, 13), (14, 14), (15, 15), (16, 16), (17, 17)],
		[null, (12, 14), (15, 17), (29, 47), null, null, (56, 74), null, null, (2, 0), null, (20, 18), (29, 29), (38, 38), (47, 47), (56, 56), (65, 65), (74, 74), (9, 18), (10, 19), null, (12, 12), (13, 13), (14, 14), (15, 15), (16, 16), (17, 17)],
		[(11, 9), null, (15, 17), null, (30, 48), null, null, (57, 75), null, (3, 5), null, (21, 23), (30, 30), (39, 39), (48, 48), (57, 57), (66, 66), (75, 75), (9, 9), (10, 10), (11, 11), null, (13, 22), (14, 23), (15, 15), (16, 16), (17, 17)],
		[(11, 9), null, (15, 17), null, (31, 49), null, null, (58, 76), null, (4, 5), null, (22, 23), (31, 31), (40, 40), (49, 49), (58, 58), (67, 67), (76, 76), (9, 9), (10, 10), (11, 11), (12, 21), null, (14, 23), (15, 15), (16, 16), (17, 17)],
		[(11, 9), null, (15, 17), null, (32, 50), null, null, (59, 77), null, (5, 3), null, (23, 21), (32, 32), (41, 41), (50, 50), (59, 59), (68, 68), (77, 77), (9, 9), (10, 10), (11, 11), (12, 21), (13, 22), null, (15, 15), (16, 16), (17, 17)],
		[(11, 9), (14, 12), null, null, null, (33, 51), null, null, (60, 78), (6, 8), null, (24, 26), (33, 33), (42, 42), (51, 51), (60, 60), (69, 69), (78, 78), (9, 9), (10, 10), (11, 11), (12, 12), (13, 13), (14, 14), null, (16, 25), (17, 26)],
		[(11, 9), (14, 12), null, null, null, (34, 52), null, null, (61, 79), (7, 8), null, (25, 26), (34, 34), (43, 43), (52, 52), (61, 61), (70, 70), (79, 79), (9, 9), (10, 10), (11, 11), (12, 12), (13, 13), (14, 14), (15, 24), null, (17, 26)],
		[(11, 9), (14, 12), null, null, null, (35, 53), null, null, (62, 80), (8, 6), null, (26, 24), (35, 35), (44, 44), (53, 53), (62, 62), (71, 71), (80, 80), (9, 9), (10, 10), (11, 11), (12, 12), (13, 13), (14, 14), (15, 24), (16, 25), null],
		[null, (21, 23), (24, 26), (27, 45), null, null, (54, 72), null, null, (0, 2), (9, 11), null, (27, 27), (36, 36), (45, 45), (54, 54), (63, 63), (72, 72), null, (19, 1), (20, 2), (21, 21), (22, 22), (23, 23), (24, 24), (25, 25), (26, 26)],
		[null, (21, 23), (24, 26), (28, 46), null, null, (55, 73), null, null, (1, 2), (10, 11), null, (28, 28), (37, 37), (46, 46), (55, 55), (64, 64), (73, 73), (18, 0), null, (20, 2), (21, 21), (22, 22), (23, 23), (24, 24), (25, 25), (26, 26)],
		[null, (21, 23), (24, 26), (29, 47), null, null, (56, 74), null, null, (2, 0), (11, 9), null, (29, 29), (38, 38), (47, 47), (56, 56), (65, 65), (74, 74), (18, 0), (19, 1), null, (21, 21), (22, 22), (23, 23), (24, 24), (25, 25), (26, 26)],
		[(20, 18), null, (24, 26), null, (30, 48), null, null, (57, 75), null, (3, 5), (12, 14), null, (30, 30), (39, 39), (48, 48), (57, 57), (66, 66), (75, 75), (18, 18), (19, 19), (20, 20), null, (22, 4), (23, 5), (24, 24), (25, 25), (26, 26)],
		[(20, 18), null, (24, 26), null, (31, 49), null, null, (58, 76), null, (4, 5), (13, 14), null, (31, 31), (40, 40), (49, 49), (58, 58), (67, 67), (76, 76), (18, 18), (19, 19), (20, 20), (21, 3), null, (23, 5), (24, 24), (25, 25), (26, 26)],
		[(20, 18), null, (24, 26), null, (32, 50), null, null, (59, 77), null, (5, 3), (14, 12), null, (32, 32), (41, 41), (50, 50), (59, 59), (68, 68), (77, 77), (18, 18), (19, 19), (20, 20), (21, 3), (22, 4), null, (24, 24), (25, 25), (26, 26)],
		[(20, 18), (23, 21), null, null, null, (33, 51), null, null, (60, 78), (6, 8), (15, 17), null, (33, 33), (42, 42), (51, 51), (60, 60), (69, 69), (78, 78), (18, 18), (19, 19), (20, 20), (21, 21), (22, 22), (23, 23), null, (25, 7), (26, 8)],
		[(20, 18), (23, 21), null, null, null, (34, 52), null, null, (61, 79), (7, 8), (16, 17), null, (34, 34), (43, 43), (52, 52), (61, 61), (70, 70), (79, 79), (18, 18), (19, 19), (20, 20), (21, 21), (22, 22), (23, 23), (24, 6), null, (26, 8)],
		[(20, 18), (23, 21), null, null, null, (35, 53), null, null, (62, 80), (8, 6), (17, 15), null, (35, 35), (44, 44), (53, 53), (62, 62), (71, 71), (80, 80), (18, 18), (19, 19), (20, 20), (21, 21), (22, 22), (23, 23), (24, 6), (25, 7), null],
		[(18, 0), null, null, null, (30, 32), (33, 35), (54, 72), null, null, (0, 0), (9, 9), (18, 18), null, (36, 38), (45, 47), (54, 54), (63, 63), (72, 72), null, (28, 46), (29, 47), (30, 30), (31, 31), (32, 32), (33, 33), (34, 34), (35, 35)],
		[(19, 1), null, null, null, (30, 32), (33, 35), (55, 73), null, null, (1, 1), (10, 10), (19, 19), null, (37, 38), (46, 47), (55, 55), (64, 64), (73, 73), (27, 45), null, (29, 47), (30, 30), (31, 31), (32, 32), (33, 33), (34, 34), (35, 35)],
		[(20, 2), null, null, null, (30, 32), (33, 35), (56, 74), null, null, (2, 2), (11, 11), (20, 20), null, (38, 36), (47, 45), (56, 56), (65, 65), (74, 74), (27, 45), (28, 46), null, (30, 30), (31, 31), (32, 32), (33, 33), (34, 34), (35, 35)],
		[null, (21, 3), null, (29, 27), null, (33, 35), null, (57, 75), null, (3, 3), (12, 12), (21, 21), null, (39, 41), (48, 50), (57, 57), (66, 66), (75, 75), (27, 27), (28, 28), (29, 29), null, (31, 49), (32, 50), (33, 33), (34, 34), (35, 35)],
		[null, (22, 4), null, (29, 27), null, (33, 35), null, (58, 76), null, (4, 4), (13, 13), (22, 22), null, (40, 41), (49, 50), (58, 58), (67, 67), (76, 76), (27, 27), (28, 28), (29, 29), (30, 48), null, (32, 50), (33, 33), (34, 34), (35, 35)],
		[null, (23, 5), null, (29, 27), null, (33, 35), null, (59, 77), null, (5, 5), (14, 14), (23, 23), null, (41, 39), (50, 48), (59, 59), (68, 68), (77, 77), (27, 27), (28, 28), (29, 29), (30, 48), (31, 49), null, (33, 33), (34, 34), (35, 35)],
		[null, null, (24, 6), (29, 27), (32, 30), null, null, null, (60, 78), (6, 6), (15, 15), (24, 24), null, (42, 44), (51, 53), (60, 60), (69, 69), (78, 78), (27, 27), (28, 28), (29, 29), (30, 30), (31, 31), (32, 32), null, (34, 52), (35, 53)],
		[null, null, (25, 7), (29, 27), (32, 30), null, null, null, (61, 79), (7, 7), (16, 16), (25, 25), null, (43, 44), (52, 53), (61, 61), (70, 70), (79, 79), (27, 27), (28, 28), (29, 29), (30, 30), (31, 31), (32, 32), (33, 51), null, (35, 53)],
		[null, null, (26, 8), (29, 27), (32, 30), null, null, null, (62, 80), (8, 8), (17, 17), (26, 26), null, (44, 42), (53, 51), (62, 62), (71, 71), (80, 80), (27, 27), (28, 28), (29, 29), (30, 30), (31, 31), (32, 32), (33, 51), (34, 52), null],
		[(18, 0), null, null, null, (39, 41), (42, 44), (54, 72), null, null, (0, 0), (9, 9), (18, 18), (27, 29), null, (45, 47), (54, 54), (63, 63), (72, 72), null, (37, 46), (38, 47), (39, 39), (40, 40), (41, 41), (42, 42), (43, 43), (44, 44)],
		[(19, 1), null, null, null, (39, 41), (42, 44), (55, 73), null, null, (1, 1), (10, 10), (19, 19), (28, 29), null, (46, 47), (55, 55), (64, 64), (73, 73), (36, 45), null, (38, 47), (39, 39), (40, 40), (41, 41), (42, 42), (43, 43), (44, 44)],
		[(20, 2), null, null, null, (39, 41), (42, 44), (56, 74), null, null, (2, 2), (11, 11), (20, 20), (29, 27), null, (47, 45), (56, 56), (65, 65), (74, 74), (36, 45), (37, 46), null, (39, 39), (40, 40), (41, 41), (42, 42), (43, 43), (44, 44)],
		[null, (21, 3), null, (38, 36), null, (42, 44), null, (57, 75), null, (3, 3), (12, 12), (21, 21), (30, 32), null, (48, 50), (57, 57), (66, 66), (75, 75), (36, 36), (37, 37), (38, 38), null, (40, 49), (41, 50), (42, 42), (43, 43), (44, 44)],
		[null, (22, 4), null, (38, 36), null, (42, 44), null, (58, 76), null, (4, 4), (13, 13), (22, 22), (31, 32), null, (49, 50), (58, 58), (67, 67), (76, 76), (36, 36), (37, 37), (38, 38), (39, 48), null, (41, 50), (42, 42), (43, 43), (44, 44)],
		[null, (23, 5), null, (38, 36), null, (42, 44), null, (59, 77), null, (5, 5), (14, 14), (23, 23), (32, 30), null, (50, 48), (59, 59), (68, 68), (77, 77), (36, 36), (37, 37), (38, 38), (39, 48), (40, 49), null, (42, 42), (43, 43), (44, 44)],
		[null, null, (24, 6), (38, 36), (41, 39), null, null, null, (60, 78), (6, 6), (15, 15), (24, 24), (33, 35), null, (51, 53), (60, 60), (69, 69), (78, 78), (36, 36), (37, 37), (38, 38), (39, 39), (40, 40), (41, 41), null, (43, 52), (44, 53)],
		[null, null, (25, 7), (38, 36), (41, 39), null, null, null, (61, 79), (7, 7), (16, 16), (25, 25), (34, 35), null, (52, 53), (61, 61), (70, 70), (79, 79), (36, 36), (37, 37), (38, 38), (39, 39), (40, 40), (41, 41), (42, 51), null, (44, 53)],
		[null, null, (26, 8), (38, 36), (41, 39), null, null, null, (62, 80), (8, 8), (17, 17), (26, 26), (35, 33), null, (53, 51), (62, 62), (71, 71), (80, 80), (36, 36), (37, 37), (38, 38), (39, 39), (40, 40), (41, 41), (42, 51), (43, 52), null],
		[(18, 0), null, null, null, (48, 50), (51, 53), (54, 72), null, null, (0, 0), (9, 9), (18, 18), (27, 29), (36, 38), null, (54, 54), (63, 63), (72, 72), null, (46, 28), (47, 29), (48, 48), (49, 49), (50, 50), (51, 51), (52, 52), (53, 53)],
		[(19, 1), null, null, null, (48, 50), (51, 53), (55, 73), null, null, (1, 1), (10, 10), (19, 19), (28, 29), (37, 38), null, (55, 55), (64, 64), (73, 73), (45, 27), null, (47, 29), (48, 48), (49, 49), (50, 50), (51, 51), (52, 52), (53, 53)],
		[(20, 2), null, null, null, (48, 50), (51, 53), (56, 74), null, null, (2, 2), (11, 11), (20, 20), (29, 27), (38, 36), null, (56, 56), (65, 65), (74, 74), (45, 27), (46, 28), null, (48, 48), (49, 49), (50, 50), (51, 51), (52, 52), (53, 53)],
		[null, (21, 3), null, (47, 45), null, (51, 53), null, (57, 75), null, (3, 3), (12, 12), (21, 21), (30, 32), (39, 41), null, (57, 57), (66, 66), (75, 75), (45, 45), (46, 46), (47, 47), null, (49, 31), (50, 32), (51, 51), (52, 52), (53, 53)],
		[null, (22, 4), null, (47, 45), null, (51, 53), null, (58, 76), null, (4, 4), (13, 13), (22, 22), (31, 32), (40, 41), null, (58, 58), (67, 67), (76, 76), (45, 45), (46, 46), (47, 47), (48, 30), null, (50, 32), (51, 51), (52, 52), (53, 53)],
		[null, (23, 5), null, (47, 45), null, (51, 53), null, (59, 77), null, (5, 5), (14, 14), (23, 23), (32, 30), (41, 39), null, (59, 59), (68, 68), (77, 77), (45, 45), (46, 46), (47, 47), (48, 30), (49, 31), null, (51, 51), (52, 52), (53, 53)],
		[null, null, (24, 6), (47, 45), (50, 48), null, null, null, (60, 78), (6, 6), (15, 15), (24, 24), (33, 35), (42, 44), null, (60, 60), (69, 69), (78, 78), (45, 45), (46, 46), (47, 47), (48, 48), (49, 49), (50, 50), null, (52, 34), (53, 35)],
		[null, null, (25, 7), (47, 45), (50, 48), null, null, null, (61, 79), (7, 7), (16, 16), (25, 25), (34, 35), (43, 44), null, (61, 61), (70, 70), (79, 79), (45, 45), (46, 46), (47, 47), (48, 48), (49, 49), (50, 50), (51, 33), null, (53, 35)],
		[null, null, (26, 8), (47, 45), (50, 48), null, null, null, (62, 80), (8, 8), (17, 17), (26, 26), (35, 33), (44, 42), null, (62, 62), (71, 71), (80, 80), (45, 45), (46, 46), (47, 47), (48, 48), (49, 49), (50, 50), (51, 33), (52, 34), null],
		[(18, 0), null, null, (45, 27), null, null, null, (57, 59), (60, 62), (0, 0), (9, 9), (18, 18), (27, 27), (36, 36), (45, 45), null, (63, 65), (72, 74), null, (55, 73), (56, 74), (57, 57), (58, 58), (59, 59), (60, 60), (61, 61), (62, 62)],
		[(19, 1), null, null, (46, 28), null, null, null, (57, 59), (60, 62), (1, 1), (10, 10), (19, 19), (28, 28), (37, 37), (46, 46), null, (64, 65), (73, 74), (54, 72), null, (56, 74), (57, 57), (58, 58), (59, 59), (60, 60), (61, 61), (62, 62)],
		[(20, 2), null, null, (47, 29), null, null, null, (57, 59), (60, 62), (2, 2), (11, 11), (20, 20), (29, 29), (38, 38), (47, 47), null, (65, 63), (74, 72), (54, 72), (55, 73), null, (57, 57), (58, 58), (59, 59), (60, 60), (61, 61), (62, 62)],
		[null, (21, 3), null, null, (48, 30), null, (56, 54), null, (60, 62), (3, 3), (12, 12), (21, 21), (30, 30), (39, 39), (48, 48), null, (66, 68), (75, 77), (54, 54), (55, 55), (56, 56), null, (58, 76), (59, 77), (60, 60), (61, 61), (62, 62)],
		[null, (22, 4), null, null, (49, 31), null, (56, 54), null, (60, 62), (4, 4), (13, 13), (22, 22), (31, 31), (40, 40), (49, 49), null, (67, 68), (76, 77), (54, 54), (55, 55), (56, 56), (57, 75), null, (59, 77), (60, 60), (61, 61), (62, 62)],
		[null, (23, 5), null, null, (50, 32), null, (56, 54), null, (60, 62), (5, 5), (14, 14), (23, 23), (32, 32), (41, 41), (50, 50), null, (68, 66), (77, 75), (54, 54), (55, 55), (56, 56), (57, 75), (58, 76), null, (60, 60), (61, 61), (62, 62)],
		[null, null, (24, 6), null, null, (51, 33), (56, 54), (59, 57), null, (6, 6), (15, 15), (24, 24), (33, 33), (42, 42), (51, 51), null, (69, 71), (78, 80), (54, 54), (55, 55), (56, 56), (57, 57), (58, 58), (59, 59), null, (61, 79), (62, 80)],
		[null, null, (25, 7), null, null, (52, 34), (56, 54), (59, 57), null, (7, 7), (16, 16), (25, 25), (34, 34), (43, 43), (52, 52), null, (70, 71), (79, 80), (54, 54), (55, 55), (56, 56), (57, 57), (58, 58), (59, 59), (60, 78), null, (62, 80)],
		[null, null, (26, 8), null, null, (53, 35), (56, 54), (59, 57), null, (8, 8), (17, 17), (26, 26), (35, 35), (44, 44), (53, 53), null, (71, 69), (80, 78), (54, 54), (55, 55), (56, 56), (57, 57), (58, 58), (59, 59), (60, 78), (61, 79), null],
		[(18, 0), null, null, (45, 27), null, null, null, (66, 68), (69, 71), (0, 0), (9, 9), (18, 18), (27, 27), (36, 36), (45, 45), (54, 56), null, (72, 74), null, (64, 73), (65, 74), (66, 66), (67, 67), (68, 68), (69, 69), (70, 70), (71, 71)],
		[(19, 1), null, null, (46, 28), null, null, null, (66, 68), (69, 71), (1, 1), (10, 10), (19, 19), (28, 28), (37, 37), (46, 46), (55, 56), null, (73, 74), (63, 72), null, (65, 74), (66, 66), (67, 67), (68, 68), (69, 69), (70, 70), (71, 71)],
		[(20, 2), null, null, (47, 29), null, null, null, (66, 68), (69, 71), (2, 2), (11, 11), (20, 20), (29, 29), (38, 38), (47, 47), (56, 54), null, (74, 72), (63, 72), (64, 73), null, (66, 66), (67, 67), (68, 68), (69, 69), (70, 70), (71, 71)],
		[null, (21, 3), null, null, (48, 30), null, (65, 63), null, (69, 71), (3, 3), (12, 12), (21, 21), (30, 30), (39, 39), (48, 48), (57, 59), null, (75, 77), (63, 63), (64, 64), (65, 65), null, (67, 76), (68, 77), (69, 69), (70, 70), (71, 71)],
		[null, (22, 4), null, null, (49, 31), null, (65, 63), null, (69, 71), (4, 4), (13, 13), (22, 22), (31, 31), (40, 40), (49, 49), (58, 59), null, (76, 77), (63, 63), (64, 64), (65, 65), (66, 75), null, (68, 77), (69, 69), (70, 70), (71, 71)],
		[null, (23, 5), null, null, (50, 32), null, (65, 63), null, (69, 71), (5, 5), (14, 14), (23, 23), (32, 32), (41, 41), (50, 50), (59, 57), null, (77, 75), (63, 63), (64, 64), (65, 65), (66, 75), (67, 76), null, (69, 69), (70, 70), (71, 71)],
		[null, null, (24, 6), null, null, (51, 33), (65, 63), (68, 66), null, (6, 6), (15, 15), (24, 24), (33, 33), (42, 42), (51, 51), (60, 62), null, (78, 80), (63, 63), (64, 64), (65, 65), (66, 66), (67, 67), (68, 68), null, (70, 79), (71, 80)],
		[null, null, (25, 7), null, null, (52, 34), (65, 63), (68, 66), null, (7, 7), (16, 16), (25, 25), (34, 34), (43, 43), (52, 52), (61, 62), null, (79, 80), (63, 63), (64, 64), (65, 65), (66, 66), (67, 67), (68, 68), (69, 78), null, (71, 80)],
		[null, null, (26, 8), null, null, (53, 35), (65, 63), (68, 66), null, (8, 8), (17, 17), (26, 26), (35, 35), (44, 44), (53, 53), (62, 60), null, (80, 78), (63, 63), (64, 64), (65, 65), (66, 66), (67, 67), (68, 68), (69, 78), (70, 79), null],
		[(18, 0), null, null, (45, 27), null, null, null, (75, 77), (78, 80), (0, 0), (9, 9), (18, 18), (27, 27), (36, 36), (45, 45), (54, 56), (63, 65), null, null, (73, 55), (74, 56), (75, 75), (76, 76), (77, 77), (78, 78), (79, 79), (80, 80)],
		[(19, 1), null, null, (46, 28), null, null, null, (75, 77), (78, 80), (1, 1), (10, 10), (19, 19), (28, 28), (37, 37), (46, 46), (55, 56), (64, 65), null, (72, 54), null, (74, 56), (75, 75), (76, 76), (77, 77), (78, 78), (79, 79), (80, 80)],
		[(20, 2), null, null, (47, 29), null, null, null, (75, 77), (78, 80), (2, 2), (11, 11), (20, 20), (29, 29), (38, 38), (47, 47), (56, 54), (65, 63), null, (72, 54), (73, 55), null, (75, 75), (76, 76), (77, 77), (78, 78), (79, 79), (80, 80)],
		[null, (21, 3), null, null, (48, 30), null, (74, 72), null, (78, 80), (3, 3), (12, 12), (21, 21), (30, 30), (39, 39), (48, 48), (57, 59), (66, 68), null, (72, 72), (73, 73), (74, 74), null, (76, 58), (77, 59), (78, 78), (79, 79), (80, 80)],
		[null, (22, 4), null, null, (49, 31), null, (74, 72), null, (78, 80), (4, 4), (13, 13), (22, 22), (31, 31), (40, 40), (49, 49), (58, 59), (67, 68), null, (72, 72), (73, 73), (74, 74), (75, 57), null, (77, 59), (78, 78), (79, 79), (80, 80)],
		[null, (23, 5), null, null, (50, 32), null, (74, 72), null, (78, 80), (5, 5), (14, 14), (23, 23), (32, 32), (41, 41), (50, 50), (59, 57), (68, 66), null, (72, 72), (73, 73), (74, 74), (75, 57), (76, 58), null, (78, 78), (79, 79), (80, 80)],
		[null, null, (24, 6), null, null, (51, 33), (74, 72), (77, 75), null, (6, 6), (15, 15), (24, 24), (33, 33), (42, 42), (51, 51), (60, 62), (69, 71), null, (72, 72), (73, 73), (74, 74), (75, 75), (76, 76), (77, 77), null, (79, 61), (80, 62)],
		[null, null, (25, 7), null, null, (52, 34), (74, 72), (77, 75), null, (7, 7), (16, 16), (25, 25), (34, 34), (43, 43), (52, 52), (61, 62), (70, 71), null, (72, 72), (73, 73), (74, 74), (75, 75), (76, 76), (77, 77), (78, 60), null, (80, 62)],
		[null, null, (26, 8), null, null, (53, 35), (74, 72), (77, 75), null, (8, 8), (17, 17), (26, 26), (35, 35), (44, 44), (53, 53), (62, 60), (71, 69), null, (72, 72), (73, 73), (74, 74), (75, 75), (76, 76), (77, 77), (78, 60), (79, 61), null]
	];


	/// <summary>
	/// The root value of P.
	/// </summary>
	private readonly int _p = p < 1
		? throw new ArgumentOutOfRangeException(nameof(p))
		: q < 1
			? throw new ArgumentOutOfRangeException(nameof(q))
			: p * SimplifyRootPart(ref q);

	/// <summary>
	/// The root value of Q.
	/// </summary>
	private readonly int _q = q;


	/// <summary>
	/// Initializes a <see cref="Distance"/> instance via both values 1.
	/// </summary>
	public Distance() : this(1, 1)
	{
	}

	/// <summary>
	/// Initializes a <see cref="Distance"/> instance via the root part, with the default integer part 1.
	/// <i>This value will automatically simplify the root expression, e.g. sqrt(18) -> 3sqrt(2).</i>
	/// </summary>
	/// <param name="q">The root value.</param>
	public Distance(int q) : this(1, q)
	{
	}


	/// <summary>
	/// The raw value of the distance. The value will be output as a <see cref="double"/> value.
	/// </summary>
	public double RawValue => _p * Math.Sqrt(_q);


	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Distance other) => (_p, _q) == (other._p, other._q);

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(Distance other) => RawValue.CompareTo(other.RawValue);

	/// <inheritdoc cref="object.ToString"/>
	/// <remarks>
	/// The output format will be "<c>psq</c>", where <c>p</c> and <c>q</c> are the variables, and <c>s</c> means "square root of".
	/// For example, "<c>3s2</c>" means <c>3 * sqrt(2)</c>, i.e. <c>sqrt(18)</c>.
	/// </remarks>
	public override string ToString()
		=> (_p, _q) switch
		{
			(_, 1) => _p.ToString(),
			(1, _) => $"{RootPartSpecifier}{_q}",
			_ => $"{_p}{RootPartSpecifier}{_q}"
		};


	/// <summary>
	/// Try to fetch the distance for the two cells.
	/// </summary>
	/// <param name="cell1">The first cell to be compared.</param>
	/// <param name="cell2">The second cell to be compared.</param>
	/// <returns>The distance result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Distance GetDistance(Cell cell1, Cell cell2)
	{
		var (x1, y1) = (cell1 / 9, cell1 % 9);
		var (x2, y2) = (cell2 / 9, cell2 % 9);
		return new((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
	}

	/// <summary>
	/// Try to fetch the distance for two cells stored in a <see cref="CellMap"/> instance.
	/// </summary>
	/// <param name="cells">The <see cref="CellMap"/> instance storing two cells.</param>
	/// <returns>The distance result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Distance GetDistance(in CellMap cells)
	{
		ArgumentException.ThrowIfAssertionFailed(cells.Count == 2);
		return GetDistance(cells[0], cells[1]);
	}

	/// <summary>
	/// Try to get the minimal and maximal distance to the specified house from the specified cell.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="house">The house.</param>
	/// <returns>The minimal and maximal distance value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (Cell Min, Cell Max)? GetDistanceTo(Cell cell, House house) => DistanceTable[cell][house];

	/// <summary>
	/// Try to get the intermediate cells that are between <paramref name="cell1"/> and <paramref name="cell2"/> in logical position
	/// for sudoku grid.
	/// </summary>
	/// <param name="cell1">The first cell.</param>
	/// <param name="cell2">The second cell. The value should be greater than <paramref name="cell1"/>.</param>
	/// <returns>The intermediate cells.</returns>
	/// <exception cref="InvalidOperationException">
	/// Throws when cells <paramref name="cell1"/> and <paramref name="cell2"/> are not in a same line (row or column).
	/// </exception>
	public static CellMap GetIntermediateCells(Cell cell1, Cell cell2)
	{
		if (cell1 == cell2)
		{
			return [];
		}

		if (cell1 > cell2)
		{
			// Keeps the less value as the first cell.
			(cell1, cell2) = (cell2, cell1);
		}

		if ((cell1.AsCellMap() + cell2).SharedLine is not (var sharedHouse and not 32))
		{
			throw new InvalidOperationException(SR.ExceptionMessage("CellsShouldInSameLine"));
		}

		var houseCells = HousesCells[sharedHouse];
		return houseCells[(Array.IndexOf(houseCells, cell1) + 1)..Array.IndexOf(houseCells, cell2)].AsCellMap();
	}

	/// <summary>
	/// Simplifies for root part.
	/// </summary>
	/// <param name="base">The root value.</param>
	/// <returns>The P value.</returns>
	private static int SimplifyRootPart(ref int @base)
	{
		var result = 1;
		var temp = @base;
		for (var i = 2; i * i <= @base;)
		{
			if (temp % (i * i) == 0)
			{
				temp /= i * i;
				result *= i;
				continue;
			}
			i = i == 2 ? 3 : i + 2;
		}

		@base = temp;
		return result;
	}
}
