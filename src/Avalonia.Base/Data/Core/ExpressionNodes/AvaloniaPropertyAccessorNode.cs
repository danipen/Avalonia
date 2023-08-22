using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Data.Core.ExpressionNodes;

internal class AvaloniaPropertyAccessorNode : ExpressionNode
{
    private readonly EventHandler<AvaloniaPropertyChangedEventArgs> _onValueChanged;

    public AvaloniaPropertyAccessorNode(AvaloniaProperty property)
    {
        Property = property;
        _onValueChanged = OnValueChanged;
    }

    public AvaloniaProperty Property { get; }

    override public void BuildString(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[builder.Length - 1] != '!')
            builder.Append('.');
        builder.Append(Property.Name);
    }

    public override bool WriteValueToSource(object? value, IReadOnlyList<ExpressionNode> nodes)
    {
        if (Source is AvaloniaObject o)
        {
            o.SetValue(Property, value);
            return true;
        }

        return false;
    }

    protected override void OnSourceChanged(object? source)
    {
        if (source is AvaloniaObject newObject)
        {
            newObject.PropertyChanged += _onValueChanged;
            SetValue(newObject.GetValue(Property));
        }
    }

    protected override void Unsubscribe(object oldSource)
    {
        if (oldSource is AvaloniaObject oldObject)
            oldObject.PropertyChanged -= _onValueChanged;
    }

    private void OnValueChanged(object? source, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Property && source is AvaloniaObject o)
            SetValue(o.GetValue(Property));
    }
}
