locale-en-us = English

Common-FixThis = Fix this
Common-Ignore = Ignore

BlendShapeMappingsPass-CombiningWithMultipleFramesUnsupported = Currently, combining multiple blend shapes with more than one frame is unsupported. Please contact the developer if you would like to request this feature.

BlendShapeMappingsPass-MissingSourceBlendShapes = MMD morph "{$morphName}" references blend shape(s) {$missingShapes} (when combining {$totalShapes} blend shapes) that weren't found. These blend shape(s) may have been removed by other tools in your Unity project. If this is unexpected, please report this to the developer along with the tools installed in your project if possible.

BlendShapeMappingsPass-SkippingMmdMorph = Found error(s) when generating MMD morph "{$morphName}". This MMD blend shape will not be generated.

CommonChecks-NoMMDComponents = No Make It MMD component found in avatar. Nothing to do
CommonChecks-MultipleMMDComponents = More than one Make It MMD component found in avatar!
CommonChecks-NewerDataVersion = Data seems to be from a newer version of Make It MMD, please update Make It MMD or delete this component.
CommonChecks-AvatarNotFound = Avatar not found!
CommonChecks-AvatarNoFaceMeshSet = Avatar has not set a face mesh!
CommonChecks-AvatarFaceSMRNotCalledBody = Avatar face mesh must be called "Body" to be recognized in MMD worlds. You can use the "MIM Rename Face For MMD" component to fix this.
CommonChecks-AvatarFaceSMRNotAtRoot = Avatar face mesh must be directly underneath your avatar in the hierarchy to be recognized in MMD worlds. If you use Modular Avatar, you may be able to fix this with "MA Bone Proxy".
CommonChecks-AvatarWriteDefaultOffFound = Animator States on this avatar with Write Default OFF were found, which can cause incompatibility with MMD worlds. You can use the "MIM Avatar Write Defaults" component to fix this.
CommonChecks-AvatarFaceSMRNoMesh = Avatar face mesh is missing!
CommonChecks-AvatarFaceSMRNoBlendShapes = Avatar face mesh has no blend shapes!
CommonChecks-AvatarFaceSMRExistingBlendShapesUnsupported = Avatars with pre-existing MMD blend shapes are currently unsupported!
CommonChecks-MorphReferencesNonExistingBlendShape = MMD morph "{$morphName}" references a blend shape "{$blendShapeName}" which doesn't exist on this avatar. This MMD blend shape will not be generated.

MappingsEditor-OpenEditor = Open Editor
MappingsEditor-ShareMenuLabel = Share...
MappingsEditor-MoreMenuLabel = More...
MappingsEditor-ShareAsUnitypackage = Share as .unitypackage...
MappingsEditor-ExportAsJson = Export as .json...
MappingsEditor-ImportFromJson = Import from .json...
MappingsEditor-ShowStoredData = Show stored data
MappingsEditor-ImportBlendShapesSuggestion = This avatar seems to already have MMD blend shapes. If these blend shapes are simple copies of the avatar's existing blend shapes, they can be imported.
MappingsEditor-ImportBlendShapesButton = Import

MappingsEditorWindow-SelectMMDMorph = Select a MMD morph
MappingsEditorWindow-EditingBlendShapesFor = Editing blendshapes for "{$morphName}"...
MappingsEditorWindow-ViewingBlendShapesForInPlayMode = Viewing blendshapes for "{$morphName}" (exit play mode to edit)...
MappingsEditorWindow-SelectedBlendShapes = Selected blendshapes
MappingsEditorWindow-AvatarBlendShapes = Avatar blendshapes
MappingsEditorWindow-HighlightDifferences = Highlight differences
MappingsEditorWindow-EnableComputeShader = Enable compute shader
MappingsEditorWindow-ThumbnailSize = Thumbnail size

