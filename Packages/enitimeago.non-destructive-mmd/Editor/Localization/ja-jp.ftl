locale-ja-jp = 日本語

Common-FixThis = 修正する
Common-Ignore = 無視する

BlendShapeMappingsPass-CombiningWithMultipleFramesUnsupported = 現在、複数のブレンドシェイプを複数フレームで組み合わせることはサポートされていません。この機能をリクエストしたい場合は、本ツールの開発者にお問い合わせしてお願いいたします。

BlendShapeMappingsPass-MissingSourceBlendShapes = MMDモーフ「{$morphName}」は、ブレンドシェイプを組み合わせる際（{$totalShapes}個のブレンドシェイプを組み合わせる中）に参照されるブレンドシェイプ {$missingShapes} を探しましたが、見つかりませんでした。これらのブレンドシェイプは、Unityプロジェクトの他のツールによって削除された可能性があります。これが予期しないものである場合は、可能であればプロジェクトにインストールされているツールと共に、この問題を開発者に報告していただければ幸いです。

BlendShapeMappingsPass-SkippingMmdMorph = MMDモーフ「{$morphName}」を生成する際にエラーが発見されました。このMMDブレンドシェイプは生成しません。

CommonChecks-NoMMDComponents = アバターにMake It MMDコンポーネントが見つかりません。処理するものがありません
CommonChecks-MultipleMMDComponents = アバターに複数のMake It MMDコンポーネントが見つけました！
CommonChecks-NewerDataVersion = 保存されたデータは新しいMake It MMDのバージョンで作ったようです。Make It MMDをアップデートし、または本コンポーネントを削除してください。
CommonChecks-AvatarNotFound = アバターが見つかりません！
CommonChecks-AvatarNoFaceMeshSet = アバターのFace Meshが設定されていません！
CommonChecks-AvatarFaceSMRNotCalledBody = アバターの Face Mesh は MMD ワールドで認識されるために 「Body」 と名前を付ける必要があります。 「MIM Rename Face For MMD」 コンポーネントを使用してこれを修正できます。
CommonChecks-AvatarFaceSMRNotAtRoot = アバターの Face Mesh は MMD ワールドで認識されるために、ヒエラルキー内でアバターの直下に配置する必要があります。 Modular Avatar を使用している場合は 「MA Bone Proxy」 で解決できる可能性があります。
CommonChecks-AvatarWriteDefaultOffFound = アバターの Animator State に Write Default が無効になっている設定が見つかりました。 MMD ワールドでの非互換性の原因となります。 「MIM Avatar Write Defaults」 コンポーネントを使用してこれを修正できます。
CommonChecks-AvatarFaceSMRNoMesh = アバターのFace Meshに、メッシュが設定されていません！
CommonChecks-AvatarFaceSMRNoBlendShapes = アバターのFace Meshに、メッシュがブレンドシェープがありません！
CommonChecks-AvatarFaceSMRExistingBlendShapesUnsupported = 現在、すでにMMDブレンドシェープあるアバターはサポートされていません！
CommonChecks-MorphReferencesNonExistingBlendShape = MMDモーフ「{$morphName}」はこのアバターに存在しないブレンドシェープ「{$blendShapeName}」に参照しています。このMMDブレンドシェープは生成しません。

MappingsEditor-OpenEditor = エディターを開く
MappingsEditor-ShareMenuLabel = 共有...
MappingsEditor-MoreMenuLabel = その他...
MappingsEditor-ShareAsUnitypackage = .unitypackageで共有する...
MappingsEditor-ExportAsJson = .jsonでエクスポートする...
MappingsEditor-ImportFromJson = .jsonからインポートする...
MappingsEditor-ShowStoredData = 保存データを表示
MappingsEditor-ImportBlendShapesSuggestion = このアバターはすでにMMDブレンドシェイプを持っているようです。もしこれらのブレンドシェイプがアバターの既存のブレンドシェイプの単純なコピーであれば、インポートできます。
MappingsEditor-ImportBlendShapesButton = インポート

MappingsEditorWindow-SelectMMDMorph = MMDモーフを選んでください
MappingsEditorWindow-EditingBlendShapesFor = 「{$morphName}」のブレンドシェイプを編集中…
MappingsEditorWindow-ViewingBlendShapesForInPlayMode = 「{$morphName}」のブレンドシェイプを表示中 (編集するには再生モードを終了してください)…
MappingsEditorWindow-SelectedBlendShapes = 選択されているブレンドシェイプ
MappingsEditorWindow-AvatarBlendShapes = アバターのブレンドシェープ
MappingsEditorWindow-HighlightDifferences = 差分を強調表示
MappingsEditorWindow-EnableComputeShader = コンピュートシェーダーを有効にする
MappingsEditorWindow-ThumbnailSize = サムネイルサイズ

