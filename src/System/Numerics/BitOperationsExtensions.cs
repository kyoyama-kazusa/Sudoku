namespace System.Numerics;

/// <summary>
/// Provides extension methods on <see cref="BitOperations"/>.
/// </summary>
/// <seealso cref="BitOperations"/>
public static partial class BitOperationsExtensions
{
	/// <summary>
	/// Represents the table that describes the number of bits set for all possible 256 values of <see cref="byte"/>.
	/// </summary>
	private static readonly byte[] BitCountLut = new byte[256];


	/// <include file="../../global-doc-comments.xml" path="g/static-constructor"/>
	static BitOperationsExtensions()
	{
		for (var i = 0; i < 256; i++)
		{
			var value = i;
			var count = (byte)0;
			while (value != 0)
			{
				count++;
				value &= value - 1;
			}
			BitCountLut[i] = count;
		}
	}


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(sbyte @this)
	{
		/// <summary>
		/// Indicates all positions (indices) whose corresponding bits are set 1.
		/// </summary>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[sbyte.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = sbyte.TrailingZeroCount(@this);
					@this &= (sbyte)(@this - 1);
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => ((uint)@this).SetAt(order);

		/// <summary>
		/// Returns an enumerator type that can iterate on each position (index) whose cooresponding bit is set 1.
		/// </summary>
		/// <returns>An enumerator object that can iterate on each position.</returns>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(byte @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[byte.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = byte.TrailingZeroCount(@this);
					@this &= (byte)(@this - 1);
				}
				return result;
			}
		}


		/// <summary>
		/// Find an index of the binary representation of a value after the specified index whose bit is set <see langword="true"/>.
		/// </summary>
		/// <param name="index">The index. The value will be automatically plus 1 in loop. Don't pass the value added 1.</param>
		/// <returns>The index.</returns>
		public int GetNextSet(int index)
		{
			for (var i = index + 1; i < 8; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Skip the specified number of set bits and iterate on the integer with other set bits.
		/// </summary>
		/// <param name="setBitPosCount">Indicates how many set bits you want to skip to iterate.</param>
		/// <returns>The byte value that only contains the other set bits.</returns>
		/// <remarks>
		/// For example:
		/// <code><![CDATA[
		/// byte value = 0b00010111;
		/// foreach (int bitPos in value.SkipSetBit(2))
		/// {
		///     yield return bitPos + 1;
		/// }
		/// ]]></code>
		/// You will get 3 and 5, because all set bit positions are 0, 1, 2 and 4, and we have skipped
		/// two of them, so the result set bit positions to iterate on are only 2 and 4.
		/// </remarks>
		public byte SkipSetBit(int setBitPosCount)
		{
			var result = @this;
			for (var (i, count) = (0, 0); i < 8; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					result &= (byte)~(1 << i);

					if (++count == setBitPosCount)
					{
						break;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Get an <see cref="int"/> value, indicating that the absolute position of all set bits with the specified set bit order.
		/// </summary>
		/// <param name="order">The number of the order of set bits.</param>
		/// <returns>The position.</returns>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => ((uint)@this).SetAt(order);

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref byte @this)
	{
		/// <summary>
		/// <para>Reverse all bits in a specified value.</para>
		/// <para>Note that the value is passed by <b>reference</b> though the method is an extension method, and returns nothing.</para>
		/// </summary>
		public void ReverseBits()
		{
			@this = (byte)(@this >> 1 & 0x55 | (@this & 0x55) << 1);
			@this = (byte)(@this >> 2 & 0x33 | (@this & 0x33) << 2);
			@this = (byte)(@this >> 4 | @this << 4);
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(short @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[short.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = short.TrailingZeroCount(@this);
					@this &= (short)(@this - 1);
				}
				return result;
			}
		}


		/// <inheritdoc cref="GetNextSet(byte, int)"/>
		public int GetNextSet(int index)
		{
			for (var i = index + 1; i < 16; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="SkipSetBit(byte, int)"/>
		public short SkipSetBit(int setBitPosCount)
		{
			var result = @this;
			for (var (i, count) = (0, 0); i < 16; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					result &= (short)~(1 << i);

					if (++count == setBitPosCount)
					{
						break;
					}
				}
			}
			return result;
		}

		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => ((uint)@this).SetAt(order);

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref short @this)
	{
		/// <inheritdoc cref="ReverseBits(ref byte)"/>
		public void ReverseBits()
		{
			@this = (short)(@this >> 1 & 0x5555 | (@this & 0x5555) << 1);
			@this = (short)(@this >> 2 & 0x3333 | (@this & 0x3333) << 2);
			@this = (short)(@this >> 4 & 0x0F0F | (@this & 0x0F0F) << 4);
			@this = (short)(@this >> 8 | @this << 8);
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ushort @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[ushort.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = ushort.TrailingZeroCount(@this);
					@this &= (ushort)(@this - 1);
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => ((uint)@this).SetAt(order);

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(int @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[PopCount((uint)@this)], 0);
				while (@this != 0)
				{
					result[p++] = TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="GetNextSet(byte, int)"/>
		public int GetNextSet(int index)
		{
			for (var i = index + 1; i < 32; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="SkipSetBit(byte, int)"/>
		public int SkipSetBit(int setBitPosCount)
		{
			var result = @this;
			for (var (i, count) = (0, 0); i < 32; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					result &= ~(1 << i);

					if (++count == setBitPosCount)
					{
						break;
					}
				}
			}
			return result;
		}

		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => ((uint)@this).SetAt(order);

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref int @this)
	{
		/// <inheritdoc cref="ReverseBits(ref byte)"/>
		public void ReverseBits()
		{
			@this = @this >> 1 & 0x55555555 | (@this & 0x55555555) << 1;
			@this = @this >> 2 & 0x33333333 | (@this & 0x33333333) << 2;
			@this = @this >> 4 & 0x0F0F0F0F | (@this & 0x0F0F0F0F) << 4;
			@this = @this >> 8 & 0x00FF00FF | (@this & 0x00FF00FF) << 8;
			@this = @this >> 16 | @this << 16;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(uint @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[uint.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order)
		{
			if (Bmi2.IsSupported)
			{
				return TrailingZeroCount(Bmi2.ParallelBitDeposit(1U << order, @this)) is var result and not FallbackConstants.@int
					? result
					: -1;
			}

			for (int i = 0, count = -1; i < sizeof(uint) << 3; i++, @this >>= 1)
			{
				if ((@this & 1) != 0 && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int32Enumerator GetEnumerator() => new((int)@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(long @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[long.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}

		/// <inheritdoc cref="GetNextSet(byte, int)"/>
		public int GetNextSet(int index)
		{
			for (var i = index + 1; i < 64; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="SkipSetBit(byte, int)"/>
		public long SkipSetBit(int setBitPosCount)
		{
			var result = @this;
			for (var (i, count) = (0, 0); i < 64; i++)
			{
				if ((@this >> i & 1) != 0)
				{
					result &= ~(1 << i);

					if (++count == setBitPosCount)
					{
						break;
					}
				}
			}
			return result;
		}

		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order) => SetAt((ulong)@this, order);

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int64Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref long @this)
	{
		/// <inheritdoc cref="ReverseBits(ref byte)"/>
		public void ReverseBits()
		{
			@this = @this >> 1 & 0x55555555_55555555L | (@this & 0x55555555_55555555L) << 1;
			@this = @this >> 2 & 0x33333333_33333333L | (@this & 0x33333333_33333333L) << 2;
			@this = @this >> 4 & 0x0F0F0F0F_0F0F0F0FL | (@this & 0x0F0F0F0F_0F0F0F0FL) << 4;
			@this = @this >> 8 & 0x00FF00FF_00FF00FFL | (@this & 0x00FF00FF_00FF00FFL) << 8;
			@this = @this >> 16 & 0x0000FFFF_0000FFFFL | (@this & 0x0000FFFF_0000FFFFL) << 16;
			@this = @this >> 32 | @this << 32;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ulong @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[ulong.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public int SetAt(int order)
		{
			if (Bmi2.X64.IsSupported)
			{
				var result = TrailingZeroCount(Bmi2.X64.ParallelBitDeposit(1UL << order, @this));
				return result != FallbackConstants.@long ? result : -1;
			}

			if (Bmi2.IsSupported)
			{
				var low = (uint)@this;
				var lowCount = PopCount(low);
				if (order < lowCount)
				{
					var src32 = 1U << order;
					var deposited = Bmi2.ParallelBitDeposit(src32, low);
					var p = TrailingZeroCount(deposited);
					return p != FallbackConstants.@int ? p : -1;
				}
				else
				{
					var high = (uint)(@this >> FallbackConstants.@int);
					var src32 = 1U << (order - lowCount);
					var deposited = Bmi2.ParallelBitDeposit(src32, high);
					var p = TrailingZeroCount(deposited);
					return p != FallbackConstants.@int ? FallbackConstants.@int + p : -1;
				}
			}

			// Generic fallback when neither BMI2.X64 nor BMI2 is available
			// (e.g. arm64). The previous binary-search implementation used a
			// uint mask, which could not reach bits >= 32 of the ulong input
			// — for inputs with set bits in both the low and high halves it
			// returned a position from the low half plus the saturating bits
			// of the unused high-half iterations, instead of the actual
			// position of the requested set bit.
			if ((uint)order >= (uint)PopCount(@this))
			{
				return -1;
			}

			var t = @this;
			for (var i = 0; t != 0; i++, t &= t - 1)
			{
				if (i == order)
				{
					return TrailingZeroCount(t);
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int64Enumerator GetEnumerator() => new((long)@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Int128 @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[(int)Int128.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = (int)Int128.TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public unsafe int SetAt(int order)
		{
			for (int i = 0, count = -1; i < sizeof(Int128) << 3; i++, @this >>= 1)
			{
				if ((@this & 1) != 0 && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int128Enumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(UInt128 @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[(int)UInt128.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = (int)UInt128.TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public unsafe int SetAt(int order)
		{
			for (int i = 0, count = -1; i < sizeof(UInt128) << 3; i++, @this >>= 1)
			{
				if ((@this & 1) != 0 && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public Int128Enumerator GetEnumerator() => new((Int128)@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(nint @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[(int)nint.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = (int)nint.TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public unsafe int SetAt(int order)
		{
			for (int i = 0, count = -1; i < sizeof(nint) << 3; i++, @this >>= 1)
			{
				if ((@this & 1) != 0 && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public NumericIntPtrEnumerator GetEnumerator() => new(@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(nuint @this)
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == 0)
				{
					return [];
				}

				var (result, p) = (new int[(int)nuint.PopCount(@this)], 0);
				while (@this != 0)
				{
					result[p++] = (int)nuint.TrailingZeroCount(@this);
					@this &= @this - 1;
				}
				return result;
			}
		}


		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(2)]
		public unsafe int SetAt(int order)
		{
			for (int i = 0, count = -1; i < sizeof(nuint) << 3; i++, @this >>= 1)
			{
				if ((@this & 1) != 0 && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}

		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(2)]
		public NumericIntPtrEnumerator GetEnumerator() => new((nint)@this);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of instance.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(T @this) where T : IBitwiseOperators<T, T, T>, INumber<T>, IShiftOperators<T, int, T>
	{
		/// <inheritdoc cref="SetAt(byte, int)"/>
		[OverloadResolutionPriority(1)]
		public unsafe int SetAt(int order)
		{
			for (int i = 0, count = -1; i < sizeof(T) << 3; i++, @this >>= 1)
			{
				if ((@this & T.MultiplicativeIdentity) != T.AdditiveIdentity && ++count == order)
				{
					return i;
				}
			}
			return -1;
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="T">The type of instance.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<T>(T @this) where T : IBinaryInteger<T>
	{
		/// <inheritdoc cref="extension(sbyte).AllSets"/>
		public ReadOnlySpan<int> AllSets
		{
			get
			{
				if (@this == T.Zero)
				{
					return [];
				}

				var (result, p) = (new int[int.CreateChecked(T.PopCount(@this))], 0);
				while (@this != T.Zero)
				{
					result[p++] = int.CreateChecked(T.TrailingZeroCount(@this));
					@this &= @this - T.One;
				}
				return result;
			}
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension<T>(T @this) where T : IBitwiseOperators<T, T, T>, IBinaryInteger<T>, IShiftOperators<T, int, T>
	{
		/// <inheritdoc cref="GetEnumerator(sbyte)"/>
		[OverloadResolutionPriority(1)]
		public unsafe GenericIntegerEnumerator<T> GetEnumerator() => new(@this, sizeof(T) << 3);
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(byte[] @this)
	{
		/// <summary>
		/// Try to get set bit offset at the specified index.
		/// </summary>
		/// <param name="k">The index.</param>
		/// <returns>The result.</returns>
		public int SetBitAt(int k)
		{
			var bitOffset = 0;
			if (Vector.IsHardwareAccelerated && @this.Length >= Vector<byte>.Count)
			{
				var i = 0;
				var vectorSize = Vector<byte>.Count;
				for (; i <= @this.Length - vectorSize; i += vectorSize)
				{
					var vec = new Vector<byte>(@this, i);
					var sum = 0;
					for (var j = 0; j < vectorSize; j++)
					{
						sum += BitCountLut[vec[j]];
					}

					if (k <= sum)
					{
						for (var j = 0; j < vectorSize; j++)
						{
							int c = BitCountLut[vec[j]];
							if (k <= c)
							{
								return bitOffset + findKthBitInByte(vec[j], k);
							}
							k -= c;
							bitOffset += 8;
						}
					}
					else
					{
						k -= sum;
						bitOffset += vectorSize * 8;
					}
				}

				// Remainder.
				for (; i < @this.Length; i++)
				{
					int c = BitCountLut[@this[i]];
					if (k <= c)
					{
						return bitOffset + findKthBitInByte(@this[i], k);
					}
					k -= c;
					bitOffset += 8;
				}
			}
			else
			{
				for (var i = 0; i < @this.Length; i++)
				{
					int c = BitCountLut[@this[i]];
					if (k <= c)
					{
						return bitOffset + findKthBitInByte(@this[i], k);
					}
					k -= c;
					bitOffset += 8;
				}
			}

			// Not found.
			return -1;


			static int findKthBitInByte(int b, int k)
			{
				for (var i = 0; i < 8; i++)
				{
					if (((b >> i) & 1) != 0 && --k == 0)
					{
						return i;
					}
				}
				return -1;
			}
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(BitOperations)
	{
		/// <summary>
		/// Pops two bits from the integer.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="second">The second position set 1.</param>
		/// <returns>The first position set 1.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopTwo(uint value, out int second)
		{
			var lowMask = value & (uint)-(int)value;
			var result = Log2(lowMask);
			value ^= lowMask;
			second = Log2(value);
			return result;
		}

		/// <inheritdoc cref="extension(BitOperations).PopTwo(uint, out int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopTwo(ulong value, out int second)
		{
			var lowMask = value & (ulong)-(long)value;
			var result = Log2(lowMask);
			value ^= lowMask;
			second = Log2(value);
			return result;
		}

		/// <inheritdoc cref="extension(BitOperations).PopTwo(uint, out int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PopTwo(nuint value, out int second)
		{
			var lowMask = value & (nuint)(-(nint)value);
			var result = Log2(lowMask);
			value ^= lowMask;
			second = Log2(value);
			return result;
		}
	}
}
