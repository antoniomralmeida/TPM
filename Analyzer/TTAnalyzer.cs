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

using LK.GeoUtils;
using LK.GeoUtils.Geometry;
using LK.OSMUtils.OSMDatabase;

namespace LK.Analyzer {
	/// <summary>
	/// Perform analysis of the travel-times
	/// </summary>
	public class TTAnalyzer {
		OSMDB _map;

		/// <summary>
		/// Creates a new instance of the analyzer
		/// </summary>
		/// <param name="map"></param>
		public TTAnalyzer(OSMDB map) {
			_map = map;
		}

		/// <summary>
		/// Performs analysis of the travel times on the specific segment and creates it's model
		/// </summary>
		/// <param name="travelTimes"></param>
		/// <param name="segment"></param>
		/// <returns></returns>
		public Model Analyze(IEnumerable<TravelTime> travelTimes, SegmentInfo segment) {
			List<TravelTime> filteredTravelTimes = new List<TravelTime>();
			foreach (var tt in travelTimes) {
				if (tt.Stops.Where(stop => stop.Length.TotalMinutes > Properties.Settings.Default.MaximalAllowedStopLength).Count() > 0)
					continue;

				filteredTravelTimes.Add(tt);
			}

			if (filteredTravelTimes.Count < Properties.Settings.Default.FreeflowMinimalCount)
				return null;

			Model result = new Model();
			result.Segment = segment;

			// Free-flow time
			result.FreeFlowTravelTime = EstimateFreeFlowTime(filteredTravelTimes);

			// traffic signals delay
			if (_map.Nodes[segment.NodeToID].Tags.ContainsTag("highway") && _map.Nodes[segment.NodeToID].Tags["highway"].Value == "traffic_signals") {
				result.TrafficSignalsDelay = EstimateTafficSignalsDelay(filteredTravelTimes, segment);
			}

			// traffic delay
			EstimateTrafficDelay(filteredTravelTimes, result);

			return result;
		}

		/// <summary>
		/// Estimates free-flow travel time from the collection of travel times
		/// </summary>
		/// <param name="travelTimes"></param>
		/// <returns></returns>
		double EstimateFreeFlowTime(IEnumerable<TravelTime> travelTimes) {
			int desiredCount = (int)Math.Max(travelTimes.Count() * Properties.Settings.Default.FreeflowPercentage / 100.0, Properties.Settings.Default.FreeflowMinimalCount);
			int count = Math.Min(travelTimes.Count(), desiredCount);

			var toEstimate = travelTimes.OrderBy(tt => tt.TotalTravelTime).Take(count);

			return toEstimate.Sum(tt => tt.TotalTravelTime.TotalSeconds - tt.Stops.Sum(stop => stop.Length.TotalSeconds)) / count;
		}

		/// <summary>
		/// Estimates traffic signals delay from the collection of travel times
		/// </summary>
		/// <param name="travelTimes"></param>
		/// <param name="segment"></param>
		/// <returns></returns>
		TrafficSignalsDelayInfo EstimateTafficSignalsDelay(IEnumerable<TravelTime> travelTimes, SegmentInfo segment) {
			int totalStops = travelTimes.Where(tt => tt.Stops.Count > 0).Count();
			double totalStopsLength = travelTimes.Where(tt => tt.Stops.Count > 0).Sum(tt => tt.Stops.Last().Length.TotalSeconds);

			if (totalStops > 0)
				return new TrafficSignalsDelayInfo() { Probability = (double)totalStops / travelTimes.Count(), Length = totalStopsLength / totalStops };
			else
				return new TrafficSignalsDelayInfo() { Probability = 0, Length = 0 };
		}

		class TravelTimeDelay {
			public TravelTime TravelTime { get; set; }
			public double Delay { get; set; }

			public override string ToString() {
				return Delay.ToString();
			}
		}

