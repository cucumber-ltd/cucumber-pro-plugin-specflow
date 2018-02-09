using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class RealValue : IValue
    {
        private readonly string _value;

        public RealValue(string value)
        {
            _value = value;
        }

        public static IValue FromString(string value)
        {
            return new RealValue(value);
        }

        public static IValue FromInteger(int value)
        {
            return new RealValue(value.ToString());
        }

        public static IValue FromBoolean(bool value)
        {
            return new RealValue(value.ToString());
        }

        private static readonly string[] FalseValues = new[] { "false", "no", "off" };

        public string GetString() => _value;
        public bool GetBoolean() => !FalseValues.Contains(_value.ToLowerInvariant());
        public int GetInt() => int.Parse(_value, CultureInfo.InvariantCulture);
        public bool IsNull() => false;
    }
}
