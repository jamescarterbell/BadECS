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

        }

        [TestMethod]
        public void TestMethod2()
        {
            
        }
    }
}

public struct TestStruct : IComponent
{
    public string Name;
}

public struct Tag : IComponent { }

public struct TestMethod
{
    
}