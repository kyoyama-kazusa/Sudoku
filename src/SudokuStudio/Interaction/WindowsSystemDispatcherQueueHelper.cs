namespace SudokuStudio.Interaction;

#if CUSTOMIZED_BACKDROP
/// <summary>
/// Represents a helper type for <see cref="DispatcherQueue"/>.
/// </summary>
/// <seealso cref="DispatcherQueue"/>
internal sealed partial class WindowsSystemDispatcherQueueHelper
{
	/// <summary>
	/// Indicates the backing field of controller object.
	/// </summary>
	private object? _dispatcherQueueController;


	/// <summary>
	/// Try ensure controller is not <see langword="null"/> and initialize options.
	/// </summary>
	public unsafe void EnsureWindowsSystemDispatcherQueueController()
	{
		if (DispatcherQueue.GetForCurrentThread() is not null)
		{
			// one already exists, so we'll just use it.
			return;
		}

		if (_dispatcherQueueController is null)
		{
			DispatcherQueueOptions options;
			options.dwSize = sizeof(DispatcherQueueOptions);
			options.threadType = 2; // DQTYPE_THREAD_CURRENT
			options.apartmentType = 2; // DQTAT_COM_STA

			_ = CreateDispatcherQueueController(options, ref _dispatcherQueueController);
		}
	}


	/// <summary>
	/// Creates a <see cref="DispatcherQueueController"/>.
	/// Use the created <see cref="DispatcherQueueController"/> to create and manage the lifetime
	/// of a <see cref="DispatcherQueue"/> to run queued tasks in priority order on the dispatcher queue's thread.
	/// </summary>
	/// <param name="options">
	/// The threading affinity and type of COM apartment for the created <see cref="DispatcherQueueController"/>.
	/// See remarks for details.
	/// </param>
	/// <param name="dispatcherQueueController">The created dispatcher queue controller.</param>
	/// <returns><c>S_OK</c> for success; otherwise a failure code.</returns>
	/// <remarks>
	/// <para>Introduced in Windows 10, version 1709.</para>
	/// <para>
	/// If <paramref name="options"/>.<c>threadType</c> is <c>DQTYPE_THREAD_DEDICATED</c>, then this function creates a thread,
	/// initializes it with the specified COM apartment, and associates a <see cref="dispatching::DispatcherQueue"/> with that thread.
	/// The dispatcher queue event loop runs on the new dedicated thread until the dispatcher queue is explicitly shut down.
	/// To avoid thread and memory leaks, call <see cref="DispatcherQueueController.ShutdownQueueAsync"/>
	/// when you are finished with the dispatcher queue.
	/// </para>
	/// <para>
	/// If <paramref name="options"/>.<c>threadType</c> is <c>DQTYPE_THREAD_CURRENT</c>,
	/// then a <see cref="DispatcherQueue"/> is created and associated with the current thread.
	/// An error results if there is already a <see cref="DispatcherQueue"/> associated with the current thread.
	/// The current thread must pump messages to allow the dispatcher queue to dispatch tasks. Before the current thread exits,
	/// it must call <see cref="DispatcherQueueController.ShutdownQueueAsync"/>,
	/// and continue pumping messages until the <see cref="IAsyncAction"/> completes.
	/// </para>
	/// <para>
	/// This call does not return until the <see cref="DispatcherQueueController"/> and new thread (if any) are created.
	/// </para>
	/// <para>
	/// <b>Important</b><br/>
	/// The <see cref="DispatcherQueueController"/>, and its associated <see cref="DispatcherQueue"/>, are WinRT objects.
	/// See their documentation for usage details.
	/// </para>
	/// </remarks>
	[DllImport("CoreMessaging")]
	private static extern unsafe int CreateDispatcherQueueController(
		[In]
		DispatcherQueueOptions options,

		[In, Out]
		[MarshalAs(UnmanagedType.IUnknown)]
		ref object? dispatcherQueueController
	);
}
#endif
