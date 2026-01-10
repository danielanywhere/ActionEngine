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
