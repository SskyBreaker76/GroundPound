﻿using UnityEngine;
using System.Collections;
using Defective.JSON;

namespace GeoJSON {

	[System.Serializable]
	public class PositionObject {
		public float latitude;
		public float longitude;

		public PositionObject() {
		}

		public PositionObject(float pointLongitude, float pointLatitude) {
			this.longitude = pointLongitude;
			this.latitude = pointLatitude;
		}

		public PositionObject(JSONObject jsonObject) {
			longitude = jsonObject.list [0].floatValue;
			latitude = jsonObject.list [1].floatValue;
		}

		public JSONObject Serialize() {

			JSONObject jsonObject = new JSONObject (JSONObject.Type.Array);
			jsonObject.Add (longitude);
			jsonObject.Add (latitude);

			return jsonObject;
		}

		override public string ToString() {
			return longitude + "," + latitude;
		}

		public float[] ToArray() {

			float[] array = new float[2];

			array [0] = longitude;
			array [1] = latitude;

			return array;
		}
	}
}
