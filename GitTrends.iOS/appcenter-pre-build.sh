#!/usr/bin/env bash
AzureConstantsFile=`find "$APPCENTER_SOURCE_DIRECTORY" -name AzureConstants.cs | head -1`
echo CognitiveServicesConstantsFile = $AzureConstantsFile

sed -i '' "s/GetUITestTokenApiKey = \"\"/GetUITestTokenApiKey = \"$UITestTokenApiKey\"/g" "$AzureConstantsFile"

sed -i '' "s/GetSyncFusionInformationApiKey = \"\"/GetSyncFusionInformationApiKey = \"$GetSyncFusionInformationApiKey\"/g" "$AzureConstantsFile"

sed -i '' "s/GetNotificationHubInformationApiKey = \"\"/GetNotificationHubInformationApiKey = \"$GetNotificationHubInformationApiKey\"/g" "$AzureConstantsFile"

sed -i '' "s/#error Missing API Keys/\/\/#error Missing API Keys/g" "$AzureConstantsFile"

echo "Finished Injecting API Keys"