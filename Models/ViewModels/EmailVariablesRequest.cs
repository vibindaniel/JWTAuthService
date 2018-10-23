namespace DuploAuth.Models.ViewModels
{
    public class EmailVariablesRequest
    {
        public string from_name { get; set; }
        public string fromAddress { get; set; }
        public string smtp_ssl { get; set; }
        public string smtp_server { get; set; }
        public string smtp_password { get; set; }
        public string smtp_port { get; set; }
        public string send_email_invite { get; set; }
        public string invite_subject { get; set; }
        public string email_html { get; set; }
        public string invite_message { get; set; }
    }
}