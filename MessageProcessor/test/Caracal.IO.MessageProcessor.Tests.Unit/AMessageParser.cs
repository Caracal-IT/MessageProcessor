using FluentAssertions;

namespace Caracal.IO.MessageProcessor.Tests.Unit;

public class AMessageParser {
  private const byte PacketLength = 20;
  private byte[] _validPacket =  
  {
    0x01, // Version 
    0x04, // Packet Id
    0x62, 0x55, 0x76, 0x5E, // Offset in seconds
    0x02, 0x20, 0x02, 0x7F, 0xFF, 0xFF, 0xFF, // TSPV 1
    0x03, 0x10, 0x0A, 0x7F, 0xFF, 0xFF, 0xFF // TSPV 2
  };
  
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
    
    // 0x62, 0x55, 0x76, 0x5E
    var a = _validPacket[2..6];
    
    // Assert
    message!.Version.Should().Be(_validPacket[0]);
    message.PacketId.Should().Be(_validPacket[1]);
    message.Date.Should().Be(baseTime);
  }
}