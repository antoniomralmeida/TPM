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
using System.IO;
using System.Xml;

namespace LK.Analyzer {
	public class XmlModelsRepository : MemoryModelsRepository, IDisposable {
		Stream _xmlStream;
		XmlWriter _xmlWriter;
		XmlReader _xmlReader;

		public XmlModelsRepository() {
		}

		public XmlModelsRepository(Stream stream) {
			_xmlStream = stream;
		}

		public XmlModelsRepository(string filename) {
			_xmlStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		}

		public void Save(Stream stream) {
			XmlWriterSettings writerSetting = new XmlWriterSettings();
			writerSetting.Indent = true;

			stream.Seek(0, 0);
			stream.SetLength(0);
			_xmlWriter = XmlTextWriter.Create(new StreamWriter(stream, new UTF8Encoding(false)), writerSetting);

			_xmlWriter.WriteStartElement("models-db");

			foreach (var segment in _storage.Keys) {
				WriteModel(segment);
			}

			_xmlWriter.WriteEndElement();
			_xmlWriter.Close();
		}

		public void Save(string filename) {
			using (Stream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
				Save(stream);
			}
		}

		public void Load(string filename) {
			using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				Load(stream);
			}
		}
		
		public void Load(Stream stream) {
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = true;
			xmlReaderSettings.IgnoreProcessingInstructions = true;
			xmlReaderSettings.IgnoreWhitespace = true;

			try {
				_xmlReader = XmlTextReader.Create(stream, xmlReaderSettings);

				_xmlReader.Read();
				while (false == _xmlReader.EOF) {


					switch (_xmlReader.NodeType) {
						case XmlNodeType.XmlDeclaration:
							_xmlReader.Read();
							continue;

						case XmlNodeType.Element:
							if (_xmlReader.Name != "models-db")
								throw new XmlException("Invalid xml root element. Expected <models-db>.");

							if (_xmlReader.IsEmptyElement == false)
								ReadRootTag();
							return;

						default:
							throw new XmlException();
					}
				}
			}
			finally {
				_xmlReader.Close();
				_xmlReader = null;
			}
		}
		
		protected void Load() {
			Load(_xmlStream);
		}

		private void ReadRootTag() {
			_xmlReader.Read();

			while (_xmlReader.NodeType != XmlNodeType.EndElement) {
				switch (_xmlReader.Name) {
					case "model":
						ReadModel();
						break;
					default:
						_xmlReader.Skip();
						break;
				}
			}
		}

		private void ReadModel() {
			SegmentInfo segment = new SegmentInfo();

			string attFrom = _xmlReader.GetAttribute("node-from");
			if (attFrom == null)
				throw new XmlException("Attribute 'from' is missing.");
			segment.NodeFromID = int.Parse(attFrom);

			string attTo = _xmlReader.GetAttribute("node-to");
			if (attTo == null)
				throw new XmlException("Attribute 'to' is missing.");
			segment.NodeToID = int.Parse(attTo);

			string attWay = _xmlReader.GetAttribute("way");
			if (attFrom == null)
				throw new XmlException("Attribute 'way' is missing.");
			segment.WayID = int.Parse(attWay);

			Model model = new Model();
			model.Segment = segment;

			string attFreeFlow = _xmlReader.GetAttribute("freeflow");
			if(attFreeFlow == null)
				throw new XmlException("Attribute 'freeflow' is missing.");
			model.FreeFlowTravelTime = double.Parse(attFreeFlow, System.Globalization.CultureInfo.InvariantCulture);

			string attAvgDelay = _xmlReader.GetAttribute("avg-delay");
			if (attAvgDelay == null)
				throw new XmlException("Attribute 'avg-delay' is missing.");
			model.AvgDelay = double.Parse(attAvgDelay, System.Globalization.CultureInfo.InvariantCulture);

			string attSignalsDelay = _xmlReader.GetAttribute("signals-delay");
			string attSignalProbability = _xmlReader.GetAttribute("signals-prob");
			if (attSignalsDelay != null && attSignalProbability != null)
				model.TrafficSignalsDelay = new TrafficSignalsDelayInfo() {
					Length = double.Parse(attSignalsDelay, System.Globalization.CultureInfo.InvariantCulture),
					Probability = double.Parse(attSignalProbability, System.Globalization.CultureInfo.InvariantCulture)
				  };
			

			if (false == _xmlReader.IsEmptyElement) {
				_xmlReader.Read();

				while (_xmlReader.NodeType != XmlNodeType.EndElement) {
					switch (_xmlReader.NodeType) {
						case XmlNodeType.Element:
							switch (_xmlReader.Name) {
								case "traffic-delay":
									model.TrafficDelay.Add(ReadTrafficDelay());
									continue;
								default:
									_xmlReader.Skip();
									continue;
							}
						default:
							_xmlReader.Skip();
							break;
					}
				}
			}

			_storage.Add(segment, model);
			_xmlReader.Skip();
		}

