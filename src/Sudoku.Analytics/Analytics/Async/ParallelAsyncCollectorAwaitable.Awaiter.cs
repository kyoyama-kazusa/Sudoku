namespace Sudoku.Analytics.Async;

public partial struct ParallelAsyncCollectorAwaitable
{
	/// <summary>
	/// Represents an awaiter object that collects steps for the specified puzzle.
	/// </summary>
	public sealed class Awaiter : IStepGathererAwaiter<ReadOnlySpan<Step>>
	{
		/// <summary>
		/// Indicates the backing grid to be analyzed.
		/// </summary>
		private readonly Grid _grid;

		/// <summary>
		/// Indicates the cancellation token that can cancel the current operation.
		/// </summary>
		private readonly CancellationToken _cancellationToken;

		/// <summary>
		/// Indicates the lock.
		/// </summary>
		private readonly Lock _lock = new();

		/// <summary>
		/// Indicates the backing analyzer.
		/// </summary>
		private readonly Collector _collector;

		/// <summary>
		/// Indicates the checking tasks.
		/// </summary>
		private readonly List<Task> _stepCheckingTasks = [];

		/// <summary>
		/// Indicates the result.
		/// </summary>
		private readonly SortedDictionary<int, Step[]> _result = [];

		/// <summary>
		/// Indicates the task to await all sub-tasks.
		/// </summary>
		private Task? _awaitAllTask;

		/// <summary>
		/// Indicates the callback action on analysis operation having been finished.
		/// </summary>
		private Action? _continuation;


