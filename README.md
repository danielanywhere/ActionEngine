# ActionEngine

This is an action-handling batch processing backend class library with several built-in features, like Input Files collection, Working Document, etc.

This library is mostly used in command-line applications performing utilitarian functions. Since I have been re-using the same basic approach in the building of at least fifteen to twenty similar yet unique applications, I felt that I should finally pull the trigger and start moving toward a standard way to handle sequence-friendly activities, where one or more actions are performed on one or more files.

Among other things that will be documented soon, the following features are built-in to this library.

<p>&nbsp;</p>

## Multi-Level Action Handling

At any step, an action of type: Batch can be used to define a series of other actions, which can either reside in the Actions property of that action or within a separate JSON-formatted configuration file.

<p>&nbsp;</p>

## Conditional Flow

Using the **If** action type, you can set a condition under which the entries in that action's **Actions** collection will be run. Branching scenarios are also very easy to maintain using multiple levels of **If**.

<p>&nbsp;</p>

## Action Sequences

An individual list of actions can be used multiple times by enclosing it within an action of type **Sequence**.

<p>&nbsp;</p>

## Multiple Actions Per File

Using the **ForEachFile** action type, you can rerun the same set of actions upon each file in the input files collection.

<p>&nbsp;</p>

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

<p>&nbsp;</p>

## Other Operations

You are also able to perform the various general activities with built-in features and functionality.

-   Work with data files.
-   Fully resolve and work with input and output files, including wildcards on filenames and folder names.
-   Load, work with, and save a working document.
-   Load, work with, and save images.

<p>&nbsp;</p>

## Example Host Application

Following is a working example from my soon-to-be-published project PPCL.

```csharp
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using ActionEngine;
using Newtonsoft.Json;
using StyleAgnosticCommandArgs;

namespace PPCL
{
  //*-------------------------------------------------------------------------*
  //* Program                                                                 *
  //*-------------------------------------------------------------------------*
  /// <summary>
  /// Main application instance for PPCL (PowerPoint Control Language).
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
      bool bShowHelp = false; //  Flag - Explicit Show Help.
      CommandArgCollection commandArgs = null;
      string key = "";        //  Current Parameter Key.
      string lowerArg = "";   //  Current Lowercase Argument.
      NameValueCollection nameValues = null;
      StringBuilder message = new StringBuilder();
      Program prg = new Program();  //  Initialized instance.

      ConsoleTraceListener consoleListener = new ConsoleTraceListener();
      Trace.Listeners.Add(consoleListener);

      Console.WriteLine("PPCL.exe");

      ActionEngine.ActionEngineUtil.RecognizedActions.AddRange(new string[]
      {
        "SlideReport"
      });

      prg.mActionItem = new PActionItem();

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
            action =
              ActionEngine.ActionEngineUtil.GetActionName(argItem.Value);
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
        //  Display Syntax.
        Console.WriteLine(message.ToString() + "\r\n" + ResourceMain.Syntax);
      }
      else
      {
        //  Run the configured application.
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
    //* ActionItem                                                            *
    //*-----------------------------------------------------------------------*
    private PActionItem mActionItem = null;
    /// <summary>
    /// Get/Set the file action item associated with this session.
    /// </summary>
    public PActionItem ActionItem
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