MmdScanAndImportWindow-AvatarToScanField = Avatar to Scan
MmdScanAndImportWindow-ScanButtonNoValidAvatar = Select a Valid Avatar
MmdScanAndImportWindow-ScanButton = Scan MMD Blend Shapes
MmdScanAndImportWindow-ScanReport = Scanned {$totalScanned} blend shapes, found matching blend shapes for {$matchingShapes}/{$knownShapes} known MMD blend shapes.
MmdScanAndImportWindow-NotFoundHeading = Couldn't find original blend shapes for:
MmdScanAndImportWindow-ImportToField = Import to
MmdScanAndImportWindow-WillImportToField = Will Import to
MmdScanAndImportWindow-ReplaceExistingToggle = Replace Existing
MmdScanAndImportWindow-ImportWithReplacementsButton = Replace {$existingCount} Existing, Import {$newCount} New to Make It MMD Editor
MmdScanAndImportWindow-ImportButton = Import {$newCount} New to Make It MMD Editor
MmdScanAndImportWindow-ImportToSelectedGameObjectButton = Import to Selected Object
MmdScanAndImportWindow-ImportToNewBrandedGameObjectButton = Import to New Make It MMD Object
MmdScanAndImportWindow-ChooseDestinationGameObjectToggle = Choose Destination GameObject
MmdScanAndImportWindow-ImportCompleteDialogTitle = Import complete
MmdScanAndImportWindow-ImportCompleteDialogMessage = Imported {$importedCount} MMD blend shapes
MmdScanAndImportWindow-ImportCompleteDialogOKButton = OK

Import-ImportFailed = Import failed
Import-ErrorFileIsEmpty = File is empty
Import-ErrorFailedToReadFile = Failed to read file: {$fileName}
Import-ErrorFailedToParseFile = Failed to parse file: {$fileName}
Import-ErrorGeneric = Import failed with error: {$errorMessage}
Import-ErrorFileFromNewerVersion = File seems to be from a newer version of Make It MMD

SaveFilePanel-SaveUnitypackage = Save .unitypackage
SaveFilePanel-SaveJson = Save .json

OpenFilePanel-OpenJson = Open .json

WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOn = Force all FX Write Defaults ON
WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOnDescription = If enabled, all Animator States on this avatar will have Write Defaults enabled. If your avatar has gimmicks not designed for Write Defaults ON, your avatar and/or these gimmicks MAY BREAK!

RemoveAnimatorLayersComponentEditor-FXAnimatorControllerLayers = FX Animator Controller Layers
RemoveAnimatorLayersComponentEditor-AllowCustomLayers = Allow custom layers
RemoveAnimatorLayersComponentEditor-AllowUnsafeRemovals = Allow unsafe removals
RemoveAnimatorLayersComponentEditor-EditModeLayersExplanation = This list may change when your avatar is built and could be inaccurate if other tools modify your avatar's FX layers. Enter Play Mode to see what layers your avatar has when built.
RemoveAnimatorLayersComponentEditor-PlayModeLayersExplanation = You are viewing your avatar's FX layers in Play Mode, which may include layers not seen in Edit Mode. Layers that got removed should be listed under "Custom Layers To Remove".
RemoveAnimatorLayersComponentEditor-SafeToRemove = Safe to remove
RemoveAnimatorLayersComponentEditor-UnsafeToRemove = Unsafe to remove
RemoveAnimatorLayersComponentEditor-CustomLayersToRemove = Custom Layers To Remove

RenameFaceForMmdComponentEditor-AlreadyCalledBody = Your avatar's face mesh is already called "Body". This component is not required.
RenameFaceForMmdComponentEditor-ActionToPerform = This component will non-destructively rename your avatar's face mesh "{$currentName}" to the required name for MMD "Body".
RenameFaceForMmdComponentEditor-ActionToPerformHasConflicts = Your avatar already has another "Body" that is not a face mesh, which will also be renamed.
RenameFaceForMmdComponentEditor-ActionToPerformSuffix = The following renames will be performed:
RenameFaceForMmdComponentEditor-IsPlaying = Currently in play mode. Renames have been applied if necessary.
