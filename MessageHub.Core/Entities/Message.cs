using System.Threading.Channels;

namespace MessageHub.Core.Entities.Message;

class Message
{
  public Guid Id { get; set; }
  public string ExternalId { get; set; }
  public string Text { get; set; }
  public string Sender { get; set; }
  public DateTime SendedAt { get; set; }
  public string ChannelName { get; set; }
  //TODO: Enum
  public int Status { get; set; }
  //TODO: Enum
  public int Direction { get; set; }
  private Message(Guid Id, string externalId, string text, string sender, DateTime sendedAt, string channelName, int status, int Direction)
  {
    this.Id = Id;
    ExternalId = externalId;
    Text = text;
    Sender = sender;
    SendedAt = sendedAt;
    ChannelName = channelName;
    Status = status;
    this.Direction = Direction;
  }
}