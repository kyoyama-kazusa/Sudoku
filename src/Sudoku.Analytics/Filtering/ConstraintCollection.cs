namespace Sudoku.Filtering;

/// <summary>
/// Represents a constraint collection.
/// </summary>
public sealed class ConstraintCollection :
	List<Constraint>,
	IAdditionOperators<ConstraintCollection, Constraint?, ConstraintCollection>,
	ICloneable,
	IElementAtMethod<ConstraintCollection, Constraint>,
	IFirstLastMethod<ConstraintCollection, Constraint>,
	IHasMethod<ConstraintCollection, Constraint>,
	ISubtractionOperators<ConstraintCollection, Constraint?, ConstraintCollection>,
	ISelectMethod<ConstraintCollection, Constraint>,
	ISliceMethod<ConstraintCollection, Constraint>,
	IOfTypeMethod<ConstraintCollection, Constraint>,
	IWhereMethod<ConstraintCollection, Constraint>
{
	/// <inheritdoc cref="List{T}()"/>
	public ConstraintCollection()
	{
	}

	/// <inheritdoc cref="List{T}(int)"/>
	public ConstraintCollection(int capacity) : base(capacity)
	{
	}

	/// <summary>
	/// Create a new instance and copy elements.
	/// </summary>
	/// <param name="other">The other collection.</param>
	public ConstraintCollection(ConstraintCollection other) : base(other)
	{
	}


	/// <inheritdoc/>
	public bool Has<TConstraint>() where TConstraint : Constraint
	{
		foreach (var element in this)
		{
			if (element is TConstraint)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// For a given <see cref="ConstraintCheckingContext"/>,
	/// determine whether the specified grid and its related analysis result satisfy the current limited constraint rules.
	/// </summary>
	/// <param name="context">The context to be used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool IsValidFor(ConstraintCheckingContext context)
	{
		foreach (var constraint in this)
		{
			if (!constraint.Check(context))
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>
	public Constraint? FirstOrDefault(Func<Constraint, bool> predicate)
	{
		foreach (var element in this)
		{
			if (predicate(element))
			{
				return element;
			}
		}
		return null;
	}

	/// <inheritdoc cref="ICloneable.Clone"/>
	public ConstraintCollection Clone()
	{
		var result = new ConstraintCollection();
		foreach (var constraint in this)
		{
			result.Add(constraint.Clone());
		}
		return result;
	}

	/// <summary>
	/// Filters the collection, only reserving constraints satisfying the specified condition.
	/// </summary>
	/// <param name="predicate">The condition to be satisfied.</param>
	/// <returns>A new collection that only contains constraints satisfying the specified condition.</returns>
	public ConstraintCollection Where(Func<Constraint, bool> predicate)
	{
		var result = new ConstraintCollection();
		foreach (var element in this)
		{
			if (predicate(element))
			{
				result.Add(element);
			}
		}
		return result;
	}

	/// <inheritdoc cref="ISliceMethod{TSelf, TSource}.Slice(int, int)"/>
	public new ConstraintCollection Slice(int start, int count)
	{
		var result = new ConstraintCollection(count);
		for (var i = start; i < start + count; i++)
		{
			result.Add(this[i]);
		}
		return result;
	}

	/// <inheritdoc cref="IOfTypeMethod{TSelf, TSource}.OfType{TResult}"/>
	public ReadOnlySpan<TConstraint> OfType<TConstraint>() where TConstraint : Constraint
	{
		var result = new List<TConstraint>();
		foreach (var element in this)
		{
			if (element is TConstraint constraint)
			{
				result.Add(constraint);
			}
		}
		return result.AsSpan();
	}

	/// <inheritdoc cref="ISelectMethod{TSelf, TSource}.Select{TResult}(Func{TSource, TResult})"/>
	public ReadOnlySpan<TResult> Select<TResult>(Func<Constraint, TResult> selector)
	{
		var result = new List<TResult>(Count);
		foreach (var element in this)
		{
			result.AddRef(selector(element));
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Finds for the constraints that are of type <typeparamref name="TConstraint"/>.
	/// If multiple constraints or no constraints are found, an exception will be thrown; otherwise, the only value will be returned.
	/// </summary>
	/// <typeparam name="TConstraint">The type of desired constraint.</typeparam>
	/// <returns>The constraint found.</returns>
	/// <exception cref="InvalidOperationException">Throws when multiple or no values matched.</exception>
	public TConstraint OfTypeSingle<TConstraint>() where TConstraint : Constraint
	{
		var result = new List<TConstraint>(2);
		foreach (var element in this)
		{
			if (element is TConstraint constraint)
			{
				result.Add(constraint);
				if (result.Count == 2)
				{
					throw new InvalidOperationException("Multiple matches found.");
				}
			}
		}
		return result is [var onlyValue] ? onlyValue : throw new InvalidOperationException();
	}

	/// <summary>
	/// Finds for the constraints that are of type <typeparamref name="TConstraint"/>.
	/// If multiple constraints are found or no constraints are found, <see langword="null"/> will be returned;
	/// otherwise, the only value.
	/// </summary>
	/// <typeparam name="TConstraint">The type of desired constraint.</typeparam>
	/// <returns>The constraint found.</returns>
	public TConstraint? OfTypeSingleOrDefault<TConstraint>() where TConstraint : Constraint
	{
		var result = new List<TConstraint>(2);
		foreach (var element in this)
		{
			if (element is TConstraint constraint)
			{
				result.Add(constraint);
				if (result.Count == 2)
				{
					return null;
				}
			}
		}
		return result is [var onlyValue] ? onlyValue : null;
	}

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();

	/// <inheritdoc/>
	Constraint IElementAtMethod<ConstraintCollection, Constraint>.ElementAt(int index) => this[index];

	/// <inheritdoc/>
	Constraint IElementAtMethod<ConstraintCollection, Constraint>.ElementAt(Index index) => this[index];

	/// <inheritdoc/>
	Constraint? IElementAtMethod<ConstraintCollection, Constraint>.ElementAtOrDefault(int index)
		=> index >= 0 && index < Count ? this[index] : default;

	/// <inheritdoc/>
	Constraint? IElementAtMethod<ConstraintCollection, Constraint>.ElementAtOrDefault(Index index)
		=> index.GetOffset(Count) is var p && p >= 0 && p < Count ? this[p] : default;

	/// <inheritdoc/>
	Constraint IFirstLastMethod<ConstraintCollection, Constraint>.First() => this[0];

	/// <inheritdoc/>
	Constraint IFirstLastMethod<ConstraintCollection, Constraint>.First(Func<Constraint, bool> predicate)
		=> FirstOrDefault(predicate)!;

	/// <inheritdoc/>
	Constraint IFirstLastMethod<ConstraintCollection, Constraint>.FirstOrDefault() => this[0];

	/// <inheritdoc/>
	Constraint IFirstLastMethod<ConstraintCollection, Constraint>.FirstOrDefault(Constraint defaultValue) => this[0];

	/// <inheritdoc/>
	Constraint? IFirstLastMethod<ConstraintCollection, Constraint>.FirstOrDefault(Func<Constraint, bool> predicate)
		=> FirstOrDefault(predicate);

	/// <inheritdoc/>
	Constraint IFirstLastMethod<ConstraintCollection, Constraint>.FirstOrDefault(Func<Constraint, bool> predicate, Constraint defaultValue)
		=> FirstOrDefault(predicate) ?? defaultValue;

	/// <inheritdoc/>
	IEnumerable<Constraint> IWhereMethod<ConstraintCollection, Constraint>.Where(Func<Constraint, bool> predicate)
		=> Where(predicate);

	/// <inheritdoc/>
	IEnumerable<Constraint> ISliceMethod<ConstraintCollection, Constraint>.Slice(int start, int count) => Slice(start, count);

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<ConstraintCollection, Constraint>.Select<TResult>(Func<Constraint, TResult> selector)
		=> Select(selector).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> IOfTypeMethod<ConstraintCollection, Constraint>.OfType<TResult>() => OfType<TResult>().ToArray();


	/// <summary>
	/// Adds a new constraint into the current collection, or do nothing if <paramref name="value"/> is <see langword="null"/>.
	/// </summary>
	/// <param name="value">The constraint.</param>
	public void operator +=(Constraint? value)
	{
		if (value is not null)
		{
			Add(value);
		}
	}

	/// <summary>
	/// Removes a constraint from the current collection, or do nothing if <paramref name="value"/> is <see langword="null"/>.
	/// </summary>
	/// <param name="value">The constraint.</param>
	public void operator -=(Constraint? value)
	{
		if (value is not null)
		{
			Remove(value);
		}
	}


	/// <inheritdoc/>
	public static ConstraintCollection operator +(ConstraintCollection left, Constraint? right)
	{
		if (right is null)
		{
			return [.. left];
		}

		var result = new ConstraintCollection(left.Count + 1);
		foreach (var element in left)
		{
			if (element == right)
			{
				return [.. left];
			}
		}

		result.Add(right);
		return result;
	}

	/// <inheritdoc/>
	public static ConstraintCollection operator -(ConstraintCollection left, Constraint? right)
	{
		if (right is null)
		{
			return [.. left];
		}

		var result = new ConstraintCollection(left.Count - 1);
		foreach (var element in left)
		{
			if (element != right)
			{
				result.Add(element);
			}
		}
		return result;
	}
}
