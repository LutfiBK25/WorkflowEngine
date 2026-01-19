📚 WORKFLOWENGINE - COMPLETE SYSTEM DOCUMENTATION

📖 Table of Contents

1.	[Project Overview](#1-project-overview)
2.	[Architecture & Design Principles](#2-architecture--design-principles)
3.	[Solution Structure](#3-solution-structure)
4.	[Domain Layer](#4-domain-layer)
5.	[Infrastructure Layer](#5-infrastructure-layer)
6.	[Application Layer](#6-application-layer)
7.	[API Layer](#7-api-layer)
8.	[Execution Flow](#8-execution-flow)
9.	[Module Types Deep Dive](#9-module-types-deep-dive)
10.	[Session Management](#10-session-management)
11.	[Configuration & Deployment](#11-configuration--deployment)
12.	[Security Considerations](#12-security-considerations)
13.	[Troubleshooting Guide](#13-troubleshooting-guide)

---

## ⚠️ IMPORTANT NOTES

**Known Code Issue:**
There is a typo in the codebase that should be corrected:
- `ActionType.Calcualte` should be `ActionType.Calculate` (missing 'l')
- This typo appears in:
  - `WorkflowEngine.Domain/ProcessEngine/Enums/ActionType.cs`
  - `WorkflowEngine.Infrastructure/ProcessEngine/Executors/ActionExecutorRegistry.cs`

**Application Layer Status:**
- The Application layer services (`ISessionService`, `IProcessEngineService`) are currently empty placeholders
- They are registered in DI but contain no implementation
- Direct usage of Infrastructure components (SessionManager, ExecutionEngine) is currently done from calling code

**API Layer Status:**
- The `WorkflowEngine.API/Controllers` folder is empty
- API endpoints need to be implemented as outlined in Section 8

---

## 1. PROJECT OVERVIEW

### 1.1 What is WorkflowEngine?
WorkflowEngine is a powerful, flexible workflow execution system built with .NET 10 that enables dynamic process automation with:
•	✅ Database integration
•	✅ Interactive user dialogs (pause/resume)
•	✅ Complex calculations
•	✅ Conditional logic
•	✅ Nested subprocess calls

### 1.2 Key Features
Feature	Description
Dynamic Execution	Execute workflows without recompiling code
Module-Based	Reusable building blocks (Process, Database, Dialog, Field, Compare, Calculate)
Pause/Resume	Workflows can pause for user input and resume later
Multi-Database	Connect to multiple PostgreSQL databases within one workflow
Call Stack Management	Nested subprocess calls up to 20 levels deep
Session Per User	One active workflow session per user
Field System	Strongly-typed variables with default value fallbacks
Human-Readable SQL	Use @FieldName syntax instead of GUIDs

### 1.3 Technology Stack
•	Framework: .NET 10
•	Language: C# 14.0
•	Database: PostgreSQL 16
•	ORM: Entity Framework Core (Database-First, TPC Strategy)
•	API: ASP.NET Core Minimal API
•	Architecture: Clean Architecture (Domain → Infrastructure → Application → API)

---

## 2. ARCHITECTURE & DESIGN PRINCIPLES

### 2.1 Clean Architecture Layers

┌─────────────────────────────────────────────────────────────┐
│                    WorkflowEngine.API                        │
│  - REST Controllers                                          │
│  - HTTP Request/Response                                     │
│  - Minimal security (for internal use)                       │
└─────────────────────────────────────────────────────────────┘
                            ↓ (depends on)
┌─────────────────────────────────────────────────────────────┐
│                WorkflowEngine.Application                    │
│  - Service Layer (ISessionService, IProcessEngineService)    │
│  - Business Logic Orchestration                              │
│  - DTOs / View Models (if needed)                            │
└─────────────────────────────────────────────────────────────┘
                            ↓ (depends on)
┌─────────────────────────────────────────────────────────────┐
│              WorkflowEngine.Infrastructure                   │
│  - ExecutionEngine & Executors                               │
│  - Database Context (EF Core)                                │
│  - Session Management (InMemorySessionStore)                 │
│  - Background Workers                                         │
└─────────────────────────────────────────────────────────────┘
                            ↓ (depends on)
┌─────────────────────────────────────────────────────────────┐
│                  WorkflowEngine.Domain                       │
│  - Entities (Application, Module, ProcessModuleDetail)       │
│  - Enums (ActionType, FieldType, ModuleType, etc.)          │
│  - Pure domain logic (NO dependencies)                       │
└─────────────────────────────────────────────────────────────┘

### 2.2 Key Design Patterns

Pattern	            Where Used	                        Purpose
Singleton	        All Executors	                    Zero allocation, thread-safe
Strategy	        IActionExecutor interface	        Pluggable execution logic
Registry	        ActionExecutorRegistry	            Map ActionType → Executor
Builder	            DialogExecutor JSON generation	    Construct complex objects
Session	            ExecutionSession	                Stateful execution context
Repository	        RepositoryDBContext	                Data access abstraction
TPC Inheritance	    Module hierarchy	                Table Per Concrete Type


### 2.3 Core Principles
1.	Stateless Executors: All executors are singletons with no instance state
2.	Stateful Sessions: ExecutionSession holds all runtime state
3.	Fail-Fast: Early validation with descriptive error messages
4.	Explicit Nullability: C# nullable reference types enabled
5.	One Session Per User: Simplified session management
6.	Human-Readable: @FieldName instead of GUIDs in SQL
7.	Separation of Concerns: Each layer has clear responsibilities
---


## 3. SOLUTION STRUCTURE

### 3.1 Project Layout

WorkflowEngine/
├── WorkflowEngine.Domain/                   # Domain entities & enums
│   └── ProcessEngine/
│       ├── Entities/
│       │   ├── Application.cs
│       │   └── Modules/
│       │       ├── Module.cs (abstract base)
│       │       ├── ProcessModule.cs (includes ProcessModuleDetail)
│       │       ├── DatabaseActionModule.cs
│       │       ├── DialogActionModule.cs
│       │       ├── FieldModule.cs
│       │       ├── CompareActionModule.cs
│       │       └── CalculateActionModule.cs (includes CalculateModuleDetail)
│       └── Enums/
│           ├── ActionType.cs
│           ├── ModuleType.cs
│           ├── FieldType.cs
│           ├── CompareOperator.cs
│           └── CalculateOperator.cs
│
├── WorkflowEngine.Infrastructure/           # Core engine implementation
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   ├── ProcessEngine/
│   │   ├── ExecutionEngine.cs              # Main engine class
│   │   ├── ModuleCache.cs                  # In-memory module cache
│   │   ├── Execution/
│   │   │   ├── ExecutionSession.cs         # Session state management
│   │   │   ├── ActionResult.cs             # Execution result wrapper (includes ExecutionResult enum)
│   │   │   └── ExecutionFrame.cs           # Call stack frame
│   │   ├── Executors/
│   │   │   ├── IActionExecutor.cs          # Executor interface
│   │   │   ├── ActionExecutorRegistry.cs   # Executor registry
│   │   │   ├── ProcessModuleExecutor.cs    # Execute workflows
│   │   │   ├── DatabaseActionExecutor.cs   # Execute SQL
│   │   │   ├── DialogExecutor.cs           # Pause for input
│   │   │   ├── CompareExecutor.cs          # Conditional logic
│   │   │   └── CalculateExecutor.cs        # Multi-step calculations
│   │   ├── Parsers/
│   │   │   ├── FieldParser.cs              # @FieldName → values
│   │   │   └── ReturnParser.cs             # RETURNS clause parsing
│   │   ├── Services/
│   │   │   └── ApplicationsService.cs      # Load modules into cache
│   │   ├── Workers/
│   │   │   └── WorkflowEngineWorker.cs     # Background cache loader
│   │   └── Persistence/
│   │       └── RepositoryDBContext.cs      # EF Core DbContext
│   └── Session/
│       ├── ISessionStore.cs                # Session storage interface
│       ├── InMemorySessionStore.cs         # In-memory implementation
│       ├── SessionManager.cs               # High-level session API
│       └── Workers/
│           └── SessionCleanupService.cs    # Background cleanup
│
├── WorkflowEngine.Application/             # Application services layer
│   ├── Extentions/
│   │   └── ServiceCollectionExtentions.cs
│   ├── Session/
│   │   ├── Dtos/                       # (Empty - reserved for future)
│   │   ├── Interfaces/                 # (Empty - reserved for future)
│   │   └── Services/
│   │       ├── ISessionService.cs      # (Currently empty interface)
│   │       └── SessionService.cs       # (Currently empty implementation)
│   └── ProcessEngine/
│       ├── Dtos/                       # (Empty - reserved for future)
│       ├── Interfaces/                 # (Empty - reserved for future)
│       └── Services/
│           ├── IProcessEngineService.cs    # (Currently empty interface)
│           └── ProcessEngineService.cs     # (Currently empty implementation)
│
└── WorkflowEngine.API/                     # Web API project
    ├── Controllers/                        # (Empty - API controllers to be added)
    ├── Program.cs                          # Application entry point
    ├── appsettings.json                    # Configuration
    └── Properties/
        └── launchSettings.json             # Launch profiles

---

## 4. DOMAIN LAYER

### 4.1 Core Entities

**Application**

public class Application
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // Application name
    public string Version { get; set; }           // Version number
    public string? VersionBuild { get; set; }
    public bool ActivateOnStart { get; set; }     // Load on engine start?
    public DateTime? LastCompiled { get; set; }
    public DateTime? LastActivated { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

**Module (Abstract Base)**

public abstract class Module
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public ModuleType ModuleType { get; set; }
    public int Version { get; set; }
    public string Name { get; set; }              // Module name (unique per app)
    public string? Description { get; set; }
    public string? LockedBy { get; set; }         // Designer lock
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

Concrete Module Types:
1.	ProcessModule - Workflow definitions
2.	DatabaseActionModule - SQL execution
3.	DialogActionModule - User input prompts
4.	FieldModule - Variable definitions
5.	CompareActionModule - Conditional comparisons
6.	CalculateActionModule - Multi-step calculations

### 4.2 Module Type Details

**ProcessModule**

Contains workflow definitions with multiple steps.

public class ProcessModule : Module
{
    public string? Comment { get; set; }
    public List<ProcessModuleDetail> Details { get; set; } = new();
    
    // Constructor sets ModuleType
    public ProcessModule()
    {
        ModuleType = ModuleType.ProcessModule;
    }
}

ProcessModuleDetail (Workflow Step):

**Note:** ProcessModuleDetail is defined within the same file as ProcessModule (ProcessModule.cs), not as a separate entity file.

public class ProcessModuleDetail
{
    public Guid Id { get; set; }
    public Guid ProcessModuleId { get; set; }
    public int Sequence { get; set; }           // Step order (1, 2, 3...)
    public string? LabelName { get; set; }      // Label for GOTO
    public ActionType? ActionType { get; set; } // What action to perform
    public Guid? ModuleId { get; set; }         // Module to execute
    public ModuleType? ActionModuleType { get; set; }
    public string? PassLabel { get; set; }      // Where to go on success
    public string? FailLabel { get; set; }      // Where to go on failure
    public bool CommentedFlag { get; set; }     // Disabled step
    public string? Comment { get; set; }
    
    public ProcessModule ProcessModule { get; set; }  // Navigation property
}

**DatabaseActionModule**

Executes SQL with field substitution.

public class DatabaseActionModule : Module
{
    public string SqlStatement { get; set; }  // SQL with @FieldName placeholders
    
    public DatabaseActionModule()
    {
        ModuleType = ModuleType.DatabaseAction;
    }
}

SQL Format:

STATEMENT(
    CONNECT WMS;
    SELECT * FROM warehouse_putaway(
        @ItemCode,
        @Quantity,
        @Location
    )
) RETURNS(@PutawayId, @Status)

**DialogActionModule**

Pauses execution for user input.

public class DialogActionModule : Module
{
    public Guid FieldModuleId { get; set; }  // Field to populate
    
    public DialogActionModule()
    {
        ModuleType = ModuleType.DialogAction;
    }
}

**FieldModule**

Defines typed variables.

public class FieldModule : Module
{
    public FieldType FieldType { get; set; }    // String, Number, Boolean, DateTime
    public string? DefaultValue { get; set; }   // Optional default value
    
    public FieldModule()
    {
        ModuleType = ModuleType.FieldModule;
    }
}

**CompareActionModule**

Performs conditional comparisons.

public class CompareActionModule : Module
{
    public CompareOperator OperatorId { get; set; }  // Equals, NotEquals, GreaterThan, etc.
    
    // Input 1
    public bool Input1IsConstant { get; set; }
    public Guid? Input1FieldId { get; set; }
    public string Input1Value { get; set; } = string.Empty;
    
    // Input 2
    public bool Input2IsConstant { get; set; }
    public Guid? Input2FieldId { get; set; }
    public string Input2Value { get; set; } = string.Empty;
    
    public CompareActionModule()
    {
        ModuleType = ModuleType.CompareAction;
    }
}

**CalculateActionModule**

Multi-step calculations.

public class CalculateActionModule : Module
{
    public List<CalculateModuleDetail> Details { get; set; } = new();
    
    public CalculateActionModule()
    {
        ModuleType = ModuleType.CalculateAction;
    }
}

CalculateModuleDetail (Calculation Step):

**Note:** CalculateModuleDetail is defined within the same file as CalculateActionModule (CalculateActionModule.cs), not as a separate entity file.

public class CalculateModuleDetail
{
    public Guid Id { get; set; }
    public Guid CalculateActionId { get; set; }
    public int Sequence { get; set; }
    
    public CalculateOperator OperatorId { get; set; }  // Assign, Add, Multiply, etc.
    
    // Input 1
    public bool Input1IsConstant { get; set; }
    public Guid? Input1FieldId { get; set; }
    public string Input1Value { get; set; } = string.Empty;
    
    // Input 2
    public bool Input2IsConstant { get; set; }
    public Guid? Input2FieldId { get; set; }
    public string Input2Value { get; set; } = string.Empty;
    
    // Output
    public Guid ResultFieldId { get; set; }  // Store result here
    
    public CalculateActionModule CalculateActionModule { get; set; }
}

### 4.3 Enums

**ActionType**

public enum ActionType
{
    Call = 1,            // Call subprocess
    ReturnPass = 2,      // Exit with success
    ReturnFail = 3,      // Exit with failure
    DatabaseExecute = 4, // Execute SQL
    Dialog = 5,          // Pause for input
    Compare = 6,         // Conditional comparison
    Calcualte = 7        // Multi-step calculation (note: typo in actual code)
}

**ModuleType**

public enum ModuleType
{
    ProcessModule = 1,
    DialogAction = 2,
    DatabaseAction = 3,
    FieldModule = 4,
    CompareAction = 5,
    CalculateAction = 6
}

**FieldType**

public enum FieldType
{
    String = 0,
    Number = 1,
    Boolean = 2,
    DateTime = 3
}

**CompareOperator**

public enum CompareOperator
{
    Equals = 1,
    NotEquals = 2,
    GreaterThan = 3,
    LessThan = 4,
    GreaterThanOrEqual = 5,
    LessThanOrEqual = 6,
    Contains = 7,
    StartsWith = 8,
    EndsWith = 9
}

**CalculateOperator**

public enum CalculateOperator
{
    Assign = 1,       // @Result = @Input1
    Concatenate = 2,  // @Result = @Input1 + @Input2 (string)
    Add = 3,          // @Result = @Input1 + @Input2 (numeric)
    Subtract = 4,     // @Result = @Input1 - @Input2
    Multiply = 5,     // @Result = @Input1 * @Input2
    Divide = 6,       // @Result = @Input1 / @Input2
    Modulus = 7,      // @Result = @Input1 % @Input2
    Clear = 8         // Remove field value
}

**ExecutionResult**

**Note:** ExecutionResult is defined in WorkflowEngine.Infrastructure.ProcessEngine.Execution.ActionResult.cs, not in the Domain layer.

public enum ExecutionResult
{
    Success,
    Fail
}

---

## 5. INFRASTRUCTURE LAYER

### 5.1 ExecutionEngine
Location: ExecutionEngine.cs
The main engine class responsible for initializing and managing the module cache.

public class ExecutionEngine
{
    private readonly Dictionary<string, string> _connectionStrings;
    private readonly IServiceProvider _serviceProvider;
    private ModuleCache _moduleCache;

    public ExecutionEngine(
        Dictionary<string, string> connectionStrings, 
        IServiceProvider serviceProvider)
    {
        _connectionStrings = connectionStrings;
        _serviceProvider = serviceProvider;
        _moduleCache = new ModuleCache();
    }

    public Dictionary<string, string> ConnectionStrings => _connectionStrings;
    public ModuleCache Cache => _moduleCache;

    public async Task LoadApplicationsToEngineAsync(
        CancellationToken cancellationToken = default)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var applicationsService = scope.ServiceProvider
                .GetRequiredService<ApplicationsService>();
            _moduleCache.ClearAll();
            await applicationsService.LoadApplicationsIntoCache(
                _moduleCache, cancellationToken);
        }
    }
}

Responsibilities:
•	✅ Initialize module cache
•	✅ Load applications from database into memory
•	✅ Provide access to connection strings
•	✅ Singleton lifetime (shared across all sessions)

---

### 5.2 ModuleCache
Location: ModuleCache.cs
Thread-safe in-memory cache for all modules.
Key Methods:
public class ModuleCache
{
    // Load modules for an application
    public void LoadApplicationModules(Guid applicationId, List<Module> modules)
    
    // Get module by ID
    public Module? GetModule(Guid applicationId, Guid moduleId)
    
    // Get module by name (for @FieldName lookup)
    public Module? GetModuleByName(Guid applicationId, string name)
    
    // Clear all cached data
    public void ClearAll()
    
    // Get all cached application IDs
    public IEnumerable<Guid> GetCachedApplicationIds()
}

Structure:

private ConcurrentDictionary<Guid, Dictionary<Guid, Module>> _applicationModules;
//      ↑ ApplicationId         ↑ ModuleId   ↑ Actual module (polymorphic)

private ConcurrentDictionary<Guid, Dictionary<string, Guid>> _applicationModulesByName;
//      ↑ ApplicationId         ↑ ModuleName ↑ ModuleId

Why Thread-Safe?
•	Multiple requests can read cache simultaneously
•	Background worker may reload cache
•	Uses ConcurrentDictionary for thread safety
---

### 5.3 ExecutionSession
Location: ExecutionSession.cs
Represents a single user's workflow execution session.
Key Properties:

public class ExecutionSession
{
    // Identity
    public Guid SessionId { get; }
    public string UserId { get; set; }
    public Guid ApplicationId { get; }
    
    // Timestamps
    public DateTime StartTime { get; }
    public DateTime LastActive { get; set; }
    
    // State
    private Dictionary<Guid, object?> _fieldValues;      // Session variables
    private Stack<ExecutionFrame> _callStack;            // Process call stack
    public string CurrentDatabase { get; set; }
    
    // Pause/Resume
    public bool IsPaused { get; set; }
    public Guid? PausedAtProcessModuleId { get; set; }
    public int? PausedAtStep { get; set; }
    public string PausedScreenJson { get; set; }
    
    // Infrastructure
    public ModuleCache ModuleCache { get; }
    public Dictionary<string, string> ConnectionStrings { get; }
}

Key Methods:

Session Lifecycle
public async Task<ActionResult> Start()        // Start workflow
public async Task<ActionResult> Restart()      // Clear state and restart
public void Pause(...)                         // Pause for user input
public void Resume()                           // Resume from pause
public bool CanResume()                        // Check if resumable



Field Management
public void SetFieldValue(Guid fieldId, object? value)
public object? GetFieldValue(Guid fieldId)     // 3-level fallback
public bool RemoveFieldValue(Guid fieldId)
public void ClearFields()

// Typed accessors
public FieldType? GetFieldType(Guid fieldId)
public FieldModule? GetFieldModule(Guid fieldId)
public T? GetFieldValueAs<T>(Guid fieldId)
public bool TrySetFieldValue(Guid fieldId, object value)

// Checks
public bool HasField(Guid fieldId)             // Explicitly set?
public bool HasFieldValue(Guid fieldId)        // Set or has default?



Call Stack Management
public int CallDepth { get; }                  // Current nesting level
public ExecutionFrame? CurrentFrame { get; }   // Top of stack
public void PushFrame(ExecutionFrame frame)
public ExecutionFrame? PopFrame()

Database Management
public string? GetDatabaseCreds(string dbName)

---

### 5.4 Field Value Resolution (3-Level Fallback)
When you call GetFieldValue(fieldId), the system checks in this order:

1. Explicit Value in Session
   ├─ Has user/workflow set this field?
   ├─ YES → Return it (even if null)
   └─ NO ↓

2. Module's Default Value
   ├─ Does FieldModule have DefaultValue?
   ├─ YES → Parse and return (string → typed value)
   └─ NO ↓

3. Type-Specific Default
   └─ Return safe default based on FieldType:
      • String → ""
      • Number → 0m
      • Boolean → false
      • DateTime → DateTime(1900, 1, 1)

Example:

// Scenario 1: User set value
session.SetFieldValue(quantityId, 50);
var qty = session.GetFieldValue(quantityId);
// Returns: 50 (explicit)

// Scenario 2: Module has default, user hasn't set
// FieldModule "Quantity" has DefaultValue = "10"
var qty = session.GetFieldValue(quantityId);
// Returns: 10 (module default, parsed to decimal)

// Scenario 3: No default, no value set
// FieldModule "TaxRate" has no DefaultValue
var taxRate = session.GetFieldValue(taxRateId);
// Returns: 0m (type default for Number)

---

### 5.5 Executors

All executors implement the same interface:
public interface IActionExecutor
{
    Task<ActionResult> ExecuteAsync(
        ExecutionSession session,
        Guid applicationId,
        Guid moduleId);
}

**ProcessModuleExecutor**

Location: ProcessModuleExecutor.cs
Executes workflow steps with pause/resume support.
Key Features:
•	✅ Call stack management (20 level max)
•	✅ Resume from pause
•	✅ Label-based navigation (PassLabel/FailLabel)
•	✅ Commented step skipping
•	✅ ReturnPass/ReturnFail handling
•	✅ Frame cleanup on exception

Execution Flow:
ExecuteAsync()
  ├─ Check call depth (< 20)
  ├─ Get ProcessModule from cache
  ├─ Check if resuming from pause
  │   ├─ YES → Resume from next step, don't push frame
  │   └─ NO → Push new frame
  ├─ ExecuteStepsFromSequenceAsync()
  │   └─ Loop through steps:
  │       ├─ Update CurrentSequence
  │       ├─ Skip if CommentedFlag
  │       ├─ Handle ReturnPass/ReturnFail → Pop frame
  │       ├─ ExecuteStepAsync()
  │       │   └─ Get executor → Execute
  │       ├─ Check if paused → Return (keep frame)
  │       ├─ Resolve next label (PassLabel/FailLabel)
  │       └─ If sequence = -1 → Pop frame, end
  └─ Pop frame on normal completion

  ---

**DatabaseActionExecutor**

Location: DatabaseActionExecutor.cs
Executes SQL with field substitution.
SQL Format:
STATEMENT(
    [CONNECT DatabaseName;]
    SQL with @FieldNames
) [RETURNS(@OutputField1, @OutputField2)]

Execution Flow:
ExecuteAsync()
  ├─ Get DatabaseActionModule
  ├─ Extract SQL from STATEMENT(...)
  ├─ Parse CONNECT statement → Get database name
  ├─ Parse RETURNS clause → Get output field names
  ├─ Substitute @FieldName → Actual values (FieldParser)
  ├─ Get connection string
  ├─ Execute SQL → Get results
  └─ Store results in fields (ReturnParser)

Example:
-- Module SQL:
STATEMENT(
    CONNECT WMS;
    SELECT quantity, status FROM inventory WHERE item = @ItemCode
) RETURNS(@AvailableQty, @ItemStatus)

-- Session has: @ItemCode = "ITEM-001"

-- After substitution:
SELECT quantity, status FROM inventory WHERE item = 'ITEM-001'

-- SQL returns: [50, "ACTIVE"]

-- Stored in session:
@AvailableQty = 50
@ItemStatus = "ACTIVE"

---

**DialogExecutor**

Location: DialogExecutor.cs
Pauses execution and generates JSON for UI.
Execution Flow:
ExecuteAsync()
  ├─ Get DialogActionModule
  ├─ Get FieldModule (target field)
  ├─ GenerateDialogJson()
  │   └─ Build JSON with:
  │       • Dialog metadata (id, name, description)
  │       • Field metadata (id, name, type)
  │       • Current value
  │       • Prompt text
  │       • Session context (sessionId, userId)
  ├─ Pause session with JSON
  └─ Return success (session now paused)

Generated JSON Example:
{
  "DialogId": "...",
  "DialogName": "PromptOrderNumber",
  "DialogDescription": "Enter order number",
  "FieldId": "...",
  "FieldName": "OrderNumber",
  "FieldType": "String",
  "FieldDescription": "Order number to process",
  "DefaultValue": null,
  "CurrentValue": "",
  "Prompt": "Please enter OrderNumber:",
  "IsRequired": true,
  "SessionId": "...",
  "UserId": "user123"
}

---

**CompareExecutor**

Location: CompareExecutor.cs
Performs conditional comparisons.
Execution Flow:

ExecuteAsync()
  ├─ Get CompareActionModule
  ├─ Get value1 (constant or field)
  ├─ Get value2 (constant or field)
  ├─ PerformComparison()
  │   └─ Switch on operator:
  │       • Equals/NotEquals → String comparison
  │       • GreaterThan/LessThan → Numeric comparison
  │       • Contains/StartsWith/EndsWith → String matching
  └─ Return Pass or Fail based on result

Example Usage:
Step: CHECK_QUANTITY
  Compare: @Quantity >= 10
  PassLabel: SUFFICIENT
  FailLabel: ERROR_INSUFFICIENT

// If @Quantity = 50 → Goes to SUFFICIENT
// If @Quantity = 5 → Goes to ERROR_INSUFFICIENT


---

**CalculateExecutor**

Location: CalculateExecutor.cs
Multi-step calculations.
Execution Flow:

ExecuteAsync()
  ├─ Get CalculateActionModule
  ├─ Loop through Details (ordered by Sequence):
  │   └─ ExecuteCalculation()
  │       ├─ Get input1 (constant or field)
  │       ├─ Get input2 (constant or field)
  │       ├─ Switch on operator:
  │       │   • Assign → Copy value
  │       │   • Concatenate → String join
  │       │   • Add/Subtract/Multiply/Divide → Math
  │       │   • Modulus → Remainder
  │       │   • Clear → Remove field
  │       └─ Store result in ResultFieldId
  └─ Return Pass

Example:
Calculate: "Order Total"
  Step 1: @Subtotal = @UnitPrice * @Quantity
  Step 2: @Tax = @Subtotal * 0.08
  Step 3: @Total = @Subtotal + @Tax

// Given: @UnitPrice = 10, @Quantity = 5
// Result: @Subtotal = 50, @Tax = 4, @Total = 54

---

### 5.6 ActionExecutorRegistry
Location: ActionExecutorRegistry.cs
Maps ActionType to executor instances.
public static class ActionExecutorRegistry
{
    private static readonly Dictionary<ActionType, IActionExecutor> _executors = new()
    {
        { ActionType.Call, ProcessModuleExecutor.Instance },
        { ActionType.DatabaseExecute, DatabaseActionExecutor.Instance },
        { ActionType.Dialog, DialogExecutor.Instance },
        { ActionType.Compare, CompareExecutor.Instance },
        { ActionType.Calcualte, CalculateExecutor.Instance }  // Note: typo matches ActionType enum
    };

    public static IActionExecutor GetExecutor(ActionType actionType)
    {
        if (_executors.TryGetValue(actionType, out var executor))
            return executor;

        throw new NotSupportedException($"No executor for {actionType}");
    }
}


Why Registry Pattern?
•	✅ No switch statements in execution code
•	✅ Easy to add new executors
•	✅ Single point of executor management
•	✅ Type-safe mapping
---

### 5.7 Parsers

**FieldParser**

Location: FieldParser.cs
Replaces @FieldName with actual values in SQL.

public static class FieldParser
{
    // For SQL (formats values appropriately)
    public static string SubstituteFieldValues(string sql, ExecutionSession session)
    {
        // Regex: @([A-Za-z0-9_]+)
        // Finds: @OrderNumber, @Quantity, etc.
        // Replaces with formatted values:
        //   • String → 'escaped value'
        //   • Number → 123.45
        //   • Boolean → 1 or 0
        //   • DateTime → '2026-01-17 15:30:00'
    }
    
    // For text (plain substitution, no formatting)
    public static string SubstituteFieldReferencesInText(
        string text, ExecutionSession session)
    {
        // For dialog messages, error messages, etc.
        // No SQL escaping/formatting
    }
}

**ReturnParser**

Location: ReturnParser.cs
Parses RETURNS(...) clause and stores results.

public static class ReturnParser
{
    // Extract field names from RETURNS(@Field1, @Field2)
    public static List<string> ParseReturnFields(string sql)
    
    // Remove RETURNS clause from SQL (for old format compatibility)
    public static string RemoveReturnsClause(string sql)
    
    // Store query results in session fields
    public static void StoreResults(
        List<string> returnFieldNames,
        List<object?> resultValues,
        ExecutionSession session)
}

---

## 6. SESSION MANAGEMENT

### 6.1 Session Storage Architecture
One Session Per User Constraint:
•	Each user can have only ONE active workflow session
•	Starting a new workflow returns the existing session if one exists
•	Sessions are stored in memory during execution
•	Sessions persist across API calls (for pause/resume)

┌─────────────────────────────────────────────────────┐
│              ISessionStore (Interface)               │
└─────────────────────────────────────────────────────┘
                          ↓ implements
┌─────────────────────────────────────────────────────┐
│         InMemorySessionStore (Development)           │
│  - ConcurrentDictionary storage                      │
│  - One session per user enforcement                  │
│  - Lost on app restart (in-memory only)              │
└─────────────────────────────────────────────────────┘


---

### 6.2 ISessionStore Interface
Location: ISessionStore.cs

public interface ISessionStore
{
    // Save or update a session
    Task SaveSessionAsync(
        ExecutionSession session, 
        CancellationToken cancellationToken = default);

    // Get session by session ID
    Task<ExecutionSession?> GetSessionAsync(
        Guid sessionId, 
        CancellationToken cancellationToken = default);

    // Get user's active session (ONE per user)
    Task<ExecutionSession?> GetUserSessionAsync(
        string userId, 
        CancellationToken cancellationToken = default);

    // Remove session by session ID
    Task RemoveSessionAsync(
        Guid sessionId, 
        CancellationToken cancellationToken = default);

    // Remove user's session by user ID
    Task RemoveUserSessionAsync(
        string userId, 
        CancellationToken cancellationToken = default);

    // Clean up expired/abandoned sessions
    Task CleanupExpiredSessionsAsync(
        TimeSpan sessionInActiveMaxAge, 
        CancellationToken cancellationToken = default);
}


---

### 6.3 InMemorySessionStore
Location: InMemorySessionStore.cs
Thread-safe in-memory session storage (for development/testing).
Internal Structure:

public class InMemorySessionStore : ISessionStore
{
    // Sessions by session ID
    private readonly ConcurrentDictionary<Guid, ExecutionSession> _sessionsBySessionId = new();
    
    // User ID → Session ID mapping (enforces one session per user)
    private readonly ConcurrentDictionary<string, Guid> _sessionIdByUserId = new();
    
    public int Count => _sessionsBySessionId.Count;
}

Key Methods:
SaveSessionAsync

public Task SaveSessionAsync(ExecutionSession session, ...)
{
    // If user already has a session, remove old one first
    if (_sessionIdByUserId.TryGetValue(session.UserId, out var existingSessionId))
    {
        _sessionsBySessionId.TryRemove(existingSessionId, out _);
    }

    // Save new session
    _sessionsBySessionId[session.SessionId] = session;
    _sessionIdByUserId[session.UserId] = session.SessionId;

    return Task.CompletedTask;
}

GetUserSessionAsync (One Session Per User)
public Task<ExecutionSession?> GetUserSessionAsync(string userId, ...)
{
    if (_sessionIdByUserId.TryGetValue(userId, out var sessionId))
    {
        _sessionsBySessionId.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }
    return Task.FromResult<ExecutionSession?>(null);
}

CleanupExpiredSessionsAsync
public Task CleanupExpiredSessionsAsync(TimeSpan sessionInActiveMaxAge, ...)
{
    var expiredTime = DateTime.UtcNow - sessionInActiveMaxAge;
    var expiredSessions = _sessionsBySessionId.Values
        .Where(s => s.StartTime < expiredTime)
        .ToList();

    foreach (var session in expiredSessions)
    {
        _sessionsBySessionId.TryRemove(session.SessionId, out _);
        _sessionIdByUserId.TryRemove(session.UserId, out _);
    }

    return Task.CompletedTask;
}

⚠️ Limitations:
•	Sessions lost on application restart
•	Not suitable for distributed deployments
•	For production, implement Redis or database-backed storage
---

### 6.4 SessionManager
Location: SessionManager.cs
High-level session management API.
Key Methods:
StartWorkflowAsync

public async Task<(ExecutionSession Session, ActionResult Result, bool IsExisting)> 
    StartWorkflowAsync(
        Guid applicationId,
        Guid processModuleId,
        string userId,
        CancellationToken cancellationToken = default)
{
    // Check if user already has an active session
    var existingSession = await _sessionStore.GetUserSessionAsync(userId);
    
    if (existingSession != null)
    {
        // Return existing session (don't create new one)
        return (
            existingSession, 
            ActionResult.Pass("Connected to existing session. Status: " + 
                (existingSession.IsPaused ? "Paused" : "Active")),
            IsExisting: true
        );
    }

    // Create new session
    var session = new ExecutionSession(
        applicationId,
        processModuleId,
        userId,
        _executionEngine.Cache,
        _executionEngine.ConnectionStrings
    );

    var result = await session.Start();

    // Save if paused (waiting for user input)
    if (session.IsPaused)
    {
        await _sessionStore.SaveSessionAsync(session, cancellationToken);
    }

    return (session, result, IsExisting: false);
}

ResumeWorkflowAsync
public async Task<(ExecutionSession? Session, ActionResult Result)> 
    ResumeWorkflowAsync(
        Guid sessionId,
        Guid fieldId,
        object value,
        CancellationToken cancellationToken = default)
{
    var session = await _sessionStore.GetSessionAsync(sessionId);

    if (session == null)
    {
        return (null, ActionResult.Fail("Session not found"));
    }

    if (!session.CanResume())
    {
        return (session, ActionResult.Fail("Session not in resumable state"));
    }

    // Store user input
    session.SetFieldValue(fieldId, value);

    // Resume execution
    var result = await session.Start();

    // Update or remove session
    if (session.IsPaused)
    {
        // Still paused (another dialog)
        await _sessionStore.SaveSessionAsync(session, cancellationToken);
    }
    else
    {
        // Completed - remove from storage
        await _sessionStore.RemoveSessionAsync(sessionId, cancellationToken);
    }

    return (session, result);
}


Other Methods
// Get session by ID
public Task<ExecutionSession?> GetSessionAsync(Guid sessionId, ...)

// Get user's session
public Task<ExecutionSession?> GetUserSessionAsync(string userId, ...)

// Cancel session by session ID
public Task<bool> CancelSessionAsync(Guid sessionId, ...)

// Cancel user's session by user ID
public Task<bool> CancelUserSessionAsync(string userId, ...)

// Cleanup expired sessions (called by background service)
public Task CleanupExpiredSessionsAsync(TimeSpan maxAge, ...)

---

### 6.5 SessionCleanupService
Location: SessionCleanupService.cs
Background service that periodically removes expired sessions.
public class SessionCleanupService : BackgroundService
{
    private readonly SessionManager _sessionManager;
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _sessionInActiveMaxAge = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                _logger.LogDebug("Running session cleanup");
                await _sessionManager.CleanupExpiredSessionsAsync(
                    _sessionInActiveMaxAge, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;  // Expected during shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }

        _logger.LogInformation("Session cleanup service stopped");
    }
}

Configuration:
•	Runs every 5 minutes
•	Removes sessions older than 1 hour
•	Configurable via constants (can be moved to appsettings.json)
---
7. APPLICATION LAYER

The Application layer provides a service-oriented interface between the API and Infrastructure layers. Currently, the services are registered but not yet implemented - they serve as placeholders for future business logic orchestration.

7.1 Service Registration
Location: ServiceCollectionExtentions.cs

```csharp
public static class ServiceCollectionExtentions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProcessEngineService, ProcessEngineService>();
        services.AddScoped<ISessionService, SessionService>();
    }
}
```

7.2 IProcessEngineService & ProcessEngineService
Location: WorkflowEngine.Application/ProcessEngine/Services/

**Current Status:** Empty interface and implementation (placeholders)

These services are intended for future business logic such as:
- Workflow validation before execution
- Application-level authorization checks
- Workflow orchestration and coordination
- Business rule enforcement
- Logging and auditing

Direct usage of Infrastructure layer components (SessionManager, ExecutionEngine) is currently done from the API layer.

7.3 ISessionService & SessionService
Location: WorkflowEngine.Application/Session/Services/

**Current Status:** Empty interface and implementation (placeholders)

These services are intended for future session management features such as:
- Session listing and filtering
- Session analytics and reporting
- User-specific session policies
- Session state transformation for API responses

---

8. API LAYER

The API layer exposes HTTP endpoints for workflow execution. Currently, the Controllers folder is empty.

8.1 Program.cs
Location: WorkflowEngine.API/Program.cs

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddApplication();           // Register Application layer
        builder.Services.AddInfrastructure(builder.Configuration);  // Register Infrastructure

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
```

8.2 API Controllers (To Be Implemented)

The following controllers should be implemented in the Controllers folder:

**WorkflowController**
- `POST /api/workflow/start` - Start a new workflow session
- `POST /api/workflow/resume` - Resume a paused workflow
- `GET /api/workflow/session/{sessionId}` - Get session status
- `POST /api/workflow/restart` - Restart a session
- `DELETE /api/workflow/session/{sessionId}` - Cancel a session

**ApplicationsController**
- `GET /api/applications` - List all applications
- `GET /api/applications/{id}` - Get application details
- `POST /api/applications/reload` - Reload application cache

**Note:** As mentioned in README.md, the planned API endpoints are:
- Start workflow
- Resume workflow
- Session status
- Restart session
- Cancel session

8.3 Configuration Files

**appsettings.json**
```json
{
  "ConnectionStrings": {
    "RepositoryDB": "Host=localhost;Database=workflow_engine;...",
    "WMS": "Host=localhost;Database=wms;...",
    "ENGINE": "Host=localhost;Database=engine;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Key Configuration Points:**
- `RepositoryDB`: PostgreSQL connection for workflow definitions
- `WMS`: Default database for workflow SQL execution
- `ENGINE`: Additional database connection (optional)

---

9. EXECUTION FLOW

(This section would detail the step-by-step execution flow of a workflow, which appears to be missing from the original documentation)

---

10. SESSION MANAGEMENT

(This section heading exists but may need reorganization since session management is covered in Section 6)

---