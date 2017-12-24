USE PersonalFinanceApplication
GO

ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Account;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Vendor;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Category;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Batch;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Status;
ALTER TABLE Account DROP CONSTRAINT FK_Account_AccountType;
ALTER TABLE Category DROP CONSTRAINT FK_Category_CategoryType;
ALTER TABLE Budget DROP CONSTRAINT FK_Budget_BudgetPeriod;
ALTER TABLE BudgetCategory DROP CONSTRAINT FK_BudgetCategory_Budget;
ALTER TABLE BudgetCategory DROP CONSTRAINT FK_BudgetCategory_Category;
ALTER TABLE VendorCategory DROP CONSTRAINT FK_VendorCategory_Vendor;
ALTER TABLE VendorCategory DROP CONSTRAINT FK_VendorCategory_Category;
ALTER TABLE VendorAbbrev DROP CONSTRAINT FK_VendorAbbrev_Vendor;

GO

--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_VendorID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_CategoryID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_AccountID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_StatusID;
--ALTER TABLE Category DROP CONSTRAINT DFT_Category_CategoryTypeID
--ALTER TABLE Account DROP CONSTRAINT DFT_Account_AccountTypeID

ALTER TABLE [Transaction] DROP CONSTRAINT UniqueTransaction;
ALTER TABLE BudgetPeriod DROP CONSTRAINT NoPeriodDuplicate;

DROP TABLE Vendor;
DROP TABLE AccountType;
DROP TABLE CategoryType;
DROP TABLE Status;
DROP TABLE Batch;
DROP TABLE Category;
DROP TABLE Account;
DROP TABLE VendorAbbrev;
DROP TABLE VendorCategory;
DROP TABLE [Transaction];
DROP TABLE BudgetPeriod;
DROP TABLE Budget;
DROP TABLE BudgetCategory;

GO

CREATE TABLE [Vendor] (
  [VendorID] int identity(0,1),
  [VendorName] nvarchar(50) NOT NULL,
  [VendorNotes] nvarchar(200),
  CONSTRAINT PK_Vendor PRIMARY KEY ([VendorID])
);

CREATE TABLE [AccountType] (
  [AccountTypeID] int identity(0,1),
  [TypeDesc] nvarchar(50) NOT NULL,
  CONSTRAINT PK_AccountType PRIMARY KEY ([AccountTypeID])
);

CREATE TABLE [Account] (
  [AccountID] int identity(0,1),
  [AccountName] nvarchar(50) NOT NULL,
  [AccountTypeID] int,
  CONSTRAINT PK_Account PRIMARY KEY ([AccountID]),
  CONSTRAINT FK_Account_AccountType FOREIGN KEY (AccountTypeID) REFERENCES AccountType(AccountTypeID)
)

CREATE TABLE [CategoryType] (
  [CategoryTypeID] int identity(0,1),
  [TypeDesc] nvarchar(50) NOT NULL,
  CONSTRAINT PK_CategoryType PRIMARY KEY ([CategoryTypeID])
);

CREATE TABLE [Category] (
  [CategoryID] int identity(0,1),
  [CategoryName] nvarchar(50) NOT NULL,
  [CategoryTypeID] int,
  CONSTRAINT PK_Category PRIMARY KEY ([CategoryID]),
  CONSTRAINT FK_Category_CategoryType FOREIGN KEY (CategoryTypeID) REFERENCES CategoryType(CategoryTypeID)
);

CREATE TABLE Batch (
  BatchID int identity(0,1),
  UploadDate datetime,
  SuccessRows int,
  FailedRows int,
  CONSTRAINT PK_Batch PRIMARY KEY (BatchID)
);

CREATE TABLE Status(
	StatusID int identity(0,1),
	StatusDesc nvarchar(50),
	CONSTRAINT PK_Status PRIMARY KEY (StatusID)
);

