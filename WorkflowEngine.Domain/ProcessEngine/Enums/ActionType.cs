using System;
using System.Collections.Generic;
using System.Text;

namespace WorkflowEngine.Domain.ProcessEngine.Enums
{
    public enum ActionType
    {
        Call = 1,
        ReturnPass = 2,
        ReturnFail = 3,
        DatabaseExecute = 4,
        Dialog = 5,
    }
}
