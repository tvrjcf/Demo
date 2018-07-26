-----------------------------------------------------------
--  
--   Mysql           测试数据库
--
--
-----------------------------------------------------------

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

--
-- 数据库: `test`
--

-- --------------------------------------------------------

--
-- 表的结构 `TableName`
--

CREATE TABLE IF NOT EXISTS `tablename` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Value` int(11) NOT NULL,
  `Time` date NOT NULL,
  `Content` text NOT NULL,
  `Sort` int(11) NOT NULL,
  `ModelId` int(11) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=8 ;

--
-- 数据 `TableName`
--

INSERT INTO `TableName` (`ID`, `Name`, `Value`, `Time`, `Content`, `Sort`, `ModelId`) VALUES
(1, '898', 5, '2010-01-01', '内容1', 1, 1),
(2, '2', 4, '2010-05-11', '内容2', 2, 2),
(3, '3', 5, '2010-01-01', '内容3', 3, 3),
(4, '4', 6, '2010-01-01', '内容', 4, 4),
(5, '7', 5, '2010-01-01', '内容5', 5, 5),
(6, '6', 2, '2007-01-01', '内容6', 6, 6),
(7, '7', 2, '2006-01-02', '内容7', 7, 7);
