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
	public class MemoryModelsRepository : IModelsRepository {
		protected Dictionary<SegmentInfo, Model> _storage;

		public MemoryModelsRepository() {
			_storage = new Dictionary<SegmentInfo, Model>();
		}

		public void AddModel(Model model) {
			if (_storage.ContainsKey(model.Segment) == false)
				_storage.Add(model.Segment, model);
			else
				_storage[model.Segment] = model;
		}

		public Model GetModel(SegmentInfo segment) {
			if (_storage.ContainsKey(segment))
				return _storage[segment];
			else
				return null;
		}

		public IEnumerable<Model> GetModels() {
			return _storage.Values;
		}

		public virtual void Commit() {
			;
		}

	}
}
