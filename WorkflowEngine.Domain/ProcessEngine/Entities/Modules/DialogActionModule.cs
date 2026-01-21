using WorkflowEngine.Domain.ProcessEngine.Enums;

namespace WorkflowEngine.Domain.ProcessEngine.Entities.Modules;

public class DialogActionModule : Module
{
    // Dialog behavior
    public DialogType DialogType { get; set; } = DialogType.PROMPT;

    // Result field (where user input goes)
    // Required for PROMPT/LIST, not used for CONFIRM/NOPROMPT
    public Guid? ResultFieldId { get; set; }

    // Content fields (all optional - store IDs of FieldModules)
    public Guid? MessageFieldId { get; set; }      // Main message/prompt text
    public Guid? Help1FieldId { get; set; }        // Help line 1
    public Guid? Help2FieldId { get; set; }        // Help line 2
    public Guid? Help3FieldId { get; set; }        // Help line 3 (optional - add if needed)
    public Guid? OptionsFieldId { get; set; }      // Function key options (F2:Next F3:Cancel)

    // List support (for DialogType.LIST)
    public Guid? ListModuleId { get; set; }        // Points to list data

    // Input masking (for passwords/sensitive data)
    public bool MaskInput { get; set; } = false;
    public string MaskCharacter { get; set; } = "*";

    public DialogActionModule()
    {
        ModuleType = ModuleType.DialogAction;
    }
}