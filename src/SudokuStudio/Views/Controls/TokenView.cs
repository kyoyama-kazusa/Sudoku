// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace SudokuStudio.Views.Controls;

/// <summary>
/// This is an controĺ easily visualize tokens, to create filtering experiences.
/// </summary>
[TemplatePart(Name = TokenViewScrollViewerName, Type = typeof(ScrollViewer))]
[TemplatePart(Name = TokenViewScrollBackButtonName, Type = typeof(ButtonBase))]
[TemplatePart(Name = TokenViewScrollForwardButtonName, Type = typeof(ButtonBase))]
public partial class TokenView : ListViewBase
{
	/// <summary>
	/// The name of the token view scroll viewer control.
	/// </summary>
	protected internal const string TokenViewScrollViewerName = "ScrollViewer";

	/// <summary>
	/// The name of the token view scroll-back button control.
	/// </summary>
	protected internal const string TokenViewScrollBackButtonName = "ScrollBackButton";

	/// <summary>
	/// The name of the token view scroll-forward button control.
	/// </summary>
	protected internal const string TokenViewScrollForwardButtonName = "ScrollForwardButton";


	/// <summary>
	/// The field that records the currently-selected index.
	/// </summary>
	private int _selectedIndex = -1;

	/// <summary>
	/// The scroll viewer control.
	/// </summary>
	private ScrollViewer? _tokenViewScroller;

	/// <summary>
	/// The button that can scroll back.
	/// </summary>
	private ButtonBase? _tokenViewScrollBackButton;

	/// <summary>
	/// The button that can scroll forward.
	/// </summary>
	private ButtonBase? _tokenViewScrollForwardButton;

	/// <summary>
	/// Temporary tracking of previous collections for removing events.
	/// </summary>
	private MethodInfo? _removeItemsSourceMethod;


	/// <summary>
	/// Creates a new instance of the <see cref="TokenView"/> class.
	/// </summary>
	public TokenView()
	{
		DefaultStyleKey = typeof(TokenView);

		// Container Generation Hooks
		RegisterPropertyChangedCallback(ItemsSourceProperty, ItemsSource_PropertyChanged);
		RegisterPropertyChangedCallback(SelectedIndexProperty, SelectedIndex_PropertyChanged);
	}


	/// <summary>
	/// Gets or sets if tokens are wrapped.
	/// </summary>
	[DependencyProperty]
	public partial bool IsWrapped { get; set; }

	/// <summary>
	/// Gets or sets if tokens can be removed.
	/// </summary>
	[DependencyProperty]
	public partial bool CanRemoveTokens { get; set; }

	/// <summary>
	/// Triggers when an item is removing.
	/// </summary>
	public event EventHandler<TokenItemRemovingEventArgs>? TokenItemRemoving;


	/// <inheritdoc/>
	protected override DependencyObject GetContainerForItemOverride() => new TokenItem();

	/// <inheritdoc/>
	protected override bool IsItemItsOwnContainerOverride(object item) => item is TokenItem;

	/// <inheritdoc/>
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		SelectedIndex = _selectedIndex;
		PreviewKeyDown -= TokenView_PreviewKeyDown;
		SizeChanged += TokenView_SizeChanged;
		_tokenViewScroller?.Loaded -= ScrollViewer_Loaded;

		_tokenViewScroller = GetTemplateChild(TokenViewScrollViewerName) as ScrollViewer;

		_tokenViewScroller?.Loaded += ScrollViewer_Loaded;

