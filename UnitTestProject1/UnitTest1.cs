using Microsoft.VisualStudio.TestTools.UnitTesting;
using BadECS;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public static int test = 0;

        [TestMethod]
        public void TestMethod1()
        {
            SystemCollection sysCollection = new SystemCollection();
            sysCollection.RegisterResource<Time>();
            sysCollection.RegisterResource<Counter>();
            TimeSystem ts = new TimeSystem();
            Count c = new Count();
            sysCollection.RegisterSystem(ts);
            sysCollection.RegisterSystem(c);
            sysCollection.RegisterSystem(new PrintTimeSystem(), ts);
            sysCollection.RegisterSystem(new PrintEven(), c);
            sysCollection.RegisterSystem(new PrintOdd(), c);
            sysCollection.StartSystems(15);
        }
    }
}

public struct Time: IComponent
{
    internal bool initialized;
    internal float lastTime;
    public float total;
    public float deltaTime;
}

public class TimeSystem : ISystem
{
    public Type[] compRead => new Type[0];

    public Type[] compWrite => new Type[0];

    public Type[] compAddable => new Type[0];

    public Type[] compExclude => new Type[0];

    public Type[] rescRead => new Type[0];

    public Type[] rescWrite => new Type[] { typeof(Time)};

    public bool entityAccess => false;

    public bool entityDeleteAccess => false;

    public void Update(SystemDataFilter filter)
    {
        Time time = filter.GetWritableResc<Time>();
        if (time.initialized)
        {
            time.deltaTime = DateTime.Now.Millisecond - time.lastTime;
            time.lastTime = DateTime.Now.Millisecond;
            time.total += time.deltaTime;
        }
        else
        {
            time.lastTime = DateTime.Now.Millisecond;
            time.total = 0;
            time.deltaTime = 0;
            time.initialized = true;
        }
        filter.SetWritableResc<Time>(time);
    }
}

public class PrintTimeSystem : ISystem
{
    public Type[] compRead => new Type[0];

    public Type[] compWrite => new Type[0];

    public Type[] compAddable => new Type[0];

    public Type[] compExclude => new Type[0];

    public Type[] rescRead => new Type[] { typeof(Time) };

    public Type[] rescWrite => new Type[0];

    public bool entityAccess => false;

    public bool entityDeleteAccess => false;

    public void Update(SystemDataFilter filter)
    {
        Time time = filter.GetReadableResc<Time>();
        Console.WriteLine("Total Time:\t" + time.total + "\tDeltaTime:\t" + time.deltaTime);
    }
}

public struct Counter: IComponent
{
    public int current;
}

public class Count: ISystem
{

    public Type[] compRead => new Type[0];

    public Type[] compWrite => new Type[0];

    public Type[] compAddable => new Type[0];

    public Type[] compExclude => new Type[0];

    public Type[] rescRead => new Type[0];

    public Type[] rescWrite => new Type[] { typeof(Counter) };

    public bool entityAccess => false;

    public bool entityDeleteAccess => false;

    public void Update(SystemDataFilter filter)
    {
        Counter counter = filter.GetWritableResc<Counter>();
        counter.current++;
        filter.SetWritableResc(counter);
    }
}

public class PrintEven : ISystem
{

    public Type[] compRead => new Type[0];

    public Type[] compWrite => new Type[0];

    public Type[] compAddable => new Type[0];

    public Type[] compExclude => new Type[0];

    public Type[] rescRead => new Type[] { typeof(Counter) };

    public Type[] rescWrite => new Type[0];

    public bool entityAccess => false;

    public bool entityDeleteAccess => false;

    public void Update(SystemDataFilter filter)
    {
        Counter counter = filter.GetReadableResc<Counter>();
        if(counter.current % 2 == 0)
        {
            Console.WriteLine("EVEN!");
        }
    }
}

public class PrintOdd : ISystem
{

    public Type[] compRead => new Type[0];

    public Type[] compWrite => new Type[0];

    public Type[] compAddable => new Type[0];

    public Type[] compExclude => new Type[0];

    public Type[] rescRead => new Type[] { typeof(Counter) };

    public Type[] rescWrite => new Type[0];

    public bool entityAccess => false;

    public bool entityDeleteAccess => false;

    public void Update(SystemDataFilter filter)
    {
        Counter counter = filter.GetReadableResc<Counter>();
        if (counter.current % 2 == 1)
        {
            Console.WriteLine("ODD!");
        }
    }
}