using regixinbound.RegixServiceReference;

namespace regixinbound
{
    public class RegixEntryPointManager
    {
        RegiXEntryPointClient client = new RegiXEntryPointClient();

        public ServiceResultData CallRegister(ServiceRequestData request)
        {
            ServiceResultData response = client.ExecuteSynchronous(request);

            return response;
        }
    }
}
