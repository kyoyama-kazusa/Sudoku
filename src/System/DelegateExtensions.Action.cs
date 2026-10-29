namespace System;

public partial class DelegateExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	extension(Action)
	{
		/// <summary>
		/// Represents pointer to <see cref="DoNothingMethod"/>.
		/// </summary>
		/// <seealso cref="DoNothingMethod"/>
		public static delegate*<void> DoNothingMethodPtr => &DoNothingMethod;

		/// <summary>
		/// Creates an <see cref="Action"/> instance that do nothing.
		/// </summary>
		public static Action DoNothing => DoNothingMethod;


		/// <summary>
		/// Represents a method that do nothing.
		/// </summary>
		public static void DoNothingMethod()
		{
		}
	}
}
