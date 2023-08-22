using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipes;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Data.Core.ExpressionNodes;
using Avalonia.Markup.Parsers;
using Avalonia.Utilities;

namespace Avalonia.Data
{
    /// <summary>
    /// A XAML binding.
    /// </summary>
    [RequiresUnreferencedCode(TrimmingMessages.ReflectionBindingRequiresUnreferencedCodeMessage)]
    public class Binding : BindingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        public Binding()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="path">The binding path.</param>
        /// <param name="mode">The binding mode.</param>
        public Binding(string path, BindingMode mode = BindingMode.Default)
            : base(mode)
        {
            Path = path;
        }

        /// <summary>
        /// Gets or sets the name of the element to use as the binding source.
        /// </summary>
        public string? ElementName { get; set; }

        /// <summary>
        /// Gets or sets the relative source for the binding.
        /// </summary>
        public RelativeSource? RelativeSource { get; set; }

        /// <summary>
        /// Gets or sets the source for the binding.
        /// </summary>
        public object? Source { get; set; }

        /// <summary>
        /// Gets or sets the binding path.
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Gets or sets a function used to resolve types from names in the binding path.
        /// </summary>
        public Func<string?, string, Type>? TypeResolver { get; set; }

        public override InstancedBinding? Initiate(
            AvaloniaObject target,
            AvaloniaProperty? targetProperty,
            object? anchor = null,
            bool enableDataValidation = false)
        {
            var nodes = new List<ExpressionNode>();
            var isRooted = false;

            // Build the expression nodes from the binding path.
            if (!string.IsNullOrEmpty(Path))
            {
                var reader = new CharacterReader(Path.AsSpan());
                var (astNodes, sourceMode) = BindingExpressionGrammar.Parse(ref reader);
                ExpressionNodeFactory.CreateFromAst(
                    astNodes,
                    TypeResolver,
                    GetNameScope(),
                    nodes,
                    out isRooted);
            }

            // If the binding isn't rooted (i.e. doesn't have a Source or start with $parent, $self,
            // #elementName etc.) then we need to add a source node. The type of source node will
            // depend on the ElementName and RelativeSource properties of the binding and if
            // neither of those are set will default to a data context node.
            if (Source is null && !isRooted &&  CreateSourceNode(targetProperty) is { } sourceNode)
                nodes.Insert(0, sourceNode);

            // If the first node is an ISourceNode then allow it to select the source; otherwise
            // use the binding source if specified, falling back to the target.
            var source = nodes.Count > 0 && nodes[0] is ISourceNode sn
                ? sn.SelectSource(Source, target, anchor ?? DefaultAnchor?.Target)
                : Source ?? target;

            // Create the binding expression and wrap it in an InstancedBinding.
            var expression = new UntypedBindingExpression(
                source,
                nodes,
                FallbackValue,
                converter: Converter,
                converterParameter: ConverterParameter,
                enableDataValidation: enableDataValidation,
                stringFormat: StringFormat,
                targetNullValue: TargetNullValue,
                targetTypeConverter: ReflectionTargetTypeConverter.Create(targetProperty));
            return new InstancedBinding(expression, Mode, Priority);
        }

        /// <summary>
        /// Hack for TreeDataTemplate to create a binding expression for an item.
        /// </summary>
        /// <param name="source">The item.</param>
        /// <remarks>
        /// Ideally we'd do this in a more generic way but didn't have time to refactor
        /// ITreeDataTemplate in time for 11.0. We should revisit this in 12.0.
        /// </remarks>
        internal UntypedBindingExpression CreateObservableForTreeDataTemplate(object source)
        {
            if (!string.IsNullOrEmpty(ElementName))
                throw new NotSupportedException("ElementName bindings are not supported in this context.");
            if (RelativeSource is not null && RelativeSource.Mode != RelativeSourceMode.DataContext)
                throw new NotSupportedException("RelativeSource bindings are not supported in this context.");
            if (Source is not null)
                throw new NotSupportedException("Source bindings are not supported in this context.");

            var nodes = new List<ExpressionNode>();
            var isRooted = false;

            if (!string.IsNullOrEmpty(Path))
            {
                var reader = new CharacterReader(Path.AsSpan());
                var (astNodes, sourceMode) = BindingExpressionGrammar.Parse(ref reader);
                ExpressionNodeFactory.CreateFromAst(
                    astNodes,
                    TypeResolver,
                    GetNameScope(),
                    nodes,
                    out isRooted);
            }

            if (isRooted)
                throw new NotSupportedException("Rooted binding paths are not supported in this context.");

            return new UntypedBindingExpression(
                source,
                nodes,
                FallbackValue,
                converter: Converter,
                converterParameter: ConverterParameter,
                targetNullValue: TargetNullValue);
        }

        private INameScope? GetNameScope()
        {
            INameScope? result = null;
            NameScope?.TryGetTarget(out result);
            return result;
        }

        private ExpressionNode? CreateSourceNode(AvaloniaProperty? targetProperty)
        {
            if (!string.IsNullOrEmpty(ElementName))
            {
                var nameScope = GetNameScope() ?? throw new InvalidOperationException(
                    "Cannot create ElementName binding when NameScope is null");
                return new NamedElementNode(nameScope, ElementName);
            }

            if (RelativeSource is not null)
                return ExpressionNodeFactory.CreateRelativeSource(RelativeSource);

            return ExpressionNodeFactory.CreateDataContext(targetProperty);
        }
    }
}
