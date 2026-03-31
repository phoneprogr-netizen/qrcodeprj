INSERT INTO SubscriptionPlans
(Name,Description,MaxQrCodes,AllowDynamicQr,AllowTracking,MaxMonthlyScans,MaxUsers,AllowExport,AllowMultiLink,AllowCustomBranding,Price,DurationMonths,IsActive)
VALUES
('Basic','Piano base',10,0,0,NULL,2,0,0,0,9.99,1,1),
('Professional','Piano professional',100,1,1,NULL,10,1,1,0,39.99,1,1),
('Enterprise','Piano enterprise',1000,1,1,NULL,NULL,1,1,1,199.99,12,1);
