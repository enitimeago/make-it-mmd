locale-zh-hant = 繁體中文

Common-FixThis = 修正
Common-Ignore = 忽略

BlendShapeMappingsPass-CombiningWithMultipleFramesUnsupported = 目前不支持合併多個非單幀的 blendshape。如果你希望有此功能，請聯繫開發者。

BlendShapeMappingsPass-MissingSourceBlendShapes = 合併 {$totalShapes} 個 blendshape 時，MMD blendshape「{$morphName}」引用了找不到的 blendshape {$missingShapes}。這些 blendshape 可能已被其他工具刪除。如果這不符合預期，請向開發者報告此問題，並盡可能提供你專案中安裝的工具資訊。

BlendShapeMappingsPass-SkippingMmdMorph = 生成 MMD blendshape「{$morphName}」時發現錯誤，此 blendshape 將不會生成。

CommonChecks-NoMMDComponents = 未在 Avatar 中發現 「Make It MMD」 元件，無事可做
CommonChecks-MultipleMMDComponents = 在 Avatar 中發現多個 「Make It MMD」 元件！
CommonChecks-NewerDataVersion = 資料似乎來自較新版本的 Make It MMD，請更新 Make It MMD 或刪除此元件。
CommonChecks-AvatarNotFound = 沒有發現 Avatar！
CommonChecks-AvatarNoFaceMeshSet = Avatar 尚未設定臉部網格！
CommonChecks-AvatarFaceSMRNotCalledBody = Avatar 的臉部網格名稱必須為「Body」，才能在 MMD 世界中被識別。你可以使用「MIM Rename Face For MMD」元件來修正此問題。
CommonChecks-AvatarWriteDefaultOffFound = Avatar 的 Animator State 設有 Write Default Off，這可能會導致不兼容 MMD 世界。你可以使用「MIM Avatar Write Defaults」元件來修正此問題。
CommonChecks-AvatarFaceSMRNoMesh = 找不到 Avatar 的臉部網格！
CommonChecks-AvatarFaceSMRNoBlendShapes = Avatar 的臉部網格沒有 blendshapes！
CommonChecks-AvatarFaceSMRExistingBlendShapesUnsupported = 目前不支援已有 MMD blendshapes 的 Avatar！
CommonChecks-MorphReferencesNonExistingBlendShape = MMD blendshape「{$morphName}」引用了此 Avatar 上不存在的 blendshape「{$blendShapeName}」。此 MMD blendshape 將不會生成。

MappingsEditor-OpenEditor = 開啟編輯器
MappingsEditor-ShareMenuLabel = 分享...
MappingsEditor-MoreMenuLabel = 更多...
MappingsEditor-ShareAsUnitypackage = 以 .unitypackage 匯出
MappingsEditor-ShowStoredData = 顯示已儲存的資料
MappingsEditor-ImportBlendShapesSuggestion = 這個 Avatar 似乎已有 MMD blendshape。如果這些 blendshape 是 Avatar 現有 blendshape 的簡單副本，則可匯入它們。
MappingsEditor-ImportBlendShapesButton = 匯入

MappingsEditorWindow-SelectMMDMorph = 選擇一個 MMD 的 blendshape
MappingsEditorWindow-EditingBlendShapesFor = 正在編輯「{$morphName}」的 blendshapes...
MappingsEditorWindow-ViewingBlendShapesForInPlayMode = 正在查看「{$morphName}」的 blendshapes（退出播放模式以進行編輯）...
MappingsEditorWindow-SelectedBlendShapes = 已選擇的 blendshapes
MappingsEditorWindow-AvatarBlendShapes = Avatar 的 blendshapes
MappingsEditorWindow-HighlightDifferences = 凸顯差異
MappingsEditorWindow-EnableComputeShader = 啟用 compute shader
MappingsEditorWindow-ThumbnailSize = 縮圖尺寸

