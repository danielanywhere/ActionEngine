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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionEngine
{
	//*-------------------------------------------------------------------------*
	//*	ActionDocumentCollection																								*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ActionDocumentItem Items.
	/// </summary>
	public class ActionDocumentCollection : List<ActionDocumentItem>
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


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ActionDocumentItem																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an individual action-related document object model.
	/// </summary>
	public class ActionDocumentItem
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
		//*	IsLocal																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="IsLocal">IsLocal</see>.
		/// </summary>
		private bool mIsLocal = false;
		/// <summary>
		/// Get/Set a value indicating whether this document is local.
		/// </summary>
		public bool IsLocal
		{
			get { return mIsLocal; }
			set { mIsLocal = value; }
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
