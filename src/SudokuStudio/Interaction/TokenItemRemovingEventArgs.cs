// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace SudokuStudio.Interaction;

/// <summary>
/// Represents for event handler data provider.
/// </summary>
/// <param name="item"><inheritdoc cref="Item" path="/summary"/></param>
/// <param name="tokenItem"><inheritdoc cref="TokenItem" path="/summary"/></param>
public class TokenItemRemovingEventArgs(object item, TokenItem tokenItem) : EventArgs
{
	/// <summary>
	/// Item being removed.
	/// </summary>
	public object Item { get; } = item;

	/// <summary>
	/// <see cref="TokenItem"/> container being closed.
	/// </summary>
	public TokenItem TokenItem { get; } = tokenItem;
}
