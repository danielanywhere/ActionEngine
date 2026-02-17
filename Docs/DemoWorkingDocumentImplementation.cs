//*-------------------------------------------------------------------------*
//* SlideWorkingDocumentItem                                                *
//*-------------------------------------------------------------------------*
/// <summary>
/// Information about an individual PowerPoint style working document.
/// </summary>
public class SlideWorkingDocumentItem : ActionDocumentItem
{
  //*************************************************************************
  //* Private                                                               *
  //*************************************************************************
  //*************************************************************************
  //* Protected                                                             *
  //*************************************************************************
  //*************************************************************************
  //* Public                                                                *
  //*************************************************************************
  //*-----------------------------------------------------------------------*
  //* _Constructor                                                          *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Create a new instance of the SlideWorkingDocumentItem item.
  /// </summary>
  public SlideWorkingDocumentItem()
  {
  }
  //*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
  /// <summary>
  /// Create a new instance of the SlideWorkingDocumentItem item.
  /// </summary>
  /// <param name="filename">
  /// The fully qualified path and filename of the document.
  /// </param>
  public SlideWorkingDocumentItem(string filename)
  {
    Name = filename;
    InitializeDocument(filename);
  }
  //*-----------------------------------------------------------------------*

  //*-----------------------------------------------------------------------*
  //*  InitializeDocument                                                   *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Initialize the document object.
  /// </summary>
  /// <param name="filename">
  /// Fully qualified path a filename of the document to load.
  /// </param>
  public void InitializeDocument(string filename)
  {

    if(filename?.Length > 0)
    {
      try
      {
        using(FileStream fileStream = File.OpenRead(filename))
        {
          using(MemoryStream memoryStream = new MemoryStream())
          {
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            mPresentation = new ShapeCrawler.Presentation(memoryStream);
          }
        }
      }
      catch(Exception ex)
      {
        Trace.WriteLine($"Error loading PowerPoint: {ex.Message}",
          $"{MessageImportanceEnum.Err}");
      }
    }
  }
  //*-----------------------------------------------------------------------*

  //*-----------------------------------------------------------------------*
  //*  Presentation                                                         *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Private member for <see cref="Presentation">Presentation</see>.
  /// </summary>
  private ShapeCrawler.Presentation mPresentation = null;
  /// <summary>
  /// Get/Set a reference to the ShapeCrawler presentation data object model
  /// representing the loaded document.
  /// </summary>
  public ShapeCrawler.Presentation Presentation
  {
    get { return mPresentation; }
    set { mPresentation = value; }
  }
  //*-----------------------------------------------------------------------*

}
//*-------------------------------------------------------------------------*

