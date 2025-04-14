namespace Sudoku.CommandLine;

/// <summary>
/// Represents a method that performs a transformation and return a <see cref="string"/> value representing the instance.
/// </summary>
/// <typeparam name="T">The type of argument.</typeparam>
/// <param name="instance">The instance.</param>
/// <returns>The string converted.</returns>
public delegate string ObjectOutputHandler<T>(in T instance) where T : allows ref struct;
