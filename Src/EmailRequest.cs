namespace EmailMicroService.Src
{
    public class EmailRequest
    {
        public string Server { get; set; } = "emailmicroservicedash@gmail.com";
        public string AppPassWord { get; set; } = "vgkg tdxs ffqu mjgr";

        public string Sender { get; set; } = "Administrador";
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<AttachmentRequest> Attachments { get; set; } = new();
    }
}
