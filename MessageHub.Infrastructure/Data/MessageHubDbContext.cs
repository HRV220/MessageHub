using MessageHub.Core.Entities;
using MessageHub.Core.Entities.ConnectedAccount;
using MessageHub.Core.Entities.ConnectedAccount.Enums;
using MessageHub.Core.Entities.ConnectedAccount.ValueObjects;
using MessageHub.Core.Entities.Conversation;
using MessageHub.Core.Entities.MergeProposal;
using MessageHub.Core.Entities.Message;
using MessageHub.Core.Entities.RelationChangeLog;
using MessageHub.Core.Entities.Settings;
using MessageHub.Core.Entities.SyncState;
using MessageHub.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MessageHub.Infrastructure.Data;

public class MessageHubDbContext : DbContext
{
  public DbSet<ConnectedAccount> ConnectedAccounts { get; set; }
  public DbSet<Conversation> Conversations { get; set; }
  public DbSet<MergeProposal> MergeProposals { get; set; }
  public DbSet<Message> Messages { get; set; }
  public DbSet<RelationChangeLog> RelationChangeLogs { get; set; }
  public DbSet<SyncState> SyncStates { get; set; }
  public DbSet<ChannelContact> ChannelContacts { get; set; }
  public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
  public DbSet<Person> Persons { get; set; }
  public DbSet<UserProfile> UserProfiles { get; set; }
  public DbSet<Settings> Settings { get; set; }

