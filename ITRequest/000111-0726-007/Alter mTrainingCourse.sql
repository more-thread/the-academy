IF NOT EXISTS (SELECT * FROM sys.columns
WHERE object_id = OBJECT_ID('TRS.mTrainingCourse')
AND name = 'CategoryCode')
BEGIN
	ALTER TABLE [TRS].[mTrainingCourse] 
	ADD [CategoryCode] NVARCHAR(MAX) NOT NULL DEFAULT('');
END


