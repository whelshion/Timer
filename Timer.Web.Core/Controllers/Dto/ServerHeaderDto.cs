using Timer.Web.Core.Utils;

namespace Timer.Web.Core.Controllers.Dto
{
    public class ServerHeaderDto
    {
        public ServerHeaderDto(Server server)
        {
            Name = server.Name;
            Address = server.Address;
        }

        public string Name { get; private set; }
        public string Address { get; private set; }
    }
}