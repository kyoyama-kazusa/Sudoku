namespace SudokuStudio.Configuration;

/// <summary>
/// Represents a preference group that defines the custom difficulty level and rating values for the techniques.
/// </summary>
public sealed partial class TechniqueInfoPreferenceGroup : PreferenceGroup
{
	private static readonly decimal RatingScaleDefaultValue = 10M;

	private static readonly Dictionary<Technique, TechniqueData> CustomizedTechniqueDataDefaultValue = [];


	/// <summary>
	/// Defines a dependency property that binds with property <see cref="RatingScale"/>.
	/// </summary>
	/// <seealso cref="RatingScale"/>
	public static readonly DependencyProperty RatingScaleProperty =
		DependencyProperty.Register(nameof(RatingScale), typeof(decimal), typeof(TechniqueInfoPreferenceGroup), new PropertyMetadata(RatingScaleDefaultValue));

	/// <summary>
	/// Defines a dependency property that binds with property <see cref="CustomizedTechniqueData"/>.
	/// </summary>
	/// <seealso cref="CustomizedTechniqueData"/>
	public static readonly DependencyProperty CustomizedTechniqueDataProperty =
		DependencyProperty.Register(nameof(CustomizedTechniqueData), typeof(Dictionary<Technique, TechniqueData>), typeof(TechniqueInfoPreferenceGroup), new PropertyMetadata(CustomizedTechniqueDataDefaultValue));


	/// <summary>
	/// Indicates the rating scale value. The value will be used by scaling the value stored in property <see cref="CustomizedTechniqueData"/>.
	/// </summary>
	/// <seealso cref="CustomizedTechniqueData"/>
	public decimal RatingScale
	{
		get => (decimal)GetValue(RatingScaleProperty);

		set => SetValue(RatingScaleProperty, value);
	}

	/// <summary>
	/// Indicates the customized technique Data.
	/// </summary>
	public Dictionary<Technique, TechniqueData> CustomizedTechniqueData
	{
		get => (Dictionary<Technique, TechniqueData>)GetValue(CustomizedTechniqueDataProperty);

		set => SetValue(CustomizedTechniqueDataProperty, value);
	}


	/// <summary>
	/// Add or update rating value.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <param name="value">The rating value to be set.</param>
	public void AppendOrUpdateRating(Technique technique, int value)
	{
		unsafe
		{
			AppendOrUpdateValue(technique, value, &dataCreator, &dataModifier);


			static TechniqueData dataCreator(Technique technique, int value)
			{
				technique.GetDefaultRating(out var directRating);
				return new(value, directRating, technique.DifficultyLevel);
			}

			static TechniqueData dataModifier(in TechniqueData data, int value) => data with { Rating = value };
		}
	}

	/// <summary>
	/// Add or update direct rating value.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <param name="value">The rating value to be set.</param>
	public void AppendOrUpdateDirectRating(Technique technique, int value)
	{
		unsafe
		{
			AppendOrUpdateValue(technique, value, &dataCreator, &dataModifier);


			static TechniqueData dataCreator(Technique technique, int value)
				=> new(technique.GetDefaultRating(out _), value, technique.DifficultyLevel);

			static TechniqueData dataModifier(in TechniqueData data, int value) => data with { DirectRating = value };
		}
	}

	/// <summary>
	/// Add or update difficulty level.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <param name="value">The difficulty level to be set.</param>
	public void AppendOrUpdateDifficultyLevel(Technique technique, DifficultyLevel value)
	{
		unsafe
		{
			AppendOrUpdateValue(technique, value, &dataCreator, &dataModifier);


			static TechniqueData dataCreator(Technique technique, DifficultyLevel value)
			{
				var rating = technique.GetDefaultRating(out var directRating);
				return new(rating, directRating, value);
			}

			static TechniqueData dataModifier(in TechniqueData data, DifficultyLevel value) => data with { Level = value };
		}
	}


	/// <summary>
	/// Append or update value.
	/// </summary>
	/// <typeparam name="T">The type of the value to be set.</typeparam>
	/// <param name="technique">The technique.</param>
	/// <param name="value">The value to be set.</param>
	/// <param name="dataCreator">The data creator method that will be called when the collection doesn't contain the technique.</param>
	/// <param name="dataModifier">The data modifier method that will be called when the collection contains the technique.</param>
	private void AppendOrUpdateValue<T>(
		Technique technique,
		T value,
		delegate*<Technique, T, TechniqueData> dataCreator,
		delegate*<in TechniqueData, T, TechniqueData> dataModifier
	) where T : allows ref struct
	{
		ref var data = ref CustomizedTechniqueData.GetValueRef(technique);
		var isNullRef = Unsafe.IsNullRef(in data);
		var a = valueUpdaterWhenNullRef;
		var b = valueUpdaterWhenNotNullRef;
		(isNullRef ? a : b)(ref data, unsafe (isNullRef ? dataCreator(technique, value) : dataModifier(data, value)));


		void valueUpdaterWhenNullRef(ref TechniqueData data, TechniqueData newData) => CustomizedTechniqueData.Add(technique, newData);

		void valueUpdaterWhenNotNullRef(ref TechniqueData data, TechniqueData newData) => data = newData;
	}

	/// <inheritdoc cref="GetRatingOrDefault(Technique)"/>
	public int? GetRating(Technique technique) => CustomizedTechniqueData.TryGetValue(technique, out var pair) ? pair.Rating : null;

	/// <summary>
	/// Try to get the rating value for the specified technique.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <returns>The rating value.</returns>
	public int GetRatingOrDefault(Technique technique)
		=> CustomizedTechniqueData.TryGetValue(technique, out var pair) ? pair.Rating : technique.GetDefaultRating(out _);

	/// <inheritdoc cref="GetDifficultyLevelOrDefault(Technique)"/>
	public DifficultyLevel? GetDifficultyLevel(Technique technique)
		=> CustomizedTechniqueData.TryGetValue(technique, out var pair) ? pair.Level : null;

	/// <summary>
	/// Try to get the difficulty level for the specified technique.
	/// </summary>
	/// <param name="technique">The technique.</param>
	/// <returns>The difficulty level.</returns>
	public DifficultyLevel GetDifficultyLevelOrDefault(Technique technique)
		=> CustomizedTechniqueData.TryGetValue(technique, out var pair) ? pair.Level : technique.DifficultyLevel;
}
