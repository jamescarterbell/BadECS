using Microsoft.VisualStudio.TestTools.UnitTesting;
using BadECS;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            World world = new World();
            world.RegisterComponent(typeof(Tag), typeof(NullList<Tag>));
            Assert.AreEqual(1, world.componentLUT.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            World world = new World();
            world.RegisterComponent(typeof(Tag), typeof(NullList<Tag>));
            
        }
    }
}

public struct TestStruct : IComponent
{
    public string Name;
}

public struct Tag : IComponent { }
