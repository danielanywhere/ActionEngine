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
	//*	ActionOptionCollection																									*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ActionOptionItem Items.
	/// </summary>
	public class ActionOptionCollection : List<ActionOptionItem>
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
		//*	Add																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Parse and add an option from its text value.
		/// </summary>
		/// <param name="optionText">
		/// Text to parse.
		/// </param>
		/// <returns>
		/// Newly created and added option.
		/// </returns>
		public ActionOptionItem Add(string optionText)
		{
			char[] comma = new char[] { ',' };
			string[] parts = null;
			ActionOptionItem result = new ActionOptionItem();


			if(optionText?.Length > 0)
			{
				//	Text has been provided.
				parts = optionText.Split(comma,
					StringSplitOptions.RemoveEmptyEntries);
				if(parts.Length > 0)
				{
					result.Name = parts[0].Trim();
					if(parts.Length > 1)
					{
						result.Value = parts[1].Trim();
					}
				}
			}
			this.Add(result);
			return result;
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ActionOptionItem																												*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an individual option value.
	/// </summary>
	public class ActionOptionItem
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
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Name">Name</see>.
		/// </summary>
		private string mName = "";
		/// <summary>
		/// Get/Set the name of the option.
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Value																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Value">Value</see>.
		/// </summary>
		private string mValue = "";
		/// <summary>
		/// Get/Set the optional value of the option.
		/// </summary>
		public string Value
		{
			get { return mValue; }
			set { mValue = value; }
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

}
