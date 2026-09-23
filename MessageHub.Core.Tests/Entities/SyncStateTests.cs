using MessageHub.Core.Entities.SyncState;
using MessageHub.Core.Entities.SyncState.Enums;

namespace MessageHub.Core.Tests.Entities;

public class SyncStateTests
{
  [Fact]
  public void Create_Updates_ReturnsIdleSyncState()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.Updates);

    Assert.Equal(SyncStatus.Idle, state.Status);
    Assert.Null(state.ConversationId);
  }

  [Fact]
  public void Create_WithEmptyConnectedAccountId_Throws()
  {
    Assert.Throws<ArgumentException>(() => SyncState.Create(Guid.Empty, SyncKind.Updates));
  }

  [Fact]
  public void Create_History_WithoutConversationId_Throws()
  {
    Assert.Throws<ArgumentException>(() => SyncState.Create(Guid.NewGuid(), SyncKind.History));
  }

  [Fact]
  public void Create_Updates_WithConversationId_Throws()
  {
    Assert.Throws<ArgumentException>(() => SyncState.Create(Guid.NewGuid(), SyncKind.Updates, Guid.NewGuid()));
  }

  [Fact]
  public void Create_History_WithConversationId_ReturnsIdleSyncState()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.History, Guid.NewGuid());

    Assert.Equal(SyncKind.History, state.Kind);
  }

  [Fact]
  public void Start_FromIdle_SetsStatusToRunning()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.Updates);

    state.Start();

    Assert.Equal(SyncStatus.Running, state.Status);
  }

  [Fact]
  public void Start_WhenAlreadyRunning_Throws()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.Updates);
    state.Start();

    Assert.Throws<InvalidOperationException>(() => state.Start());
  }

  [Fact]
  public void RecordSuccess_SetsStatusToDoneAndClearsRetry()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.Updates);
    state.Start();
    state.RecordFailure(DateTime.UtcNow.AddMinutes(1));

    state.RecordSuccess("cursor-1");

    Assert.Equal(SyncStatus.Done, state.Status);
    Assert.Equal("cursor-1", state.Cursor);
    Assert.Equal(0, state.RetryCount);
    Assert.Null(state.NextRetryAt);
  }

  [Fact]
  public void RecordFailure_IncrementsRetryCountAndSetsNextRetryAt()
  {
    var state = SyncState.Create(Guid.NewGuid(), SyncKind.Updates);
    var nextRetryAt = DateTime.UtcNow.AddMinutes(5);

    state.RecordFailure(nextRetryAt);

    Assert.Equal(SyncStatus.Error, state.Status);
    Assert.Equal(1, state.RetryCount);
    Assert.Equal(nextRetryAt, state.NextRetryAt);
  }
}
