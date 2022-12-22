using System.Runtime.Serialization;

namespace Play.Inventory.Api.Exceptions
{
    [Serializable]
    internal class UnknownItemException : Exception
    {
        public Guid ItemId;

        public UnknownItemException()
        {
        }

        public UnknownItemException(Guid itemId) : base($"Unknown item '{itemId}'")
        {
            ItemId = itemId;
        }
    }

}