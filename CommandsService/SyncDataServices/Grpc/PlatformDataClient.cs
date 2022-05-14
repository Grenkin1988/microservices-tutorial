using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService;
using System.Collections.Generic;

namespace CommandsService.SyncDataServices
{
    public class PlatformDataClient : IPlatformDataClient
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<PlatformDataClient> _logger;

        public PlatformDataClient(IConfiguration configuration,
            IMapper mapper,
            ILogger<PlatformDataClient> logger)
        {
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            _logger.LogInformation("Calling GRPC Service {GrpcPlatform}", _configuration["GrpcPlatform"]);
            var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
            var client = new GrpcPlatform.GrpcPlatformClient(channel);
            var request = new GetAllRequest();

            try
            {
                var reply = client.GetAllPlatforms(request);
                _logger.LogInformation("Received from GRPC Service {Count} platforms", reply.Platform.Count);
                return _mapper.Map<IEnumerable<Platform>>(reply.Platform);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Could not call GRPC Server");
                throw;
            }
        }
    }
}
