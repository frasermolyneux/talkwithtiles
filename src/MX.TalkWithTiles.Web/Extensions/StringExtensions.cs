using System.Net.Mail;

namespace MX.TalkWithTiles.Web.Extensions;

public static class StringExtensions
{
    public static bool IsEmail(this string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}