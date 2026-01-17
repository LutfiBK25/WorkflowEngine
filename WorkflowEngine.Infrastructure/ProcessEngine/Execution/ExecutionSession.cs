
using System.ComponentModel.DataAnnotations;
using WorkflowEngine.Domain.ProcessEngine.Entities;
using WorkflowEngine.Domain.ProcessEngine.Entities.Modules;
using WorkflowEngine.Domain.ProcessEngine.Enums;
using WorkflowEngine.Infrastructure.ProcessEngine.Executors;

namespace WorkflowEngine.Infrastructure.ProcessEngine.Execution;

/// <summary>
/// Represents a user execution session, tracking session state, user context, and process execution frames.
/// </summary>
/// <remarks>An ExecutionSession encapsulates the state and context for a single user or process execution,
/// including session identifiers, user information, current database, and a call stack for managing nested execution
/// frames. It also supports pausing and resuming execution, as well as storing arbitrary session field values. This
/// class is not thread-safe; concurrent access should be externally synchronized if used in multi-threaded
/// scenarios.</remarks>
public class ExecutionSession
{

    // Initial Session Data
    public Guid SessionId { get; }
    public DateTime StartTime { get; }
    private readonly Guid _application;
    private readonly Guid _processModule;
    public string UserId { get; set; }
    private readonly ModuleCache _moduleCache;
    private Dictionary<string, string> _connectionStrings;

    public string CurrentDatabase { get; set; } = string.Empty; // Tracking current Databases

    private readonly Stack<ExecutionFrame> _callStack = new(); // Process Module Call Stack

    private Dictionary<Guid, object> _fieldValues = new(); // Session Fields


    // Pause/Resume Support
    // Session is waiting for an Input (Dialog Action)
    public bool IsPaused { get; set; }
    public Guid? PausedAtProcessModuleId { get; set; } // Which Process
    public int? PausedAtStep { get; set; } // Which Step
    public string PausedScreenJson { get; set; } = string.Empty; // Current Screen

    

    public ExecutionSession(Guid application,
        Guid processModule,
        string userId,
        ModuleCache moduleCache,
        Dictionary<string,string> connectionStrings)
    {
        SessionId = Guid.NewGuid();
        StartTime = DateTime.UtcNow;
        _application = application;
        _processModule = processModule;
        UserId = userId;
        _moduleCache = moduleCache;
        _connectionStrings = connectionStrings;
    }

    // We need to access these
    public ModuleCache ModuleCache => _moduleCache;
    public Dictionary<string, string> ConnectionStrings => _connectionStrings;
    public Guid ApplicationId => _application;



    #region Session Management
    // Starting the session
    public async Task<ActionResult> Start()
    {
        try
        {
            var executor = ActionExecutorRegistry.GetExecutor(ActionType.Call);

            // ✅ Fixed: Correct parameter order (moduleId, applicationId)
            return await executor.ExecuteAsync(this, _application, _processModule);
        }
        catch (NotSupportedException ex)
        {
            return ActionResult.Fail($"Process executor not found: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Session start failed: {ex.Message}", ex);
        }
    }

    public async Task<ActionResult> Restart()
    {
        _callStack.Clear();
        _fieldValues.Clear();
        CurrentDatabase = string.Empty;
        IsPaused = false;
        PausedAtProcessModuleId = null;
        PausedAtStep = null;
        PausedScreenJson = string.Empty;

        return await Start();
    }

    public void Pause(Guid processModuleId, int step, string screenJson = null)
    {
        IsPaused = true;
        PausedAtProcessModuleId = processModuleId;
        PausedAtStep = step;
        PausedScreenJson = screenJson ?? string.Empty;
    }

    public void Resume()
    {
        IsPaused = false;
        PausedAtStep = null;
        PausedScreenJson = null;
        // CurrentStep remains set to continue from next step
    }

    public bool CanResume()
    {
        return IsPaused && PausedAtProcessModuleId.HasValue && PausedAtStep.HasValue;
    }

    #endregion

    #region Fields Management
    /// <summary>
    /// Sets a field value explicitly
    /// </summary>
    public void SetFieldValue(Guid fieldId, object value)
    {
        _fieldValues[fieldId] = value;
    }

    /// <summary>
    /// Gets a field value, falling back to module's default if not set
    /// </summary>
    public object? GetFieldValue(Guid fieldId)
    {
        // First check if value was explicitly set in session
        if (_fieldValues.TryGetValue(fieldId, out var value))
        {
            return value;
        }

