using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class NullValue : IValue
    {
        public string GetString() => null;
        public bool GetBoolean() => false;
        public int GetInt() => 0;
        public bool IsNull() => true;
    }
}
