using FluentAssertions;

namespace Caracal.IO.MessageProcessor.Tests.Unit;

public class AMessageParser {
  private const byte PacketLength = 20;
  private readonly byte[] _validPacket;

  public AMessageParser() =>
    _validPacket = PacketBuilder.CreateDefaultPacket().Build();
  
  [Fact]
  public void ShouldReturnInvalidMessageWhenInputIsNull() {
    // Arrange
    byte[]? invalidPacket = null;
    
    // Act
    var message = MessageParser.Parse(invalidPacket, PacketLength) as InvalidMessage;
    
    // Assert
    message.Should().BeOfType<InvalidMessage>();
    message!.Error.Should().Be("Packet is null");
  }

  [Theory]
  [InlineData(new byte[0])]
  [InlineData(new byte[] { 0x01 })]
  [InlineData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 })]
  [InlineData(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 })]
  public void ShouldReturnInvalidMessageForInvalidPacketLength(byte[] packet) {
    // Act
    var message = MessageParser.Parse(packet, PacketLength) as InvalidMessage;
    
    // Assert
    message.Should().BeOfType<InvalidMessage>();
    message!.Error.Should().Be($"Invalid length {packet.Length} should be {PacketLength}");
  }

  [Fact]
  public void ShouldParseHeaderForValidMessage() {
    // Arrange
    var baseTime = _validPacket[2..6].GetDateFromEpoch();

    // Act
    var message = MessageParser.Parse(_validPacket, PacketLength) as ValidMessage;
    
    // Assert
    message!.Version.Should().Be(_validPacket[0]);
    message.PacketId.Should().Be(_validPacket[1]);
    message.Date.Should().Be(baseTime);
  }

  [Fact]
  public void ShouldParseTspVsForValidMessage() {
    // Arrange
    var baseDateTime = _validPacket[2..6].GetDateFromEpoch();
    
    // Act
    var message = MessageParser.Parse(_validPacket, PacketLength) as ValidMessage;

    // Assert
    message!.TspVs.Should().HaveCount(2);

    message.TspVs.First().Status.Should().BeEquivalentTo(new byte[]{0x02, 0x20});
    message.TspVs.First().Offset.Should().Be(2);
    message.TspVs.First().Date.Should().Be(baseDateTime.AddSeconds(2));
    message.TspVs.First().Value.Should().BeApproximately(1.2345F, 4);
    
    message.TspVs.Last().Status.Should().BeEquivalentTo(new byte[]{0x03, 0x10});
    message.TspVs.Last().Offset.Should().Be(10);
    message.TspVs.Last().Date.Should().Be(baseDateTime.AddSeconds(10));
    message.TspVs.Last().Value.Should().BeApproximately(1.2345F, 4);
  }
}