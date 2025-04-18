namespace Sudoku.Analytics.Async;

/// <summary>
/// Provides with extension methods on <see cref="StepGatherer"/>.
/// </summary>
public static class AsyncStepGatherer
{
	/// <summary>
	/// Provides extension members on <see cref="Analyzer"/>.
	/// </summary>
	extension(Analyzer @this)
	{
		/// <summary>
		/// Asynchronously analyzes the specified puzzle.
		/// </summary>
		/// <param name="grid">The grid to be analyzed.</param>
		/// <param name="progress">The progress reporter.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>An <see cref="AsyncAnalyzerAwaitable"/> object that can analyze the puzzle asynchronously.</returns>
		public AsyncAnalyzerAwaitable AnalyzeAsync(
			in Grid grid,
			IProgress<StepGathererProgressPresenter>? progress = null,
			CancellationToken cancellationToken = default
		) => new(@this, in grid, progress, false, cancellationToken);

		/// <summary>
		/// Analyzes the specified grid, to find for all possible steps and iterate them in asynchronous way.
		/// </summary>
		/// <param name="grid">Indicates the grid to be analyzed.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>A sequence that allows user iterating on it in asynchronous way.</returns>
		public async IAsyncEnumerable<Step> EnumerateStepsAsync(
			Grid grid,
			[EnumeratorCancellation] CancellationToken cancellationToken = default
		)
		{
			var channel = Channel.CreateUnbounded<Step>(new() { SingleReader = true, SingleWriter = true });
			try
			{
				@this.StepFound += this_StepFound;
				_ = Task.Run(
					() =>
					{
						try
						{
							@this.Analyze(grid, cancellationToken: cancellationToken);
						}
						finally
						{
							channel.Writer.TryComplete();
						}
					},
					cancellationToken
				);

				while (await channel.Reader.WaitToReadAsync(cancellationToken))
				{
					if (channel.Reader.TryRead(out var step))
					{
						yield return step;
					}
				}

				// Wait for completion.
				await channel.Reader.Completion;
			}
			finally
			{
				@this.StepFound -= this_StepFound;
			}


			void this_StepFound(object? sender, AnalyzerStepFoundEventArgs e) => channel.Writer.TryWrite(e.Step);
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="Collector"/>.
	/// </summary>
	extension(Collector @this)
	{
		/// <summary>
		/// Asynchronously collects steps from a puzzle.
		/// </summary>
		/// <param name="grid">The grid to be analyzed.</param>
		/// <param name="progress">The progress reporter.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>An <see cref="ParallelAsyncCollectorAwaitable"/> object that can analyze the puzzle asynchronously.</returns>
		public AsyncCollectorAwaitable CollectAsync(
			in Grid grid,
			IProgress<StepGathererProgressPresenter>? progress = null,
			CancellationToken cancellationToken = default
		) => new(@this, in grid, progress, false, cancellationToken);

		/// <summary>
		/// Asynchronously collects steps from a puzzle, with parallel checking on all <see cref="StepSearcher"/> instances.
		/// </summary>
		/// <param name="grid">The grid to be analyzed.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		/// <returns>An <see cref="ParallelAsyncCollectorAwaitable"/> object that can analyze the puzzle asynchronously.</returns>
		public ParallelAsyncCollectorAwaitable ParallelCollectAsync(in Grid grid, CancellationToken cancellationToken = default)
			=> new(@this, in grid, cancellationToken);
	}
}
