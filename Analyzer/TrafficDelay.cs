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
	/// Describes the delay caused by congestions / heavy traffic
	/// </summary>
	public struct TrafficDelayInfo {
		/// <summary>
		/// Gets or sets day(s) when this traffic delay occurs
		/// </summary>
		public DayOfWeek AppliesTo { get; set; }

		/// <summary>
		/// Gets the time of the day, when this traffic delay starts
		/// </summary>
		public TimeSpan From { get; set; }

		/// <summary>
		/// Gets or sets the time of the day, when this traffic delay ends
		/// </summary>
		public TimeSpan To { get; set; }

		/// <summary>
		/// Gets or sets the length of the traffic delay in seconds
		/// </summary>
		public double Delay { get; set; }

		/// <summary>
		/// Returns string representation of the object
		/// </summary>
		/// <returns>The string representation of the object</returns>
		public override string ToString() {
			return string.Format("{0} {1}-{2} # {3}", AppliesTo, From, To, Delay);
		}

		public bool Contains(DateTime time) {
			if ((AppliesTo & DayOfWeekHelper.FromDate(time)) == 0)
				return false;

			return (time.TimeOfDay >= From && time.TimeOfDay <= To);
		}
	}

	/// <summary>
	/// Group and align multiple traffic delays
	/// </summary>
	public class TrafficDelayMap {
		protected const int _minutesIdDay = 24 * 60;

		private int _resolution;
		private double[,] _map;
		private double _freeflow;

		/// <summary>
		/// Creates a new instance of the TrafficDelayMap
		/// </summary>
		/// <param name="resolution">The resolution of the map in minutes</param>
		public TrafficDelayMap(int resolution, double freeflow) {
			if (_minutesIdDay % resolution != 0)
				throw new ArgumentException("Time resolution must divide 24 * 60");

			_resolution = resolution;
			_map = new double[7, _minutesIdDay / resolution];
			for (int i = 0; i < 7; i++) {
				for (int ii = 0; ii < _minutesIdDay / resolution; ii++) {
					_map[i, ii] = double.NegativeInfinity;
				}
			}

			_freeflow = freeflow;
		}

		/// <summary>
		/// Adds delay to the TrafficDelayMap
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="day"></param>
		/// <param name="delay"></param>
		public void AddDelay(TimeSpan from, TimeSpan to, DayOfWeek day, double delay) {
			for (int i = 0; i < DayOfWeekHelper.Days.Length; i++) {
				if ((day & DayOfWeekHelper.Days[i]) > 0) {
					int indexFrom = (int)from.TotalMinutes / _resolution;
					int indexTo = (int)to.TotalMinutes / _resolution;

					for (int ii = indexFrom; ii <= indexTo; ii++) {
						if (_map[i, ii] == double.NegativeInfinity || Math.Abs(_map[i, ii] - delay) / delay < Properties.Settings.Default.MinimalModelDelayDifference / 100.0) {
							_map[i, ii] = delay;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns all delays from the TrafficDelayMap, the similar delays are joined together and algned to the resolution
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TrafficDelayInfo> GetDelays() {
			List<TrafficDelayInfo> result = new List<TrafficDelayInfo>();

			int dayIndex = 0;
			int timeIndex = 0;

			while (MapIsEmpty(ref dayIndex, ref timeIndex) == false) {
				TrafficDelayInfo delay = new TrafficDelayInfo();
				delay.Delay = _map[dayIndex, timeIndex];
				delay.From = new TimeSpan(0, timeIndex * _resolution, 0);

				for (int i = 0; i < 7; i++) {
					if (Math.Abs(_map[i, timeIndex] - delay.Delay) < 0.2 || Math.Abs(_map[i, timeIndex] - delay.Delay) / _freeflow < Properties.Settings.Default.MinimalModelDelayDifference / 100.0)
						delay.AppliesTo |= DayOfWeekHelper.Days[i];
				}
				DayOfWeek timeBinDays = delay.AppliesTo;

				while (timeIndex < _minutesIdDay / _resolution && timeBinDays == delay.AppliesTo) {
					for (int i = 0; i < 7; i++) {
						if ((timeBinDays & DayOfWeekHelper.Days[i]) > 0)
							_map[i, timeIndex] = double.NegativeInfinity;
					}
					
					timeIndex++;

					timeBinDays = 0;
					if (timeIndex < _minutesIdDay / _resolution) {
						for (int i = 0; i < 7; i++) {
							if (Math.Abs(_map[i, timeIndex] - delay.Delay) < 0.2  || Math.Abs(_map[i, timeIndex] - delay.Delay) / _freeflow < Properties.Settings.Default.MinimalModelDelayDifference / 100.0)
								timeBinDays |= DayOfWeekHelper.Days[i];
						}
					}
				}

				delay.To = new TimeSpan(0, timeIndex * _resolution, 0);
				if (delay.Delay > 0.1) {
					result.Add(delay);
				}
			}

			return result;
		}

		/// <summary>
		/// Checks whetre the map is empty and returns index of the first nonempty cell
		/// </summary>
		/// <param name="dayIndex"></param>
		/// <param name="timeIndex"></param>
		/// <returns></returns>
		bool MapIsEmpty(ref int dayIndex, ref int timeIndex) {
			for (int ti = 0; ti < _minutesIdDay / _resolution; ti++) {
				for (int di = 0; di < 7; di++) {
					if (_map[di, ti] > 0) {
						dayIndex = di;
						timeIndex = ti;
						return false;
					}
				}
			}

			return true;
		}
	}
}
