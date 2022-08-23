using System.Reflection;

namespace GitHelperApp.Generators.Markdown;

// Fork of https://raw.githubusercontent.com/jpierson/to-markdown-table/develop/src/ToMarkdownTable/LinqMarkdownTableExtensions.cs
public static class LinqMarkdownTableExtensions
{
    public static IEnumerable<string> ToMarkdownTable<T>(this IEnumerable<T> source, IEnumerable<string> columnNames = null)
    {
        var properties = typeof(T).GetRuntimeProperties();
        var fields = typeof(T)
            .GetRuntimeFields()
            .Where(f => f.IsPublic);

        var gettables = Enumerable.Union(
            properties.Select(p => new { p.Name, GetValue = (Func<object, object>)p.GetValue, Type = p.PropertyType }),
            fields.Select(p => new { p.Name, GetValue = (Func<object, object>)p.GetValue, Type = p.FieldType }));

        if (columnNames != null && columnNames.Count() != gettables.Count())
        {
            throw new ArgumentOutOfRangeException($"Can't build markdown table! Can't match columns with configuration: {string.Join(", ", columnNames)}");
        }

        columnNames ??= gettables.Select(p => p.Name); // Default column names are property names of source

        var maxColumnValues = source
            .Select(x => gettables.Select(p => p.GetValue(x)?.ToString()?.Length ?? 0))
            .Union(new[] { columnNames.Select(p => p.Length) })
            .Aggregate(
                new int[gettables.Count()].AsEnumerable(),
                (accumulate, x) => accumulate.Zip(x, Math.Max))
            .ToArray();

        var headerLine = "| " + string.Join(" | ", columnNames.Select((n, i) => n.PadRight(maxColumnValues[i]))) + " |";

        var isNumeric = new Func<Type, bool>(type =>
            type == typeof(Byte) ||
            type == typeof(SByte) ||
            type == typeof(UInt16) ||
            type == typeof(UInt32) ||
            type == typeof(UInt64) ||
            type == typeof(Int16) ||
            type == typeof(Int32) ||
            type == typeof(Int64) ||
            type == typeof(Decimal) ||
            type == typeof(Double) ||
            type == typeof(Single));

        var rightAlign = new Func<Type, char>(type => isNumeric(type) ? ':' : ' ');

        var headerDataDividerLine =
            "| " +
            string.Join(
                "| ",
                gettables.Select((g, i) => new string('-', maxColumnValues[i]) + rightAlign(g.Type))) +
            "|";

        var lines = new[]
        {
            headerLine,
            headerDataDividerLine,
        }.Union(
            source
                .Select(s =>
                    "| " + string.Join(" | ", gettables.Select((n, i) => (n.GetValue(s)?.ToString() ?? "").PadRight(maxColumnValues[i]))) + " |"));

        return lines;
    }
}