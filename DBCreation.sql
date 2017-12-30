USE PersonalFinanceApplication
GO

ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Account;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Vendor;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Category;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Batch;
ALTER TABLE [Transaction] DROP CONSTRAINT FK_Transaction_Status;
ALTER TABLE Account DROP CONSTRAINT FK_Account_AccountType;
ALTER TABLE Category DROP CONSTRAINT FK_Category_CategoryType;
ALTER TABLE BudgetCategory DROP CONSTRAINT FK_BudgetCategory_Budget;
ALTER TABLE BudgetCategory DROP CONSTRAINT FK_BudgetCategory_Category;
ALTER TABLE VendorCategory DROP CONSTRAINT FK_VendorCategory_Vendor;
ALTER TABLE VendorCategory DROP CONSTRAINT FK_VendorCategory_Category;
ALTER TABLE VendorAbbrev DROP CONSTRAINT FK_VendorAbbrev_Vendor;
ALTER TABLE Goal DROP CONSTRAINT FK_Goal_Account;
ALTER TABLE Goal DROP CONSTRAINT FK_Goal_Budget;

GO

--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_VendorID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_CategoryID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_AccountID;
--ALTER TABLE [Transaction] DROP CONSTRAINT DFT_Transaction_StatusID;
--ALTER TABLE Category DROP CONSTRAINT DFT_Category_CategoryTypeID
--ALTER TABLE Account DROP CONSTRAINT DFT_Account_AccountTypeID

ALTER TABLE [Transaction] DROP CONSTRAINT UniqueTransaction;
ALTER TABLE Category DROP CONSTRAINT UniqueCategory;
ALTER TABLE Vendor DROP CONSTRAINT UniqueVendor;

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
DROP TABLE Budget;
DROP TABLE BudgetCategory;
DROP TABLE Goal;

GO

CREATE TABLE [Vendor] (
  [VendorID] int identity(0,1),
  [VendorName] nvarchar(50) NOT NULL,
  [VendorNotes] nvarchar(200),
  CONSTRAINT PK_Vendor PRIMARY KEY ([VendorID]),
  CONSTRAINT UniqueVendor UNIQUE (VendorName)
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
  [DateColumn] int,
  [DescriptionColumn] int,
  [AmountColumn] int,
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
  CONSTRAINT FK_Category_CategoryType FOREIGN KEY (CategoryTypeID) REFERENCES CategoryType(CategoryTypeID),
  CONSTRAINT UniqueCategory UNIQUE (CategoryName)
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


CREATE TABLE [Budget] (
  [BudgetID] int identity(0,1),
  [BudgetName] nvarchar(50) NOT NULL,
  [Description] nvarchar(200),
  [BeginDate] date NOT NULL,
  [EndDate] date NOT NULL,
  CONSTRAINT PK_Budget PRIMARY KEY ([BudgetID]),
);

CREATE TABLE [BudgetCategory] (
  [BudgetID] int,
  [CategoryID] int,
  [Amount] decimal(18,2),
  [UsedAmount] decimal(18,2),
  [RemainingAmount] decimal(18,2),
  PRIMARY KEY ([BudgetID], [CategoryID]),
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

CREATE TABLE [Goal] (
   [GoalID] int identity(1,1),
   [AccountID] int,
   [BudgetID] int,
   [Description] nvarchar(200),
   [BeginningAmount] decimal(18,2),
   [GoalAmount] decimal(18,2),
   CONSTRAINT PK_Goal PRIMARY KEY ([GoalID]),
   CONSTRAINT FK_Goal_Account FOREIGN KEY (AccountID) REFERENCES Account(AccountID),
   CONSTRAINT FK_Goal_Budget FOREIGN KEY (BudgetID) REFERENCES Budget(BudgetID)
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

INSERT INTO Account(AccountName,AccountTypeID) VALUES
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
('Home')
;

INSERT INTO Category(CategoryName) VALUES
('Unknown'),
('Groceries'),
('Dining'),
('Gas'),
('Hyundai Maintenance'),
('Christmas Gifts'),
('General Merchandise'),
('Recreation'),
('Parking'),
('Service'),
('Gifts'),
('Cell Phone'),
('Education'),
('Insurance'),
('Fines/Fees'),
('Tithes/Offerings'),
('Clothing'),
('Entertainment'),
('Travel'),
('Rent'),
('Account Transfer'),
('Income')

;

INSERT INTO Vendor(VendorName) VALUES
('Unknown'),
('Walmart'),
('Target'),
('Amazon'),
('Shell'),
('Winco'),
('Brick Oven'),
('Taqueria El Vaquero'),
('Wendys'),
('Little Caesars'),
('7-Eleven'),
('Saigon Cafe'),
('Cougar Express'),
('Dennys'),
('Mcdonald'),
('Subway'),
('Five Guys'),
('Wild Ginger'),
('Emanuels Fresh Grill'),
('Byu Creamery On Ninth'),
('Chicks Cafe'),
('Rancheritos'),
('Cafe Rio'),
('In-N-Out'),
('Smashburger'),
('Zupas'),
('Moochies'),
('Pizza Pie'),
('Dairy Queen'),
('Costa Vida'),
('Malt Shoppe'),
('Sonny Boys Bbq'),
('Zaxbys'),
('Zeeks'),
('Savannahs Candy Kitchen'),
('Applebees'),
('Tucanos'),
('Papa Johns'),
('Rodizio'),
('JCWs'),
('Kneaders'),
('Balance Rock Eatery'),
('Chick-Fil-A'),
('Taco Amigo'),
('Vending'),
('Saturdays Waffle'),
('Bom Acai'),
('The Mighty Bake'),
('Del Taco'),
('Sodalicious'),
('Coca Cola'),
('Swig'),
('Arbys'),
('Baskin'),
('Betos'),
('Taqueria 27'),
('Sushi Burrito'),
('Cream'),
('Einstein Bros Bagels'),
('Barneys'),
('Berkeley Thai House'),
('Maverik'),
('Chevron'),
('Phillips 66'),
('Last Chance Store'),
('Harts'),
('Texaco'),
('Sinclair'),
('Mountainland One Stop'),
('Hollow Mountain'),
('Fast Gas'),
('Maceys'),
('CVS'),
('Smiths'),
('Dollar Tree'),
('Days Market'),
('Kroger'),
('Trader Joes'),
('La Pequenita Market'),
('Wawa'),
('Dans Food'),
('South End Market'),
('Ridleys'),
('Us Mobile'),
('Project Fi'),
('Boondocks'),
('Miracle Bowl'),
('Jack And Jill Lanes'),
('Groupon'),
('Megaplex'),
('Vidangel'),
('Cinemark'),
('Google Play'),
('Youtube Videos '),
('Autozone'),
('Safelite'),
('Jims Auto Accessories'),
('Utah DMV'),
('Murdock Hyundai'),
('Grease Monkey'),
('Jiffy Lube'),
('Vail Resorts'),
('Zion Mountain Ranch'),
('Mad Dog'),
('Recreation Outlet'),
('Utah Paddle Board'),
('Utah State Parks'),
('Orem Ultimate'),
('Sundance'),
('Zion National Park'),
('Hang Time'),
('The Quarry'),
('Timpanogos Cyclery'),
('Hobby Lobby'),
('Best Buy'),
('Stewarts Ace Hardware'),
('Home Depot'),
('Sports Authority'),
('Ross'),
('Lehi Factory Store'),
('H&M'),
('REI')
;

INSERT INTO VendorCategory(VendorID,CategoryID,TransactionCount) VALUES
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

