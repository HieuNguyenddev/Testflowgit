using System.Globalization;
using System.Text;

namespace FlowGit.API.Data;

public static class TextSearch
{
    public static string Normalize(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(char.ToLowerInvariant(character is 'đ' or 'Đ' ? 'd' : character));
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