  public MessageHubDbContext(DbContextOptions<MessageHubDbContext> options) : base(options)
  {
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder
        .UseSqlite($"Data Source={DatabasePath.Get()}")
        .AddInterceptors(new SqliteConnectionInterceptor());
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ConnectedAccount>(b =>
    {
      b.Property(e => e.ChannelType).HasConversion(c => c.Code, s => ChannelType.From(s));
      b.Property(e => e.Status).HasConversion<string>();
      b.Property(e => e.InitialHistoryDepth).HasConversion<string>();

      b.HasIndex(e => new { e.ChannelType, e.AccountId })
        .IsUnique()
        .HasFilter("account_id IS NOT NULL")
        .HasDatabaseName("ux_connected_accounts_channel_account");

      b.HasIndex(e => new { e.ChannelType, e.Email })
        .IsUnique()
        .HasFilter("account_id IS NULL")
        .HasDatabaseName("ux_connected_accounts_channel_email");
    });

    modelBuilder.Entity<Conversation>(e =>
    {
      e.Property(p => p.Status).HasConversion<string>();
      e.Property(p => p.Type).HasConversion<string>();
      e.Property(p => p.LastMessagePreview).HasColumnType("varchar(200)");

      e.HasOne<ConnectedAccount>()
        .WithMany()
        .HasForeignKey(p => p.ConnectedAccountId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne<ChannelContact>()
        .WithMany()
        .HasForeignKey(p => p.ChannelContactId)
        .OnDelete(DeleteBehavior.SetNull);

      e.HasIndex(p => new { p.ConnectedAccountId, p.ExternalId })
        .IsUnique()
        .HasDatabaseName("ux_conversations_account_external");

      e.HasIndex(p => p.ChannelContactId)
        .HasFilter("channel_contact_id IS NOT NULL")
        .HasDatabaseName("ix_conversations_channel_contact");

      e.HasIndex(p => new { p.Status, p.LastMessageAt })
        .HasDatabaseName("ix_conversations_status_last_message");
    });
    modelBuilder.Entity<MergeProposal>(e =>
    {
      e.Property(p => p.Confidence).HasConversion<string>();
      e.Property(p => p.Status).HasConversion<string>();

      e.HasOne<Person>()
        .WithMany()
        .HasForeignKey(p => p.PersonLowId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne<Person>()
        .WithMany()
        .HasForeignKey(p => p.PersonHighId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(p => new { p.PersonLowId, p.PersonHighId })
      .IsUnique()
      .HasDatabaseName("ux_merge_proposals_pair");

      e.HasIndex(p => new { p.CreatedAt })
      .IsDescending()
      .HasFilter("status = 'Pending'")
      .HasDatabaseName("ix_merge_proposals_pending");

      e.HasIndex(p => new { p.PersonHighId })
      .HasDatabaseName("ix_merge_proposals_high");
    });
    modelBuilder.Entity<Message>(e =>
    {
      e.Property(p => p.Direction).HasConversion<string>();
      e.Property(p => p.Status).HasConversion<string>();
      e.Property(p => p.IsRead).HasDefaultValue(false);
      e.Property(p => p.RetryCount).HasDefaultValue(0);

      e.HasOne<Conversation>()
        .WithMany()
        .HasForeignKey(p => p.ConversationId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne<ConversationParticipant>()
        .WithMany()
        .HasForeignKey(p => p.SenderParticipantId)
        .OnDelete(DeleteBehavior.SetNull);

      e.HasIndex(p => new { p.ConversationId, p.SentAt, p.Id })
      .IsDescending(false, true, true)
      .HasDatabaseName("ix_messages_conversation_sent_at");

      e.HasIndex(p => new { p.ConversationId, p.ExternalId })
      .IsUnique()
      .HasFilter("external_id IS NOT NULL")
      .HasDatabaseName("ux_messages_conversation_external");

      e.HasIndex(p => p.ClientMessageId)
      .IsUnique()
      .HasFilter("client_message_id IS NOT NULL")
      .HasDatabaseName("ux_messages_client_message_id");

      e.HasIndex(p => p.NextRetryAt)
      .HasFilter("status = 'Pending'")
      .HasDatabaseName("ix_messages_send_queue");

      e.HasIndex(p => p.ConversationId)
      .HasFilter("is_read = 0")
      .HasDatabaseName("ix_messages_unread");
    });
    modelBuilder.Entity<RelationChangeLog>(e =>
    {
      e.Property(p => p.Operation).HasConversion<string>();

      // No FK on purpose (see RelationChangeLog XML doc) — append-only, must outlive referenced rows.
      e.HasIndex(p => new { p.PersonId, p.CreatedAt })
        .IsDescending(false, true)
        .HasDatabaseName("ix_relation_change_logs_person");

      e.HasIndex(p => p.RelatedPersonId)
        .HasFilter("related_person_id IS NOT NULL")
        .HasDatabaseName("ix_relation_change_logs_related_person");
    });
    modelBuilder.Entity<Settings>(e =>
    {
      e.Property(p => p.Scope).HasConversion<string>();

      e.HasOne<ConnectedAccount>()
        .WithMany()
        .HasForeignKey(p => p.ConnectedAccountId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(p => p.Key)
        .IsUnique()
        .HasFilter("scope = 'Global'")
        .HasDatabaseName("ux_settings_global_key");

      e.HasIndex(p => new { p.ConnectedAccountId, p.Key })
        .IsUnique()
        .HasFilter("scope = 'Account'")
        .HasDatabaseName("ux_settings_account_key");
    });
    modelBuilder.Entity<SyncState>(e =>
    {
      e.Property(p => p.Kind).HasConversion<string>();
      e.Property(p => p.Status).HasConversion<string>();

      e.HasOne<ConnectedAccount>()
        .WithMany()
        .HasForeignKey(p => p.ConnectedAccountId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne<Conversation>()
        .WithMany()
        .HasForeignKey(p => p.ConversationId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(p => p.ConnectedAccountId)
        .IsUnique()
        .HasFilter("kind = 'Updates'")
        .HasDatabaseName("ux_sync_states_account_updates");

      e.HasIndex(p => p.ConversationId)
        .IsUnique()
        .HasFilter("kind = 'History'")
        .HasDatabaseName("ux_sync_states_conversation_history");

      e.HasIndex(p => p.NextRetryAt)
        .HasFilter("next_retry_at IS NOT NULL")
        .HasDatabaseName("ix_sync_states_next_retry");
    });
    modelBuilder.Entity<ChannelContact>(e =>
    {
      e.HasOne(p => p.Person)
        .WithMany(p => p.ChannelContacts)
        .HasForeignKey(p => p.PersonId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(p => p.ConnectedAccount)
        .WithMany(a => a.ChannelContacts)
        .HasForeignKey(p => p.ConnectedAccountId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasIndex(p => new { p.ConnectedAccountId, p.ExternalId })
        .IsUnique()
        .HasDatabaseName("ux_channel_contacts_account_external");

      e.HasIndex(p => p.PersonId)
        .HasDatabaseName("ix_channel_contacts_person");

      e.HasIndex(p => p.Username)
        .HasFilter("username IS NOT NULL")
        .HasDatabaseName("ix_channel_contacts_username");

      e.HasIndex(p => p.Phone)
        .HasFilter("phone IS NOT NULL")
        .HasDatabaseName("ix_channel_contacts_phone");

      e.HasIndex(p => p.Email)
        .HasFilter("email IS NOT NULL")
        .HasDatabaseName("ix_channel_contacts_email");
    });
    modelBuilder.Entity<ConversationParticipant>(e =>
    {
      e.HasOne<Conversation>()
        .WithMany(c => c.Participants)
        .HasForeignKey(p => p.ConversationId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne<ChannelContact>()
        .WithMany()
        .HasForeignKey(p => p.ChannelContactId)
        .OnDelete(DeleteBehavior.SetNull);

      e.HasIndex(p => new { p.ConversationId, p.ExternalId })
        .IsUnique()
        .HasDatabaseName("ux_conversation_participants_conversation_external");

      e.HasIndex(p => p.ChannelContactId)
        .HasFilter("channel_contact_id IS NOT NULL")
        .HasDatabaseName("ix_conversation_participants_channel_contact");
    });
    modelBuilder.Entity<Person>(e =>
    {
      e.ToTable("person");

      e.HasOne<ChannelContact>()
        .WithOne()
        .HasForeignKey<Person>(p => p.PreferredChannelContactId)
        .OnDelete(DeleteBehavior.SetNull);

      e.HasIndex(p => p.SearchName)
        .HasDatabaseName("ix_person_search_name");

      e.HasIndex(p => p.Phone)
        .HasFilter("phone IS NOT NULL")
        .HasDatabaseName("ix_person_phone");

      e.HasIndex(p => p.Email)
        .HasFilter("email IS NOT NULL")
        .HasDatabaseName("ix_person_email");
    });
    modelBuilder.Entity<UserProfile>(e =>
    {
      // Всегда одна строка с id = 1 (8.3 ТЗ) — не автоинкремент, приложение задаёт значение само.
      e.Property(p => p.Id).ValueGeneratedNever();
    });

    // Идёт последним: снимает snake_case-имена и Unix-ms-конвертер дат с полностью
    // собранной модели — раньше часть свойств/сущностей (например Settings, ChannelType)
    // ещё не существовала в modelBuilder.Model на момент прохода.
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
      if (entity.GetTableName() is string tableName)
      {
        entity.SetTableName(ToSnakeCase(tableName));
      }
      foreach (var property in entity.GetProperties())
      {
        if (property.GetColumnName() is string columnName)
        {
          property.SetColumnName(ToSnakeCase(columnName));
        }

        if (property.ClrType == typeof(DateTime))
        {
          property.SetValueConverter(DateTimeToUnixMsConverter);
        }
        else if (property.ClrType == typeof(DateTime?))
        {
          property.SetValueConverter(NullableDateTimeToUnixMsConverter);
        }
      }
    }
  }

  private static readonly ValueConverter<DateTime, long> DateTimeToUnixMsConverter = new(
    v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
    v => DateTimeOffset.FromUnixTimeMilliseconds(v).UtcDateTime);

  private static readonly ValueConverter<DateTime?, long?> NullableDateTimeToUnixMsConverter = new(
    v => v.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)).ToUnixTimeMilliseconds() : null,
    v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value).UtcDateTime : null);

  private static string ToSnakeCase(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;

    int extra = 0;
    for (int i = 1; i < input.Length; i++)
    {
      if (char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
        extra++;
    }

    return string.Create(input.Length + extra, input, static (span, src) =>
    {
      int pos = 0;
      for (int i = 0; i < src.Length; i++)
      {
        char c = src[i];
        if (i > 0 && char.IsUpper(c) && !char.IsUpper(src[i - 1]))
          span[pos++] = '_';
        span[pos++] = char.ToLowerInvariant(c);
      }
    });
  }
}



