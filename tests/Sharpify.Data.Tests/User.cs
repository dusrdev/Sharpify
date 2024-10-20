
using MemoryPack;

namespace Sharpify.Data.Tests;

public sealed class User : IFilterable<User> {
	public Person Person { get; init; }

	public User(Person person) {
		Person = person;
	}

    public static User Deserialize(ReadOnlySpan<byte> data) {
        var p = MemoryPackSerializer.Deserialize<Person>(data);
		return new(p);
    }

    public static User[]? DeserializeMany(ReadOnlySpan<byte> data) {
        var persons = MemoryPackSerializer.Deserialize<Person[]>(data);
		return persons!.Select(p => new User(p)).ToArray();
    }

    public static byte[]? Serialize(User? value) {
		if (value is null) {
			return null;
		}
        return MemoryPackSerializer.Serialize(value.Person);
    }

    public static byte[]? SerializeMany(User[]? values) {
        if (values is null) {
            return null;
        }
		return MemoryPackSerializer.Serialize(values.Select(v => v.Person).ToArray());
    }
}