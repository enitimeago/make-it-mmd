#region

using nadena.dev.ndmf;

#endregion

namespace enitimeago.NonDestructiveMMD.vendor
{
    /// <summary>
    /// This interface tags components which supply additional animation controllers for merging. They will be given
    /// an opportunity to apply animation path updates when the TrackObjectRenamesContext is committed.
    /// </summary>
    internal interface IOnCommitObjectRenames
    {
        void OnCommitObjectRenames(BuildContext buildContext, PathMappings renameContext);
    }
}
