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
	/// Specifies the day of week, or a group of days
	/// </summary>
	[Flags()]
	public enum DayOfWeek {
		Monday = 1,
		Tuesday = 2,
		Wednesday = 4,
		Thursday = 8,
		Friday = 16,
		Saturday = 32,
		Sunday = 64,

		Weekend = Saturday | Sunday,
		Workday = Monday | Tuesday | Wednesday | Thursday | Friday,

		Any = Weekend | Workday
	}

	/// <summary>
	/// Helper class that contains various functions for working with DayOfWeek type
	/// </summary>
	public static class DayOfWeekHelper {
		public static DayOfWeek[] Days = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
		
		/// <summary>
		/// Creates a new DayOfWeek from the DateTime object
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static DayOfWeek FromDate(DateTime date) {
			switch (date.DayOfWeek) {
				case System.DayOfWeek.Monday: return DayOfWeek.Monday;
				case System.DayOfWeek.Tuesday: return DayOfWeek.Tuesday;
				case System.DayOfWeek.Wednesday: return DayOfWeek.Wednesday;
				case System.DayOfWeek.Thursday: return DayOfWeek.Thursday;
				case System.DayOfWeek.Friday: return DayOfWeek.Friday;
				case System.DayOfWeek.Saturday: return DayOfWeek.Saturday;
				case System.DayOfWeek.Sunday: return DayOfWeek.Sunday;
				default: return DayOfWeek.Any;
			}
		}
	}
}
