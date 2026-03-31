CREATE OR ALTER VIEW vw_ClientQrUsage AS
SELECT c.Id AS ClientId, c.CompanyName, COUNT(q.Id) AS TotalQr, SUM(CASE WHEN q.IsDynamic=1 THEN 1 ELSE 0 END) AS DynamicQr
FROM Clients c
LEFT JOIN QrCodes q ON q.ClientId=c.Id AND q.IsDeleted=0
GROUP BY c.Id, c.CompanyName;

CREATE OR ALTER VIEW vw_TopQrScans AS
SELECT TOP 100 q.Id, q.ClientId, q.Title, q.ScanCount, q.LastScanAt
FROM QrCodes q
WHERE q.IsDeleted=0
ORDER BY q.ScanCount DESC;
