#if NDMMD_VRCSDK3_AVATARS

using nadena.dev.ndmf;

namespace enitimeago.NonDestructiveMMD
{
    public class MapperPlugin : Plugin<MapperPlugin>
    {
        public override string QualifiedName => "enitimeago.non-destructive-mmd";
        public override string DisplayName => "Make It MMD";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming);
            seq.Run(MakeBlendShapesPass.Instance);
            seq.Run(WriteDefaultsPass.Instance);
        }
    }
}

#endif
