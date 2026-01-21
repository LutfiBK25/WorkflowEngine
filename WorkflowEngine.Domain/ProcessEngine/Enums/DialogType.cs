

namespace WorkflowEngine.Domain.ProcessEngine.Enums;

public enum DialogType
{
    PROMPT = 1,      // Show input field, collect user input
    CONFIRM = 2,     // Yes/No confirmation, no input field
    NOPROMPT = 3,    // Display only, press any key
    LIST = 4         // Select from list
}
