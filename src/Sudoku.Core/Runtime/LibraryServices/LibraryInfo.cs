namespace Sudoku.Runtime.LibraryServices;

/// <summary>
/// Represents an entry that plays with a puzzle library file.
/// </summary>
/// <param name="directory">Indicates the parent directory that stores the library.</param>
/// <param name="fileId">Indicates the file name used. The value should be valid as a file name, without file extension.</param>
/// <remarks><i>
/// This type only supports for Windows now. For other OS platforms,
/// I will allow them in the future because I'm not familiar with file systems on other OS platforms.
/// </i></remarks>
[DebuggerStepThrough]
[StructLayout(LayoutKind.Auto)]
[SupportedOSPlatform("windows")]
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.EqualityOperators | TypeImplFlags.Equatable)]
public readonly partial struct LibraryInfo(
	[Field(Accessibility = "internal"), HashCodeMember, StringMember] string directory,
	[Property, HashCodeMember, StringMember] string fileId
) :
	IAsyncEnumerable<Grid>,
	IEqualityOperators<LibraryInfo, LibraryInfo, bool>,
	IEquatable<LibraryInfo>
{
	/// <summary>
	/// Indicates the separator character.
	/// </summary>
	private const char SeparatorChar = ',';


	/// <summary>
	/// Indicates the file header of config files after created or initialized.
	/// </summary>
	public static readonly string ConfigFileHeader = "$ Header of the File $";


	/// <summary>
	/// Indicates whether the library-related files are initialized.
	/// </summary>
	public bool IsInitialized
		=> (File.Exists(ConfigFilePath), File.Exists(LibraryFilePath)) switch
		{
			(true, true) => true,
			(false, false) => false,
			_ => throw new InvalidOperationException(SR.ExceptionMessage("DifferentExistenceOfConfigAndLibraryFile"))
		};

	/// <summary>
	/// Indicates the number of puzzles stored in this file.
	/// </summary>
	/// <remarks>
	/// This property is run synchronously, calling <see cref="GetCountAsync"/>.
	/// <b>Always measure performance if you want to use this property.</b>
	/// </remarks>
	/// <seealso cref="GetCountAsync"/>
	public int Count => GetCountAsync().Result;

	/// <summary>
	/// Indicates the path of the library file. The file only contains the puzzles.
	/// If you want to check for details of the configuration, use <see cref="ConfigFilePath"/> instead.
	/// </summary>
	/// <seealso cref="ConfigFilePath"/>
	[EquatableMember]
	public string LibraryFilePath => $@"{_directory}\{FileId}";

	/// <summary>
	/// Indicates the path of configuration file. The file contains the information of the library.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Due to the design of the library APIs, a puzzle library contains two parts, separated with 2 files.
	/// One is the configuration file, and the other is the library details, only containing puzzles.
	/// </para>
	/// <para><i>
	/// Call this property will implicitly create config file if file is not found. No exception will be thrown here.
	/// </i></para>
	/// </remarks>
	public string ConfigFilePath => $@"{_directory}\{FileId}{ConfigFileExtension}";

	/// <summary>
	/// Indicates the author of the library. Return <see langword="null"/> if no author configured.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the library is not initialized.</exception>
	/// <exception cref="FileNotFoundException">Throws when the config file is missing.</exception>
	public string? Author
	{
		get
		{
			var pattern = AuthorPattern;
			return IsInitialized
				? (
					from line in File.ReadLines(ConfigFilePath)
					select pattern.Match(line).Groups into groups
					where groups.Count == 2
					select groups[1].Value
				).FirstOrDefault()
				: throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		set
		{
			if (!File.Exists(ConfigFilePath))
			{
				throw new FileNotFoundException(SR.ExceptionMessage("NotExist"));
			}

			ConfigFileReplaceOrAppend(AuthorPattern.IsMatch, value);
		}
	}

	/// <summary>
	/// Indicates the name of the library.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the library is not initialized.</exception>
	/// <exception cref="FileNotFoundException">Throws when the config file is missing.</exception>
	public string? Name
	{
		get
		{
			var pattern = NamePattern;
			return IsInitialized
				? (
					from line in File.ReadLines(ConfigFilePath)
					select pattern.Match(line).Groups into groups
					where groups.Count == 2
					select groups[1].Value
				).FirstOrDefault()
				: throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		set
		{
			if (!File.Exists(ConfigFilePath))
			{
				throw new FileNotFoundException(SR.ExceptionMessage("NotExist"));
			}

			ConfigFileReplaceOrAppend(NamePattern.IsMatch, value);
		}
	}

	/// <summary>
	/// Indicates the description to the library.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the library is not initialized.</exception>
	/// <exception cref="FileNotFoundException">Throws when the config file is missing.</exception>
	public string? Description
	{
		get
		{
			var pattern = DescriptionPattern;
			return IsInitialized
				? (
					from line in File.ReadLines(ConfigFilePath)
					select pattern.Match(line).Groups into groups
					where groups.Count == 2
					select groups[1].Value
				).FirstOrDefault()
				: throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		set
		{
			if (!File.Exists(ConfigFilePath))
			{
				throw new FileNotFoundException(SR.ExceptionMessage("NotExist"));
			}

			ConfigFileReplaceOrAppend(DescriptionPattern.IsMatch, value);
		}
	}

	/// <summary>
	/// Indicates the tags of the library.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the library is not initialized.</exception>
	/// <exception cref="FileNotFoundException">Throws when the config file is missing.</exception>
	public string[]? Tags
	{
		get
		{
			var pattern = TagsPattern;
			return IsInitialized
				? (
					from line in File.ReadLines(ConfigFilePath)
					select pattern.Match(line).Groups into groups
					where groups.Count == 2
					select groups[1].Value into line
					select line.Split(SeparatorChar)
				).FirstOrDefault()
				: throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		set
		{
			if (!File.Exists(ConfigFilePath))
			{
				throw new FileNotFoundException(SR.ExceptionMessage("NotExist"));
			}

			ConfigFileReplaceOrAppend(TagsPattern.IsMatch, value is not null ? string.Join(SeparatorChar, value) : null);
		}
	}

	/// <summary>
	/// Indicates the last modified time of the library file.
	/// </summary>
	public DateTime LastModifiedTime => File.GetLastWriteTime(LibraryFilePath);


	/// <summary>
	/// Indicates the supported extension of config file. The extension will be used by API in runtime, recognizing config files.
	/// </summary>
	public static string ConfigFileExtension { get; set; } = ".txt";

	[GeneratedRegex(@"author:\s*([\s\S]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, 5000)]
	private static partial Regex AuthorPattern { get; }

	[GeneratedRegex(@"name:\s*([\S\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, 5000)]
	private static partial Regex NamePattern { get; }

	[GeneratedRegex(@"description:\s*([\S\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, 5000)]
	private static partial Regex DescriptionPattern { get; }

	[GeneratedRegex(@"tags:\s*([\S\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, 5000)]
	private static partial Regex TagsPattern { get; }


	/// <summary>
	/// Try to get the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The target <see cref="Grid"/> instance at the specified index.</returns>
	/// <remarks>
	/// This property is run synchronously, calling <see cref="GetAtAsync(int, CancellationToken)"/>.
	/// <b>Always measure performance if you want to use this indexer.</b>
	/// </remarks>
	/// <seealso cref="GetAtAsync(int, CancellationToken)"/>
	public Grid this[int index] => GetAtAsync(index).Result;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out string libraryPath, out string configPath)
		=> (libraryPath, configPath) = (LibraryFilePath, ConfigFilePath);

	/// <summary>
	/// Initializes the library-related files if not found. If initialized, throw <see cref="LibraryInitializationException"/>.
	/// </summary>
	/// <exception cref="LibraryInitializationException">Throws when the library has already been initialized.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Initialize()
	{
		if (!IsInitialized)
		{
			using var fs = File.Create(ConfigFilePath);
			using var sw = new StreamWriter(fs);
			sw.WriteLine(ConfigFileHeader);

			if (!File.Exists(LibraryFilePath))
			{
				File.Create(LibraryFilePath).Close();
			}
			return;
		}

		throw new LibraryInitializationException(this);
	}

	/// <summary>
	/// Delete the current library, removing files from local path.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Delete()
	{
		if (this is (var libPath, var configPath) { IsInitialized: false })
		{
			File.Delete(configPath);
			File.Delete(libPath);
		}
	}

	/// <summary>
	/// Clears the current library, removing all puzzles stored in this library, making the file empty,
	/// but reserving the files not deleted.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the library isn't initialized.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ClearPuzzles()
	{
		if (IsInitialized)
		{
			File.Create(LibraryFilePath).Close();
			return;
		}

		throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
	}

	/// <summary>
	/// Determines whether the library contains at least one puzzle.
	/// </summary>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Any() => File.ReadLines(LibraryFilePath).Any();

	/// <summary>
	/// <para>
	/// Append a puzzle, represented as a <see cref="string"/> value,
	/// into the specified file path represented as a puzzle library.
	/// </para>
	/// <para>
	/// If the library is not initialized, it will be automatically initialized. No exceptions will be thrown on this case.
	/// </para>
	/// </summary>
	/// <param name="grid">The grid text code to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task"/> instance that can be used in <see langword="await"/> expression.</returns>
	/// <exception cref="InvalidOperationException">Throws when the grid cannot be recognized.</exception>
	public async Task AppendPuzzleAsync(string grid, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			Initialize();
		}

		var sb = new StringBuilder();
		using (var sr = new StreamReader(LibraryFilePath))
		{
			if (!sr.EndsWithNewLine)
			{
				sb.AppendLine();
			}
		}

		if (!Grid.TryParse(grid, out _))
		{
			throw new InvalidOperationException(SR.ExceptionMessage("UnrecognizedGridFormat"));
		}

		sb.AppendLine(grid);

		await using var sw = new StreamWriter(LibraryFilePath, true);
		await sw.WriteAsync(sb, cancellationToken);
	}

	/// <inheritdoc cref="AppendPuzzleAsync(string, CancellationToken)"/>
	public async Task AppendPuzzleAsync(Grid grid, CancellationToken cancellationToken = default)
		=> await AppendPuzzleAsync(GetSingleLineGridString(grid), cancellationToken);

	/// <summary>
	/// <para>
	/// Append a list of puzzles, represented as a list of <see cref="string"/> values,
	/// into the specified file path represented as a puzzle library.
	/// </para>
	/// <para>
	/// If the library is not initialized, it will be automatically initialized. No exceptions will be thrown on this case.
	/// </para>
	/// </summary>
	/// <param name="grids">A list of grid text code to be appended.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> of an <see cref="int"/> instance indicating how many text code are appended into the file.
	/// </returns>
	public async Task<int> AppendPuzzlesAsync(IEnumerable<string> grids, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			Initialize();
		}

		var sb = new StringBuilder();
		using (var sr = new StreamReader(LibraryFilePath))
		{
			if (!sr.EndsWithNewLine)
			{
				sb.AppendLine();
			}
		}

		var result = 0;
		foreach (var grid in grids)
		{
			if (Grid.TryParse(grid, out _))
			{
				sb.AppendLine(grid);
				result++;
			}
		}

		await using var sw = new StreamWriter(LibraryFilePath, true);
		await sw.WriteAsync(sb, cancellationToken);
		return result;
	}

	/// <inheritdoc cref="AppendPuzzlesAsync(IEnumerable{string}, CancellationToken)"/>
	public async Task<int> AppendPuzzlesAsync(IAsyncEnumerable<string> grids, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			Initialize();
		}

		var sb = new StringBuilder();
		using (var sr = new StreamReader(LibraryFilePath))
		{
			if (!sr.EndsWithNewLine)
			{
				sb.AppendLine();
			}
		}

		var result = 0;
		await foreach (var line in grids)
		{
			if (Grid.TryParse(line, out _))
			{
				sb.AppendLine(line);
				result++;
			}
		}

		await using var sw = new StreamWriter(LibraryFilePath, true);
		await sw.WriteAsync(sb, cancellationToken);
		return result;
	}

	/// <inheritdoc cref="AppendPuzzlesAsync(IEnumerable{string}, CancellationToken)"/>
	public async Task AppendPuzzlesAsync(IEnumerable<Grid> grids, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			Initialize();
		}

		var sb = new StringBuilder();
		using (var sr = new StreamReader(LibraryFilePath))
		{
			if (!sr.EndsWithNewLine)
			{
				sb.AppendLine();
			}
		}

		foreach (var grid in grids)
		{
			sb.AppendLine(GetSingleLineGridString(grid));
		}

		await using var sw = new StreamWriter(LibraryFilePath, true);
		await sw.WriteAsync(sb, cancellationToken);
	}

	/// <summary>
	/// Removes all puzzles that exactly same as the specified one from the file.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task"/> instance that can be used in <see langword="await"/> expression.</returns>
	/// <exception cref="InvalidOperationException">Throw when the library file is not initialized.</exception>
	public async Task RemovePuzzleAsync(string grid, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		var linesToKeep = new List<string>();
		await foreach (var line in File.ReadLinesAsync(LibraryFilePath, cancellationToken))
		{
			if (line != grid)
			{
				linesToKeep.Add(line);
			}
		}

		var tempFile = Path.GetTempFileName();
		await File.WriteAllLinesAsync(tempFile, linesToKeep, cancellationToken);
		File.Delete(LibraryFilePath);
		File.Move(tempFile, LibraryFilePath);
	}

	/// <inheritdoc cref="RemovePuzzleAsync(string, CancellationToken)"/>
	public async Task RemovePuzzleAsync(Grid grid, CancellationToken cancellationToken = default)
		=> await RemovePuzzleAsync(GetSingleLineGridString(grid), cancellationToken);

	/// <summary>
	/// Removes a list of duplicate puzzles stored in the current library.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task"/> instance that can be used in <see langword="await"/> expression.</returns>
	/// <exception cref="InvalidOperationException">Throws when the puzzle should be initialized first.</exception>
	public async Task RemoveDuplicatePuzzlesAsync(CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		var textSet = new HashSet<ReadOnlyMemory<char>>(ReadOnlyMemoryOfCharComparer.Instance);
		var altLookup = textSet.GetAlternateLookup<ReadOnlySpan<char>>();
		await foreach (var grid in EnumerateTextAsync(cancellationToken))
		{
			altLookup.Add(grid);
		}

		await using var sw = new StreamWriter(LibraryFilePath, false);
		var sb = new StringBuilder();
		foreach (var text in textSet)
		{
			sb.Append(text).AppendLine();
		}

		await sw.WriteAsync(sb, cancellationToken);
	}

	/// <summary>
	/// <para>Write a puzzle into a file just created.</para>
	/// <para>
	/// If the library is not initialized, it will be automatically initialized. No exceptions will be thrown on this case.
	/// </para>
	/// </summary>
	/// <param name="grid">The grid to be written.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task"/> instance that can be used in <see langword="await"/> expression.</returns>
	/// <exception cref="InvalidOperationException">Throws when the grid cannot be recognized.</exception>
	public async Task WriteAsync(string grid, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			Initialize();
		}

		if (Grid.TryParse(grid, out _))
		{
			await File.WriteAllTextAsync(LibraryFilePath, grid, cancellationToken);
		}
	}

	/// <inheritdoc cref="WriteAsync(string, CancellationToken)"/>
	public async Task WriteAsync(Grid grid, CancellationToken cancellationToken = default)
		=> await WriteAsync(GetSingleLineGridString(grid), cancellationToken);

	/// <summary>
	/// Calculates how many puzzles in this file.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task{TResult}"/> of an <see cref="int"/> value indicating the result.</returns>
	/// <remarks>
	/// <b><i>If you want to check whether the puzzle has at least one puzzle, please use method <see cref="Any"/> instead.</i></b>
	/// </remarks>
	/// <exception cref="InvalidOperationException">Throws when the library file is not initialized.</exception>
	/// <seealso cref="Any"/>
	public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		var result = 0;
		await foreach (var _ in EnumerateTextAsync(cancellationToken))
		{
			result++;
		}

		return result;
	}

	/// <summary>
	/// Gets the <see cref="Grid"/> at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>A <see cref="Task{TResult}"/> of <see cref="Grid"/> instance as the result.</returns>
	/// <exception cref="InvalidOperationException">Throws when the library file is not initialized.</exception>
	/// <exception cref="IndexOutOfRangeException">Throws when the index is out of range.</exception>
	public async Task<Grid> GetAtAsync(int index, CancellationToken cancellationToken = default)
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("FileShouldBeInitializedFirst"));
		}

		var i = -1;
		await foreach (var text in EnumerateTextAsync(cancellationToken))
		{
			if (++i == index)
			{
				return Grid.Parse(text);
			}
		}

		throw new IndexOutOfRangeException();
	}

	/// <summary>
	/// Creates a <see cref="TextPointer"/> instance that uses <see cref="FileStream"/> to read puzzles line by line.
	/// </summary>
	/// <returns>A <see cref="TextPointer"/> instance that reads for the current library.</returns>
	/// <remarks><b>
	/// This method returns an instance that implements <see cref="IAsyncDisposable"/> and <see cref="IDisposable"/>,
	/// meaning you must call <see cref="IAsyncDisposable.DisposeAsync"/> or <see cref="IDisposable.Dispose"/>
	/// after you finishing using the return value:
	/// <code><![CDATA[await using var pointer = library.CreateTextPointer();]]></code>
	/// </b></remarks>
	/// <seealso cref="TextPointer"/>
	/// <seealso cref="IDisposable"/>
	/// <seealso cref="IAsyncDisposable"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TextPointer CreateTextPointer() => new(this);

	/// <inheritdoc/>
	public async IAsyncEnumerator<Grid> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		await foreach (var text in EnumerateTextAsync(cancellationToken))
		{
			yield return Grid.Parse(text);
		}
	}

	/// <summary>
	/// Enumerates raw text codes stored in the library.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
	/// <returns>An async-iterable collection of <see cref="string"/> values as raw text codes.</returns>
	public async IAsyncEnumerable<string> EnumerateTextAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		if (!File.Exists(LibraryFilePath))
		{
			yield break;
		}

		await foreach (var text in File.ReadLinesAsync(LibraryFilePath, cancellationToken))
		{
			yield return text;
		}
	}

	/// <summary>
	/// Replace or append the value into the file, using the specified match method.
	/// </summary>
	/// <param name="match">The matcher method.</param>
	/// <param name="replaceOrAppendValue">The value to replace with original value, or appened.</param>
	/// <param name="callerPropertyName">
	/// Indicates the property name as caller. This parameter shouldn't be assigned. It will be assigned by compiler.
	/// </param>
	/// <exception cref="InvalidOperationException">Throws when multiple same properties found.</exception>
	private void ConfigFileReplaceOrAppend(
		Func<string, bool> match,
		string? replaceOrAppendValue,
		[CallerMemberName] string callerPropertyName = null!
	)
	{
		var linesToKeep = new List<string>();
		var isFound = false;
		foreach (var line in File.ReadLines(ConfigFilePath))
		{
			if (!match(line))
			{
				linesToKeep.Add(line);
				continue;
			}

			if (isFound)
			{
				throw new InvalidOperationException(SR.ExceptionMessage("MultipleSamePropertiesFound"));
			}

			a(linesToKeep, callerPropertyName, replaceOrAppendValue);
			isFound = true;
		}
		if (!isFound)
		{
			a(linesToKeep, callerPropertyName, replaceOrAppendValue);
		}

		var tempFile = Path.GetTempFileName();
		File.WriteAllLines(tempFile, linesToKeep);
		File.Delete(ConfigFilePath);
		File.Move(tempFile, ConfigFilePath);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void a(List<string> list, string prefix, string? replaceOrAppendValue)
		{
			if (replaceOrAppendValue is not null)
			{
				list.Add($"{prefix}: {replaceOrAppendValue}");
			}
		}
	}


	/// <summary>
	/// Registers the config file extension. Argument should contain prefix period token '<c>.</c>'.
	/// </summary>
	/// <param name="extension">The extension of the config file. Period '<c>.</c>' required.</param>
	/// <remarks>
	/// <b>Please note that only one extension can work. If you call this method multiple times, only the last one will work.</b>
	/// </remarks>
	/// <exception cref="ArgumentException">Throws when the argument <paramref name="extension"/> is not valid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RegisterConfigFileExtension(string extension)
		=> ConfigFileExtension = extension.Any(Path.GetInvalidPathChars().Contains)
			? throw new ArgumentException(SR.ExceptionMessage("ArgExtensionShouldBeValid"), nameof(extension))
			: extension;

	/// <summary>
	/// Returns <c>grid.ToString("#")</c>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string GetSingleLineGridString(in Grid grid) => grid.ToString("#");
}
