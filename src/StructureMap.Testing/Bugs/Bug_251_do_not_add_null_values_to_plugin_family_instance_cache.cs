﻿using System.Linq;
using Xunit;

namespace StructureMap.Testing.Bugs
{
    public class Bug_251_do_not_add_null_instances
    {
        [Fact]
        public void should_not_add_null_instance()
        {
            var container = new Container(x => x.For<ITest>().Use<Test>());

            container.Model.Pipeline.Instances.GetAllInstances().Any(i => i == null).ShouldBeFalse();
            container.TryGetInstance<ITest>("test").ShouldBeNull();
            container.Model.Pipeline.Instances.GetAllInstances().Any(i => i == null).ShouldBeFalse();
        }

        public interface ITest
        {
        }

        public class Test : ITest
        {
        }
    }
}