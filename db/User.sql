BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "User" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "UserName" TEXT NOT NULL UNIQUE,
    "Password" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    
    "PendingEmail" TEXT NULL,
    "VerificationToken" TEXT NULL,
    "TokenExpiration" TEXT NULL, -- SQLite stores dates as TEXT
    
    "Admin" INTEGER NOT NULL DEFAULT 0,     -- 0=False, 1=True
    "Visible" INTEGER NOT NULL DEFAULT 1,   -- 1=True
    "Approved" INTEGER NOT NULL DEFAULT 0,  -- 0=False
    "CreatedAt" TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_User_UserName" ON "User" ("UserName");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_User_Email" ON "User" ("Email" COLLATE NOCASE);

COMMIT;