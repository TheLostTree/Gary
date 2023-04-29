using Vortice.DXGI;

namespace Gary.Widgets.DamageTracker.Util;

public class NumberFormat
{
    public static String Format(double num)
    {
        return $"{num:0.00}";
    }
}