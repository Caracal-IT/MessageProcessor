namespace Caracal.IO.MessageProcessor; 

public sealed class MessageParser {
  private readonly byte[]? _packet;
  private readonly byte _packetLength;
  private string _lastError;

  private MessageParser(byte[]? packet, byte packetLength) {
    _packet = packet;
    _packetLength = packetLength;
    _lastError = string.Empty;
  }

  public static Message Parse(byte[]? bytes, byte packetLength) => 
    new MessageParser(bytes, packetLength).Parse();

  private Message Parse() {
    if (!IsMessageValid())
      return new InvalidMessage { Error = _lastError };

    return new InvalidMessage() {Error = ""};
  }

  private bool IsMessageValid() {
    if (_packet == null) {
      _lastError = "Packet is null";
      return false;
    }

    if (_packet.Length < _packetLength) {
      _lastError = $"Invalid length {_packet.Length} should be {_packetLength}";
      return false;
    }

    return true;
  }
  
}