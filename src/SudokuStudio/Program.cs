#pragma warning disable IDE0079
#pragma warning disable CA1806

namespace SudokuStudio;

/// <summary>
/// Represents a type that encapsulates the entry point.
/// </summary>
file static class Program
{
	/// <summary>
	/// Indicates the entry point method.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	[STAThread]
	private static void Main(string[] args)
	{
		checkProcessRequirements();
		ComWrappersSupport.InitializeComWrappers();
		Application.Start(
			static _ =>
			{
				var context = new dispatching::DispatcherQueueSynchronizationContext(dispatching::DispatcherQueue.GetForCurrentThread());
				SynchronizationContext.SetSynchronizationContext(context);
				new App();
			}
		);


		[DllImport("Microsoft.ui.xaml", EntryPoint = "XamlCheckProcessRequirements")]
		static extern unsafe void checkProcessRequirements();
	}
}
