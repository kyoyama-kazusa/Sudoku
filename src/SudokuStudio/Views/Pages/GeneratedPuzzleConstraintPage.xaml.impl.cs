namespace SudokuStudio.Views.Pages;

public partial class GeneratedPuzzleConstraintPage
{
	/// <summary>
	/// Indicates the default spacing.
	/// </summary>
	private static readonly double DefaultSpacing = 3;

	/// <summary>
	/// Indicates the default margin.
	/// </summary>
	private static readonly Thickness DefaultMargin = new(0, 6, 0, 6);


	private partial SettingsExpander? Create_BottleneckTechnique(BottleneckTechniqueConstraint constraint)
	{
		if (constraint is not { Techniques: var techniques })
		{
			return null;
		}

		//
		// chosen techniques displayer
		//
		var displayerControl = new TextBlock
		{
			MaxWidth = 400,
			TextWrapping = TextWrapping.WrapWholeWords,
			VerticalAlignment = VerticalAlignment.Center,
			Text = techniques.GetTechniqueString()
		};

		//
		// technique view
		//
		var techniqueControl = new TechniqueView
		{
			SelectionMode = TechniqueViewSelectionMode.Multiple,
			SelectedTechniques = techniques,
			Margin = new(-56, 0, 0, 0)
		};
		techniqueControl.SelectedTechniquesChanged += (_, e) => displayerControl.Text = (constraint.Techniques = e.TechniqueSet).GetTechniqueString();

		// Fixes #558: 'SettingsExpander' always requires 'SettingsCard' as the children control
		// for 'SettingsExpander.Items' property. See https://github.com/kyoyama-kazusa/Sudoku/issues/558
		// Reference link: https://github.com/CommunityToolkit/Windows/issues/302#issuecomment-1857686711
		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_BottleneckTechnique", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Items = { new SettingsCard { Content = techniqueControl, ContentAlignment = ContentAlignment.Left } },
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { displayerControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_DifficultyLevel(DifficultyLevelConstraint constraint)
	{
		if (constraint is not { DifficultyLevel: var difficultyLevel, Operator: var @operator })
		{
			return null;
		}

		//
		// operator selection
		//
		var operatorControl = ComparisonOperatorControl(@operator, constraint);

		//
		// difficulty level selection
		//
		var difficultyLevelControl = new ComboBox
		{
			PlaceholderText = SR.Get("GeneratedPuzzleConstraintPage_ChooseDifficultyLevel", App.CurrentCulture),
			Items =
			{
				new ComboBoxItem { Content = DifficultyLevel.Easy.GetName(App.CurrentCulture), Tag = DifficultyLevel.Easy },
				new ComboBoxItem { Content = DifficultyLevel.Moderate.GetName(App.CurrentCulture), Tag = DifficultyLevel.Moderate },
				new ComboBoxItem { Content = DifficultyLevel.Hard.GetName(App.CurrentCulture), Tag = DifficultyLevel.Hard },
				new ComboBoxItem { Content = DifficultyLevel.Fiendish.GetName(App.CurrentCulture), Tag = DifficultyLevel.Fiendish },
				new ComboBoxItem { Content = DifficultyLevel.Nightmare.GetName(App.CurrentCulture), Tag = DifficultyLevel.Nightmare }
			}
		};
		EnumBinder<ComboBox, ComboBoxItem, DifficultyLevel>(difficultyLevelControl, difficultyLevel, value => constraint.DifficultyLevel = value);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_DifficultyLevel", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { operatorControl, difficultyLevelControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_Symmetry(SymmetryConstraint constraint)
	{
		// There may exist a bug that we cannot select "SymmetricType.None" because the flag value is 0, no flags can use this bit.

		if (constraint is not { SymmetricTypes: var symmetricTypes })
		{
			return null;
		}

		//
		// symmetry selection
		//
		var symmetryControl = new Segmented
		{
			SelectionMode = ListViewSelectionMode.Multiple,
			Style = (Style)Application.Current.Resources["ButtonSegmentedStyle"]!,
			Items =
			{
				new SegmentedItem { Content = SR.Get("SymmetricType_Central", App.CurrentCulture), Tag = SymmetricType.Central },
				new SegmentedItem { Content = SR.Get("SymmetricType_Diagonal", App.CurrentCulture), Tag = SymmetricType.Diagonal },
				new SegmentedItem { Content = SR.Get("SymmetricType_Diagonal", App.CurrentCulture), Tag = SymmetricType.AntiDiagonal },
				new SegmentedItem { Content = SR.Get("SymmetricType_YAxis", App.CurrentCulture), Tag = SymmetricType.YAxis },
				new SegmentedItem { Content = SR.Get("SymmetricType_XAxis", App.CurrentCulture), Tag = SymmetricType.XAxis },
				new SegmentedItem { Content = SR.Get("SymmetricType_AxisBoth", App.CurrentCulture), Tag = SymmetricType.AxisBoth },
				new SegmentedItem { Content = SR.Get("SymmetricType_DiagonalBoth", App.CurrentCulture), Tag = SymmetricType.DiagonalBoth },
				new SegmentedItem { Content = SR.Get("SymmetricType_All", App.CurrentCulture), Tag = SymmetricType.All }
			}
		};
		symmetryControl.SelectionChanged += (_, _) =>
		{
			foreach (SegmentedItem element in symmetryControl.Items)
			{
				var type = (SymmetricType)element.Tag!;
				var possibleItemInChosenItemsList = symmetryControl.SelectedItems.FirstOrDefault(e => ReferenceEquals(e, element));
				if (possibleItemInChosenItemsList is not null)
				{
					constraint.SymmetricTypes |= type;
				}
				else
				{
					constraint.SymmetricTypes &= ~type;
				}
			}

			// Special case: If a user has unselected all symmetric types, we should define as an invalid value.
			if (constraint.SymmetricTypes == 0)
			{
				constraint.SymmetricTypes = SymmetryConstraint.InvalidSymmetricType;
			}
		};
		foreach (SegmentedItem element in symmetryControl.Items)
		{
			if (symmetricTypes.HasFlag((SymmetricType)element.Tag!))
			{
				symmetryControl.SelectedItems.Add(element);
			}
		}

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Symmetry", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = symmetryControl,
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_Conclusion(ConclusionConstraint constraint)
	{
		if (constraint is not { Conclusion: var conclusion, ShouldAppear: var shouldAppear })
		{
			return null;
		}

		//
		// conclusion type selector
		//
		var conclusionTypeControl = new ComboBox
		{
			Items =
			{
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_Equality", App.CurrentCulture),
					Tag = Assignment
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_Inequality", App.CurrentCulture),
					Tag = Elimination
				}
			},
			VerticalAlignment = VerticalAlignment.Center
		};
		EnumBinder<ComboBox, ComboBoxItem, ConclusionType>(
			conclusionTypeControl,
			constraint.Conclusion.ConclusionType,
			value => constraint.Conclusion = new(value, constraint.Conclusion.Candidate)
		);

		var candidatePicker = new CandidatePicker { SelectedCandidate = constraint.Conclusion.Candidate };
		candidatePicker.SelectedCandidateChanged += selectedCandidateCallback;

		//
		// appear label
		//
		var appearLabelControl = new TextBlock
		{
			Text = SR.Get("GeneratedPuzzleConstraintPage_ShouldAppearLabel", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center
		};

		//
		// appear control
		//
		var appearControl = new ToggleSwitch { IsOn = shouldAppear };
		appearControl.RegisterPropertyChangedCallback(ToggleSwitch.IsOnProperty, appearControlCallback);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Conclusion", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { conclusionTypeControl, candidatePicker, appearLabelControl, appearControl }
			},
			Tag = constraint
		};


		void selectedCandidateCallback(CandidatePicker _, CandidatePickerSelectedCandidateChangedEventArgs __)
			=> constraint.Conclusion = new(constraint.Conclusion.ConclusionType, candidatePicker.SelectedCandidate);

		void appearControlCallback(DependencyObject d, DependencyProperty _) => constraint.ShouldAppear = ((ToggleSwitch)d).IsOn;
	}

	private partial SettingsCard? Create_Lasting(LastingConstraint constraint)
	{
		if (constraint is not { LimitCount: var limitCount, TechniqueFlag: var technique, Operator: var @operator })
		{
			return null;
		}

		//
		// operator
		//
		var operatorControl = ComparisonOperatorControl(@operator, constraint);

		//
		// technique selector
		//
		var techniqueSelectorControl = new Segmented
		{
			Style = (Style)Application.Current.Resources["ButtonSegmentedStyle"]!,
			Items =
			{
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_Lasting_HiddenSingleRow", App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingleRow
				},
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_Lasting_HiddenSingleColumn", App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingleColumn
				},
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_Lasting_NakedSingle", App.CurrentCulture),
					Tag = SingleTechniqueFlag.NakedSingle
				}
			}
		};
		EnumBinder<Segmented, SegmentedItem, SingleTechniqueFlag>(
			techniqueSelectorControl,
			constraint.TechniqueFlag,
			value => constraint.TechniqueFlag = value
		);

