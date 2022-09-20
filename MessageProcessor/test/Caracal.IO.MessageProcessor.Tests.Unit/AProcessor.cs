namespace Caracal.IO.MessageProcessor.Tests.Unit; 

public class AProcessor: IDisposable {
	private readonly IDevice _device;
	private readonly ILogger<Processor> _logger;
	private readonly CancellationTokenSource _cancellationToken;

	public AProcessor() {
		_device = Substitute.For<IDevice>();
		_logger = Substitute.For<ILogger<Processor>>();
		_cancellationToken = new CancellationTokenSource();
		_cancellationToken.CancelAfter(TimeSpan.FromMilliseconds(10));
	}

	[Fact]
	public async Task ShouldSendRequestForFirstItem() {
		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		
		// Assert
		_device.Received(1).RequestOldPacket(0);
	}

	[Fact]
	public async Task ShouldProcessFirstMessage() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
													    .Build();
		
		_device.RequestOldPacket(0).Returns(packet);
		
		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x02 && b[1] == 0x20), 0x02, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x03 && b[1] == 0x10), 0x0A, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}

	[Fact]
	public async Task ShouldProcessTwoSequentialMessages() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
			                        .Build();
		
		_device.RequestOldPacket(0).Returns(packet);
		
		var secondPacket = PacketBuilder.CreateDefaultPacket()
															      .WithPacketId(0x05)
			                              .Build();
		
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x0B;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x0C;

		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		_device.MessageReceived += Raise.EventWith(this, new MessageEventArgs(secondPacket));
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x08 && b[1] == 0x20), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x09 && b[1] == 0x10), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}
	
	[Fact]
	public async Task ShouldProcessShouldNotProcessOldMessages() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
			                        .Build();
		
		_device.RequestOldPacket(0).Returns(packet);
		
		var secondPacket = PacketBuilder.CreateDefaultPacket()
																    .WithPacketId(0x05)
																	  .Build();
		
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x01;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x01;

		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		_device.MessageReceived += Raise.EventWith(this, new MessageEventArgs(secondPacket));
		
		// Assert
		_device.DidNotReceive().PostTspv(Arg.Is<byte[]>(b => b[0] == 0x08 && b[1] == 0x20), 0x01, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.DidNotReceive().PostTspv(Arg.Is<byte[]>(b => b[0] == 0x09 && b[1] == 0x10), 0x01, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}
	
	[Fact]
	public async Task ShouldProcessTwoSequentialMessagesWithRollover() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
			                        .Build();
		
		packet[1] = 255;
		_device.RequestOldPacket(0).Returns(packet);
		
		var secondPacket = PacketBuilder.CreateDefaultPacket()
																	  .WithPacketId(0x01)
																		.Build();
		
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x0B;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x0C;

		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		_device.MessageReceived += Raise.EventWith(this, new MessageEventArgs(secondPacket));
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x08 && b[1] == 0x20), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x09 && b[1] == 0x10), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}

	[Fact]
	public async Task ShouldProcessMissedMessages() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
			                        .Build();
		
		_device.RequestOldPacket(0).Returns(packet);
		
		var secondPacket = PacketBuilder.CreateDefaultPacket()
																	  .WithPacketId(0x05)
																		.Build();
		
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x0B;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x0B;
		_device.RequestOldPacket(5).Returns(secondPacket);
		
		var thirdPacket =PacketBuilder.CreateDefaultPacket()
																  .WithPacketId(0x06)
															    .Build();
		
		thirdPacket[6] = 0x10;
		thirdPacket[8] = 0x0C;
		thirdPacket[13] = 0x11;
		thirdPacket[15] = 0x0C;
		
		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		_device.MessageReceived += Raise.EventWith(this, new MessageEventArgs(thirdPacket));
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x08 && b[1] == 0x20), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x09 && b[1] == 0x10), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x10 && b[1] == 0x20), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x11 && b[1] == 0x10), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}
	
	[Fact]
	public async Task ShouldProcessMissedMessagesWithRollOver() {
		// Arrange
		var packet = PacketBuilder.CreateDefaultPacket()
			                        .Build();
		
		packet[1] = 255;
		_device.RequestOldPacket(0).Returns(packet);
		
		var secondPacket = PacketBuilder.CreateDefaultPacket()
																	  .WithPacketId(0x01)
																		.Build();
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x0B;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x0B;
		_device.RequestOldPacket(1).Returns(secondPacket);
		
		var thirdPacket = PacketBuilder.CreateDefaultPacket()
																   .WithPacketId(0x02)
																   .Build();
		
		thirdPacket[6] = 0x10;
		thirdPacket[8] = 0x0C;
		thirdPacket[13] = 0x11;
		thirdPacket[15] = 0x0C;
		
		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		_device.MessageReceived += Raise.EventWith(this, new MessageEventArgs(thirdPacket));
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x08 && b[1] == 0x20), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x09 && b[1] == 0x10), 0x0B, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x10 && b[1] == 0x20), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x11 && b[1] == 0x10), 0x0C, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}

	public void Dispose() => _cancellationToken.Dispose();
}