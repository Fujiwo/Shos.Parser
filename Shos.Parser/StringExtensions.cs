namespace Shos.Parser;

public static class StringExtensions
{
    public static string ToPascalCase(this string @this)
    {
        if (@this.Length == 0)
            return @this;

        var result = @this.Substring(0, 1).ToUpper();
        if (@this.Length > 1)
            result += @this.Substring(1, @this.Length - 1);

        return result;
    }
}
