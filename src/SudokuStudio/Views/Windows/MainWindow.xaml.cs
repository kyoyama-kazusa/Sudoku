namespace SudokuStudio.Views.Windows;

/// <summary>
/// Provides with a <see cref="Window"/> instance that is running as main instance of the program.
/// </summary>
/// <seealso cref="Window"/>
public sealed partial class MainWindow :
	Window,
	IBackgroundPictureSupportedWindow
#if CUSTOMIZED_BACKDROP
	,
	IBackdropSupportedWindow
#endif
	,
	IThemeSupportedWindow
{
#if CUSTOMIZED_BACKDROP
	/// <summary>
	/// Indicates helper object for <see cref="DispatcherQueue"/>.
	/// </summary>
	/// <seealso cref="DispatcherQueue"/>
	private WindowsSystemDispatcherQueueHelper? _wsdqHelper;

	/// <summary>
	/// Indicates Mica backdrop controller.
	/// </summary>
	private MicaController? _micaController;

	/// <summary>
	/// Indicates desktop acrylic backdrop controller.
	/// </summary>
	private DesktopAcrylicController? _acrylicController;

	/// <summary>
	/// Indicates backdrop configuration.
	/// </summary>
	private SystemBackdropConfiguration? _configurationSource;
#endif


	/// <summary>
	/// Initializes a <see cref="MainWindow"/> instance.
	/// </summary>
	public MainWindow()
	{
		InitializeComponent();
		InitializeField();

#if CUSTOMIZED_TITLE_BAR
		InitializeAppWindow();
		SetAppTitleBarState();
		SetAppIcon();
#endif
	}


	/// <inheritdoc/>
	Panel IBackdropSupportedWindow.RootGridLayout => RootGridLayout;

	/// <inheritdoc/>
	Panel IBackgroundPictureSupportedWindow.RootGridLayout => RootGridLayout;


	/// <inheritdoc cref="NavigateToPage(Type, bool)"/>
	public void NavigateToPage(Type pageType) => NavigateToPage(pageType, false);

	/// <summary>
	/// Try to navigate to the target page with the custom data.
	/// </summary>
	/// <param name="pageType">The page type.</param>
	/// <param name="value">The value to be passed.</param>
	public void NavigateToPage(Type pageType, object? value) => NavigationPage.PageToWithValue = (pageType, value);

	/// <summary>
	/// Try to navigate to the target page.
	/// </summary>
	/// <param name="pageType">The target page type.</param>
	/// <param name="stackPage">Indicates whether the page should be stacked.</param>
	public void NavigateToPage(Type pageType, bool stackPage) => NavigationPage.PageToWithStack = (stackPage, pageType);

	/// <summary>
	/// Try to navigate to the target page with the specified number of pages to pop.
	/// </summary>
	/// <param name="pageType">The target page type.</param>
	/// <param name="popPageCount">The number of pages to pop.</param>
	public void NavigateToPage(Type pageType, int popPageCount) => NavigationPage.PageToPop = (pageType, popPageCount);

	/// <inheritdoc/>
	void IThemeSupportedWindow.ManuallySetTitleBarButtonsColor(Theme theme)
	{
		// Check to see if customization is supported. Currently only supported on Windows 11.
		if (AppWindowTitleBar.IsCustomizationSupported())
		{
			var titleBar = AppWindow.TitleBar;
			if (!titleBar.ExtendsContentIntoTitleBar)
			{
				titleBar.ExtendsContentIntoTitleBar = true;
			}

			// Sets the background color on "those" three buttons to transparent.
			titleBar.ButtonBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			titleBar.ButtonInactiveForegroundColor = Colors.Gray;

			switch (theme)
			{
				case Theme.Default when !App.ShouldSystemUseDarkMode():
				case Theme.Light:
				{
					titleBar.ButtonForegroundColor = Colors.Black;
					titleBar.ButtonHoverBackgroundColor = Colors.LightGray;
					titleBar.ButtonHoverForegroundColor = Colors.Black;
					titleBar.ButtonPressedBackgroundColor = Colors.DimGray;
					titleBar.ButtonPressedForegroundColor = Colors.Black;
					break;
				}
				case Theme.Default when App.ShouldSystemUseDarkMode():
				case Theme.Dark:
				{
					titleBar.ButtonForegroundColor = Colors.White;
					titleBar.ButtonHoverBackgroundColor = Colors.DarkGray;
					titleBar.ButtonHoverForegroundColor = Colors.White;
					titleBar.ButtonPressedBackgroundColor = Colors.LightGray;
					titleBar.ButtonPressedForegroundColor = Colors.White;
					break;
				}
			}
		}
	}

#if CUSTOMIZED_BACKDROP
	/// <summary>
	/// Try to set Mica backdrop.
	/// </summary>
	/// <param name="useMicaAlt">Indicates whether the current Mica backdrop use alternated configuration.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the Mica backdrop is supported.</returns>
	bool IBackdropSupportedWindow.TrySetMicaBackdrop(bool useMicaAlt)
	{
		if (MicaController.IsSupported())
		{
			KeepWsdqHelperNotNull();

			// Hooking up the policy object.
			_configurationSource = null;
			_configurationSource = new();
			Activated += Window_Activated;
			Closed += (_, _) => ((IBackdropSupportedWindow)this).DisposeBackdropRelatedResources();
			((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

			// Initial configuration state.
			_configurationSource.IsInputActive = true;
			SetConfigurationSourceTheme();

			_micaController = new() { Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base };

			// Enable the system backdrop.
			// Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
			_micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
			_micaController.SetSystemBackdropConfiguration(_configurationSource);
			return true; // Succeeded.
		}

		return false; // Mica is not supported on this system.
	}

	/// <summary>
	/// Try to set Acrylic backdrop.
	/// </summary>
	/// <param name="useAcrylicThin">Indicates whether the current Acrylic backdrop use thin configuration.</param>
	/// <returns>A <see cref="bool"/> value indicating whether the Acrylic backdrop is supported.</returns>
	bool IBackdropSupportedWindow.TrySetAcrylicBackdrop(bool useAcrylicThin)
	{
		if (DesktopAcrylicController.IsSupported())
		{
			KeepWsdqHelperNotNull();

			// Hooking up the policy object.
			_configurationSource = null;
			_configurationSource = new();
			Activated += Window_Activated;
			Closed += (_, _) => ((IBackdropSupportedWindow)this).DisposeBackdropRelatedResources();
			((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

			// Initial configuration state.
			_configurationSource.IsInputActive = true;
			SetConfigurationSourceTheme();

			_acrylicController = new() { Kind = useAcrylicThin ? DesktopAcrylicKind.Thin : DesktopAcrylicKind.Base };

			// Enable the system backdrop.
			// Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
			_acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
			_acrylicController.SetSystemBackdropConfiguration(_configurationSource);
			return true; // Succeeded.
		}

		return false; // Acrylic is not supported on this system.
	}

	/// <summary>
	/// Try to dispose resource of backdrop-related resources.
	/// </summary>
	void IBackdropSupportedWindow.DisposeBackdropRelatedResources()
	{
		if (_configurationSource is not null)
		{
			// Make sure any Mica/Acrylic controller is disposed so it doesn't try to use this closed window.
			if (_micaController is not null)
			{
				_micaController.Dispose();
				_micaController = null;
			}
			if (_acrylicController is not null)
			{
				_acrylicController.Dispose();
				_acrylicController = null;
			}

			Activated -= Window_Activated;
			_configurationSource = null;
		}
	}
#endif

	/// <summary>
	/// Initializes for fields.
	/// </summary>
	private void InitializeField() => NavigationPage.ParentWindow = this;

	/// <summary>
	/// Saves for preferences.
	/// </summary>
	private void SavePreference()
		=> ProgramPreferenceFileHandler.Write(CommonPaths.UserPreference, Application.Current.AsApp().Preference);

	/// <summary>
	/// Saves for puzzle generating history.
	/// </summary>
	private void SavePuzzleGeneratingHistory()
	{
		if (Application.Current is App
			{
				Preference.UIPreferences.SavePuzzleGeneratingHistory: true,
				PuzzleGeneratingHistory: { Puzzles: { Count: not 0 } puzzles } history
			})
		{
			if (File.Exists(CommonPaths.GeneratingHistory)
				&& PuzzleGeneratingHistoryFileHandler.Read(CommonPaths.GeneratingHistory) is { } @base)
			{
				@base.Puzzles.AddRange(puzzles);
				PuzzleGeneratingHistoryFileHandler.Write(CommonPaths.GeneratingHistory, @base);
			}
			else
			{
				PuzzleGeneratingHistoryFileHandler.Write(CommonPaths.GeneratingHistory, history);
			}
		}
	}

	/// <summary>
	/// Sets the state of app title bars conditionally.
	/// </summary>
	private void SetAppTitleBarState()
	{
#if SEARCH_AUTO_SUGGESTION_BOX
		AppTitleBar.Visibility = Visibility.Visible;
		AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Collapsed;
#else
		AppTitleBar.Visibility = Visibility.Collapsed;
		AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Visible;
#endif
	}

	/// <summary>
	/// Update value for open-pane length into preference.
	/// </summary>
	private void UpdateOpenPaneLengthToPreference()
		=> Application.Current.AsApp().Preference.UIPreferences.MainNavigationPageOpenPaneLength = (decimal)Round(NavigationPage.MainNavigationView.OpenPaneLength, 1);

#if CUSTOMIZED_TITLE_BAR
	/// <summary>
	/// Initializes for property <see cref="Window.AppWindow"/>.
	/// </summary>
	/// <remarks>
	/// For more information please visit <see href="https://learn.microsoft.com/en-us/windows/apps/develop/title-bar">this link</see>.
	/// This passage is for full customization of application title bar.
	/// </remarks>
	private void InitializeAppWindow()
	{
		AppWindow.Changed += appWindowChangedHandler;

		// Check to see if customization is supported. Currently only supported on Windows 11.
		if (AppWindowTitleBar.IsCustomizationSupported())
		{
			var theme = Application.Current.AsApp().Preference.UIPreferences.CurrentTheme;
			((IThemeSupportedWindow)this).ManuallySetTitleBarButtonsColor(theme);

#if SEARCH_AUTO_SUGGESTION_BOX
			AppTitleBar.Loaded += (_, _) => SetDragRegionForCustomTitleBar(AppWindow);
			AppTitleBar.SizeChanged += (_, _) => SetDragRegionForCustomTitleBar(AppWindow);
#else
			AppTitleBarWithoutAutoSuggestBox.Loaded += (_, _) => SetDragRegionForCustomTitleBar(AppWindow);
			AppTitleBarWithoutAutoSuggestBox.SizeChanged += (_, _) => SetDragRegionForCustomTitleBar(AppWindow);
#endif
		}
		else
		{
			// Title bar customization using these APIs is currently supported only on Windows 11.
			// In other cases, hide the custom title bar element.
#if SEARCH_AUTO_SUGGESTION_BOX
			AppTitleBar.Visibility = Visibility.Collapsed;
#else
			AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Collapsed;
#endif
		}

#if CUSTOMIZED_BACKDROP
		InitialSetBackdrop();
#endif


		void appWindowChangedHandler(AppWindow sender, AppWindowChangedEventArgs args)
		{
			if ((sender, args) is not ({ Presenter.Kind: var kind, TitleBar: var titleBar }, { DidPresenterChange: var didPresenterChange }))
			{
				return;
			}

			if (didPresenterChange && AppWindowTitleBar.IsCustomizationSupported())
			{
				switch (kind)
				{
					case AppWindowPresenterKind.CompactOverlay:
					{
						// Compact overlay - hide custom title bar and use the default system title bar instead.
#if SEARCH_AUTO_SUGGESTION_BOX
						AppTitleBar.Visibility = Visibility.Collapsed;
#else
						AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Collapsed;
#endif
						titleBar.ResetToDefault();
						break;
					}
					case AppWindowPresenterKind.FullScreen:
					{
						// Full screen - hide the custom title bar and the default system title bar.
#if SEARCH_AUTO_SUGGESTION_BOX
						AppTitleBar.Visibility = Visibility.Collapsed;
#else
						AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Collapsed;
#endif
						titleBar.ExtendsContentIntoTitleBar = true;
						break;
					}
					case AppWindowPresenterKind.Overlapped:
					{
						// Normal - hide the system title bar and use the custom title bar instead.
#if SEARCH_AUTO_SUGGESTION_BOX
						AppTitleBar.Visibility = Visibility.Visible;
#else
						AppTitleBarWithoutAutoSuggestBox.Visibility = Visibility.Visible;
#endif
						titleBar.ExtendsContentIntoTitleBar = true;
						SetDragRegionForCustomTitleBar(sender);
						break;
					}
					default:
					{
						// Use the default system title bar.
						titleBar.ResetToDefault();
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Sets the draggable region that is used for the whole window.
	/// </summary>
	/// <param name="appWindow">The <see cref="AppWindow"/> instance.</param>
	private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
	{
		if (AppWindowTitleBar.IsCustomizationSupported() && appWindow.TitleBar.ExtendsContentIntoTitleBar)
		{
			var scaleAdjustment = GetScaleAdjustment();
#if SEARCH_AUTO_SUGGESTION_BOX
			RightPaddingColumn.Width = new(appWindow.TitleBar.RightInset / scaleAdjustment);
			LeftPaddingColumn.Width = new(appWindow.TitleBar.LeftInset / scaleAdjustment);
			appWindow.TitleBar.SetDragRectangles(
				[
					new(
						(int)(LeftPaddingColumn.ActualWidth * scaleAdjustment),
						0,
						(int)((IconColumn.ActualWidth + TitleColumn.ActualWidth + LeftDragColumn.ActualWidth) * scaleAdjustment),
						(int)(AppTitleBar.ActualHeight * scaleAdjustment)
					),
					new(
						(int)(
							(
								LeftPaddingColumn.ActualWidth
									+ IconColumn.ActualWidth
									+ TitleTextBlock.ActualWidth
									+ LeftDragColumn.ActualWidth
									+ SearchColumn.ActualWidth
							) * scaleAdjustment
						),
						0,
						(int)(RightDragColumn.ActualWidth * scaleAdjustment),
						(int)(AppTitleBar.ActualHeight * scaleAdjustment)
					)
				]
			);
#else
			appWindow.TitleBar.SetDragRectangles(
				[
					new(
						0,
						0,
						(int)(AppTitleBarWithoutAutoSuggestBox.ActualWidth * scaleAdjustment),
						(int)(AppTitleBarWithoutAutoSuggestBox.ActualHeight * scaleAdjustment)
					)
				]
			);
#endif
		}
	}
#endif

#if CUSTOMIZED_BACKDROP
	/// <summary>
	/// Try to update configuration source theme.
	/// </summary>
	private void SetConfigurationSourceTheme()
		=> _configurationSource?.Theme = ((FrameworkElement)Content).ActualTheme switch
		{
			ElementTheme.Light => SystemBackdropTheme.Light,
			ElementTheme.Dark => SystemBackdropTheme.Dark,
			_ => App.ShouldSystemUseDarkMode() ? SystemBackdropTheme.Dark : SystemBackdropTheme.Light
		};

	/// <summary>
	/// Make <see cref="_wsdqHelper"/> not null.
	/// </summary>
	[MemberNotNull(nameof(_wsdqHelper))]
	private void KeepWsdqHelperNotNull()
	{
		if (_wsdqHelper is null)
		{
			_wsdqHelper = new();
			_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
		}
	}

	/// <summary>
	/// Initialize backdrop settings read.
	/// </summary>
	private void InitialSetBackdrop()
	{
		var backdrop = Application.Current.AsApp().Preference.UIPreferences.Backdrop;
		WindowComposition.SetBackdrop(this, backdrop);
	}
#endif

#if CUSTOMIZED_TITLE_BAR
	/// <summary>
	/// Try to set icon of the program.
	/// </summary>
	private void SetAppIcon() => AppWindow.SetIcon(@"Assets\images\Logo.ico");

	/// <summary>
	/// Try to adjust the scaling.
	/// </summary>
	/// <returns>The scaling result value.</returns>
	/// <exception cref="InvalidOperationException">Throws when the computer cannot handle scaling correctly.</exception>
	private double GetScaleAdjustment()
	{
		var hWnd = WindowNative.GetWindowHandle(this);
		var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
		var displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
		var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

		// Get DPI.
		var result = Interoperability.GetDpiForMonitor(hMonitor, MonitorDpiType.MDT_Default, out var dpiX, out _);
		if (result != 0)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("UnableGetDpi"));
		}

		var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
		return scaleFactorPercent / 100.0;
	}
#endif


	private void Window_Closed(object sender, WindowEventArgs args)
	{
		UpdateOpenPaneLengthToPreference();
		SavePreference();
		SavePuzzleGeneratingHistory();
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs args)
	{
#if CUSTOMIZED_BACKDROP
		_configurationSource?.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
#endif
	}

#if CUSTOMIZED_BACKDROP
	private void Window_ThemeChanged(FrameworkElement sender, object args)
	{
		if (_configurationSource is not null)
		{
			SetConfigurationSourceTheme();
		}
	}
#endif
}
