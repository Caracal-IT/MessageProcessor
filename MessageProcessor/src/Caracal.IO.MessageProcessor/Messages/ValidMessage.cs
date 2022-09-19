namespace Caracal.IO.MessageProcessor.Messages; 

public sealed class ValidMessage: Message {
  private readonly byte[] _packet;
  
  public byte Version => _packet[0];
  public byte PacketId => _packet[1];
  
  public DateTime Date => _packet[2..6].GetDateFromEpoch();

  public Tspv[] TspVs { get; init; }

  public ValidMessage(byte[] packet) {
    _packet = packet;
    
    TspVs = new[] {
      new Tspv(_packet[^14..^7]),
      new Tspv(_packet[^7..])
    };
  }
}