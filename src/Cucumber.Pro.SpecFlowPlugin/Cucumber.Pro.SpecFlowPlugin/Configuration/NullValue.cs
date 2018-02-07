using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class NullValue : IValue
    {
        public string GetString()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean()
        {
            throw new NotImplementedException();
        }

        public int GetInt()
        {
            throw new NotImplementedException();
        }

        public bool IsNull()
        {
            return true;
        }
    }
}
