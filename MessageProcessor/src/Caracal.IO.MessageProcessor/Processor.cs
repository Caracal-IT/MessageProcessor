namespace Caracal.IO.MessageProcessor; 

public sealed class Processor {
  private byte _currentPacketId = 0;
  private DateTime _lastProcessedDate = DateTime.MinValue;

  private const int PackingSize = 20;
  private readonly IDevice _device;
  private readonly ILogger<Processor> _logger;
  private readonly CancellationToken _cancellationToken;
  
  private Processor(ILogger<Processor> logger, IDevice device, CancellationToken cancellationToken) {
    _device = device;
    _logger = logger;
    _cancellationToken = cancellationToken;
    
    _device.MessageReceived += (_, packet) => ProcessMessage(MessageParser.Parse(packet, PackingSize));
  }

  public static async Task ProcessAsync(ILogger<Processor> logger, IDevice device, CancellationToken cancellationToken) {
    var processor = new Processor(logger, device, cancellationToken);
    await processor.ProcessAsync();
  }

  private async Task ProcessAsync() {
    var packet = _device.RequestOldPacket(0);
    var msg = MessageParser.Parse(packet, PackingSize);

    if (msg is ValidMessage message)
      ProcessFirstMessage(message);
    else
      ProcessMessage((InvalidMessage) msg);

    try {
      while (!_cancellationToken.IsCancellationRequested) {
        await Task.Delay(1000, _cancellationToken);
      }
    }
    catch (TaskCanceledException) {
      _logger.LogInformation("Processor stopped");
    }
  }

  private void ProcessMessage(Message message) { 
    switch (message) {
      case InvalidMessage msg: ProcessMessage(msg); break;
      case ValidMessage msg: ProcessMessage(msg); break;
      default: _logger.LogError(new EventId(2, $"Message of type {message.GetType()} not supported"), "Message not supported"); break;
    }
  }

  private void ProcessFirstMessage(ValidMessage message) {
    _lastProcessedDate = message.TspVs.Last().Date;
    _currentPacketId = message.PacketId;
    SendMessageToDevice(message);
  }

  private void ProcessMessage(ValidMessage message) {
    if (message.TspVs.Last().Date < _lastProcessedDate)
      return;
    
    if (IsNextItem(message)) {
      SendMessageToDevice(message);
      return;
    }

    RequestMissingPackets(message);
    SendMessageToDevice(message);
  }

  private void RequestMissingPackets(ValidMessage message) {
    var numberOfMissingItems = message.PacketId - _currentPacketId;

    if (message.PacketId < _currentPacketId) 
      numberOfMissingItems = (_currentPacketId + message.PacketId) % 255;

    for (var i = 0; i < numberOfMissingItems; i++) {
      ProcessOldMessage(i);
    }
  }

  private void ProcessOldMessage(int index) {
    var id = (byte)  ((_currentPacketId + index) % 255 + 1);
    var msg = MessageParser.Parse(_device.RequestOldPacket(id), PackingSize);

    if(msg is ValidMessage m && m.TspVs.Last().Date < _lastProcessedDate)
      SendMessageToDevice(m);
  }

  private void SendMessageToDevice(ValidMessage message) {
    _currentPacketId = message.PacketId;

    foreach (var tsvp in message.TspVs) {
      _device.PostTspv(tsvp.Status, tsvp.Offset, tsvp.Value);  
    }
    
    _lastProcessedDate = message.TspVs.Last().Date;
  }

  private bool IsNextItem(ValidMessage message) =>
    message.PacketId == _currentPacketId + 1
    || (_currentPacketId == 255 && message.PacketId == 1);

  private void ProcessMessage(InvalidMessage message) =>
    _logger.LogError(new EventId(1, message.Error), "Un unexpected error occured processing the message");
}