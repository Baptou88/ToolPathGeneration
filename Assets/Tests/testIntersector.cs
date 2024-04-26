using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class testIntersector
{
    // A Test behaves as an ordinary method
    [Test]
    public void testIntersectorSimplePasses()
    {
        // Use the Assert class to test conditions
        Assert.True(true);
    }

    public void testIntersectorPerpendicularSegments()
    {
        
        //Line l1;
    }
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator testIntersectorWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
