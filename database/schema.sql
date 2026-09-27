CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Appointment" (
        "AppointmentId" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "ServiceId" uuid NOT NULL,
        "ResourceId" uuid NOT NULL,
        "BranchId" uuid NOT NULL,
        "StartUtc" timestamp with time zone NOT NULL,
        "EndUtc" timestamp with time zone NOT NULL,
        "Status" integer NOT NULL,
        "CancellationReason" character varying(500),
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Appointment" PRIMARY KEY ("AppointmentId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "ChannelAccount" (
        "ChannelAccountId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "ExternalId" character varying(200) NOT NULL,
        "DisplayName" character varying(200) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_ChannelAccount" PRIMARY KEY ("ChannelAccountId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Client" (
        "ClientId" uuid NOT NULL,
        "FullName" character varying(200),
        "PhoneNumber" character varying(32) NOT NULL,
        "Email" character varying(320),
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Client" PRIMARY KEY ("ClientId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Conversation" (
        "ConversationId" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "ChannelAccountId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "AssignedStaffId" uuid,
        "StartedAtUtc" timestamp with time zone NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Conversation" PRIMARY KEY ("ConversationId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Resource" (
        "ResourceId" uuid NOT NULL,
        "BranchId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Type" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Resource" PRIMARY KEY ("ResourceId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Service" (
        "ServiceId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Duration" interval NOT NULL,
        "Price" numeric(10,2) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Service" PRIMARY KEY ("ServiceId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Staff" (
        "StaffId" uuid NOT NULL,
        "FullName" character varying(200) NOT NULL,
        "Email" character varying(320) NOT NULL,
        "Role" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Staff" PRIMARY KEY ("StaffId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Tenant" (
        "TenantId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "BusinessType" integer NOT NULL,
        "TimeZoneId" character varying(100) NOT NULL,
        "Status" integer NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Tenant" PRIMARY KEY ("TenantId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "TimeBlock" (
        "TimeBlockId" uuid NOT NULL,
        "ResourceId" uuid NOT NULL,
        "StartUtc" timestamp with time zone NOT NULL,
        "EndUtc" timestamp with time zone NOT NULL,
        "Reason" character varying(500),
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_TimeBlock" PRIMARY KEY ("TimeBlockId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Message" (
        "MessageId" uuid NOT NULL,
        "ConversationId" uuid NOT NULL,
        "Direction" integer NOT NULL,
        "Sender" integer NOT NULL,
        "Body" text NOT NULL,
        "SentAtUtc" timestamp with time zone NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Message" PRIMARY KEY ("MessageId"),
        CONSTRAINT "FK_Message_Conversation" FOREIGN KEY ("ConversationId") REFERENCES "Conversation" ("ConversationId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "AvailabilityRule" (
        "AvailabilityRuleId" uuid NOT NULL,
        "ResourceId" uuid NOT NULL,
        "DayOfWeek" integer NOT NULL,
        "StartTime" time without time zone NOT NULL,
        "EndTime" time without time zone NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_AvailabilityRule" PRIMARY KEY ("AvailabilityRuleId"),
        CONSTRAINT "FK_AvailabilityRule_Resource" FOREIGN KEY ("ResourceId") REFERENCES "Resource" ("ResourceId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE TABLE "Branch" (
        "BranchId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Address" character varying(500) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Branch" PRIMARY KEY ("BranchId"),
        CONSTRAINT "FK_Branch_Tenant" FOREIGN KEY ("TenantId") REFERENCES "Tenant" ("TenantId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Appointment_ResourceId_StartUtc_EndUtc" ON "Appointment" ("ResourceId", "StartUtc", "EndUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Appointment_TenantId_ClientId" ON "Appointment" ("TenantId", "ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_AvailabilityRule_ResourceId_DayOfWeek" ON "AvailabilityRule" ("ResourceId", "DayOfWeek");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Branch_TenantId" ON "Branch" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE UNIQUE INDEX "UQ_ChannelAccount_Type_ExternalId" ON "ChannelAccount" ("Type", "ExternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE UNIQUE INDEX "UQ_Client_Tenant_PhoneNumber" ON "Client" ("TenantId", "PhoneNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Conversation_ClientId" ON "Conversation" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Conversation_TenantId_Status" ON "Conversation" ("TenantId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Message_ConversationId_SentAtUtc" ON "Message" ("ConversationId", "SentAtUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Resource_TenantId_BranchId" ON "Resource" ("TenantId", "BranchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_Service_TenantId" ON "Service" ("TenantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE UNIQUE INDEX "UQ_Staff_Tenant_Email" ON "Staff" ("TenantId", "Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    CREATE INDEX "IX_TimeBlock_ResourceId_StartUtc_EndUtc" ON "TimeBlock" ("ResourceId", "StartUtc", "EndUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260919104908_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260919104908_InitialCreate', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    ALTER TABLE "Staff" ADD "PhoneNumber" character varying(32) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE TABLE "BranchWorkingHours" (
        "BranchWorkingHoursId" uuid NOT NULL,
        "BranchId" uuid NOT NULL,
        "DayOfWeek" integer NOT NULL,
        "OpenTime" time without time zone,
        "CloseTime" time without time zone,
        "IsClosed" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_BranchWorkingHours" PRIMARY KEY ("BranchWorkingHoursId"),
        CONSTRAINT "FK_BranchWorkingHours_Branch" FOREIGN KEY ("BranchId") REFERENCES "Branch" ("BranchId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE TABLE "Faq" (
        "FaqId" uuid NOT NULL,
        "Question" character varying(500) NOT NULL,
        "Answer" character varying(2000) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Faq" PRIMARY KEY ("FaqId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE TABLE "Holiday" (
        "HolidayId" uuid NOT NULL,
        "BranchId" uuid,
        "Date" date NOT NULL,
        "Name" character varying(200) NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        "TenantId" uuid NOT NULL,
        CONSTRAINT "PK_Holiday" PRIMARY KEY ("HolidayId"),
        CONSTRAINT "FK_Holiday_Branch" FOREIGN KEY ("BranchId") REFERENCES "Branch" ("BranchId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE TABLE "LoginOtp" (
        "LoginOtpId" uuid NOT NULL,
        "StaffId" uuid NOT NULL,
        "CodeHash" character varying(128) NOT NULL,
        "ExpiresAtUtc" timestamp with time zone NOT NULL,
        "ConsumedAtUtc" timestamp with time zone,
        "AttemptCount" integer NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LoginOtp" PRIMARY KEY ("LoginOtpId"),
        CONSTRAINT "FK_LoginOtp_Staff" FOREIGN KEY ("StaffId") REFERENCES "Staff" ("StaffId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE UNIQUE INDEX "UQ_Staff_PhoneNumber" ON "Staff" ("PhoneNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE UNIQUE INDEX "UQ_BranchWorkingHours_Branch_DayOfWeek" ON "BranchWorkingHours" ("BranchId", "DayOfWeek");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE INDEX "IX_Faq_Tenant_IsActive" ON "Faq" ("TenantId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE INDEX "IX_Holiday_BranchId" ON "Holiday" ("BranchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE INDEX "IX_Holiday_Tenant_Date" ON "Holiday" ("TenantId", "Date");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    CREATE INDEX "IX_LoginOtp_StaffId" ON "LoginOtp" ("StaffId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922174445_AddAuthDashboardAndSettings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260922174445_AddAuthDashboardAndSettings', '8.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922181830_AddSignupOtp') THEN
    CREATE TABLE "SignupOtp" (
        "SignupOtpId" uuid NOT NULL,
        "PhoneNumber" character varying(32) NOT NULL,
        "CodeHash" character varying(128) NOT NULL,
        "ExpiresAtUtc" timestamp with time zone NOT NULL,
        "ConsumedAtUtc" timestamp with time zone,
        "AttemptCount" integer NOT NULL,
        "CreatedOn" timestamp(0) with time zone NOT NULL,
        "CreatedBy" uuid,
        "ModifiedOn" timestamp(0) with time zone,
        "ModifiedBy" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_SignupOtp" PRIMARY KEY ("SignupOtpId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922181830_AddSignupOtp') THEN
    CREATE INDEX "IX_SignupOtp_PhoneNumber" ON "SignupOtp" ("PhoneNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922181830_AddSignupOtp') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260922181830_AddSignupOtp', '8.0.10');
    END IF;
END $EF$;
COMMIT;

