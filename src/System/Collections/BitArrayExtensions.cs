namespace System.Collections;

/// <summary>
/// Provides extension methods on <see cref="BitArray"/>.
/// </summary>
/// <seealso cref="BitArray"/>
public static class BitArrayExtensions
{
	/// <summary>
	/// Represents a field to be used as table lookup.
	/// </summary>
	private static readonly Vector128<byte> NibblePopCount = Vector128.Create((byte)0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4);


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(BitArray @this)
	{
		/// <summary>
		/// Get the cardinality of the specified <see cref="BitArray"/>,
		/// indicating the total number of bits set <see langword="true"/>.
		/// </summary>
		public int Cardinality
		{
			get
			{
				var data = Entry.GetArrayField(@this);
				if (Avx2.IsSupported)
				{
					return popcnt_Avx2(data);
				}
				if (Sse2.IsSupported)
				{
					return popcnt_Sse2(data);
				}
				if (Popcnt.IsSupported)
				{
					return popcnt_Popcnt(data);
				}

				var sum = 0;
				foreach (var b in data)
				{
					sum += PopCount(b);
				}
				return sum;


				[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "<Pending>")]
				static unsafe int popcnt_Avx2(byte[] data)
				{
					var length = data.Length;
					var i = 0;
					var sum = 0;
					var lowMask = Vector256.Create((byte)0x0F);
					var lookup = NibblePopCount;
					fixed (byte* ptr = data)
					{
						for (; i + 32 <= length; i += 32)
						{
							var v = Avx.LoadVector256(ptr + i);

							// low nibble
							var lo = Avx2.And(v, lowMask);
							var popLo = Avx2.Shuffle(lookup, lo.GetLower()); // need 128-bit lanes
							var popLoHi = Avx2.Shuffle(lookup, lo.GetUpper());

							// high nibble
							var hi = Avx2.And(Avx2.ShiftRightLogical(v.AsUInt16(), 4).AsByte(), lowMask);
							var popHi = Avx2.Shuffle(lookup, hi.GetLower());
							var popHiHi = Avx2.Shuffle(lookup, hi.GetUpper());

							var sumLo = Avx2.Add(popLo, popHi);
							var sumHi = Avx2.Add(popLoHi, popHiHi);

							// horizontal sum
							sum += horizontalSum(sumLo);
							sum += horizontalSum(sumHi);
						}
					}
					for (; i < length; i++)
					{
						sum += PopCount(data[i]);
					}
					return sum;
				}

				static unsafe int popcnt_Sse2(byte[] data)
				{
					var length = data.Length;
					var i = 0;
					var sum = 0;
					var lowMask = Vector128.Create((byte)0x0F);
					var lookup = NibblePopCount;
					fixed (byte* ptr = data)
					{
						for (; i + 16 <= length; i += 16)
						{
							var v = Sse2.LoadVector128(ptr + i);
							var lo = Sse2.And(v, lowMask);
							var hi = Sse2.And(Sse2.ShiftRightLogical(v.AsUInt16(), 4).AsByte(), lowMask);
							var popLo = Ssse3.Shuffle(lookup, lo);
							var popHi = Ssse3.Shuffle(lookup, hi);
							var total = Sse2.Add(popLo, popHi);
							sum += horizontalSum(total);
						}
					}
					for (; i < length; i++)
					{
						sum += PopCount(data[i]);
					}
					return sum;
				}

				static unsafe int popcnt_Popcnt(byte[] data)
				{
					var i = 0;
					var sum = 0;
					if (Popcnt.X64.IsSupported)
					{
						fixed (byte* ptr = data)
						{
							for (; i + 8 <= data.Length; i += 8)
							{
								var value = Unsafe.ReadUnaligned<ulong>(ptr + i);
								sum += (int)Popcnt.X64.PopCount(value);
							}
						}
					}
					else
					{
						fixed (byte* ptr = data)
						{
							for (; i + 4 <= data.Length; i += 4)
							{
								var value = Unsafe.ReadUnaligned<uint>(ptr + i);
								sum += (int)Popcnt.PopCount(value);
							}
						}
					}
					for (; i < data.Length; i++)
					{
						sum += PopCount(data[i]);
					}
					return sum;
				}

				static int horizontalSum(Vector128<byte> v)
				{
					var result = 0;
					for (var i = 0; i < 16; i++)
					{
						result += v.GetElement(i);
					}
					return result;
				}
			}
		}


		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		//[Obsolete(DeprecatedMessages.ExtensionOperator_Equality, false)]
		public bool SequenceEqual([NotNullWhen(true)] BitArray? other)
			=> other is not null && @this.Length == other.Length
			&& Entry.GetArrayField(@this).SequenceEqual(Entry.GetArrayField(other));