CREATE TABLE [Transaction] (
  [TransactionID] int identity(1,1),
  [Description] nvarchar(100) NOT NULL,
  [Date] date NOT NULL,
  [Amount] decimal(18,2) NOT NULL,
  [AccountID] int,
  [VendorID] int,
  [CategoryID] int,
  [BatchID] int,
  [StatusID] int,
  [VendorDetected] bit,
  CONSTRAINT PK_Transaction PRIMARY KEY ([TransactionID]),
  CONSTRAINT FK_Transaction_Account FOREIGN KEY (AccountID) REFERENCES Account(AccountID),
  CONSTRAINT FK_Transaction_Vendor FOREIGN KEY (VendorID) REFERENCES Vendor(VendorID),
  CONSTRAINT FK_Transaction_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
  CONSTRAINT FK_Transaction_Batch FOREIGN KEY (BatchID) REFERENCES Batch(BatchID),
  CONSTRAINT FK_Transaction_Status FOREIGN KEY (StatusID) REFERENCES Status(StatusID), 
  CONSTRAINT UniqueTransaction UNIQUE (Description, Date, Amount)
);

CREATE TABLE [BudgetPeriod] (
  [PeriodID] int identity(0,1),
  [PeriodBegin] date NOT NULL,
  [PeriodEnd] date NOT NULL,
  [PeriodName] nvarchar(50) NOT NULL,
  CONSTRAINT PK_BudgetPeriod PRIMARY KEY ([PeriodID]),
  CONSTRAINT NoPeriodDuplicate UNIQUE (PeriodBegin, PeriodEnd)
);

CREATE TABLE [Budget] (
  [BudgetID] int identity(0,1),
  [BudgetName] nvarchar(50) NOT NULL,
  [BudgetAmount] decimal(18,2) NOT NULL,
  [PeriodID] int,
  CONSTRAINT PK_Budget PRIMARY KEY ([BudgetID]),
  CONSTRAINT FK_Budget_BudgetPeriod FOREIGN KEY (PeriodID) REFERENCES BudgetPeriod(PeriodID)
);

CREATE TABLE [BudgetCategory] (
  [BudgetID] int ,
  [CategoryID] int,
  PRIMARY KEY ([BudgetID], CategoryID),
  CONSTRAINT FK_BudgetCategory_Budget FOREIGN KEY (BudgetID) REFERENCES Budget(BudgetID),
  CONSTRAINT FK_BudgetCategory_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

CREATE TABLE [VendorCategory] (
  [VendorID] int,
  [CategoryID] int,
  TransactionCount int,
  CONSTRAINT PK_VendorCategory PRIMARY KEY(VendorID, CategoryID),
  CONSTRAINT FK_VendorCategory_Vendor FOREIGN KEY (VendorID) REFERENCES Vendor(VendorID),
  CONSTRAINT FK_VendorCategory_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

CREATE TABLE [VendorAbbrev] (
  [AbbrevID] int identity(0,1),
  [VendorID] int,
  [Abbrev] nvarchar(50) NOT NULL,
  CONSTRAINT PK_VendorAbbrev PRIMARY KEY ([AbbrevID]),
  CONSTRAINT FK_VendorAbbrev_Vendor FOREIGN KEY (VendorID) REFERENCES Vendor(VendorID)
);

INSERT INTO Status VALUES
('Unconfirmed'),
('Confirmed');

INSERT INTO AccountType VALUES
('Unknown'),
('Checking'),
('Savings'),
('Loan'),
('Credit Card'),
('Money Market');

INSERT INTO Account VALUES
('Unknown', 0),
('R&E USAA Visa', 4),
('R&E USAA Checking', 1),
('R&E USAA Savings', 2),
('Evan UCCU Visa', 4);

INSERT INTO CategoryType VALUES
('Unknown'),
('Food'),
('Transportation'),
('Education'),
('Gifts'),
('Home');

INSERT INTO Category VALUES
('Unknown', 0),
('Groceries', 1),
('Dining', 1),
('Gas', 2),
('Hyundai Maintenance', 2),
('Christmas Gifts', 4);

INSERT INTO Vendor(VendorName) VALUES
('Unknown'),
('Walmart'),
('Target'),
('Amazon'),
('Shell'),
('Winco');

INSERT INTO VendorCategory VALUES
(5, 1, 10),
(3, 5, 4),
(1, 1, 3),
(1, 5, 3),
(4, 3, 2);

INSERT INTO [VendorAbbrev] VALUES
(1, 'WALMART'),
(1, 'WM SUPERCENTER'),
(1, 'WAL-MART'),
(2, 'TARGET'),
(3, 'AMAZON');

