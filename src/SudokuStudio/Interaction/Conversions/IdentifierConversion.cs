namespace SudokuStudio.Interaction.Conversions;

/// <summary>
/// Provides with conversion methods used by XAML designer, about <see cref="ColorDescriptor"/> instances.
/// </summary>
/// <seealso cref="ColorDescriptor"/>
internal static class IdentifierConversion
{
	public static Color GetColor(ColorDescriptor id)
	{
		var uiPref = Application.CurrentApp.Preference.UIPreferences;
		return id switch
		{
			(byte a, byte r, byte g, byte b) => Color.FromArgb(a, r, g, b),
			int idValue when getValueById(idValue, out var color) => color,
			ColorDescriptorAlias namedKind => namedKind switch
			{
				ColorDescriptorAlias.Normal => uiPref.NormalColor,
				ColorDescriptorAlias.Assignment => uiPref.AssignmentColor,
				ColorDescriptorAlias.OverlappedAssignment => uiPref.OverlappedAssignmentColor,
				ColorDescriptorAlias.Elimination => uiPref.EliminationColor,
				ColorDescriptorAlias.Cannibalism => uiPref.CannibalismColor,
				ColorDescriptorAlias.Exofin => uiPref.ExofinColor,
				ColorDescriptorAlias.Endofin => uiPref.EndofinColor,
				ColorDescriptorAlias.Link => uiPref.ChainColor,
				ColorDescriptorAlias.Auxiliary1 => uiPref.AuxiliaryColors[0],
				ColorDescriptorAlias.Auxiliary2 => uiPref.AuxiliaryColors[1],
				ColorDescriptorAlias.Auxiliary3 => uiPref.AuxiliaryColors[2],
				ColorDescriptorAlias.AlmostLockedSet1 => uiPref.AlmostLockedSetsColors[0],
				ColorDescriptorAlias.AlmostLockedSet2 => uiPref.AlmostLockedSetsColors[1],
				ColorDescriptorAlias.AlmostLockedSet3 => uiPref.AlmostLockedSetsColors[2],
				ColorDescriptorAlias.AlmostLockedSet4 => uiPref.AlmostLockedSetsColors[3],
				ColorDescriptorAlias.AlmostLockedSet5 => uiPref.AlmostLockedSetsColors[4],
				ColorDescriptorAlias.Rectangle1 => uiPref.RectangleColors[0],
				ColorDescriptorAlias.Rectangle2 => uiPref.RectangleColors[1],
				ColorDescriptorAlias.Rectangle3 => uiPref.RectangleColors[2],
				_ => throw new InvalidOperationException(SR.ExceptionMessage("SuchColorCannotBeFound"))
			},
			_ => throw new InvalidOperationException(SR.ExceptionMessage("SuchInstanceIsInvalid"))
		};


		bool getValueById(int idValue, out Color result)
		{
			var palette = uiPref.UserDefinedColorPalette;
			return (result = palette.Count > idValue ? palette[idValue] : Colors.Transparent) != Colors.Transparent;
		}
	}
}
