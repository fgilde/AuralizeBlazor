using System;
using AuralizeBlazor.Options;
using Nextended.Core.Helper;

namespace AuralizeBlazor.Extensions;

internal static class EnumExtensions
{
    public static string GetLabel(this Enum val)
    {
        var customAttributes = (LabelAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(LabelAttribute), false);
        return customAttributes.Length == 0 ? val.ToDescriptionString() : customAttributes[0].Text;
    }
}