		//
		// limit count
		//
		var limitCountControl = LimitCountControl(limitCount, constraint);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Lasting", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { techniqueSelectorControl, operatorControl, limitCountControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_Minimal(MinimalConstraint constraint)
	{
		if (constraint is not { ShouldBeMinimal: var value })
		{
			return null;
		}

		//
		// minimal selector
		//
		var minimalControl = new ToggleSwitch { IsOn = value };
		minimalControl.RegisterPropertyChangedCallback(ToggleSwitch.IsOnProperty, (d, _) => constraint.ShouldBeMinimal = ((ToggleSwitch)d).IsOn);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Minimal", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = minimalControl,
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_PearlOrDiamond<TConstraint>(TConstraint constraint) where TConstraint : PearlOrDiamondConstraint
	{
		if (constraint is not { CheckPearl: var checkPearl, ShouldBePearlOrDiamond: var value })
		{
			return null;
		}

		//
		// pearl or diamond selector
		//
		var control = new ToggleSwitch { IsOn = value };
		control.RegisterPropertyChangedCallback(ToggleSwitch.IsOnProperty, (d, _) => constraint.ShouldBePearlOrDiamond = ((ToggleSwitch)d).IsOn);

		return new()
		{
			Header = SR.Get($"GeneratedPuzzleConstraintPage_{(checkPearl ? "Pearl" : "Diamond")}", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = control,
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_CountBetween(CountBetweenConstraint constraint)
	{
		if (constraint is not
			{
				Range: { Start.Value: var min, End.Value: var max },
				CellState: var cellState,
				BetweenRule: var rule
			})
		{
			return null;
		}

		//
		// minimum value box
		//
		var minimumControl = new IntegerBox { Width = 150, Minimum = 17, Maximum = 80, SmallChange = 1, LargeChange = 5, Value = min };
		var maximumControl = new IntegerBox { Width = 150, Minimum = 18, Maximum = 81, SmallChange = 1, LargeChange = 5, Value = max };
		minimumControl.ValueChanged += (_, _) =>
		{
			maximumControl.Minimum = minimumControl.Value + 1;
			if (minimumControl.Value >= maximumControl.Value)
			{
				maximumControl.Value++;
			}
			rangeSetter();
		};
		maximumControl.ValueChanged += (_, _) =>
		{
			minimumControl.Maximum = maximumControl.Value - 1;
			if (maximumControl.Value <= minimumControl.Value)
			{
				minimumControl.Value--;
			}
			rangeSetter();
		};

		//
		// cell-state selector
		//
		var cellStateControl = new ComboBox
		{
			PlaceholderText = SR.Get("GeneratedPuzzleConstraintPage_ChooseCellState", App.CurrentCulture),
			Items =
			{
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_GivensCount", App.CurrentCulture),
					Tag = CellState.Given
				},
#if false
				new ComboBoxItem
				{
					Content = ResourceDictionary.Get("GeneratedPuzzleConstraintPage_ModifiablesCount", App.CurrentCulture),
					Tag = CellState.Modifiable
				},
#endif
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_EmptiesCount", App.CurrentCulture),
					Tag = CellState.Empty
				}
			}
		};
		EnumBinder<ComboBox, ComboBoxItem, CellState>(
			cellStateControl,
			cellState,
			value =>
			{
				constraint.CellState = value;

				// Sync for minimum and maximum value for integer boxes.
				if (value is CellState.Given or CellState.Empty)
				{
					((minimumControl.Minimum, minimumControl.Maximum), (maximumControl.Minimum, maximumControl.Maximum)) = value switch
					{
						CellState.Given => ((17, 80), (18, 81)),
						CellState.Empty => ((1, 63), (2, 64))
					};
				}
			}
		);

		//
		// "and" text block
		//
		var andTextBlockControl = new TextBlock
		{
			Text = SR.Get("GeneratedPuzzleConstraintPage_AndTextBlock", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center
		};

		//
		// between rule selector
		//
		var betweenRuleControl = BetweenRuleControl(constraint, rule);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_CountBetween", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children =
				{
					cellStateControl,
					minimumControl,
					andTextBlockControl,
					maximumControl,
					betweenRuleControl
				}
			},
			Tag = constraint
		};


		void rangeSetter() => constraint.Range = minimumControl.Value..maximumControl.Value;
	}

	private partial SettingsCard? Create_Ittoryu(IttoryuConstraint constraint)
	{
		if (constraint is not { LimitedSingle: var limitedSingle, Operator: var @operator, Rounds: var rounds })
		{
			return null;
		}

		//
		// highest technique displayer
		//
		var highestTechniqueControl = new TextBlock
		{
			Text = SR.Get("GeneratedPuzzleConstraintPage_HighestSingleTechnique", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center
		};

		//
		// limited single
		//
		var singleControl = new Segmented
		{
			Style = (Style)Application.Current.Resources["ButtonSegmentedStyle"]!,
			Items =
			{
				new SegmentedItem
				{
					Content = SingleTechniqueFlag.HiddenSingleBlock.GetName(App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingleBlock
				},
				new SegmentedItem
				{
					Content = SingleTechniqueFlag.HiddenSingleRow.GetName(App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingleRow
				},
				new SegmentedItem
				{
					Content = SingleTechniqueFlag.HiddenSingleColumn.GetName(App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingleColumn
				},
				new SegmentedItem
				{
					Content = SingleTechniqueFlag.NakedSingle.GetName(App.CurrentCulture),
					Tag = SingleTechniqueFlag.NakedSingle
				}
			}
		};
		EnumBinder<Segmented, SegmentedItem, SingleTechniqueFlag>(singleControl, limitedSingle, value => constraint.LimitedSingle = value);

		//
		// operator selector
		//
		var operatorControl = ComparisonOperatorControl(@operator, constraint);

		//
		// rounds box
		//
		var roundsControl = LimitCountControl(rounds, constraint);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Ittoryu", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { highestTechniqueControl, singleControl, operatorControl, roundsControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsExpander? Create_Technique(TechniqueConstraint constraint)
	{
		if (constraint is not { Techniques: var techniques })
		{
			return null;
		}

		//
		// chosen techniques displayer
		//
		var displayerControl = new TextBlock
		{
			MaxWidth = 400,
			TextWrapping = TextWrapping.WrapWholeWords,
			VerticalAlignment = VerticalAlignment.Center,
			Text = techniques.GetTechniqueString()
		};

		//
		// technique view
		//
		var techniqueControl = new TechniqueView
		{
			SelectionMode = TechniqueViewSelectionMode.Multiple,
			SelectedTechniques = techniques,
			Margin = new(-56, 0, 0, 0)
		};
		techniqueControl.SelectedTechniquesChanged += (_, e) => displayerControl.Text = (constraint.Techniques = e.TechniqueSet).GetTechniqueString();

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_Technique", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Items = { new SettingsCard { Content = techniqueControl, ContentAlignment = ContentAlignment.Left } },
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { displayerControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsExpander? Create_TechniqueCount(TechniqueCountConstraint constraint)
	{
		if (constraint is not { Technique: var technique, LimitCount: var appearingTimes, Operator: var @operator })
		{
			return null;
		}

		//
		// chosen techniques displayer
		//
		var displayerControl = new TextBlock
		{
			MaxWidth = 400,
			TextWrapping = TextWrapping.WrapWholeWords,
			VerticalAlignment = VerticalAlignment.Center,
			Text = $"{technique.GetName(App.CurrentCulture)}{SR.Get("_Token_Comma2", App.CurrentCulture)}{SR.Get("GeneratedPuzzleConstraintPage_AppearingTimes", App.CurrentCulture)}"
		};

		//
		// technique view
		//
		var techniqueControl = new TechniqueView
		{
			SelectionMode = TechniqueViewSelectionMode.Single,
			SelectedTechniques = [technique],
			Margin = new(-56, 0, 0, 0)
		};
		techniqueControl.CurrentSelectedTechniqueChanged += (_, e) =>
		{
			var technique = e.Technique;
			constraint.Technique = technique;
			displayerControl.Text = $"{technique.GetName(App.CurrentCulture)}{SR.Get("_Token_Comma2", App.CurrentCulture)}{SR.Get("GeneratedPuzzleConstraintPage_AppearingTimes", App.CurrentCulture)}";
		};

		//
		// appearing times
		//
		var appearingTimesControl = LimitCountControl(appearingTimes, constraint);

		//
		// comparison operator
		//
		var operatorControl = ComparisonOperatorControl(@operator, constraint);

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_TechniqueCount", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Items = { new SettingsCard { Content = techniqueControl, ContentAlignment = ContentAlignment.Left } },
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { displayerControl, operatorControl, appearingTimesControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsExpander? Create_TechniqueSet(TechniqueSetConstraint constraint)
	{
		if (constraint is not { Techniques: var techniques })
		{
			return null;
		}

		//
		// chosen techniques displayer
		//
		var displayerControl = new TextBlock
		{
			MaxWidth = 400,
			TextWrapping = TextWrapping.WrapWholeWords,
			VerticalAlignment = VerticalAlignment.Center,
			Text = techniques.GetTechniqueString()
		};

		//
		// technique view
		//
		var techniqueControl = new TechniqueView
		{
			SelectionMode = TechniqueViewSelectionMode.Multiple,
			SelectedTechniques = techniques,
			Margin = new(-56, 0, 0, 0)
		};
		techniqueControl.SelectedTechniquesChanged += (_, e) => displayerControl.Text = (constraint.Techniques = e.TechniqueSet).GetTechniqueString();

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_TechniqueSet", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Items = { new SettingsCard { Content = techniqueControl, ContentAlignment = ContentAlignment.Left } },
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { displayerControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsExpander? Create_EliminationCount(EliminationCountConstraint constraint)
	{
		if (constraint is not { LimitCount: var limitCount, Operator: var @operator, Technique: var technique })
		{
			return null;
		}

		//
		// Operator control
		//
		var operatorControl = ComparisonOperatorControl(@operator, constraint);

		//
		// Number control
		//
		var limitCountControl = LimitCountControl(limitCount, constraint);

		//
		// chosen techniques displayer
		//
		var displayerControl = new TextBlock
		{
			MaxWidth = 400,
			TextWrapping = TextWrapping.WrapWholeWords,
			VerticalAlignment = VerticalAlignment.Center,
			Text = $"{technique.GetName(App.CurrentCulture)}{SR.Get("_Token_Comma2", App.CurrentCulture)}"
		};

		//
		// technique view
		//
		var techniqueControl = new TechniqueView
		{
			SelectionMode = TechniqueViewSelectionMode.Single,
			ShowMode = TechniqueViewShowMode.OnlyEliminations,
			Margin = new(-56, 0, 0, 0),
			SelectedTechniques = [technique]
		};
		techniqueControl.CurrentSelectedTechniqueChanged += (_, e) =>
		{
			var technique = e.Technique;
			constraint.Technique = technique;
			displayerControl.Text = $"{technique.GetName(App.CurrentCulture)}{SR.Get("_Token_Comma2", App.CurrentCulture)}";
		};

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_EliminationCount", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Items = { new SettingsCard { Content = techniqueControl, ContentAlignment = ContentAlignment.Left } },
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { displayerControl, operatorControl, limitCountControl }
			},
			Tag = constraint
		};
	}

	private partial SettingsCard? Create_PrimarySingle(PrimarySingleConstraint constraint)
	{
		if (constraint is not { Primary: var prefer, AllowsHiddenSingleInLines: var allowsForLine })
		{
			return null;
		}

		//
		// prefer label
		//
		var preferredLabel1 = new TextBlock
		{
			Text = SR.Get("GeneratedPuzzleConstraintPage_PrimaryTechniqueLabelPart1", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center
		};
		var preferredLabel2 = new TextBlock
		{
			Text = SR.Get("GeneratedPuzzleConstraintPage_PrimaryTechniqueLabelPart2", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center
		};

		//
		// technique selector
		//
		var techniqueSelectorControl = new Segmented
		{
			Style = (Style)Application.Current.Resources["ButtonSegmentedStyle"]!,
			Items =
			{
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_PrimaryFullHouse", App.CurrentCulture),
					Tag = SingleTechniqueFlag.FullHouse
				},
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_PrimaryHiddenSingle", App.CurrentCulture),
					Tag = SingleTechniqueFlag.HiddenSingle
				},
				new SegmentedItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_PrimaryNakedSingle", App.CurrentCulture),
					Tag = SingleTechniqueFlag.NakedSingle
				}
			}
		};
		EnumBinder<Segmented, SegmentedItem, SingleTechniqueFlag>(techniqueSelectorControl, constraint.Primary, value => constraint.Primary = value);

		//
		// allow line control
		//
		var allowsLineControl = new ToggleSwitch { IsOn = constraint.AllowsHiddenSingleInLines };
		allowsLineControl.RegisterPropertyChangedCallback(
			ToggleSwitch.IsOnProperty,
			(d, _) => constraint.AllowsHiddenSingleInLines = ((ToggleSwitch)d).IsOn
		);
		techniqueSelectorControl.SelectionChanged += (sender, _) => allowsLineControl.IsEnabled = sender is Segmented
		{
			SelectedItem: SegmentedItem { Tag: SingleTechniqueFlag.HiddenSingle }
		};

		return new()
		{
			Header = SR.Get("GeneratedPuzzleConstraintPage_PrimarySingle", App.CurrentCulture),
			Description = constraint.Description,
			Margin = DefaultMargin,
			Content = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = DefaultSpacing,
				Children = { preferredLabel1, techniqueSelectorControl, preferredLabel2, allowsLineControl }
			},
			Tag = constraint
		};
	}


	/// <summary>
	/// The core method that binds a field of type <typeparamref name="TEnum"/> to a <typeparamref name="TControl"/> instance.
	/// </summary>
	/// <typeparam name="TItemControl">The type of the item control.</typeparam>
	/// <typeparam name="TControl">The type of the control.</typeparam>
	/// <typeparam name="TEnum">The type of the enumeration.</typeparam>
	/// <param name="control">The control to be operated.</param>
	/// <param name="valueToCompare">The value to compare.</param>
	/// <param name="constraintCallback">The constraint callback method.</param>
	private static unsafe void EnumBinder<TControl, TItemControl, TEnum>(TControl control, TEnum valueToCompare, Action<TEnum> constraintCallback)
		where TControl : Selector
		where TItemControl : SelectorItem
		where TEnum : unmanaged, Enum
	{
		var selectedIndex = 0;
		foreach (var element in control.Items)
		{
			if (element is not TItemControl { Tag: TEnum enumValue })
			{
				selectedIndex++;
				continue;
			}

			switch (sizeof(TEnum))
			{
				case 1 or 2 or 4:
				{
					var opRawValue = Unsafe.As<TEnum, int>(ref enumValue);
					var comparisonValueRawValue = Unsafe.As<TEnum, int>(ref valueToCompare);
					if (opRawValue != comparisonValueRawValue)
					{
						selectedIndex++;
						continue;
					}
					break;
				}
				case 8:
				{
					var opRawValue = Unsafe.As<TEnum, long>(ref enumValue);
					var comparisonValueRawValue = Unsafe.As<TEnum, long>(ref valueToCompare);
					if (opRawValue != comparisonValueRawValue)
					{
						selectedIndex++;
						continue;
					}
					break;
				}
			}

			break;
		}
		control.SelectedIndex = selectedIndex;
		control.SelectionChanged += (_, _) =>
		{
			if (control.SelectedIndex is var index and not -1 && control.Items[index] is TItemControl { Tag: TEnum value })
			{
				constraintCallback(value);
			}
		};
	}

	/// <summary>
	/// Creates a <see cref="ComboBox"/> object for comparison operator displaying.
	/// </summary>
	/// <typeparam name="TConstraint">The type of the constraint.</typeparam>
	/// <param name="operator">The operator value.</param>
	/// <param name="constraint">The constraint.</param>
	/// <returns>A <see cref="ComboBox"/> instance.</returns>
	private static ComboBox ComparisonOperatorControl<TConstraint>(ComparisonOperator @operator, TConstraint constraint)
		where TConstraint : Constraint, IComparisonOperatorConstraint
	{
		var operatorControl = new ComboBox
		{
			PlaceholderText = SR.Get("GeneratedPuzzleConstraintPage_ChooseComparisonOperator", App.CurrentCulture),
			VerticalAlignment = VerticalAlignment.Center,
			Items =
			{
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_Equality", App.CurrentCulture),
					Tag = ComparisonOperator.Equality
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_Inequality", App.CurrentCulture),
					Tag = ComparisonOperator.Inequality
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_GreaterThan", App.CurrentCulture),
					Tag = ComparisonOperator.GreaterThan
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_GreaterThanOrEqual", App.CurrentCulture),
					Tag = ComparisonOperator.GreaterThanOrEqual
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_LessThan", App.CurrentCulture),
					Tag = ComparisonOperator.LessThan
				},
				new ComboBoxItem
				{
					Content = SR.Get("_ComparisonOperator_LessThanOrEqual", App.CurrentCulture),
					Tag = ComparisonOperator.LessThanOrEqual
				}
			}
		};
		EnumBinder<ComboBox, ComboBoxItem, ComparisonOperator>(operatorControl, @operator, value => constraint.Operator = value);

		return operatorControl;
	}

	/// <summary>
	/// Creates an <see cref="IntegerBox"/> object for limit count displaying.
	/// </summary>
	/// <typeparam name="TConstraint">The type of the constraint.</typeparam>
	/// <param name="limitCount">The limit count.</param>
	/// <param name="constraint">The constraint.</param>
	/// <returns>An <see cref="IntegerBox"/> instance.</returns>
	private static IntegerBox LimitCountControl<TConstraint>(int limitCount, TConstraint constraint)
		where TConstraint : Constraint, ILimitCountConstraint<int>
	{
		var limitCountControl = new IntegerBox
		{
			Width = 150,
			Minimum = TConstraint.Minimum,
			Maximum = TConstraint.Maximum,
			Value = limitCount,
			VerticalAlignment = VerticalAlignment.Center
		};
		limitCountControl.ValueChanged += (_, _) => constraint.LimitCount = limitCountControl.Value;
		return limitCountControl;
	}

	/// <summary>
	/// Creates a between rule control.
	/// </summary>
	/// <param name="constraint">Indicates the constraint.</param>
	/// <param name="rule">Indicates the rule.</param>
	/// <returns>A <see cref="ComboBox"/> result.</returns>
	private static ComboBox BetweenRuleControl<TConstraint>(TConstraint constraint, BetweenRule rule)
		where TConstraint : Constraint, IBetweenRuleConstraint
	{
		var betweenRuleControl = new ComboBox
		{
			PlaceholderText = SR.Get("GeneratedPuzzleConstraintPage_ChooseBetweenRule", App.CurrentCulture),
			Items =
			{
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_BothOpen", App.CurrentCulture),
					Tag = BetweenRule.BothOpen
				},
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_OnlyLeftOpen", App.CurrentCulture),
					Tag = BetweenRule.LeftOpen
				},
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_OnlyRightOpen", App.CurrentCulture),
					Tag = BetweenRule.RightOpen
				},
				new ComboBoxItem
				{
					Content = SR.Get("GeneratedPuzzleConstraintPage_BothClosed", App.CurrentCulture),
					Tag = BetweenRule.BothClosed
				}
			}
		};

		EnumBinder<ComboBox, ComboBoxItem, BetweenRule>(betweenRuleControl, rule, value => constraint.BetweenRule = value);
		return betweenRuleControl;
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Try to fetch the string representation of the techniques chosen in UI.
	/// </summary>
	/// <param name="this">The techniques chosen.</param>
	/// <returns>The string representation.</returns>
	public static string GetTechniqueString(this TechniqueSet @this)
		=> @this switch
		{
		[] => SR.Get("GeneratedPuzzleConstraintPage_NoTechniquesSelected", App.CurrentCulture),
		[var technique] => technique.GetName(App.CurrentCulture),
			_ => string.Join(
				SR.Get("_Token_Comma", App.CurrentCulture),
				from technique in @this select technique.GetName(App.CurrentCulture)
			)
		};
}
