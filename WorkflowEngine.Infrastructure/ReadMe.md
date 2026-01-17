Complete Flow Digram

┌─────────────────────────────────────────────────────────────┐
│ 1. ProcessModuleExecutor executes step                      │
│    → Step has ActionType.Dialog                             │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. DialogExecutor.ExecuteAsync()                            │
│    → Gets DialogActionModule                                │
│    → Gets FieldModule (where input will go)                 │
│    → Generates dialog JSON (metadata for UI)                │
│    → Calls session.Pause(processId, sequence, dialogJson)   │
│    → Returns ActionResult.Pass()                            │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. ProcessModuleExecutor checks if paused                   │
│    if (session.IsPaused)                                    │
│        return result;  ← Stops execution, returns to caller │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Your External Code (API/Worker/UI)                       │
│    → Receives result with session.IsPaused = true           │
│    → Parses session.PausedScreenJson                        │
│    → Shows UI/prompt to user                                │
│    → WAITS FOR USER INPUT ⏸️                                │
└─────────────────────────────────────────────────────────────┘
                         ↓
                  ⏱️ Time passes...
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. User provides input (via UI, API, console, etc.)         │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Your External Code stores the input                      │
│    ✅ session.SetFieldValue(fieldId, userInput);            │
│    ↑ THIS IS WHERE THE VALUE IS STORED!                     │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Your External Code resumes execution                     │
│    await session.Start();  // Or ResumeSession()            │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. ProcessModuleExecutor.ExecuteAsync() called again        │
│    → Detects session.IsPaused = true                        │
│    → Resumes from next step (PausedAtStep + 1)              │
│    → session.Resume() clears pause flags                    │
│    → Continues execution with user input now in session!    │
└─────────────────────────────────────────────────────────────┘






ExecutionSession
    ├── Start() → Calls ProcessModuleExecutor
    ├── Provides: ModuleCache, ConnectionStrings, ApplicationId
    └── Manages: Call Stack, Field Values, Pause/Resume
                ↓
    ActionExecutorRegistry
        └── Maps ActionType → IActionExecutor
                ↓
    IActionExecutor (Interface)
        ├── ExecuteAsync(session, applicationId, moduleId)
        └── Implemented by:
            ├── ProcessModuleExecutor.Instance (singleton)
            ├── DialogExecutor.Instance (singleton)
            └── DatabaseActionExecutor.Instance (singleton)



GetFieldValue() Decision Flow:
┌─────────────────────────────────────┐
│ 1. Value in _fieldValues?           │
│    YES → Return it (even if null)   │ ← Explicit always wins
└─────────────────────────────────────┘
           NO ↓
┌─────────────────────────────────────┐
│ 2. FieldModule.DefaultValue exists? │
│    YES → Parse and return           │ ← Module config
└─────────────────────────────────────┘
           NO ↓
┌─────────────────────────────────────┐
│ 3. Return type default:             │
│    • String → ""                    │ ← Safe fallbacks
│    • Number → 0                     │
│    • Boolean → false                │
│    • DateTime → 1900-01-01          │
└─────────────────────────────────────┘




ExecuteStepsFromSequenceAsync Exit Points:
┌──────────────────────────────────────────┐
│ 1. Step not found                        │
│    → Pop frame ✅                        │
└──────────────────────────────────────────┘
┌──────────────────────────────────────────┐
│ 2. ReturnPass                            │
│    → Pop frame ✅                        │
└──────────────────────────────────────────┘
┌──────────────────────────────────────────┐
│ 3. ReturnFail                            │
│    → Pop frame ✅                        │
└──────────────────────────────────────────┘
┌──────────────────────────────────────────┐
│ 4. Paused (Dialog)                       │
│    → DON'T pop ✅ (resume needs frame)   │
└──────────────────────────────────────────┘
┌──────────────────────────────────────────┐
│ 5. Sequence = -1 (label not found)       │
│    → Pop frame ✅                        │
└──────────────────────────────────────────┘
┌──────────────────────────────────────────┐
│ 6. Max iterations                        │
│    → Pop frame ✅                        │
└──────────────────────────────────────────┘

