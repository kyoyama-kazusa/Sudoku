namespace SudokuStudio.BindableSource;

/// <summary>
/// Represents a type that can be used for binding as source, for the table-like grid controls to display techniques used,
/// using technique name to distinct them.
/// </summary>
/// <seealso cref="AnalysisResult"/>
internal sealed class SummaryViewBindableSource : INotifyPropertyChanged
{
	/// <summary>
	/// Indicates the name of the technique.
	/// </summary>
	public required string TechniqueName
	{
		get;

		set
		{
			field = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Indicates the difficulty level of the technique.
	/// </summary>
	public required DifficultyLevel DifficultyLevel
	{
		get;

		set
		{
			field = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Indicates the total difficulty of the group of steps.
	/// </summary>
	public required decimal TotalDifficulty
	{
		get;

		set
		{
			field = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Indicates the maximum difficulty of the group of steps.
	/// </summary>
	public required decimal MaxDifficulty
	{
		get;

		set
		{
			field = value;
			OnPropertyChanged();
		}
	}

	/// <summary>
	/// Indicates the number of steps in this group.
	/// </summary>
	public required int CountOfSteps
	{
		get;

		set
		{
			field = value;
			OnPropertyChanged();
		}
	}


	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;


	/// <summary>
	/// Triggers event <see cref="PropertyChanged"/>.
	/// </summary>
	/// <param name="propertyName">The property name.</param>
	private void OnPropertyChanged([CallerMemberName] string propertyName = null!) => PropertyChanged?.Invoke(this, new(propertyName));

	/// <summary>
	/// Creates the list of <see cref="SummaryViewBindableSource"/> as the result value,
	/// via the specified <paramref name="analysisResult"/> instance of <see cref="AnalysisResult"/> type.
	/// </summary>
	/// <param name="analysisResult">
	/// The <see cref="AnalysisResult"/> instance that is used for creating the result value.
	/// </param>
	/// <returns>The result list of <see cref="SummaryViewBindableSource"/>-typed elements.</returns>
	/// <exception cref="InvalidOperationException">Throws when the puzzle hasn't been solved.</exception>
	public static ObservableCollection<SummaryViewBindableSource> CreateListFrom(AnalysisResult analysisResult)
	{
		var pref = Application.Current.AsApp().Preference.TechniqueInfoPreferences;
		return analysisResult switch
		{
			{ IsSolved: true, StepsSpan: var steps } => [.. g(steps, pref)],
			{ IsPartiallySolved: true, StepsSpan: var steps } => [.. g(steps, pref)],
			_ => throw new InvalidOperationException(SR.ExceptionMessage("GridMustBeSolvedOrNotBad"))
		};


		static decimal r(Step step)
		{
			var pref = Application.Current.AsApp().Preference.TechniqueInfoPreferences;
			return pref.GetRating(step.Code) switch { { } v => v, _ => step.Difficulty } / pref.RatingScale;
		}

		static unsafe SummaryViewBindableSource[] g(ReadOnlySpan<Step> steps, TechniqueInfoPreferenceGroup pref)
		{
			var stepsSortedByName = new List<Step>();
			stepsSortedByName.AddRange(steps);
			stepsSortedByName.Sort(
				static (left, right) => left.DifficultyLevel.CompareTo(right.DifficultyLevel) is var difficultyLevelComparisonResult and not 0
					? difficultyLevelComparisonResult
					: left.Code.CompareTo(right.Code) is var codeComparisonResult and not 0
						? codeComparisonResult
						: Step.CompareName(left, right, App.CurrentCulture)
			);
			return [
				..
				from step in stepsSortedByName
				group step by step.GetName(App.CurrentCulture) into stepGroup
				let stepGroupArray = (Step[])[.. stepGroup]
				let difficultyLevels =
					from step in stepGroupArray
					let code = step.Code
					group step by pref.GetDifficultyLevelOrDefault(code) into stepGroupedByDifficultyLevel
					select stepGroupedByDifficultyLevel.Key into targetDifficultyLevel
					orderby targetDifficultyLevel
					select targetDifficultyLevel
				select new SummaryViewBindableSource
				{
					TechniqueName = stepGroup.Key,
					DifficultyLevel = difficultyLevels.Aggregate(@delegate.EnumFlagMerger),
					TotalDifficulty = stepGroupArray.SumUnsafe(&r),
					MaxDifficulty = stepGroupArray.MaxUnsafe(&r),
					CountOfSteps = stepGroupArray.Length
				}
			];
		}
	}
}
