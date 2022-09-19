namespace Caracal.IO.MessageProcessor.Messages; 

public sealed class ValidMessage: Message {
  public byte Version { get; }
  public byte PacketId { get; }
  public DateTime Date {get; }
  public Tspv[] TspVs { get; }

  public ValidMessage(byte[] packet) {
    Version = packet[0];
    PacketId = packet[1];
    Date = packet[2..6].GetDateFromEpoch();
    
    TspVs = new[] {
      new Tspv(packet[^14..^7], Date),
      new Tspv(packet[^7..], Date)
    };
  }
}