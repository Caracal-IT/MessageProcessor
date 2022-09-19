using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Caracal.IO.MessageProcessor.Tests.Unit; 

public class AProcessor: IDisposable {
	private readonly IDevice _device;
	private readonly ILogger<Processor> _logger;
	private readonly CancellationTokenSource _cancellationToken;
	
	private byte[] _defaultPacket =  
	{
		0x01, // Version 
		0x04, // Packet Id
		0x62, 0x55, 0x76, 0x5E, // Offset in seconds
		0x02, 0x20, 0x02, 0x4D, 0x06, 0x9E, 0x3F, // TSPV 1
		0x03, 0x10, 0x0A, 0x4D, 0x06, 0x9E, 0x3F // TSPV 2
	};
	
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
		_device.RequestOldPacket(0).Returns(_defaultPacket);
		
		// Act
		await Processor.ProcessAsync(_logger, _device, _cancellationToken.Token);
		
		// Assert
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x02 && b[1] == 0x20), 0x02, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
		_device.Received(1).PostTspv(Arg.Is<byte[]>(b => b[0] == 0x03 && b[1] == 0x10), 0x0A, Arg.Is<float>(v => v > 1.2345 && v < 1.2346));
	}

	[Fact]
	public async Task ShouldProcessTwoSequentialMessages() {
		// Arrange
		_device.RequestOldPacket(0).Returns(_defaultPacket);
		
		var secondPacket = (byte[]) _defaultPacket.Clone();
		secondPacket[1] = 0x05;
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
		_device.RequestOldPacket(0).Returns(_defaultPacket);
		
		var secondPacket = (byte[]) _defaultPacket.Clone();
		secondPacket[1] = 0x05;
		secondPacket[6] = 0x08;
		secondPacket[8] = 0x0B;
		secondPacket[13] = 0x09;
		secondPacket[15] = 0x0B;
		_device.RequestOldPacket(5).Returns(secondPacket);
		
		var thirdPacket = (byte[]) _defaultPacket.Clone();
		thirdPacket[1] = 0x06;
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