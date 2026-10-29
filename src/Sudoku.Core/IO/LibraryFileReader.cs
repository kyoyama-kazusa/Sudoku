namespace Sudoku.IO;

/// <summary>
/// Represents a UTF-8 format library file reader.
/// </summary>
internal sealed class LibraryFileReader : IAsyncDisposable
{
	/// <summary>
	/// Represents constant line feed <c>'\n'</c>.
	/// </summary>
	private const byte Lf = (byte)'\n';

	/// <summary>
	/// Represents constant carriage return <c>'\r'</c>.
	/// </summary>
	private const byte Cr = (byte)'\r';


	/// <summary>
	/// Indicates the reader stream.
	/// </summary>
	private readonly FileStream _readerStream;

	/// <summary>
	/// Indicates whether the object is disposed.
	/// </summary>
	private bool _isDisposed;


	/// <summary>
	/// Initializes a <see cref="LibraryFileReader"/> instance.
	/// If the target library file doesn't exist, the constructor will automatically create one
	/// because the backing code uses option <see cref="FileMode.OpenOrCreate"/>.
	/// </summary>
	/// <param name="filePath">The file path.</param>
	/// <param name="bufferSize">The buffer size.</param>
	/// <param name="exists">Indicates whether the file exists.</param>
	/// <seealso cref="FileMode.OpenOrCreate"/>
	public LibraryFileReader(string filePath, int bufferSize, out bool exists)
	{
		(FilePath, BufferSize) = (filePath, bufferSize);
		exists = File.Exists(filePath);
		_readerStream = new(
			filePath,
			FileMode.OpenOrCreate,
			FileAccess.Read,
			FileShare.Read,
			bufferSize,
			FileOptions.Asynchronous | FileOptions.SequentialScan
		);
	}


	/// <summary>
	/// Indicates the file path.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Indicates the buffer size.
	/// </summary>
	public int BufferSize { get; }


	/// <summary>
	/// Initializes a <see cref="LibraryFileReader"/> instance via the specified file path.
	/// </summary>
	/// <param name="path">The file path.</param>
	/// <param name="exists">Indicates whether the file exists.</param>
	public LibraryFileReader(string path, out bool exists) : this(path, 4096 * 4, out exists)
	{
	}


	/// <summary>
	/// Count the number of lines of the file.
	/// </summary>
	/// <returns>The number of lines.</returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public ulong CountLines()
	{
		const int MegaByte = 1024 * 1024;
		var lineCount = 0UL;
		var previous = (byte)0;

		using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, MegaByte);
		var buffer = ArrayPool<byte>.Shared.Rent(MegaByte);
		try
		{
			while (true)
			{
				var bytesRead = fs.Read(buffer);
				if (bytesRead == 0)
				{
					break;
				}

				var span = buffer.AsSpan(0, bytesRead);
				if (Vector.IsHardwareAccelerated)
				{
					processVectorized(span, ref lineCount, ref previous);
				}
				else
				{
					processFallback(span, ref lineCount, ref previous);
				}
			}

			// The last line may not participate in checking.
			//if (previous == Cr || previous == Lf)
			//{
			//	lineCount++;
			//}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
		return lineCount;


		static void processFallback(ReadOnlySpan<byte> data, ref ulong lineCount, ref byte prevChar)
		{
			foreach (var b in data)
			{
				if (b == Lf)
				{
					if (prevChar == Cr)
					{
						lineCount++;
					}
				}
				else if (prevChar == Cr)
				{
					lineCount++;
				}
				prevChar = b;
			}
		}

		static void processVectorized(ReadOnlySpan<byte> data, ref ulong lineCount, ref byte prevChar)
		{
			var vectorSize = Vector<byte>.Count;
			var localCount = 0UL;
			fixed (byte* ptr = data)
			{
				var i = 0;
				for (; i <= data.Length - vectorSize; i += vectorSize)
				{
					var vector = Unsafe.ReadUnaligned<Vector<byte>>(ptr + i);
					var lfVector = new Vector<byte>(Lf);
					var crVector = new Vector<byte>(Cr);
					var lfMask = Vector.Equals(vector, lfVector);
					var crMask = Vector.Equals(vector, crVector);
					for (var j = 0; j < vectorSize; j++)
					{
						var current = vector[j];
						if (lfMask[j] == byte.MaxValue)
						{
							if (prevChar == Cr)
							{
								localCount++;
							}
						}
						else if (prevChar == Cr)
						{
							localCount++;
						}

						prevChar = current;
					}
				}

				for (; i < data.Length; i++)
				{
					var current = data[i];
					if (current == Lf)
					{
						if (prevChar == Cr)
						{
							localCount++;
						}
					}
					else if (prevChar == Cr)
					{
						localCount++;
					}

					prevChar = current;
				}
			}

			Interlocked.Add(ref lineCount, localCount);
		}
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		await _readerStream.DisposeAsync();
		_isDisposed = true;
	}

