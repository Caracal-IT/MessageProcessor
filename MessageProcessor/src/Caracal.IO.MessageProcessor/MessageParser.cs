namespace Caracal.IO.MessageProcessor; 

public sealed class MessageParser {
  private readonly byte[]? _bytes;

  private MessageParser(byte[]? bytes) => _bytes = bytes;

  public static Packet Parse(byte[]? bytes) => new MessageParser(bytes).Parse();

  private Packet Parse() {
    return new InvalidPacket();
  }
  
}