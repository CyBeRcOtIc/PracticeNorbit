CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE "Projects" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT "PK_Projects" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Projects_Code" UNIQUE ("Code")
);

CREATE TABLE "Tasks" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "ProjectId" UUID NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT "PK_Tasks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Tasks_Projects_ProjectId" FOREIGN KEY ("ProjectId")
        REFERENCES "Projects" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "TimeEntries" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "TaskId" UUID NOT NULL,
    "Date" DATE NOT NULL,
    "Hours" NUMERIC(4,2) NOT NULL,
    "Description" VARCHAR(300) NOT NULL,
    CONSTRAINT "PK_TimeEntries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TimeEntries_Tasks_TaskId" FOREIGN KEY ("TaskId")
        REFERENCES "Tasks" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_TimeEntries_Hours" CHECK ("Hours" > 0 AND "Hours" <= 24)
);

CREATE INDEX "IX_Tasks_ProjectId" ON "Tasks" ("ProjectId");
CREATE INDEX "IX_TimeEntries_TaskId" ON "TimeEntries" ("TaskId");
CREATE INDEX "IX_TimeEntries_Date" ON "TimeEntries" ("Date");