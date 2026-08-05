namespace Sudoku.Solving.Asp;

/// <summary>
/// The P/Invoke wrapper of the libclingo C API (clingo 5.8.x).
/// </summary>
/// <remarks>
/// <para>
/// Wraps the minimal API subset needed to load, ground and solve an ASP program.
/// All methods returning a C <c>bool</c> are annotated with <c>[return: MarshalAs(UnmanagedType.I1)]</c>
/// to correctly handle the difference between the 1-byte C <c>stdbool.h</c> <c>bool</c> and the 4-byte .NET <c>BOOL</c>.
/// </para>
/// <para>
/// <c>clingo_symbol_t</c> is defined as <c>uint64_t</c> in clingo 5.8, represented as <see langword="ulong"/> in this file.
/// </para>
/// </remarks>
[SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "<Pending>")]
[SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "<Pending>")]
public static class ClingoInterop
{
	/// <summary>
	/// The error message string that indicates an unknown error.
	/// </summary>
	public const string UnknownErrorMessageString = "Unknown error.";

	/// <summary>
	/// Gets only the atoms declared with <c>#show</c>.
	/// </summary>
	public const uint ShowTypeShown = 2;

	/// <summary>
	/// Yield mode: produce models in the <c>clingo_solve_handle_model</c> call.
	/// </summary>
	public const uint SolveModeYield = 2;

	/// <summary>
	/// The solve result: the program is satisfiable.
	/// </summary>
	public const uint SolveResultSatisfiable = 1;


	/// <summary>
	/// The function pointer of <see cref="NoOpLogger"/>, which can be passed as the <c>logger</c> parameter to <see cref="ClingoControlNew"/>.
	/// </summary>
	/// <remarks>
	/// Annotated as a function pointer and converted to <see langword="void"/>* to finally get <see cref="nint"/>.
	/// </remarks>
	public static readonly unsafe nint NoOpLoggerPtr = (nint)(delegate* unmanaged[Cdecl]<uint, nint, nint, void>)&NoOpLogger;


	/// <summary>
	/// The static constructor of the current type, used to initialize some special design of the API.
	/// </summary>
	/// <remarks>
	/// The purpose of this static constructor is to prefer loading the clingo shared library
	/// downloaded by the repository scripts (scripts/fetch-clingo.sh / .ps1) into
	/// miscellaneous/dll/clingo, which is copied to the output directory by the projects referencing this assembly.
	/// If it is not found, returns 0 (or <see cref="nint.Zero"/>) so that .NET falls back to the system default search
	/// (e.g. LD_LIBRARY_PATH).
	/// </remarks>
	/// <seealso cref="nint.Zero"/>
	static ClingoInterop()
	{
		NativeLibrary.SetDllImportResolver(
			typeof(ClingoInterop).Assembly,
			static (libraryName, assembly, searchPath) =>
			{
				if (libraryName != "clingo")
				{
					return 0;
				}

				foreach (var fileName in ("clingo.dll", "libclingo.so", "libclingo.dylib"))
				{
					var path = Path.Combine(AppContext.BaseDirectory, fileName);
					if (File.Exists(path))
					{
						return NativeLibrary.Load(path);
					}
				}

				return 0;
			}
		);
	}


	/// <summary>
	/// Creates a new control object.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_control_new")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoControlNew(
		[In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] arguments,
		nuint argumentsSize,
		nint logger,
		nint loggerData,
		uint messageLimit,
		out nint control
	);

	/// <summary>
	/// Frees the control object.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_control_free")]
	public static extern void ClingoControlFree(nint control);

	/// <summary>
	/// Adds a piece of ASP program text to the control.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_control_add")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoControlAdd(
		nint control,
		[MarshalAs(UnmanagedType.LPStr)] string name,
		[In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] parameters,
		nuint parametersSize,
		[MarshalAs(UnmanagedType.LPStr)] string program
	);

	/// <summary>
	/// Grounds the specified program parts.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_control_ground")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoControlGround(
		nint control,
		[In] Part[] parts,
		nuint partsSize,
		nint groundCallback,
		nint groundCallbackData
	);

	/// <summary>
	/// Starts solving the currently grounded logic program.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_control_solve")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoControlSolve(
		nint control,
		uint mode,
		nint assumptions,
		nuint assumptionsSize,
		nint notify,
		nint data,
		out nint handle
	);

	/// <summary>
	/// Discards the current model and searches for the next one.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_solve_handle_resume")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSolveHandleResume(nint handle);

	/// <summary>
	/// Gets the next model (<c>*model</c> is NULL if there is no more model).
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_solve_handle_model")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSolveHandleModel(nint handle, out nint model);

	/// <summary>
	/// Stops the search and frees the handle.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_solve_handle_close")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSolveHandleClose(nint handle);

	/// <summary>
	/// Gets the number of symbols of the specified type in the model.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_model_symbols_size")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoModelSymbolsSize(nint model, uint show, out nuint size);

	/// <summary>
	/// Gets the symbols of the specified type in the model, filling the caller-provided buffer.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_model_symbols")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoModelSymbols(nint model, uint show, [Out] ulong[] symbols, nuint size);

	/// <summary>
	/// Gets the name of the function/ID symbol. The returned pointer is valid for the lifetime of the process.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_symbol_name")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSymbolName(ulong symbol, out nint name);

	/// <summary>
	/// Gets the pointer and the count of the argument list of the function/tuple symbol.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_symbol_arguments")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSymbolArguments(ulong symbol, out nint arguments, out nuint argumentsSize);

	/// <summary>
	/// Gets the value of the number symbol.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_symbol_number")]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool ClingoSymbolNumber(ulong symbol, out int number);

	/// <summary>
	/// Gets the description of the last error. Returns NULL if there is no error.
	/// </summary>
	[DllImport("clingo", EntryPoint = "clingo_error_message")]
	public static extern nint ClingoErrorMessage();

	/// <summary>
	/// Throws a <see cref="ClingoException"/> if the API call fails (returns <see langword="false"/>).
	/// </summary>
	/// <exception cref="ClingoException">Thrown when the argument <paramref name="success"/> is <see langword="false"/>.</exception>
	public static void ThrowOnError([MarshalAs(UnmanagedType.I1), DoesNotReturnIf(false)] bool success)
	{
		if (!success)
		{
			var msgPtr = ClingoErrorMessage();
			var message = msgPtr != 0 ? Marshal.PtrToStringAnsi(msgPtr) ?? UnknownErrorMessageString : UnknownErrorMessageString;
			throw new ClingoException(message);
		}
	}

	/// <summary>
	/// The no-op logger callback, used to swallow the info/warning output of clingo.
	/// </summary>
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void NoOpLogger(uint code, nint message, nint data)
	{
	}
}
