using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Sudoku.SourceGeneration;

/// <summary>
/// Represents a source generator that copies the whole method, and replaces some variables with cached fields.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class CachedMethodGenerator : IIncrementalGenerator
{
	#region String constants

	private const string InterceptorMethodCallerAttributeTypeFullName = "System.Diagnostics.CodeAnalysis.InterceptorMethodCallerAttribute";

	private const string InterceptorPolymorphicAttributeTypeFullName = "System.Diagnostics.CodeAnalysis.InterceptorPolymorphicAttribute";

	private const string CachedAttributeTypeFullName = "Sudoku.Analytics.Caching.MemoryCachedAttribute";

	private const string GeneratedNamespace = "Sudoku.Analytics.InterceptorGenerated";

	private const string InterceptsLocationAttributeFullName = "System.Runtime.CompilerServices.InterceptsLocationAttribute";

	private const string CommentLineBegin = "VARIABLE_DECLARATION_BEGIN";

	private const string CommentLineEnd = "VARIABLE_DECLARATION_END";

	private const string DefaultBehaviorPropertyName = "DefaultBehavior";

	private const string AttributeInsertionMatchString = "<<insert-here>>";

	private const string ThrowNotSupportedExceptionString = "throw new global::System.NotSupportedException(\"The target type of '@this' is not supported.\")";
	#endregion


	#region Diagnostic descriptors
	/// <summary>
	/// Represents message "Necessary type is missing".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0100 = new(
		nameof(IC0100),
		"Necessary type is missing",
		"Necessary type '{0}' is missing",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Method marked '[InterceptorMethodCaller]' or '[Cached]' can only be block body".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0101 = new(
		nameof(IC0101),
		"Method marked '[InterceptorMethodCaller]' or '[Cached]' can only be block body",
		"Method '{0}' marked '[InterceptorMethodCaller]' or '[Cached]' can only be block body",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Method marked '[InterceptorMethodCaller]' requires at least one invocation expression
	/// that references to a method marked '[Cached]'".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0102 = new(
		nameof(IC0102),
		"Method marked '[InterceptorMethodCaller]' requires at least one invocation expression that references to a method marked '[Cached]'",
		"Method '{0}' marked '[InterceptorMethodCaller]' requires at least one invocation expression that references to a method marked '[Cached]'",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Method marked '[Cached]' cannot be <see langword="partial"/>".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0103 = new(
		nameof(IC0103),
		"Method marked '[Cached]' cannot be partial",
		"Method '{0}' marked '[Cached]' cannot be partial",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Lacks of usage of necessary comments:
	/// '<c>VARIABLE_DECLARATION_BEGIN</c>' and '<c>VARIABLE_DECLARATION_END</c>'".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0104 = new(
		nameof(IC0104),
		$"Lacks of usage of necessary comments: '{CommentLineBegin}' and '{CommentLineEnd}'",
		$"Lacks of usage of necessary comments: '{CommentLineBegin}' and '{CommentLineEnd}'",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Duplicate comments:
	/// '<c>VARIABLE_DECLARATION_BEGIN</c>' and '<c>VARIABLE_DECLARATION_END</c>'".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0105 = new(
		nameof(IC0105),
		$"Duplicate comments: '{CommentLineBegin}' and '{CommentLineEnd}'",
		$"Duplicate comments: '{CommentLineBegin}' and '{CommentLineEnd}'",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "If method is marked '[Cached]',
	/// it is disallowed to consume instance members by using <see langword="this"/> or <see langword="base"/>".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0106 = new(
		nameof(IC0106),
		"If method is marked '[Cached]', it is disallowed to consume instance members by using 'this' or 'base'",
		"If method is marked '[Cached]', it is disallowed to consume instance members by using 'this' or 'base'",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Method marked '[Cached]' cannot be generic".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0107 = new(
		nameof(IC0107),
		"Method marked '[Cached]' cannot be generic",
		"Method '{0}' marked '[Cached]' cannot be generic",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "Method cannot be local function, lambda and the other functions that cannot be referenced by name".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0108 = new(
		nameof(IC0108),
		"Method cannot be local function, lambda and the other functions that cannot be referenced by name",
		"Method cannot be local function, lambda and the other functions that cannot be referenced by name",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);

	/// <summary>
	/// Represents message "The method is unsupported because it is generic method at present,
	/// no matter whether it has type parameter constraints".
	/// </summary>
	private static readonly DiagnosticDescriptor IC0109 = new(
		nameof(IC0109),
		"The method is unsupported because it is generic method at present, no matter whether it has type parameter constraints",
		"The method is unsupported because it is generic method at present, no matter whether it has type parameter constraints",
		"Interceptor.Design",
		DiagnosticSeverity.Error,
		true
	);
	#endregion


	#region Other static readonly fields
	/// <summary>
	/// Indicates the raw source code for intercepts location attribute.
	/// </summary>
	private static readonly string InterceptsLocationAttributeSourceCode =
		$$"""
		#nullable enable

		namespace System.Runtime.CompilerServices;

		/// <summary>
		/// <para>
		/// Interceptors are an experimental compiler feature planned to ship in .NET 8 (with support for C# only).
		/// The feature may be subject to breaking changes or removal in a future release.
		/// </para>
		/// <para>
		/// An interceptor is a method which can declaratively substitute a call to an intercept-able method with a call
		/// to itself at compile time.
		/// This substitution occurs by having the interceptor declare the source locations of the calls that it intercepts.
		/// This provides a limited facility to change the semantics of existing code
		/// by adding new code to a compilation (e.g. in a source generator).
		/// </para>
		/// </summary>
		/// <param name="filePath">The required file path.</param>
		/// <param name="line">The line.</param>
		/// <param name="character">The character position.</param>
		[global::System.AttributeUsageAttribute(global::System.AttributeTargets.Method, AllowMultiple = true)]
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("CachedMethodGenerator", "1.0")]
		[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
		internal sealed partial class InterceptsLocationAttribute(string filePath, int line, int character) : global::System.Attribute
		{
			/// <summary>
			/// Indicates the target file path.
			/// </summary>
			public string FilePath { get; } = filePath;

			/// <summary>
			/// Indicates the line number.
			/// </summary>
			public int Line { get; } = line;

			/// <summary>
			/// Indicates the character number.
			/// </summary>
			public int Character { get; } = character;
		}
		""";

	/// <summary>
	/// Represents a list of properties that will be replaced with cached members.
	/// </summary>
	/// <remarks>
	/// Please note that here we don't replace expression <c>grid.GetSolution()</c> with <c>Solution</c> at present
	/// because it'll change syntax node
	/// (from <see cref="InvocationExpressionSyntax"/> to <see cref="MemberAccessExpressionSyntax"/>).
	/// It may be considered in the future.
	/// </remarks>
	private static readonly string[] ValidVariableNames = [
		"__EmptyCells",
		"__BivalueCells",
		"__CandidatesMap",
		"__DigitsMap",
		"__ValuesMap"
	];
	#endregion


	#region Initialize method
	/// <inheritdoc/>
	/// <remarks>
	/// This generator will aim to methods, to find whether a method block contains at least one intercepted usage.
	/// If so, such method will be replaced with cached method.
	/// The requirement is that, the callee method should be applied with '<c>[InterceptorMethodCaller]</c>'.
	/// </remarks>
	public void Initialize(IncrementalGeneratorInitializationContext context)
		=> context.RegisterSourceOutput(
			context.SyntaxProvider
				.ForAttributeWithMetadataName(
					InterceptorMethodCallerAttributeTypeFullName,
					static (node, _) => node is MethodDeclarationSyntax,
					static TransformResult? (gasc, cancellationToken) =>
					{
						if (gasc is not
							{
								TargetNode: MethodDeclarationSyntax
								{
									ExpressionBody: var expressionBody,
									Identifier: { ValueText: var methodName } identifierToken
								} node,
								TargetSymbol: IMethodSymbol { MethodKind: var methodKind } currentMethodSymbol,
								SemanticModel: { Compilation: var compilation } semanticModel
							})
						{
							return null;
						}

						var interceptorInstanceTypesAttributeSymbol = compilation.GetTypeByMetadataName(InterceptorPolymorphicAttributeTypeFullName);
						if (interceptorInstanceTypesAttributeSymbol is null)
						{
							return Diagnostic.Create(IC0100, null, messageArgs: [InterceptorPolymorphicAttributeTypeFullName]);
						}

						var cachedAttributeSymbol = compilation.GetTypeByMetadataName(CachedAttributeTypeFullName);
						if (cachedAttributeSymbol is null)
						{
							return Diagnostic.Create(IC0100, null, messageArgs: [CachedAttributeTypeFullName]);
						}

						if (methodKind is MethodKind.LambdaMethod or MethodKind.LocalFunction)
						{
							return Diagnostic.Create(IC0108, null, messageArgs: null);
						}

						if (expressionBody is not null)
						{
							return Diagnostic.Create(IC0101, node.GetLocation(), messageArgs: [methodName]);
						}

						// Iterate on each invocation expression syntax node, to find any referenced methods marked '[Cached]' attribute.
						foreach (var invocation in node.DescendantNodes().OfType<InvocationExpressionSyntax>())
						{
							// Try to check referenced method data.
							// If the method cannot be found, we should skip code generation for the current method.
							var resultTriplet = default(LocationResult?);
							var invocationExpression = invocation.Expression;
							switch (semanticModel.GetSymbolInfo(invocation, cancellationToken))
							{
								case { CandidateSymbols: [IMethodSymbol { DeclaringSyntaxReferences: var syntaxRefs } methodSymbol] }
								when methodSymbol.GetAttributes().Any(cachedAttributeChecker):
								{
									resultTriplet = g(methodSymbol, invocationExpression, syntaxRefs, out var diagnostic, cancellationToken);
									if (resultTriplet is null && diagnostic is not null)
									{
										return diagnostic;
									}
									break;
								}
								case { Symbol: IMethodSymbol { DeclaringSyntaxReferences: var syntaxRefs } methodSymbol }
								when methodSymbol.GetAttributes().Any(cachedAttributeChecker):
								{
									resultTriplet = g(methodSymbol, invocationExpression, syntaxRefs, out var diagnostic, cancellationToken);
									if (resultTriplet is null && diagnostic is not null)
									{
										return diagnostic;
									}
									break;
								}
							}

							// Verify the triplet data.
							if (resultTriplet is not
								{
									Location: { SourceTree.FilePath: var filePath } invocationLocation,
									RefMethodDeclarationSyntaxNode:
									{
										Body: var referencedMethodBody,
										Modifiers: var referencedMethodModifiers
									} referencedMethodDeclaration,
									RefMethodSymbol:
									{
										ReturnType: var referencedMethodReturnType,
										Parameters: var referencedMethodParameters,
										Name: var referencedMethodName,
										IsGenericMethod: var referencedMethodIsGenericMethod
									} referencedMethodSymbol
								})
							{
								continue;
							}

							if (referencedMethodIsGenericMethod)
							{
								return Diagnostic.Create(IC0107, invocationLocation, messageArgs: [referencedMethodName]);
							}
							if (referencedMethodModifiers.Any(SyntaxKind.StaticKeyword)
								&& referencedMethodBody is not { Statements: var bodyStatements })
							{
								return Diagnostic.Create(IC0101, invocationLocation, messageArgs: [referencedMethodName]);
							}

							// Now we have the referenced method data.
							// We should determine whether the method is block-bodied, and remove variable declarations
							// bound with cached-related properties, and append attributes [InterceptsLocation], emitting referenced locations.
							// However, here we cannot collect for all possible usages of [InterceptsLocation], so it will be postponed to be checked.
							var statements = getStatements(
								bodyStatements,
								out var duplicateBeginCommentOrEndComment,
								out var existsBeginComment,
								out var existsEndComment
							);
							if (duplicateBeginCommentOrEndComment)
							{
								return Diagnostic.Create(IC0105, identifierToken.GetLocation(), messageArgs: [methodName]);
							}
							if (referencedMethodModifiers.Any(SyntaxKind.StaticKeyword) && !(existsBeginComment && existsEndComment))
							{
								return Diagnostic.Create(IC0104, identifierToken.GetLocation(), messageArgs: [methodName]);
							}

							// If the referenced method is an instance method instead of a static method,
							// we should consider inserting an extra parameter as instance member accessing.
							var instanceParameter = string.Empty;
							if (referencedMethodSymbol is { IsStatic: false, ContainingType: { IsRefLikeType: var isRefStruct } containingType })
							{
								var scopedKeyword = isRefStruct ? "scoped " : string.Empty;
								var containingTypeString = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
								instanceParameter = $"this {scopedKeyword}{containingTypeString} @this, ";

								// Check whether the referenced method declaration has instance member accessing.
								// If so, we should replace them with '@this.' invocation (or disallow now).
								var hasThisMemberAccessing = false;
							loop_tempNode:
								foreach (var tempNode in referencedMethodDeclaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
								{
									// Detect 'this.' references has two cases:
									//   1) this.Member
									//   2) Member, with 'Member' is an instance member in the current type 'containingType'
									// By check its operation, we can know which kind of the reference it is.
									try
									{
										switch (semanticModel.GetOperation(tempNode, cancellationToken))
										{
											// 1) this.Member or base.Member
											case IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance }:
											// 2) Member
											case IMemberReferenceOperation { Member: { IsStatic: false, ContainingType: var memberContainingType } }
											when SymbolEqualityComparer.Default.Equals(memberContainingType, containingType):
											{
												hasThisMemberAccessing = true;
												break loop_tempNode;
											}
										}
									}
									catch (ArgumentException)
									{
										// May throw this to report message 'Syntax node is not within syntax tree'.
									}
								}

							if (hasThisMemberAccessing)
								{
									// Today I won't handle this because it is too complex to be checked...
									return Diagnostic.Create(IC0106, identifierToken.GetLocation(), messageArgs: null);
								}
							}

							// Check whether the method marked '[InterceptorMethodCaller]' is also marked '[InterceptorInstanceTypes]'.
							// If so, we should generate an extra entry method to route functions by checking the target type
							// of this parameter '@this'.
							var attributesMarked = currentMethodSymbol.GetAttributes();
							var instanceTypesAttributeMarked = attributesMarked.FirstOrDefault(interceptorInstanceTypesAttributeChecker);
							if (instanceTypesAttributeMarked is { ConstructorArguments: [{ Values: var instanceTypesRawValue }] })
							{
								var instanceTypes = (
									from i in instanceTypesRawValue
									let targetType = i.Value as INamedTypeSymbol
									where targetType is not null
									select targetType into targetType
									let targetTypeString = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
									select (Type: targetType, TypeString: targetTypeString)
								).ToArray();
								var defaultBehavior = (InterceptorInstanceRoutingDefaultBehavior)instanceTypesAttributeMarked.GetNamedArgument(DefaultBehaviorPropertyName, 0);
								var parametersInvocationString = string.Join(
									", ",
									from parameterSymbol in referencedMethodSymbol.Parameters
									let parameterRefKind = parameterSymbol.RefKind switch
									{
										RefKind.Ref => "ref ",
										RefKind.Out => "out ",
										RefKind.RefReadOnlyParameter => "in ",
										RefKind.In => string.Empty, // in parameters may not pass with explicitly-declared 'in' keywords.
										_ => string.Empty
									}
									select $"{parameterRefKind}{parameterSymbol.Name}"
								);
								var switchBranches = string.Join(
									"\r\n\t\t\t",
									from instanceType in instanceTypes
									let targetTypeName = instanceType.Type.Name
									let targetTypeString = instanceType.TypeString
									let interceptedMethodGroup = $$"""__{{targetTypeName}}_{{referencedMethodName}}_Intercepted.{{referencedMethodSymbol.Name}}"""
									select referencedMethodReturnType.SpecialType == SpecialType.System_Void
										? $$"""
										case {{targetTypeString}} instance:
													{
														{{interceptedMethodGroup}}(instance, {{parametersInvocationString}});
														break;
													}
										"""
										: $$"""
										{{targetTypeString}} instance
														=> {{interceptedMethodGroup}}(instance, {{parametersInvocationString}}),
										"""
								);
								var isVoid = referencedMethodReturnType.SpecialType == SpecialType.System_Void;
								var defaultBehaviorString = defaultBehavior switch
								{
									InterceptorInstanceRoutingDefaultBehavior.DoNothingOrReturnDefault => isVoid ? "break" : "default",
									_ => isVoid ? $"{ThrowNotSupportedExceptionString};" : ThrowNotSupportedExceptionString
								};
								var switchStatementOrExpressionString = isVoid
									? $$"""
											switch (@this)
											{
												{{switchBranches}}
												default:
												{
													{{defaultBehaviorString}};
												}	
											}
									"""
									: $$"""
											return @this switch
											{
												{{switchBranches}},
												_ => {{defaultBehaviorString}}
											};
									""";
								var returnTypeString = getReturnTypeString(referencedMethodReturnType, referencedMethodSymbol);
								var parametersString = getParametersString(referencedMethodParameters);
								var ((startLine, startCharacter), _) = invocationLocation.GetMappedLineSpan();
								var instanceTypesGeneratedResult = new List<SuccessTransformResult>
								{
									new(
										$$"""
										/// <summary>
										/// Interceptor method that will replace implementation on such referenced method invocation.
										/// </summary>
										[global::System.CodeDom.Compiler.GeneratedCodeAttribute("CachedMethodGenerator", "1.0")]
										[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
										public static class __{{referencedMethodSymbol.ContainingType.Name}}_{{referencedMethodName}}_Intercepted
										{
											/// <summary>
											/// The backing entry that can route functions by checking the target type of instance parameter <paramref name="this"/>.
											/// </summary>
											{{AttributeInsertionMatchString}}
											[global::System.CodeDom.Compiler.GeneratedCodeAttribute("CachedMethodGenerator", "1.0")]
											[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
											public static {{returnTypeString}} {{referencedMethodName}}_SwitchEntry({{instanceParameter}}{{parametersString}})
											{
										{{switchStatementOrExpressionString}}
											}
										}
										""",
										new(filePath, startLine + 1, startCharacter + 1)
									)
								};

								foreach (var (instanceType, instanceTypeString) in instanceTypes)
								{
									if (instanceType.GetMembers().FirstOrDefault(referencedMethodSymbolChecker) is not IMethodSymbol
										{
											DeclaringSyntaxReferences: [var syntaxRef],
											Parameters: var referencedMethodParametersA,
											ReturnType: var referencedMethodReturnTypeA,
											IsGenericMethod: var isGenericMethodA
										} referencedMethodSymbolA)
									{
										continue;
									}

									if (isGenericMethodA)
									{
										// Generic methods are not supported at present.
										return Diagnostic.Create(IC0109, identifierToken.GetLocation(), messageArgs: null);
									}

									var invocationLocationA = (invocationExpression as MemberAccessExpressionSyntax)?.Name.GetLocation();
									if (invocationLocationA is null)
									{
										continue;
									}

									if (syntaxRef.GetSyntax(cancellationToken) is not MethodDeclarationSyntax
										{
											Body.Statements: var statementsA,
											SyntaxTree.FilePath: var filePathA
										})
									{
										continue;
									}

									var bodyStatementsA = getStatements(
										statementsA,
										out var duplicateBeginCommentOrEndCommentA,
										out var existsBeginCommentA,
										out var existsEndCommentA
									);
									if (duplicateBeginCommentOrEndCommentA)
									{
										continue;
									}
									if (!existsBeginCommentA || !existsEndCommentA)
									{
										continue;
									}

									instanceTypesGeneratedResult.Add(
										getResult(
											invocationLocationA,
											referencedMethodParametersA,
											referencedMethodSymbolA,
											referencedMethodReturnTypeA,
											$"{instanceTypeString} @this, ",
											bodyStatementsA,
											filePathA,
											false,
											cancellationToken
										)
									);
								}
								return instanceTypesGeneratedResult.ToArray();
							}

							// Return the value (not aggregate).
							return getResult(
								invocationLocation,
								referencedMethodParameters,
								referencedMethodSymbol,
								referencedMethodReturnType,
								instanceParameter,
								statements,
								filePath,
								true,
								cancellationToken
							);


							bool referencedMethodSymbolChecker(ISymbol member)
								=> member is IMethodSymbol && member.Name == referencedMethodName && member.IsOverride;
						}

						return Diagnostic.Create(IC0102, identifierToken.GetLocation(), messageArgs: [methodName]);


						static string getReturnTypeString(ITypeSymbol referencedMethodReturnType, IMethodSymbol referencedMethodSymbol)
						{
							var returnTypeName = referencedMethodReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
							var returnRefKindType = referencedMethodSymbol switch
							{
								{ ReturnsByRefReadonly: true } => "ref readonly ",
								{ ReturnsByRef: true } => "ref ",
								_ => string.Empty
							};
							return $"{returnRefKindType}{returnTypeName}";
						}

						static string getParametersString(ImmutableArray<IParameterSymbol> referencedMethodParameters)
						{
							return string.Join(
								", ",
								from parameter in referencedMethodParameters
								let refKindString = getRefKindString(parameter.RefKind)
								let scopedKindString = getScopedKindString(parameter.ScopedKind)
								let typeNameString = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
								let defaultValue = getDefaultValueString(parameter)
								let defaultValueString = defaultValue is null ? string.Empty : $" = {defaultValue}"
								select $"{scopedKindString}{refKindString}{typeNameString} {parameter.Name}{defaultValueString}"
							);


							static string getRefKindString(RefKind refKind)
								=> refKind switch
								{
									RefKind.Ref => "ref ",
									RefKind.Out => "out ",
									RefKind.In => "in ",
									RefKind.RefReadOnlyParameter => "ref readonly ",
									_ => string.Empty
								};

							static string getScopedKindString(ScopedKind scopedKind)
								=> scopedKind switch { ScopedKind.ScopedRef or ScopedKind.ScopedValue => "scoped ", _ => string.Empty };

							static string? getDefaultValueString(IParameterSymbol parameter)
								=> parameter.HasExplicitDefaultValue
									? parameter.ExplicitDefaultValue switch
									{
										// No default value
										null => "default",

										// String with quotes => multi-line raw string
										string and [.. var s, '"'] =>
											$""""
											"""
											{s}"
											"""
											"""",

										// String => single-line raw string
										string s =>
											$""""
											"""{s}"""
											"""",

										// Character => 'c'
										char c => $"'{c}'",

										// The other values
										var value => value.ToString()
									}
									: null;
						}

						static LocationResult? g(
							IMethodSymbol methodSymbol,
							ExpressionSyntax invocationExpression,
							ImmutableArray<SyntaxReference> syntaxRefs,
							out Diagnostic? diagnostic,
							CancellationToken cancellationToken = default
						)
						{
							if (syntaxRefs is not [var syntaxRef])
							{
								diagnostic = Diagnostic.Create(
									IC0103,
									Location.Create(syntaxRefs[0].SyntaxTree, syntaxRefs[0].Span),
									messageArgs: [invocationExpression.ToString()]
								);
								return null;
							}

							var invocationLocation = (invocationExpression as MemberAccessExpressionSyntax)?.Name.GetLocation();
							var referencedMethodDeclaration = syntaxRef.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
							var referencedMethodSymbol = methodSymbol;
							diagnostic = null;
							return new(invocationLocation, referencedMethodDeclaration, referencedMethodSymbol);
						}

						static List<StatementSyntax> getStatements(
							SyntaxList<StatementSyntax> baseStatements,
							out bool duplicateBeginCommentOrEndComment,
							out bool existsBeginComment,
							out bool existsEndComment
						)
						{
							var result = new List<StatementSyntax>();
							(var flag, (existsBeginComment, existsEndComment, duplicateBeginCommentOrEndComment)) = (false, (false, false, false));
							foreach (var statement in baseStatements)
							{
								if (statement.HasLeadingTrivia)
								{
									var leadingTrivia = statement.GetLeadingTrivia().ToFullString();
									if (leadingTrivia.Contains(CommentLineBegin))
									{
										flag = true;
										if (!existsBeginComment)
										{
											existsBeginComment = true;
										}
										else
										{
											duplicateBeginCommentOrEndComment = true;
											break;
										}
										continue;
									}
									if (leadingTrivia.Contains(CommentLineEnd))
									{
										// Get leading trivia, and only removes special line.
										var otherLines = new List<string>();
										foreach (var triviaLine in leadingTrivia.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
										{
											if (!triviaLine.Contains(CommentLineEnd))
											{
												otherLines.Add(triviaLine);
											}
										}

										result.Add(
											statement
												.WithoutLeadingTrivia()
												.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(string.Join("\r\n", otherLines)))
										);
										flag = false;
										if (!existsEndComment)
										{
											existsEndComment = true;
										}
										else
										{
											duplicateBeginCommentOrEndComment = true;
											break;
										}
										continue;
									}
								}
								if (!flag)
								{
									result.Add(statement);
								}
							}
							return result;
						}

						static SuccessTransformResult getResult(
							Location invocationLocation,
							ImmutableArray<IParameterSymbol> referencedMethodParameters,
							IMethodSymbol referencedMethodSymbol,
							ITypeSymbol referencedMethodReturnType,
							string instanceParameter,
							List<StatementSyntax> statements,
							string filePath,
							bool emitInterceptsLocationAttribute,
							CancellationToken cancellationToken = default
						)
						{
							var ((startLine, startCharacter), _) = invocationLocation.GetMappedLineSpan();
							var parametersString = getParametersString(referencedMethodParameters);
							var xmlDocCommentsStr = getXmlDocComments(referencedMethodSymbol, cancellationToken);
							var returnTypeStr = getReturnTypeString(referencedMethodReturnType, referencedMethodSymbol);
							return new(
								$$"""
								/// <summary>
								/// Interceptor method that will replace implementation on such referenced method invocation.
								/// </summary>
								[global::System.CodeDom.Compiler.GeneratedCodeAttribute("CachedMethodGenerator", "1.0")]
								[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
								public static class __{{referencedMethodSymbol.ContainingType.Name}}_{{referencedMethodSymbol.Name}}_Intercepted
								{
									{{xmlDocCommentsStr}}
									{{(emitInterceptsLocationAttribute ? AttributeInsertionMatchString : "/// <!-- Nothing inserts here -->")}}
									[global::System.CodeDom.Compiler.GeneratedCodeAttribute("CachedMethodGenerator", "1.0")]
									[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
									public static {{returnTypeStr}} {{referencedMethodSymbol.Name}}({{instanceParameter}}{{parametersString}})
									{
								{{string.Join("\r\n", getTargetStringAppendIndenting(statements))}}
									}
								}
								""",
								new(filePath, startLine + 1, startCharacter + 1)
							);


							static string getXmlDocComments(IMethodSymbol referencedMethodSymbol, CancellationToken cancellationToken = default)
							{
								// Get XML doc comments.
								// The target XML doc comment structure won't include triple slashes. We should append them;
								// In addition, consider indenting, we should append indenting \t's.
								// And, we should remove the first and last line (<member> tag).
								var xmlDocCommentsSplitByLine = referencedMethodSymbol
									.GetDocumentationCommentXml(cancellationToken: cancellationToken)?
									.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

								var xmlDocCommentLines = new List<string>();
								if (xmlDocCommentsSplitByLine is not null)
								{
									for (var i = 1; i < xmlDocCommentsSplitByLine.Length - 1; i++)
									{
										xmlDocCommentLines.Add($"/// {xmlDocCommentsSplitByLine[i].TrimStart()}");
									}
								}

								return xmlDocCommentLines.Count == 0
									? """
										/// <summary>
										/// Intercepted method.
										/// </summary>
									"""
									: string.Join("\r\n\t", xmlDocCommentLines);
							}

							static List<string> getTargetStringAppendIndenting(List<StatementSyntax> statements)
							{
								// Replace reserved identifiers with cached properties.
								var targetString = string.Join("\r\n", from statement in statements select statement.ToFullString());
								return [
									..
									ValidVariableNames
										.Aggregate(
											targetString,
											static (current, variableName) => current.Replace(variableName, variableName[2..])
										)
										.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
								];
							}
						}

						bool cachedAttributeChecker(AttributeData a)
							=> SymbolEqualityComparer.Default.Equals(a.AttributeClass, cachedAttributeSymbol);

						bool interceptorInstanceTypesAttributeChecker(AttributeData a)
							=> SymbolEqualityComparer.Default.Equals(a.AttributeClass, interceptorInstanceTypesAttributeSymbol);
					}
				)
				.Where(static data => data is not null)
				.Select(static (data, _) => data!)
				.Collect(),
			static (spc, transformResults) =>
			{
				var failed = new List<FailedTransformResult>();
				var success = new List<SuccessTransformResult>();
				foreach (var result in transformResults)
				{
					Action r = result switch
					{
						FailedTransformResult f => () => failed.Add(f),
						SuccessTransformResult s => () => success.Add(s),
						AggregateSuccessTransformResult a => () => success.AddRange(a)
					};
					r();
				}
				failed.ForEach(f => spc.ReportDiagnostic(f.Diagnostic));
				if (failed.Count != 0)
				{
					return;
				}

				var list = new List<string>();
				foreach (var resultGroup in from result in success group result by result.Text)
				{
					// Replace all with new values.
					var attribute = new List<string>();
					foreach (var (_, (filePath, lineNumber, characterNumber)) in resultGroup)
					{
						attribute.Add($""""[global::{InterceptsLocationAttributeFullName}("""{filePath}""", {lineNumber}, {characterNumber})]"""");
					}
					list.Add(resultGroup.Key.Replace(AttributeInsertionMatchString, string.Join("\r\n\t", attribute)));
				}

				spc.AddSource("InterceptsLocationAttribute.cs", InterceptsLocationAttributeSourceCode);
				spc.AddSource(
					"Intercepted.g.cs",
					$"""
					#nullable enable
					#pragma warning disable

					namespace {GeneratedNamespace};
			
					{string.Join("\r\n\r\n", list)}
					"""
				);
			}
		);
	#endregion
}

/// <summary>
/// Indicates the default behavior on routing for interceptor instance type checking.
/// </summary>
file enum InterceptorInstanceRoutingDefaultBehavior
{
	/// <summary>
	/// Indicates the default behavior is to return <see langword="default"/> or do nothing.
	/// </summary>
	DoNothingOrReturnDefault,

	/// <summary>
	/// Indicates the default behavior is to throw a <see cref="NotSupportedException"/>.
	/// </summary>
	ThrowNotSupportedException
}

/// <summary>
/// Represents a triplet of values as the location result.
/// </summary>
/// <param name="Location">Indicates the location.</param>
/// <param name="RefMethodDeclarationSyntaxNode">Indicates the referenced method declaration (syntax node).</param>
/// <param name="RefMethodSymbol">Indicates the referenced method symbol.</param>
file readonly record struct LocationResult(
	Location? Location,
	MethodDeclarationSyntax? RefMethodDeclarationSyntaxNode,
	IMethodSymbol RefMethodSymbol
);

/// <summary>
/// Represents location information for an intercepted method.
/// </summary>
/// <param name="FilePath">Indicates the file path of the target method.</param>
/// <param name="Line">Indicates the line number.</param>
/// <param name="Character">Indicates the character position.</param>
file readonly record struct InterceptedLocation(string FilePath, int Line, int Character);

/// <summary>
/// Represents a transform result.
/// </summary>
/// <param name="Success">Indicates whether the result describes for "successful".</param>
file abstract record TransformResult(bool Success)
{
	/// <summary>
	/// Implicit cast from <see cref="Diagnostic"/> to <see cref="TransformResult"/>.
	/// </summary>
	/// <param name="diagnostic">The diagnostic result.</param>
	public static implicit operator TransformResult(Diagnostic diagnostic) => new FailedTransformResult(diagnostic);

	/// <summary>
	/// Implicit cast from <see cref="SuccessTransformResult"/> array to <see cref="TransformResult"/>.
	/// </summary>
	/// <param name="results">The results.</param>
	public static implicit operator TransformResult(SuccessTransformResult[] results)
		=> new AggregateSuccessTransformResult(results);
}

/// <summary>
/// Represents a transform result after successfully to be handled.
/// </summary>
/// <param name="Text">Indicates the source text generated.</param>
/// <param name="Location">Indicates the location information.</param>
file sealed record SuccessTransformResult(string Text, InterceptedLocation Location) : TransformResult(true);

/// <summary>
/// Represents a transform result after failed to be handled.
/// </summary>
/// <param name="Diagnostic">The diagnostic result.</param>
file sealed record FailedTransformResult(Diagnostic Diagnostic) : TransformResult(false);

/// <summary>
/// Represents a transform result that creates a list of <see cref="SuccessTransformResult"/> instances to be generated once.
/// </summary>
/// <param name="Results">Indicates the results created.</param>
file sealed record AggregateSuccessTransformResult(SuccessTransformResult[] Results) :
	TransformResult(true),
	IEnumerable<SuccessTransformResult>
{
	/// <inheritdoc/>
	public IEnumerator<SuccessTransformResult> GetEnumerator() => Results.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Results.GetEnumerator();
}

file static class Extensions
{
	extension(FileLinePositionSpan @this)
	{
		public void Deconstruct(out LinePosition startLinePosition, out LinePosition endLinePosition)
			=> (startLinePosition, endLinePosition) = (@this.StartLinePosition, @this.EndLinePosition);
	}

	extension(LinePosition @this)
	{
		public void Deconstruct(out int line, out int character) => (line, character) = (@this.Line, @this.Character);
	}

	extension<TKey, TValue>(KeyValuePair<TKey, TValue> @this) where TKey : notnull
	{
		public void Deconstruct(out TKey key, out TValue value) => (key, value) = (@this.Key, @this.Value);
	}

	extension(AttributeData @this)
	{
		[return: NotNullIfNotNull(nameof(defaultValue))]
		public T? GetNamedArgument<T>(string namedArgumentName, T? defaultValue = default)
		{
			if (@this.NamedArguments is not (var namedArgs and not []))
			{
				return defaultValue;
			}

			foreach (var (name, typedConstant) in namedArgs)
			{
				if (name == namedArgumentName)
				{
					return (T?)typedConstant.Value;
				}
			}
			return defaultValue;
		}
	}
}
