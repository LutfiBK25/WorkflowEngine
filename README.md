# WorkflowEngine

A powerful, flexible workflow execution engine built with .NET 10 that enables visual process automation with database integration and interactive user dialogs.

┌─────────────────────────────────────────────────────┐
│                External World                       │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│          Public API (User Management)                │
│  - User authentication                               │
│  - User authorization                                │
│  - Session management                                │
│  - Business logic                                    │
│  - Rate limiting                                     │
│  - Input validation                                  │
└─────────────────────────────────────────────────────┘
                        ↓ (Internal Network)
┌─────────────────────────────────────────────────────┐
│        Workflow Engine API (Internal)                │
│  - Start workflow                                    │
│  - Resume workflow                                   │
│  - Get session status                                │
│  - Cancel session                                    │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│              PostgreSQL Database                     │
└─────────────────────────────────────────────────────┘


## Features

### Core Capabilities
- **Dynamic Process Execution** - Execute complex workflows with branching logic, loops, and nested subprocess calls
- **Database Integration** - Execute SQL queries with automatic field substitution using human-readable `@FieldName` syntax
- **Interactive Dialogs** - Pause execution to collect user input and resume seamlessly
- **Session Management** - Stateful execution with pause/resume across HTTP requests
- **Type-Safe Field System** - Strongly-typed fields (String, Number, Boolean, DateTime) with default value fallbacks
- **Multi-Database Support** - Connect to different databases within a single workflow using `CONNECT` statements
- **Module Cache** - High-performance in-memory module caching with thread-safe operations
- **Clean Architecture** - Separation of domain, infrastructure, and API layers

### Workflow Features
- **Process Modules** - Define multi-step workflows with labels and conditional branching
- **Database Actions** - Execute SQL with parameter substitution and return value mapping
- **Dialog Actions** - Prompt users for input with field metadata for UI rendering
- **Field Modules** - Define typed variables with default values
- **Subprocess Calls** - Nested process execution with call stack management (max depth: 20)
- **Error Handling** - PassLabel/FailLabel routing for success/failure paths
- **Step Comments** - Disable steps without deleting them


## Configuration

### Application Modules
- **Process Modules** - Workflow definitions with steps
- **Database Action Modules** - SQL templates with field references
- **Dialog Action Modules** - User input prompts linked to fields
- **Field Modules** - Typed variables (String, Number, Boolean, DateTime)
- **Compare Action Modules** - Compare Two values and return Pass or Fail

### Module Types
- `ProcessModule` (ActionType.Call) - Execute subprocess
- `DatabaseActionModule` (ActionType.DatabaseExecute) - Run SQL
- `DialogActionModule` (ActionType.Dialog) - Show user prompt
- `ReturnPass` (ActionType.ReturnPass) - Exit with success
- `ReturnFail` (ActionType.ReturnFail) - Exit with failure
- `CompareActionModule` Compare Two Fields

## Database Schema

### Key Tables
- `t_applications` - Application containers
- `t_process_modules` - Workflow definitions
- `t_process_module_details` - Workflow steps
- `t_database_action_modules` - SQL action templates
- `t_dialog_action_modules` - User input prompts
- `t_field_modules` - Variable definitions
- `t_compare_action_modules` - Compare Action
- `t_calculate_action_modules` - Calculate Action
_ `t_calculate_module_details` - Calculate Steps

### Adding ActionModule
1. Create a class in Domain Inheriting `Module` class
2. Add ModuleType
3. Add to ActionType
4. Add to `ReposityDBContext`
5. In Package Manager set infratructure as Startup Project and choose it in default project
    A. Add-Migration AddNewNameActionModule
    B. Update-Database
6. In Infrastructure add to LoadApplicationsIntoCache


### Adding a New Executor
1. Create class implementing `IActionExecutor`
2. Add singleton instance: `public static readonly YourExecutor Instance = new();`
3. Register in `ActionExecutorRegistry`
4. Implement `ExecuteAsync(ExecutionSession, Guid, Guid)`