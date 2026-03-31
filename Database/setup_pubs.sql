-- ============================================================
-- PUBs Database Setup Script for Star Publications
-- Run this script against your SQL Server instance to create
-- and populate the PUBs database used by the application.
-- ============================================================

USE master;
GO

-- Create the pubs database if it does not already exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'pubs')
BEGIN
    CREATE DATABASE pubs;
END
GO

USE pubs;
GO

-- ── publishers ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'publishers')
BEGIN
    CREATE TABLE publishers (
        pub_id   CHAR(4)      NOT NULL PRIMARY KEY,
        pub_name VARCHAR(40)  NULL,
        city     VARCHAR(20)  NULL,
        state    CHAR(2)      NULL,
        country  VARCHAR(30)  NULL DEFAULT 'USA'
    );
END
GO

-- ── pub_info ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'pub_info')
BEGIN
    CREATE TABLE pub_info (
        pub_id  CHAR(4)   NOT NULL PRIMARY KEY
            REFERENCES publishers(pub_id),
        pr_info TEXT      NULL
    );
END
GO

-- ── titles ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'titles')
BEGIN
    CREATE TABLE titles (
        title_id  VARCHAR(6)   NOT NULL PRIMARY KEY,
        title     VARCHAR(80)  NOT NULL,
        type      CHAR(12)     NULL DEFAULT 'UNDECIDED',
        pub_id    CHAR(4)      NULL REFERENCES publishers(pub_id),
        price     MONEY        NULL,
        advance   MONEY        NULL,
        royalty   INT          NULL,
        ytd_sales INT          NULL,
        notes     VARCHAR(200) NULL,
        pubdate   DATETIME     NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ── authors ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'authors')
BEGIN
    CREATE TABLE authors (
        au_id     VARCHAR(11)  NOT NULL PRIMARY KEY,
        au_lname  VARCHAR(40)  NOT NULL,
        au_fname  VARCHAR(20)  NOT NULL,
        phone     CHAR(12)     NOT NULL DEFAULT 'UNKNOWN',
        address   VARCHAR(40)  NULL,
        city      VARCHAR(20)  NULL,
        state     CHAR(2)      NULL,
        zip       CHAR(5)      NULL,
        contract  BIT          NOT NULL
    );
END
GO

-- ── titleauthor ──────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'titleauthor')
BEGIN
    CREATE TABLE titleauthor (
        au_id       VARCHAR(11)  NOT NULL REFERENCES authors(au_id),
        title_id    VARCHAR(6)   NOT NULL REFERENCES titles(title_id),
        au_ord      TINYINT      NULL,
        royaltyper  INT          NULL,
        PRIMARY KEY (au_id, title_id)
    );
END
GO

-- ── stores ───────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'stores')
BEGIN
    CREATE TABLE stores (
        stor_id      CHAR(4)      NOT NULL PRIMARY KEY,
        stor_name    VARCHAR(40)  NULL,
        stor_address VARCHAR(40)  NULL,
        city         VARCHAR(20)  NULL,
        state        CHAR(2)      NULL,
        zip          CHAR(5)      NULL
    );
END
GO

-- ── sales ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sales')
BEGIN
    CREATE TABLE sales (
        stor_id   CHAR(4)      NOT NULL REFERENCES stores(stor_id),
        ord_num   VARCHAR(20)  NOT NULL,
        ord_date  DATETIME     NOT NULL,
        qty       SMALLINT     NOT NULL,
        payterms  VARCHAR(12)  NOT NULL,
        title_id  VARCHAR(6)   NOT NULL REFERENCES titles(title_id),
        PRIMARY KEY (stor_id, ord_num, title_id)
    );
END
GO

-- ════════════════════════════════════════
-- Sample data (mirrors the classic PUBs db)
-- ════════════════════════════════════════

