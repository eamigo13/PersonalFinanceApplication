CREATE TABLE [Vendor] (
  [VendorID] int identity(1,1),
  [VendorName] nvarchar(50) NOT NULL,
  [VendorNotes] nvarchar(200),
  PRIMARY KEY ([VendorID])
);

CREATE TABLE [AccountType] (
  [AccountTypeID] int identity(1,1),
  [TypeDesc] nvarchar(50) NOT NULL,
  PRIMARY KEY ([AccountTypeID])
);

CREATE TABLE [Account] (
  [AccountID] int identity(1,1),
  [AccountName] nvarchar(50) NOT NULL,
  [AccountTypeID] int FOREIGN KEY REFERENCES AccountType(AccountTypeID),
  PRIMARY KEY ([AccountID])
)

CREATE TABLE [CategoryType] (
  [CategoryTypeID] int identity(1,1),
  [TypeDesc] nvarchar(50) NOT NULL,
  PRIMARY KEY ([CategoryTypeID])
);

CREATE TABLE [Category] (
  [CategoryID] int identity(1,1),
  [CategoryName] nvarchar(50) NOT NULL,
  [CategoryTypeID] int FOREIGN KEY REFERENCES CategoryType(CategoryTypeID),
  PRIMARY KEY ([CategoryID])
);

CREATE TABLE [Transaction] (
  [TransactionID] int identity(1,1),
  [Description] nvarchar(100) NOT NULL,
  [Date] date NOT NULL,
  [Amount] decimal(18,2) NOT NULL,
  [AccountID] int FOREIGN KEY REFERENCES Account(AccountID),
  [VendorID] int FOREIGN KEY REFERENCES Vendor(VendorID),
  [CategoryID] int FOREIGN KEY REFERENCES Category(CategoryID),
  PRIMARY KEY ([TransactionID]),
  CONSTRAINT UniqueTransaction UNIQUE (Description, Date, Amount)
);

CREATE TABLE [BudgetPeriod] (
  [PeriodID] int identity(1,1),
  [PeriodBegin] date NOT NULL,
  [PeriodEnd] date NOT NULL,
  [PeriodName] nvarchar(50) NOT NULL,
  PRIMARY KEY ([PeriodID]),
  CONSTRAINT NoPeriodDuplicate UNIQUE (PeriodBegin, PeriodEnd)
);

CREATE TABLE [Budget] (
  [BudgetID] int identity(1,1),
  [BudgetName] nvarchar(50) NOT NULL,
  [BudgetAmount] decimal(18,2) NOT NULL,
  [PeriodID] int FOREIGN KEY REFERENCES BudgetPeriod(PeriodID),
  PRIMARY KEY ([BudgetID])
);

CREATE TABLE [BudgetCategory] (
  [BudgetID] int FOREIGN KEY REFERENCES Budget(BudgetID),
  [CategoryID] int FOREIGN KEY REFERENCES Category(CategoryID),
  PRIMARY KEY ([BudgetID], CategoryID)
);

CREATE TABLE [VendorCategory] (
  [VendorID] int FOREIGN KEY REFERENCES Vendor(VendorID),
  [CategoryID] int FOREIGN KEY REFERENCES Category(CategoryID),
  PRIMARY KEY(VendorID, CategoryID)
);

CREATE TABLE [VendorAbbrev] (
  [AbbrevID] int identity(1,1),
  [VendorID] int FOREIGN KEY REFERENCES Vendor(VendorID),
  [Abbrev] nvarchar(50) NOT NULL,
  PRIMARY KEY ([AbbrevID])
);


