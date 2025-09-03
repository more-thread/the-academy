
CREATE OR ALTER VIEW [TRS].[vw_HR_EmployeeInfo]
AS


	select cast(hr.EMPLOYEE_NO as nvarchar(max))[EmployeeNo],
	info.EMP_FULL_NAME[EmployeeName], 
	info.EMP_FNAME[FirstName],
	info.EMP_LNAME[LastName],
	info.EMP_MNAME[MiddleName],
	EMPLOYEE_STATUS[EmployeeStatus], 
	eservice.SERVICE_DESCRIPTION[ServiceDescription],
	isnull(EMPLOYEE_SERVICE,'')[EmployeeService],
	msys.PositionName, 
	POSITION_CODE[PositionCode],
	EMPLOYEE_TYPE_DESC[EmployeeTypeDesc],
	hr.EMPLOYEE_TYPE[EmployeeType],
	JOBCLASS_DESCRIPTION[JobClassDescription],
	hr.JOBCLASS_CODE[JobClassCode],
	hr.EMPLOYEE_LEVEL[EmployeeLevel],
	hr.BRANCH_CODE[Branch],
	hr.SATELLITE_CODE[Satellite],
	superior.EmpID[SuperiorID],
	superior.FullName[SuperiorFullname],
	msys.SectionID,
	msys.SectionName,
	sectionhead.EmpID[SectionHeadID],
	sectionhead.FullName[SectionHeadFullname],
	msys.DepartmentID,
	msys.DepartmentName,
	depthead.EmpID[DepartmentHeadID],
	depthead.FullName[DepartmentHeadFullname],
	msys.DivisionID,
	msys.DivisionName,
	isnull(divhead.EmpID,'')[DivisionHeadID],
	isnull(divhead.FullName,'')[DivisionHeadFullname],
	isnull(info.HP_NO,'') [PersonalPhoneNo],
	isnull(msys.EmailAddress,'') [EmailAddress]
	from Onehrmstestdb.hr.tEmploymentInfoCurrent hr
	left join Onehrmstestdb.hr.tEmployeeInfo info on info.EMPLOYEE_NO = hr.EMPLOYEE_NO
	left join Onehrmstestdb.Administration.mSystemUsers msys on msys.empid = hr.EMPLOYEE_NO
	left join Onehrmstestdb.hr.mEmployeeType etype on etype.EMPLOYEE_TYPE = hr.EMPLOYEE_TYPE
	left join Onehrmstestdb.hr.mJobClass ejob on ejob.JOBCLASS_CODE = hr.JOBCLASS_CODE
	left join Onehrmstestdb.hr.mService eservice on eservice.SERVICE_CODE = hr.EMPLOYEE_SERVICE
	left join Onehrmstestdb.administration.mSystemUsers superior on superior.EmpID = msys.SuperiorID
	left join Onehrmstestdb.administration.mSystemUsers sectionhead on sectionhead.EmpID = msys.SectionHeadID
	left join Onehrmstestdb.administration.mSystemUsers depthead on depthead.EmpID = msys.DepartmentHeadID
	left join Onehrmstestdb.administration.mSystemUsers divhead on divhead.EmpID = msys.DivisionHeadID
	where
	EMPLOYEE_STATUS = 'A' OR EMPLOYEE_SERVICE IN (select 'LO'[LEAVE] union all select 'SUS'[LEAVE] union all select LEAVE_CODE from OneHRMSTestDB.hr.mStaffMovementLeaveMaster where PMSIncluded = 1)
GO
/****** Object:  View [TRS].[vw_HR_JobClasses]    Script Date: 9/24/2024 4:27:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER  VIEW [TRS].[vw_HR_JobClasses]
AS
SELECT JOBCLASS_CODE[JobClassCode], JOBCLASS_DESCRIPTION[JobClassDescription]
FROM   OneHRMSTestDB.HR.mJobClass
WHERE  JOBCLASS_CODE IN ('PRPR', 'PSPC', 'NPP01-DIR', 'RSRM', 'NPP01-CM1', 'NPP01-CM2', 'NPP01-CMT', 'NPP01-CS1','NPP01-CS2','NPP01-CS3','NPP01-AST',
'SEA1',
'SEA2',
'SEA3',
'SEA4',
'SEA5',
'SEA6',
'PRB1',
'PRB2',
'PRB3'
)

GO
/****** Object:  View [TRS].[vw_HR_Regions]    Script Date: 9/24/2024 4:27:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER VIEW [TRS].[vw_HR_Regions]
AS

SELECT 'REGION 1'[Region], 'AGOO'[Branch]
UNION ALL
SELECT 'REGION 1'[Region], 'CAND'[Branch]
UNION ALL
SELECT 'REGION 1'[Region], 'PASI'[Branch]
UNION ALL
SELECT 'REGION 2'[Region], 'ISAB'[Branch]

GO