		/// <summary>
		/// Try to get internal array field.
		/// </summary>
		/// <returns>The field.</returns>
		public byte[] GetInternalArrayField() => Entry.GetArrayField(@this);

		/// <summary>
		/// Slices the current <see cref="BitArray"/> instance.
		/// </summary>
		/// <param name="start">The start index.</param>
		/// <param name="count">The number.</param>
		/// <returns>The result.</returns>
		public BitArray Slice(int start, int count)
		{
			var result = new BitArray(count);
			for (var (i, j) = (start, 0); i < start + count; i++, j++)
			{
				result[j] = @this[i];
			}
			return result;
		}

		/// <summary>
		/// Slices the current <see cref="BitArray"/> instance.
		/// </summary>
		/// <param name="start">The start index.</param>
		/// <returns>The result.</returns>
		public BitArray Slice(int start) => @this.Slice(start, @this.Count - start);

		/// <summary>
		/// Performs bitwise-or operation with the other instance at the start position, without equivalent length of the other object.
		/// </summary>
		/// <param name="other">The other object.</param>
		/// <returns>The current instance.</returns>
		public BitArray AlignedOr(BitArray other)
		{
			if (other.Count == 0)
			{
				return @this;
			}

			var indexCount = (other.Count + 7) / 8;
			var internalBits = Entry.GetArrayField(@this);
			var otherInternalBits = Entry.GetArrayField(other);
			for (var i = 0; i < indexCount; i++)
			{
				internalBits[i] |= otherInternalBits[i];
			}
			return @this;
		}


		/// <summary>
		/// Performs bitwise-and assignment.
		/// </summary>
		/// <param name="other">The other instance.</param>
		public void operator &=(BitArray other) => @this.And(other);

		/// <summary>
		/// Performs bitwise-or assignment.
		/// </summary>
		/// <param name="other">The other instance.</param>
		public void operator |=(BitArray other) => @this.Or(other);

		/// <summary>
		/// Performs bitwise-exclusive-or assignment.
		/// </summary>
		/// <param name="other">The other instance.</param>
		public void operator ^=(BitArray other) => @this.Xor(other);


		/// <summary>
		/// Performs negation operation.
		/// </summary>
		/// <param name="value">The instance.</param>
		/// <returns>The negated result.</returns>
		public static BitArray operator ~(BitArray value)
		{
			var result = new BitArray(value);
			result.Not();
			return result;
		}

#if false
		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
		[OverloadResolutionPriority(1)]
		public static bool operator ==(BitArray? left, BitArray? right)
			=> (left, right) switch { (null, null) => true, (not null, not null) => left.SequenceEqual(right), _ => false };

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
		[OverloadResolutionPriority(1)]
		public static bool operator !=(BitArray? left, BitArray? right) => !(left == right);
#endif

		/// <summary>
		/// Performs bitwise-and operation.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The <see cref="BitArray"/> result.</returns>
		public static BitArray operator &(BitArray left, BitArray right)
		{
			var result = new BitArray(left);
			result &= right;
			return result;
		}

		/// <summary>
		/// Performs bitwise-or operation.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The <see cref="BitArray"/> result.</returns>
		public static BitArray operator |(BitArray left, BitArray right)
		{
			var result = new BitArray(left);
			result |= right;
			return result;
		}

		/// <summary>
		/// Performs bitwise-exclusive-or operation.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The <see cref="BitArray"/> result.</returns>
		public static BitArray operator ^(BitArray left, BitArray right)
		{
			var result = new BitArray(left);
			result ^= right;
			return result;
		}
	}
}

/// <summary>
/// Represents an entry to call internal fields on <see cref="BitArray"/>.
/// </summary>
/// <seealso cref="BitArray"/>
file static class Entry
{
	/// <summary>
	/// Try to fetch the internal field <c>m_array</c> in type <see cref="BitArray"/>.
	/// </summary>
	/// <param name="this">The list.</param>
	/// <returns>The reference to the internal field.</returns>
	/// <remarks>
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@name='others']"/>
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="//g/dotnet/version[@value='8']/feature[@name='unsafe-accessor']/target[@name='field-related-method']"/>
	/// </remarks>
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = LibraryIdentifiers.BitArray_Array)]
	public static extern safe ref byte[] GetArrayField(BitArray @this);
}
