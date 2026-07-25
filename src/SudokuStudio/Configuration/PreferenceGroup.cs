namespace SudokuStudio.Configuration;

/// <summary>
/// Defines a preference group that contains a list of preference items that can be serialized and deserialized by JSON.
/// </summary>
public closed class PreferenceGroup : DependencyObject
{
	/// <summary>
	/// Try to cover the preference from the specified instance <paramref name="other"/>.
	/// </summary>
	/// <param name="other">The newer instance that is used for covering the current instance.</param>
	public void CoverBy(PreferenceGroup other)
	{
		foreach (var propertyInfo in GetType().GetProperties())
		{
			if (!propertyInfo.CanWrite)
			{
				// Cannot cover with the new value.
				continue;
			}

			if (propertyInfo.Name is nameof(Dispatcher) or nameof(DispatcherQueue))
			{
				// Special properties.
				continue;
			}

			propertyInfo.SetValue(this, propertyInfo.GetValue(other));
		}
	}
}
