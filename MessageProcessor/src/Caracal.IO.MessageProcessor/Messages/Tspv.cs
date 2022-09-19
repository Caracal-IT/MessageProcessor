namespace Caracal.IO.MessageProcessor.Messages; 

public sealed class Tspv {
  public byte[] Status { get; }
  public byte Offset { get; }
  public DateTime Date { get; }
  public float Value { get; }
  
  public Tspv(byte[] packet, DateTime baseDate) {
    Status = packet[..2];
    Offset = packet[2];
    Date = baseDate.AddSeconds(packet[2]);
    Value = BitConverter.ToSingle(packet[^4..]);
  }
}