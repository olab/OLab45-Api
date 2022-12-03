namespace OLabWebAPI.Services.TurkTalk.Contracts
{
    public abstract class Method
    {
        public string MethodName { get; set; }
        public string RecipientGroupName { get; set; }

        public Method(string recipientGroupName, string methodName)
        {
            MethodName = methodName;
            RecipientGroupName = recipientGroupName;
        }

        public abstract string ToJson();
    }
}