#!/usr/bin/env python3
"""
Read a user database, bcrypt all plaintext passwords in the 'user' table, and
write the hashes back. Supports SQLite (file path) and Postgres (connection
string). Skips or aborts if passwords look already hashed.
"""

import argparse
import re
import sqlite3
import sys

try:
    import bcrypt
except ImportError:
    print("Error: bcrypt is required. Install with: pip install bcrypt", file=sys.stderr)
    sys.exit(1)

# Bcrypt hashes start with $2a$, $2b$, or $2y$
BCRYPT_PREFIXES = ("$2a$", "$2b$", "$2y$")

IDENT_RE = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")


def looks_like_bcrypt(value: str) -> bool:
    """Return True if the string looks like a bcrypt hash."""
    if not value or len(value) < 7:
        return False
    return value.startswith(BCRYPT_PREFIXES)


def quote_ident(ident: str) -> str:
    """Quote a SQL identifier after validating it's simple/safe."""
    if not IDENT_RE.match(ident):
        raise SystemExit(f"Invalid identifier: {ident!r}")
    return f'"{ident}"'


def target_looks_like_postgres(target: str) -> bool:
    t = (target or "").strip()
    return t.startswith("postgresql://") or t.startswith("postgres://") or "dbname=" in t


def connect_sqlite(path: str) -> sqlite3.Connection:
    return sqlite3.connect(path)


def connect_postgres(dsn: str):
    try:
        import psycopg
    except ImportError:
        print(
            "Error: psycopg is required for Postgres. Install with: pip install psycopg[binary]",
            file=sys.stderr,
        )
        sys.exit(1)
    return psycopg.connect(dsn)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Bcrypt passwords in the 'user' table of a SQLite or Postgres database."
    )
    parser.add_argument(
        "database",
        metavar="DB",
        help="SQLite DB file path OR Postgres connection string",
    )
    db_group = parser.add_mutually_exclusive_group()
    db_group.add_argument(
        "--sqlite",
        action="store_true",
        help="Force treating DB argument as a SQLite file path",
    )
    db_group.add_argument(
        "--postgres",
        action="store_true",
        help="Force treating DB argument as a Postgres connection string",
    )
    parser.add_argument(
        "--table",
        default="user",
        help="Table name (default: user)",
    )
    parser.add_argument(
        "--username-column",
        default="username",
        help="Unique key column (default: username)",
    )
    parser.add_argument(
        "--password-column",
        default="password",
        help="Password column to hash (default: password)",
    )
    parser.add_argument(
        "--rounds",
        type=int,
        default=12,
        help="Bcrypt cost factor (default: 12)",
    )
    args = parser.parse_args()

    table_sql = quote_ident(args.table)
    username_col_sql = quote_ident(args.username_column)
    password_col_sql = quote_ident(args.password_column)

    use_postgres = args.postgres or (not args.sqlite and target_looks_like_postgres(args.database))

    if use_postgres:
        backend = "postgres"
        conn = connect_postgres(args.database)
        select_sql = f"SELECT {username_col_sql}, {password_col_sql} FROM {table_sql}"
        try:
            with conn.cursor() as cur:
                cur.execute(select_sql)
                rows = cur.fetchall()
        except Exception as e:
            print(f"Error reading table '{args.table}': {e}", file=sys.stderr)
            sys.exit(1)
    else:
        backend = "sqlite"
        conn = connect_sqlite(args.database)
        select_sql = f"SELECT {username_col_sql}, {password_col_sql} FROM {table_sql}"
        try:
            cur = conn.execute(select_sql)
            rows = cur.fetchall()
        except sqlite3.OperationalError as e:
            print(f"Error reading table '{args.table}': {e}", file=sys.stderr)
            sys.exit(1)

    if not rows:
        print("No rows in the user table.")
        return

    already_hashed = [
        (r[0], r[1])
        for r in rows
        if looks_like_bcrypt((r[1] or "") if isinstance(r[1], str) else "")
    ]

    if already_hashed:
        print("Some passwords appear to be already bcrypt-encrypted:")
        for username, pwd in already_hashed:
            preview = (pwd[:20] + "…") if len(pwd) > 20 else pwd
            print(f"  - {username}: {preview}")
        reply = input("Abort the process? [y/N]: ").strip().lower()
        if reply in ("y", "yes"):
            print("Aborted.")
            sys.exit(0)

    updated = 0
    if backend == "postgres":
        update_sql = f"UPDATE {table_sql} SET {password_col_sql} = %s WHERE {username_col_sql} = %s"
        try:
            with conn.cursor() as cur:
                for username, plain in rows:
                    if not plain or not isinstance(plain, str):
                        continue
                    if looks_like_bcrypt(plain):
                        continue
                    raw = plain.encode("utf-8")
                    if len(raw) > 72:
                        raw = raw[:72]
                    hashed = bcrypt.hashpw(
                        raw,
                        bcrypt.gensalt(rounds=args.rounds),
                    ).decode("ascii")
                    cur.execute(update_sql, (hashed, username))
                    updated += 1
            conn.commit()
        finally:
            conn.close()
    else:
        update_sql = f"UPDATE {table_sql} SET {password_col_sql} = ? WHERE {username_col_sql} = ?"
        try:
            for username, plain in rows:
                if not plain or not isinstance(plain, str):
                    continue
                if looks_like_bcrypt(plain):
                    continue
                raw = plain.encode("utf-8")
                if len(raw) > 72:
                    raw = raw[:72]
                hashed = bcrypt.hashpw(
                    raw,
                    bcrypt.gensalt(rounds=args.rounds),
                ).decode("ascii")
                conn.execute(update_sql, (hashed, username))
                updated += 1
            conn.commit()
        finally:
            conn.close()
    print(f"Updated {updated} password(s).")


if __name__ == "__main__":
    main()