		/// <summary>
		/// Estimates traffic delay from the collection of travel times
		/// </summary>
		/// <param name="travelTimes"></param>
		/// <param name="model"></param>
		void EstimateTrafficDelay(IEnumerable<TravelTime> travelTimes, Model model) {
			List<TravelTimeDelay> delays = new List<TravelTimeDelay>();
			foreach (var traveltime in travelTimes) {
				double delay = 0;
				if (model.TrafficSignalsDelay.Probability > 0 && traveltime.Stops.Count > 0)
					delay = traveltime.TotalTravelTime.TotalSeconds - model.FreeFlowTravelTime - traveltime.Stops.Last().Length.TotalSeconds;
				else
					delay = traveltime.TotalTravelTime.TotalSeconds - model.FreeFlowTravelTime;

				delay = Math.Max(0, delay);
				delays.Add(new TravelTimeDelay() { TravelTime = traveltime, Delay = delay });
			}

			// avg delay as median of delays
			delays.Sort(new Comparison<TravelTimeDelay>((TravelTimeDelay td1, TravelTimeDelay td2) => td1.Delay.CompareTo(td2.Delay)));
			model.AvgDelay = delays[delays.Count / 2].Delay;

			TrafficDelayMap delaysMap = new TrafficDelayMap(Properties.Settings.Default.ModelResolution, model.FreeFlowTravelTime);

			List<List<TravelTimeDelay>> travelTimeClusters = null;
			DBScan<TravelTimeDelay> clusterAnalyzer = new DBScan<TravelTimeDelay>(new DBScan<TravelTimeDelay>.FindNeighbours(FindNeighbours));
			for (int i = _parameters.Length -1; i >= 0; i--) {
				_parametersIndex = i;

				int desiredClusterSize = (int)Math.Max(Properties.Settings.Default.MinimalClusterSize, travelTimes.Count() * Properties.Settings.Default.ClusterSizePercentage / 100.0);
				int clusterSize = Math.Min(travelTimes.Count(), desiredClusterSize);
				travelTimeClusters = clusterAnalyzer.ClusterAnalysis(delays, clusterSize);

				foreach (var cluster in travelTimeClusters) {
					TrafficDelayInfo delayInfo = new TrafficDelayInfo();
					if (_parameters[_parametersIndex].Dates == DatesHandling.Any)
						delayInfo.AppliesTo = DayOfWeek.Any;
					else if (_parameters[_parametersIndex].Dates == DatesHandling.WeekendWorkdays)
						delayInfo.AppliesTo = (DayOfWeek.Workday & DayOfWeekHelper.FromDate(cluster[0].TravelTime.TimeStart)) > 0 ? DayOfWeek.Workday : DayOfWeek.Weekend;
					else
						delayInfo.AppliesTo = DayOfWeekHelper.FromDate(cluster[0].TravelTime.TimeStart);

					cluster.Sort(new Comparison<TravelTimeDelay>((TravelTimeDelay td1, TravelTimeDelay td2) => td1.Delay.CompareTo(td2.Delay)));

					delayInfo.Delay = cluster.Sum(tt => tt.Delay) / cluster.Count;
					delayInfo.From = cluster.Min(tt => tt.TravelTime.TimeStart.TimeOfDay).Subtract(new TimeSpan(0, _parameters[_parametersIndex].MembersTimeDifference / 2, 0));
					if (delayInfo.From < new TimeSpan(0))
						delayInfo.From = new TimeSpan(0);
					
					delayInfo.To = cluster.Max(tt => tt.TravelTime.TimeEnd.TimeOfDay).Add(new TimeSpan(0, _parameters[_parametersIndex].MembersTimeDifference / 2, 0));
					if (delayInfo.To > new TimeSpan(23, 59, 59)) {
						delayInfo.To = new TimeSpan(23, 59, 59);
					}
					delaysMap.AddDelay(delayInfo.From, delayInfo.To, delayInfo.AppliesTo, delayInfo.Delay);
				}

				if (travelTimeClusters.Sum(cluster => cluster.Count) > Properties.Settings.Default.ClusterAnalysisStopPercentage * delays.Count / 100)
					break;
			}

			model.TrafficDelay.AddRange(delaysMap.GetDelays());
		}

		int _parametersIndex = 0;
		ClusterParameters[] _parameters = new ClusterParameters[] {
			new ClusterParameters() {DelayDifferencePercentage = 15, Dates = DatesHandling.Any, MembersTimeDifference = 120},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.Any, MembersTimeDifference = 60},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.WeekendWorkdays, MembersTimeDifference = 90},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.WeekendWorkdays, MembersTimeDifference = 60},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.WeekendWorkdays, MembersTimeDifference = 30},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.Days, MembersTimeDifference = 60},
			new ClusterParameters() {DelayDifferencePercentage = 10, Dates = DatesHandling.Days, MembersTimeDifference = 30},
		};

		/// <summary>
		/// Callback function for the DBScan clustering algorithm
		/// </summary>
		/// <param name="target"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		List<TravelTimeDelay> FindNeighbours(TravelTimeDelay target, IList<TravelTimeDelay> items) {
			double eps = _parameters[_parametersIndex].DelayDifferencePercentage * target.TravelTime.TotalTravelTime.TotalSeconds / 100.0;

			List<TravelTimeDelay> result = new List<TravelTimeDelay>();
			for (int i = 0; i < items.Count; i++) {
				if (items[i] != target && Math.Abs(target.Delay - items[i].Delay) < eps && _parameters[_parametersIndex].AreClose(target.TravelTime.TimeStart, items[i].TravelTime.TimeStart))
					result.Add(items[i]);
			}

			return result;
		}

	}
}
