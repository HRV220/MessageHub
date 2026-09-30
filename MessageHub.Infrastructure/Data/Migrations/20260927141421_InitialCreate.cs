using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessageHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "connected_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    channel_type = table.Column<string>(type: "TEXT", nullable: false),
                    account_id = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    initial_history_depth = table.Column<string>(type: "TEXT", nullable: false),
                    sync_enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    connected_at = table.Column<long>(type: "INTEGER", nullable: false),
                    last_sync_at = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connected_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relation_change_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    operation = table.Column<string>(type: "TEXT", nullable: false),
                    person_id = table.Column<int>(type: "INTEGER", nullable: true),
                    related_person_id = table.Column<int>(type: "INTEGER", nullable: true),
                    channel_contact_id = table.Column<int>(type: "INTEGER", nullable: true),
                    proposal_id = table.Column<int>(type: "INTEGER", nullable: true),
                    snapshot = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relation_change_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    password_hash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    password_salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    kdf_params = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    scope = table.Column<string>(type: "TEXT", nullable: false),
                    connected_account_id = table.Column<int>(type: "INTEGER", nullable: true),
                    key = table.Column<string>(type: "TEXT", nullable: false),
                    value = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_settings_connected_accounts_connected_account_id",
                        column: x => x.connected_account_id,
                        principalTable: "connected_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_contacts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    person_id = table.Column<int>(type: "INTEGER", nullable: false),
                    connected_account_id = table.Column<int>(type: "INTEGER", nullable: false),
                    external_id = table.Column<string>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: true),
                    display_name = table.Column<string>(type: "TEXT", nullable: true),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_contacts", x => x.id);
                    table.ForeignKey(
                        name: "FK_channel_contacts_connected_accounts_connected_account_id",
                        column: x => x.connected_account_id,
                        principalTable: "connected_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    connected_account_id = table.Column<int>(type: "INTEGER", nullable: false),
                    external_id = table.Column<string>(type: "TEXT", nullable: false),
                    type = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    channel_contact_id = table.Column<int>(type: "INTEGER", nullable: true),
                    last_message_at = table.Column<long>(type: "INTEGER", nullable: true),
                    last_incoming_at = table.Column<long>(type: "INTEGER", nullable: true),
                    last_message_preview = table.Column<string>(type: "varchar(200)", nullable: true),
                    unread_count = table.Column<int>(type: "INTEGER", nullable: false),
                    added_at = table.Column<long>(type: "INTEGER", nullable: true),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversations_channel_contacts_channel_contact_id",
                        column: x => x.channel_contact_id,
                        principalTable: "channel_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_conversations_connected_accounts_connected_account_id",
                        column: x => x.connected_account_id,
                        principalTable: "connected_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    first_name = table.Column<string>(type: "TEXT", nullable: true),
                    last_name = table.Column<string>(type: "TEXT", nullable: true),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    search_name = table.Column<string>(type: "TEXT", nullable: false),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true),
                    note = table.Column<string>(type: "TEXT", nullable: true),
                    preferred_channel_contact_id = table.Column<int>(type: "INTEGER", nullable: true),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_person_channel_contacts_preferred_channel_contact_id",
                        column: x => x.preferred_channel_contact_id,
                        principalTable: "channel_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    conversation_id = table.Column<int>(type: "INTEGER", nullable: false),
                    external_id = table.Column<string>(type: "TEXT", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", nullable: true),
                    channel_contact_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversation_participants_channel_contacts_channel_contact_id",
                        column: x => x.channel_contact_id,
                        principalTable: "channel_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sync_states",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    connected_account_id = table.Column<int>(type: "INTEGER", nullable: false),
                    conversation_id = table.Column<int>(type: "INTEGER", nullable: true),
                    kind = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    cursor = table.Column<string>(type: "TEXT", nullable: true),
                    history_from = table.Column<long>(type: "INTEGER", nullable: true),
                    last_success_at = table.Column<long>(type: "INTEGER", nullable: true),
                    last_error_at = table.Column<long>(type: "INTEGER", nullable: true),
                    retry_count = table.Column<int>(type: "INTEGER", nullable: false),
                    next_retry_at = table.Column<long>(type: "INTEGER", nullable: true),
                    updated_at = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_states", x => x.id);
                    table.ForeignKey(
                        name: "FK_sync_states_connected_accounts_connected_account_id",
                        column: x => x.connected_account_id,
                        principalTable: "connected_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sync_states_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "merge_proposals",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    person_low_id = table.Column<int>(type: "INTEGER", nullable: false),
                    person_high_id = table.Column<int>(type: "INTEGER", nullable: false),
                    confidence = table.Column<string>(type: "TEXT", nullable: false),
                    reasons = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<long>(type: "INTEGER", nullable: false),
                    resolved_at = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merge_proposals", x => x.id);
                    table.ForeignKey(
                        name: "FK_merge_proposals_person_person_high_id",
                        column: x => x.person_high_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_merge_proposals_person_person_low_id",
                        column: x => x.person_low_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    conversation_id = table.Column<int>(type: "INTEGER", nullable: false),
                    external_id = table.Column<string>(type: "TEXT", nullable: true),
                    client_message_id = table.Column<string>(type: "TEXT", nullable: true),
                    channel_type = table.Column<string>(type: "TEXT", nullable: false),
                    direction = table.Column<string>(type: "TEXT", nullable: false),
                    sender_participant_id = table.Column<int>(type: "INTEGER", nullable: true),
                    text = table.Column<string>(type: "TEXT", nullable: false),
                    attachments_info = table.Column<string>(type: "TEXT", nullable: true),
                    sent_at = table.Column<long>(type: "INTEGER", nullable: false),
                    received_at = table.Column<long>(type: "INTEGER", nullable: false),
                    edited_at = table.Column<long>(type: "INTEGER", nullable: true),
                    remote_deleted_at = table.Column<long>(type: "INTEGER", nullable: true),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    is_read = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    last_error = table.Column<string>(type: "TEXT", nullable: true),
                    retry_count = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    next_retry_at = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_conversation_participants_sender_participant_id",
                        column: x => x.sender_participant_id,
                        principalTable: "conversation_participants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_channel_contacts_email",
                table: "channel_contacts",
                column: "email",
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_channel_contacts_person",
                table: "channel_contacts",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_contacts_phone",
                table: "channel_contacts",
                column: "phone",
                filter: "phone IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_channel_contacts_username",
                table: "channel_contacts",
                column: "username",
                filter: "username IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_channel_contacts_account_external",
                table: "channel_contacts",
                columns: new[] { "connected_account_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_connected_accounts_channel_account",
                table: "connected_accounts",
                columns: new[] { "channel_type", "account_id" },
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_connected_accounts_channel_email",
                table: "connected_accounts",
                columns: new[] { "channel_type", "email" },
                unique: true,
                filter: "account_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_channel_contact",
                table: "conversation_participants",
                column: "channel_contact_id",
                filter: "channel_contact_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_conversation_participants_conversation_external",
                table: "conversation_participants",
                columns: new[] { "conversation_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversations_channel_contact",
                table: "conversations",
                column: "channel_contact_id",
                filter: "channel_contact_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_status_last_message",
                table: "conversations",
                columns: new[] { "status", "last_message_at" });

            migrationBuilder.CreateIndex(
                name: "ux_conversations_account_external",
                table: "conversations",
                columns: new[] { "connected_account_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_merge_proposals_high",
                table: "merge_proposals",
                column: "person_high_id");

            migrationBuilder.CreateIndex(
                name: "ix_merge_proposals_pending",
                table: "merge_proposals",
                column: "created_at",
                descending: new bool[0],
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ux_merge_proposals_pair",
                table: "merge_proposals",
                columns: new[] { "person_low_id", "person_high_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_sent_at",
                table: "messages",
                columns: new[] { "conversation_id", "sent_at", "id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_messages_send_queue",
                table: "messages",
                column: "next_retry_at",
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_participant_id",
                table: "messages",
                column: "sender_participant_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_unread",
                table: "messages",
                column: "conversation_id",
                filter: "is_read = 0");

            migrationBuilder.CreateIndex(
                name: "ux_messages_client_message_id",
                table: "messages",
                column: "client_message_id",
                unique: true,
                filter: "client_message_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_messages_conversation_external",
                table: "messages",
                columns: new[] { "conversation_id", "external_id" },
                unique: true,
                filter: "external_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_person_email",
                table: "person",
                column: "email",
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_person_phone",
                table: "person",
                column: "phone",
                filter: "phone IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_person_preferred_channel_contact_id",
                table: "person",
                column: "preferred_channel_contact_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_search_name",
                table: "person",
                column: "search_name");

            migrationBuilder.CreateIndex(
                name: "ix_relation_change_logs_person",
                table: "relation_change_logs",
                columns: new[] { "person_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_relation_change_logs_related_person",
                table: "relation_change_logs",
                column: "related_person_id",
                filter: "related_person_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_settings_account_key",
                table: "settings",
                columns: new[] { "connected_account_id", "key" },
                unique: true,
                filter: "scope = 'Account'");

            migrationBuilder.CreateIndex(
                name: "ux_settings_global_key",
                table: "settings",
                column: "key",
                unique: true,
                filter: "scope = 'Global'");

            migrationBuilder.CreateIndex(
                name: "ix_sync_states_next_retry",
                table: "sync_states",
                column: "next_retry_at",
                filter: "next_retry_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_sync_states_account_updates",
                table: "sync_states",
                column: "connected_account_id",
                unique: true,
                filter: "kind = 'Updates'");

            migrationBuilder.CreateIndex(
                name: "ux_sync_states_conversation_history",
                table: "sync_states",
                column: "conversation_id",
                unique: true,
                filter: "kind = 'History'");

            migrationBuilder.AddForeignKey(
                name: "FK_channel_contacts_person_person_id",
                table: "channel_contacts",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_channel_contacts_connected_accounts_connected_account_id",
                table: "channel_contacts");

            migrationBuilder.DropForeignKey(
                name: "FK_channel_contacts_person_person_id",
                table: "channel_contacts");

            migrationBuilder.DropTable(
                name: "merge_proposals");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "relation_change_logs");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "sync_states");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "connected_accounts");

            migrationBuilder.DropTable(
                name: "person");

            migrationBuilder.DropTable(
                name: "channel_contacts");
        }
    }
}