        // Fallback: Check if field module has a default value
        var fieldModule = _moduleCache.GetModule(_application, fieldId) as FieldModule;
        if (fieldModule?.DefaultValue != null)
        {
            // Parse default value based on field type
            return ParseDefaultValue(fieldModule.DefaultValue, fieldModule.FieldType);
        }

        // Fallback to type - specific default
        return GetTypeDefaultValue(fieldModule.FieldType);
    }

    /// <summary>
    /// Parse default value string to appropriate type
    /// </summary>
    private object? ParseDefaultValue(string defaultValue, FieldType fieldType)
    {
        if (string.IsNullOrEmpty(defaultValue))
            return null;

        try
        {
            return fieldType switch
            {
                FieldType.String => defaultValue,
                FieldType.Number => decimal.Parse(defaultValue),
                FieldType.Boolean => bool.Parse(defaultValue),
                FieldType.DateTime => DateTime.Parse(defaultValue),
                _ => defaultValue
            };
        }
        catch
        {
            // If parsing fails, return as string
            return defaultValue;
        }
    }

    /// <summary>
    /// Get default value for a field type
    /// </summary>
    private object GetTypeDefaultValue(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.String => string.Empty,           // ""
            FieldType.Number => 0m,                     // 0 (decimal)
            FieldType.Boolean => false,                 // false
            FieldType.DateTime => new DateTime(1900, 1, 1),  // 1900-01-01
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the field type for a given field module
    /// </summary>
    public FieldType? GetFieldType(Guid fieldId)
    {
        var fieldModule = _moduleCache.GetModule(_application, fieldId) as FieldModule;
        return fieldModule?.FieldType;
    }

    /// <summary>
    /// Gets the full field module definition
    /// </summary>
    public FieldModule? GetFieldModule(Guid fieldId)
    {
        return _moduleCache.GetModule(_application, fieldId) as FieldModule;
    }

    /// <summary>
    /// Gets a strongly-typed field value with validation
    /// </summary>
    public T? GetFieldValueAs<T>(Guid fieldId)
    {
        var value = GetFieldValue(fieldId);

        if (value == null)
            return default;

        try
        {
            if (value is T typedValue)
                return typedValue;

            // Try conversion
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Sets a field value with type validation
    /// </summary>
    public bool TrySetFieldValue(Guid fieldId, object value)
    {
        var fieldType = GetFieldType(fieldId);

        if (!fieldType.HasValue)
            return false; // Field module not found

        // Validate type matches
        bool isValid = fieldType.Value switch
        {
            FieldType.String => value is string,
            FieldType.Number => value is decimal or int or long or double or float,
            FieldType.Boolean => value is bool,
            FieldType.DateTime => value is DateTime,
            _ => false
        };

        if (!isValid)
            return false;

        _fieldValues[fieldId] = value;
        return true;
    }


    /// <summary>
    /// Check if field has been explicitly set (not just has default)
    /// </summary>
    public bool HasField(Guid fieldId)
    {
        return _fieldValues.ContainsKey(fieldId);
    }

    /// <summary>
    /// Check if field has a value (either set or default)
    /// </summary>
    public bool HasFieldValue(Guid fieldId)
    {
        if (_fieldValues.ContainsKey(fieldId))
            return true;

        var fieldModule = GetFieldModule(fieldId);
        return fieldModule?.DefaultValue != null;
    }

    public void ClearFields()
    {
        _fieldValues.Clear();
    }


    #endregion

    #region Call Stack Management

    /// <summary>
    /// Gets the current number of process calls on the call stack.
    /// </summary>
    public int CallDepth => _callStack.Count;

    /// <summary>
    /// Gets the execution frame at the top of the call stack, or null if the call stack is empty.
    /// </summary>
    public ExecutionFrame? CurrentFrame => _callStack.Count > 0 ? _callStack.Peek() : null;

    /// <summary>
    /// Adds the specified execution frame to the top of the call stack.
    /// </summary>
    /// <param name="frame">The execution frame to push onto the call stack. Cannot be null.</param>
    public void PushFrame(ExecutionFrame frame) => _callStack.Push(frame);


    /// <summary>
    /// Removes and returns the top execution frame from the call stack, if one is available.
    /// </summary>
    /// <returns>The removed <see cref="ExecutionFrame"/> if the call stack is not empty; otherwise, <see langword="null"/>.</returns>
    public ExecutionFrame? PopFrame() => _callStack.Count > 0 ? _callStack.Pop() : null;

    #endregion


    #region Databases Management
    public string? GetDatabaseCreds(string dbName)
    {
        _connectionStrings.TryGetValue(dbName, out string? creds);
        return creds;
    }
    #endregion

}
