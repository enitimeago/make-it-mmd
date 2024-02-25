#if NDMMD_VRCSDK3_AVATARS

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
            seq.Run(BlendShapeMappingsPass.Instance);
            seq.Run(RemoveAnimatorLayersPass.Instance);
            seq.Run(WriteDefaultsPass.Instance);
        }
    }
}

#endif