-- Publishers
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '0736')
    INSERT INTO publishers VALUES ('0736','New Moon Books','Boston','MA','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '0877')
    INSERT INTO publishers VALUES ('0877','Binnet & Hardley','Washington','DC','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '1389')
    INSERT INTO publishers VALUES ('1389','Algodata Infosystems','Berkeley','CA','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '1622')
    INSERT INTO publishers VALUES ('1622','Five Lakes Publishing','Chicago','IL','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '1756')
    INSERT INTO publishers VALUES ('1756','Ramona Publishers','Dallas','TX','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '9901')
    INSERT INTO publishers VALUES ('9901','GGG&G',N'München',NULL,'Germany');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '9952')
    INSERT INTO publishers VALUES ('9952','Scootney Books','New York','NY','USA');
IF NOT EXISTS (SELECT 1 FROM publishers WHERE pub_id = '9999')
    INSERT INTO publishers VALUES ('9999','Lucerne Publishing','Paris',NULL,'France');
GO

-- pub_info
IF NOT EXISTS (SELECT 1 FROM pub_info WHERE pub_id = '0736')
    INSERT INTO pub_info VALUES ('0736','New Moon Books was founded in 1977 and specialises in business and psychology titles.');
IF NOT EXISTS (SELECT 1 FROM pub_info WHERE pub_id = '0877')
    INSERT INTO pub_info VALUES ('0877','Binnet & Hardley is the oldest of the publishing houses in our database, established 1955.');
IF NOT EXISTS (SELECT 1 FROM pub_info WHERE pub_id = '1389')
    INSERT INTO pub_info VALUES ('1389','Algodata Infosystems publishes cutting-edge computer science and programming titles.');
GO

-- Authors
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '172-32-1176')
    INSERT INTO authors VALUES ('172-32-1176','White','Johnson','408 496-7223','10932 Bigge Rd.','Menlo Park','CA','94025',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '213-46-8915')
    INSERT INTO authors VALUES ('213-46-8915','Green','Marjorie','415 986-7020','309 63rd St. #411','Oakland','CA','94618',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '238-95-7766')
    INSERT INTO authors VALUES ('238-95-7766','Carson','Cheryl','415 548-7723','589 Darwin Ln.','Berkeley','CA','94705',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '267-41-2394')
    INSERT INTO authors VALUES ('267-41-2394','O''Leary','Michael','408 286-2428','22 Cleveland Av. #14','San Jose','CA','95128',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '274-80-9391')
    INSERT INTO authors VALUES ('274-80-9391','Straight','Dean','415 834-2919','5420 College Av.','Oakland','CA','94609',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '409-56-7008')
    INSERT INTO authors VALUES ('409-56-7008','Bennet','Abraham','415 658-9932','6223 Bateman St.','Berkeley','CA','94705',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '427-17-2319')
    INSERT INTO authors VALUES ('427-17-2319','Dull','Ann','415 836-7128','3410 Blonde St.','Palo Alto','CA','94301',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '486-29-1786')
    INSERT INTO authors VALUES ('486-29-1786','Locksley','Chastity','415 585-4620','18 Broadway Av.','San Francisco','CA','94130',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '672-71-3249')
    INSERT INTO authors VALUES ('672-71-3249','Yokomoto','Akiko','415 935-4228','3 Silver Ct.','Walnut Creek','CA','94595',1);
IF NOT EXISTS (SELECT 1 FROM authors WHERE au_id = '712-45-1867')
    INSERT INTO authors VALUES ('712-45-1867','del Castillo','Innes','615 996-8275','2286 Cram Pl. #86','Ann Arbor','MI','48105',1);
GO

-- Titles
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'BU1032')
    INSERT INTO titles VALUES ('BU1032','The Busy Executive''s Database Guide','business','1389',19.99,5000,10,4095,'An overview of available database systems with emphasis on common business applications.',CAST('1991-06-12' AS DATETIME));
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'BU1111')
    INSERT INTO titles VALUES ('BU1111','Cooking with Computers: Surreptitious Balance Sheets','business','1389',11.95,5000,10,3876,'Helpful hints on how to use your electronic resources to the best advantage.',CAST('1991-06-09' AS DATETIME));
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'BU2075')
    INSERT INTO titles VALUES ('BU2075','You Can Combat Computer Stress!','business','0736',2.99,10125,24,18722,'The latest medical and psychological techniques for living with the electronic office.',CAST('1991-06-30' AS DATETIME));
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'MC2222')
    INSERT INTO titles VALUES ('MC2222','Silicon Valley Gastronomic Treats','mod_cook','0877',19.99,0,12,2032,'Favorite recipes for quick, easy, and elegant meals.',CAST('1991-06-09' AS DATETIME));
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'PS1372')
    INSERT INTO titles VALUES ('PS1372','Computer Phobic AND Non-Phobic Individuals: Behavior Variations','psychology','0877',21.59,7000,10,375,'A must-read for any developer curious about human-computer interaction.',CAST('1991-10-21' AS DATETIME));
