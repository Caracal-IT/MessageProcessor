namespace Caracal.IO.MessageProcessor;

public interface IDevice {
  event EventHandler<MessageEventArgs> MessageReceived; 
  byte[] RequestOldPacket(byte packetId);
  void PostTspv(byte[] status, byte date, float pv);
}

public class MessageEventArgs: EventArgs {
  public byte[] Message { get; }

  public MessageEventArgs(byte[] message) => Message = message;
}