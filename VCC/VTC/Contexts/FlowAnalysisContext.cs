using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTC
{
    public class FlowAnalysisContext
    {
        public bool UnreachableReported { get; set; }

        /*
                public bool IsDefinitelyAssigned(VariableInfo variable)
                {
                    return variable.IsAssigned(DefiniteAssignment);
                }

                public bool IsStructFieldDefinitelyAssigned(VariableInfo variable, string name)
                {
                    return variable.IsStructFieldAssigned(DefiniteAssignment, name);
                }

                public void SetVariableAssigned(VariableInfo variable, bool generatedAssignment = false)
                {
                    variable.SetAssigned(DefiniteAssignment, generatedAssignment);
                }

                public void SetStructFieldAssigned(VariableInfo variable, string name)
                {
                    variable.SetStructFieldAssigned(DefiniteAssignment, name);
                }
                */
    }
}
