# ActionEngine

This is an action-handling batch processing backend class library with several built-in features, like Input Files collection, Working Document, etc.

This library is mostly used in command-line applications performing utilitarian functions. Since I have been re-using the same basic approach in the building of at least fifteen to twenty similar yet unique applications, I felt that I should finally pull the trigger and start moving toward a standard way to handle sequence-friendly activities, where one or more actions are performed on one or more files.

Among other things that will be documented soon, the following features are built-in to this library.

---

## Built-In Variables

The following variables are built-in to the base **ActionItem**.

-   **Action**. Not inheritable. The name of the action associated with this entry. See the Built-In Actions list below for a list of the automatically recognized actions. Add more actions by adding their names to the static **RecognizedActions** list and overriding the protected method **RunCustomAction()** as shown in the examples below.
-   **Actions**. Not inheritable. A collection of child actions of the current action. Used in **Batch**, **ForEachFile**, **If**, and **RunSequence** actions.
-   **Base**. Inheritable. A generic string used to set the base number or filename pattern of the source or target files, depending upon the action. Can be specified in the **CheckElements** function parameter. See the **CopyRange** method of **FileActionItem** in the [FileTools](https://github.com/danielanywhere/FileTools) repository.
-   **Condition**. Inheritable. A string representing the condition expression for flow control on this action. Used in **If**.
-   **ConfigFilename**. Not inheritable. The path and filename of the configuration file for this action. Used in **Batch**.
-   **Count**. Inheritable. A generic count associated with the current action. Can be specified in the **CheckElements** function parameter.
-   **CurrentFile**. Inheritable. A file info reference to the current active file in use.
-   **DataDir**. Not inheritable. The internally calculated data directory info.
-   **DataFilename**. Inheritable. The path and filename of the reference data file. Can be specified in the **CheckElements** function parameter.
-   **DataFiles**. Inheritable. Reference to the list of FileInfo objects representing files used as reference in this session.
-   **DataNames**. Inheritable. Reference to the list of filenames or folder names with or without wildcards. This parameter can be specified multiple times on the command line with different values to load multiple input files. Typically, this value corresponds with a command-line parameter 'DataFiles'.
-   **DateTimeValue**. Inheritable. A generic date and time to be associated with the current action. Can be specified in the **CheckElements** function parameter.
-   **DefaultVariables**. Not inheritable. List of Name/Value combinations representing default variable values assigned to this action. Although this list is not inheritable, a composite list of all variable values at the present and higher levels is checked when using the **GetVariable** function.
-   **Digits**. Inheritable. The generic number of digits associated with the current action. Can be specified in the **CheckElements** function parameter.
-   **Images**. Not inheritable. A list of images in this session.
-   **InputDir**. Not inheritable. Reference to the internally calculated input directory info.
-   **InputFilename**. Inheritable. The input path and filename of the input file.
-   **InputFiles**. Inheritable. A reference to the collection of file information used as input in this session.
-   **InputFolderName**. Inheritable. A generic folder name for operations where the folder is the focus of the operation, rather than a file.
-   **InputNames**. Inheritable. A reference to the list of filenames or folder names with or without wildcards. Because it is a collection, this parameter can be specified multiple times on the command line with different values to load multiple input files.
-   **Message**. Not inheritable. A message to be displayed when this action is run.
-   **Options**. Not inheritable. A reference to the collection of options assigned to this action. Although this value is not inheritable, the named option anywhere between this level and any of its ancestors can be found by calling the **GetOptionByName** function.
-   **OutputDir**. Inheritable. Reference to the internally calculated output directory info.
-   **OutputFile**. Inheritable. Reference to the internally calculated output file info.
-   **OutputFilename**. Inheritable. The output path and filename for this action.
-   **OutputFolderName**. Inheritable. The output path and folder name for this action if the intent is for a folder instead of a file.
-   **OutputName**. Inheritable. An output pattern that allows for filenames or folder names with or without wildcards.
-   **Parent**. Not inheritable. The reference to the parent collection of this item. To retrieve the parent action, use an access similar to **this.Parent.Parent**.
-   **Pattern**. Inheritable. A regular expression pattern for files, folders, or other appropriate strings.
-   **Properties**. Not inheritable. A reference to the collection of Name/Value properties assigned to this action. Although this property is not inheritable, a property from this or any parent levels can be retrieved by calling the **GetPropertyByName** function.
-   **Range**. Inheritable. A reference to the start and end values of a generic range. This value can be specified in the parameter of the **CheckElements** function parameter.
-   **RecognizedActions**. Static. Not inheritable. Reference to a collection of all recognized action names in this session. If you wish to add customized actions, add the name of that action to **RecognizedActions**, then process for its presence by overriding the protected method **RunCustomAction**. as shown in the examples below. Alternatively, because the built-in actions are loaded into the list during initialization, you can also avoid processing of any of those actions by removing their names from the list before processing.
-   **Sequences**. Inheritable. The collection of sequences defined for this action. Used by **RunSequence**.
-   **SourceDir**. Not inheritable. Internally calculated generic source directory info.
-   **SourceFolderName**. Inheritable. The path and folder name of the data source for this action.
-   **Stop**. Inheritable. A value indicating whether the process should be stopped at this action.
-   **Text**. Inheritable. The generic text of the current action.
-   **VariableName**. Inheritable. The name of the variable to access in this action.
-   **Variables**. Combinatorially inherited. A reference to the collection of variables on this instance.
-   **WorkingDocument**. Inheritable. A reference to the working key file for operations in this instance.
-   **WorkingDocumentIndex**. Inheritable. The input file index representing the working document.
-   **WorkingImage**. Not inheritable. A reference to the current working image in this session.
-   **WorkingPath**. Inheritable. The working path for operations in this instance. If the working path is set, any of the input and output filenames can optionally be expressed as relative values.

---

## Built-In Actions

The following actions are built-in to the **Action** property of the Action item base.

-   **None**. Placeholder for action unknown or not selected. In this version, the Action property might also be blank or null, since it is a normal string. However, all of those conditions are counted as 'None'.
-   **Batch**. Perform a batch of file operations from a single configuration file.
-   **DrawImage**. Draw the image specified by ImageName onto the working image at the location specified by user properties Left and Top.
-   **FileOpenImage**. Open the image file specified in the current input file. Name it in the local images collection with the name specified in the user property ImageName.
-   **FileOverlayImage**. Open each image from the range and place the image specified in InputFilename at the options specified by Left, Top, Width, and Height.
-   **FileSaveImage**. Save the working image to the currently specified OutputFile.
-   **ForEachFile**. Run the Actions collection of the action through all of the files currently loaded in the InputFiles collection, setting the CurrentFile property for each pass.
-   **If**. Run one or more sets of actions if their conditions are true. In this version, each condition is an expression that evaluates to true or false, able to use the public static methods of System.Math, and the values of the local variables CurrentFilename and CurrentFileNumber.
-   **ImageBackground**. Set the background color of the working image, overlaying the previous contents to create the new background.
-   **ImagesClear**. Clear all images from the Images collection.
-   **OpenWorkingDocument**. Open the working document. This is a blank protected virtual void method you implement yourself to create a derivative of an ActionDocumentItem that is assigned to the built-in WorkingDocument variable at the current level (see example below). In this version, the working document is seen as being a transitive document. In other words, its purpose is slightly different than either the input or output files in that multiple input files might be used to apply changes to the working document, and the working document itself might be converted before being saved as the output document.
-   **RunSequence**. Run the sequence specified in the 'SequenceName' user property.
-   **SaveWorkingDocument**. Save the working document. This is a blank protected virtual void method you implement yourself to save the derivative of an ActionDocumentItem that was assigned to the built-in WorkingDocument variable at the current level (see example below).
-   **SetWorkingImage**. Set the current working image to the one with the local name found in the user property ImageName.
-   **SizeImage**. Scale the image to a new size, as specified in user properties Width and Height.

---

## Multi-Level Action Handling

At any step, an action of type: Batch can be used to define a series of other actions, which can either reside in the Actions property of that action or within a separate JSON-formatted configuration file.

---

## Conditional Flow

Using the **If** action type, you can set a condition under which the entries in that action's **Actions** collection will be run. Branching scenarios are also very easy to maintain using multiple levels of **If**.

---

## Action Sequences

An individual list of actions can be used multiple times by enclosing it within an action of type **Sequence**.

---

## Multiple Actions Per File

Using the **ForEachFile** action type, you can rerun the same set of actions upon each file in the input files collection.

---

## Check for Supplied Values

Anywhere in the instance, you can make a call to **CheckElements** to see if any combination of the following values have been supplied. This is usually performed as soon as execution enters your custom method.

-   **Action**. Whether the action itself was supplied.
-   **Base**. Whether a base value has been supplied.
-   **Count**. Whether the count value has been specified.
-   **DataFilename**. Whether the data filename has been specified and was legal.
-   **DateTimeValue**. Whether the general date time value has been specified.
-   **Digits**. Whether the count of digits has been specified.
-   **InputFilename**. Whether the input filename has been specified.
-   **InputFolderName**, Whether the input folder name has been specified.
-   **Inputs**. Whether the list of input file or folder names have been specified, depending upon the context.
-   **OutputFilename**. Whether the output filename has been specified.
-   **OutputFoldername**. Whether the output folder name has been specified.
-   **OutputName**. Whether the output file or folder name has been specified, depending upon the context.
-   **Pattern**. Whether the regular expression pattern has been specified.
-   **Prefix**. Whether the prefix flag has been specified.
-   **Range**. Whether the range, start through end, has been specified.
-   **SourceFolderName**. Whether the source data folder name has been specified.
-   **Suffix**. Whether the suffix flag has been specified.
-   **Text**. Whether the text-only pattern has been specified.
-   **WorkingPath**. Whether the working path has been specified.
-   **OptionPrefix**. Whether the prefix option has been specified.
-   **OptionSuffix**. Whether the suffix option has been specified.

---

## Other Operations

You are also able to perform the various general activities with built-in features and functionality.

-   Work with data files.
-   Fully resolve and work with input and output files, including wildcards on filenames and folder names.
-   Load, work with, and save a working document.
-   Load, work with, and save images.

---

## Basic Schema

Configuration data can be loaded from and saved to a JSON-formatted text file. The following schema represents the generic base of the library itself. Any public properties you add to your own implementation class will automatically be added to this schema.

```plaintext
ActionItem
{
	Action: string,
	Actions: ActionItem[],
	Base: string,
	Conditions: ConditionItem[],
	ConfigFilename: string,
	Count: float,
	DataFilename: string,
	DataNames: string[],
	DateTimeValue: DateTime,
	Digits: int,
	InputFilename: string,
	InputFolderName: string,
	InputNames: string[],
	Message: string,
	Options: ActionOptionItem[],
	OutputFilename: string,
	OutputFolderName: string,
	OutputName: string,
	Pattern: string,
	Properties: NameValueItem[],
	Range: StartEndItem,
	Sequences: SequenceItem[],
	SourceFolderName: string,
	Text: string,
	WorkingPath: string
}

ActionOptionItem
{
	Name: string,
	Value: string
}

ConditionItem
{
	Assignment: string,
	ConditionItem: string
}

NameValueItem
{
	Name: string,
	Value: string
}

SequenceItem
{
	Name: string,
	Actions: ActionItem[]
}

StartEndItem
{
	StartValue: string
	EndValue: string,
}


```

---

### Example JSON File

Following is an example of a basic JSON file using the above schema.

```json
{
	"Remark": "Example file saved as 'C:\\Scripts\\Processing.json'",
	"WorkingPath": "C:\\Temp\\Audio",
	"InputFilename": "*.mp3",
	"Action": "ForEachFile",
	"Actions":
	[
		{
			"Remark": "These actions are demo implementation-level samples.",
			"Action": "ProcessAudio-FirstPass",
			"Settings": "First;pass;settings"
		},
		{
			"Action": "ProcessAudio-SecondPass",
			"Settings": "Second;pass;settings"
		},
		{
			"Action": "ProcessAudio-ThirdPass",
			"Settings": "Third;pass;settings"
		}
	]
}


```

---

### Example Command-Line

The following command-line is compatible with the JSON example in the preceding section.

```plaintext
audioprocessor.exe /action:batch /configfile:C:\Scripts\Processing.json

```

---

## Implementing Your Class

The action item and collection base classes are generic and abstract, meaning that you must create your own concrete implementations of each one. Because of the recursive nature of the action, the item and collection are implemented as a pair, so when you plan to use one, you will need to define both. However, only your inherited item will require internal code implementation.

Following are typical examples of definitions for the item and collection classes.

---

### Collection Implementation

The collection doesn't require any implementation in the class body. It is provided to seal the loop for the Curiously Recurring Template Pattern (CRTP).

```csharp
//*-------------------------------------------------------------------------*
//* MyActionCollection                                                      *
//*-------------------------------------------------------------------------*
/// <summary>
/// Collection of MyActionItem Items.
/// </summary>
public class MyActionCollection :
 ActionCollectionBase<MyActionItem, MyActionCollection>
{
}
//*-------------------------------------------------------------------------*

```

---

### Item Implementation

The following item definition contains a few superfluous lines to demonstrate a primitive version of a concrete class that inherits the complete functionality of the ActionEngine.

```csharp
//*-------------------------------------------------------------------------*
//* MyActionItem                                                            *
//*-------------------------------------------------------------------------*
/// <summary>
/// Information about an individual concrete item.
/// </summary>
public class MyActionItem : ActionItemBase<MyActionItem, MyActionCollection>
{
 //*************************************************************************
 //* Private                                                               *
 //*************************************************************************
 //*-----------------------------------------------------------------------*
 //* ProcessAudio                                                          *
 //*-----------------------------------------------------------------------*
 /// <summary>
 /// Demo method. Process an audio pass.
 /// </summary>
 /// <param name="item">
 /// Reference to the action item containing processing information.
 /// </param>
 /// <param name="passIndex">
 /// The logical pass to process.
 /// </param>
 private void ProcessAudio(MyActionItem item, int passIndex)
 {
  // ...
 }
 //*-----------------------------------------------------------------------*

 //*************************************************************************
 //* Protected                                                             *
 //*************************************************************************
 //*-----------------------------------------------------------------------*
 //* RunCustomAction                                                       *
 //*-----------------------------------------------------------------------*
 /// <summary>
 /// Run your custom actions by overriding this method.
 /// </summary>
 protected override void RunCustomAction()
 {
  string action = Action.ToLower();

  switch(action)
  {
   case "processaudio-firstpass":
    ProcessAudio(this, 1);
    break;
   case "processaudio-secondpass":
    ProcessAudio(this, 2);
    break;
   case "processaudio-thirdpass":
    ProcessAudio(this, 3);
    break;
  }
 }
 //*-----------------------------------------------------------------------*

 //*-----------------------------------------------------------------------*
 //* WriteLocalOutput                                                      *
 //*-----------------------------------------------------------------------*
 /// <summary>
 /// Override to write the local output of this operation during a step.
 /// </summary>
 protected override void WriteLocalOutput()
 {
  base.WriteLocalOutput();
 }
 //*-----------------------------------------------------------------------*

 //*************************************************************************
 //* Public                                                                *
 //*************************************************************************

 /*
 // Add any of your own public properties to include them in the JSON
 // schema for your class. If you wish to make those properties inheritable
 // from inner levels, use a technique similar to the following.

 public string MyInheritableValue
 {
  get
  {
   string result = mMyInheritableValue;

   if(string.IsNulllOrEmpty(result))
   {
    if(Parent?.Parent != null)
    {
     result = Parent.Parent.MyInheritableValue;
    }
    else
    {
     result = "";
    }
   }
   return result;
  }
 }

 */

}
//*-------------------------------------------------------------------------*

```

---

## Example Host Application

Following is a working command-line program using the audio examples from above.

```csharp
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using ActionEngine;
using Newtonsoft.Json;
using StyleAgnosticCommandArgs;

namespace AudioProcessor
{
 //*-------------------------------------------------------------------------*
 //* Program                                                                 *
 //*-------------------------------------------------------------------------*
 /// <summary>
 /// Main application instance for My Audio Processing Application.
 /// </summary>
 public class Program
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
  //* _Main                                                                 *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Configure and run the application.
  /// </summary>
  public static async Task Main(string[] args)
  {
   string action = "";
   bool bActivity = false;
   bool bShowHelp = false; // Flag - Explicit Show Help.
   CommandArgCollection commandArgs = null;
   string key = "";        // Current Parameter Key.
   string lowerArg = "";   // Current Lowercase Argument.
   NameValueCollection nameValues = null;
   StringBuilder message = new StringBuilder();
   Program prg = new Program();  // Initialized instance.

   ConsoleTraceListener consoleListener = new ConsoleTraceListener();
   Trace.Listeners.Add(consoleListener);

   Console.WriteLine("AudioProcessor.exe");

   MyActionItem.RecognizedActions.AddRange(new string[]
   {
    "ProcessAudio-FirstPass",
    "ProcessAudio-SecondPass",
    "ProcessAudio-ThirdPass"
   });

   prg.mActionItem = new MyActionItem();

   commandArgs = new CommandArgCollection(args);
   foreach(CommandArgItem argItem in commandArgs)
   {
    key = argItem.Name.ToLower();
    switch(key)
    {
     case "":
      key = argItem.Value.ToLower();
      switch(key)
      {
       case "?":
        bShowHelp = true;
        break;
       case "wait":
        prg.mWaitAfterEnd = true;
        break;
      }
      break;
     case "action":
      action = MyActionItem.GetActionName(argItem.Value);
      if(action != "None")
      {
       prg.ActionItem.Action = action;
       bActivity = true;
      }
      else
      {
       message.Append("Error: No action specified...");
       bShowHelp = true;
      }
      break;
     case "configfile":
      prg.ActionItem.ConfigFilename = argItem.Value;
      break;
     case "infile":
      prg.ActionItem.InputNames.Add(argItem.Value);
      break;
     case "option":
      prg.ActionItem.Options.Add(argItem.Value);
      break;
     case "outfile":
      prg.ActionItem.OutputFilename = argItem.Value;
      break;
     case "properties":
      try
      {
       nameValues = JsonConvert.DeserializeObject<NameValueCollection>(
        argItem.Value);
       foreach(NameValueItem propertyItem in nameValues)
       {
        prg.mActionItem.Properties.Add(propertyItem);
       }
      }
      catch(Exception ex)
      {
       Console.WriteLine($"Error parsing properties: {ex.Message}");
       bShowHelp = true;
      }
      break;
     case "workingpath":
      prg.ActionItem.WorkingPath = argItem.Value;
      break;
    }
   }

   if(!bShowHelp && !bActivity)
   {
    message.AppendLine(
     "Please specify an action or a stand-alone activity.");
    bShowHelp = true;
   }
   if(bShowHelp)
   {
    // Display Syntax.
    Console.WriteLine(message.ToString() + "\r\n" + ResourceMain.Syntax);
   }
   else
   {
    // Run the configured application.
    await prg.Run();
   }
   if(prg.mWaitAfterEnd)
   {
    Console.WriteLine("Press [Enter] to exit...");
    Console.ReadLine();
   }
  }
  //*-----------------------------------------------------------------------*

  //*-----------------------------------------------------------------------*
  //* ActionItemBase                                                        *
  //*-----------------------------------------------------------------------*
  private MyActionItem mActionItem = null;
  /// <summary>
  /// Get/Set the file action item associated with this session.
  /// </summary>
  public MyActionItem ActionItem
  {
   get { return mActionItem; }
   set { mActionItem = value; }
  }
  //*-----------------------------------------------------------------------*

  //*-----------------------------------------------------------------------*
  //* Run                                                                   *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Run the configured application.
  /// </summary>
  public async Task Run()
  {
   if(!ActionEngine.ActionEngineUtil.ActionIsNone(mActionItem.Action))
   {
    await mActionItem.Run();
   }
  }
  //*-----------------------------------------------------------------------*

  //*-----------------------------------------------------------------------*
  //* WaitAfterEnd                                                          *
  //*-----------------------------------------------------------------------*
  /// <summary>
  /// Private member for <see cref="WaitAfterEnd">WaitAfterEnd</see>.
  /// </summary>
  private bool mWaitAfterEnd = false;
  /// <summary>
  /// Get/Set a value indicating whether to wait for user keypress after
  /// processing has completed.
  /// </summary>
  public bool WaitAfterEnd
  {
   get { return mWaitAfterEnd; }
   set { mWaitAfterEnd = value; }
  }
  //*-----------------------------------------------------------------------*

 }
 //*-------------------------------------------------------------------------*


}

```

---

## Working Document Example

The [SlideTools](https://github.com/danielanywhere/SlideTools) application for managing PowerPoint files at the repository https://github.com/danielanywhere/SlideTools demonstrates one method for opening and saving the working document while keeping the using the built-in WorkingDocumentIndex as an active index into the built-in InputFiles collection.

---

### ActionDocumentItem Implementation

The WorkingDocument built-in variable is based upon an ActionDocumentItem. The following block demonstrates the specialization used for SlideTools.

```csharp
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


```

---

### Open the Working Document

The following example from SlideTools demonstrates opening the working document from the script-specified location in the InputFiles list.

```csharp
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

```

---

### Save the Working Document

In the SlideTools application, the working document is saved through [ShapeCrawler's](https://github.com/ShapeCrawler/ShapeCrawler) Presentation object.

```csharp
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

```


## Updates

| Version | Description |
|---------|-------------|
| 26.2117.4453 | Initial publication to NuGet. |


## More Information

For more information, please see the GitHub project:
[danielanywhere/ActionEngine](https://github.com/danielanywhere/ActionEngine)

Full API documentation is available at this library's [GitHub User Page](https://danielanywhere.github.io/ActionEngine).

