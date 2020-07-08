#!/usr/bin/env bash
if [ "$APPCENTER_XAMARIN_CONFIGURATION" == "Release" ];then

    echo "Post Build Script Started"
    
    SolutionFile=`find "$APPCENTER_SOURCE_DIRECTORY" -name GitTrends.sln`
    SolutionFileFolder=`dirname $SolutionFile`

    UITestProject=`find "$APPCENTER_SOURCE_DIRECTORY" -name GitTrends.UITests.csproj`

    echo SolutionFile: $SolutionFile
    echo SolutionFileFolder: $SolutionFileFolder
    echo UITestProject: $UITestProject

    chmod -R 777 $SolutionFileFolder

    msbuild "$UITestProject" /property:Configuration="Debug"

    UITestDLL=`find "$APPCENTER_SOURCE_DIRECTORY" -name "GitTrends.UITests.dll" | grep bin  | grep -v ref | head -1`
    echo UITestDLL: $UITestDLL

    UITestBuildDir=`dirname $UITestDLL`
    echo UITestBuildDir: $UITestBuildDir

    UITestVersionNumber=`grep '[0-9]' $UITestProject | grep Xamarin.UITest|grep -o '[0-9]\{1,3\}\.[0-9]\{1,3\}\.[0-9]\{1,3\}'`
    echo UITestVersionNumber: $UITestVersionNumber

    TestCloudExe=`find ~/.nuget | grep test-cloud.exe | grep $UITestVersionNumber | head -1`
    echo TestCloudExe: $TestCloudExe

    TestCloudExeDirectory=`dirname $TestCloudExe`
    echo TestCloudExeDirectory: $TestCloudExeDirectory

    IPAFile=`find "$APPCENTER_SOURCE_DIRECTORY" -name *.ipa | head -1`
    echo IPAFile: $IPAFile

    DSYMFile=`find "$APPCENTER_SOURCE_DIRECTORY" -name *.dSYM | head -1`
    echo DSYMFile: $DSYMFile

    npm install -g appcenter-cli@1.2.2

    appcenter login --token $AppCenterAPIToken

    appcenter crashes upload-symbols --app "CDA-Global-Beta/GitTrends-iOS-Debug" --symbol $DSYMFile

    appcenter test run uitest --app "CDA-Global-Beta/GitTrends-iOS-Debug" --devices "CDA-Global-Beta/current-ios-minus-1" --app-path $IPAFile --test-series "master" --locale "en_US" --build-dir $UITestBuildDir --dsym-dir $DSYMFile --uitest-tools-dir $TestCloudExeDirectory --async
fi

#appcenter test run uitest --app "CDA-Global-Beta/GitTrends-iOS-Debug" --devices "CDA-Global-Beta/current-ios-minus-1" --app-path ~/GitHub/GitTrends/GitTrends.iOS/bin/iPhone/Release/GitTrends.ipa --test-series "master" --locale "en_US" --build-dir ~/GitHub/GitTrends/GitTrends.UITests/bin/Debug/ --uitest-tools-dir /Users/bramin/.nuget/packages/xamarin.uitest/3.0.8-dev1/tools --async