using FluentAssertions;

namespace Caracal.IO.MessageProcessor.Tests.Unit;

public class AMessageParser {
  [Fact]
  public void ShouldReturnNullPacketWhenInputIsNull() {
    // Arrange
    byte[]? invalidPacket = null;
    
    // Act
    var message = MessageParser.Parse(invalidPacket);
    
    // Assert
    message.Should().BeOfType<InvalidPacket>();
  }
}