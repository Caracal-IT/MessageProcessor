namespace Caracal.IO.MessageProcessor.Tests.Unit.Builders; 

public sealed class PacketBuilder {
  private readonly byte[] _packet;

  private PacketBuilder() {
    _packet = new byte[] {
      0x01, // Version 
      0x04, // Packet Id
      0x62, 0x55, 0x76, 0x5E, // Offset in seconds
      0x02, 0x20, 0x02, 0x4D, 0x06, 0x9E, 0x3F, // TSPV 1
      0x03, 0x10, 0x0A, 0x4D, 0x06, 0x9E, 0x3F // TSPV 2
    };
  }

  public static PacketBuilder CreateDefaultPacket() => new ();
  
  public static PacketBuilder CreateSecondPacket() {
    var packet =  CreateDefaultPacket().WithPacketId(0x05)
															      
                                       .WithFirstTspvStatus(0x08, 0x20)
                                       .WithFirstTspvDateOffset(0x0B)
      
                                       .WithSecondTspvStatus(0x09, 0x10)
                                       .WithSecondTspvDateOffset(0x0B);
    
    return packet;
  }
  
  public static PacketBuilder CreateThirdPacket() {
    var packet =  CreateDefaultPacket().WithPacketId(0x06)
																  
                                       .WithFirstTspvStatus(0x10)
                                       .WithFirstTspvDateOffset(0x0C)
															    
                                       .WithSecondTspvStatus(0x11)
                                       .WithSecondTspvDateOffset(0x0C);
    
    return packet;
  }

  public PacketBuilder WithPacketId(byte id) {
    _packet[1] = id;
    return this;
  }

  public PacketBuilder WithFirstTspvStatus(byte firstBit, byte? secondBit = null) {
    _packet[6] = firstBit;

    if (secondBit.HasValue)
      _packet[7] = secondBit.Value;
    
    return this;
  }

  public PacketBuilder WithFirstTspvDateOffset(byte offset) {
    _packet[8] = offset;
    
    return this;
  }
  
  public PacketBuilder WithSecondTspvStatus(byte firstBit, byte? secondBit = null) {
    _packet[13] = firstBit;

    if (secondBit.HasValue)
      _packet[14] = secondBit.Value;
    
    return this;
  }

  public PacketBuilder WithSecondTspvDateOffset(byte offset) {
    _packet[15] = offset;
    
    return this;
  }

  public byte[] Build() => (byte[]) _packet.Clone();
}