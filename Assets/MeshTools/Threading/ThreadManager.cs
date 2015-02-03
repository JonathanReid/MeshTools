#if !UNITY_METRO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

//
// The ThreadManager class lets you easy start and stop threaded code.
// Example usage shown below...
//
// StartCoroutine(ThreadManager.Start(ThreadCalculateStuff, OnCalculationThreadComplete, 1.0f)); //you can carry on doing things in the main thread after calling this...
// 
// void ThreadCalculateStuff(object threadID)
// {
// 		//do your threaded calculations here... then call stop...
// 		ThreadManager.Stop((int)threadID);
// }
//
// void OnCalculationThreadComplete()
// {
//      //put stuff here that needs to happen after the thread was completed...	
// }
//

public static class ThreadManager
{
    private static Dictionary<int, Thread> threads = new Dictionary<int, Thread>();
    private static int threadCount = 0;
	
	public static bool debugMode = false;

    public static IEnumerator Start(ParameterizedThreadStart startMethod, Action completeCallback, float delay)
    {
        if(debugMode) Debug.Log("ThreadManager.Start");

        int id = threadCount++;
        threads.Add(id, new Thread(startMethod));

        yield return new WaitForSeconds(delay);

        if(debugMode) Debug.Log("ThreadManager: Starting Thread '" + startMethod.Method.Name + "'");
        if(threads.ContainsKey(id))
        {
            threads[id].Start(id);
        }

        while(IsRunning(id))
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
        if(debugMode) Debug.Log("ThreadManager: Ending Thread '" + startMethod.Method.Name + "' with callback '" + completeCallback.Method.Name + "'");

        completeCallback();
    }

    public static bool IsRunning(int id)
    {
        return threads.ContainsKey(id);
    }

    public static void Stop(int id)
    {
        if(debugMode) Debug.Log("ThreadManager.Stop");
        if(threads.ContainsKey(id))
        {
            threads.Remove(id);
        }
        //No need to manually call thread.Abort(), as that throws an error on some platforms.
        //The thread is complete and will get garbage collected as it's not doing anything and nothing references it any more.
        //See here for more details - http://www.interact-sw.co.uk/iangblog/2004/11/12/cancellation
    }

}

#endif
