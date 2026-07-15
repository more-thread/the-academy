namespace TRS.Global
{
    public class EmailTemplate
    {
        public string Subject { get; }
        public string HtmlBody { get; }

        public EmailTemplate(string subject, string htmlBody)
        {
            Subject = subject;
            HtmlBody = htmlBody;
        }
    }

    public static class EmailTemplates
    {
        public static readonly Dictionary<string, EmailTemplate> All = new()
        {
            ["TrainingFeedback"] = new EmailTemplate(
                "Training - Training Feedback",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "Congratulations on finishing the training course on, {TrainingCode} - {CourseTitle}.<br>" +
                "As part of our training analysis, we are conducting a training feedback to determine this training's effectiveness. In this regard, we urge you to complete the form as honestly as possible.<br>" +
                "Kindly visit the Training Feedback Form through the Registrar System.<br>" +
                "Your sincere and constructive feedback will help us assess the relevance of this program and develop future courses customized to our company's needs. <br>" +
                "We appreciate your time and effort in providing us with your feedback.</p>"
            ),
            ["RegistrationForConfirmation"] = new EmailTemplate(
                "Training - New Registration For Confirmation",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that you have been registered to this training,<br>" +
                "<b>{TrainingCode} - {CourseTitle}</b>.<br>" +
                "Training Coordinators will review and confirm your registration.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
            ),
            ["RegistrationForApproval"] = new EmailTemplate(
                "Training - New Registration For Approval",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that {EmployeeName} has registered to this training,<br>" +
                "<b>{TrainingCode} - {CourseTitle}</b>.<br>" +
                "Your approval is required to proceed with the registration.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to approve or disapprove the registration.</p>"
            ),
            ["RegistrationApproved"] = new EmailTemplate(
                "Training - New Registration Approved",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that your registration to this training," +
                "<b>{TrainingCode} - {CourseTitle}</b> has been approved.<br>" +
                "Training Coordinators will review and confirm your registration.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
            ),
            ["RegistrationDisapproved"] = new EmailTemplate(
                "Training - New Registration Disapproved",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that your registration to this training, <b>{TrainingCode} - {CourseTitle}</b> has been disapproved due to this reason: {Reason}.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
            ),
            ["RegistrationConfirmed"] = new EmailTemplate(
                "Training - Registration Confirmed",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that your registration to this training, <b>{TrainingCode} - {CourseTitle}</b>, has been confirmed.<br>" +
                "Your attendance is highly appreciated.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
            ),
            ["RegistrationRejected"] = new EmailTemplate(
                "Training - Registration Rejected",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to inform you that your registration to this training, <b>{TrainingCode} - {CourseTitle}</b> has been rejected due to this reason: {Reason}.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
            ),
            ["ScheduleReminder"] = new EmailTemplate(
                "Training - Schedule Reminder",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to remind you of your training about <b>{TrainingCode} - {CourseTitle}</b>, scheduled on {StartDate} - {EndDate} at {StartTime} - {EndTime}.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
            ),
            ["ScheduleForConfirmationReminder"] = new EmailTemplate(
                "Training - Schedule For Confirmation Reminder",
                "<p>Dear Ma'am/Sir,<br><br>" +
                "This is to remind you that there were changes in the schedule of this training, <b>{TrainingCode} - {CourseTitle}</b>, that you registered.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the changes and confirm your attendance with the new schedule.</p>"
            )
        };

        public static EmailTemplate Get(string id) => All.TryGetValue(id, out var template) ? template : null;

        public static string FillTemplate(string template, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(template) || parameters == null)
                return template;

            foreach (var kvp in parameters)
                template = template.Replace("{" + kvp.Key + "}", kvp.Value ?? string.Empty);

            return template;
        }
    }
}