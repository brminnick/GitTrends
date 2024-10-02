namespace GitTrends.UnitTests;

class MockDeviceInfo : IDeviceInfo
{
	public MockDeviceInfo() => Version = new Version(VersionString);

	public string Model { get; } = "Test";

	public string Manufacturer { get; } = "Test";

	public string Name { get; } = "Test";

	public string VersionString { get; } = "1.0";

	public Version Version { get; }

	public DevicePlatform Platform { get; } = DevicePlatform.Android;

	public DeviceIdiom Idiom { get; } = DeviceIdiom.Phone;

	public DeviceType DeviceType { get; } = DeviceType.Unknown;
}