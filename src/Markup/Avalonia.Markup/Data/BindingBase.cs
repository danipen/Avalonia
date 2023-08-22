using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Avalonia.Data
{
    public abstract class BindingBase : IBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        public BindingBase()
        {
            FallbackValue = AvaloniaProperty.UnsetValue;
            TargetNullValue = AvaloniaProperty.UnsetValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="mode">The binding mode.</param>
        public BindingBase(BindingMode mode = BindingMode.Default)
            :this()
        {
            Mode = mode;
        }

        /// <summary>
        /// Gets or sets the <see cref="IValueConverter"/> to use.
        /// </summary>
        public IValueConverter? Converter { get; set; }

        /// <summary>
        /// Gets or sets a parameter to pass to <see cref="Converter"/>.
        /// </summary>
        public object? ConverterParameter { get; set; }

        /// <summary>
        /// Gets or sets the value to use when the binding is unable to produce a value.
        /// </summary>
        public object? FallbackValue { get; set; }

        /// <summary>
        /// Gets or sets the value to use when the binding result is null.
        /// </summary>
        public object? TargetNullValue { get; set; }

        /// <summary>
        /// Gets or sets the binding mode.
        /// </summary>
        public BindingMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the binding priority.
        /// </summary>
        public BindingPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the string format.
        /// </summary>
        public string? StringFormat { get; set; }

        public WeakReference? DefaultAnchor { get; set; }

        public WeakReference<INameScope?>? NameScope { get; set; }

        /// <inheritdoc/>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = TrimmingMessages.TypeConversionSupressWarningMessage)]
        public abstract InstancedBinding? Initiate(
            AvaloniaObject target,
            AvaloniaProperty? targetProperty,
            object? anchor = null,
            bool enableDataValidation = false);
    }
}
