-- MySQL dump 10.13  Distrib 5.1.49, for debian-linux-gnu (i486)
--
-- Host: localhost    Database: gridspider
-- ------------------------------------------------------
-- Server version	5.1.49-1~bpo50+1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Agent`
--

DROP TABLE IF EXISTS `Agent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Agent` (
  `AgentID` char(16) NOT NULL,
  `Grid` mediumint(2) NOT NULL,
  `Name` text,
  `LastScrape` datetime NOT NULL DEFAULT '1970-01-01 12:00:00',
  `About` text,
  `FirstLife` text,
  `LockID` int(11) DEFAULT NULL,
  PRIMARY KEY (`Grid`,`AgentID`)
) ENGINE=MyISAM AUTO_INCREMENT=406 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Grid`
--

DROP TABLE IF EXISTS `Grid`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Grid` (
  `PKey` int(11) NOT NULL AUTO_INCREMENT,
  `name` text,
  `LoginURI` text,
  `Description` text,
  `new` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`PKey`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Logins`
--

DROP TABLE IF EXISTS `Logins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Logins` (
  `PKey` int(11) NOT NULL AUTO_INCREMENT,
  `grid` int(11) DEFAULT NULL,
  `First` text,
  `Last` text,
  `Password` text,
  `LockID` int(11) DEFAULT NULL,
  `LastScrape` datetime DEFAULT NULL,
  `PID` int(11) DEFAULT NULL,
  PRIMARY KEY (`PKey`)
) ENGINE=MyISAM AUTO_INCREMENT=12 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Object`
--

DROP TABLE IF EXISTS `Object`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Object` (
  `Region` bigint(32) NOT NULL,
  `Grid` int(11) NOT NULL,
  `Creator` char(16) DEFAULT NULL,
  `Owner` char(16) DEFAULT NULL,
  `Name` text,
  `Description` text,
  `Flags` bit(8) DEFAULT NULL,
  `SalePrice` mediumint(11) DEFAULT NULL,
  `Prims` smallint(11) DEFAULT NULL,
  `Location` int(11) DEFAULT NULL,
  `LastScrape` datetime DEFAULT NULL,
  `LocalID` int(11) NOT NULL DEFAULT '0',
  `ID` char(16) DEFAULT NULL,
  `SaleType` tinyint(4) DEFAULT NULL COMMENT 'Not = 0  Original = 1 Copy = 2,  Contents = 3',
  `ParcelID` tinyint(4) DEFAULT NULL,
  `Perms` int(11) NOT NULL DEFAULT '0' COMMENT 'Next Owner Permissions Mask { Transfer = 1 << 13, Modify = 1 << 14, Copy = 1 << 15, Move = 1 << 19, Damage = 1 << 20 }',
  `uniquekey` int(11) NOT NULL AUTO_INCREMENT,
  PRIMARY KEY (`Region`,`LocalID`),
  UNIQUE KEY `uniquekey` (`uniquekey`),
  FULLTEXT KEY `gText` (`Name`,`Description`)
) ENGINE=MyISAM AUTO_INCREMENT=1094303 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Temporary table structure for view `ObjectView`
--

DROP TABLE IF EXISTS `ObjectView`;
/*!50001 DROP VIEW IF EXISTS `ObjectView`*/;
SET @saved_cs_client     = @@character_set_client;
SET character_set_client = utf8;
/*!50001 CREATE TABLE `ObjectView` (
  `CreatorName` text,
  `OwnerName` text,
  `GridName` text,
  `RegionName` text,
  `GridLoginURI` text,
  `Creator` char(16),
  `Owner` char(16),
  `ObjectName` text,
  `SalePrice` mediumint(11),
  `SaleType` tinyint(4),
  `Perms` int(11),
  `ObjectID` char(16),
  `OLocalID` int(11),
  `Description` text,
  `Location` int(11)
) ENGINE=MyISAM */;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `Parcel`
--

DROP TABLE IF EXISTS `Parcel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Parcel` (
  `PKey` int(11) NOT NULL AUTO_INCREMENT,
  `Region` bigint(21) NOT NULL,
  `Grid` int(11) NOT NULL,
  `Description` text,
  `Size` smallint(11) DEFAULT NULL,
  `Dwell` smallint(11) DEFAULT NULL,
  `Owner` char(16) DEFAULT NULL,
  `ParcelFlags` int(11) DEFAULT NULL,
  `Rating` bit(8) DEFAULT NULL,
  `LastScrape` datetime DEFAULT NULL,
  `Name` text,
  `GroupID` char(16) DEFAULT NULL,
  `ParcelID` tinyint(4) NOT NULL DEFAULT '0',
  `SalePrice` int(11) DEFAULT '-1',
  PRIMARY KEY (`PKey`,`Region`,`Grid`,`ParcelID`),
  FULLTEXT KEY `Name` (`Name`,`Description`)
) ENGINE=MyISAM AUTO_INCREMENT=295922 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Region`
--

DROP TABLE IF EXISTS `Region`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Region` (
  `Grid` int(11) NOT NULL,
  `Handle` bigint(21) NOT NULL,
  `LastScrape` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `ID` char(16) DEFAULT NULL,
  `Owner` char(16) DEFAULT NULL,
  `Name` text,
  `Status` tinyint(4) NOT NULL DEFAULT '0',
  `LockID` int(11) NOT NULL DEFAULT '0',
  `pkey` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Grid`,`Handle`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SearchQueue`
--

DROP TABLE IF EXISTS `SearchQueue`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SearchQueue` (
  `PKey` int(11) NOT NULL AUTO_INCREMENT,
  `Priority` int(11) DEFAULT NULL,
  `Grid` int(11) DEFAULT NULL,
  `QueuedAT` datetime DEFAULT NULL,
  `QueuedBY` int(11) DEFAULT NULL,
  PRIMARY KEY (`PKey`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Users` (
  `PKey` int(11) NOT NULL AUTO_INCREMENT,
  `Username` text,
  `Password` text,
  `Email` text,
  `Verified` tinyint(4) DEFAULT NULL,
  `AccountCreated` datetime DEFAULT NULL,
  PRIMARY KEY (`PKey`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Final view structure for view `ObjectView`
--

/*!50001 DROP TABLE IF EXISTS `ObjectView`*/;
/*!50001 DROP VIEW IF EXISTS `ObjectView`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8 */;
/*!50001 SET character_set_results     = utf8 */;
/*!50001 SET collation_connection      = utf8_unicode_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`%` SQL SECURITY DEFINER */
/*!50001 VIEW `ObjectView` AS select `Agent`.`Name` AS `CreatorName`,`Agent`.`Name` AS `OwnerName`,`Grid`.`name` AS `GridName`,`Region`.`Name` AS `RegionName`,`Grid`.`LoginURI` AS `GridLoginURI`,`Object`.`Creator` AS `Creator`,`Object`.`Owner` AS `Owner`,`Object`.`Name` AS `ObjectName`,`Object`.`SalePrice` AS `SalePrice`,`Object`.`SaleType` AS `SaleType`,`Object`.`Perms` AS `Perms`,`Object`.`ID` AS `ObjectID`,`Object`.`LocalID` AS `OLocalID`,`Object`.`Description` AS `Description`,`Object`.`Location` AS `Location` from (((`Agent` join `Object`) join `Grid`) join `Region`) where ((`Agent`.`AgentID` = `Object`.`Creator`) and (`Agent`.`AgentID` = `Object`.`Owner`) and (`Grid`.`PKey` = `Object`.`Grid`) and (`Region`.`Handle` = `Object`.`Region`)) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2011-01-27  7:57:00
