-----------------------------------------------------------
--  
--   Mysql           �������ݿ�
--
--
-----------------------------------------------------------

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

--
-- ���ݿ�: `test`
--

-- --------------------------------------------------------

--
-- ��Ľṹ `TableName`
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
-- ���� `TableName`
--

INSERT INTO `TableName` (`ID`, `Name`, `Value`, `Time`, `Content`, `Sort`, `ModelId`) VALUES
(1, '898', 5, '2010-01-01', '����1', 1, 1),
(2, '2', 4, '2010-05-11', '����2', 2, 2),
(3, '3', 5, '2010-01-01', '����3', 3, 3),
(4, '4', 6, '2010-01-01', '����', 4, 4),
(5, '7', 5, '2010-01-01', '����5', 5, 5),
(6, '6', 2, '2007-01-01', '����6', 6, 6),
(7, '7', 2, '2006-01-02', '����7', 7, 7);