MmdScanAndImportWindow-AvatarToScanField = 要掃描的 Avatar
MmdScanAndImportWindow-ScanButtonNoValidAvatar = 選擇一個有效的 Avatar
MmdScanAndImportWindow-ScanButton = 掃描 MMD BlendShapes
MmdScanAndImportWindow-ScanReport = 已掃描 {$totalScanned} 個 blendshapes, 發現 {$matchingShapes}/{$knownShapes} 個已知匹配的 MMD BlendShape。
MmdScanAndImportWindow-NotFoundHeading = 找不到原始 blendshapes：
MmdScanAndImportWindow-ImportToField = 匯入到
MmdScanAndImportWindow-WillImportToField = 將會匯入到
MmdScanAndImportWindow-ReplaceExistingToggle = 替換現有的 blendshapes
MmdScanAndImportWindow-ImportWithReplacementsButton = 替換 {$existingCount} 個現有的 blendshape，匯入 {$newCount} 個新的 blendshape 至 Make It MMD 編輯器
MmdScanAndImportWindow-ImportButton = 匯入 {$newCount} 個新的 blendshape 至 Make It MMD 編輯器
MmdScanAndImportWindow-ImportToSelectedGameObjectButton = 匯入到選定的對象
MmdScanAndImportWindow-ImportToNewBrandedGameObjectButton = 匯入到新的 Make It MMD 物件
MmdScanAndImportWindow-ChooseDestinationGameObjectToggle = 選擇目標物件
MmdScanAndImportWindow-ImportCompleteDialogTitle = 匯入完成
MmdScanAndImportWindow-ImportCompleteDialogMessage = 已匯入 {$importedCount} 個 MMD blendshapes
MmdScanAndImportWindow-ImportCompleteDialogOKButton = 好的

SaveFilePanel-SaveUnitypackage = 儲存 .unitypackage

WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOn = 強制 FX Write Defaults ON
WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOnDescription = 如果啟用，此 Avatar 的所有 Animator 狀態都會是 Write Defaults ON。
    若 Avatar 有不是為了 Write Defaults ON 而設計的東西，可能會故障！

RemoveAnimatorLayersComponentEditor-FXAnimatorControllerLayers = FX Animator Controller Layers
RemoveAnimatorLayersComponentEditor-AllowCustomLayers = 允許自定義 layers
RemoveAnimatorLayersComponentEditor-AllowUnsafeRemovals = 允許不安全的更改
RemoveAnimatorLayersComponentEditor-EditModeLayersExplanation = 這個列表在 Avatar 建置後可能會發生變化，或是因為其他工具修改了 Avatar 的 FX 導致不準確。進入播放模式查看 Avatar 建置後的 layers。
RemoveAnimatorLayersComponentEditor-PlayModeLayersExplanation = 你正在播放模式下查看 Avatar 的 FX layer（可能包含了在編輯模式下看不到的 layer）。要被移除的 layer 應列在「要移除的自定義 Layers」。
RemoveAnimatorLayersComponentEditor-SafeToRemove = 可安全移除
RemoveAnimatorLayersComponentEditor-UnsafeToRemove = 不安全的移除
RemoveAnimatorLayersComponentEditor-CustomLayersToRemove = 要移除的自定義 Layers

RenameFaceForMmdComponentEditor-AlreadyCalledBody = Avatar 的臉部網格已命名為「Body」。不需要此元件。
RenameFaceForMmdComponentEditor-ActionToPerform = 此元件將非破壞性地將 Avatar 的臉部網格從「{$currentName}」重新命名為 MMD 所需的名稱「Body」。
RenameFaceForMmdComponentEditor-ActionToPerformHasConflicts = Avatar 已經有另一個名為「Body」的非臉部網格，它也將被重新命名。
RenameFaceForMmdComponentEditor-ActionToPerformSuffix = 將進行以下重命名：
RenameFaceForMmdComponentEditor-IsPlaying = 目前處於播放模式。已套用重命名（如果需要）。
