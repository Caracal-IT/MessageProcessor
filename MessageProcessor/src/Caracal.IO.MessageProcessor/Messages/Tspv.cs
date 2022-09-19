namespace Caracal.IO.MessageProcessor.Messages; 

public sealed class Tspv {
  private readonly byte[] _packet;
  
  public int Status { get; }
  public int OffSet { get; }
  public float Value { get; }
  
  public Tspv(byte[] packet) {
    _packet = packet;

    Status = packet[..2].ToInt32();
    OffSet = packet[2];
    Value = BitConverter.ToSingle(packet[^4..]);
  }
}