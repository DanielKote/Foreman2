using System;

namespace Foreman;

/// <summary>Format provider for strings and numbers shown in the UI.</summary>
internal static class DisplayCulture {
    public static IFormatProvider Format => CultureInfo.CurrentCulture;
}