		/// <summary>
		/// Initializes an <see cref="Awaiter"/> instance via the specified analyzer.
		/// </summary>
		/// <param name="collector">Indicates the collector.</param>
		/// <param name="grid">Indicates the grid.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
		public Awaiter(Collector collector, in Grid grid, CancellationToken cancellationToken)
		{
			(_grid, _collector, _cancellationToken) = (grid, collector, cancellationToken);

			// Use thread pool to execute the analysis operation.
			ThreadPool.QueueUserWorkItem(CoreOperation);
		}


		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(_result))]
		public bool IsCompleted
		{
			get
			{
				lock (_lock)
				{
					return _awaitAllTask?.IsCompleted ?? false;
				}
			}
		}

		/// <inheritdoc/>
		public ReadOnlySpan<Step> Result
		{
			get
			{
				lock (_lock)
				{
					return GetResult();
				}
			}
		}

		/// <inheritdoc/>
		public Exception? Exception
		{
			get
			{
				lock (_lock)
				{
					return _awaitAllTask?.Exception;
				}
			}
		}

		/// <inheritdoc/>
		Lock IStepGathererAwaiter<ReadOnlySpan<Step>>.Lock => _lock;


		/// <summary>
		/// Returns the result value, or throw the internal exception if unhandled exception is encountered.
		/// </summary>
		/// <returns>The result value.</returns>
		public ReadOnlySpan<Step> GetResult()
		{
			// Flatten the field _result into the a whole list.
			var result = new List<Step>();
			foreach (var steps in _result.Values)
			{
				result.AddRange(steps);
			}
			return result.AsSpan();
		}

		/// <inheritdoc/>
		public void OnCompleted(Action continuation) => OnCompletedInternal(false, continuation);

		/// <inheritdoc/>
		public void UnsafeOnCompleted(Action continuation) => OnCompletedInternal(true, continuation);

		/// <inheritdoc/>
		void IStepGathererAwaiter<ReadOnlySpan<Step>>.OnCompletedInternal(bool continueOnCapturedContext, Action continuation)
			=> OnCompletedInternal(continueOnCapturedContext, continuation);

		/// <inheritdoc/>
		void IStepGathererAwaiter<ReadOnlySpan<Step>>.StartContinuation(bool continueOnCapturedContext, Action continuation)
			=> StartContinuation(continueOnCapturedContext, continuation);

		/// <inheritdoc/>
		void IStepGathererAwaiter<ReadOnlySpan<Step>>.CoreOperation(object? state) => CoreOperation(state);

		/// <summary>
		/// Executes a custom method on work having been completed.
		/// </summary>
		/// <param name="continueOnCapturedContext">Indicates whether to capture and marshal back to the current context.</param>
		/// <param name="continuation">The method to be executed.</param>
		private void OnCompletedInternal(bool continueOnCapturedContext, Action continuation)
		{
			lock (_lock)
			{
				if (IsCompleted)
				{
					StartContinuation(continueOnCapturedContext, continuation);
				}
				else
				{
					_continuation = continuation;
				}
			}
		}

		/// <summary>
		/// Determine whether we should switch execution context, and start continuation in thread pool from the context decided.
		/// </summary>
		/// <param name="continueOnCapturedContext">Indicates whether to capture and marshal back to the current context.</param>
		/// <param name="continuation">The method to be executed.</param>
		private void StartContinuation(bool continueOnCapturedContext, Action continuation)
		{
			if (continueOnCapturedContext)
			{
				ThreadPool.QueueUserWorkItem(callback);
			}
			else
			{
				var context = ExecutionContext.Capture();
				ThreadPool.QueueUserWorkItem(_ => ExecutionContext.Run(context!, callback, null));
			}


			void callback(object? _) => continuation();
		}

		/// <summary>
		/// Indicates the backing operation.
		/// </summary>
		/// <param name="state">An object containing information to be used by the callback method.</param>
		private void CoreOperation(object? state)
		{
			try
			{
				// Create a list of tasks, and assign them into the collection.
				AssignCheckingTasks();

				// Try to await them.
				_awaitAllTask = Task.WhenAll(_stepCheckingTasks);

				// Start and wait for it.
				_awaitAllTask.Wait();
			}
			catch
			{
			}
			finally
			{
				Action? continuation;
				lock (_lock)
				{
					continuation = _continuation;
					_continuation = null;
				}

				if (continuation is not null)
				{
					StartContinuation(false, continuation);
				}
			}
		}

		/// <summary>
		/// Updates field <see cref="_stepCheckingTasks"/> to get all possible tasks to be checked.
		/// </summary>
		/// <seealso cref="_stepCheckingTasks"/>
		private void AssignCheckingTasks()
		{
			if (!Enum.IsDefined(_collector.DifficultyLevelMode))
			{
				throw new InvalidOperationException(SR.ExceptionMessage("ModeIsUndefined"));
			}

			if (_grid.IsSolved)
			{
				return;
			}

			// Apply setters.
			StepGatherer.ApplySetters(_collector);

			// Initialize values.
			Initialize(_grid, _grid.SolutionGrid);

			// Create a list of tasks by the searchers to be checked.
			foreach (var searcher in _collector.ResultStepSearchers)
			{
				var task = new Task(stepsCreator, _cancellationToken);
				task.Start();
				_stepCheckingTasks.Add(task);


				void stepsCreator()
				{
					var accumulator = new List<Step>();
					var context = new StepAnalysisContext(_grid, in _grid)
					{
						Accumulator = accumulator,
						OnlyFindOne = false,
						Options = _collector.Options,
						CancellationToken = _cancellationToken
					};
					switch (searcher)
					{
						case { RunningArea: var runningArea } when !runningArea.HasFlag(StepSearcherRunningArea.Collecting):
						case { Metadata.SupportsSukaku: false } when _grid.PuzzleType == SudokuType.Sukaku:
						{
							break;
						}
						case { Level: var currentLevel }:
						{
							if (_cancellationToken.IsCancellationRequested)
							{
								return;
							}

							searcher.Collect(ref context);
							break;
						}
					}

					if (accumulator.Count == 0)
					{
						return;
					}

					// Assign the found steps into target collection.
					var key = searcher.Priority;
					if (!_result.TryAdd(key, [.. accumulator]))
					{
						_result[key].AddRange(accumulator);
					}
				}
			}
		}
	}
}
