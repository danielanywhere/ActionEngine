/*
 * Copyright (c). 2026 Daniel Patterson, MCSD (danielanywhere).
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

namespace ActionEngine
{
	//*-------------------------------------------------------------------------*
	//*	VariableCollection																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of VariableItem Items.
	/// </summary>
	public class VariableCollection : List<VariableItem>
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
		//* GetValue																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the value of the specified object in the variables collection.
		/// </summary>
		/// <param name="name">
		/// Name of the variable to read.
		/// </param>
		/// <returns>
		/// Reference to the specified item value in the collection, if found.
		/// Otherwise, null.
		/// </returns>
		public object GetValue(string name)
		{
			VariableItem item = null;
			object result = null;

			if(name?.Length > 0)
			{
				item = this.FirstOrDefault(x =>
					StringComparer.OrdinalIgnoreCase.Equals(x.Name, name));
				if(item != null)
				{
					result = item.Value;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SetValue																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the value of the specified variable, creating it if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name">
		/// Name of the variable to access.
		/// </param>
		/// <param name="value">
		/// Value to set.
		/// </param>
		public void SetValue(string name, object value)
		{
			VariableItem item = null;

			if(name?.Length > 0)
			{
				item = this.FirstOrDefault(x =>
					StringComparer.OrdinalIgnoreCase.Equals(x.Name, name));
				if(item == null)
				{
					item = new VariableItem()
					{
						Name = name
					};
					this.Add(item);
				}
				item.Value = value;
				//if(value?.Length > 0)
				//{
				//	item.Value = value;
				//}
				//else
				//{
				//	item.Value = "";
				//}
			}
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	VariableItem																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an individual variable name and value.
	/// </summary>
	public class VariableItem
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
		//*	Copy																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a shallow copy of the caller's source variable.
		/// </summary>
		/// <param name="source">
		/// </param>
		/// <returns>
		/// </returns>
		public static VariableItem Copy(VariableItem source)
		{
			VariableItem result = null;

			if(source != null)
			{
				result = new VariableItem()
				{
					Name = source.Name,
					Value = source.Value
				};
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Name">Name</see>.
		/// </summary>
		private string mName = "";
		/// <summary>
		/// Get/Set the name of the variable.
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
		private object mValue = null;
		/// <summary>
		/// Get/Set the value of the variable.
		/// </summary>
		public object Value
		{
			get { return mValue; }
			set { mValue = value; }
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

}
