using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public interface IValue
    {
        string GetString();
        bool GetBoolean();
        int GetInt();
        bool IsNull();
    }
}
