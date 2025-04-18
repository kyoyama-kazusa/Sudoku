// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace SudokuStudio.Views.Controls;

/// <summary>
/// Represents for a token item.
/// </summary>
[ContentProperty(Name = nameof(Content))]
[TemplatePart(Name = TokenItemRemoveButtonName, Type = typeof(ButtonBase))]
public partial class TokenItem : ListViewItem
{
	/// <summary>
	/// The template case for left-side icon state.
	/// </summary>
	protected internal const string IconLeftState = "IconLeft";

	/// <summary>
	/// The template case for icon-only state.
	/// </summary>
	protected internal const string IconOnlyState = "IconOnly";

	/// <summary>
	/// The template case for content-only state.
	/// </summary>
	protected internal const string ContentOnlyState = "ContentOnly";

	/// <summary>
	/// The template case for the state remove-button being visible.
	/// </summary>
	protected internal const string RemoveButtonVisibleState = "RemoveButtonVisible";

	/// <summary>
	/// The template case for the state remove-button being invisible.
	/// </summary>
	protected internal const string RemoveButtonNotVisibleState = "RemoveButtonNotVisible";

	/// <summary>
	/// The name of the token item remove button in the template.
	/// </summary>
	protected internal const string TokenItemRemoveButtonName = "PART_RemoveButton";


	/// <summary>
	/// The target remove button control.
	/// </summary>
	internal ButtonBase? _tokenItemRemoveButton;


	/// <summary>
	/// Initializes a <see cref="TokenItem"/> instance.
	/// </summary>
	public TokenItem() => DefaultStyleKey = typeof(TokenItem);


	/// <summary>
	/// Gets or sets a value indicating whether the tab can be closed by the user with the close button.
	/// </summary>
	[DependencyProperty]
	public partial bool IsRemoveable { get; set; }

	/// <summary>
	/// Gets or sets the icon.
	/// </summary>
	[DependencyProperty]
	public partial IconElement Icon { get; set; }


	/// <summary>
	/// Fired when the Tab's close button is clicked.
	/// </summary>
	public event EventHandler<TokenItemRemovingEventArgs>? Removing;


	/// <inheritdoc/>
	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		_tokenItemRemoveButton?.Click -= TokenItemRemoveButton_Click;

		_tokenItemRemoveButton = GetTemplateChild(TokenItemRemoveButtonName) as ButtonBase;

		_tokenItemRemoveButton?.Click += TokenItemRemoveButton_Click;

		IconChanged();
		ContentChanged();
		IsRemoveableChanged();
	}

	/// <inheritdoc/>
	protected override void OnContentChanged(object oldContent, object newContent)
	{
		base.OnContentChanged(oldContent, newContent);
		ContentChanged();
	}

	/// <summary>
	/// Callback method for changing <see cref="Icon"/> property.
	/// </summary>
	/// <param name="oldValue">The original value.</param>
	/// <param name="newValue">The new value.</param>
	protected virtual void OnIconPropertyChanged(IconElement oldValue, IconElement newValue) => IconChanged();

	/// <summary>
	/// Callback method for changing <see cref="IsRemoveable"/> property.
	/// </summary>
	/// <param name="oldValue">The original value.</param>
	/// <param name="newValue">The new value.</param>
	protected virtual void OnIsRemoveablePropertyChanged(bool oldValue, bool newValue) => IsRemoveableChanged();

	/// <summary>
	/// Call this method when the content is changed.
	/// </summary>
	private void ContentChanged() => VisualStateManager.GoToState(this, Content is not null ? IconLeftState : IconOnlyState, true);

	/// <summary>
	/// Call this method when the icon is changed.
	/// </summary>
	private void IconChanged() => VisualStateManager.GoToState(this, Icon is not null ? IconLeftState : ContentOnlyState, true);

	/// <summary>
	/// Call this method when <see cref="IsRemoveable"/> is changed.
	/// </summary>
	private void IsRemoveableChanged()
		=> VisualStateManager.GoToState(this, IsRemoveable ? RemoveButtonVisibleState : RemoveButtonNotVisibleState, true);


	[Callback]
	private static void IsRemoveablePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TokenItem)d).OnIsRemoveablePropertyChanged((bool)e.OldValue, (bool)e.NewValue);

	[Callback]
	private static void IconPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TokenItem)d).OnIconPropertyChanged((IconElement)e.OldValue, (IconElement)e.NewValue);


	private void TokenItemRemoveButton_Click(object sender, RoutedEventArgs e)
	{
		if (IsRemoveable)
		{
			Removing?.Invoke(this, new(Content, this));
		}
	}
}
