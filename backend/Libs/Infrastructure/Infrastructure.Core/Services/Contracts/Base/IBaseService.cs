using Infrastructure.Core.Services.Contracts.Create;
using Infrastructure.Core.Services.Contracts.Delete;
using Infrastructure.Core.Services.Contracts.Get;
using Infrastructure.Core.Services.Contracts.Update;

namespace Infrastructure.Core.Services.Contracts.Base;

/// <summary>
/// Контракт для логики CRUD Service
/// </summary>
public interface IBaseService<in TCreateRequest, in TUpdateRequest, in TGetRequest, in TDeleteRequest, TResponse> :
    ICreateService<TCreateRequest, TResponse>,
    IUpdateService<TUpdateRequest, TResponse>,
    IGetService<TGetRequest, TResponse>,
    IDeleteService<TDeleteRequest, TResponse>
{
}