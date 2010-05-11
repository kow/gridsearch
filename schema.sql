
use spider;

#DROP TABLE Grid;
#DROP TABLE Agent;
#DROP TABLE Region;
#DROP TABLE Parcel;
#DROP TABLE Object;
#DROP TABLE SearchQueue;
#DROP TABLE Users;

CREATE TABLE Grid (
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name TEXT,
    LoginURI TEXT,
    Description TEXT
);

CREATE TABLE Agent (
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    ID TEXT,
    Grid INT,
    Name TEXT,
    LastScrape DATETIME 
);

CREATE TABLE Region (
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Grid INT,
    LocalID INT,
    Rating BIT(8),
    LastScrape DATETIME
);

CREATE TABLE Parcel (
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Region INT,
    Grid INT,
    Description TEXT,
    Size INT,
    Dwell INT,
    Owner INT,
    ParcelFlags BIT(8),
    Rating BIT(8),
    LastScrape DATETIME
);

CREATE TABLE Object (
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Region INT,
    Grid INT,
    Creator INT,
    Owner INT,
    Name TEXT,
    Decription TEXT,
    Flags BIT(8),
    SalePrice INT,
    Prims INT,
    Location INT,
    LastScrape DATETIME
);

CREATE TABLE SearchQueue(
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Priority INT,
    Grid INT,
    QueuedAT DATETIME,
    QueuedBY INT
);

CREATE TABLE Users(
    PKey INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username TEXT,
    Password TEXT,    
    Email TEXT,
    Verified TINYINT,
    AccountCreated DATETIME
);