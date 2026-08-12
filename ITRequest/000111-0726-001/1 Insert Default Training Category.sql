IF NOT EXISTS(SELECT * FROM TRS.mCourseCategory WHERE CategoryCode = 'CRS-0003')
BEGIN
	SET IDENTITY_INSERT [TRS].[mCourseCategory] ON 
	INSERT [TRS].[mCourseCategory] ([CategoryCode], [RecordNo], [CategoryTitle], [Status], [CreatedBy], [CreatedByComputerUsed], [DateCreated], [ModifiedBy], [ModifiedByComputerUsed], [DateModified]) VALUES (N'CRS-0003', 3, N'', 0, N'ROBE_VELORIA', N'IT01-VROBERT', CAST(N'2026-08-07T13:40:50.9900000' AS DateTime2), NULL, NULL, NULL)
	SET IDENTITY_INSERT [TRS].[mCourseCategory] OFF
END