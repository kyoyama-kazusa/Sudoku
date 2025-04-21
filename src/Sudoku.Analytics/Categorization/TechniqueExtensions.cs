namespace Sudoku.Categorization;

/// <summary>
/// Provides with extension methods on <see cref="Technique"/>.
/// </summary>
/// <seealso cref="Technique"/>
public static class TechniqueExtensions
{
	/// <summary>
	/// Represents <see langword="typeof"/>(<see cref="Technique"/>).
	/// </summary>
	private static readonly Type TypeOfTechnique = typeof(Technique);


	/// <summary>
	/// Provides extension members on <see cref="Technique"/>.
	/// </summary>
	extension(Technique @this)
	{
		/// <summary>
		/// Indicates whether the specified technique produces assignments.
		/// </summary>
		/// <remarks>
		/// <para>
		/// In mechanism, an assignment technique must produce assignment conclusions.
		/// However, some techniques can also produce them like BUG + 1, Discontinuous Nice Loop, etc..
		/// This method today won't return <see langword="true"/> for such techniques now, but this rule might be changed in the future.
		/// </para>
		/// <para>
		/// If you want to check whether the technique is a single, please call method <see cref="get_IsDirect(Technique)"/>
		/// or <see cref="get_IsSingle(Technique)"/> instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="get_IsDirect(Technique)"/>
		/// <seealso cref="get_IsSingle(Technique)"/>
		public bool IsAssignment => @this.GetGroup() is TechniqueGroup.Single or TechniqueGroup.ComplexSingle;

		/// <summary>
		/// Indicates whether the specified technique is a single technique.
		/// </summary>
		public bool IsSingle => @this.GetGroup() is TechniqueGroup.Single or TechniqueGroup.ComplexSingle;

		/// <summary>
		/// Indicates whether the specified technique is a technique that can only be produced in direct views.
		/// </summary>
		public bool IsDirect
			=> TypeOfTechnique
				.GetField(@this.ToString())!
				.GetCustomAttribute<TechniqueMetadataAttribute>()?
				.Features
				.HasFlag(TechniqueFeatures.DirectTechniques)
			?? false;

		/// <summary>
		/// Indicates whether the technique is last resort.
		/// </summary>
		public bool IsLastResort
			=> @this.GetGroup() is TechniqueGroup.BowmanBingo or TechniqueGroup.PatternOverlay
			or TechniqueGroup.Templating or TechniqueGroup.BruteForce;

		/// <summary>
		/// Indicates whether the specified technique supports for customization on difficulty values.
		/// </summary>
		public bool SupportsCustomizingDifficulty
			=> Enum.IsDefined(@this) && @this != Technique.None
			&& !@this.IsLastResort
			&& TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>() is
			{
				Rating: not int.MinValue,
				DifficultyLevel: not (DifficultyLevel)int.MinValue
			};

		/// <summary>
		/// Indicates whether the technique supports for Siamese rule.
		/// </summary>
		public bool SupportsSiamese
			=> TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()?.SupportsSiamese is true
			|| @this.GetGroup().SupportsSiamese;
	}


	/// <summary>
	/// Try to get the base difficulty value for the specified technique.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <param name="directRatingValue">
	/// An extra value that is defined in direct mode. If undefined, the argument will keep a same value as the return value.
	/// </param>
	/// <returns>The difficulty value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetDefaultRating(this Technique @this, out int directRatingValue)
	{
		var attribute = TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()!;
		directRatingValue = attribute.DirectRating == 0 ? attribute.Rating : attribute.DirectRating;
		return attribute.Rating;
	}

	/// <summary>
	/// Try to get the name of the current <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <param name="formatProvider">The culture information.</param>
	/// <returns>The name of the current technique.</returns>
	/// <exception cref="ResourceNotFoundException">Throws when the target name is not found in resource dictionary.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetName(this Technique @this, IFormatProvider? formatProvider)
		=> SR.TryGet(@this.ToString(), out var resource, formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture)
			? resource
			: SR.Get(@this.ToString(), SR.DefaultCulture);

