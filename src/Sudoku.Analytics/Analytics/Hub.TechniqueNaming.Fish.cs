namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class TechniqueNaming
	{
		/// <summary>
		/// Represents fish naming rules.
		/// </summary>
		public static class Fish
		{
			/// <summary>
			/// Gets the name of the fish via the specified size.
			/// </summary>
			/// <param name="size">The size.</param>
			/// <returns>The fish name.</returns>
			/// <exception cref="ArgumentOutOfRangeException">Throws when the argument <paramref name="size"/> is 0.</exception>
			public static string GetFishEnglishName(Digit size)
				=> size switch
				{
					0 => throw new ArgumentOutOfRangeException(nameof(size)),
					1 => "Cyclopsfish",
					2 => "X-Wing",
					3 => "Swordfish",
					4 => "Jellyfish",
					5 => "Squirmbag",
					6 => "Whale",
					7 => "Leviathan",
					_ => $"{size}-Fish"
				};

			/// <summary>
			/// Infers the technique of the specified complex fish pattern, specified as <see cref="ComplexFishStep"/>.
			/// </summary>
			/// <param name="step">The step.</param>
			/// <returns>The technique.</returns>
			public static Technique GetTechnique(ComplexFishStep step)
			{
				// Creates a buffer to store the characters that isn't a space or a bar.
				var name = internalName();
				var buffer = (stackalloc char[name.Length]);
				var bufferLength = 0;
				foreach (var ch in name)
				{
					if (ch is not ('-' or ' '))
					{
						buffer[bufferLength++] = ch;
					}
				}

				return Technique.Parse(buffer[..bufferLength]);


				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				ReadOnlySpan<char> internalName()
				{
					var finKindStr = finKind() is var finModifier and not FishFinKind.Normal
						? step.IsSiamese ? $"Siamese {finModifier} " : $"{finModifier} "
						: string.Empty;
					var shapeKindStr = shapeKind() is var shapeModifier and not FishShapeKind.Basic ? $"{shapeModifier} " : string.Empty;
					return $"{finKindStr}{shapeKindStr}{GetFishEnglishName(step.Size)}";
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				FishFinKind finKind()
					=> step.IsSashimi switch { true => FishFinKind.Sashimi, false => FishFinKind.Finned, _ => FishFinKind.Normal };

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				FishShapeKind shapeKind() => step.IsFranken ? FishShapeKind.Franken : FishShapeKind.Mutant;
			}
		}
	}
}
