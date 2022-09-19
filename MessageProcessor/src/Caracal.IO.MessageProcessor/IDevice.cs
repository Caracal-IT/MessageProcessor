namespace Caracal.IO.MessageProcessor;

public interface IDevice {
  event EventHandler<byte[]> MessageReceived; 
  byte[] RequestOldPacket(byte packetId);
  void PostTspv(byte[] status, byte date, float pv);
}