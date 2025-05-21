namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="CandidatePickerSelectedCandidateChangedEventHandler"/>.
/// </summary>
/// <seealso cref="CandidatePickerSelectedCandidateChangedEventHandler"/>
/// <param name="newValue"><inheritdoc cref="NewValue" path="/summary"/></param>
public sealed class CandidatePickerSelectedCandidateChangedEventArgs(Candidate newValue) : EventArgs
{
	/// <summary>
	/// The new value.
	/// </summary>
	public int NewValue { get; } = newValue;
}