		private TrafficDelayInfo ReadTrafficDelay() {
			string attFrom = _xmlReader.GetAttribute("from");
			if (attFrom == null)
				throw new XmlException("Attribute 'from' is missing.");
			TimeSpan from = TimeSpan.Parse(attFrom);

			string attTo = _xmlReader.GetAttribute("to");
			if (attTo == null)
				throw new XmlException("Attribute 'to' is missing.");
			TimeSpan to = TimeSpan.Parse(attTo);

			string attDay = _xmlReader.GetAttribute("day");
			if (attDay == null)
				throw new XmlException("Attribute 'day' is missing.");
			DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), attDay);

			string attDelay = _xmlReader.GetAttribute("delay");
			if (attDelay == null)
				throw new XmlException("Attribute 'delay' is missing.");
			double delay = double.Parse(attDelay, System.Globalization.CultureInfo.InvariantCulture);

			_xmlReader.Skip();

			return new TrafficDelayInfo() { From = from, To = to, Delay = delay, AppliesTo = day };
		}

		protected void Save() {
			Save(_xmlStream);
		}

		void WriteModel(SegmentInfo segment) {
			Model model = _storage[segment];

			if (model.FreeFlowTravelTime == 0)
				return;

			_xmlWriter.WriteStartElement("model");
			_xmlWriter.WriteAttributeString("node-from", segment.NodeFromID.ToString());
			_xmlWriter.WriteAttributeString("node-to", segment.NodeToID.ToString());
			_xmlWriter.WriteAttributeString("way", segment.WayID.ToString());

			_xmlWriter.WriteAttributeString("freeflow", model.FreeFlowTravelTime.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
			_xmlWriter.WriteAttributeString("avg-delay", model.AvgDelay.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
			
			if (model.TrafficSignalsDelay.Probability > 0) {
				_xmlWriter.WriteAttributeString("signals-delay", model.TrafficSignalsDelay.Length.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
				_xmlWriter.WriteAttributeString("signals-prob", model.TrafficSignalsDelay.Probability.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
			}

			foreach (var delay in model.TrafficDelay) {
				WriteDelay(delay);
			}

			_xmlWriter.WriteEndElement();
		}

		void WriteDelay(TrafficDelayInfo delay) {
			_xmlWriter.WriteStartElement("traffic-delay");
			_xmlWriter.WriteAttributeString("from", FormatTimeSpan(delay.From));
			_xmlWriter.WriteAttributeString("to", FormatTimeSpan(delay.To));
			_xmlWriter.WriteAttributeString("day", delay.AppliesTo.ToString());

			_xmlWriter.WriteAttributeString("delay", delay.Delay.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));

			_xmlWriter.WriteEndElement();
		}

		private static string FormatTimeSpan(TimeSpan span) {
			return span.Hours.ToString("00") + ":" +
						 span.Minutes.ToString("00") + ":" +
						 span.Seconds.ToString("00");
		}

		public override void Commit() {
			Save();
		}


		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (_xmlStream != null) {
					_xmlStream.Close();
					_xmlStream.Dispose();
					_xmlStream = null;
				}
			}
		}
	}
}
