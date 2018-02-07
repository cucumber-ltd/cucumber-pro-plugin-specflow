using System;
using System.Collections.Generic;
using System.Text;

namespace Cucumber.Pro.SpecFlowPlugin.Configuration
{
    public class Config
    {
        private readonly Dictionary<string, IValue> _valueByProperty = new Dictionary<string, IValue>();
        private readonly Dictionary<string, Config> _configByProperty = new Dictionary<string, Config>();

        public string GetString(string key)
        {
            return GetIn(Normalize(key), false).GetString();
        }

        public bool GetBoolean(string key)
        {
            return GetIn(Normalize(key), false).GetBoolean();
        }

        public int GetInteger(string key)
        {
            return GetIn(Normalize(key), false).GetInt();
        }

        public bool IsNull(string key)
        {
            return GetIn(Normalize(key), true).IsNull();
        }

        public void SetNull(string key)
        {
            SetIn(Normalize(key), new NullValue());
        }

        public void Set(string key, string value)
        {
            SetIn(Normalize(key), RealValue.FromString(value));
        }

        public void Set(string key, bool value)
        {
            SetIn(Normalize(key), RealValue.FromBoolean(value));
        }

        private void SetValue(string property, IValue value)
        {
            _valueByProperty[property.ToLowerInvariant()] = value;
        }

        public Config GetChild(string property)
        {
            if (!_configByProperty.TryGetValue(property.ToLowerInvariant(), out var config))
            {
                config = new Config();
                _configByProperty[property.ToLowerInvariant()] = config;
            }
            return config;
        }

        private IValue GetValue(string property)
        {
            return _valueByProperty.TryGetValue(property.ToLowerInvariant(), out var value) ? value : null;
        }

        private IValue GetIn(string normalizedKey, bool allowNull)
        {
            var path = ToPath(normalizedKey);
            var config = this;
            for (int i = 0; i < path.Length; i++)
            {
                String property = path[i];
                if (i == path.Length - 1)
                {
                    var value = config.GetValue(property);
                    if (value != null) return value;
                    if (allowNull) return new NullValue();
                    throw new KeyNotFoundException(normalizedKey);
                }
                else
                {
                    config = config.GetChild(property.ToLowerInvariant());
                    if (config == null)
                    {
                        if (allowNull) return new NullValue();
                        throw new KeyNotFoundException(normalizedKey);
                    }
                }
            }
            throw new InvalidOperationException("path cannot be empty");
        }

        private void SetIn(string normalizedKey, IValue value)
        {
            var path = ToPath(normalizedKey);
            var config = this;
            for (int i = 0; i < path.Length; i++)
            {
                var property = path[i];
                if (i == path.Length - 1)
                {
                    config.SetValue(property, value);
                    return;
                }
                else
                {
                    config = config.GetChild(property);
                }
            }
        }

        private string[] ToPath(string key)
        {
            return Normalize(key).Split('.');
        }

        private string Normalize(string key)
        {
            return key.Replace('_', '.').ToLowerInvariant();
        }

        private void AppendTo(StringBuilder stringBuilder, string indent)
        {
            foreach (var item in _valueByProperty)
            {
                stringBuilder.Append(indent);
                stringBuilder.Append(item.Key);
                stringBuilder.Append(":");
                stringBuilder.Append(item.Value.IsNull() ? "" : " " + item.Value.GetString());
                stringBuilder.AppendLine();
            }
            foreach (var configItem in _configByProperty)
            {
                stringBuilder.Append(indent);
                stringBuilder.Append(configItem.Key);
                stringBuilder.Append(":");
                stringBuilder.AppendLine();
                configItem.Value.AppendTo(stringBuilder, indent + "  ");
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            AppendTo(stringBuilder, "");
            return stringBuilder.ToString();
        }
    }
}
