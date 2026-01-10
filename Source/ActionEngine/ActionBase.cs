/*
* Copyright (c). 2025-2026 Daniel Patterson, MCSD (danielanywhere).
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <https://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

using Flee;
using Flee.PublicTypes;
using Geometry;
using Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SkiaSharp;

using static ActionEngine.ActionEngineUtil;


namespace ActionEngine
{
	//*-------------------------------------------------------------------------*
	//*	ActionCollectionBase																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ActionItemBase Items.
	/// </summary>
	public abstract class ActionCollectionBase<TAction, TCollection> :
		List<TAction>
		where TAction : ActionItemBase<TAction, TCollection>
		where TCollection : ActionCollectionBase<TAction, TCollection>, new()
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************

		//*-----------------------------------------------------------------------*
		//* CreateNew																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create and return a reference to a new collection of the defined type.
		/// </summary>
		/// <returns>
		/// Reference to a new collection of the defined concrete type.
		/// </returns>
		public static TCollection CreateNew() { return new TCollection(); }
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Parent">Parent</see>.
		/// </summary>
		private TAction mParent = null;
		/// <summary>
		/// Get/Set a reference to the batch file to which this sequence belongs.
		/// </summary>
		[JsonIgnore]
		public TAction Parent
		{
			get { return mParent; }
			set
			{
				//	NOTE: This is only stupid because Newtonsoft JSON ...
				//	bypasses an overridden Add(Item) method.
				mParent = value;
				foreach(TAction actionItem in this)
				{
					actionItem.Parent = (TCollection)this;
				}
			}
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ActionItemBase																													*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an individual action that will be taken.
	/// </summary>
	public abstract class ActionItemBase<TAction, TCollection>
		where TAction : ActionItemBase<TAction, TCollection>
		where TCollection : ActionCollectionBase<TAction, TCollection>, new()
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		/// <summary>
		/// Public properties of this class.
		/// </summary>
		private static List<PropertyInfo> mPublicProperties =
			new List<PropertyInfo>();
		/// <summary>
		/// Working path monitor.
		/// </summary>
		private static string mWorkingPathLast = "";

		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//* CheckElements																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Check all of the specified elements and return a value indicating
		/// whether the masked items were all valid.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action item for which the elements are being
		/// tested.
		/// </param>
		/// <param name="element">
		/// Bitmasked action element flags to require on this action.
		/// </param>
		/// <param name="includeInherited">
		/// Optional value indicating whether to include inherited values,
		/// if true, or to include only local values, if false. Default = true.
		/// </param>
		/// <param name="quiet">
		/// Optional value indicating whether to run the operation in quiet mode.
		/// If true, no warnings or errors will be sent. Default = false.
		/// </param>
		/// <returns>
		/// Value indicating whether the check was successful.
		/// </returns>
		/// <remarks>
		/// Error messages are printed to the console when one or more of the
		/// specified elements are not found.
		/// </remarks>
		protected static bool CheckElements(TAction item,
			ActionElementEnum element, bool includeInherited = true,
			bool quiet = false)
		{
			int count = 0;
			DirectoryInfo dir = null;
			FileInfo file = null;
			int index = 0;
			bool result = true;
			bool sendMessages = !quiet;
			string vBase = "";
			float vCount = 0f;
			List<FileInfo> vDataFiles = null;
			DateTime vDateTime = DateTime.MinValue;
			string vInputFilename = "";
			List<FileInfo> vInputFiles = null;
			int vInt = 0;
			string vPattern = "";
			StartEndItem vRange = null;
			string vText = "";
			string workingFolder = "";

			if(item != null && element != ActionElementEnum.None)
			{
				if((element & ActionElementEnum.Action) != ActionElementEnum.None)
				{
					if(ActionIsNone(item.Action))
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: No action specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Base) != ActionElementEnum.None)
				{
					vBase = (includeInherited ? item.Base : item.mBase);
					if(vBase.Length == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Base is required in this action.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Count) != ActionElementEnum.None)
				{
					vCount = (includeInherited ? item.Count : item.mCount);
					if(vCount == 0f)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Count is required for this action.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.DateTimeValue) !=
					ActionElementEnum.None)
				{
					vDateTime = (includeInherited ?
						item.DateTimeValue : item.mDateTimeValue);
					if(vDateTime == DateTime.MinValue)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: DateTime was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Digits) != ActionElementEnum.None)
				{
					vInt = (includeInherited ? item.Digits : item.mDigits);
					if(vInt == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: A value is required for Digits.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.DataFilename) !=
					ActionElementEnum.None)
				{
					//	In this version, when DataFilename is expressed, only files
					//	are specified in the DataFiles collection.
					vDataFiles = (includeInherited ?
						item.DataFiles : item.mDataFiles);
					count = vDataFiles.Count;
					for(index = 0; index < count; index++)
					{
						file = vDataFiles[index];
						if((file.Attributes & FileAttributes.Directory) !=
							(FileAttributes)0)
						{
							//	This item is a directory. Remove it.
							vDataFiles.RemoveAt(index);
							index--;  //	Deindex.
							count--;  //	Decount.
						}
					}
					if(vDataFiles.Count == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: No data files were specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.InputFilename) !=
					ActionElementEnum.None)
				{
					//	In this version, when InputFilename is expressed, only files
					//	are specified in the InputFiles collection.
					vInputFiles = (includeInherited ?
						item.InputFiles : item.mInputFiles);
					count = vInputFiles.Count;
					for(index = 0; index < count; index++)
					{
						file = vInputFiles[index];
						if((file.Attributes & FileAttributes.Directory) !=
							(FileAttributes)0)
						{
							//	This item is a directory. Remove it.
							vInputFiles.RemoveAt(index);
							index--;  //	Deindex.
							count--;  //	Decount.
						}
					}
					if(vInputFiles.Count == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Input files were not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.InputFolderName) !=
					ActionElementEnum.None)
				{
					//	In this version, when InputFoldername is expressed, only
					//	folders are specified in the InputFiles collection.
					vInputFiles = (includeInherited ?
						item.InputFiles : item.mInputFiles);
					count = vInputFiles.Count;
					for(index = 0; index < count; index++)
					{
						file = vInputFiles[index];
						if((file.Attributes & FileAttributes.Directory) ==
							(FileAttributes)0)
						{
							//	This item is a file. Remove it.
							vInputFiles.RemoveAt(index);
							index--;  //	Deindex.
							count--;  //	Decount.
						}
					}
					if(vInputFiles.Count > 0)
					{
						//	Input folders are present.
						item.InputDir = new DirectoryInfo(vInputFiles[0].FullName);
					}
					if(vInputFiles.Count == 0)
					{
						//	If no files are specified, use the working folder.
						workingFolder = GetPropertyByName(item, nameof(WorkingPath));
						if(workingFolder.Length > 0 && Directory.Exists(workingFolder))
						{
							file = new FileInfo(workingFolder);
							vInputFiles.Add(file);
							item.InputDir = new DirectoryInfo(workingFolder);
						}
					}
					if(vInputFiles.Count == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Input folders were not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Inputs) != ActionElementEnum.None)
				{
					//	Multiple input files.
					vInputFiles = (includeInherited ?
						item.InputFiles : item.mInputFiles);
					if(vInputFiles.Count == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: No input files specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.OutputFilename) !=
					ActionElementEnum.None)
				{
					vInputFiles = (includeInherited ?
						item.InputFiles : item.mInputFiles);
					vInputFilename = (includeInherited ?
						item.InputFilename : item.mInputFilename);
					if(item.OutputFile == null &&
						vInputFiles?.Count == 1 &&
						((element & ActionElementEnum.InputFilename) !=
						ActionElementEnum.None))
					{
						//	If the input and output are both expected, and
						//	only the input was supplied, then use the
						//	input file as the output.
						item.OutputFilename = vInputFilename;
						item.OutputFile = vInputFiles[0];
					}
					if(item.OutputFile == null)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Output filename was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
					else
					{
						dir = new DirectoryInfo(
							Path.GetDirectoryName(item.OutputFile.FullName));
						if(!dir.Exists)
						{
							try
							{
								dir.Create();
							}
							catch
							{
								if(sendMessages)
								{
									Trace.WriteLine(
										" Error: Could not create output directory.",
										$"{MessageImportanceEnum.Err}");
								}
							}
						}
					}
				}
				if((element & ActionElementEnum.OutputFoldername) !=
					ActionElementEnum.None)
				{
					if(item.OutputDir == null)
					{
						//	If no output folder was specified, use the working folder.
						workingFolder = GetPropertyByName(item, nameof(WorkingPath));
						if(workingFolder.Length > 0 && Directory.Exists(workingFolder))
						{
							dir = new DirectoryInfo(workingFolder);
							item.OutputDir = dir;
						}
					}
					if(item.OutputDir == null)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Output folder name was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.OutputName) != ActionElementEnum.None)
				{
					//	In this version, output can be either a file or a folder.
					if(item.OutputDir == null && item.OutputFile == null)
					{
						if(sendMessages)
						{
							Trace.WriteLine(
								" Error: Output name was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Pattern) != ActionElementEnum.None)
				{
					vPattern = (includeInherited ? item.Pattern : item.mPattern);
					if(vPattern.Length == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Pattern was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Range) != ActionElementEnum.None)
				{
					//	In this version, the range can be a single ended specification.
					vRange = (includeInherited ? item.Range : item.mRange);
					if(vRange.StartValue.Length == 0 &&
						vRange.EndValue.Length == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Range was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.SourceFolderName) !=
					ActionElementEnum.None)
				{
					//	This version only has one source folder.
					item.SourceDir = null;
					if(item.SourceFolderName.Length > 0)
					{
						//	Source folder name has been specified.
						item.SourceDir = new DirectoryInfo(
							AbsolutePath(item.WorkingPath, item.SourceFolderName));
						if(!item.SourceDir.Exists)
						{
							//	If the folder doesn't exist, release it.
							item.SourceDir = null;
						}
					}
					if(item.SourceDir == null)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Source folder was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.Text) != ActionElementEnum.None)
				{
					vText = (includeInherited ? item.Text : item.mText);
					if(vText.Length == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Text parameter was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
				}
				if((element & ActionElementEnum.WorkingPath) !=
					ActionElementEnum.None)
				{
					//	Working path can always be inherited.
					if(item.WorkingPath.Length == 0)
					{
						if(sendMessages)
						{
							Trace.WriteLine(" Error: Working path was not specified.",
								$"{MessageImportanceEnum.Err}");
						}
						result = false;
					}
					else
					{
						dir = new DirectoryInfo(
							GetPropertyByName(item, nameof(WorkingPath)));
						if(!dir.Exists)
						{
							if(sendMessages)
							{
								Trace.WriteLine(" Error: Working path does not exist.",
									$"{MessageImportanceEnum.Err}");
							}
							result = false;
						}
						else if((dir.Attributes & FileAttributes.Directory) !=
							FileAttributes.Directory)
						{
							if(sendMessages)
							{
								Trace.WriteLine(
									" Error: A file was specified as the working directory.",
									$"{MessageImportanceEnum.Err}");
							}
							result = false;
						}
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ClearDataFiles																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear the local DataFiles collection for the immediate parent item.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item calling for the DataFiles collection to
		/// be cleared.
		/// </param>
		protected static void ClearDataFiles(TAction item)
		{
			if(item != null)
			{
				if(item.Parent?.Parent != null)
				{
					item.Parent.Parent.mDataFiles.Clear();
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ClearInputFiles																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear the local InputFiles collection for the immediate parent item.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item calling for the InputFiles collection to
		/// be cleared.
		/// </param>
		protected static void ClearInputFiles(TAction item)
		{
			if(item != null)
			{
				if(item?.Parent.Parent != null)
				{
					item.Parent.Parent.mInputFiles.Clear();
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* DeserializeFile																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Deserialize the contents of the supplied file.
		/// </summary>
		/// <param name="content">
		/// The JSON file content to deserialize.
		/// </param>
		/// <returns>
		/// The deserialized top action from the file.
		/// </returns>
		protected virtual TAction DeserializeFile(string content)
		{
			TAction result = null;

			if(content?.Length > 0)
			{
				try
				{
					result = JsonConvert.DeserializeObject<TAction>(content);
				}
				catch(Exception ex)
				{
					Trace.WriteLine($"Error deserializing: {ex.Message}",
						MessageImportanceEnum.Err.ToString());
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* DrawImage																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Draw the image specified by ImageName onto the working image at the
		/// location specified by user properties Left and Top.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item describing the image to draw and the
		/// location at which to draw it.
		/// </param>
		protected static void DrawImage(TAction item)
		{
			int height = 0;
			SKBitmap bitmap = null;
			BitmapInfoItem sourceImage = null;
			SKRect sourceRect = SKRect.Empty;
			SKRect targetRect = SKRect.Empty;
			int width = 0;
			int x = 0;
			int y = 0;

			if(item != null && WorkingImage?.Bitmap != null)
			{
				sourceImage = Images.FirstOrDefault(x =>
					x.Name == GetPropertyByName(item, "ImageName"));
				if(sourceImage != null)
				{
					Trace.WriteLine($" {sourceImage.Name}",
						$"{MessageImportanceEnum.Info}");
					bitmap = sourceImage.Bitmap;
					sourceRect =
						new SKRect(0, 0, bitmap.Width, bitmap.Height);
					x = ToInt(GetPropertyByName(item, "Left"));
					y = ToInt(GetPropertyByName(item, "Top"));
					width = ToInt(GetPropertyByName(item, "Width"));
					height = ToInt(GetPropertyByName(item, "Height"));
					if(width == 0)
					{
						width = bitmap.Width;
					}
					if(height == 0)
					{
						height = bitmap.Height;
					}
					targetRect = new SKRect(x, y, width, height);
					DrawBitmap(bitmap, WorkingImage.Bitmap, sourceRect, targetRect);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* FileOpenImage																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Open the image specified in InputFilename.
		/// </summary>
		/// <param name="item">
		/// Reference to the item from which the item will be opened.
		/// </param>
		/// <remarks>
		/// This method works upon the currently open file.
		/// </remarks>
		protected static void FileOpenImage(TAction item)
		{
			SKBitmap bitmap = null;
			SKBitmap bitmapA = null;
			FileInfo file = null;
			string imageFilename = "";
			BitmapInfoItem imageInfo = null;
			string imageName = "";
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

			if(item != null)
			{
				imageFilename = GetPropertyByName(item, "ImageFilename");
				if(imageFilename.Length > 0)
				{
					file = new FileInfo(AbsolutePath(item.WorkingPath, imageFilename));
				}
				else
				{
					file = GetCurrentFile(item);
				}
				if(file != null && file.Exists)
				{
					imageName = GetPropertyByName(item, "ImageName");
					if(imageName.Length == 0)
					{
						//imageName = $"Image{PadLeft("0", Images.Count, 5)}";
						imageName = file.Name;
					}
					Trace.WriteLine($" {imageName}",
						$"{MessageImportanceEnum.Info}");
					bitmap = SKBitmap.Decode(file.FullName);
					bitmapA = new SKBitmap(bitmap.Width, bitmap.Height,
						SKColorType.Rgba8888, SKAlphaType.Premul);
					DrawBitmap(bitmap, bitmapA, SKPoint.Empty);
					bitmap.Dispose();
					bitmap = bitmapA;
					imageInfo = Images.FirstOrDefault(x => x.Name == imageName);
					if(imageInfo == null)
					{
						imageInfo = new BitmapInfoItem()
						{
							Name = imageName
						};
						Images.Add(imageInfo);
					}
					imageInfo.Bitmap = bitmap;
					if(ToBool(GetPropertyByName(item, "IsWorkingImage"), true))
					{
						WorkingImage = imageInfo;
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* FileOverlayImage																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Open each image from the range and place the image specified in
		/// InputFilename at the options specified by Left, Top, Width, and Height.
		/// </summary>
		/// <param name="item">
		/// Reference to the item from which the item will be opened.
		/// </param>
		protected static void FileOverlayImage(TAction item)
		{
			SKRect area = SKRect.Empty;
			bool bContinue = true;
			bool bRange = false;
			byte[] bytes = null;
			int height = 0;
			int left = 0;
			SKBitmap maskBitmap = null;
			FileInfo maskFile = null;
			List<string> names = null;
			ActionOptionItem optionHeight = null;
			ActionOptionItem optionLeft = null;
			ActionOptionItem optionMask = null;
			ActionOptionItem optionTop = null;
			ActionOptionItem optionWidth = null;
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
			SKBitmap sourceBitmap = null;
			FileInfo sourceFile = null;
			List<string> sourceFilenames = new List<string>();
			int top = 0;
			int width = 0;

			if(item != null)
			{
				optionMask = GetOptionByName(item, "MaskFilename");
				if(optionMask != null)
				{
					maskFile = new FileInfo(optionMask.Value);
					if(!maskFile.Exists)
					{
						maskFile = null;
					}
				}
				optionLeft = GetOptionByName(item, "Left");
				optionTop = GetOptionByName(item, "Top");
				optionWidth = GetOptionByName(item, "Width");
				optionHeight = GetOptionByName(item, "Height");
				if(item.InputFiles.Count > 0)
				{
					bContinue = true;
				}
				else
				{
					bRange = bContinue = CheckElements(item,
						ActionElementEnum.InputFolderName |
						ActionElementEnum.Range);
				}
				if(bContinue &&
					maskFile != null &&
					optionLeft != null && optionTop != null &&
					optionWidth != null && optionHeight != null)
				{
					maskBitmap = SKBitmap.Decode(maskFile.FullName);
					left = ToInt(optionLeft.Value);
					top = ToInt(optionTop.Value);
					width = ToInt(optionWidth.Value);
					height = ToInt(optionHeight.Value);
					if(bRange)
					{
						//	Range-based.
						names = EnumerateRange(item.Range, item.Digits);
						foreach(string nameItem in names)
						{
							sourceFilenames.Add(
								Path.Combine(item.InputDir.FullName, nameItem));
						}
					}
					else
					{
						sourceFilenames = new List<string>();
						foreach(FileInfo fileInfoItem in item.InputFiles)
						{
							sourceFilenames.Add(fileInfoItem.FullName);
						}
					}
					if(sourceFilenames.Count > 0)
					{
						//	Source filenames were generated.
						foreach(string sourceFilenameItem in sourceFilenames)
						{
							sourceFile = new FileInfo(sourceFilenameItem);
							if(sourceFile.Exists)
							{
								bytes = File.ReadAllBytes(sourceFile.FullName);
								using(var ms = new MemoryStream(bytes))
								{
									sourceBitmap = SKBitmap.Decode(ms);
								}
								area =
									new SKRect(0, 0, sourceBitmap.Width, sourceBitmap.Height);
								DrawBitmap(maskBitmap, sourceBitmap, area);
								try
								{
									SaveBitmap(sourceBitmap, sourceFile.FullName);
									Trace.WriteLine(
										$" File saved: {Path.GetFileName(sourceFile.FullName)}",
										$"{MessageImportanceEnum.Info}");
								}
								catch(Exception ex)
								{
									Trace.WriteLine($"Error: {ex.Message}",
										$"{MessageImportanceEnum.Err}");
								}
								Trace.WriteLine($" {Path.GetFileName(sourceFilenameItem)}",
									$"{MessageImportanceEnum.Info}");
							}
						}
					}
					else
					{
						Trace.WriteLine(
							" Error: Source filenames could not be enumerated from the " +
							"given range.",
							$"{MessageImportanceEnum.Err}");
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* FileSaveImage																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Save the working image to the specified OutputFilename.
		/// </summary>
		/// <param name="item">
		/// Reference to the item from which the item will be opened.
		/// </param>
		protected static void FileSaveImage(TAction item)
		{
			FileInfo file = null;

			if(item != null && WorkingImage != null)
			{
				if(item.OutputFile != null)
				{
					file = item.OutputFile;
				}
				else if(item.CurrentFile != null && item.OutputDir != null)
				{
					file = new FileInfo(
						Path.Combine(item.OutputDir.FullName, item.CurrentFile.Name));
				}
				if(file != null)
				{
					AssureFolder(file.Directory.FullName, true, quiet: true);
					try
					{
						SaveBitmap(WorkingImage.Bitmap, file.FullName);
						Trace.WriteLine($" File saved: {file.Name}",
							$"{MessageImportanceEnum.Info}");
					}
					catch(Exception ex)
					{
						Trace.WriteLine($"Error: {ex.Message}",
							$"{MessageImportanceEnum.Err}");
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ForEachFile																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the Actions collection of the presented object through all of the
		/// files in this item's InputFiles collection using the CurrentFile
		/// property for each one.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action item representing the loop base.
		/// </param>
		protected static async void ForEachFile(TAction item)
		{
			if(item != null)
			{
				foreach(FileInfo fileItem in item.InputFiles)
				{
					item.CurrentFile = fileItem;
					if(item.Actions.Count > 0)
					{
						await RunActions(item.Actions);
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		////*-----------------------------------------------------------------------*
		////* GetActions																														*
		////*-----------------------------------------------------------------------*
		///// <summary>
		///// Return the list of implemented actions.
		///// </summary>
		///// <returns>
		///// Reference to the list of implemented actions.
		///// </returns>
		//protected abstract List<ActionItemBase> GetActions();
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetCurrentFile																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the current file.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action for which the current file will be
		/// retrieved.
		/// </param>
		/// <returns>
		/// Reference to the current file in focus, if found. Otherwise, null.
		/// </returns>
		/// <remarks>
		/// If a file has been placed in focus by a loop or other method, that
		/// object will be returned from the CurrentFile property. Otherwise, the
		/// first item in InputFiles collection is returned.
		/// </remarks>
		protected static FileInfo GetCurrentFile(TAction item)
		{
			FileInfo result = null;

			if(item != null)
			{
				//	An item has been provided.
				//	Test order.
				//	-	Local current file.
				//	- Local first item.
				//	- Parent current file.
				//	- Parent first item.
				if(item.mCurrentFile != null)
				{
					result = item.mCurrentFile;
				}
				else if(item.mInputFiles.Count > 0)
				{
					result = item.mInputFiles[0];
				}
				else
				{
					result = item.CurrentFile;
					if(result == null && item.InputFiles.Count > 0)
					{
						result = item.InputFiles[0];
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetPrecision																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the precision specified in the supplied item's Precision user
		/// property.
		/// </summary>
		/// <param name="item">
		/// Reference to the item containing the properties to check.
		/// </param>
		/// <returns>
		/// Decimal precision to use on the current item.
		/// </returns>
		protected static int GetPrecision(TAction item)
		{
			int precision = 3;
			NameValueItem property = null;

			if(item != null)
			{
				property = item.Properties.FirstOrDefault(x =>
					x.Name.ToLower() == "precision");
				if(property != null)
				{
					precision = ToInt(property.Value);
					Trace.WriteLine($" Decimal Precision: {precision}",
						$"{MessageImportanceEnum.Info}");
				}
				else
				{
					Trace.WriteLine(" Precision property not specified. " +
						$"Defaulting to {precision}.",
						$"{MessageImportanceEnum.Info}");
				}
			}
			return precision;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* IdentifyDataFiles																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Identify the data files at the current level for the specified item.
		/// </summary>
		/// <param name="item">
		/// Reference to the item being fulfilled.
		/// </param>
		/// <remarks>
		/// <para>
		/// When this method is called, make sure that the InitializeLevels
		/// method has already been called.
		/// </para>
		/// <para>
		/// Only call CheckElements after first calling IdentifyFiles. That
		/// method relies on the file objects in this version.
		/// </para>
		/// <para>
		/// In this version, only the local DataFiles group is resolved. If you
		/// want to use a globally resolvable template, implement a user property
		/// containing that template name and set the DataFilename, etc., property
		/// at the site with a reference to that custom property.
		/// </para>
		/// </remarks>
		protected static void IdentifyDataFiles(TAction item)
		{
			DirectoryInfo dir = null;
			FileInfo file = null;
			string filename = "";
			bool result = true;

			if(item != null && item.mDataNames.Count > 0)
			{
				//	Working path.
				if(item.WorkingPath.Length > 0)
				{
					dir = new DirectoryInfo(
						GetPropertyByName(item, nameof(WorkingPath)));
					if(!dir.Exists)
					{
						Trace.WriteLine(" Error: Working path does not exist.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
					else if((dir.Attributes & FileAttributes.Directory) !=
						FileAttributes.Directory)
					{
						Trace.WriteLine(
							" Error: A file was specified as the working directory.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
				}

				//	Input.
				item.mDataDir = null;
				item.mDataFiles.Clear();

				if(result)
				{
					if(item.mDataNames.Count > 0)
					{
						//	Data files are present.
						foreach(string filenameItem in item.mDataNames)
						{
							filename = AbsolutePath(
								GetPropertyByName(item, nameof(WorkingPath)),
								NormalizeValue(item, filenameItem));
							if(filename.Length > 0)
							{
								//	A filename has been retrieved.
								//	Check for wildcards and resolve variables.
								item.mDataFiles.AddRange(
									ResolveFilename(filename, false));
							}
						}
						if(item.mDataFiles.Count > 0)
						{
							file = item.mDataFiles[0];
							if((file.Attributes & FileAttributes.Directory) ==
								(FileAttributes)0)
							{
								//	This item is a file.
								item.mDataDir = new DirectoryInfo(file.Directory.FullName);
							}
							else
							{
								//	This item is a directory.
								item.mDataDir = new DirectoryInfo(file.FullName);
							}
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* IdentifyInputFiles																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Identify the input and output files and directories at the current
		/// level for the specified item.
		/// </summary>
		/// <param name="item">
		/// Reference to the item being fulfilled.
		/// </param>
		/// <remarks>
		/// <para>
		/// When this method is called, make sure that the InitializeLevels
		/// method has already been called.
		/// </para>
		/// <para>
		/// Only call CheckElements after first calling IdentifyFiles. That
		/// method relies on the file objects in this version.
		/// </para>
		/// <para>
		/// In this version, only the local InputFiles group is resolved. If you
		/// want to use a globally resolvable template, implement a user property
		/// containing that template name and set the InputFilename, etc., property
		/// at the site with a reference to that custom property.
		/// </para>
		/// </remarks>
		protected static void IdentifyInputFiles(TAction item)
		{
			DirectoryInfo dir = null;
			FileInfo file = null;
			string filename = "";
			bool result = true;

			if(item != null && item.mInputNames.Count > 0)
			{
				//	Working path.
				if(item.WorkingPath.Length > 0)
				{
					dir = new DirectoryInfo(
						GetPropertyByName(item, nameof(WorkingPath)));
					if(!dir.Exists)
					{
						Trace.WriteLine(" Error: Working path does not exist.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
					else if((dir.Attributes & FileAttributes.Directory) !=
						FileAttributes.Directory)
					{
						Trace.WriteLine(
							" Error: A file was specified as the working directory.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
				}

				//	Input.
				item.mInputDir = null;
				item.mInputFiles.Clear();

				if(result)
				{
					if(item.mInputNames.Count > 0)
					{
						//	Input files are present.
						foreach(string filenameItem in item.mInputNames)
						{
							filename = AbsolutePath(
								GetPropertyByName(item, nameof(WorkingPath)),
								NormalizeValue(item, filenameItem));
							if(filename.Length > 0)
							{
								//	A filename has been retrieved.
								//	Check for wildcards and resolve variables.
								item.mInputFiles.AddRange(
									ResolveFilename(filename, false));
							}
						}
						if(item.mInputFiles.Count > 0)
						{
							file = item.mInputFiles[0];
							if((file.Attributes & FileAttributes.Directory) ==
								(FileAttributes)0)
							{
								//	This item is a file.
								item.mInputDir = new DirectoryInfo(file.Directory.FullName);
							}
							else
							{
								//	This item is a directory.
								item.mInputDir = new DirectoryInfo(file.FullName);
							}
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* IdentifyOutputFiles																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Identify the output files and directories at the current
		/// level for the specified item.
		/// </summary>
		/// <param name="item">
		/// Reference to the item being fulfilled.
		/// </param>
		/// <remarks>
		/// <para>
		/// When this method is called, make sure that the InitializeLevels
		/// method has already been called.
		/// </para>
		/// <para>
		/// Only call CheckElements after first calling IdentifyFiles. That
		/// method relies on the file objects in this version.
		/// </para>
		/// </remarks>
		protected static void IdentifyOutputFiles(TAction item)
		{
			DirectoryInfo dir = null;
			FileInfo file = null;
			string filename = "";
			List<FileInfo> files = null;
			bool result = true;

			if(item != null)
			{
				//	Working path.
				if(item.WorkingPath.Length > 0)
				{
					dir = new DirectoryInfo(
						GetPropertyByName(item, nameof(WorkingPath)));
					if(!dir.Exists)
					{
						Trace.WriteLine(" Error: Working path does not exist.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
					else if((dir.Attributes & FileAttributes.Directory) !=
						FileAttributes.Directory)
					{
						Trace.WriteLine(
							" Error: A file was specified as the working directory.",
							$"{MessageImportanceEnum.Err}");
						result = false;
					}
				}

				//	Output.
				item.OutputDir = null;
				item.OutputFile = null;

				if(result)
				{
					if(item.OutputName?.Length > 0 && item.IsOutputLocal())
					{
						//	Output folder or file is present.
						files = new List<FileInfo>();
						filename = AbsolutePath(
							GetPropertyByName(item, nameof(WorkingPath)),
							GetPropertyByName(item, nameof(OutputName)));
						files.AddRange(ResolveFilename(filename, true));
						if(files.Count > 0)
						{
							file = files[0];
							if((!file.Exists && file.Extension.Length > 0) ||
								(file.Exists &&
								((file.Attributes & FileAttributes.Directory) ==
								(FileAttributes)0)))
							{
								//	This item is a file.
								item.OutputFile = file;
								item.OutputDir = new DirectoryInfo(file.Directory.FullName);
							}
							else
							{
								//	This item is a directory.
								item.OutputDir = new DirectoryInfo(file.FullName);
							}
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* If																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run one or more sets of actions if their conditions are true.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item for which this action is being
		/// called.
		/// </param>
		protected static async void If(TAction item)
		{
			bool bMatch = false;
			ConditionCollection conditions = null;
			bool conditionResult = false;
			ExpressionContext context;
			IDynamicExpression dynCondition = null;

			if(item != null)
			{
				context = new ExpressionContext();
				//// Allow the expression to use all static public methods of
				//// System.Math.
				//context.Imports.AddType(typeof(Math));
				context.Variables["CurrentFilename"] = item.CurrentFile.Name;
				context.Variables["CurrentFileNumber"] =
					GetIndexValue(item.CurrentFile.Name);

				foreach(TAction actionItem in item.Actions)
				{
					if(!actionItem.Options.Exists(x => x.Name.ToLower() == "mute"))
					{
						conditions = GetConditions(actionItem);
						bMatch = true;
						foreach(ConditionItem conditionItem in conditions)
						{
							dynCondition = context.CompileDynamic(conditionItem.Condition);
							conditionResult = (bool)dynCondition.Evaluate();
							if(!conditionResult)
							{
								bMatch = false;
								break;
							}
						}
						if(bMatch)
						{
							//	This item evaluates to true. Run its actions.
							if(actionItem.Actions.Count > 0)
							{
								await RunActions(actionItem.Actions);
							}
						}
					}
					else
					{
						Trace.WriteLine($"Action {actionItem.Action} is muted...",
							$"{MessageImportanceEnum.Info}");
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ImageBackground																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the background color or image on the working image.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action item for which this action is being
		/// called.
		/// </param>
		protected static void ImageBackground(TAction item)
		{
			string backgroundColor = "";
			SKBitmap backgroundBitmap = null;
			string backgroundFilename = "";
			SKBitmap bitmap = null;
			SKRect rectSource = SKRect.Empty;
			SKRect rectTarget = SKRect.Empty;
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
			int height = 0;
			int width = 0;

			if(item != null && WorkingImage?.Bitmap != null)
			{
				//	An item was presented and a working image is present.
				width = WorkingImage.Bitmap.Width;
				height = WorkingImage.Bitmap.Height;
				rectTarget = new SKRect(0, 0, width, height);
				backgroundBitmap = new SKBitmap(width, height);

				//	Set the color first.
				backgroundColor = GetPropertyByName(item, "BackgroundColor");
				if(backgroundColor.Length > 0)
				{
					using(SKCanvas canvas = new SKCanvas(WorkingImage.Bitmap))
					{
						using(SKPaint paint = new SKPaint())
						{
							paint.Color = SKColor.Parse(backgroundColor);
							paint.Style = SKPaintStyle.Fill;
							canvas.DrawRect(rectTarget, paint);
						}
					}
				}
				//	Check for image.
				backgroundFilename = GetPropertyByName(item, "BackgroundImage");
				if(backgroundFilename.Length > 0)
				{
					bitmap = SKBitmap.Decode(
						AbsolutePath(item.WorkingPath, backgroundFilename));
					DrawBitmap(bitmap, WorkingImage.Bitmap, rectSource, rectTarget);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ImagesClear																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear the contents of the Images collection.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action item for which this action is being
		/// called.
		/// </param>
		/// <remarks>
		/// This method also clears the WorkingImage property.
		/// </remarks>
		protected static void ImagesClear(TAction item)
		{
			if(item != null)
			{
				Images.Clear();
				WorkingImage = null;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InitializeFilenames																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Initialize values for working at this and child levels.
		/// </summary>
		/// <param name="item">
		/// Reference to the item to be intialized.
		/// </param>
		/// <remarks>
		///	<para>When preparing the object for use:</para>
		///	<list type="bullet">
		///	<item>All input files at a level should be read from a single
		///	reference source. Check for an inputs collection.</item>
		///	<item>If blank, check for filename and add to inputs
		///	collection.</item>
		///	<item>If blank, check for foldername and add to inputs
		///	collection.</item>
		///	<item>All output files at a level should be written from a
		///	single reference source. Check for the Output.</item>
		///	<item>If blank, check for the output filename.</item>
		///	<item>If blank, check for the output foldername.</item>
		///	</list>
		///	<para>
		///	In this version, the conversion is made on every action at every level.
		///	</para>
		/// </remarks>
		protected static void InitializeFilenames(TAction item)
		{
			if(item != null)
			{
				//	Input filenames.
				if(item.mInputNames.Count == 0 &&
					(item.mInputFilename?.Length > 0 ||
					item.mInputFolderName?.Length > 0))
				{
					//	The input names collection was not specified, but either a
					//	filename or foldername were provided.
					if(item.mInputFilename?.Length > 0)
					{
						//	An input filename was provided at this level.
						item.mInputNames.AddRange(
							ResolveWildcards(
								GetPropertyByName(item,
									nameof(WorkingPath)), item.mInputFilename));
						//item.mInputNames.Add(item.mInputFilename);
						item.mInputFilename = "";
					}
					if(item.mInputFolderName?.Length > 0)
					{
						//	An input foldername was provided at this level.
						item.mInputNames.Add(item.mInputFolderName);
						item.mInputFiles.Add(new FileInfo(item.mInputFolderName));
						//	DEP20240225.1102 - I don't remember the original reason
						//	for clearing this variable. Its raw value is needed in
						//	directory deletion routine.
						//	Be aware this may need to be uncommented.
						//item.mInputFolderName = "";
					}
				}
				//	Data filenames.
				if(item.mDataNames.Count == 0 &&
					item.mDataFilename?.Length > 0)
				{
					//	The data names collection was not specified, but either a
					//	filename or foldername were provided.
					if(item.mDataFilename?.Length > 0)
					{
						//	An data filename was provided at this level.
						item.mDataNames.AddRange(
							ResolveWildcards(
								GetPropertyByName(item,
									nameof(WorkingPath)), item.mDataFilename));
						item.mDataFilename = "";
					}
				}
				//	Output filenames.
				if((item.mOutputName == null || item.mOutputName.Length == 0) &&
					(item.mOutputFilename?.Length > 0 ||
					item.mOutputFolderName?.Length > 0))
				{
					//	The output name was not specified and either an output filename
					//	or output foldername are present.
					if(item.mOutputFilename?.Length > 0)
					{
						//	An output filename was provided at this level.
						item.mOutputName = item.mOutputFilename;
					}
					else if(item.mOutputFolderName?.Length > 0)
					{
						//	An output folder name was provided at this level.
						item.mOutputName = item.mOutputFolderName;
					}
				}
				//	In this version, all child items are processed as they are
				//	encountered.
				////	Process all child levels.
				//foreach(FileActionItem actionItem in item.mActions)
				//{
				//	InitializeFilenames(actionItem);
				//}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InitializeParent																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Initialize the Parent property in all of the decendants of the
		/// specified item.
		/// </summary>
		/// <param name="action">
		/// Reference to an action derivative.
		/// </param>
		protected static void InitializeParent(TAction action)
		{
			TCollection actions = null;

			if(action != null)
			{
				actions = action.Actions;
				actions.Parent = action;
				foreach(TAction actionItem in action.Actions)
				{
					actionItem.Parent = actions;
					InitializeParent(actionItem);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InitializeProperties																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Initialize the public properties list of this class so they can be
		/// used repeatedly with minimal overhead.
		/// </summary>
		protected static void InitializeProperties()
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			PropertyInfo[] properties = null;

			if(mPublicProperties.Count == 0)
			{
				//	Only initialize once.
				properties =
					typeof(TAction).GetProperties(bindingFlags);
				foreach(PropertyInfo propertyInfoItem in properties)
				{
					mPublicProperties.Add(propertyInfoItem);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* NormalizeValue																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Normalize the filename, using the values of any local properties
		/// necessary.
		/// </summary>
		/// <param name="item">
		/// Reference to the item from which variables will be resolved.
		/// </param>
		/// <param name="value">
		/// Value to normalize.
		/// </param>
		/// <returns>
		/// Fully normalized version of the provided filename.
		/// </returns>
		protected static string NormalizeValue(TAction item,
			string value)
		{
			MatchCollection matches = null;
			List<NameValueItem> replacements = new List<NameValueItem>();
			string result = "";

			if(value?.Length > 0)
			{
				result = value;
				matches = Regex.Matches(result, ResourceMain.rxEmbeddedFieldName);
				if(matches.Count > 0)
				{
					foreach(Match matchItem in matches)
					{
						if(!replacements.Exists(x =>
							x.Name == GetValue(matchItem, "field")))
						{
							replacements.Add(new NameValueItem()
							{
								Name = GetValue(matchItem, "field"),
								Value = GetPropertyByName(item, GetValue(matchItem, "name"))
							});
						}
					}
					foreach(NameValueItem replaceItem in replacements)
					{
						result = result.Replace(replaceItem.Name, replaceItem.Value);
					}
					//	Run at least one more time after having made replacements.
					result = NormalizeValue(item, result);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* OpenWorkingDocument																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Open the working file to allow multiple operations to be completed
		/// in the same session.
		/// </summary>
		protected virtual void OpenWorkingDocument()
		{
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* RunActions																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run all of the unmuted actions in the collection.
		/// </summary>
		/// <param name="actions">
		/// Reference to a collection of file actions to run.
		/// </param>
		/// <returns>
		/// Reference to the asynchronous task that was launched.
		/// </returns>
		protected static async Task RunActions(List<TAction> actions)
		{
			if(actions?.Count > 0)
			{
				foreach(TAction actionItem in actions)
				{
					if(!actionItem.Options.Exists(x => x.Name.ToLower() == "mute"))
					{
						await actionItem.Run();
						if(actionItem.Stop)
						{
							Trace.WriteLine("Batch stopped...",
								$"{MessageImportanceEnum.Info}");
							break;
						}
					}
					else
					{
						Trace.WriteLine(
							$"Action {actionItem.Action} is muted...",
							$"{MessageImportanceEnum.Info}");
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* RunCustomAction																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run a custom defined action.
		/// </summary>
		protected virtual void RunCustomAction()
		{

		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* RunSequence																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the specified sequence.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action that specifies the sequence to run.
		/// </param>
		/// <remarks>
		/// This method loads the Actions collection from the steps found in
		/// the referenced sequence.
		/// </remarks>
		protected static async void RunSequence(TAction item)
		{
			string name = "";
			SequenceItem<TAction> sequence = null;

			if(item != null)
			{
				name = GetPropertyByName(item, "SequenceName");
				if(name?.Length > 0)
				{
					Trace.WriteLine($" {name}", $"{MessageImportanceEnum.Info}");
					sequence =
						item.Sequences.FirstOrDefault(x => x.SequenceName == name);
					if(sequence != null)
					{
						//	Copy all of the actions.
						foreach(TAction actionItem in sequence.Actions)
						{
							item.Actions.Add(DeepCopy(actionItem));
						}
						item.Actions.Parent = item;
						//	Run each action.
						await RunActions(item.Actions);
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SaveWorkingDocument																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Save the working file to the specified output file.
		/// </summary>
		/// <param name="item">
		/// Reference to the item where the file action is defined.
		/// </param>
		protected static void SaveWorkingDocument(TAction item)
		{
			string content = "";

			//	TODO: Save working PowerPoint.

			//if(item != null)
			//{
			//	if(CheckElements(item, ActionElementEnum.OutputFilename))
			//	{
			//		if(item.WorkingPowerPoint?.Document != null)
			//		{
			//			content = item.WorkingSvg.Document.Html;
			//			File.WriteAllText(item.OutputFile.FullName, content);
			//			Trace.WriteLine($" SVG file written: {item.OutputFile.Name}",
			//				$"{MessageImportanceEnum.Info}");
			//		}
			//		else
			//		{
			//			Trace.WriteLine($" Error: No working SVG file is open...",
			//				$"{MessageImportanceEnum.Err}");
			//		}
			//	}
			//}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SetWorkingImage																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the working image to the one specified by the user property
		/// ImageName.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item specifying the file to be activated.
		/// </param>
		protected static void SetWorkingImage(TAction item)
		{
			BitmapInfoItem imageInfo = null;
			string imageName = "";

			WorkingImage = null;
			if(item != null)
			{
				imageName = GetPropertyByName(item, "ImageName");
				if(imageName?.Length > 0)
				{
					//	Image was specified.
					Trace.WriteLine($" Working Image: {imageName}",
						$"{MessageImportanceEnum.Info}");
					imageInfo = Images.FirstOrDefault(x => x.Name == imageName);
					if(imageInfo != null)
					{
						WorkingImage = imageInfo;
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SizeImage																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Scale the working image to a new size to the dimensions found in the
		/// Width and Height user properties.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item describing the new size to use on
		/// the working image.
		/// </param>
		protected static void SizeImage(TAction item)
		{
			SKBitmap bitmap = null;
			int height = 0;
			SKRect targetRect = SKRect.Empty;
			int width = 0;

			if(item != null && WorkingImage != null)
			{
				//	The item and the working image are both present.
				width = ToInt(GetPropertyByName(item, "Width"));
				height = ToInt(GetPropertyByName(item, "Height"));
				if(width > 0 && height > 0)
				{
					//	Dimensions were supplied.
					Trace.WriteLine($" {width}, {height}",
						$"{MessageImportanceEnum.Info}");
					targetRect = new SKRect(0, 0, width, height);
					bitmap = new SKBitmap((int)targetRect.Width, (int)targetRect.Height);
					DrawBitmap(WorkingImage.Bitmap, bitmap, targetRect);
					WorkingImage.Bitmap.Dispose();
					WorkingImage.Bitmap = bitmap;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* WriteLocalOutput																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Write the local output from the specified action.
		/// </summary>
		protected virtual void WriteLocalOutput()
		{

		}
		//*-----------------------------------------------------------------------*

		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	_Constructor																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create a new instance of the ActionItemBase Item.
		/// </summary>
		public ActionItemBase()
		{
			InitializeProperties();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Action																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Action">Action</see>.
		/// </summary>
		private string mAction = "None";
		/// <summary>
		/// Get/Set the action associated with this entry.
		/// </summary>
		/// <remarks>
		/// This property is non-inheritable.
		/// </remarks>
		public string Action
		{
			get { return mAction; }
			set { mAction = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Actions																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Actions">Actions</see>.
		/// </summary>
		private TCollection mActions = new TCollection();
		/// <summary>
		/// Get a reference to the collection of child SVG actions.
		/// </summary>
		/// <remarks>
		/// This property is non-inheritable.
		/// </remarks>
		public TCollection Actions
		{
			get { return mActions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Base																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Base">Base</see>.
		/// </summary>
		private string mBase = null;
		/// <summary>
		/// Get/Set the base number or filename pattern of the source or target
		/// files, depending upon the action.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public string Base
		{
			get
			{
				string result = mBase;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Base;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mBase = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Conditions																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Conditions">Conditions</see>.
		/// </summary>
		private ConditionCollection mConditions = new ConditionCollection();
		/// <summary>
		/// Get a reference to the collection of conditions assigned to this
		/// action.
		/// </summary>
		/// <remarks>
		/// This property is not inheritable. However, properties from parent
		/// levels are retrieved when calling the GetConditions function.
		/// </remarks>
		public ConditionCollection Conditions
		{
			get { return mConditions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ConfigFilename																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="ConfigFilename">ConfigFilename</see>.
		/// </summary>
		private string mConfigFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the configuration file for this
		/// action.
		/// </summary>
		/// <remarks>
		/// This property is non-inheritable.
		/// </remarks>
		public string ConfigFilename
		{
			get { return mConfigFilename; }
			set { mConfigFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Count																																	*
		//*-----------------------------------------------------------------------*
		private float mCount = float.MinValue;
		/// <summary>
		/// Get/Set the count associated with the current action.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public float Count
		{
			get
			{
				float result = mCount;

				if(result == float.MinValue)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Count;
					}
					else
					{
						result = 0f;
					}
				}
				return result;
			}
			set { mCount = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CurrentFile																														*
		//*-----------------------------------------------------------------------*
		private FileInfo mCurrentFile = null;
		/// <summary>
		/// Get/Set a reference to the current active file in-use.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		[JsonIgnore]
		public FileInfo CurrentFile
		{
			get
			{
				FileInfo result = mCurrentFile;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.CurrentFile;
					}
				}
				return result;
			}
			set { mCurrentFile = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DataDir																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="DataDir">DataDir</see>.
		/// </summary>
		private DirectoryInfo mDataDir = null;
		/// <summary>
		/// Get/Set the internal, calculated data directory.
		/// </summary>
		/// <remarks>
		/// This property is non-inerhitable.
		/// </remarks>
		[JsonIgnore]
		public DirectoryInfo DataDir
		{
			get { return mDataDir; }
			set { mDataDir = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DataFilename																													*
		//*-----------------------------------------------------------------------*
		private string mDataFilename = null;
		/// <summary>
		/// Get/Set the path and filename of the reference data file.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'DataFile'.</para>
		/// </remarks>
		public string DataFilename
		{
			get
			{
				string result = mDataFilename;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.DataFilename;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mDataFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DataFiles																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="DataFiles">DataFiles</see>.
		/// </summary>
		private List<FileInfo> mDataFiles = new List<FileInfo>();
		/// <summary>
		/// Get a reference to the collection of file information used as reference
		/// data in this session.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		[JsonIgnore]
		public List<FileInfo> DataFiles
		{
			get
			{
				List<FileInfo> result = mDataFiles;

				if(result.Count == 0)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.DataFiles;
					}
				}
				return result;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DataNames																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="DataNames">DataNames</see>.
		/// </summary>
		private List<string> mDataNames = new List<string>();
		/// <summary>
		/// Get a reference to the list of filenames or foldernames with
		/// or without wildcards. This parameter can be specified multiple times
		/// on the command line with different values to load multiple input files.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'DataFiles'.</para>
		/// </remarks>
		public List<string> DataNames
		{
			get
			{
				List<string> result = mDataNames;

				if(result.Count == 0 && Parent?.Parent != null)
				{
					//	If the local list is not overridden, then default to the
					//	parent.
					result = Parent.Parent.DataNames;
				}
				return result;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	DateTimeValue																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="DateTimeValue">DateTimeValue</see>.
		/// </summary>
		private DateTime mDateTimeValue = DateTime.MinValue;
		/// <summary>
		/// Get/Set the date and time associated with the current action.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'DateTime'.</para>
		/// </remarks>
		public DateTime DateTimeValue
		{
			get
			{
				DateTime result = mDateTimeValue;

				if(result == DateTime.MinValue && Parent?.Parent != null)
				{
					result = Parent.Parent.DateTimeValue;
				}
				return result;
			}
			set { mDateTimeValue = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Digits																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Digits">Digits</see>.
		/// </summary>
		private int mDigits = int.MinValue;
		/// <summary>
		/// Get/Set the number of digits associated with the current action.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public int Digits
		{
			get
			{
				int result = mDigits;

				if(result == int.MinValue)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Digits;
					}
					else
					{
						result = 0;
					}
				}
				return result;
			}
			set { mDigits = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetActionName																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the predefined version of the caller-supplied action name.
		/// </summary>
		/// <param name="actionName">
		/// Casual name of the action to look up.
		/// </param>
		/// <returns>
		/// Formal name of the specified action, if found.
		/// </returns>
		public static string GetActionName(string actionName)
		{
			string result = "None";

			if(actionName?.Length > 0)
			{
				foreach(string actionNameItem in mRecognizedActions)
				{
					if(ComparesEqual(actionName, actionNameItem))
					{
						result = actionNameItem;
						break;
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetConditions																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a collection of conditions defined at the caller's item level
		/// and at all of its parents.
		/// </summary>
		/// <param name="item">
		/// Reference to the file action item to inspect.
		/// </param>
		/// <returns>
		/// Reference to a collection of all conditions defined at the current and
		/// baser levels.
		/// </returns>
		public static ConditionCollection GetConditions(TAction item)
		{
			ConditionCollection conditions = null;
			ConditionCollection result = new ConditionCollection();

			if(item != null)
			{
				if(item.Parent != null && item.Parent.Parent != null)
				{
					conditions = GetConditions(item.Parent.Parent);
					foreach(ConditionItem conditionItem in conditions)
					{
						result.Add(conditionItem);
					}
				}
				//	Write the local items last.
				foreach(ConditionItem conditionItem in item.Conditions)
				{
					result.Add(conditionItem);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetOptionByName																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the option specified by name from this or a parent entity.
		/// </summary>
		/// <param name="item">
		/// Reference to the item for which the option will be found.
		/// </param>
		/// <param name="optionName">
		/// Name of the option to retrieve.
		/// </param>
		/// <returns>
		/// Reference to the specified option, if found. Otherwise, null.
		/// </returns>
		public static ActionOptionItem GetOptionByName(TAction item,
			string optionName)
		{
			ActionOptionItem result = null;

			if(item != null && optionName?.Length > 0)
			{
				result = item.Options.FirstOrDefault(x =>
					x.Name.ToLower() == optionName.ToLower());
				if(result == null && item.Parent.Parent != null)
				{
					result = GetOptionByName(item.Parent.Parent, optionName);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetPropertyByName																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the user property specified by name from this or a parent
		/// entity.
		/// </summary>
		/// <param name="item">
		/// Reference to the item for which the property will be retrieved.
		/// </param>
		/// <param name="propertyName">
		/// Name of the property to retrieve.
		/// </param>
		/// <param name="resolveVariables">
		/// Value indicating whether to resolve variables on this call.
		/// </param>
		/// <returns>
		/// Reference to the specified property, if found. Otherwise, null.
		/// </returns>
		public static string GetPropertyByName(TAction item,
			string propertyName, bool resolveVariables = true)
		{
			PropertyInfo propertySystem = null;
			NameValueItem propertyUser = null;
			object propertyValue = null;
			string result = "";

			if(item != null && propertyName?.Length > 0)
			{
				propertySystem =
					mPublicProperties.FirstOrDefault(x => x.Name.ToLower() ==
					propertyName.ToLower());
				if(propertySystem != null)
				{
					//	Built-in property.
					propertyValue = propertySystem.GetValue(item);
					if(propertyValue != null)
					{
						result = propertyValue.ToString();
					}
				}
				else
				{
					//	User property.
					propertyUser = item.Properties.FirstOrDefault(x =>
						x.Name.ToLower() == propertyName.ToLower());
					if(propertyUser != null)
					{
						result = propertyUser.Value;
					}
					else if(item.Parent != null && item.Parent.Parent != null)
					{
						result = GetPropertyByName(item.Parent.Parent,
							propertyName, false);
					}
				}
				if(result.Length > 0 && resolveVariables)
				{
					result = NormalizeValue(item, result);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetSpecifiedOrWorking																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a reference either to the PowerPoint specified in the local file
		/// arguments or the previously loaded working document.
		/// </summary>
		/// <param name="item">
		/// Reference to the action item within which the file arguments will
		/// be found.
		/// </param>
		/// <returns>
		/// Reference to the PowerPoint document found, if successul. Otherwise,
		/// null.
		/// </returns>
		private static ActionDocumentItem GetSpecifiedOrWorking(TAction item)
		{
			string content = "";
			ActionDocumentItem doc = null;

			//	TODO: Specified or working method needs work.
			if(item != null)
			{
				if(CheckElements(item,
					ActionElementEnum.InputFilename |
					ActionElementEnum.OutputFilename,
					includeInherited: false, quiet: true))
				{
					////	Just load the document if the filenames were specified.
					//content = File.ReadAllText(item.InputFiles[0].FullName);
					//doc = new SvgDocumentItem(content);
				}
				else
				{
					doc = item.WorkingDocument;
				}
			}
			return doc;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Images																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Images">Images</see>.
		/// </summary>
		private static BitmapInfoCollection mImages = new BitmapInfoCollection();
		/// <summary>
		/// Get a reference to the collection of images in this session.
		/// </summary>
		[JsonIgnore]
		public static BitmapInfoCollection Images
		{
			get { return mImages; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputDir																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="InputDir">InputDir</see>.
		/// </summary>
		private DirectoryInfo mInputDir = null;
		/// <summary>
		/// Get/Set the internal, calculated input directory.
		/// </summary>
		/// <remarks>
		/// This property is non-inerhitable.
		/// </remarks>
		[JsonIgnore]
		public DirectoryInfo InputDir
		{
			get { return mInputDir; }
			set { mInputDir = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputFilename																													*
		//*-----------------------------------------------------------------------*
		private string mInputFilename = null;
		/// <summary>
		/// Get/Set the input path and filename of the input file.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'InFile'.</para>
		/// </remarks>
		public string InputFilename
		{
			get
			{
				string result = mInputFilename;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.InputFilename;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mInputFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputFiles																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="InputFiles">InputFiles</see>.
		/// </summary>
		private List<FileInfo> mInputFiles = new List<FileInfo>();
		/// <summary>
		/// Get a reference to the collection of file information used as input in
		/// this session.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		[JsonIgnore]
		public List<FileInfo> InputFiles
		{
			get
			{
				List<FileInfo> result = mInputFiles;

				if(result.Count == 0 && Parent?.Parent != null)
				{
					result = Parent.Parent.InputFiles;
				}
				return result;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputFolderName																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="InputFolderName">InputFolderName</see>.
		/// </summary>
		private string mInputFolderName = null;
		/// <summary>
		/// Get/Set the path and folder name of the input for this action.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'InFolder'.</para>
		/// </remarks>
		public string InputFolderName
		{
			get
			{
				string result = mInputFolderName;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.InputFolderName;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mInputFolderName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputNames																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="InputNames">InputNames</see>.
		/// </summary>
		private List<string> mInputNames = new List<string>();
		/// <summary>
		/// Get a reference to the list of filenames or foldernames with
		/// or without wildcards. This parameter can be specified multiple times
		/// on the command line with different values to load multiple input files.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'Inputs'.</para>
		/// </remarks>
		public List<string> InputNames
		{
			get
			{
				List<string> result = mInputNames;

				if(result.Count == 0 && Parent?.Parent != null)
				{
					//	If the local list is not overridden, then default to the
					//	parent.
					result = Parent.Parent.InputNames;
				}
				return result;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* IsOutputLocal																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the output filenames are local at
		/// this level.
		/// </summary>
		/// <returns>
		/// True if an output filename has been specified at this level.
		/// </returns>
		public bool IsOutputLocal()
		{
			return (mOutputName?.Length > 0);
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Message																																*
		//*-----------------------------------------------------------------------*
		private string mMessage = "";
		/// <summary>
		/// Get/Set a message to be displayed when this action is run.
		/// </summary>
		public string Message
		{
			get { return mMessage; }
			set { mMessage = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Options																																*
		//*-----------------------------------------------------------------------*
		private ActionOptionCollection mOptions =
			new ActionOptionCollection();
		/// <summary>
		/// Get a reference to the collection of options assigned to this action.
		/// </summary>
		/// <remarks>
		/// This property is not inheritable. However, options from parent levels
		/// are retrieved when calling the GetOptionByName function.
		/// </remarks>
		public ActionOptionCollection Options
		{
			get { return mOptions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputFile																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="OutputFile">OutputFile</see>.
		/// </summary>
		private FileInfo mOutputFile = null;
		/// <summary>
		/// Get/Set the internal, calculated output file.
		/// </summary>
		/// <remarks>
		/// This property is non-inheritable.
		/// </remarks>
		[JsonIgnore]
		public FileInfo OutputFile
		{
			get { return mOutputFile; }
			set { mOutputFile = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputDir																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="OutputDir">OutputDir</see>.
		/// </summary>
		private DirectoryInfo mOutputDir = null;
		/// <summary>
		/// Get/Set the internal, calculated output directory.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		[JsonIgnore]
		public DirectoryInfo OutputDir
		{
			get
			{
				DirectoryInfo directory = mOutputDir;

				if(directory == null && Parent != null && Parent != null)
				{
					directory = Parent.Parent.OutputDir;
				}
				return directory;
			}
			set { mOutputDir = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputFilename																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="OutputFilename">OutputFilename</see>.
		/// </summary>
		private string mOutputFilename = null;
		/// <summary>
		/// Get/Set the output path and filename for this action.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'OutFile'.</para>
		/// </remarks>
		public string OutputFilename
		{
			get
			{
				string result = mOutputFilename;

				if(result == null)
				{
					if(Parent != null)
					{
						result = mOutputFilename;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mOutputFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputFolderName																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="OutputFolderName">OutputFolderName</see>.
		/// </summary>
		private string mOutputFolderName = null;
		/// <summary>
		/// Get/Set the output path and folder name for this action.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'OutFolder'.</para>
		/// </remarks>
		public string OutputFolderName
		{
			get
			{
				string result = mOutputFolderName;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.OutputFolderName;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mOutputFolderName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputName																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="OutputName">OutputName</see>.
		/// </summary>
		private string mOutputName = null;
		/// <summary>
		/// Get/Set an output pattern that allows for filenames or foldernames
		/// with or without wildcards. This parameter can be specified muliple
		/// times on the command line with different values to write to multiple
		/// output files.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'Output'.</para>
		/// </remarks>
		public string OutputName
		{
			get
			{
				string result = mOutputName;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.OutputName;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mOutputName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Parent">Parent</see>.
		/// </summary>
		private TCollection mParent = null;
		/// <summary>
		/// Get/Set a reference to the parent of this item.
		/// </summary>
		/// <remarks>
		/// This property is non-inheritable.
		/// </remarks>
		[JsonIgnore]
		public TCollection Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Pattern																																*
		//*-----------------------------------------------------------------------*
		private string mPattern = null;
		/// <summary>
		/// Get/Set a regular expression pattern for files, folders, or other
		/// appropriate strings.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public string Pattern
		{
			get
			{
				string result = mPattern;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Pattern;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mPattern = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Properties																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Properties">Properties</see>.
		/// </summary>
		private NameValueCollection mProperties = new NameValueCollection();
		/// <summary>
		/// Get a reference to the collection of properties assigned to this
		/// action.
		/// </summary>
		/// <remarks>
		/// This property is not inheritable. However, properties from parent
		/// levels are retrieved when calling the GetPropertyByName function.
		/// </remarks>
		public NameValueCollection Properties
		{
			get { return mProperties; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Range																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Range">Range</see>.
		/// </summary>
		private StartEndItem mRange = null;
		/// <summary>
		/// Get/Set a reference to the start and end values of the range.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public StartEndItem Range
		{
			get
			{
				StartEndItem result = mRange;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Range;
					}
					else
					{
						result = mRange = new StartEndItem();
					}
				}
				return result;
			}
			set { mRange = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RecognizedActions																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="RecognizedActions">RecognizedActions</see>.
		/// </summary>
		private static List<string> mRecognizedActions = new List<string>()
		{
			"None",
			"Batch",
			"DrawImage",
			"FileOpenImage",
			"FileOverlayImage",
			"FileSaveImage",
			"ForEachFile",
			"If",
			"ImageBackground",
			"ImagesClear",
			"OpenWorkingDocument",
			"RunSequence",
			"SaveWorkingDocument",
			"SetWorkingImage",
			"SizeImage"
		};
		/// <summary>
		/// Get a reference to the collection of recognized actions in this
		/// session.
		/// </summary>
		public static List<string> RecognizedActions
		{
			get { return mRecognizedActions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the configured action.
		/// </summary>
		/// <returns>
		/// Reference to a Task that represents the asynchronous operation.
		/// </returns>
		public async Task Run()
		{
			string action = "";
			//List<FileActionItem> actionItems = null;
			string content = "";
			FileInfo file = null;
			string lineNumber = "";
			Match match = null;
			string position = "";
			List<TAction> soloItems = null;
			string sourceFilename = "";
			string targetFilename = "";
			TAction topItem = null;

			//	TODO: Create an error exit routine...
			//	Decide which errors require exit and which can just be reported.

			if(mWorkingPathLast != WorkingPath)
			{
				Trace.WriteLine($"Working Path: {WorkingPath}",
					$"{MessageImportanceEnum.Info}");
				mWorkingPathLast = WorkingPath;
			}

			Trace.WriteLine($"Action {mAction}...", $"{MessageImportanceEnum.Info}");
			if(Message?.Length > 0)
			{
				Trace.WriteLine($" {Message}", $"{MessageImportanceEnum.Info}");
			}

			if(ComparesEqual(mAction, "batch"))
			{
				//	If this is a batch action, read the contents of
				//	ConfigFilename.
				if(ConfigFilename?.Length > 0)
				{
					Trace.WriteLine(
						$"Opening configuration file: {Path.GetFileName(ConfigFilename)}",
						$"{MessageImportanceEnum.Info}");
					sourceFilename = AbsolutePath(
						GetPropertyByName((TAction)this, nameof(WorkingPath)),
						GetPropertyByName((TAction)this, nameof(ConfigFilename)));
					content = File.ReadAllText(sourceFilename);
					if(content?.Length > 0)
					{
						try
						{
							topItem = DeserializeFile(content);
							//topItem = JsonConvert.DeserializeObject<ActionItemBase>(content);
							if(ComparesEqual(topItem.Action, "batch"))
							{
								//	All of the top item information is added to this item.
								//	Allow working path to be specified at the first level.
								CopyFields(topItem, this,
									skipList: new string[]
									{
									"mAction", "mConfigFilename",
									"mCurrentFile", "mParent"
									},
									nonBlanks: new string[] { "mWorkingPath" });
							}
							else
							{
								//	The top item is a child of this action.
								this.Actions.Add(topItem);
							}
							this.Actions.Parent = (TAction)this;
							InitializeParent((TAction)this);
						}
						catch(Exception ex)
						{
							lineNumber = "Unknown";
							position = "Unknown";
							match = Regex.Match(ex.Message,
								ResourceMain.rxJsonErrorLinePosition);
							if(match.Success)
							{
								lineNumber = GetValue(match, "line");
								position = GetValue(match, "position");
							}
							Trace.WriteLine(
								"Error loading configuration file: " +
								$"Line: {lineNumber}, Position: {position}",
								$"{MessageImportanceEnum.Err}");
						}
					}
					else
					{
						Trace.WriteLine("Error: No configuration data loaded from: " +
							$"{sourceFilename}", $"{MessageImportanceEnum.Err}");
					}
				}
				else
				{
					Trace.WriteLine("Error: Config filename not specified...",
						$"{MessageImportanceEnum.Err}");
				}
			}
			this.Actions.Parent = (TAction)this;
			//if(mParent == null)
			//{
			//	//	Initialize all levels from the top level.
			//	InitializeLevels(this);
			//}
			InitializeFilenames((TAction)this);
			//if(mAction != ActionTypeEnum.Batch)
			//{
			//	//	When this level isn't a batch, identify all folders and files
			//	//	for the action.
			//	In this version, input files can be defined at any level.
			IdentifyInputFiles((TAction)this);
			IdentifyDataFiles((TAction)this);
			//}
			IdentifyOutputFiles((TAction)this);
			action = mAction.ToLower();
			switch(action)
			{
				case "batch":
					//	This is a file batch.
					//	Check first to see if there is a solo.
					soloItems = this.Actions.FindAll(x =>
						x.Options.Exists(y => y.Name.ToLower() == "solo"));
					if(soloItems?.Count > 0)
					{
						//	Only run the solo items.
						await RunActions(soloItems);
					}
					else
					{
						//	Run all non-muted items.
						await RunActions(this.Actions);
					}
					if(IsOutputLocal())
					{
						WriteLocalOutput();
						//switch(this.OutputType)
						//{
						//	case RenderFileTypeEnum.RectangleInfoList:
						//		targetFilename =
						//			AbsolutePath(
						//				GetPropertyByName(this, nameof(WorkingPath)),
						//				GetPropertyByName(this, nameof(OutputName)));
						//		file = new FileInfo(targetFilename);
						//		Trace.WriteLine(
						//			$"Writing Rectangles to {file.Name}",
						//			$"{MessageImportanceEnum.Info}");
						//		content = JsonConvert.SerializeObject(RectangleInfoList);
						//		File.WriteAllText(file.FullName, content);
						//		break;
						//}
					}
					break;
				case "drawimage":
					DrawImage((TAction)this);
					break;
				case "fileopenimage":
					FileOpenImage((TAction)this);
					break;
				case "fileoverlayimage":
					FileOverlayImage((TAction)this);
					break;
				case "filesaveimage":
					FileSaveImage((TAction)this);
					break;
				case "foreachfile":
					ForEachFile((TAction)this);
					break;
				case "if":
					//	Run comparisons in this item's Actions collection.
					If((TAction)this);
					break;
				case "imagebackground":
					//	Paint the specified image background color and / or image
					//	on the current working image.
					ImageBackground((TAction)this);
					break;
				case "imagesclear":
					//	Clear the current working image set.
					ImagesClear((TAction)this);
					break;
				case "openworkingdocument":
					//	Open the working file to allow multiple operations to be
					//	completed in the same session.
					OpenWorkingDocument();
					break;
				case "runsequence":
					RunSequence((TAction)this);
					break;
				case "saveworkingdocument":
					SaveWorkingDocument((TAction)this);
					break;
				case "setworkingimage":
					SetWorkingImage((TAction)this);
					break;
				case "sizeimage":
					SizeImage((TAction)this);
					break;
				default:
					RunCustomAction();
					//Trace.WriteLine($" Error: {Action} not implemented...",
					//	$"{MessageImportanceEnum.Err}");
					break;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Sequences																															*
		//*-----------------------------------------------------------------------*
		private SequenceCollection<TAction> mSequences =
			new SequenceCollection<TAction>();
		/// <summary>
		/// Get a reference to the collection of sequences defined for this action.
		/// </summary>
		/// <remarks>
		/// This property is not inheritable.
		/// </remarks>
		public SequenceCollection<TAction> Sequences
		{
			get
			{
				SequenceCollection<TAction> result = mSequences;

				if(result.Count == 0 && Parent != null && Parent.Parent != null)
				{
					result = Parent.Parent.Sequences;
				}
				return result;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeAction																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Action property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeAction()
		{
			return true;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeActions																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Actions property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeActions()
		{
			return mActions.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeBase																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Base property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeBase()
		{
			return mBase?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeConditions																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Conditions property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeConditions()
		{
			return mConditions.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeConfigFilename																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the ConfigFilename property should
		/// be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeConfigFilename()
		{
			return mConfigFilename?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeCount																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Count property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeCount()
		{
			return mCount != float.MinValue;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeDataFilename																						*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the DataFilename property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeDataFilename()
		{
			return mDataFilename?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeDataNames																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the DataNames property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeDataNames()
		{
			return mDataNames.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeDateTimeValue																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the DateTimeValue property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeDateTimeValue()
		{
			return DateTime.Compare(mDateTimeValue, DateTime.MinValue) != 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeDigits																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Digits property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeDigits()
		{
			return mDigits != int.MinValue;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeInputFilename																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the InputFilename property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeInputFilename()
		{
			return mInputFilename?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeInputFolderName																				*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the InputFolderName property should
		/// be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeInputFolderName()
		{
			return mInputFolderName?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeInputNames																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the InputNames property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeInputNames()
		{
			return mInputNames.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeMessage																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Message property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeMessage()
		{
			return mMessage?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeOptions																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Options property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeOptions()
		{
			return mOptions.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeOutputFilename																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the OutputFilename property should
		/// be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeOutputFilename()
		{
			return mOutputFilename?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeOutputFolderName																				*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the OutputFolderName property should
		/// be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeOutputFolderName()
		{
			return mOutputFolderName?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeOutputName																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the OutputName property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeOutputName()
		{
			return mOutputName?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializePattern																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Pattern property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializePattern()
		{
			return mPattern?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeProperties																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Properties property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeProperties()
		{
			return mProperties.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeRange																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Range property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeRange()
		{
			return mRange != null;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeSequences																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Sequences property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeSequences()
		{
			return mSequences.Count > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeSourceFolderName																				*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the SourceFolderName property should
		/// be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeSourceFolderName()
		{
			return mSourceFolderName?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeText																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the Text property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeText()
		{
			return mText?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeWorkingDocumentIndex																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the WorkingDocumentIndex property
		/// should be serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeWorkingDocumentIndex()
		{
			return mWorkingDocumentIndex > -1;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShouldSerializeWorkingPath																						*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the WorkingPath property should be
		/// serialized.
		/// </summary>
		/// <returns>
		/// A value indicating whether or not to serialize the property.
		/// </returns>
		public virtual bool ShouldSerializeWorkingPath()
		{
			return mWorkingPath?.Length > 0;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SourceDir																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="SourceDir">SourceDir</see>.
		/// </summary>
		private DirectoryInfo mSourceDir = null;
		/// <summary>
		/// Get/Set the internal, calculated source directory.
		/// </summary>
		/// <remarks>
		/// This property is non-inerhitable.
		/// </remarks>
		[JsonIgnore]
		public DirectoryInfo SourceDir
		{
			get { return mSourceDir; }
			set { mSourceDir = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SourceFolderName																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="SourceFolderName">SourceFolderName</see>.
		/// </summary>
		private string mSourceFolderName = null;
		/// <summary>
		/// Get/Set the path and folder name of the data source for this action.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'InFolder'.</para>
		/// </remarks>
		public string SourceFolderName
		{
			get
			{
				string result = mSourceFolderName;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.SourceFolderName;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mSourceFolderName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Stop																																	*
		//*-----------------------------------------------------------------------*
		private bool mStop = false;
		/// <summary>
		/// Get/Set a value indicating whether the process should be stopped.
		/// </summary>
		[JsonIgnore]
		public bool Stop
		{
			get { return mStop; }
			set
			{
				mStop = value;
				if(Parent?.Parent != null)
				{
					Parent.Parent.Stop = value;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		////*-----------------------------------------------------------------------*
		////*	StyleWorksheets																												*
		////*-----------------------------------------------------------------------*
		///// <summary>
		///// Private member for <see cref="StyleWorksheets">StyleWorksheets</see>.
		///// </summary>
		//private List<string> mStyleWorksheets = null;
		///// <summary>
		///// Get/Set a reference to the collection of style extension worksheets
		///// defined at this level.
		///// </summary>
		//public List<string> StyleWorksheets
		//{
		//	get
		//	{
		//		List<string> result = mStyleWorksheets;

		//		if(result == null && mParent != null)
		//		{
		//			result = mParent.GetStyleWorksheets();
		//			if(result == null)
		//			{
		//				//	Return a safe value.
		//				result = new List<string>();
		//			}
		//		}
		//		return result;
		//	}
		//	set { mStyleWorksheets = value; }
		//}
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Text																																	*
		//*-----------------------------------------------------------------------*
		private string mText = null;
		/// <summary>
		/// Get/Set the text of the current action.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public string Text
		{
			get
			{
				string result = mText;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.Text;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set { mText = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingDocument																												*
		//*-----------------------------------------------------------------------*
		private ActionDocumentItem mWorkingDocument = null;
		/// <summary>
		/// Get/Set the working key file for operations in this instance.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// </remarks>
		[JsonIgnore]
		public ActionDocumentItem WorkingDocument
		{
			get
			{
				ActionDocumentItem result = mWorkingDocument;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.WorkingDocument;
					}
				}
				return result;
			}
			set
			{
				if(mInputFilename?.Length > 0 ||
					Parent == null || Parent.Parent == null)
				{
					mWorkingDocument = value;
					if(mWorkingDocument != null)
					{
						mWorkingDocument.IsLocal = false;
					}
				}
				else if(string.IsNullOrEmpty(mInputFilename) &&
					Parent?.Parent != null)
				{
					Parent.Parent.WorkingDocument = value;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingDocumentIndex																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="WorkingDocumentIndex">WorkingDocumentIndex</see>.
		/// </summary>
		private int mWorkingDocumentIndex = -1;
		/// <summary>
		/// Get/Set the input file index representing the working document.
		/// </summary>
		/// <remarks>
		/// This property is inheritable.
		/// </remarks>
		public int WorkingDocumentIndex
		{
			get
			{
				int result = mWorkingDocumentIndex;

				if(result == -1)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.WorkingDocumentIndex;
					}
				}
				if(result == -1)
				{
					result = 0;
				}
				return result;
			}
			set { mWorkingDocumentIndex = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingImage																													*
		//*-----------------------------------------------------------------------*
		private static BitmapInfoItem mWorkingImage = null;
		/// <summary>
		/// Get/Set a reference to the current working image in this session.
		/// </summary>
		[JsonIgnore]
		public static BitmapInfoItem WorkingImage
		{
			get { return mWorkingImage; }
			set { mWorkingImage = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingPath																														*
		//*-----------------------------------------------------------------------*
		private string mWorkingPath = null;
		/// <summary>
		/// Get/Set the working path for operations in this instance.
		/// </summary>
		/// <remarks>
		/// <para>This property is inheritable.</para>
		/// <para>Corresponds with the command-line parameter 'WorkingPath'.</para>
		/// </remarks>
		public string WorkingPath
		{
			get
			{
				string result = mWorkingPath;

				if(result == null)
				{
					if(Parent?.Parent != null)
					{
						result = Parent.Parent.WorkingPath;
					}
					else
					{
						result = "";
					}
				}
				return result;
			}
			set
			{
				mWorkingPath = value;
			}
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
