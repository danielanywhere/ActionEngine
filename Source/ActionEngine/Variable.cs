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
		public void SetValue(string name, string value)
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
				if(value?.Length > 0)
				{
					item.Value = value;
				}
				else
				{
					item.Value = "";
				}
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
		private string mValue = "";
		/// <summary>
		/// Get/Set the value of the variable.
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
