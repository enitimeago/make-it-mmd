#if NDMMD_VRCSDK3_AVATARS

using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;

namespace enitimeago.NonDestructiveMMD
{
    public class NonDestructiveMmdPlugin : Plugin<NonDestructiveMmdPlugin>
    {
        public override string QualifiedName => "enitimeago.non-destructive-mmd";
        public override string DisplayName => "Make It MMD";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming);
            seq.AfterPlugin("nadena.dev.modular-avatar");
            seq.WithRequiredExtension(typeof(AnimatorServicesContext), _ =>
            {
                seq.Run(RenameFaceForMmdPass.Instance);
            });
            seq.Run(BlendShapeMappingsPass.Instance);
            seq.WithRequiredExtension(typeof(AnimatorServicesContext), _ =>
            {
                seq.Run(RemoveAnimatorLayersPass.Instance);
                seq.Run(WriteDefaultsPass.Instance);
            });
        }
    }
}

#endif
