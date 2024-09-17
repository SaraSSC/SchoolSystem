namespace SchoolSystem.Helpers.Emails
{
    public interface IEmailHelper
    {
        Response SendEmail(string to, string subject, string body);
    }
}
