using StereoKit;

using System;
using System.Collections.Generic;
using System.Numerics;

using VisualScript;

namespace VisualScript;

public class App
{
	public static SKSettings Settings => new() {
		appName = "VisualScript",
		assetsFolder = "Assets",
		displayPreference = DisplayMode.MixedReality
	};

	private readonly List<IWorldObject> _worldObjects = new();

	public T AddWorldObject<T>(Type type) where T : class, IWorldObject {
		if (type is null) {
			return null;
		}
		if (!typeof(T).IsAssignableFrom(type)) {
			return null;
		}
		var e = (T)Activator.CreateInstance(type);
		e.Init(this);
		return e;
	}

	public WorldObject AddWorldObject(Type type) {
		return AddWorldObject<WorldObject>(type);
	}

	public T AddWorldObject<T>() where T : class, IWorldObject, new() {
		var e = new T();
		e.Init(this);
		return e;
	}

	private int _currentID = 0;

	internal int Add(IWorldObject worldObject) {
		lock (_worldObjects) {
			_worldObjects.Add(worldObject);
			return _currentID++;
		}
	}
	internal void Remove(IWorldObject worldObject) {
		lock (_worldObjects) {
			_worldObjects.Remove(worldObject);
		}
	}
	public int WorldObjectCount() {
		lock (_worldObjects) {
			return _worldObjects.Count;
		}
	}

	public void Init() {
		Player = new Player(this);
		AddWorldObject<Floor>();
	}

	public Player Player { get; private set; }

	public void Step() {
		var amount = _worldObjects.Count - 1;
		for (var i = amount; i >= 0; i--) {
			try {
				_worldObjects[i].Step();
			}
			catch (Exception ex) {
				Log.Err("Step Failed ERROR:" + ex);
			}
		}
		Player.Step();
	}
}