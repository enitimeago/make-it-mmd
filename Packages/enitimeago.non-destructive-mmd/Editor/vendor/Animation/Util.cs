/*
* MIT License
*
* Copyright (c) 2022 bd_
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using nadena.dev.ndmf;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using Object = UnityEngine.Object;

namespace enitimeago.NonDestructiveMMD.vendor
{
    [InitializeOnLoad]
    internal static class Util
    {
        internal static IEnumerable<AnimatorState> States(AnimatorController ac)
        {
            HashSet<AnimatorStateMachine> visitedStateMachines = new HashSet<AnimatorStateMachine>();
            Queue<AnimatorStateMachine> pending = new Queue<AnimatorStateMachine>();

            foreach (var layer in ac.layers)
            {
                if (layer.stateMachine != null) pending.Enqueue(layer.stateMachine);
            }

            while (pending.Count > 0)
            {
                var next = pending.Dequeue();
                if (visitedStateMachines.Contains(next)) continue;
                visitedStateMachines.Add(next);

                foreach (var child in next.stateMachines)
                {
                    if (child.stateMachine != null) pending.Enqueue(child.stateMachine);
                }

                foreach (var state in next.states)
                {
                    yield return state.state;
                }
            }
        }

        internal static bool IsProxyAnimation(Motion m)
        {
            var path = AssetDatabase.GetAssetPath(m);

            // This is a fairly wide condition in order to deal with:
            // 1. Future additions of proxy animations (so GUIDs are out)
            // 2. Unitypackage based installations of the VRCSDK
            // 3. VCC based installations of the VRCSDK
            // 4. Very old VCC based installations of the VRCSDK where proxy animations were copied into Assets
            return path.Contains("/AV3 Demo Assets/Animation/ProxyAnim/proxy")
                   || path.Contains("/VRCSDK/Examples3/Animation/ProxyAnim/proxy");
        }
    }
    internal class BuildReport
    {
        internal static void LogException(Exception e, string additionalStackTrace = "")
        {
            ErrorReport.ReportException(e, additionalStackTrace);
        }

        internal static T ReportingObject<T>(UnityEngine.Object obj, Func<T> action)
        {
            return ErrorReport.WithContextObject(obj, action);
        }

        internal static void ReportingObject(UnityEngine.Object obj, Action action)
        {
            ErrorReport.WithContextObject(obj, action);
        }

        [Obsolete("Use NDMF's ObjectRegistry instead")]
        public static void RemapPaths(string original, string cloned)
        {
        }
    }
}
