using BLL.CommandsQueries.GenericCRUD.Create;
using BLL.CommandsQueries.GenericCRUD.Delete;
using BLL.CommandsQueries.GenericCRUD.GetAll;
using BLL.CommandsQueries.GenericCRUD.GetById;
using BLL.CommandsQueries.GenericCRUD.Update;
using BLL.Services;
using BLL.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

public abstract class GenericCrudController<TKey, TViewModel, TCreateViewModel, TUpdateViewModel>(
    ISender sender)
    : BaseController
    where TViewModel : class
    where TCreateViewModel : class
    where TUpdateViewModel : class
{
    protected readonly ISender Sender = sender;

    [HttpGet]
    public virtual async Task<ActionResult<ServiceResponse<List<TViewModel>>>> GetAll(CancellationToken ct)
    {
        var query = new GetAll.Query<TViewModel>();
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("paginated")]
    public virtual async Task<ActionResult<ServiceResponse<PaginatedItemsVM<TViewModel>>>> GetAllPaginated([FromQuery] PagedVM pagedVm, CancellationToken ct)
    {
        var query = new GetAllPaginated.Query<TViewModel>(pagedVm);
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<ServiceResponse<TViewModel>>> GetById(TKey id, CancellationToken ct)
    {
        var query = new GetById.Query<TKey, TViewModel> { Id = id };
        var result = await Sender.Send(query, ct);
        return GetResult(result);
    }

    [HttpPost]
    public virtual async Task<ActionResult<ServiceResponse<TViewModel>>> Create([FromBody] TCreateViewModel vm, CancellationToken ct)
    {
        var command = new Create.Command<TCreateViewModel, TViewModel> { Model = vm };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpPut("{id}")]
    public virtual async Task<ActionResult<ServiceResponse<TViewModel>>> Update(TKey id, [FromBody] TUpdateViewModel vm, CancellationToken ct)
    {
        var command = new Update.Command<TUpdateViewModel, TKey, TViewModel> { Id = id, Model = vm };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete(TKey id, CancellationToken ct)
    {
        var command = new Delete.Command<TViewModel, TKey> { Id = id };
        var result = await Sender.Send(command, ct);
        return GetResult(result);
    }
}