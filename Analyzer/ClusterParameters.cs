//  Travel Time Analysis project
//  Copyright (C) 2010 Lukas Kabrt
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LK.Analyzer {
	/// <summary>
	/// Defines cluster parameters
	/// </summary>
	public struct ClusterParameters {
		/// <summary>
		/// Defines maximal time difference between two cluster members in minutes
		/// </summary>
		public int MembersTimeDifference;

		/// <summary>
		/// Defines wheter day of week is important
		/// </summary>
		public DatesHandling Dates;

		/// <summary>
		/// Defines maximal delay difference between two cluster members in percents of the travel time
		/// </summary>
		public double DelayDifferencePercentage;

		/// <summary>
		/// Test whether two DateTime objects are close in the terms of this cluster parameters
		/// </summary>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <returns></returns>
		public bool AreClose(DateTime item1, DateTime item2) {
			double minutes = Math.Abs((item1.TimeOfDay - item2.TimeOfDay).TotalMinutes);
			DayOfWeek item1Day = DayOfWeekHelper.FromDate(item1);
			DayOfWeek item2Day = DayOfWeekHelper.FromDate(item2);

			if (Dates == DatesHandling.Days) {
				return (item1.DayOfWeek == item2.DayOfWeek) && (minutes < MembersTimeDifference);
			}
			else if (Dates == DatesHandling.WeekendWorkdays) {
				return (minutes < MembersTimeDifference) &&
							 (((DayOfWeek.Workday & item1Day) > 0 && ((DayOfWeek.Workday & item2Day) > 0)) || ((DayOfWeek.Weekend & item1Day) > 0 && ((DayOfWeek.Weekend & item2Day) > 0)));
			}
			else {
				return (minutes < MembersTimeDifference);
			}
		}
	}

	public enum DatesHandling {
		Days,
		WeekendWorkdays,
		Any
	}
}
