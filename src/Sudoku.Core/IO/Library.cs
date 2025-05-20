namespace Sudoku.IO;

/// <summary>
/// Represents a puzzle library.
/// </summary>
/// <param name="directoryPath"><inheritdoc cref="_directoryPath" path="/summary"/></param>
/// <param name="identifier"><inheritdoc cref="_identifier" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class Library(string directoryPath, string identifier) :
	IAsyncEnumerable<Grid>,
	IEquatable<Library>,
	IEqualityOperators<Library, Library, bool>
{
	/// <summary>
	/// Indicates the JSON options to serialize or deserialize object.
	/// </summary>
	private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
	{
		WriteIndented = true,
		IndentCharacter = ' ',
		IndentSize = 4,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};


	/// <summary>
	/// Indicates the directory path.
	/// </summary>
	private readonly string _directoryPath = directoryPath;

	/// <summary>
	/// Indicates the library identifier.
	/// </summary>
	private readonly string _identifier = identifier;

	/// <summary>
	/// Indicates the lock object to keep operation thread-safe.
	/// </summary>
	private readonly Lock _fileLock = new();


	/// <summary>
	/// Indicates the information path.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	[StringMember]
	public string InfoPath => $"{_directoryPath}/{_identifier}.json";

	/// <summary>
	/// Indicates the library path.
	/// </summary>
	[StringMember]
	public string LibraryPath => $"{_directoryPath}/{_identifier}.txt";


	/// <summary>
	/// Writes the name to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteName(string value) => WriteProperty(static (info, value) => info.Name = value, value);

	/// <summary>
	/// Writes the description to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteDescription(string value) => WriteProperty(static (info, value) => info.Description = value, value);

	/// <summary>
	/// Writes the author to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteAuthor(string value) => WriteProperty(static (info, value) => info.Author = value, value);

	/// <summary>
	/// Writes the tags to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteTags(ReadOnlyMemory<string> value) => WriteProperty(static (info, value) => info.Tags = value.ToArray(), value);

	/// <summary>
	/// Reads the name of the library information file.
	/// </summary>
	/// <returns>The name.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ReadName() => ReadProperty(static info => info.Name);

	/// <summary>
	/// Reads the description of the library information file.
	/// </summary>
	/// <returns>The description.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ReadDescription() => ReadProperty(static info => info.Description);

	/// <summary>
	/// Reads the author of the library information file.
	/// </summary>
	/// <returns>The author.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ReadAuthor() => ReadProperty(static info => info.Author);

	/// <summary>
	/// Reads the tags of the library information file.
	/// </summary>
	/// <returns>The tags.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlyMemory<string> ReadTags() => ReadProperty(static info => info.Tags);

	/// <summary>
	/// Writes a new grid into the target file; if the file doesn't exist, it will create a new file,
	/// and then append the puzzle into the file.
	/// </summary>
	/// <param name="grid">The grid to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>A <see cref="Task"/> object that handles the asynchronous operation.</returns>
	public async Task WriteLineAsync(Grid grid, CancellationToken cancellationToken = default)
	{
		await using var writer = new LibraryFileWriter(LibraryPath, out _);
		await writer.WriteLineAsync(grid.ToString("."), cancellationToken);
	}

	/// <summary>
	/// Loads the other library, and reads for all puzzles and appends to the current library file.
	/// If any exceptions thrown or cancelled, no updates will be applied
	/// (the current file will keep the original state, rather than successfully written some puzzles before cancelled
	/// or exception encountered).
	/// </summary>
	/// <param name="other">The other library to merge into the current library.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> object that handles the asynchronous operation;
	/// with a <see cref="bool"/> result indicating whether the operation is failed (cancelled, exception encountered, etc.).
	/// </returns>
	public async Task<bool> TryMergeFromAsync(Library other, CancellationToken cancellationToken = default)
	{
		var tempFilePath = default(string);
		var backupFilePath = default(string);
		var originalFilePath = LibraryPath;
		await using var otherReader = new LibraryFileReader(other.LibraryPath, out var exists);
		if (!exists)
		{
			// The target file doesn't exist.
			return true;
		}

		try
		{
			// Generate a temporary file.
			tempFilePath = Path.GetTempFileName();
			backupFilePath = $"{originalFilePath}.bak";

			// Copies the current file into backup file.
			File.Copy(originalFilePath, tempFilePath, true);

			// The core operation to append lines.
			// Please note that this await foreach operation may throw OperationCanceledException,
			// so it will be catched and make a rollback instead of skipping the foreach loop.
			await using (var sw = new StreamWriter(tempFilePath, true))
			{
				await foreach (var line in otherReader.ReadLinesAsync(cancellationToken))
				{
					if (Grid.TryParse(line, out _))
					{
						await sw.WriteLineAsync(line);
					}
				}
			}

			// Replace with original file.
			File.Replace(tempFilePath, originalFilePath, backupFilePath, true);

			// Successful. Now delete the backup file.
			File.Delete(backupFilePath);
		}
		catch
		{
			rollback(originalFilePath, backupFilePath);
			return false;
		}
		finally
		{
			// Delete temporary file if exists.
			if (tempFilePath is not null && File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
		}
		return true;


		static void rollback(string originalFilePath, string? backupFilePath)
		{
			try
			{
				// If the backup file exists, the operation will be failed.
				if (File.Exists(backupFilePath))
				{
					// Recover the file from backup.
					File.Replace(backupFilePath, originalFilePath, null);
				}
			}
			catch
			{
			}
		}
	}

	/// <summary>
	/// Totals the number of puzzles up, and return the result.
	/// </summary>
	/// <returns>
	/// A <see cref="Task{TResult}"/> object that handles the asynchronous operation;
	/// with a result type <see cref="long"/> indicating the number of lines.
	/// </returns>
	public async Task<long> GetPuzzlesCountAsync()
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		return exists ? reader.CountLines() : 0;
	}

	/// <summary>
	/// Reads the specified number of puzzles from the library file
	/// without any conversions to <see cref="Grid"/> (only displays the raw text).
	/// If the file doesn't exist, nothing will be returned.
	/// </summary>
	/// <param name="start">Indicates the start index.</param>
	/// <param name="length">Indicates the desired number of puzzles.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An enumerable object that allows iterating values asynchronously.</returns>
	public async IAsyncEnumerable<string> ReadRawRangeAsync(
		ulong start,
		ulong length,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			// If the file is created just now, nothing will be iterated.
			yield break;
		}

		await foreach (var line in reader.ReadLinesRangeAsync(start, start + length, cancellationToken))
		{
			yield return line;
		}
	}

	/// <summary>
	/// Reads the specified number of puzzles from the library file;
	/// if the file doesn't exist, nothing will be returned.
	/// </summary>
	/// <param name="start">Indicates the start index.</param>
	/// <param name="length">Indicates the desired number of puzzles.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An enumerable object that allows iterating values asynchronously.</returns>
	public async IAsyncEnumerable<Grid> ReadRangeAsync(
		ulong start,
		ulong length,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			// If the file is created just now, nothing will be iterated.
			yield break;
		}

		await foreach (var line in reader.ReadLinesRangeAsync(start, start + length, cancellationToken))
		{
			if (Grid.TryParse(line, out var grid))
			{
				yield return grid;
			}
		}
	}

	/// <inheritdoc/>
	public async IAsyncEnumerator<Grid> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		await using var reader = new LibraryFileReader(LibraryPath, out var exists);
		if (!exists)
		{
			yield break;
		}

		await foreach (var line in reader.ReadLinesAsync(cancellationToken))
		{
			if (Grid.TryParse(line, out var grid))
			{
				yield return grid;
			}
		}
	}

	/// <summary>
	/// Writes the property of type <typeparamref name="T"/> to the file.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="valueAssignment">The result value assigning method.</param>
	/// <param name="value">The value to be set.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteProperty<T>(Action<LibraryInfo, T> valueAssignment, T value)
	{
		var info = LoadOrCreate();
		valueAssignment(info, value);
		Save(info);
	}

	/// <summary>
	/// Saves the file.
	/// </summary>
	/// <param name="info">The information to be saved.</param>
	private void Save(LibraryInfo info)
	{
		lock (_fileLock)
		{
			var json = JsonSerializer.Serialize(info, DefaultSerializerOptions);
			File.WriteAllText(InfoPath, json);
		}
	}

	/// <summary>
	/// Reads the property of value of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="resultValueCreator">The result value creator.</param>
	/// <returns>The result value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private T ReadProperty<T>(Func<LibraryInfo, T> resultValueCreator)
	{
		var info = LoadOrCreate();
		return resultValueCreator(info);
	}

	/// <summary>
	/// Loads the file, or creates the default instance if the file doesn't exist.
	/// </summary>
	/// <returns>The file loaded.</returns>
	private LibraryInfo LoadOrCreate()
	{
		lock (_fileLock)
		{
			if (!File.Exists(InfoPath))
			{
				return new();
			}

			var json = File.ReadAllText(InfoPath);
			return JsonSerializer.Deserialize<LibraryInfo>(json, DefaultSerializerOptions) ?? new();
		}
	}


	/// <summary>
	/// Creates a <see cref="Library"/> instance (and local files) via the specified information.
	/// </summary>
	/// <param name="directoryPath">The directory path.</param>
	/// <param name="identifier">The identifier.</param>
	/// <param name="libraryInfo">The library information.</param>
	/// <returns>The library instance.</returns>
	public static Library CreateLibrary(string directoryPath, string identifier, LibraryInfo libraryInfo)
	{
		var result = new Library(directoryPath, identifier);
		var info = result.LoadOrCreate();
		info.Name = libraryInfo.Name;
		info.Author = libraryInfo.Author;
		info.Description = libraryInfo.Description;
		info.Tags = libraryInfo.Tags;
		result.Save(info);
		return result;
	}
}
