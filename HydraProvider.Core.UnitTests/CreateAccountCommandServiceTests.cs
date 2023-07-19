using NSubstitute;
using Tgs.HydraProvider;
using Tgs.HydraProvider.ApplicationServices;
using Tgs.SerialNumberGenerator;

namespace HydraProvider.Core.UnitTests;

public class CreateAccountCommandServiceTests
{
	[Fact]
	public async Task 建立一個帳號()
	{
		// Arrange
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeSerialNumberGenerator = Substitute.For<ISerialNumberGenerator>();

		var sut = new CreateAccountCommandService(
			fakeTgsRepository,
			new HashAlgorithmFactory(),
			fakeSerialNumberGenerator);

		var command = new CreateAccountCommand("test", Guid.NewGuid(), "tester", "123");

		var snapshotId = Guid.NewGuid();
		_ = fakeSerialNumberGenerator.GenerateAsync()
			.Returns(snapshotId);

		// Act
		await sut.ExecuteAsync(command);

		// Assert
		_ = fakeTgsRepository.Received(1)
			.AddAccountEventAsync(Arg.Any<AccountEvent>());
		_ = fakeTgsRepository.Received(1)
			.UpsertAccountAsync(
				Arg.Any<AccountSnapshot>(),
				Arg.Is(snapshotId),
				Arg.Any<DateTime>());
	}
}
