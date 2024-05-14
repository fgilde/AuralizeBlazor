using System;

namespace AuralizeBlazor.Options;

[AttributeUsage(AttributeTargets.Field)]
internal class LabelAttribute: Attribute
{
    public string Text { get; set; }
    public LabelAttribute(string s)
    {
        Text = s;
    }
}