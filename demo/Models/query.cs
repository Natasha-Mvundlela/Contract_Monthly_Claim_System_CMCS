namespace Contract_Monthly_Claim_System_CMCS.Models
{
    public class query
    {
        //they must all be public and non static
        public bool login(string Email_Address, string Password, string Role)
        {
            if (Email_Address == "Natasha Mvundli" && Password == "12345" && Role=="Lecturer")
            {
                return true;
            }
            return false;
        }
    }
}
