using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PlatformService.Data;
using System.Threading.Tasks;

namespace PlatformService.SyncDataService.Grpc
{
    public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GrpcPlatformService> _logger;

        public GrpcPlatformService(IPlatformRepo repository,
            IMapper mapper,
            ILogger<GrpcPlatformService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Get all Platforms from PlatformService");
            var response = new PlatformResponse();
            var platforms = _repository.GetAllPlatforms();

            foreach (var platform in platforms)
            {
                response.Platform.Add(_mapper.Map<GrpcPlatformModel>(platform));
            }
            _logger.LogInformation("Get all Platforms from PlatformService sending {Count} platforms",
                response.Platform.Count);
            return Task.FromResult(response);
        }
    }
}
