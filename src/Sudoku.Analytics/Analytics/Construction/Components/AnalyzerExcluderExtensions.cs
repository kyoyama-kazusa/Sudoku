namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Provides with extension methods on <see cref="AnalysisResult"/> to check for excluders.
/// </summary>
/// <seealso cref="AnalysisResult"/>
public static partial class AnalysisResultExcluderExtensions
{
	[GeneratedRegex("""(Row|Column)HiddenSingle\d{3}""", RegexOptions.Compiled)]
	private static partial Regex SubtypeTextPattern { get; }


	/// <summary>
	/// Provides extension members on <see cref="AnalysisResult"/>.
	/// </summary>
	extension(AnalysisResult @this)
	{
		/// <summary>
		/// Indicates whether the solving steps contains at least one <see cref="Step"/>
		/// using hidden singles in line, with block excluders.
		/// </summary>
		public bool HasBlockExcluders
		{
			get
			{
				foreach (var step in @this.StepsSpan)
				{
					if (step is not HiddenSingleStep { Subtype: var subtype })
					{
						continue;
					}

					var text = subtype.ToString();
					if (!SubtypeTextPattern.IsMatch(text))
					{
						continue;
					}

					if (text[^3] != '0')
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
