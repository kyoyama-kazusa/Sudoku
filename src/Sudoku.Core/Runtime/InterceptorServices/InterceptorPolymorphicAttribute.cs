namespace Sudoku.Runtime.InterceptorServices;

/// <summary>
/// Represents an attribute type that can be applied to a method, indicating the method uses interceptor,
/// but the target method is an instance method with inheritance. Specified types are the possible derived types of the instance.
/// </summary>
/// <param name="types"><inheritdoc cref="Types" path="/summary"/></param>
/// <remarks>
/// <para>
/// This attribute will be used by source generator to generate an extra entry to consume all possible types,
/// which is useful for <see langword="abstract"/>, <see langword="virtual"/> and <see langword="sealed"/> methods.
/// </para>
/// <para>
/// Usage:
/// <code><![CDATA[
/// [InterceptorMethodCaller]
/// [InterceptorPolymorphic(typeof(A), typeof(B))]
/// public static void Method()
/// {
///     // Implementation may use a method whose bound instance type is the base type of 'A' and 'B',
///     // specified in 'InterceptorPolymorphicAttribute'.
/// }
/// ]]></code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class InterceptorPolymorphicAttribute(params Type[] types) : Attribute
{
	/// <summary>
	/// Indicates the routing default behavior on generated method for <see langword="default"/> label
	/// or <see langword="_"/> token in <see langword="switch"/> statement or expression respectively.
	/// </summary>
	public InterceptorPolymorphicBehavior DefaultBehavior { get; init; }

	/// <summary>
	/// Indicates all possible types.
	/// </summary>
	public Type[] Types { get; } = types;
}
