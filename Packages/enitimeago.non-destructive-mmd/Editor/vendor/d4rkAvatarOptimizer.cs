using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using BlendableLayer = VRC.SDKBase.VRC_AnimatorLayerControl.BlendableLayer;
using UnityEditor;

namespace enitimeago.NonDestructiveMMD.vendor.d4rkAvatarOptimizer
{
    internal static class UselessFXLayerFinder
    {
        public static HashSet<int> FindInAvatar(VRCAvatarDescriptor avatarDescriptor)
        {
            return new d4rkAvatarOptimizer(avatarDescriptor.gameObject).FindUselessFXLayers();
        }
    }
    internal class FakeMonoBehaviour
    {
        private GameObject _gameObject;
        public Transform transform => _gameObject.transform;
        public FakeMonoBehaviour(GameObject gameObject)
        {
            _gameObject = gameObject;
        }
        public T GetComponent<T>()
        {
            return _gameObject.GetComponent<T>();
        }
    }

    // d4rkAvatarOptimizer
    //
    // MIT License
    // 
    // Copyright (c) 2021 d4rkpl4y3r
    // 
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    // 
    // The above copyright notice and this permission notice shall be included in all
    // copies or substantial portions of the Software.
    // 
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    // SOFTWARE.

    internal static class AnimatorControllerExtensions
    {
        public static IEnumerable<AnimatorState> EnumerateAllStates(this AnimatorStateMachine stateMachine)
        {
            var queue = new Queue<AnimatorStateMachine>();
            queue.Enqueue(stateMachine);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var subStateMachine in current.stateMachines)
                {
                    queue.Enqueue(subStateMachine.stateMachine);
                }
                foreach (var state in current.states.Select(s => s.state))
                {
                    yield return state;
                }
            }
        }
        public static IEnumerable<StateMachineBehaviour> EnumerateAllBehaviours(this AnimatorStateMachine stateMachine)
        {
            var queue = new Queue<AnimatorStateMachine>();
            queue.Enqueue(stateMachine);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var subStateMachine in current.stateMachines)
                {
                    queue.Enqueue(subStateMachine.stateMachine);
                }
                foreach (var behaviour in current.behaviours)
                {
                    yield return behaviour;
                }
                foreach (var state in current.states.Select(s => s.state))
                {
                    foreach (var behaviour in state.behaviours)
                    {
                        yield return behaviour;
                    }
                }
            }
        }

        public static IEnumerable<AnimationClip> EnumerateAllClips(this Motion motion)
        {
            if (motion is AnimationClip clip)
            {
                yield return clip;
            }
            else if (motion is BlendTree tree)
            {
                var childNodes = tree.children;
                for (int i = 0; i < childNodes.Length; i++)
                {
                    if (childNodes[i].motion == null)
                    {
                        continue;
                    }
                    foreach (var childClip in childNodes[i].motion.EnumerateAllClips())
                    {
                        yield return childClip;
                    }
                }
            }
        }
    }
    internal class d4rkAvatarOptimizer : FakeMonoBehaviour
    {
        public d4rkAvatarOptimizer(GameObject gameObject) : base(gameObject) { }

        private bool OptimizeFXLayer = true;
        public AnimatorController GetFXLayer()
        {
            var avDescriptor = GetComponent<VRCAvatarDescriptor>();
            var rootAnimator = GetComponent<Animator>();
            var baseLayerCount = rootAnimator != null ? (rootAnimator.avatar.isHuman ? 5 : 3) : 3;
            if (avDescriptor == null || avDescriptor.baseAnimationLayers.Length != baseLayerCount)
                return null;
            return avDescriptor.baseAnimationLayers[baseLayerCount - 1].animatorController as AnimatorController;
        }
        private AnimatorControllerLayer[] cache_GetFXLayerLayers = null;
        public AnimatorControllerLayer[] GetFXLayerLayers()
        {
            if (cache_GetFXLayerLayers != null)
                return cache_GetFXLayerLayers;
            var fxLayer = GetFXLayer();
            return cache_GetFXLayerLayers = fxLayer != null ? fxLayer.layers : new AnimatorControllerLayer[0];
        }
        public Transform GetTransformFromPath(string path)
        {
            if (path == "")
                return transform;
            string[] pathParts = path.Split('/');
            Transform t = transform;
            for (int i = 0; i < pathParts.Length; i++)
            {
                t = t.Find(pathParts[i]);
                if (t == null)
                    return null;
            }
            return t;
        }
        private HashSet<int> cache_FindUselessFXLayers = null;
        public HashSet<int> FindUselessFXLayers()
        {
            if (cache_FindUselessFXLayers != null)
                return cache_FindUselessFXLayers;
            var fxLayer = GetFXLayer();
            if (fxLayer == null || !OptimizeFXLayer)
                return new HashSet<int>();
            //Profiler.StartSection("FindUselessFXLayers()");
            var avDescriptor = GetComponent<VRCAvatarDescriptor>();

            var isAffectedByLayerWeightControl = new HashSet<int>();

            for (int i = 0; i < avDescriptor.baseAnimationLayers.Length; i++)
            {
                var controller = avDescriptor.baseAnimationLayers[i].animatorController as AnimatorController;
                if (controller == null)
                    continue;
                var controllerLayers = controller.layers;
                for (int j = 0; j < controllerLayers.Length; j++)
                {
                    var stateMachine = controllerLayers[j].stateMachine;
                    if (stateMachine == null)
                        continue;
                    foreach (var behaviour in stateMachine.EnumerateAllBehaviours())
                    {
                        if (behaviour is VRCAnimatorLayerControl layerControl && layerControl.playable == BlendableLayer.FX)
                        {
                            isAffectedByLayerWeightControl.Add(layerControl.layer);
                        }
                    }
                }
            }

            var uselessLayers = new HashSet<int>();

            var possibleBindingTypes = new Dictionary<string, HashSet<string>>();
            bool IsPossibleBinding(EditorCurveBinding binding)
            {
                if (!possibleBindingTypes.TryGetValue(binding.path, out var possibleTypeNames))
                {
                    possibleTypeNames = new HashSet<string>();
                    var transform = GetTransformFromPath(binding.path);
                    if (transform != null)
                    {
                        // AnimationUtility.GetAnimatableBindings(transform.gameObject, gameObject)
                        // is too slow, so we just check if the components mentioned in the bindings exist at that path
                        possibleTypeNames.UnionWith(transform.GetComponents<Component>().Select(c => c.GetType().FullName));
                        possibleTypeNames.Add(typeof(GameObject).FullName);
                    }
                    possibleBindingTypes[binding.path] = possibleTypeNames;
                }
                return possibleTypeNames.Contains(binding.type.FullName);
            }

            var fxLayerLayers = GetFXLayerLayers();
            int lastNonUselessLayer = fxLayerLayers.Length;
            for (int i = fxLayerLayers.Length - 1; i >= 0; i--)
            {
                var layer = fxLayerLayers[i];
                bool isNotFirstLayerOrLastNonUselessLayerCanBeFirst = i != 0 ||
                    (lastNonUselessLayer < fxLayerLayers.Length && fxLayerLayers[lastNonUselessLayer].avatarMask == layer.avatarMask
                        && fxLayerLayers[lastNonUselessLayer].defaultWeight == 1 && !isAffectedByLayerWeightControl.Contains(lastNonUselessLayer));
                var stateMachine = layer.stateMachine;
                if (stateMachine == null)
                {
                    if (isNotFirstLayerOrLastNonUselessLayerCanBeFirst)
                    {
                        uselessLayers.Add(i);
                    }
                    continue;
                }
                if (stateMachine.EnumerateAllBehaviours().Any())
                {
                    lastNonUselessLayer = i;
                    continue;
                }
                if (i != 0 && layer.defaultWeight == 0 && !isAffectedByLayerWeightControl.Contains(i))
                {
                    uselessLayers.Add(i);
                    continue;
                }
                if (isNotFirstLayerOrLastNonUselessLayerCanBeFirst && stateMachine.stateMachines.Length == 0 && stateMachine.states.Length == 0)
                {
                    uselessLayers.Add(i);
                    continue;
                }
                var layerBindings = stateMachine.EnumerateAllStates()
                    .SelectMany(s => s.motion == null ? new AnimationClip[0] : s.motion.EnumerateAllClips()).Distinct()
                    .SelectMany(c => AnimationUtility.GetCurveBindings(c).Concat(AnimationUtility.GetObjectReferenceCurveBindings(c)));
                if (layerBindings.All(b => !IsPossibleBinding(b)))
                {
                    uselessLayers.Add(i);
                    continue;
                }
                lastNonUselessLayer = i;
            }
            //Profiler.EndSection();
            return cache_FindUselessFXLayers = uselessLayers;
        }
    }
}