	/// <summary>
	/// Try to get the English name of the current <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>The name of the current technique.</returns>
	/// <exception cref="ResourceNotFoundException">Throws when the target name is not found in resource dictionary.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetEnglishName(this Technique @this) => SR.Get(@this.ToString(), SR.DefaultCulture);

	/// <summary>
	/// Try to get the abbreviation of the current <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>The abbreviation of the current technique.</returns>
	/// <seealso cref="TechniqueGroup"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string? GetAbbreviation(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()?.Abbreviation
		?? (SR.TryGet($"TechniqueAbbr_{@this}", out var resource, SR.DefaultCulture) ? resource : @this.GetGroup().Abbreviation);

	/// <summary>
	/// Try to get all configured links to EnjoySudoku forum describing the current technique.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>All configured links.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string[] GetReferenceLinks(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()?.Links ?? [];

	/// <summary>
	/// Try to get all aliases of the current <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <param name="formatProvider">The culture information.</param>
	/// <returns>
	/// All possible aliases of the current technique.
	/// If the technique does not contain any aliases, the return value will be <see langword="null"/>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string[]? GetAliasedNames(this Technique @this, IFormatProvider? formatProvider)
		=> SR.TryGet($"TechniqueAlias_{@this}", out var resource, formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture)
			? resource.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			: null;

	/// <summary>
	/// Combine the technique with indirect technique, to produce a new <see cref="Technique"/> field, describing the complex single usage.
	/// For example, <see cref="Technique.CrosshatchingBlock"/> combining with <see cref="Technique.HiddenPair"/>
	/// will produce <see cref="Technique.HiddenPairCrosshatchingBlock"/>.
	/// </summary>
	/// <param name="this">
	/// The direct technique used. The value can be:
	/// <list type="bullet">
	/// <item><see cref="Technique.FullHouse"/></item>
	/// <item><see cref="Technique.CrosshatchingBlock"/></item>
	/// <item><see cref="Technique.CrosshatchingRow"/></item>
	/// <item><see cref="Technique.CrosshatchingColumn"/></item>
	/// <item><see cref="Technique.NakedSingle"/></item>
	/// </list>
	/// </param>
	/// <param name="indirect">
	/// The indirect technique used. The value can be ones of group <see cref="TechniqueGroup.LockedCandidates"/>
	/// and <see cref="TechniqueGroup.Subset"/>.
	/// </param>
	/// <returns>The combined result.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="this"/> is not a direct technique.</exception>
	/// <exception cref="NotSupportedException">
	/// Throws when the argument <paramref name="indirect"/> is neither locked candidates nor subset.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Technique ComplexSingleUsing(this Technique @this, Technique indirect)
		=> @this switch
		{
			Technique.FullHouse or >= Technique.CrosshatchingBlock and <= Technique.CrosshatchingColumn or Technique.NakedSingle
				=> indirect.GetGroup() switch
				{
					TechniqueGroup.LockedCandidates or TechniqueGroup.Subset => Enum.Parse<Technique>($"{indirect}{@this}"),
					_ when Enum.IsDefined(indirect) => throw new NotSupportedException(SR.ExceptionMessage("ComplexSingleNotSupportedToday")),
					_ => throw new ArgumentOutOfRangeException(nameof(indirect))
				},
			_ => throw new ArgumentOutOfRangeException(nameof(@this))
		};

	/// <summary>
	/// Try to get the group that the current <see cref="Technique"/> belongs to. If a technique doesn't contain a corresponding group,
	/// this method will return <see langword="null"/>. No exception will be thrown.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>The <see cref="TechniqueGroup"/> value that the current <see cref="Technique"/> belongs to.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueGroup? TryGetGroup(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())?.GetCustomAttribute<TechniqueMetadataAttribute>()?.ContainingGroup;

