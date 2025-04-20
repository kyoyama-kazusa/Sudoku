namespace Sudoku.CommandLine.Commands;

/// <summary>
/// Provides print technique command.
/// </summary>
internal sealed class PrintTechniquesCommand : CommandBase
{
	/// <summary>
	/// Initializes a <see cref="PrintTechniquesCommand"/> instance.
	/// </summary>
	public PrintTechniquesCommand() : base("techniques", "Prints all built-in techniques")
	{
		OptionsCore = [new TechniqueCategoryOption(), new CultureOption()];
		this.AddRange(OptionsCore);

		this.SetHandler(HandleCore);
	}


	/// <inheritdoc/>
	public override SymbolList<Option> OptionsCore { get; }


	/// <inheritdoc/>
	protected override void HandleCore(InvocationContext context)
	{
		if (this is not ([TechniqueCategoryOption o1, CultureOption o2], _))
		{
			return;
		}

		var result = context.ParseResult;
		var category = result.GetValueForOption(o1);
		var culture = result.GetValueForOption(o2);
		scoped ReadOnlySpan<string> cultures;
		if (culture is null)
		{
			cultures = ["zh-CN", "en-US"];
		}
		else if (culture.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
		{
			cultures = ["zh-CN"];
		}
		else if (culture.Equals("en-US", StringComparison.OrdinalIgnoreCase))
		{
			cultures = ["en-US"];
		}
		else
		{
			Console.WriteLine("\e[31mSpecified culture value is invalid. The value must be either 'en-US' or 'zh-CN'.\e[0m");
			return;
		}

		foreach (var technique in
			from techniqueField in Technique.Values
			where techniqueField != Technique.None
			let @group = techniqueField.TryGetGroup() ?? TechniqueGroup.None
			where category != TechniqueGroup.None && @group == category || category == TechniqueGroup.None
			select techniqueField)
		{
			Console.WriteLine(
				string.Join(
					'\t',
					from c in cultures
					select new CultureInfo(c) into cultureInfo
					select technique.GetName(cultureInfo)
				)
			);
		}
	}
}
