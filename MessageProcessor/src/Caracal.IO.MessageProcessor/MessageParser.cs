namespace Caracal.IO.MessageProcessor; 

public sealed class MessageParser {
  private const byte PacketLength = 20;
  private readonly byte[]? _packet;
  private string _lastError;

  private MessageParser(byte[]? packet) {
    _packet = packet;
    _lastError = string.Empty;
  }

  public static Message Parse(byte[]? bytes) => 
    new MessageParser(bytes).Parse();

  private Message Parse() {
    if (!IsMessageValid())
      return new InvalidMessage { Error = _lastError };

    return new ValidMessage(_packet!);
  }

  private bool IsMessageValid() {
    if (_packet == null) {
      _lastError = "Packet is null";
      return false;
    }

    if (_packet.Length == PacketLength) return true;
    
    _lastError = $"Invalid length {_packet.Length} should be {PacketLength}";
    return false;
  }
}