IF NOT EXISTS (SELECT 1 FROM titles WHERE title_id = 'TC3218')
    INSERT INTO titles VALUES ('TC3218','Onions, Leeks, and Garlic: Cooking Secrets of the Mediterranean','trad_cook','0877',20.95,7000,10,375,'Detailed instructions on how to make authentic Mediterranean dishes.',CAST('1991-10-21' AS DATETIME));
GO

-- Title-Author relationships
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '409-56-7008' AND title_id = 'BU1032')
    INSERT INTO titleauthor VALUES ('409-56-7008','BU1032',1,60);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '486-29-1786' AND title_id = 'BU1032')
    INSERT INTO titleauthor VALUES ('486-29-1786','BU1032',2,40);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '213-46-8915' AND title_id = 'BU1111')
    INSERT INTO titleauthor VALUES ('213-46-8915','BU1111',1,40);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '267-41-2394' AND title_id = 'BU1111')
    INSERT INTO titleauthor VALUES ('267-41-2394','BU1111',2,40);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '672-71-3249' AND title_id = 'BU1111')
    INSERT INTO titleauthor VALUES ('672-71-3249','BU1111',3,20);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '172-32-1176' AND title_id = 'PS1372')
    INSERT INTO titleauthor VALUES ('172-32-1176','PS1372',1,75);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '274-80-9391' AND title_id = 'BU2075')
    INSERT INTO titleauthor VALUES ('274-80-9391','BU2075',1,100);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '427-17-2319' AND title_id = 'TC3218')
    INSERT INTO titleauthor VALUES ('427-17-2319','TC3218',1,75);
IF NOT EXISTS (SELECT 1 FROM titleauthor WHERE au_id = '238-95-7766' AND title_id = 'MC2222')
    INSERT INTO titleauthor VALUES ('238-95-7766','MC2222',1,100);
GO

-- Stores
IF NOT EXISTS (SELECT 1 FROM stores WHERE stor_id = '6380')
    INSERT INTO stores VALUES ('6380','Eric the Read Books','788 Catamaugus Ave.','Seattle','WA','98056');
IF NOT EXISTS (SELECT 1 FROM stores WHERE stor_id = '7066')
    INSERT INTO stores VALUES ('7066','Barnum''s','567 Pasadena Ave.','Tustin','CA','92789');
IF NOT EXISTS (SELECT 1 FROM stores WHERE stor_id = '7067')
    INSERT INTO stores VALUES ('7067','News & Brews','577 First St.','Los Gatos','CA','96745');
IF NOT EXISTS (SELECT 1 FROM stores WHERE stor_id = '7131')
    INSERT INTO stores VALUES ('7131','Doc-U-Mat: Quality Laundry and Books','24-A Avogadro Way','Remulade','WA','98014');
IF NOT EXISTS (SELECT 1 FROM stores WHERE stor_id = '8042')
    INSERT INTO stores VALUES ('8042','Bookbeat','679 Carson St.','Portland','OR','89076');
GO

-- Sales orders
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '6380' AND ord_num = '6871' AND title_id = 'BU1032')
    INSERT INTO sales VALUES ('6380','6871','1994-09-14',5,'Net 60','BU1032');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '6380' AND ord_num = '722a' AND title_id = 'PS1372')
    INSERT INTO sales VALUES ('6380','722a','1994-09-13',3,'Net 60','PS1372');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '7066' AND ord_num = 'A2976' AND title_id = 'BU1111')
    INSERT INTO sales VALUES ('7066','A2976','1993-05-24',50,'Net 30','BU1111');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '7067' AND ord_num = 'D4482' AND title_id = 'PS1372')
    INSERT INTO sales VALUES ('7067','D4482','1994-09-14',10,'Net 60','PS1372');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '7131' AND ord_num = 'N914008' AND title_id = 'BU2075')
    INSERT INTO sales VALUES ('7131','N914008','1994-09-14',20,'Net 30','BU2075');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '8042' AND ord_num = '423LL922' AND title_id = 'MC2222')
    INSERT INTO sales VALUES ('8042','423LL922','1994-09-14',15,'ON invoice','MC2222');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '8042' AND ord_num = 'QQ2299' AND title_id = 'BU1032')
    INSERT INTO sales VALUES ('8042','QQ2299','1993-10-28',75,'ON invoice','BU1032');
IF NOT EXISTS (SELECT 1 FROM sales WHERE stor_id = '7067' AND ord_num = 'P2121' AND title_id = 'TC3218')
    INSERT INTO sales VALUES ('7067','P2121','1992-06-15',40,'Net 30','TC3218');
GO

PRINT 'PUBs database setup complete.';
GO
