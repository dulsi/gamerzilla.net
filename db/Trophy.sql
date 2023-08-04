PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Game" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Game" PRIMARY KEY AUTOINCREMENT,
    "ShortName" TEXT NOT NULL,
    "GameName" TEXT NOT NULL,
    "VersionNum" INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS "Image" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Image" PRIMARY KEY AUTOINCREMENT,
    "GameId" INTEGER NOT NULL,
    "TrophyId" INTEGER NOT NULL,
    "Achieved" INTEGER NOT NULL,
    "data" BLOB NULL
);
CREATE TABLE IF NOT EXISTS "Trophy" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Trophy" PRIMARY KEY AUTOINCREMENT,
    "GameId" INTEGER NOT NULL,
    "TrophyName" TEXT NOT NULL,
    "TrophyDescription" TEXT NOT NULL,
    "MaxProgress" INTEGER NOT NULL,
    CONSTRAINT "FK_Trophy_Game_GameId" FOREIGN KEY ("GameId") REFERENCES "Game" ("Id") ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS "UserStat" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserStat" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "GameId" INTEGER NOT NULL,
    "TrophyId" INTEGER NOT NULL,
    "Achieved" INTEGER NOT NULL,
    "Progress" INTEGER NOT NULL,
    CONSTRAINT "FK_UserStat_Game_GameId" FOREIGN KEY ("GameId") REFERENCES "Game" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserStat_Trophy_TrophyId" FOREIGN KEY ("TrophyId") REFERENCES "Trophy" ("Id") ON DELETE CASCADE
);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('Game',0);
INSERT INTO sqlite_sequence VALUES('Trophy',0);
INSERT INTO sqlite_sequence VALUES('Image',0);
INSERT INTO sqlite_sequence VALUES('UserStat',0);
CREATE UNIQUE INDEX "IX_Game_ShortName" ON "Game" ("ShortName");
CREATE INDEX "IX_Trophy_GameId" ON "Trophy" ("GameId");
CREATE INDEX "IX_UserStat_GameId" ON "UserStat" ("GameId");
CREATE INDEX "IX_UserStat_TrophyId" ON "UserStat" ("TrophyId");
COMMIT;
