namespace Sudoku.Solving.Asp;

/// <summary>
/// The exception that is thrown when a libclingo API call fails.
/// </summary>
/// <param name="message"><inheritdoc cref="Exception(string)" path="/param[@name='message']"/></param>
public sealed class ClingoException(string message) : Exception(message);