	/// <summary>
	/// Asynchronously read for the specified lines.
	/// </summary>
	/// <param name="startLine">Indicates the start line index.</param>
	/// <param name="endLine">Indicates the end line index.</param>
	/// <param name="cancellationToken">Indicates the cancellation token.</param>
	/// <returns>An asynchronous enumerator instance.</returns>
	/// <exception cref="OperationCanceledException">Throws when <paramref name="cancellationToken"/> is requested.</exception>
	public async IAsyncEnumerable<string> ReadLinesRangeAsync(
		ulong startLine,
		ulong endLine,
		[EnumeratorCancellation] CancellationToken cancellationToken = default
	)
	{
		var buffer = new byte[BufferSize];
		var remainingBytes = new List<byte>(256);
		var currentLineNumber = 1UL;
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Read the block asynchronously.
			var bytesRead = await _readerStream.ReadAsync(buffer, cancellationToken);
			if (bytesRead == 0)
			{
				break;
			}

			// To parse the lines.
			var (lines, newRemaining) = parseLines(buffer.AsMemory(0, bytesRead), remainingBytes);

			// Hanlde for the range of valid line number.
			foreach (var line in lines)
			{
				if (++currentLineNumber > endLine)
				{
					// Unexpected end-of-line.
					yield break;
				}

				if (currentLineNumber > startLine)
				{
					yield return line;
				}
			}

			remainingBytes = newRemaining;
			if (currentLineNumber >= endLine)
			{
				break;
			}
		}

		// Handle for the last.
		if (remainingBytes.Count > 0 && currentLineNumber < endLine)
		{
			currentLineNumber++;
			if (currentLineNumber > startLine)
			{
				yield return Encoding.UTF8.GetString(remainingBytes.AsSpan());
			}
		}


		static (List<string> Lines, List<byte> Remaining) parseLines(ReadOnlyMemory<byte> data, List<byte> previousRemaining)
		{
			var lines = new List<string>();
			var currentLine = new List<byte>(previousRemaining);
			var span = data.Span;
			for (var i = 0; i < span.Length; i++)
			{
				var b = span[i];
				if (b == (byte)'\n')
				{
					if (currentLine.Count > 0 && currentLine[^1] == (byte)'\r')
					{
						currentLine.RemoveAt(currentLine.Count - 1);
					}

					lines.Add(Encoding.UTF8.GetString(currentLine.AsSpan()));
					currentLine.Clear();
				}
				else
				{
					currentLine.Add(b);
				}
			}
			return (lines, currentLine);
		}
	}

	/// <summary>
	/// Loads the puzzles line by line, and enumerate them without loading them together.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>An enumerator object that allows iterating values asynchronously.</returns>
	/// <exception cref="OperationCanceledException">Throws when <paramref name="cancellationToken"/> is requested.</exception>
	public IAsyncEnumerable<string> ReadAllLinesAsync(CancellationToken cancellationToken = default)
		=> ReadLinesRangeAsync(1, ulong.MaxValue, cancellationToken: cancellationToken);
}
