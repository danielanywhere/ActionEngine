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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActionEngine
{
	//*-------------------------------------------------------------------------*
	//*	ActionEngineBase																												*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Base functionality for the ActionEngine.
	/// </summary>
	public class ActionEngineBase
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


	}
	//*-------------------------------------------------------------------------*
}
