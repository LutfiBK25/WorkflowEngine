

using System.Text.RegularExpressions;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Parsers;

public static class FieldParser
{
    // Pattern to match @FieldName
    private static readonly Regex FieldReferencePattern = new Regex(
        @"@([A-Za-z0-9_]+)",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Replaces all @FieldName references with actual values from session
    /// </summary>
    public static string SubstituteFieldValues(string sql, ExecutionSession session)
    {
        return FieldReferencePattern.Replace(sql, match =>
        {
            string fieldName = match.Groups[1].Value;

            // Get field module by name
            var fieldModule = session.ModuleCache.GetModuleByName(
                session.ApplicationId,
                fieldName
            ) as FieldModule;

            if (fieldModule == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found in application");
            }

            // Get value from session
            var value = session.GetFieldValue(fieldModule.Id);

            // Format value for SQL
            return FormatValueForSql(value, fieldModule.FieldType);
        });
    }

    /// <summary>
    /// Format value appropriately for SQL based on field type
    /// </summary>
    private static string FormatValueForSql(object? value, Domain.ProcessEngine.Enums.FieldType fieldType)
    {
        if (value == null)
            return "NULL";

        return fieldType switch
        {
            Domain.ProcessEngine.Enums.FieldType.String => $"'{EscapeSqlString(value.ToString()!)}'",
            Domain.ProcessEngine.Enums.FieldType.Number => value.ToString()!,
            Domain.ProcessEngine.Enums.FieldType.Boolean => (bool)value ? "1" : "0",
            Domain.ProcessEngine.Enums.FieldType.DateTime => $"'{((DateTime)value):yyyy-MM-dd HH:mm:ss}'",
            _ => $"'{EscapeSqlString(value.ToString()!)}'"
        };
    }

    /// <summary>
    /// Escape single quotes in SQL strings
    /// </summary>
    private static string EscapeSqlString(string value)
    {
        return value.Replace("'", "''");
    }

    /// <summary>
    /// Substitute @FieldName references in plain text (for dialog messages)
    /// Returns plain text, not SQL-escaped
    /// </summary>
    public static string SubstituteFieldReferencesInText(string text, ExecutionSession session)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return FieldReferencePattern.Replace(text, match =>
        {
            string fieldName = match.Groups[1].Value;

            // Get field module by name
            var fieldModule = session.ModuleCache.GetModuleByName(
                session.ApplicationId,
                fieldName
            ) as FieldModule;

            if (fieldModule == null)
            {
                return match.Value;  // Keep original if not found
            }

            // Get value from session
            var value = session.GetFieldValue(fieldModule.Id);

            // Return as plain text (not SQL-formatted)
            return value?.ToString() ?? "";
        });
    }
}
