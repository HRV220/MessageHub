CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "connected_accounts" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_connected_accounts" PRIMARY KEY AUTOINCREMENT,
    "channel_type" TEXT NOT NULL,
    "account_id" TEXT NULL,
    "email" TEXT NULL,
    "display_name" TEXT NOT NULL,
    "status" TEXT NOT NULL,
    "initial_history_depth" TEXT NOT NULL,
    "sync_enabled" INTEGER NOT NULL,
    "connected_at" INTEGER NOT NULL,
    "last_sync_at" INTEGER NULL
);

CREATE TABLE "relation_change_logs" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_relation_change_logs" PRIMARY KEY AUTOINCREMENT,
    "operation" TEXT NOT NULL,
    "person_id" INTEGER NULL,
    "related_person_id" INTEGER NULL,
    "channel_contact_id" INTEGER NULL,
    "proposal_id" INTEGER NULL,
    "snapshot" TEXT NULL,
    "created_at" INTEGER NOT NULL
);

CREATE TABLE "user_profiles" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_user_profiles" PRIMARY KEY,
    "username" TEXT NOT NULL,
    "password_hash" BLOB NOT NULL,
    "password_salt" BLOB NOT NULL,
    "kdf_params" TEXT NOT NULL,
    "created_at" INTEGER NOT NULL,
    "updated_at" INTEGER NOT NULL
);

CREATE TABLE "settings" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_settings" PRIMARY KEY AUTOINCREMENT,
    "scope" TEXT NOT NULL,
    "connected_account_id" INTEGER NULL,
    "key" TEXT NOT NULL,
    "value" TEXT NOT NULL,
    "updated_at" INTEGER NOT NULL,
    CONSTRAINT "FK_settings_connected_accounts_connected_account_id" FOREIGN KEY ("connected_account_id") REFERENCES "connected_accounts" ("id") ON DELETE CASCADE
);

CREATE TABLE "channel_contacts" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_channel_contacts" PRIMARY KEY AUTOINCREMENT,
    "person_id" INTEGER NOT NULL,
    "connected_account_id" INTEGER NOT NULL,
    "external_id" TEXT NOT NULL,
    "username" TEXT NULL,
    "display_name" TEXT NULL,
    "phone" TEXT NULL,
    "email" TEXT NULL,
    "created_at" INTEGER NOT NULL,
    CONSTRAINT "FK_channel_contacts_connected_accounts_connected_account_id" FOREIGN KEY ("connected_account_id") REFERENCES "connected_accounts" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_channel_contacts_person_person_id" FOREIGN KEY ("person_id") REFERENCES "person" ("id") ON DELETE CASCADE
);

CREATE TABLE "conversations" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_conversations" PRIMARY KEY AUTOINCREMENT,
    "connected_account_id" INTEGER NOT NULL,
    "external_id" TEXT NOT NULL,
    "type" TEXT NOT NULL,
    "status" TEXT NOT NULL,
    "title" TEXT NULL,
    "channel_contact_id" INTEGER NULL,
    "last_message_at" INTEGER NULL,
    "last_incoming_at" INTEGER NULL,
    "last_message_preview" varchar(200) NULL,
    "unread_count" INTEGER NOT NULL,
    "added_at" INTEGER NULL,
    "created_at" INTEGER NOT NULL,
    "updated_at" INTEGER NOT NULL,
    CONSTRAINT "FK_conversations_channel_contacts_channel_contact_id" FOREIGN KEY ("channel_contact_id") REFERENCES "channel_contacts" ("id") ON DELETE SET NULL,
    CONSTRAINT "FK_conversations_connected_accounts_connected_account_id" FOREIGN KEY ("connected_account_id") REFERENCES "connected_accounts" ("id") ON DELETE CASCADE
);

CREATE TABLE "person" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_person" PRIMARY KEY AUTOINCREMENT,
    "first_name" TEXT NULL,
    "last_name" TEXT NULL,
    "display_name" TEXT NOT NULL,
    "search_name" TEXT NOT NULL,
    "phone" TEXT NULL,
    "email" TEXT NULL,
    "note" TEXT NULL,
    "preferred_channel_contact_id" INTEGER NULL,
    "created_at" INTEGER NOT NULL,
    "updated_at" INTEGER NOT NULL,
    CONSTRAINT "FK_person_channel_contacts_preferred_channel_contact_id" FOREIGN KEY ("preferred_channel_contact_id") REFERENCES "channel_contacts" ("id") ON DELETE SET NULL
);

