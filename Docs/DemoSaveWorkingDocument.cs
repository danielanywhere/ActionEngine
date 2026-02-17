//*-----------------------------------------------------------------------*
//* SaveWorkingDocument                                                   *
//*-----------------------------------------------------------------------*
/// <summary>
/// Save the working file to the specified output file.
/// </summary>
protected override void SaveWorkingDocument()
{
  if(WorkingDocument != null &&
    WorkingDocument is SlideWorkingDocumentItem workingDocument &&
    CheckElements(this, ActionElementEnum.OutputFilename))
  {
    // Document is open and output file has been specified.
    try
    {
      using(MemoryStream memoryStream = new MemoryStream())
      {
        workingDocument.Presentation.Save(memoryStream);
        using(FileStream fileStream = File.Create(OutputFile.FullName))
        {
          memoryStream.Position = 0;
          memoryStream.CopyTo(fileStream);
        }
      }
      Trace.WriteLine($" Document saved to: {OutputFile.Name}",
        $"{MessageImportanceEnum.Info}");
    }
    catch(Exception ex)
    {
      Trace.WriteLine(
        $"Error while saving document: {OutputFile.Name}\r\n" +
        $"  {ex.Message}",
        $"{MessageImportanceEnum.Err}");
    }
  }
}
//*-----------------------------------------------------------------------*
