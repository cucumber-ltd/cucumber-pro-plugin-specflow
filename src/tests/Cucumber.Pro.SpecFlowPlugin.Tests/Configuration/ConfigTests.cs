using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Pro.SpecFlowPlugin.Configuration;
using Xunit;

namespace Cucumber.Pro.SpecFlowPlugin.Tests.Configuration
{
    public class ConfigTests
    {
        [Fact]
        public void Gets_and_sets_value()
        {
            Config config = new Config();
            config.Set("name", "progress");
            Assert.Equal("progress", config.GetString("name"));
        }

        [Fact]
        public void Gets_boolean()
        {
            Config config = new Config();
            config.Set("a", true);
            config.Set("b", false);

            Assert.True(config.GetBoolean("a"));
            Assert.False(config.GetBoolean("b"));
        }

        [Fact]
        public void Gets_deep_value()
        {
            Config root = new Config();

            Config one = root.GetChild("one");
            Config two = one.GetChild("two");

            two.Set("hello", "world");
            Assert.Equal("world", root.GetString("one.two.hello"));
        }

        [Fact]
        public void Throws_exception_when_no_value_set()
        {
            Config config = new Config();
            Assert.Throws<KeyNotFoundException>(() => config.GetString("not.set"));
        }

        [Fact]
        public void Unset_value_is_null()
        {
            Config config = new Config();
            Assert.True(config.IsNull("booya.kasha"));
            Assert.True(config.IsNull("booya"));
        }

        [Fact]
        public void Set_value_is_not_null()
        {
            Config config = new Config();
            config.Set("booya.kasha", "wat");
            config.Set("ninky", "nonk");
            Assert.False(config.IsNull("booya.kasha"));
            Assert.False(config.IsNull("ninky"));
        }

        [Fact]
        public void Has_tostring_representation()
        {
            Config config = new Config();
            config.Set("a.b.c.d.e", "1");
            config.Set("a.c.d.e", "3");
            config.Set("a.d.e", "4");

            const string expected = "" +
                "b:\n" +
                "  c:\n" +
                "    d:\n" +
                "      e: 1\n" +
                "c:\n" +
                "  d:\n" +
                "    e: 3\n" +
                "d:\n" +
                "  e: 4\n";
            Assert.Equal(expected, config.ToString());
        }
    }
}