CREATE TABLE "conversation_participants" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_conversation_participants" PRIMARY KEY AUTOINCREMENT,
    "conversation_id" INTEGER NOT NULL,
    "external_id" TEXT NOT NULL,
    "display_name" TEXT NULL,
    "channel_contact_id" INTEGER NULL,
    CONSTRAINT "FK_conversation_participants_channel_contacts_channel_contact_id" FOREIGN KEY ("channel_contact_id") REFERENCES "channel_contacts" ("id") ON DELETE SET NULL,
    CONSTRAINT "FK_conversation_participants_conversations_conversation_id" FOREIGN KEY ("conversation_id") REFERENCES "conversations" ("id") ON DELETE CASCADE
);

CREATE TABLE "sync_states" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_sync_states" PRIMARY KEY AUTOINCREMENT,
    "connected_account_id" INTEGER NOT NULL,
    "conversation_id" INTEGER NULL,
    "kind" TEXT NOT NULL,
    "status" TEXT NOT NULL,
    "cursor" TEXT NULL,
    "history_from" INTEGER NULL,
    "last_success_at" INTEGER NULL,
    "last_error_at" INTEGER NULL,
    "retry_count" INTEGER NOT NULL,
    "next_retry_at" INTEGER NULL,
    "updated_at" INTEGER NOT NULL,
    CONSTRAINT "FK_sync_states_connected_accounts_connected_account_id" FOREIGN KEY ("connected_account_id") REFERENCES "connected_accounts" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_sync_states_conversations_conversation_id" FOREIGN KEY ("conversation_id") REFERENCES "conversations" ("id") ON DELETE CASCADE
);

CREATE TABLE "merge_proposals" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_merge_proposals" PRIMARY KEY AUTOINCREMENT,
    "person_low_id" INTEGER NOT NULL,
    "person_high_id" INTEGER NOT NULL,
    "confidence" TEXT NOT NULL,
    "reasons" TEXT NOT NULL,
    "status" TEXT NOT NULL,
    "created_at" INTEGER NOT NULL,
    "resolved_at" INTEGER NULL,
    CONSTRAINT "FK_merge_proposals_person_person_high_id" FOREIGN KEY ("person_high_id") REFERENCES "person" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_merge_proposals_person_person_low_id" FOREIGN KEY ("person_low_id") REFERENCES "person" ("id") ON DELETE CASCADE
);

CREATE TABLE "messages" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_messages" PRIMARY KEY AUTOINCREMENT,
    "conversation_id" INTEGER NOT NULL,
    "external_id" TEXT NULL,
    "client_message_id" TEXT NULL,
    "channel_type" TEXT NOT NULL,
    "direction" TEXT NOT NULL,
    "sender_participant_id" INTEGER NULL,
    "text" TEXT NOT NULL,
    "attachments_info" TEXT NULL,
    "sent_at" INTEGER NOT NULL,
    "received_at" INTEGER NOT NULL,
    "edited_at" INTEGER NULL,
    "remote_deleted_at" INTEGER NULL,
    "status" TEXT NOT NULL,
    "is_read" INTEGER NOT NULL DEFAULT 0,
    "last_error" TEXT NULL,
    "retry_count" INTEGER NOT NULL DEFAULT 0,
    "next_retry_at" INTEGER NULL,
    CONSTRAINT "FK_messages_conversation_participants_sender_participant_id" FOREIGN KEY ("sender_participant_id") REFERENCES "conversation_participants" ("id") ON DELETE SET NULL,
    CONSTRAINT "FK_messages_conversations_conversation_id" FOREIGN KEY ("conversation_id") REFERENCES "conversations" ("id") ON DELETE CASCADE
);

CREATE INDEX "ix_channel_contacts_email" ON "channel_contacts" ("email") WHERE email IS NOT NULL;

