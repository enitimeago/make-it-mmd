#if NDMMD_VRCSDK3_AVATARS

using enitimeago.NonDestructiveMMD.vendor;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class MapperPlugin : Plugin<MapperPlugin>
    {
        public override string QualifiedName => "enitimeago.non-destructive-mmd";
        public override string DisplayName => "Make It MMD";

        protected override void Configure()
        {
            Sequence seq = InPhase(BuildPhase.Resolving);
            // Clone animator controllers first to allow safe mutation.
            // Modular Avatar does this, but unless this is moved into somewhere common
            // then it's not ideal to rely on it just to clone all animators.
            seq.Run("Clone animators", AnimationUtil.CloneAllControllers);

            seq = InPhase(BuildPhase.Transforming);
            seq.Run(MakeBlendShapesPass.Instance);
            seq.Run(WriteDefaultsPass.Instance);
        }
    }
}

#endif
