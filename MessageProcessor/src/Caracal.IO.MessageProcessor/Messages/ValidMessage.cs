namespace Caracal.IO.MessageProcessor.Messages; 

public sealed class ValidMessage: Message {
  private readonly byte[] _packet;
  
  public byte Version => _packet[0];
  public byte PacketId => _packet[1];
  
  public DateTime Date => _packet[2..6].GetDateFromEpoch();

  public ValidMessage(byte[] packet) {
    _packet = packet;
  }
}