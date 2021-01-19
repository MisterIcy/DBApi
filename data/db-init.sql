USE OmegaUnitTests;
CREATE TABLE TestEntity (
    TestEntityIdentifier INT IDENTITY 
);

CREATE TABLE Categories (
    CategoryId INT IDENTITY,
    Title nvarchar(255) NOT NULL DEFAULT ""
);

CREATE TABLE Products (
    ProductId INT IDENTITY,
    Title nvarchar(255) NOT NULL,
    Slug nvarchar(255) NOT NULL,
    Description nvarchar(max) not null,
    SKU nvarchar(100) null default null,
    ProductCode nvarchar(100) not null,
    CategoryId int null default null    
);

Insert Into Categories (Title) VALUES ("Test Category 1"), ("Test Category 2"), ("Test Category 3");

INSERT INTO Products (Title, Slug, Description, SKU, ProductCode, CategoryId) VALUES (
    "Test Product", "test-product", "This is a test product", "tst-prd", "48481939921", 1
);
