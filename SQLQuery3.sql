

SELECT 
  DATEPART(YEAR, TimeStamp) AS Year, 
  DatePart(MONTH, TimeStamp) as month, 
  COUNT(*) AS usage
FROM usage 
GROUP BY DATEPART(YEAR, TimeStamp),DATEPART(Month, TimeStamp) ;
