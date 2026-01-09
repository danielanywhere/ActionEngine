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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkiaSharp;

namespace ActionEngine
{
	//*-------------------------------------------------------------------------*
	//*	ActionEngineUtil																												*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Utility features and functionality for PowerPoint Control Language.
	/// </summary>
	public class ActionEngineUtil
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		/// <summary>
		/// Absolute directory pattern index hints.
		/// </summary>
		private static string[] mAbsIndex = new string[]
		{
			":"
		};

		/// <summary>
		/// Absolute directory start pattern hints.
		/// </summary>
		private static string[] mAbsStart = new string[]
		{
			@"\", "/", "~", "$", @"\\", "//"
		};

		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//* AbsolutePath																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the absolute path found between the working and relative paths.
		/// </summary>
		/// <param name="workingPath">
		/// The working or default path.
		/// </param>
		/// <param name="relPath">
		/// The relative path or possible fully qualified override.
		/// </param>
		/// <returns>
		/// The absolute path found for the two components.
		/// </returns>
		public static string AbsolutePath(string workingPath, string relPath)
		{
			string result = "";

			if(workingPath?.Length > 0 && (relPath == null || relPath.Length == 0))
			{
				//	Only the working path was specified.
				result = workingPath;
			}
			else if((workingPath == null || workingPath.Length == 0) &&
				relPath?.Length > 0)
			{
				//	Only the relative path was specified.
				result = relPath;
			}
			else if(IsAbsoluteDir(relPath))
			{
				//	Relative path is a full path.
				result = relPath;
			}
			else
			{
				//	Both the working and relative paths contain information.
				result = Path.Combine(workingPath, relPath);
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ActionIsNone																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the value of the caller's action is
		/// effectively 'None'.
		/// </summary>
		/// <param name="actionType">
		/// The action to inspect.
		/// </param>
		/// <returns>
		/// True if the caller's action is effectively 'None'. Otherwise, false.
		/// </returns>
		public static bool ActionIsNone(string actionType)
		{
			return string.IsNullOrEmpty(actionType) ||
				actionType.ToLower() == "none";
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* AssureFolder																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Assure the specified folder path exists and return it to the caller if
		/// so.
		/// </summary>
		/// <param name="pathName">
		/// Full path name of the folder to test for.
		/// </param>
		/// <param name="create">
		/// Value indicating whether to create the path if it doesn't yet exist.
		/// </param>
		/// <param name="message">
		/// Message to display with console messages about this folder.
		/// </param>
		/// <param name="quiet">
		/// Value indicating whether to suppress messages.
		/// </param>
		/// <returns>
		/// Reference to the DirectoryInfo representing the folder if it was
		/// possible that the folder existed. Null if the path led to a file
		/// or was not created.
		/// </returns>
		public static DirectoryInfo AssureFolder(string pathName,
			bool create = false, string message = "", bool quiet = false)
		{
			string fullName = "";
			DirectoryInfo result = null;

			if(pathName?.Length > 0)
			{
				fullName = GetFullFoldername(pathName, create, message, quiet);
				if(fullName.Length > 0)
				{
					result = new DirectoryInfo(fullName);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ComparesEqual																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the two strings can be compared as
		/// equal without attention to case.
		/// </summary>
		/// <param name="value1">
		/// First value to compare.
		/// </param>
		/// <param name="value2">
		/// Second value to compare.
		/// </param>
		/// <returns>
		/// True if the two strings compare as equal without attention to case.
		/// Otherwise, false.
		/// </returns>
		public static bool ComparesEqual(string value1, string value2)
		{
			bool result = false;

			if(value1?.Length > 0 && value2?.Length > 0)
			{
				result = StringComparer.OrdinalIgnoreCase.Equals(value1, value2);
			}
			else if(string.IsNullOrEmpty(value1) &&
				string.IsNullOrEmpty(value2))
			{
				result = true;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* CopyFields<T>																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Copy the private fields of public properties from the source to target.
		/// </summary>
		/// <typeparam name="T">
		/// Type of object to operate upon.
		/// </typeparam>
		/// <param name="source">
		/// Reference to the source object.
		/// </param>
		/// <param name="target">
		/// Reference to the target object.
		/// </param>
		/// <param name="skipList">
		/// Optional list of field names to skip.
		/// </param>
		/// <param name="nonBlanks">
		/// Optional list of field names to write if non-blank.
		/// </param>
		public static void CopyFields<T>(T source, T target,
			string[] skipList = null,
			string[] nonBlanks = null) where T : class
		{
			BindingFlags bindingFlagsF =
				BindingFlags.Instance | BindingFlags.NonPublic;
			BindingFlags bindingFlagsP =
				BindingFlags.Instance | BindingFlags.Public;
			bool bWrite = true;
			Type elementType = null;
			FieldInfo[] fields = typeof(T).GetFields(bindingFlagsF);
			MethodInfo addMethod = null;
			PropertyInfo[] properties = typeof(T).GetProperties(bindingFlagsP);
			IEnumerable<object> sourceList = null;
			IEnumerable<object> targetList = null;
			object workingValue = null;

			foreach(FieldInfo field in fields)
			{
				if(field.Name.Length > 1 &&
					(skipList == null || !skipList.Contains(field.Name)) &&
					properties.FirstOrDefault(x =>
						x.Name == field.Name.Substring(1)) != null)
				{
					workingValue = field.GetValue(source);
					if(workingValue != null && workingValue is IEnumerable<object>)
					{
						//	The following blind copy is okay, because both lists are
						//	expected to be of the same type.
						sourceList = (IEnumerable<object>)workingValue;
						targetList = (IEnumerable<object>)field.GetValue(target);
						if(sourceList.Count() > 0)
						{
							elementType = sourceList.First().GetType();
							addMethod =
								workingValue.GetType().GetMethod("Add", new[] { elementType });
							foreach(Object item in sourceList)
							{
								addMethod.Invoke(targetList, new object[] { item });
							}
						}
					}
					else
					{
						if(nonBlanks?.Contains(field.Name) == true)
						{
							bWrite = (workingValue?.ToString().Length > 0);
						}
						else
						{
							bWrite = true;
						}
						if(bWrite)
						{
							field.SetValue(target, workingValue);
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* DeepCopy																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a new instance of the provided item.
		/// </summary>
		/// <param name="item">
		/// Reference to the item to be copied.
		/// </param>
		/// <returns>
		/// Reference to a completely new instance of the file action item
		/// provided by the caller.
		/// </returns>
		public static T DeepCopy<T>(T item)
		{
			string content = "";
			T result = default(T);

			if(item != null)
			{
				content = JsonConvert.SerializeObject(item);
				result = JsonConvert.DeserializeObject<T>(content);
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* DrawBitmap																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Draw the source bitmap on the target bitmap using the specified source
		/// and target images and a starting point at which to place the source.
		/// </summary>
		/// <param name="sourceBitmap">
		/// Reference to the source bitmap to be drawn.
		/// </param>
		/// <param name="targetBitmap">
		/// Reference to the target bitmap to receive the update.
		/// </param>
		/// <param name="targetPoint">
		/// The point at which drawing will begin on the target image.
		/// </param>
		public static void DrawBitmap(SKBitmap sourceBitmap, SKBitmap targetBitmap,
			SKPoint targetPoint)
		{
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

			if(sourceBitmap != null && targetBitmap != null)
			{
				using(SKCanvas canvas = new SKCanvas(targetBitmap))
				{
					using(SKPaint paint = new SKPaint() { IsAntialias = true })
					{
						using(SKImage image = SKImage.FromBitmap(sourceBitmap))
						{
							canvas.DrawImage(image, targetPoint, samplingOptions, paint);
						}
					}
				}
			}
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Draw the source bitmap on the target bitmap using the specified source
		/// image and target image rectangles.
		/// </summary>
		/// <param name="sourceBitmap">
		/// Reference to the source bitmap.
		/// </param>
		/// <param name="targetBitmap">
		/// Reference to the target bitmap.
		/// </param>
		/// <param name="sourceRect">
		/// Reference to the source rectangle.
		/// </param>
		/// <param name="targetRect">
		/// Reference to the target rectangle.
		/// </param>
		public static void DrawBitmap(SKBitmap sourceBitmap, SKBitmap targetBitmap,
			SKRect sourceRect, SKRect targetRect)
		{
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

			if(sourceBitmap != null && targetBitmap != null &&
				sourceRect != SKRect.Empty && targetRect != SKRect.Empty)
			{
				using(SKCanvas canvas = new SKCanvas(targetBitmap))
				{
					using(SKPaint paint = new SKPaint() { IsAntialias = true })
					{
						using(SKImage image = SKImage.FromBitmap(sourceBitmap))
						{
							canvas.DrawImage(image,
								sourceRect, targetRect, samplingOptions, paint);
						}
					}
				}
			}
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Draw the source bitmap on the target bitmap using the specified source
		/// and target images and a starting point at which to place the source.
		/// </summary>
		/// <param name="sourceBitmap">
		/// Reference to the source bitmap to be drawn.
		/// </param>
		/// <param name="targetBitmap">
		/// Reference to the target bitmap to receive the update.
		/// </param>
		/// <param name="targetRect">
		/// The area at which drawing will begin on the target image.
		/// </param>
		public static void DrawBitmap(SKBitmap sourceBitmap, SKBitmap targetBitmap,
			SKRect targetRect)
		{
			SKSamplingOptions samplingOptions =
				new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

			if(sourceBitmap != null && targetBitmap != null &&
				targetRect != SKRect.Empty)
			{
				using(SKCanvas canvas = new SKCanvas(targetBitmap))
				{
					using(SKPaint paint = new SKPaint() { IsAntialias = true })
					{
						using(SKImage image = SKImage.FromBitmap(sourceBitmap))
						{
							canvas.DrawImage(image, targetRect, samplingOptions, paint);
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* EnumerateFilesAndDirectories																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Enumerate through files and directories to return a list of fully
		/// qualified files, given a solid base path and a search pattern that
		/// can include a combination of file and directory names.
		/// </summary>
		/// <param name="directoryPath">
		/// Base directory path from where the search will start.
		/// </param>
		/// <param name="searchPattern">
		/// Search pattern containing wild cards.
		/// </param>
		/// <returns>
		/// Reference to a list of file and folder paths matching the provided
		/// search pattern.
		/// </returns>
		public static List<string> EnumerateFilesAndDirectories(
			string directoryPath, string searchPattern)
		{
			// Replace * and ? characters in search pattern with equivalent regex
			// syntax.
			DirectoryInfo dir = null;
			DirectoryInfo[] dirs = null;
			FileInfo[] files = null;
			int leftPath = 0;
			int leftWild = 0;
			string level = "";
			string regexPattern = "";
			string remainder = "";
			char[] pathMark = new char[] { '\\', '/' };
			List<string> results = new List<string>();
			char[] wild = new char[] { '*', '?' };
			string workingPath = "";
			string workingSearch = "";

			if(directoryPath?.Length > 0 && searchPattern?.Length > 0)
			{
				//	Resolve directories first, then resolve filenames or folders in
				//	each directory.
				//	When entering, the search pattern may continue one or more levels
				//	that are not affected by wildcards. Transfer those to the search
				//	base.
				workingSearch = searchPattern;
				workingPath = directoryPath;
				if(workingSearch.IndexOfAny(wild) > -1 &&
					workingSearch.IndexOfAny(pathMark) > -1)
				{
					//	There are directory marks and wildcards in the search pattern.
					//	Move all non-wild path levels to the left.
					leftWild = workingSearch.IndexOfAny(wild);
					leftPath = workingSearch.IndexOfAny(pathMark);
					while(leftPath > -1 && leftPath < leftWild)
					{
						//	The next character is a path mark. Transfer that to the left.
						workingPath = Path.Combine(workingPath,
							workingSearch.Substring(0, leftPath));
						if(leftPath + 1 < workingSearch.Length)
						{
							workingSearch = workingSearch.Substring(leftPath + 1);
						}
						else
						{
							//	Landing in this location should be impossible.
							workingSearch = "";
							break;
						}
						leftWild = workingSearch.IndexOfAny(wild);
						leftPath = workingSearch.IndexOfAny(pathMark);
					}
					//	At this point, the working path is solid and the working
					//	search contains a wildcard.
					if(workingSearch.IndexOfAny(pathMark) > -1)
					{
						//	Find folder names at the current level that match the
						//	current wildcard.
						level = workingSearch.Substring(0, leftPath);
						if(workingSearch.Length > leftPath + 1)
						{
							remainder = workingSearch.Substring(leftPath + 1);
						}
						else
						{
							remainder = "";
						}
						regexPattern = "^" +
							Regex.Escape(level).
								Replace(@"\*", ".*").
									Replace(@"\?", ".") + "$";
						dir = new DirectoryInfo(workingPath);
						if(dir.Exists)
						{
							//	Directory found.
							dirs = dir.GetDirectories();
							foreach(DirectoryInfo dirItem in dirs)
							{
								if(Regex.IsMatch(dirItem.Name, regexPattern))
								{
									//	This directory is a match.
									if(remainder.Length > 0)
									{
										//	Continue resolving to the right.
										results.AddRange(
											EnumerateFilesAndDirectories(dirItem.FullName,
											remainder));
									}
									else
									{
										//	This is the end of the line for the search.
										//	Most likely, there was a path terminator.
										results.Add(dirItem.FullName);
									}
								}
							}
						}
					}
				}
				//	After base folders have been moved to the directory path,
				//	the search pattern can be resolved.
				if(workingSearch.IndexOfAny(wild) > -1)
				{
					//	There is only an end-level wildcard.
					//	Check for folders and files.
					regexPattern = "^" +
						Regex.Escape(workingSearch).
							Replace(@"\*", ".*").
								Replace(@"\?", ".") + "$";
					dir = new DirectoryInfo(workingPath);
					if(dir.Exists)
					{
						dirs = dir.GetDirectories();
						foreach(DirectoryInfo dirItem in dirs)
						{
							if(Regex.IsMatch(dir.Name, regexPattern))
							{
								results.Add(dirItem.FullName);
							}
						}
						files = dir.GetFiles();
						foreach(FileInfo fileItem in files)
						{
							if(Regex.IsMatch(fileItem.Name, regexPattern))
							{
								results.Add(fileItem.FullName);
							}
						}
					}
				}
				else
				{
					//	Otherwise, the entire search string is the specification.
					results.Add(Path.Combine(workingPath, workingSearch));
				}
			}
			return results;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	EnumerateRange																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a list of items representing the start through end of the range.
		/// </summary>
		/// <param name="range">
		/// Reference to the range to enumerate.
		/// </param>
		/// <param name="digitCount">
		/// Count of digits required on the output value.
		/// </param>
		/// <param name="defaultExtension">
		/// The default extension to add to the files in the range if one
		/// has not been supplied on the range itself.
		/// </param>
		/// <returns>
		/// Reference to a newly created list of items enumerating all of the
		/// possible items in the range.
		/// </returns>
		/// <remarks>
		/// In this version, a single numerical seed can be surrounded by any
		/// non-numerical value. If more than one numerical seed exist in the
		/// source string, those values will be treated as literal.
		/// If no numerical values are provided, or the pattern doesn't match
		/// between start and end values, only the start and end values are
		/// returned.
		/// </remarks>
		public static List<string> EnumerateRange(StartEndItem range,
			int digitCount = 0, string defaultExtension = "")
		{
			int digits = digitCount;
			string extension = defaultExtension;
			string firstPart = "";
			string lastPart = "";
			int index = 0;
			Match match = null;
			List<string> result = new List<string>();
			int seed1 = 0;
			int seed2 = 0;
			int seedMax = 0;
			int seedMin = 0;

			if(range != null && range.StartValue.Length > 0)
			{
				if(range.StartValue.Contains('.'))
				{
					extension = "";
				}
				else if(defaultExtension?.Length > 0)
				{
					extension = defaultExtension;
				}
				//	If the start value was specified, it will be returned
				//	unconditionally.
				result.Add($"{range.StartValue}{extension}");
				if(range.EndValue.Length > 0)
				{
					//	An end value was specified.
					match = Regex.Match(range.StartValue, ResourceMain.rxNumericalSeed);
					if(match != null && match.Success)
					{
						//	A numerical seed was found in the start.
						firstPart = GetValue(match, "pre");
						lastPart = GetValue(match, "post");
						digits = Math.Max(digits, GetValue(match, "seed").Length);
						seed1 = ToInt(GetValue(match, "seed"));
						match = Regex.Match(range.EndValue, ResourceMain.rxNumericalSeed);
						if(match != null && match.Success)
						{
							seed2 = ToInt(GetValue(match, "seed"));
							digits = Math.Max(digits, GetValue(match, "seed").Length);
							if(firstPart == GetValue(match, "pre") &&
								lastPart == GetValue(match, "post"))
							{
								//	Start and end pattern values align.
								if(seed1 != seed2)
								{
									//	The start and end values refer to different seeds. If
									//	they are equal, only a single item is returned to
									//	the caller.
									seedMin = Math.Min(seed1, seed2);
									seedMax = Math.Max(seed1, seed2);
									for(index = seedMin + 1; index <= seedMax; index++)
									{
										result.Add(
											$"{firstPart}{PadLeft("0", index, digits)}" +
											$"{lastPart}{extension}");
									}
								}
							}
							else
							{
								//	Start and end pattern values are different.
								result.Add($"{range.EndValue}{extension}");
							}
						}
					}
					else
					{
						//	The first item didn't match a numerical seed so there is no
						//	need to continue.
						result.Add($"{range.EndValue}{extension}");
					}
				}
			}
			return result;
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
		//* GetExtension																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the extension portion of the filename.
		/// </summary>
		/// <param name="filename">
		/// Filename to inspect.
		/// </param>
		/// <returns>
		/// The extension portion of the filename.
		/// </returns>
		public static string GetExtension(string filename)
		{
			int index = 0;
			string result = "";

			if(filename?.Length > 0)
			{
				index = filename.IndexOf('.');
				if(index > -1)
				{
					result = filename.Substring(index);
				}
				else
				{
					//	No extension.
					result = filename;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetFullFoldername																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the fully qualified path of the relatively or fully specified
		/// folder.
		/// </summary>
		/// <param name="foldername">
		/// Relative or absolute name of the folder to retrieve.
		/// </param>
		/// <param name="create">
		/// Value indicating whether the folder can be created if it does not
		/// exist.
		/// </param>
		/// <param name="message">
		/// Message to display with folder name.
		/// </param>
		/// <param name="quiet">
		/// Value indicating whether to suppress messages.
		/// </param>
		/// <returns>
		/// Fully qualified path of the specified folder, if found.
		/// Otherwise, an empty string.
		/// </returns>
		public static string GetFullFoldername(string foldername,
			bool create = false, string message = "", bool quiet = false)
		{
			DirectoryInfo dir = null;
			bool exists = false;
			string result = "";

			if(foldername?.Length == 0)
			{
				//	If no folder was specified, use the current working directory.
				dir = new DirectoryInfo(System.Environment.CurrentDirectory);
			}
			else
			{
				//	Some type of filename has been specified.
				if(IsAbsoluteDir(foldername))
				{
					//	Absolute.
					dir = new DirectoryInfo(foldername);
				}
				else
				{
					//	Relative.
					dir = new DirectoryInfo(
						Path.Combine(System.Environment.CurrentDirectory, foldername));
				}
				exists = dir.Exists;
				if(!exists && !create)
				{
					Console.WriteLine($"Path not found: {message} {dir.FullName}");
					dir = null;
				}
				else if(!exists && create)
				{
					//	Folder can be created.
					dir.Create();
				}
				else if(exists &&
					((dir.Attributes & FileAttributes.Directory) !=
					FileAttributes.Directory))
				{
					//	This object is a file.
					Console.WriteLine("Path is a file. " +
						$"Directory expected: {dir.FullName}");
					dir = null;
				}
			}
			if(dir != null)
			{
				if(!quiet)
				{
					Console.WriteLine($"{message} Directory: {dir.FullName}");
				}
				result = dir.FullName;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetIndexValue																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the binary index value of the numerical filename pattern.
		/// </summary>
		/// <param name="filename">
		/// Name of the file to check for index value.
		/// </param>
		/// <returns>
		/// Numerical index value found within the caller's filename, if found.
		/// Otherwise, 0.
		/// </returns>
		public static int GetIndexValue(string filename)
		{
			Match match = null;
			int result = 0;

			if(filename?.Length > 0)
			{
				match = Regex.Match(filename, ResourceMain.rxNumericalSeed);
				if(match.Success)
				{
					result = ToInt(GetValue(match, "seed"));
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetValue																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the value of the specified group member in the provided match.
		/// </summary>
		/// <param name="match">
		/// Reference to the match to be inspected.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the value will be found.
		/// </param>
		/// <returns>
		/// The value found in the specified group, if found. Otherwise, empty
		/// string.
		/// </returns>
		public static string GetValue(Match match, string groupName)
		{
			string result = "";

			if(match != null && match.Groups[groupName] != null &&
				match.Groups[groupName].Value != null)
			{
				result = match.Groups[groupName].Value;
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Return the value of the specified group member in a match found with
		/// the provided source and pattern.
		/// </summary>
		/// <param name="source">
		/// Source string to search.
		/// </param>
		/// <param name="pattern">
		/// Regular expression pattern to apply.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the value will be found.
		/// </param>
		/// <returns>
		/// The value found in the specified group, if found. Otherwise, empty
		/// string.
		/// </returns>
		public static string GetValue(string source, string pattern,
			string groupName)
		{
			Match match = null;
			string result = "";

			if(source?.Length > 0 && pattern?.Length > 0 && groupName?.Length > 0)
			{
				match = Regex.Match(source, pattern);
				if(match.Success)
				{
					result = GetValue(match, groupName);
				}
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Return the string value of the specified property within the caller's
		/// File Action.
		/// </summary>
		/// <param name="actionItem">
		/// Reference to the file action item to be inspected.
		/// </param>
		/// <param name="propertyName">
		/// Name of the property to read on the file action action item.
		/// </param>
		/// <returns>
		/// String representation of the specified property value, if found.
		/// Otherwise, an empty string.
		/// </returns>
		public static string GetValue(ActionItem actionItem,
			string propertyName)
		{
			PropertyInfo property = null;
			string result = "";
			object returned = null;
			Type type = null;

			if(actionItem != null && propertyName?.Length > 0)
			{
				type = actionItem.GetType();
				if(type != null)
				{
					property = type.GetProperty(propertyName);
					if(property != null)
					{
						returned = property.GetValue(actionItem, null);
					}
				}
				if(returned != null)
				{
					result = returned.ToString();
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* IsAbsoluteDir																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the given path name is absolute.
		/// </summary>
		/// <param name="pathName">
		/// The path name to inspect.
		/// </param>
		/// <returns>
		/// True if the caller's path is absolute. Otherwise, false.
		/// </returns>
		/// <remarks>
		/// This implementation is purely practical and doesn't pay any attention
		/// to the convoluted articles in Wikipedia or elsewhere. In this
		/// operation, an absolute path is one that can NOT be added to the end of
		/// another path portion, and a relative path is one that can.
		/// </remarks>
		public static bool IsAbsoluteDir(string pathName)
		{
			bool result = true;

			if(pathName?.Length > 0)
			{
				result = mAbsStart.Any(x => pathName.StartsWith(x)) ||
					mAbsIndex.Any(x => pathName.IndexOf(x) > -1);
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* PadLeft																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Pad the caller's value to the left with the specified pattern until it
		/// is greater than or equal to the specified total width.
		/// </summary>
		/// <param name="pattern">
		/// Pattern to pad the value with.
		/// </param>
		/// <param name="value">
		/// Value to pad.
		/// </param>
		/// <param name="totalWidth">
		/// The total minimum width allowable.
		/// </param>
		/// <returns>
		/// The caller's value, padded left until it has reached at least the
		/// minimum total width.
		/// </returns>
		public static string PadLeft(string pattern, int value, int totalWidth)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(value);
			if(pattern?.Length > 0 && totalWidth > 0)
			{
				while(builder.Length < totalWidth)
				{
					builder.Insert(0, pattern);
				}
			}
			return builder.ToString();
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
		//* ResolveFilename																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Resolve the supplied path and filename to return all files that match
		/// that name, including wildcards.
		/// </summary>
		/// <param name="fullFilename">
		/// The full filename to parse.
		/// </param>
		/// <param name="create">
		/// Value indicating whether the file will be created if it doesn't yet
		/// exist.
		/// </param>
		/// <returns>
		/// List of existing files found.
		/// </returns>
		/// <remarks>
		/// This method does not distinguish a difference between a file and a
		/// directory. That is left to the calling procedure.
		/// </remarks>
		public static List<FileInfo> ResolveFilename(string fullFilename,
			bool create)
		{
			DirectoryInfo dir = null;
			FileInfo file = null;
			List<string> filenames = null;
			List<FileInfo> files = new List<FileInfo>();
			int leftWild = 0;
			int leftPath = 0;
			char[] pathMark = new char[] { '\\', '/' };
			char[] wild = new char[] { '*', '?' };

			if(fullFilename?.Length > 0)
			{
				if(fullFilename.IndexOfAny(wild) > -1)
				{
					//	The filename contains one or more wildcards. Use regular
					//	expressions to chunk the parts.
					leftPath = fullFilename.IndexOfAny(pathMark);
					leftWild = fullFilename.IndexOfAny(wild);
					if(leftPath > -1 && leftPath < leftWild)
					{
						//	A base path is specified.
						filenames = EnumerateFilesAndDirectories(
							fullFilename.Substring(0, leftPath),
							fullFilename.Substring(leftPath + 1));
					}
					else
					{
						//	The entire path contains wildcards.
						filenames = EnumerateFilesAndDirectories("", fullFilename);
					}
					foreach(string filenameItem in filenames)
					{
						file = new FileInfo(filenameItem);
						if(file.Exists)
						{
							files.Add(file);
						}
					}
				}
				else
				{
					//	No wildcards encountered.
					file = new FileInfo(fullFilename);
					if(!file.Exists)
					{
						//	The file doesn't exist.
						dir = new DirectoryInfo(file.FullName);
						if(dir.Exists)
						{
							//	The file exists and is a directory.
							files.Add(file);
						}
						else if(create)
						{
							files.Add(file);
						}
					}
					else
					{
						//	The file exists.
						files.Add(file);
					}
				}
			}
			return files;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ResolveWildcards																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Resolve any wildcards in the caller's working path and filename.
		/// </summary>
		/// <param name="workingPath">
		/// The base working path to use.
		/// </param>
		/// <param name="filename">
		/// The filename that might contain wildcards.
		/// </param>
		/// <returns>
		/// List of resolved filenames identified.
		/// </returns>
		public static List<string> ResolveWildcards(string workingPath,
			string filename)
		{
			char[] backslashes = new char[] { '\\', '/' };
			StringBuilder builder = new StringBuilder();
			int count = 0;
			string endLevel = "";
			FileInfo[] files = null;
			List<string> filenames = new List<string>();
			string fullFilename = "";
			int index = 0;
			string[] levels = null;
			char[] wildcards = new char[] { '*', '?' };
			string workingLevel = "";

			if(filename?.Length > 0)
			{
				fullFilename = AbsolutePath(workingPath, filename);
				levels = fullFilename.Split(backslashes);
				if(levels.Length > 1)
				{
					//	There is at least a base directory and a file.
					endLevel = levels[^1];
					if(endLevel.IndexOfAny(wildcards) > -1)
					{
						//	The end level contains wildcards.
						count = levels.Length;
						for(index = 0; index < count - 1; index++)
						{
							if(builder.Length > 0)
							{
								builder.Append('\\');
							}
							builder.Append(levels[index]);
						}
						workingLevel = builder.ToString();
						if(Directory.Exists(workingLevel))
						{
							files = new DirectoryInfo(workingLevel).GetFiles(endLevel);
							foreach(FileInfo fileItem in files)
							{
								filenames.Add(fileItem.FullName);
							}
						}
					}
					else
					{
						//	Use the full filename to get the path.
						if(File.Exists(fullFilename) || Directory.Exists(fullFilename))
						{
							filenames.Add(fullFilename);
						}
					}
				}
			}
			return filenames;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SaveBitmap																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Save the bitmap to the specified file.
		/// </summary>
		/// <param name="bitmap">
		/// Reference to the bitmap to be saved.
		/// </param>
		/// <param name="filename">
		/// The fully qualified path and filename to which the file will be
		/// saved.
		/// </param>
		public static void SaveBitmap(SKBitmap bitmap, string filename)
		{
			if(bitmap != null && filename?.Length > 0)
			{
				using(SKImage image = SKImage.FromBitmap(bitmap))
				{
					using(SKData data = image.Encode())
					{
						using(FileStream stream = File.Create(filename))
						{
							data.SaveTo(stream);
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ToBool																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Provide fail-safe conversion of string to boolean value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <returns>
		/// Boolean value. False if not convertible.
		/// </returns>
		public static bool ToBool(object value)
		{
			bool result = false;
			if(value != null)
			{
				result = ToBool(value.ToString());
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Provide fail-safe conversion of string to boolean value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <param name="defaultValue">
		/// The default value to return if the value was not present.
		/// </param>
		/// <returns>
		/// Boolean value. False if not convertible.
		/// </returns>
		public static bool ToBool(string value, bool defaultValue = false)
		{
			//	A try .. catch block was originally implemented here, but the
			//	following text was being sent to output on each unsuccessful
			//	match.
			//	Exception thrown: 'System.FormatException' in mscorlib.dll
			bool result = false;

			if(value?.Length > 0)
			{
				result = Regex.IsMatch(value, ResourceMain.rxBoolTrue);
			}
			else
			{
				result = defaultValue;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ToInt																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Provide fail-safe conversion of string to numeric value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <returns>
		/// Int32 value. 0 if not convertible.
		/// </returns>
		public static int ToInt(object value)
		{
			int result = 0;
			if(value != null)
			{
				result = ToInt(value.ToString());
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Provide fail-safe conversion of string to numeric value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <returns>
		/// Int32 value. 0 if not convertible.
		/// </returns>
		public static int ToInt(string value)
		{
			int result = 0;
			try
			{
				result = int.Parse(value);
			}
			catch { }
			return result;
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