CREATE INDEX "ix_channel_contacts_person" ON "channel_contacts" ("person_id");

CREATE INDEX "ix_channel_contacts_phone" ON "channel_contacts" ("phone") WHERE phone IS NOT NULL;

CREATE INDEX "ix_channel_contacts_username" ON "channel_contacts" ("username") WHERE username IS NOT NULL;

CREATE UNIQUE INDEX "ux_channel_contacts_account_external" ON "channel_contacts" ("connected_account_id", "external_id");

CREATE UNIQUE INDEX "ux_connected_accounts_channel_account" ON "connected_accounts" ("channel_type", "account_id") WHERE account_id IS NOT NULL;

CREATE UNIQUE INDEX "ux_connected_accounts_channel_email" ON "connected_accounts" ("channel_type", "email") WHERE account_id IS NULL;

CREATE INDEX "ix_conversation_participants_channel_contact" ON "conversation_participants" ("channel_contact_id") WHERE channel_contact_id IS NOT NULL;

CREATE UNIQUE INDEX "ux_conversation_participants_conversation_external" ON "conversation_participants" ("conversation_id", "external_id");

CREATE INDEX "ix_conversations_channel_contact" ON "conversations" ("channel_contact_id") WHERE channel_contact_id IS NOT NULL;

CREATE INDEX "ix_conversations_status_last_message" ON "conversations" ("status", "last_message_at");

CREATE UNIQUE INDEX "ux_conversations_account_external" ON "conversations" ("connected_account_id", "external_id");

CREATE INDEX "ix_merge_proposals_high" ON "merge_proposals" ("person_high_id");

CREATE INDEX "ix_merge_proposals_pending" ON "merge_proposals" ("created_at" DESC) WHERE status = 'Pending';

CREATE UNIQUE INDEX "ux_merge_proposals_pair" ON "merge_proposals" ("person_low_id", "person_high_id");

CREATE INDEX "ix_messages_conversation_sent_at" ON "messages" ("conversation_id", "sent_at" DESC, "id" DESC);

CREATE INDEX "ix_messages_send_queue" ON "messages" ("next_retry_at") WHERE status = 'Pending';

CREATE INDEX "IX_messages_sender_participant_id" ON "messages" ("sender_participant_id");

CREATE INDEX "ix_messages_unread" ON "messages" ("conversation_id") WHERE is_read = 0;

CREATE UNIQUE INDEX "ux_messages_client_message_id" ON "messages" ("client_message_id") WHERE client_message_id IS NOT NULL;

CREATE UNIQUE INDEX "ux_messages_conversation_external" ON "messages" ("conversation_id", "external_id") WHERE external_id IS NOT NULL;

CREATE INDEX "ix_person_email" ON "person" ("email") WHERE email IS NOT NULL;

CREATE INDEX "ix_person_phone" ON "person" ("phone") WHERE phone IS NOT NULL;

CREATE UNIQUE INDEX "IX_person_preferred_channel_contact_id" ON "person" ("preferred_channel_contact_id");

CREATE INDEX "ix_person_search_name" ON "person" ("search_name");

CREATE INDEX "ix_relation_change_logs_person" ON "relation_change_logs" ("person_id", "created_at" DESC);

CREATE INDEX "ix_relation_change_logs_related_person" ON "relation_change_logs" ("related_person_id") WHERE related_person_id IS NOT NULL;

CREATE UNIQUE INDEX "ux_settings_account_key" ON "settings" ("connected_account_id", "key") WHERE scope = 'Account';

CREATE UNIQUE INDEX "ux_settings_global_key" ON "settings" ("key") WHERE scope = 'Global';

CREATE INDEX "ix_sync_states_next_retry" ON "sync_states" ("next_retry_at") WHERE next_retry_at IS NOT NULL;

CREATE UNIQUE INDEX "ux_sync_states_account_updates" ON "sync_states" ("connected_account_id") WHERE kind = 'Updates';

CREATE UNIQUE INDEX "ux_sync_states_conversation_history" ON "sync_states" ("conversation_id") WHERE kind = 'History';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260927134931_InitialCreate', '10.0.12');

COMMIT;