MmdScanAndImportWindow-AvatarToScanField = スキャンするアバター
MmdScanAndImportWindow-ScanButtonNoValidAvatar = 有効なアバターを選択してください
MmdScanAndImportWindow-ScanButton = MMDブレンドシェイプをスキャン
MmdScanAndImportWindow-ScanReport = {$totalScanned}個のブレンドシェイプをスキャンしました。{$matchingShapes}/{$knownShapes}個の既知のMMDブレンドシェイプが見つかりました。
MmdScanAndImportWindow-NotFoundHeading = 元のブレンドシェイプが見つかりませんでした:
MmdScanAndImportWindow-ImportToField = インポート先
MmdScanAndImportWindow-WillImportToField = インポート先
MmdScanAndImportWindow-ReplaceExistingToggle = 既存のものを置き換える
MmdScanAndImportWindow-ImportWithReplacementsButton = {$existingCount}個の既存のものを置き換え、{$newCount}個の新しいものをMake It MMD エディタにインポート
MmdScanAndImportWindow-ImportButton = {$newCount}個の新しいものをMake It MMD エディタにインポート
MmdScanAndImportWindow-ImportToSelectedGameObjectButton = 選択されたオブジェクトにインポート
MmdScanAndImportWindow-ImportToNewBrandedGameObjectButton = 新しいMake It MMDオブジェクトにインポート
MmdScanAndImportWindow-ChooseDestinationGameObjectToggle = 目的のオブジェクトを選択
MmdScanAndImportWindow-ImportCompleteDialogTitle = インポート完了
MmdScanAndImportWindow-ImportCompleteDialogMessage = {$importedCount}個のMMDブレンドシェイプをインポートしました
MmdScanAndImportWindow-ImportCompleteDialogOKButton = OK

Import-ImportFailed = インポートが失敗しました
Import-ErrorFileIsEmpty = ファイルが空です
Import-ErrorFailedToReadFile = ファイルの読み取りに失敗しました: {$fileName}
Import-ErrorFailedToParseFile = ファイルの解析に失敗しました: {$fileName}
Import-ErrorGeneric = エラーが発生してインポートに失敗しました: {$errorMessage}
Import-ErrorFileFromNewerVersion = ファイルは Make It MMD の新しいバージョンからのようです

SaveFilePanel-SaveUnitypackage = .unitypackageで保存
SaveFilePanel-SaveJson = .jsonで保存

OpenFilePanel-OpenJson = .jsonファイルを選択

WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOn = FX の WriteDefaults を全て強制的に有効化する
WriteDefaultsComponentEditor-ForceAvatarWriteDefaultsOnDescription = チェックを有効化するとすべてのAnimator State の Write Defaults が有効化されます。Write Defaults ON が前提されていない別途で導入されたギミックがある場合、こちらのギミックまたはアバター自体が正常に機能しなくなる可能性があります。

RemoveAnimatorLayersComponentEditor-FXAnimatorControllerLayers = FXアニメーターコントローラーのレイヤー
RemoveAnimatorLayersComponentEditor-AllowCustomLayers = カスタムのレイヤーを許可する
RemoveAnimatorLayersComponentEditor-AllowUnsafeRemovals = 安全でない削除を許可する
RemoveAnimatorLayersComponentEditor-EditModeLayersExplanation = このリストは、アバターがビルドされると変更される場合があり、他のツールがアバターのFXレイヤーを変更すると不正確になる可能性があります。ビルドされたときにアバターが持つレイヤーを確認するには、再生モードに入ってください。
RemoveAnimatorLayersComponentEditor-PlayModeLayersExplanation = 現在は再生モードでアバターのFXレイヤーが表示されています。編集モードで見えないレイヤーも含まれる場合があります。削除されたレイヤーは「削除するカスタムのレイヤー」の下にリストされるはずです。
RemoveAnimatorLayersComponentEditor-SafeToRemove = 安全に削除できる
RemoveAnimatorLayersComponentEditor-UnsafeToRemove = 安全に削除できない
RemoveAnimatorLayersComponentEditor-CustomLayersToRemove = 削除するカスタムのレイヤー

RenameFaceForMmdComponentEditor-AlreadyCalledBody = このアバターの顔メッシュはすでに「Body」という名前が付けられています。このコンポーネントの必要はありません。
RenameFaceForMmdComponentEditor-ActionToPerform = このコンポーネントは、非破壊的にアバターの顔のメッシュの名前を「{$currentName}」からMMD用の名前「Body」に変更します。
RenameFaceForMmdComponentEditor-ActionToPerformHasConflicts = このアバターはすでに顔じゃない「Body」オブジェクトがあるため、追加に変更します。
RenameFaceForMmdComponentEditor-ActionToPerformSuffix = 下記の名前の変更を行います：
RenameFaceForMmdComponentEditor-IsPlaying = 現在はプレイモードに入っています。必要な名前変更は行いました。
