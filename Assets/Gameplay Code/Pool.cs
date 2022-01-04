using System;
using System.Collections.Generic;

public class Pool<T> {
	public PoolDelegate<T> @delegate { private get; set; }
	public PoolProvider<T> @provider { private get; set; }
	
	public Pool() {
		_queue = new Queue<T>();
	}

	private readonly Queue<T> _queue;

	public T Get() {
		T obj;
		
		if (_queue.Count > 0)
			obj = _queue.Dequeue();
		else if (@provider != null)
			obj = @provider.Getter();
		else
			obj = default;
		
		@delegate?.Retrieved(obj);
		return obj;
	}

	public void Return(T obj) {
		@delegate?.Returned(obj);
		_queue.Enqueue(obj);
	}
}

public interface PoolDelegate<T> {
	public void Retrieved(T obj);
	public void Returned(T obj);
}

public interface PoolProvider<T> {
	public Func<T> Getter { get; }
}

public class DefaultPoolDelegate<T> : PoolDelegate<T> {
	private readonly Action<T> _retrieved;
	private readonly Action<T> _returned;

	public DefaultPoolDelegate(Action<T> retrieved, Action<T> returned) {
		_retrieved = retrieved;
		_returned = returned;
	}

	public void Retrieved(T obj) {
		_retrieved?.Invoke(obj);
	}

	public void Returned(T obj) {
		_returned?.Invoke(obj);
	}
}

public class DefaultPoolProvider<T> : PoolProvider<T> {
	public DefaultPoolProvider(Func<T> getter) {
		Getter = getter;
	}
	
	public Func<T> Getter { get; private set; }
}