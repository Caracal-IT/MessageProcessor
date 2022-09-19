namespace Caracal.IO.MessageProcessor.Messages;

public sealed class InvalidMessage : Message {
  public string Error { get; init; } = null!;
}