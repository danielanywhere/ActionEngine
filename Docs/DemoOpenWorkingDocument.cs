//*-----------------------------------------------------------------------*
//* OpenWorkingDocument                                                   *
//*-----------------------------------------------------------------------*
/// <summary>
/// Open the working file to allow multiple operations to be completed
/// in the same session.
/// </summary>
protected override void OpenWorkingDocument()
{
  string content = "";
  ActionDocumentItem doc = null;
  int docIndex = 0;

  if(CheckElements(this,
    ActionElementEnum.InputFilename))
  {
    // Load the document if a filename was specified.
    docIndex = WorkingDocumentIndex;
    if(docIndex > -1 && docIndex < InputFiles.Count)
    {
      WorkingDocument =
        new SlideWorkingDocumentItem(InputFiles[docIndex].FullName);
      Trace.WriteLine(
        $" Working document: {this.InputFiles[docIndex].Name}",
        $"{MessageImportanceEnum.Info}");
    }
    else
    {
      Trace.WriteLine(
        $" Working document index out of range at: {docIndex}",
        $"{MessageImportanceEnum.Warn}");
    }
  }
}
//*-----------------------------------------------------------------------*
