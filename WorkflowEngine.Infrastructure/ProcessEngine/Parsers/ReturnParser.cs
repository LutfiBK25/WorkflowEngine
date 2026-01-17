using System.Text.RegularExpressions;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Parsers;

public static class ReturnParser
{
    // Updated pattern to match RETURNS outside STATEMENT()
    // Matches: RETURNS(@Field1, @Field2, ...)
    private static readonly Regex ReturnsPattern = new Regex(
        @"RETURNS\s*\(\s*(@[A-Za-z0-9_]+(?:\s*,\s*@[A-Za-z0-9_]+)*)\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    // Pattern to extract individual field names
    private static readonly Regex FieldNamePattern = new Regex(
        @"@([A-Za-z0-9_]+)",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Parse RETURNS clause and return field names
    /// Works with both formats:
    /// - Old: ... RETURNS(@Field1, @Field2);
    /// </summary>
    public static List<string> ParseReturnFields(string sql)
    {
        var match = ReturnsPattern.Match(sql);

        if (!match.Success)
        {
            return new List<string>();
        }

        string returnClause = match.Groups[1].Value;
        var fieldNames = new List<string>();

        var fieldMatches = FieldNamePattern.Matches(returnClause);
        foreach (Match fieldMatch in fieldMatches)
        {
            fieldNames.Add(fieldMatch.Groups[1].Value);
        }

        return fieldNames;
    }

    /// <summary>
    /// Remove RETURNS clause from SQL (not needed anymore with STATEMENT wrapper)
    /// Kept for backward compatibility
    /// </summary>
    public static string RemoveReturnsClause(string sql)
    {
        return ReturnsPattern.Replace(sql, "").TrimEnd();
    }

    /// <summary>
    /// Store query results in session fields
    /// </summary>
    public static void StoreResults(
        List<string> returnFieldNames,
        List<object?> resultValues,
        ExecutionSession session)
    {
        if (returnFieldNames.Count != resultValues.Count)
        {
            throw new InvalidOperationException(
                $"Return field count mismatch: expected {returnFieldNames.Count}, got {resultValues.Count}");
        }

        for (int i = 0; i < returnFieldNames.Count; i++)
        {
            string fieldName = returnFieldNames[i];
            object? value = resultValues[i];

            // Get field module by name
            var fieldModule = session.ModuleCache.GetModuleByName(
                session.ApplicationId,
                fieldName
            ) as FieldModule;

            if (fieldModule == null)
            {
                throw new InvalidOperationException($"Return field '{fieldName}' not found in application");
            }

            // Store value in session
            session.SetFieldValue(fieldModule.Id, value ?? DBNull.Value);
        }
    }
}
