using CouchDB.Driver.Extensions;
using Grpc.Core;
using MeshApp.WorkOrchestrator.DbContext;
using MeshApp.WorkOrchestrator.Services;
using MeshApp.WorkStructure;
using MeshApp.WorkStructure.Error;

namespace MeshApp.WorkOrchestrator.RpcServices
{
    public class WorkflowBuilderService : WorkflowBuilder.WorkflowBuilderBase
    {
        private readonly ILogger<WorkflowBuilderService> _logger;
        private readonly IIntentResolverBuilder _intentResolverBuilder;
        private readonly WorkflowDbContext _dbContext;

        public WorkflowBuilderService(ILogger<WorkflowBuilderService> logger, IIntentResolverBuilder intentResolverBuilder, WorkflowDbContext dbContext)
        {
            _logger = logger;
            _intentResolverBuilder = intentResolverBuilder;
            _dbContext = dbContext;
        }

        public override Task<DiscoverIntentResolverResponse> DiscoverIntentResolvers(DiscoverIntentResolverRequest request, ServerCallContext context)
        {
            if (request.SourceType != DiscoverIntentResolverRequest.Types.SourceType.FileSystem)
            {
                return Task.FromResult(new DiscoverIntentResolverResponse
                {
                    Error = new WorkStructure.Error.Error
                    {
                        ErrorCode = WorkStructure.Error.ErrorCode.InternalError,
                        ErrorMessage = "Discover logic other than FileSystem is not supported at the moment."
                    }
                });
            }

            if (string.IsNullOrEmpty(request.SearchLocation))
                return Task.FromResult(new DiscoverIntentResolverResponse
                {
                    Error = new WorkStructure.Error.Error
                    {
                        ErrorCode = WorkStructure.Error.ErrorCode.NotFound,
                        ErrorMessage = "SearchLocation not provided in the request."
                    }
                });

            var intentMap = _intentResolverBuilder.FindIntentResolvers(request.SearchLocation);

            return Task.FromResult(new DiscoverIntentResolverResponse
            {
                IntentMap = intentMap
            });
        }

        public override async Task<FindWorkflowsResponse> FindWorkflows(FindWorkflowsRequest request, ServerCallContext context)
        {
            try
            {
                var baseQuery = _dbContext.Workflows.AsQueryable();

                if (!string.IsNullOrEmpty(request.SearchId))
                {
                    baseQuery = baseQuery.Where(w => w.Workflow.WorkflowId == request.SearchId.ToLower());
                }
                if (!string.IsNullOrEmpty(request.SearchName))
                {
                    baseQuery = baseQuery.Where(w => w.Workflow.WorkflowName == request.SearchName.ToLower());
                }

                var workflows = await baseQuery.ToListAsync();

                var response = new FindWorkflowsResponse();

                if (workflows != null)
                {
                    response.Workflows.AddRange(workflows.Select(w => w.Workflow));
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while performing {nameof(FindWorkflows)} RPC");
                return new FindWorkflowsResponse
                {
                    Error = new WorkStructure.Error.Error
                    {
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }

        public override async Task<CreateOrUpdateWorkflowResponse> CreateOrUpdateWorkflow(CreateOrUpdateWorkflowRequest request, ServerCallContext context)
        {
            try
            {
                if (request?.Workflow == null)
                    throw new InvalidOperationException($"Workflow content is empty in the request.");

                // Check existing
                WorkflowDocument? existing = null;
                if (!string.IsNullOrEmpty(request.Workflow.WorkflowId) || !string.IsNullOrEmpty(request.Workflow.WorkflowName))
                {
                    existing = await _dbContext.Workflows
                        .Where(wf => wf.Workflow.WorkflowId == request.Workflow.WorkflowId.ToLower()
                            || wf.Workflow.WorkflowName == request.Workflow.WorkflowName)
                        .FirstOrDefaultAsync();
                }

                if (existing != null)
                {
                    existing.Workflow = request.Workflow;

                    await _dbContext.Workflows.AddOrUpdateAsync(existing, batch: false, context.CancellationToken);

                    return new CreateOrUpdateWorkflowResponse
                    {
                        Workflow = existing.Workflow
                    };
                }
                else
                {
                    request.Workflow.WorkflowId = Guid.NewGuid().ToString().ToLower();
                    request.Workflow.WorkflowName = request.Workflow.WorkflowName;
                    var persistedWorkflow = await _dbContext.Workflows.AddAsync(new WorkflowDocument
                    {
                        Workflow = request.Workflow
                    }, batch: false, context.CancellationToken);

                    return new CreateOrUpdateWorkflowResponse
                    {
                        Workflow = persistedWorkflow.Workflow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while performing {nameof(CreateOrUpdateWorkflow)} RPC");
                return new CreateOrUpdateWorkflowResponse
                {
                    Error = new WorkStructure.Error.Error
                    {
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }
    }
}
