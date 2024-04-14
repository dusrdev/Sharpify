namespace Sharpify.Data;

/// <summary>
/// Represents a filterable type.
/// </summary>
/// <typeparam name="T">The type of the filterable object.</typeparam>
public interface IFilterableType<T> {
	/// <summary>
	/// Serializes the specified value into a byte array.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	static abstract byte[]? Serialize(T? value);

	/// <summary>
	/// Serializes multiple values into a byte array.
	/// </summary>
	/// <param name="values"></param>
	/// <returns></returns>
	static abstract byte[]? SerializeMany(T[]? values);

	/// <summary>
	/// Deserializes the specified data into a value.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	static abstract T? Deserialize(byte[]? data);

	/// <summary>
	/// Deserializes the specified data into multiple values.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	static abstract T[]? DeserializeMany(byte[]? data);
}
