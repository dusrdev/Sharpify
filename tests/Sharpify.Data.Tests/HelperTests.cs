using Bogus;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data.Tests;

public class HelperTests {
	[Theory]
	[InlineData(new byte[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new[] { "1", "2", "3", "4", "5", "6" }, 6)]
	public void GetRequiredLength_Unmanaged<T>(T[] data, int expectedLength) {
		var serialized = MemoryPackSerializer.Serialize(data);
		var requiredLength = Helper.GetRequiredLength(serialized);
		Assert.Equal(expectedLength, requiredLength);
	}

	[Fact]
	public void GetRequiredLength_Person() {
		var faker = new Faker();
		var data = Enumerable.Range(1, faker.Random.Int(10, 100)).Select(_ => new Person(faker.Name.FullName(), faker.Random.Int(1, 100))).ToArray();
		var serialized = MemoryPackSerializer.Serialize(data);
		var requiredLength = Helper.GetRequiredLength(serialized);
		Assert.Equal(data.Length, requiredLength);
	}

	[Theory]
	[InlineData(new byte[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new double[] { 1, 2, 3, 4, 5, 6 }, 6)]
	[InlineData(new[] { "1", "2", "3", "4", "5", "6" }, 6)]
	public void ReadToRentedBufferWriter_Unmanaged<T>(T[] data, int expectedLength) {
		var serialized = MemoryPackSerializer.Serialize(data);
		var requiredLength = Helper.GetRequiredLength(serialized);
		var buffer = new RentedBufferWriter<T>(requiredLength + 5);
		try {
			Helper.ReadToRenterBufferWriter(ref buffer, serialized, requiredLength);
			Assert.Equal(expectedLength, buffer.Position);
		} finally {
			buffer?.Dispose();
		}
	}

	[Fact]
	public void ReadToRentedBufferWriter_Person() {
		var faker = new Faker();
		var data = Enumerable.Range(1, faker.Random.Int(10, 100)).Select(_ => new Person(faker.Name.FullName(), faker.Random.Int(1, 100))).ToArray();
		var serialized = MemoryPackSerializer.Serialize(data);
		var requiredLength = Helper.GetRequiredLength(serialized);
		var buffer = new RentedBufferWriter<Person>(requiredLength + 5);
		try {
			Helper.ReadToRenterBufferWriter(ref buffer, serialized, requiredLength);
			Assert.Equal(requiredLength, buffer.Position);
		} finally {
			buffer?.Dispose();
		}
	}
}