namespace MeshApp.WorkStructure.Interfaces
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FulfilsIntentAttribute : Attribute
    {
        private readonly string _intentName;
        public FulfilsIntentAttribute(string IntentName)
        {
            _intentName = IntentName;
        }

        public string GetFulfiledIntentName()
        {
            return _intentName;
        }
    }
}
