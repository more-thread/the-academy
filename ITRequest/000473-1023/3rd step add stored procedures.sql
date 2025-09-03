use AgooTrainingRegistrarSystem2024

/****** Object:  StoredProcedure [TRS].[sp_Global_GetUserForms]    Script Date: 9/24/2024 4:27:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Rivera, Jerameel S.
-- Create date: 08/13/2024
-- Description:	Get all forms
-- =============================================
CREATE OR ALTER PROCEDURE [TRS].[sp_Global_GetUserForms]
@EmpID AS NVARCHAR(20)
AS
BEGIN

DECLARE @ULIISAdmin_Server AS NVARCHAR(200),
		@ULIISAdmin_DBName AS NVARCHAR(200),
		@ITPMS_Server AS NVARCHAR(200),
		@ITPMS_DBName AS NVARCHAR(200),
		@query AS NVARCHAR(MAX)

SELECT TOP 1 @ULIISAdmin_Server=IPAddress,@ULIISAdmin_DBName=BranchName FROM onehrmstestdb.Rehandling.mIPAddressTable WHERE ModuleID='ULIISADMIN' AND Status='A'

select @ITPMS_Server = IPAddress,@ITPMS_DBName = BranchName from OneHRMSTestDB.Rehandling.mIPAddressTable where ModuleID = 'ITPMS' and  Status = 'A'


SET @query ='
;with devID as (
				select ''JSR'' [DevInitials]
), devInfo as (
				select ''JSR | (1)239 | jerameel_rivera@universalleaf.com.ph''AS [DevInfo]
), empInfo as (
	select POSITION_DESCRIPTION,
			 EMPLOYEE_LEVEL
	from OneHRMSTestDB.hr.tEmploymentInfoCurrent a
	left join OneHRMSTestDB.hr.mPosition b on b.POSITION_CODE = a.POSITION_CODE and b.Status =''A''
	where EMPLOYEE_NO = '''+@empid+'''
)

select '''+@EmpID+'''[EmployeeNo],a.FormID,FormName,AccessibleDescription,CurrentVersion,ISNULL(a.DateModified,a.DateCreated) DateModified
,devID.DevInitials,devInfo.DevInfo,
a.SubMenuID,sub.SubMenuName,Controller,Action,Icon,
CASE WHEN ISNULL(b.ViewOnly,0)=1 THEN ''VIEW''
				WHEN ISNULL(b.[Create],0)=1 THEN ''CREATE''
				WHEN ISNULL(b.Approve,0)=1 THEN ''APPROVE''
				else ''NONE''
		END AccessType
from ['+@ULIISAdmin_Server+'].['+@ULIISAdmin_DBName+'].Administration.mForm a
inner join ['+@ULIISAdmin_Server+'].['+@ULIISAdmin_DBName+'].Administration.msubmenu sub on a.SubMenuID = sub.SubMenuID and sub.ModuleID = ''TRS''
left join ['+@ITPMS_Server+'].['+@ITPMS_DBName+'].Administration.tUAPM uapm on a.FormID collate SQL_Latin1_General_CP1_CI_AS  = uapm.Form and uapm.EmployeePosition COLLATE SQL_Latin1_General_CP437_CI_AS = (select POSITION_DESCRIPTION from empInfo) and uapm.Status = ''A''
left JOIN ['+@ULIISAdmin_Server+'].['+@ULIISAdmin_DBName+'].Administration.tUserGroupPermission b ON A.FormID=B.FormID AND A.ModuleID=B.ModuleID and b.status = ''A'' and b.UserGroupID='''+@EmpID+''' 
cross join devID 
cross join devInfo 
where a.ModuleID = ''TRS''
and (a.FormID = (
select case when a.formid in (
	''TRMM-0001'',
	''TRTT-0005'',
	''TRTT-0001'',
	''TRAA-0001'',
	''TRTT-0004''
	) then b.FormID else a.formid end) AND uapm.Form is not null)
	order by a.RecordNo
'
exec (@query)

END

GO
/****** Object:  StoredProcedure [TRS].[sp_Global_GetUserInfo]    Script Date: 9/24/2024 4:27:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Jerameel Rivera
-- Create date: 08/20/2024
-- Description:	Get User Info
-- =============================================
CREATE OR ALTER PROCEDURE [TRS].[sp_Global_GetUserInfo]
(
		@EmpID AS NVARCHAR(150)		
)
AS
BEGIN

DECLARE @ULIISAdmin_Server AS NVARCHAR(200),
		@ULIISAdmin_DBName AS NVARCHAR(200),
		@query AS NVARCHAR(MAX)

SELECT TOP 1 @ULIISAdmin_Server=IPAddress,@ULIISAdmin_DBName=BranchName FROM onehrmstestdb.Rehandling.mIPAddressTable WHERE ModuleID='ULIISADMIN' AND Status='A'

SET @query ='
SELECT cast(EmpID as nvarchar(max))EmpID, 
FullName = OneHRMSTestDB.[dbo].[ProperCase](FirstName) COLLATE DATABASE_DEFAULT  + '' '' + OneHRMSTestDB.[dbo].[ProperCase](LastName) COLLATE DATABASE_DEFAULT,
LastName = OneHRMSTestDB.[dbo].[ProperCase](LastName), 
FirstName = OneHRMSTestDB.[dbo].[ProperCase](FirstName), 
PositionName = OneHRMSTestDB.[dbo].[ProperCase](PositionName), BranchID, UserID, 
EmailAddress, HREmployeeStatus, DisplayPic = B.BLOBData,DepartmentID,
DepartmentName = OneHRMSTestDB.[dbo].[ProperCase](DepartmentName),SectionID,
SectionName = OneHRMSTestDB.[dbo].[ProperCase](SectionName),
BranchName= UPPER(A.BranchName)
FROM OneHRMSTestDB.Administration.mSystemUsers A
LEFT JOIN OneHRMSTestDB.[HR].tEmployeeImage B ON B.EMPLOYEE_NO = A.EmpID
WHERE EmpID = '''+@EmpID +'''
'

EXEC(@query)

END

GO
/****** Object:  StoredProcedure [TRS].[sp_Global_SendEmail]    Script Date: 9/24/2024 4:27:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jerameel Rivera
-- Create date: 08/20/2024
-- Description: A stored procedure for sending emails using SMTP.
-- =============================================
CREATE OR ALTER PROCEDURE [TRS].[sp_Global_SendEmail]
    @To nvarchar(max) = null,
	@CCTo nvarchar(max) = null,
	@BCCTo nvarchar(max) = null,
	@Message nvarchar(max),
	@Subject nvarchar(max)
AS
    
	
	Declare @Body varchar(max)

	
	Set @Body =
	'<html>
	<head>
	<style>
	body {
		font-family: Century Gothic;
		font-size: 11pt;
	}    
	#spanFooter {
					font-family: Century Gothic;
					font-size: 10pt;
					color: blue;
				}
	</style>
	</head>
	<body>
	'+@Message+'
	<br>
	<br>
	<p>Thank you,
	</br><b>Training Registrar System</b>
	<br><br><br><br>
		<footer>
			<span style="font-family:Century Gothic;font-size: 11pt; text-align:left">
			<td>
			<img src="https://s8.postimg.cc/bstee5get/ULPI_WHITE.png" width="215"> <br>
			<label><strong>TRAINING REGISTRAR SYSTEM</strong></label><br>  </td>
			<label></label><br>
			<label></label><br>
			</span>
		</footer>
	<p></p><p></p>
	<hr = "100%">
	<p><i><font color="blue">(This email has been automatically generated. Please do not reply to this email address as all responses are directed to an unattended mailbox and will not receive a response.)</font></i></p></body>

	</body>
	</html>'

	EXEC msdb.dbo.sp_send_dbmail
	@profile_name ='OneHRMS Mail',
	@recipients = @To,
	@copy_recipients= @CCTo ,
	@blind_copy_recipients= @BCCTo,
	@Body = @Body,
	@Body_Format = 'HTML',
	@subject = @Subject
	
GO