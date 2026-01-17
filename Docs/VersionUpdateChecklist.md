# Version Update Checklist

Perform the following tasks when publishing a new version of *Dans.ActionEngine.Library*.

 - [ ] Make any changes necessary to **ReadMe.docx**.
 - [ ] Update **Docs/NuGetUpdates.md**. This step is required prior to compile because the file is included in the NuGet package.
 - [ ] Run **Scripts/CreateReadMe.cmd**.
 - [ ] Make sure the project is in **Debug** mode.
 - [ ] Update the version number in **ActionEngine.csproj**.
 - [ ] Compile and test all changes to the library.
 - [ ] Switch the project mode to **Release** and compile.
 - [ ] Check the GitHub project online to make sure any issues to be addressed in this version have been completed.
 - [ ] Open **Scripts/ActionEngineDocumentation.shfbproj** and update the version number in **HelpFileVersion**.
 - [ ] Open **Scripts/ActionEngineDocumentation.shfbproj** in SHFB and compile the new version of the API.
 - [ ] Check in API documentation on **danielanywhere.github.io**. Use the summary 'ActionEngine API updates for version {Version}'.
 - [ ] Check in source changes on **danielanywhere/ActionEngine**. Use the summary 'Updates for version {Version}'.
 - [ ] Update or close any associated GitHub issues.
 - [ ] Upload new release version to **NuGet**.