	/// <summary>
	/// Try to get the group that the current <see cref="Technique"/> belongs to.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>The <see cref="TechniqueGroup"/> value that the current <see cref="Technique"/> belongs to.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the specified <see cref="Technique"/> does not belong to any <see cref="TechniqueGroup"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueGroup GetGroup(this Technique @this) => @this.TryGetGroup()!.Value;

	/// <summary>
	/// Try to get its static difficulty level for the specified technique.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>
	/// The difficulty level that the current technique belongs to.
	/// If it doesn't contain a static difficulty level value, <see cref="DifficultyLevel.Unknown"/> will be returned.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DifficultyLevel GetDifficultyLevel(this Technique @this)
	{
		var fi = TypeOfTechnique.GetField(@this.ToString())!;
		return (fi.GetCustomAttribute<TechniqueMetadataAttribute>(), fi.GetCustomAttribute<TechniqueMetadataAttribute>()) switch
		{
			({ Features: var feature }, _) when feature.HasFlag(TechniqueFeatures.NotImplemented) => DifficultyLevel.Unknown,
			(_, { DifficultyLevel: var level }) => level,
			_ => throw new MissingDifficultyLevelException(@this.ToString())
		};
	}

	/// <summary>
	/// Try to get the corresponding <see cref="SingleTechniqueFlag"/> for the specified single <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>The corresponding <see cref="SingleTechniqueFlag"/> instance.</returns>
	/// <exception cref="InvalidOperationException">Throws when the technique is not single.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SingleTechniqueFlag GetSingleTechnique(this Technique @this)
		=> @this switch
		{
			Technique.FullHouse => SingleTechniqueFlag.FullHouse,
			Technique.LastDigit => SingleTechniqueFlag.LastDigit,
			Technique.CrosshatchingBlock or Technique.HiddenSingleBlock => SingleTechniqueFlag.HiddenSingleBlock,
			Technique.CrosshatchingRow or Technique.HiddenSingleRow => SingleTechniqueFlag.HiddenSingleRow,
			Technique.CrosshatchingColumn or Technique.HiddenSingleColumn => SingleTechniqueFlag.HiddenSingleColumn,
			Technique.NakedSingle => SingleTechniqueFlag.NakedSingle,
			_ => throw new InvalidOperationException(SR.ExceptionMessage("ArgumentMustBeSingle"))
		};

	/// <summary>
	/// Try to get features for the current <see cref="Technique"/>.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>All found features for the current <see cref="Technique"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TechniqueFeatures GetFeature(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())?.GetCustomAttribute<TechniqueMetadataAttribute>()?.Features ?? 0;

	/// <summary>
	/// Try to get supported pencilmark-visibility modes that the current <see cref="Technique"/> can be used in application.
	/// </summary>
	/// <param name="this">The <see cref="Technique"/> instance.</param>
	/// <returns>
	/// <para>A <see cref="PencilmarkVisibility"/> value indicating the supported modes.</para>
	/// <para>
	/// The result will be merged via flag values. Use <see cref="Enum.HasFlag(Enum)"/> to determine what flag you want to check.
	/// </para>
	/// </returns>
	/// <seealso cref="Enum.HasFlag(Enum)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static PencilmarkVisibility GetSupportedPencilmarkVisibilityModes(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()?.PencilmarkVisibility
		?? PencilmarkVisibilities.All;

	/// <summary>
	/// Try to get suitable <see cref="Type"/> which refers to a <see cref="Step"/> type,
	/// whose contained property <see cref="Step.Code"/> may create this technique.
	/// </summary>
	/// <param name="this">The current technique.</param>
	/// <returns>
	/// A valid <see cref="Type"/> instance, or <see langword="null"/> if it may not be referred by all <see cref="Step"/>
	/// derived types.
	/// </returns>
	/// <seealso cref="Step"/>
	/// <seealso cref="Step.Code"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Type? GetSuitableStepType(this Technique @this)
		=> TypeOfTechnique.GetField(@this.ToString())!.GetCustomAttribute<TechniqueMetadataAttribute>()?.StepType;
}
