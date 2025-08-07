namespace EmailMicroService.Src
{
    public class AttachmentRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
    }

}
