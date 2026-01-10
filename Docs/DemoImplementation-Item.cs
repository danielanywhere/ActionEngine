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
