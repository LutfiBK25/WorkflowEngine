using Npgsql;
using System.Text.RegularExpressions;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Infrastructure.ProcessEngine.Execution;
using WorkflowEngine.Infrastructure.ProcessEngine.Parsers;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Executors;

// New Format:
//      STATEMENT(
//          CONNECT WMS;
//          SELECT * FROM warehouse_putaway(
//              @Field_Name1,
//              @Field_Name2,
//              @Field_Name3
//          )
//      ) RETURNS(@Field_Name4, @Field_Name5)
public class DatabaseActionExecutor : IActionExecutor
{
    public static readonly DatabaseActionExecutor Instance = new();

    // Pattern to match STATEMENT(...) wrapper
    private static readonly Regex StatementPattern = new Regex(
        @"STATEMENT\s*\((.*?)\)\s*(?=RETURNS|$)",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled
    );

    // Pattern to match CONNECT statement
    private static readonly Regex ConnectPattern = new Regex(
        @"^\s*CONNECT\s+([A-Za-z0-9_]+)\s*;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private DatabaseActionExecutor() { }

    public async Task<ActionResult> ExecuteAsync(
        ExecutionSession session,
        Guid applicationId,
        Guid moduleId)
    {
        try
        {
            // Get database action module
            var dbModule = session.ModuleCache.GetModule(applicationId, moduleId) as DatabaseActionModule;
            if (dbModule == null)
            {
                return ActionResult.Fail($"Database action module {moduleId} not found");
            }

            string originalStatement = dbModule.SqlStatement;
            string sql;
            string? databaseName = null;

            // 1. Extract SQL from STATEMENT(...) wrapper
            var statementMatch = StatementPattern.Match(originalStatement);
            if (statementMatch.Success)
            {
                // SQL is inside STATEMENT(...)
                sql = statementMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Fallback: no STATEMENT wrapper, use entire statement
                sql = originalStatement;
            }

            // 2. Parse CONNECT statement from the extracted SQL
            var connectMatch = ConnectPattern.Match(sql);
            if (connectMatch.Success)
            {
                databaseName = connectMatch.Groups[1].Value;
                // Remove CONNECT statement from SQL
                sql = ConnectPattern.Replace(sql, "").TrimStart();
            }

            // 3. Parse RETURNS clause from original statement (outside STATEMENT)
            var returnFieldNames = ReturnParser.ParseReturnFields(originalStatement);

            // 4. Substitute @FieldName with actual values
            sql = FieldParser.SubstituteFieldValues(sql, session);

            // 5. Get connection string
            string? connectionString;
            if (!string.IsNullOrEmpty(databaseName))
            {
                connectionString = session.GetDatabaseCreds(databaseName);
                if (string.IsNullOrEmpty(connectionString))
                {
                    return ActionResult.Fail($"Database '{databaseName}' connection not found");
                }
            }
            else if (!string.IsNullOrEmpty(session.CurrentDatabase))
            {
                connectionString = session.GetDatabaseCreds(session.CurrentDatabase);
            }
            else
            {
                connectionString = session.ConnectionStrings.GetValueOrDefault("DEFAULT");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                return ActionResult.Fail("No database connection configured");
            }

            // 6. Execute SQL and get results
            var results = await ExecuteSqlAsync(connectionString, sql);

            // 7. Store results in session fields (if RETURNS was specified)
            if (returnFieldNames.Count > 0)
            {
                ReturnParser.StoreResults(returnFieldNames, results, session);
            }

            return ActionResult.Pass(
                $"Database action '{dbModule.Name}' executed successfully. " +
                $"Rows affected/returned: {results.Count}");
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Database execution failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Execute SQL and return first row values
    /// </summary>
    private async Task<List<object?>> ExecuteSqlAsync(string connectionString, string sql)
    {
        var results = new List<object?>();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        // Read first row
        if (await reader.ReadAsync())
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                results.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
            }
        }

        return results;
    }
}