		PreviewKeyDown += TokenView_PreviewKeyDown;
		OnIsWrappedChanged();
	}

	/// <inheritdoc/>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);

		if (element is TokenItem c)
		{
			c.Loaded += Token_Loaded;
			c.Removing += Token_Removing;

			if (!c.IsRemoveable && ReferenceEquals(c.ReadLocalValue(TokenItem.IsRemoveableProperty), DependencyProperty.UnsetValue))
			{
				var isRemovableBinding = new Binding { Source = this, Path = new(nameof(CanRemoveTokens)), Mode = BindingMode.OneWay };
				c.SetBinding(TokenItem.IsRemoveableProperty, isRemovableBinding);
			}
		}
	}

	/// <summary>
	/// Triggers when <see cref="IsWrapped"/> is changed.
	/// </summary>
	/// <param name="oldValue">The original value.</param>
	/// <param name="newValue">The new value.</param>
	protected virtual void OnIsWrappedPropertyChanged(bool oldValue, bool newValue) => OnIsWrappedChanged();

	/// <summary>
	/// Triggers when <see cref="CanRemoveTokens"/> is changed.
	/// </summary>
	/// <param name="oldValue">The original value.</param>
	/// <param name="newValue">The new value.</param>
	protected virtual void OnCanRemoveTokensPropertyChanged(bool oldValue, bool newValue) => OnCanRemoveTokensChanged();

	/// <inheritdoc/>
	protected override void OnItemsChanged(object e) => base.OnItemsChanged((IVectorChangedEventArgs)e);


	/// <summary>
	/// Update visibility of the scroll buttons.
	/// </summary>
	private void UpdateScrollButtonsVisibility()
	{
		if (_tokenViewScrollForwardButton is not null && _tokenViewScroller is not null)
		{
			_tokenViewScrollForwardButton.Visibility = _tokenViewScroller.ScrollableWidth > 0 ? Visibility.Visible : Visibility.Collapsed;
		}
	}

	/// <summary>
	/// Get the current container item.
	/// </summary>
	/// <returns>The container item.</returns>
	private TokenItem? GetCurrentContainerItem()
		=> (XamlRoot is not null ? FocusManager.GetFocusedElement(XamlRoot) : FocusManager.GetFocusedElement()) as TokenItem;

	/// <summary>
	/// Call this method when the property <see cref="IsWrapped"/> is changed.
	/// </summary>
	private void OnIsWrappedChanged()
		=> _tokenViewScroller?.HorizontalScrollBarVisibility = IsWrapped ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Hidden;

	/// <summary>
	/// Call this method when the property <see cref="CanRemoveTokens"/> is changed.
	/// </summary>
	private void OnCanRemoveTokensChanged()
	{
	}


	[Callback]
	private static void IsWrappedPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TokenView)d).OnIsWrappedPropertyChanged((bool)e.OldValue, (bool)e.NewValue);

	[Callback]
	private static void CanRemoveTokensPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TokenView)d).OnCanRemoveTokensPropertyChanged((bool)e.OldValue, (bool)e.NewValue);


	private void SelectedIndex_PropertyChanged(DependencyObject sender, DependencyProperty dp)
	{
		// This is a workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/8257
		if (_selectedIndex == -1 && SelectedIndex > -1)
		{
			// We catch the correct SelectedIndex and save it.
			_selectedIndex = SelectedIndex;
		}
	}

	private void TokenView_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateScrollButtonsVisibility();

	private void ScrollTabBackButton_Click(object sender, RoutedEventArgs e)
		=> _tokenViewScroller?.ChangeView(_tokenViewScroller.HorizontalOffset - _tokenViewScroller.ViewportWidth, null, null);

	private void ScrollTabForwardButton_Click(object sender, RoutedEventArgs e)
		=> _tokenViewScroller?.ChangeView(_tokenViewScroller.HorizontalOffset + _tokenViewScroller.ViewportWidth, null, null);

	private void TokenViewScroller_ViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
	{
		if (_tokenViewScrollBackButton is not null)
		{
			if (e.FinalView.HorizontalOffset < 1)
			{
				_tokenViewScrollBackButton.Visibility = Visibility.Collapsed;
			}
			else if (e.FinalView.HorizontalOffset > 1)
			{
				_tokenViewScrollBackButton.Visibility = Visibility.Visible;
			}
		}

		if (_tokenViewScrollForwardButton is not null)
		{
			if (_tokenViewScroller is not null)
			{
				if (e.FinalView.HorizontalOffset > _tokenViewScroller.ScrollableWidth - 1)
				{
					_tokenViewScrollForwardButton.Visibility = Visibility.Collapsed;
				}
				else if (e.FinalView.HorizontalOffset < _tokenViewScroller.ScrollableWidth - 1)
				{
					_tokenViewScrollForwardButton.Visibility = Visibility.Visible;
				}
			}
		}
	}

	private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
	{
		if (_tokenViewScroller is not null)
		{
			_tokenViewScroller.Loaded -= ScrollViewer_Loaded;
		}
		if (_tokenViewScrollBackButton is not null)
		{
			_tokenViewScrollBackButton.Click -= ScrollTabBackButton_Click;
		}

		if (_tokenViewScrollForwardButton is not null)
		{
			_tokenViewScrollForwardButton.Click -= ScrollTabForwardButton_Click;
		}

		if (_tokenViewScroller is not null)
		{
			_tokenViewScroller.ViewChanging += TokenViewScroller_ViewChanging;
			_tokenViewScrollBackButton = _tokenViewScroller.FindDescendant(TokenViewScrollBackButtonName) as ButtonBase;
			_tokenViewScrollForwardButton = _tokenViewScroller.FindDescendant(TokenViewScrollForwardButtonName) as ButtonBase;
		}

		if (_tokenViewScrollBackButton is not null)
		{
			_tokenViewScrollBackButton.Click += ScrollTabBackButton_Click;
		}

		if (_tokenViewScrollForwardButton is not null)
		{
			_tokenViewScrollForwardButton.Click += ScrollTabForwardButton_Click;
		}

		UpdateScrollButtonsVisibility();
	}

	private void Token_Removing(object? sender, TokenItemRemovingEventArgs e)
	{
		if (ItemFromContainer(e.TokenItem) is { } item)
		{
			var args = new TokenItemRemovingEventArgs(item, e.TokenItem);
			TokenItemRemoving?.Invoke(this, args);

			if (ItemsSource is not null)
			{
				_removeItemsSourceMethod?.Invoke(ItemsSource, [item]);
			}
			else
			{
				_tokenViewScroller?.UpdateLayout();
				Items.Remove(item);
			}
		}
		UpdateScrollButtonsVisibility();
	}

	private void Token_Loaded(object sender, RoutedEventArgs e)
	{
		if (sender is TokenItem token)
		{
			token.Loaded -= Token_Loaded;
		}
	}

	private void TokenView_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
	{
		(
			e.Key switch
			{
				VirtualKey.Left => () => e.Handled = moveFocus(MoveDirection.Previous),
				VirtualKey.Right => () => e.Handled = moveFocus(MoveDirection.Next),
				VirtualKey.Back or VirtualKey.Delete => () => e.Handled = removeItem(),
				_ => new Action(@delegate.DoNothing)
			}
		)();


		bool moveFocus(MoveDirection direction)
		{
			var retVal = false;
			if (ItemFromContainer(GetCurrentContainerItem()) is DependencyObject currentItem)
			{
				var previousIndex = Items.IndexOf(currentItem);
				var index = previousIndex;

				if (direction == MoveDirection.Previous)
				{
					if (previousIndex > 0)
					{
						index--;
					}
					else
					{
						retVal = true;
					}
				}
				else if (direction == MoveDirection.Next)
				{
					if (previousIndex < Items.Count - 1)
					{
						index++;
					}
				}

				// Only do stuff if the index is actually changing
				if (index != previousIndex)
				{
					if (ContainerFromIndex(index) is TokenItem newItem)
					{
						newItem.Focus(FocusState.Keyboard);
					}
					retVal = true;
				}
			}
			return retVal;
		}

		bool removeItem()
		{
			if (GetCurrentContainerItem() is TokenItem currentContainerItem && currentContainerItem.IsRemoveable)
			{
				Items.Remove(currentContainerItem);
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	private void ItemsSource_PropertyChanged(DependencyObject sender, DependencyProperty dp)
		// Use reflection to store a 'Remove' method of any possible collection in ItemsSource
		// Cache for efficiency later.
		=> _removeItemsSourceMethod = ItemsSource?.GetType().GetMethod("Remove");
}

/// <summary>
/// Represents an internal type describing move direction.
/// </summary>
file enum MoveDirection
{
	/// <summary>
	/// Indicates the move direction is to the next side.
	/// </summary>
	Next,

	/// <summary>
	/// Indicates the move direction is to the previous side.
	/// </summary>
	Previous
}
