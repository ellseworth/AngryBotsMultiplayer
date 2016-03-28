using System;
using UnityEngine;

public class CachedComponent<T> where T : Component
{
	public static implicit operator T(CachedComponent<T> source) { return source.Value; }

	private GameObject _object;
	private T _cache;

	public T Value
	{
		get
		{
			if(!_cache) _cache = _object.GetComponent<T>();
			return _cache;
		}
	}

	public CachedComponent(GameObject carrier)
	{
		if (!carrier) throw new ArgumentNullException("carrier");
		_object = carrier;
	}
}
