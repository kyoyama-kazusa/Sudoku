namespace Sudoku.Analytics;

/// <summary>
/// Provides with a solving step that describes for a technique usage,
/// with conclusions and detail data for the corresponding technique pattern.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="IDrawable.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="IDrawable.Views" path="/summary"/></param>
/// <param name="options">
/// Indicates an optional instance that provides with extra information for a step searcher.
/// This instance can be used for checking some extra information about a step such as notations to a cell, candidate, etc.
/// </param>
[TypeImpl(
	TypeImplFlags.AllObjectMethods | TypeImplFlags.AllEqualityComparisonOperators | TypeImplFlags.Equatable,
	OtherModifiersOnEquals = "sealed",
	OtherModifiersOnGetHashCode = "sealed",
	OtherModifiersOnToString = "sealed",
	OtherModifiersOnEquatableEquals = "virtual")]
public abstract partial class Step(
	[Property] StepConclusions conclusions,
	[Property] View[]? views,
	[Property] StepGathererOptions options
) :
	IComparable<Step>,
	IComparisonOperators<Step, Step, bool>,
	IDrawable,
	IEquatable<Step>,
	IFormattable,
	IStep<Step>
{
	/// <summary>
	/// Indicates whether the step is an assignment. The possible result values are:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>The step is assignment, meaning all conclusions are assignment ones</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>The step is elimination, meaning all conclusions are elimination ones</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>The step contains mixed conclusion types</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <exception cref="NotSupportedException">Throws when the step contains no conclusions.</exception>
	public bool? IsAssignment
		=> Conclusions.Span.Aggregate(0, static (interim, next) => interim | (next.ConclusionType == Assignment ? 1 : 2)) switch
		{
			1 => true,
			2 => false,
			3 => null,
			_ => throw new NotSupportedException(SR.ExceptionMessage("StepContainsNoConclusions"))
		};

	/// <summary>
	/// Indicates the English name of the technique.
	/// </summary>
	public virtual string EnglishName => Code.EnglishName;

	/// <summary>
	/// Indicates the difficulty of this technique step.
	/// </summary>
	/// <remarks>
	/// Generally this property holds the default and basic difficulty of the step.
	/// If the step's difficulty rating requires multiple factors, this property will provide with a basic difficulty value
	/// as elementary and default rating value; other factors will be given in the other property <see cref="Factors"/>.
	/// </remarks>
	/// <seealso cref="Factors"/>
	public abstract int BaseDifficulty { get; }

	/// <summary>
	/// Indicates the total difficulty of the technique step. This value is the total sum of merged result from two properties
	/// <see cref="BaseDifficulty"/> and <see cref="Factors"/>.
	/// </summary>
	/// <seealso cref="BaseDifficulty"/>
	/// <seealso cref="Factors"/>
	/// <seealso cref="Factor"/>
	public int Difficulty => BaseDifficulty + Factors.Sum(this);

	/// <summary>
	/// Indicates the string representation of the conclusions of the step.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	public string ConclusionText => Options.Converter.ConclusionConverter(Conclusions.Span);

	/// <summary>
	/// The technique code of this instance used for comparison (e.g. search for specified puzzle that contains this technique).
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	public abstract Technique Code { get; }

	/// <summary>
	/// The difficulty level of this step.
	/// </summary>
	/// <remarks>
	/// Although the type of this property is marked <see cref="FlagsAttribute"/>,
	/// we still can't set multiple flag values into the result. The flags are filtered
	/// during generating puzzles.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// Throws when the target difficulty level is <see cref="DifficultyLevel.Unknown"/>.
	/// </exception>
	/// <seealso cref="FlagsAttribute"/>
	public DifficultyLevel DifficultyLevel
		=> Code.GetDifficultyLevel() is var level and not 0
			? level
			: throw new InvalidOperationException(SR.ExceptionMessage("TechniqueLevelCannotBeDetermined"));

	/// <summary>
	/// Represents a type that describes which mode this step can be created in a puzzle grid.
	/// </summary>
	public abstract PencilmarkVisibility PencilmarkType { get; }

	/// <summary>
	/// Indicates all interpolations used by description information to the current step, stored in resource dictionary.
	/// </summary>
	public virtual InterpolationArray Interpolations => [];

	/// <summary>
	/// Represents a collection of factors that describes the difficulty rating on extra values.
	/// </summary>
	public virtual FactorArray Factors => [];

	/// <summary>
	/// Indicates all digits used in the corresponding pattern of the current step.
	/// </summary>
	public abstract Mask DigitsUsed { get; }

	/// <summary>
	/// <para>Indicates whether property <see cref="FormatTypeIdentifier"/> will inherit from base type.</para>
	/// <para>
	/// By default, this property is <see langword="false"/>, meaning technique resource key must match its containing type.
	/// </para>
	/// <para>
	/// If there's no corresponding resource found, <see cref="ToString(IFormatProvider?)"/>
	/// and other methods in a same method group will fail to output information,
	/// and return default value same as <see cref="ToSimpleString(IFormatProvider?)"/>.
	/// </para>
	/// </summary>
	/// <seealso cref="FormatTypeIdentifier"/>
	/// <seealso cref="ToString(IFormatProvider?)"/>
	/// <seealso cref="ToSimpleString(IFormatProvider?)"/>
	protected virtual bool TechniqueResourceKeyInheritsFromBase => false;

	/// <summary>
	/// Indicates the identifier of this type. This property will be used in resource.
	/// </summary>
	protected string FormatTypeIdentifier => (TechniqueResourceKeyInheritsFromBase ? GetType().BaseType! : GetType()).Name;

	/// <inheritdoc/>
	ReadOnlyMemory<View> IDrawable.Views => Views;

	/// <summary>
	/// Indicates the resource key that can access description to the current instance.
	/// </summary>
	private string TechniqueResourceKey => $"TechniqueFormat_{FormatTypeIdentifier}";


	/// <summary>
	/// Compares two <see cref="Step"/> instances, determining which one is greater.
	/// </summary>
	/// <param name="other">The other object to be compared.</param>
	/// <returns>
	/// An <see cref="int"/> value indicating the result. The comparison rule is:
	/// <list type="number">
	/// <item>If the argument <paramref name="other"/> is <see langword="null"/>, return -1.</item>
	/// <item>
	/// If the argument <paramref name="other"/> isn't <see langword="null"/>, compare the technique used.
	/// If the code is greater, the instance will be greater.
	/// </item>
	/// </list>
	/// The rule (2) can also be replaced with customized logic
	/// if you want to make the comparison perform better and more strict.
	/// </returns>
	/// <remarks>
	/// <para>
	/// Please note that the argument can be <see langword="null"/>, which is expected. If the argument is not of the same type
	/// as <see langword="this"/>, we should return 1 to describe the comparison is not successful
	/// (the number 1 indicates <see langword="this"/> is greater).
	/// </para>
	/// <para>In addition, the return value must be -1, 0 or 1; otherwise, an unexpected behavior might be raised.</para>
	/// </remarks>
	public virtual int CompareTo(Step? other) => other is null ? -1 : Math.Sign(Code - other.Code);

	/// <summary>
	/// Try to fetch the name of this technique step, with the specified culture.
	/// </summary>
	/// <param name="formatProvider">The culture information provider instance.</param>
	/// <returns>The string representation.</returns>
	public virtual string GetName(IFormatProvider? formatProvider) => Code.GetName(GetCulture(formatProvider));

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(IFormatProvider? formatProvider)
		=> GetResourceFormat(null) is null
			? ToSimpleString(formatProvider)
			: GetCulture(formatProvider) is var culture && SR.Get("_Token_Colon", culture) is var colonToken
				? Interpolations[culture] switch
				{
					{ Values: { } formatArgs }
						=> $"{GetName(formatProvider)}{colonToken}{FormatDescription(culture, formatArgs)} => {ConclusionText}",
					_
						=> $"{GetName(formatProvider)}{colonToken}{FormatTypeIdentifier} => {ConclusionText}"
				}
				: throw new();

	/// <summary>
	/// Gets the string representation for the current step, describing only its technique name and conclusions.
	/// </summary>
	/// <param name="formatProvider">The culture information.</param>
	/// <returns>The string value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToSimpleString(IFormatProvider? formatProvider) => $"{GetName(formatProvider)} => {ConclusionText}";

	/// <summary>
	/// Compares the real name of the step to the specified one. This method is to distinct names on displaying in UI.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="formatProvider">
	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)" path="/param[@name='formatProvider']"/>
	/// </param>
	/// <returns>An <see cref="int"/> value indicating which one is logically larger.</returns>
	/// <remarks>
	/// <para>
	/// Some techniques may not contain a correct order of comparison on its name.
	/// For example, in Chinese, digit characters <c>2</c> (i.e. "&#20108;") and <c>3</c> (i.e. "&#19977;")
	/// won't satisfy the comparison rule on the default order of their own Unicode value.
	/// <c>2</c> is for <c>U + 4e8c</c>, while <c>3</c> is for <c>U + 4e09</c>.
	/// In logic, <c>U + 4e8c</c> (2) has a larger Unicode value with <c>4e09</c> (3).
	/// However, in meaning of such text, <c>3</c> is greater than <c>2</c>. This method will handle on this case.
	/// </para>
	/// <para>By default, this method only checks for the Unicode order of two strings (default string comparison rule).</para>
	/// </remarks>
	protected internal virtual int NameCompareTo(Step other, IFormatProvider? formatProvider)
	{
		var left = GetName(formatProvider);
		var right = other.GetName(formatProvider);
		return left.CompareTo(right);
	}

	/// <summary>
	/// Try to get the current culture used. The return value won't be <see langword="null"/>.
	/// </summary>
	/// <param name="formatProvider">
	/// The format provider instance.
	/// The value can be <see langword="null"/> if you want to check for default culture used in <see cref="Options"/>.
	/// </param>
	/// <returns>The corresponding culture information.</returns>
	/// <seealso cref="Options"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected CultureInfo GetCulture(IFormatProvider? formatProvider)
		=> formatProvider as CultureInfo ?? Options.CurrentCulture;

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <summary>
	/// Try to format description of the current instance.
	/// </summary>
	/// <param name="culture">The culture information.</param>
	/// <param name="formatArguments">The format arguments.</param>
	/// <returns>The final result.</returns>
	/// <exception cref="ResourceNotFoundException">Throws when the specified culture doesn't contain the specified resource.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string FormatDescription(CultureInfo? culture, params ReadOnlySpan<string> formatArguments)
		=> GetResourceFormat(culture) is { } p
			? string.Format(culture, p, formatArguments)
			: throw new ResourceNotFoundException(typeof(Step).Assembly, TechniqueResourceKey, culture);

	/// <summary>
	/// Returns the format of the specified culture.
	/// The return value can be <see langword="null"/> if the step doesn't contain an equivalent resource key.
	/// </summary>
	/// <param name="culture">Indicates the current culture used.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string? GetResourceFormat(CultureInfo? culture)
		=> SR.TryGet(TechniqueResourceKey, out var resource, culture) ? resource : null;
}
