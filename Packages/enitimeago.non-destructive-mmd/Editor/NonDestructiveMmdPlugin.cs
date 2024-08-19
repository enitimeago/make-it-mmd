#if NDMMD_VRCSDK3_AVATARS

using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;

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
            seq.Run(BlendShapeMappingsPass.Instance);
            seq.WithRequiredExtension(typeof(AnimationServicesContext), _ =>
            {
                seq.Run(RenameFaceForMmdPass.Instance);
            });
            seq.Run(RemoveAnimatorLayersPass.Instance);
            seq.Run(WriteDefaultsPass.Instance);
        }
    }
}

#endif
