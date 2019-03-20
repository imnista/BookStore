# SQL

Create book store tables in sqlite.

## Table: Book

> `[Id]`: GUID

```sql
CREATE TABLE [Book](
  [Id] GUID CONSTRAINT [PK_Book] PRIMARY KEY NOT NULL, 
  [Name] nvarchar(200), 
  [Quantity] INTEGER NOT NULL, 
  [Image] nvarchar(200), 
  [Description] nvarchar(500), 
  [UserName] nvarchar(256), 
  [CreateTime] DATETIME NOT NULL, 
  [UpdateTime] DATETIME NOT NULL);
```

## Table: BorrowList

> `[Id]`: GUID

```sql
CREATE TABLE [BorrowList](
  [Id] GUID PRIMARY KEY NOT NULL, 
  [BookId] GUID NOT NULL CONSTRAINT [FK_BorrowList_0_0] REFERENCES "_sqliteexpert_temp_table_1"([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION, 
  [Email] NVARCHAR(256) NOT NULL, 
  [BorrowDate] DATETIME NOT NULL, 
  [ReturnDate] DATETIME, 
  [DueDate] DATETIME);
```

- [Id]

  Primary key. GUID.

- [BookId]

  Foreign key, associate with `[Book].[Id]`.

- [Email]

  Borrower's e-mail address. E.g., `bec@example.com`.

- [BorrowDate]

  The date a book is borrowed. E.g., `2019-03-20 09:32:50.1444558`.

- [ReturnDate]

  The date a book is returned. E.g., `2019-06-20 09:32:50.1444558`.

- [DueDate]

  The date a book should be returned. E.g., `2019-07-20 09:32:50.1444558`.
